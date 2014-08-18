using Mle.Util;
using Mle.ViewModels;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace Mle.Network {
    /// <summary>
    /// BackgroundDownloadProgress is a struct and its members are not bindable,
    /// therefore this class wraps it and provides bindable properties.
    /// </summary>
    public class BindableDownloadOperation : ViewModelBase {
        public DownloadOperation Download { get; private set; }

        private BackgroundDownloadProgress Progress {
            get { return Download.Progress; }
        }
        public BindableDownloadOperation(DownloadOperation download) {
            Download = download;
        }
        public BackgroundTransferStatus Status {
            get { return Progress.Status; }
        }
        public ulong BytesReceived {
            get { return Progress.BytesReceived; }
        }
        public ulong TotalBytesToReceive {
            get { return Progress.TotalBytesToReceive; }
        }
        public string TotalBytesToReceiveReadable {
            get {
                return TotalBytesToReceive == 0 ? "Unknown" : "" + TotalBytesToReceive;
            }
        }
        public async Task UpdateProgress() {
            await StoreUtil.OnUiThread(() => {
                OnPropertyChanged("Status");
                OnPropertyChanged("BytesReceived");
                OnPropertyChanged("TotalBytesToReceive");
                OnPropertyChanged("TotalBytesToReceiveReadable");
            });
            //Debug.WriteLine(Progress.Status + ": " + Progress.BytesReceived + " / " + Progress.TotalBytesToReceive);
        }
    }
}
