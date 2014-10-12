using Microsoft.Phone.BackgroundTransfer;
using Mle.Background;
using Mle.IO;
using Mle.Network;
using Mle.Util;
using Mle.ViewModels;
using Mle.ViewModels.Background;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class DownloadsViewModel : MessagingViewModel {
        private static readonly string tempDownloadDir = "\\shared\\transfers\\";

        public DownloadPreferencesModel RequestSettings { get; private set; }
        public TransferStatusModel WaitingStatus { get; private set; }

        private IEnumerable<BackgroundTransferRequest> transferRequests;
        public IEnumerable<BackgroundTransferRequest> TransferRequests {
            get { return transferRequests; }
            set {
                this.SetProperty(ref this.transferRequests, value);
                OnPropertyChanged("IsTransfersEmpty");
            }
        }
        public bool IsTransfersEmpty {
            get { return TransferRequests == null || TransferRequests.Count() == 0; }
        }
        public ICommand AddTransferRequest { get; private set; }
        public ICommand CancelTransfer { get; private set; }
        public ICommand CancelAllTransfers { get; private set; }
        private volatile bool initComplete = false;

        public DownloadsViewModel() {
            using(var isoStore = IsolatedStorageFile.GetUserStoreForApplication()) {
                if(!isoStore.DirectoryExists(tempDownloadDir)) {
                    isoStore.CreateDirectory(tempDownloadDir);
                }
            }
            RequestSettings = new DownloadPreferencesModel();
            WaitingStatus = new TransferStatusModel();
            AddTransferRequest = new DelegateCommand<DownloadItem>(item => AddTransfer(item.RemoteUri, item.Destination));
            CancelTransfer = new AsyncDelegateCommand<string>(async transferId => {
                await RemoveTransfer(transferId);
                await UpdateTransfersList();
            });
            CancelAllTransfers = new AsyncUnitCommand(RemoveAllTransfers);
        }
        public bool TransferCountExceeded() {
            // Check to see if the maximum number of requests per app has been exceeded.
            return LoadTransfersCount() >= 25;
        }
        public virtual void AddTransfer(Uri remoteUri, string destination) {
            AddTransfer(remoteUri, destination, t => { });
        }
        /// <summary>
        /// Downloads the file at the given URI to the phone's isolated storage.
        /// 
        /// Downloads in progress go to the /shared/transfers directory after which
        /// they are moved to the destination directory upon completion.
        /// 
        /// The destination need not contain the required /shared/transfers path but the
        /// path to which the file will be moved once the download has completed.
        /// 
        /// </summary>
        /// <param name="remoteUri">file to download</param>
        /// <param name="destination">path in isolated storage once the download is completed</param>
        public virtual void AddTransfer(Uri remoteUri, string destination, Action<BackgroundTransferRequest> onFinished) {
            var transfer = PrepareTransfer(remoteUri, destination, onFinished);
            SubmitTransfer(transfer);
        }
        /// <summary>
        /// Downloads the specified remote uri, returns the local uri.
        /// 
        /// Note that if the app is suspended while the download is in progress, 
        /// I suppose the caller will never see the URI return value.
        /// 
        /// </summary>
        /// <param name="remoteUri">source uri</param>
        /// <param name="destination">destination path in isolated storage</param>
        /// <returns>the local uri to the downloaded file</returns>
        public Task<Uri> AddTransferAsync(Uri remoteUri, string destination) {
            var tcs = new TaskCompletionSource<Uri>();
            AddTransfer(remoteUri, destination, onFinished: transfer => {
                if(transfer.TransferError == null) {
                    // might throw invalidoperationexception
                    tcs.SetResult(new Uri(transfer.Tag, UriKind.RelativeOrAbsolute));
                } else {
                    tcs.SetException(transfer.TransferError);
                }
            });
            return tcs.Task;
        }
        private void SubmitTransfer(BackgroundTransferRequest transfer) {
            try {
                BackgroundTransferService.Add(transfer);
                // updates local cache
                //TransferRequests.Add(transfer);
            } catch(InvalidOperationException ioe) {
                TryToDispose(transfer);
                if(ioe.Message != "The request has already been submitted") {
                    throw;
                }
            } catch(Exception) {
                TryToDispose(transfer);
                throw;
            }
        }
        private void TryToDispose(BackgroundTransferRequest transfer) {
            try {
                transfer.Dispose();
            } catch(NullReferenceException) {
                // jesus this api sucks, how is this possible?
            } catch(Exception) {
                // intentional
            }
        }
        private BackgroundTransferRequest PrepareTransfer(Uri remoteUri, string destination) {
            return PrepareTransfer(remoteUri, destination, t => { });
        }
        private BackgroundTransferRequest PrepareTransfer(Uri remoteUri, string destination,
            Action<BackgroundTransferRequest> onFinished) {
            if(TransferCountExceeded()) {
                throw new BackgroundTransferException("The maximum number of background file transfer requests for this application has been exceeded.");
            }
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(remoteUri);
            // GET and POST are supported.
            transferRequest.Method = "GET";
            //var inProgressRelativePath = destination.Length > 150 ? Path.GetFileName(destination) : destination;
            //var inProgressSubPath = Path.GetDirectoryName(destination);
            //// too long file paths throw a "DirectoryNotFoundException"
            //var nameCandidate = Path.GetFileNameWithoutExtension(destination)
            //    .Replace(".", "")
            //    .Replace("(", "")
            //    .Replace(")", "")
            //    .Replace(" ", "");
            //var name = nameCandidate.Length > 0 ? nameCandidate : "temp";
            //var inProgressTempPath = tempDownloadDir + "\\" + inProgressSubPath + "\\" + name + Path.GetExtension(destination);
            var inProgressTempPath = tempDownloadDir + TailIfNeeded(destination);
            var inProgressUri = new Uri(inProgressTempPath, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = inProgressUri;
            // DownloadLocation does not always equal inProgressUri, encoding takes place, apparently in the setter.
            var inProgressDir = Path.GetDirectoryName(transferRequest.DownloadLocation.OriginalString);
            FileUtils.CreateDirIfNotExists(inProgressDir);
            transferRequest.Tag = destination;
            transferRequest.TransferPreferences = RequestSettings.TransferPrefs();
            transferRequest.TransferStatusChanged += async (sender, e) => {
                await OnTransferStatusUpdate(e.Request, onFinished);
                OnDownloadStatusUpdate(transferRequest);
            };
            transferRequest.TransferProgressChanged += (sender, e) => {
                OnDownloadStatusUpdate(transferRequest);
            };
            return transferRequest;
        }
        /// <summary>
        /// Ensures that the supplied path is no more than 150 chars in length.
        /// 
        /// Returns the path if it's no more than 150 chars. Otherwise returns the filename 
        /// if it's no more than 150 chars. Otherwise returns the last 150 chars of the filename.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the path, potentially shortened as described above</returns>
        private string TailIfNeeded(string path) {
            var maxLength = 100;
            if(path.Length <= maxLength) {
                return path;
            } else {
                var fileName = Path.GetFileName(path);
                if(fileName.Length <= maxLength) {
                    return fileName;
                } else {
                    return fileName.Substring(fileName.Length - maxLength);
                }
            }
        }

        // TODO can we use the request instances here instead of the id?
        public virtual async Task RemoveTransfer(string transferID) {
            var transfer = BackgroundTransferService.Find(transferID);
            if(transfer != null) {
                RemoveTransfer(transfer);
                await UpdateTransfersList();
            }
        }
        private void RemoveTransfer(BackgroundTransferRequest transfer) {
            using(var t = transfer) {
                BackgroundTransferService.Remove(t);
            }
        }
        public virtual Task RemoveAllTransfers() {
            DownloadDataContext.Clear();
            foreach(var transfer in BackgroundTransferService.Requests) {
                RemoveTransfer(transfer);
            }
            return UpdateTransfersList();
        }

        private Task OnTransferStatusUpdate(BackgroundTransferRequest transfer) {
            return OnTransferStatusUpdate(transfer, t => { });
        }
        protected virtual void OnDownloadStatusUpdate(BackgroundTransferRequest transfer) {

        }
        private async Task OnTransferStatusUpdate(
            BackgroundTransferRequest transfer,
            Action<BackgroundTransferRequest> onFinished) {
            switch(transfer.TransferStatus) {
                // TransferStatus.Completed might be reported more than once for the same transfer
                case TransferStatus.Completed:
                    await OnTransferComplete(transfer);
                    onFinished(transfer);
                    break;
                default:
                    WaitingStatus.Update(transfer.TransferStatus);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transfer"></param>
        /// <exception cref="IsolatedStorageException">If an IO error occurs while processing a successfully transferred file.</exception>
        protected virtual async Task OnTransferComplete(BackgroundTransferRequest transfer) {
            // Removes the transfer request in order to make room in the 
            // queue for more transfers. Transfers are not automatically
            // removed by the system.
            await RemoveTransfer(transfer.RequestId);

            var maybeError = transfer.TransferError;
            if(maybeError == null) {
                if(transfer.BytesReceived == transfer.TotalBytesToReceive) {
                    var finalDestination = MoveSuccessfullyDownloadedFile(transfer);
                    OnDownloadComplete(transfer, finalDestination);
                } else {
                    TryToDeleteIfExists(transfer.DownloadLocation.OriginalString);
                }
            } else {
                OnTransferCompletedWithErrors(transfer);
            }
            if(initComplete) {
                //Debug.WriteLine("Download complete");
                LoadTransfersFromPersistentStorage();
            }
        }
        private string MoveSuccessfullyDownloadedFile(BackgroundTransferRequest completedRequest) {
            string finalDestination = completedRequest.Tag;
            using(var isoStore = IsolatedStorageFile.GetUserStoreForApplication()) {
                var tempDownloadPath = completedRequest.DownloadLocation.OriginalString;
                if(!isoStore.FileExists(tempDownloadPath)) {
                    // this handler has already run previously, ignore
                    return finalDestination;
                }
                if(isoStore.FileExists(finalDestination)) {
                    isoStore.DeleteFile(finalDestination);
                } else {
                    var dir = Path.GetDirectoryName(finalDestination);
                    if(!isoStore.DirectoryExists(dir)) {
                        isoStore.CreateDirectory(dir);
                    }
                }
                isoStore.MoveFile(tempDownloadPath, finalDestination);
            }
            return finalDestination;
        }
        private void TryToDeleteIfExists(string file) {
            using(var isoStore = IsolatedStorageFile.GetUserStoreForApplication()) {
                Utils.Suppress<Exception>(() => {
                    if(isoStore.FileExists(file)) {
                        isoStore.DeleteFile(file);
                    }
                });
            }
        }
        protected virtual void OnDownloadComplete(BackgroundTransferRequest transfer, string localPath) {

        }
        protected virtual void OnTransferCompletedWithErrors(BackgroundTransferRequest transfer) {

        }
        public async Task Init() {
            await InstallStatusCallbacks();
            LoadTransfersFromPersistentStorage();
        }
        /// <summary>
        /// Initializes the existing transfers with the default status update handlers.
        /// 
        /// Note that while status update handlers are already added as the transfer request is created,
        /// they are only valid until the user closes the app.
        /// 
        /// We therefore need to re-install them at launch if the user has 
        /// closed the app while there are transfer(s) in progress.
        /// </summary>
        private async Task InstallStatusCallbacks() {
            WaitingStatus.Reset();
            await UpdateTransfersList();
            var reqs = TransferRequests;
            foreach(var transfer in reqs) {
                transfer.TransferStatusChanged += (sender, e) => {
                    OnTransferStatusUpdate(e.Request);
                    OnDownloadStatusUpdate(transfer);
                };
                transfer.TransferProgressChanged += (sender, e) => {
                    OnDownloadStatusUpdate(transfer);
                };
                // removes completed transfers
                await OnTransferStatusUpdate(transfer);
            }
            initComplete = true;
        }
        protected void LoadTransfersFromPersistentStorage() {
            // Initializing downloads eats a lot of CPU, so I only add 3 at a time
            if(LoadTransfersCount() < 3) {
                var downloadables = DownloadDataContext.Pop(3);
                //Debug.WriteLine("Downloading " + downloadables.Count + " more files...");
                foreach(var item in downloadables) {
                    AddTransfer(item.Source, item.Destination);
                }
            }
            // downloading 25 files concurrently is very expensive it seems...
            //var canTake = Math.Max(25 - LoadTransfersCount(), 0);
            //var downloadables = DownloadDataContext.Pop(canTake);
            //Debug.WriteLine("Downloading " + downloadables.Count + " more files...");
            //foreach (var item in downloadables) {
            //    AddTransfer(item.Source, item.Destination);
            //}
        }

        public async Task InstallProgressCallbacks() {
            await UpdateTransfersList();
            var reqs = TransferRequests;
            foreach(var transfer in reqs) {
                transfer.TransferStatusChanged += async (sender, e) => {
                    await UpdateTransfersList();
                    OnDownloadStatusUpdate(transfer);
                };
                transfer.TransferProgressChanged += async (sender, e) => {
                    await UpdateTransfersList();
                    OnDownloadStatusUpdate(transfer);
                };
            }
            await UpdateTransfersList();
        }
        protected int LoadTransfersCount() {
            var reqs = BackgroundTransferService.Requests;
            if(reqs != null) {
                var count = reqs.Count();
                DisposeAll(reqs);
                return count;
            }
            return 0;
        }
        private async Task UpdateTransfersList() {
            // The Requests property returns new references, so make sure that
            // you dispose of the old references to avoid memory leaks.
            DisposeAll(TransferRequests);
            await PhoneUtil.OnUiThread(() => {
                TransferRequests = BackgroundTransferService.Requests;
            });
        }
        private void DisposeAll(IEnumerable<BackgroundTransferRequest> reqs) {
            if(reqs != null) {
                foreach(var request in reqs) {
                    request.Dispose();
                }
            }
        }
    }
}
