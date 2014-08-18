using Microsoft.Devices;
using Mle.Util;
using System.Windows.Media;

namespace Mle.ViewModels {
    public class QRModel : ViewModelBase {
        private string code = "Please scan a barcode.";
        public string Code {
            get { return code; }
            private set { SetProperty(ref code, value); }
        }

        private PhoneBarcodeReader reader;
        private VideoBrush video;

        public QRModel(VideoBrush video) {
            reader = new PhoneBarcodeReader();
            reader.CodeAvailable += async code => {
                await PhoneUtil.OnUiThread(() => Code = code);
            };
            this.video = video;
        }
        public PhotoCamera Camera {
            get { return reader.Camera; }
        }
        public void Init() {
            //reader.InitCamera(video);
        }
        public void Dispose() {
            reader.Dispose();
        }
    }
}
