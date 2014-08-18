using Microsoft.Phone.Controls;
using Mle.ViewModels;
using System.Windows.Navigation;

namespace Mle {
    public partial class QRScanPage : PhoneApplicationPage {

        public QRModel ViewModel { get; private set; }

        public QRScanPage() {
            InitializeComponent();
            ViewModel = new QRModel(viewfinderBrush);
            DataContext = ViewModel;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            ViewModel.Init();
            // set the VideoBrush source to the camera output
            videoRotateTransform.CenterX = videoRectangle.Width / 2;
            videoRotateTransform.CenterY = videoRectangle.Height / 2;
            videoRotateTransform.Angle = 90;
            base.OnNavigatedTo(e);

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            ViewModel.Dispose();
            base.OnNavigatedFrom(e);
        }
    }
}