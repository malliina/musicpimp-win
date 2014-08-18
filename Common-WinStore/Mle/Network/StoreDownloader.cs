using Mle.IO;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Mle.Network {
    public abstract class StoreDownloader : ViewModelBase {

        public ObservableCollection<BindableDownloadOperation> ActiveDownloads { get; private set; }

        public bool IsDownloadsEmpty {
            get { return ActiveDownloads.Count == 0; }
        }

        protected CancellationTokenSource Cts { get; private set; }

        public ICommand CancelAllDownloads { get; private set; }

        protected StoreFileUtils FileUtils { get; private set; }

        public StoreDownloader(StoreFileUtils fileUtils) {
            FileUtils = fileUtils;
            ActiveDownloads = new ObservableCollection<BindableDownloadOperation>();
            Cts = new CancellationTokenSource();
            CancelAllDownloads = new UnitCommand(CancelAll);
            ActiveDownloads.CollectionChanged += (s, e) => {
                OnPropertyChanged("IsDownloadsEmpty");
            };
        }
        public StoreDownloader(StoreFileUtils fileUtils, string username, string password)
            : this(fileUtils) {
        }
        public async Task<Uri[]> FollowActiveDownloadsAsync() {
            IReadOnlyList<DownloadOperation> downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            List<Task<Uri>> tasks = new List<Task<Uri>>();
            foreach (var download in downloads) {
                tasks.Add(AttachProgressFollower(download));
            }
            return await Task.WhenAll(tasks);
        }
        protected Task<Uri> Download(Uri source, string username, string password, StorageFile dest) {
            var downloader = new BackgroundDownloader();
            downloader.SetRequestHeader(HttpUtil.Authorization, HttpUtil.BasicAuthHeader(username, password));
            //downloader.ServerCredential = new Windows.Security.Credentials.PasswordCredential() {
            //    UserName = username,
            //    Password = password
            //};
            return Download(source, dest, downloader);
        }
        private Task<Uri> Download(Uri source, StorageFile dest, BackgroundDownloader downloader) {
            var download = downloader.CreateDownload(source, dest);
            return StartDownload(download);
        }
        protected async Task<Uri> Download(Uri source, string username, string password, string destination) {
            var destinationFile = await FileTo(destination);
            return await Download(source, username, password, destinationFile);
        }
        protected async Task<StorageFile> FileTo(string path) {
            var rootFolder = FileUtils.Folder;
            var destPath = StoreFileUtils.WindowsSeparators(path);
            // todo: Change ReplaceExisting to Fail, check existence before
            return await rootFolder.CreateFileAsync(destPath, CreationCollisionOption.ReplaceExisting);
        }
        /// <summary>
        /// Starts the download operation and awaits its completion.
        /// 
        /// This method only works if the download destination 
        /// resides inside the app local storage.
        /// </summary>
        /// <param name="download">operation to start</param>
        /// <returns>the uri to the downloaded file</returns>
        protected Task<Uri> StartDownload(DownloadOperation download) {
            return HandleDownload(download, alreadyStarted: false);
        }
        protected Task<Uri> AttachProgressFollower(DownloadOperation download) {
            return HandleDownload(download, alreadyStarted: true);
        }
        private async Task<Uri> HandleDownload(DownloadOperation download, bool alreadyStarted) {
            var bindableDownload = new BindableDownloadOperation(download);
            try {
                ActiveDownloads.Add(bindableDownload);
                var callback = new Progress<DownloadOperation>(OnDownloadStatusUpdate);
                IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> op = null;
                if (alreadyStarted) {
                    var d = download.AttachAsync();
                    op = download.AttachAsync();
                } else {
                    op = download.StartAsync();
                }
                op.Progress = (a, p) => {
                    OnBindableStatusUpdate(bindableDownload);
                };
                // awaits completion of download
                DownloadOperation completedDownload = await op.AsTask(Cts.Token, callback);
                return FileUtils.UriFor(completedDownload.ResultFile);
            } finally {
                // download complete
                ActiveDownloads.Remove(bindableDownload);
            }
        }
        private async void OnBindableStatusUpdate(BindableDownloadOperation download) {
            await download.UpdateProgress();
        }
        protected virtual void OnDownloadStatusUpdate(DownloadOperation download) {

        }

        private void CancelAll() {
            Cts.Cancel();
            Cts.Dispose();
            Cts = new CancellationTokenSource();
            ActiveDownloads.Clear();
        }
    }
}
