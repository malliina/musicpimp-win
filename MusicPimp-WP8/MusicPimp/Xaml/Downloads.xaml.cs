using Mle.MusicPimp.ViewModels;
using Mle.Pages;
using System;
using System.Windows.Controls;

namespace MusicPimp.Xaml {
    public partial class Downloads : BasePhonePage {

        public ExceptionAwareTransferModel ViewModel {
            get { return PimpViewModel.Instance.Downloader; }
        }

        public Downloads() {
            InitializeComponent();
            DataContext = ViewModel;
            var t = ViewModel.InstallProgressCallbacks();
        }
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e) {
            WithPivotIndex2(DownloadsPivot, pivotIndex => {
                switch(pivotIndex) {
                    case 0: // downloads
                        break;
                    case 1: // log
                        ViewModel.UpdateProps();
                        break;
                }
            });

        }
        private async void CancelAllButton_Click(object sender, EventArgs e) {
            await ViewModel.RemoveAllTransfers();
        }
    }
}