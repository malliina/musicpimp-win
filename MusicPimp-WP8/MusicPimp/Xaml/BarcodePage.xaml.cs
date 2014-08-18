using Microsoft.Phone.Controls;
using Mle.MusicPimp.Beam;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    public partial class BarcodePage : PhoneApplicationPage {

        public PhoneBarcodeReader BarcodeViewModel { get; private set; }
        private BeamBarcodeHandler handler;

        public BarcodePage() {
            try {
                InitializeComponent();
                BarcodeViewModel = new PhoneBarcodeReader();
                DataContext = BarcodeViewModel;
                handler = new BeamBarcodeHandler(BarcodeViewModel, PhoneUtil.Instance);
            } catch(Exception) {
                // WTF???
            }
           
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            /// "It is recommended practice to initialise and dispose 
            /// of the camera object when we navigate to and from our page."
            /// 
            /// http://blogs.msdn.com/b/richmac/archive/2011/11/07/creating-a-qr-code-reader-app-for-windows-phone-7.aspx
            Utils.Suppress<Exception>(handler.Dispose);
            handler.Dispose();
            //BarcodeViewModel.Dispose();
            base.OnNavigatedFrom(e);
        }
    }
}