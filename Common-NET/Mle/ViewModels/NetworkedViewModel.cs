using Mle.ViewModels;

namespace Mle.MusicPimp.ViewModels {
    public class NetworkedViewModel : ViewModelBase {
        private bool isOnline = false;
        public bool IsOnline {
            get { return isOnline; }
            set {
                if(SetProperty(ref isOnline, value)) {
                    OnPropertyChanged("NetworkStatus");
                };
            }
        }
        public virtual string NetworkStatus {
            get { return IsOnline ? "connected" : "not connected"; }
        }
    }
}
