using Microsoft.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using ZXing;

namespace Mle.ViewModels {
    public class BarcodeVM : ViewModelBase {

        private VideoBrush videoBrush;
        public VideoBrush VideoBrush {
            get { return videoBrush; }
            set { SetProperty(ref videoBrush, value); }
        }
        private PhotoCaptureDevice device;
        private Size resolution;

        public BarcodeVM() {
            init();
        }
        private async void init() {
            resolution = await GetBestCaptureResolution();
            await InitPhotoCaptureDevice(resolution);
            await StartCapturingAsync();

            while (true) {
                var result = await GetBarcodeAsync();
                if (result != null) {
                    Debug.WriteLine("Scanned: " + result.Text);
                }
            }
        }
        private async Task<Result> GetBarcodeAsync() {
            await device.FocusAsync();
            return DetectBarcodeAsync();
        }

        private async Task InitPhotoCaptureDevice(Size size) {
            device = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, size);
            var compTransform = new CompositeTransform() {
                CenterX = .5,
                CenterY = .5,
                Rotation= device.SensorRotationInDegrees - 90
            };
            VideoBrush = new VideoBrush() {
                RelativeTransform = compTransform,
                Stretch = Stretch.Fill,
            };
            VideoBrush.SetSource(device);
        }
        private async Task<Size> GetBestCaptureResolution() {
            // The last size in the AvailableCaptureResolutions is the lowest available
            var captureResolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back);
            var previewResolutions = PhotoCaptureDevice.GetAvailablePreviewResolutions(CameraSensorLocation.Back);

            Size resolution = await Task.Factory.StartNew(() => captureResolutions.Last(
                c => (c.Width > 1000.0 || c.Height > 1000.0) && previewResolutions.Any(p => (c.Width / c.Height).Equals(p.Width / p.Height))));
            return resolution;
        }
        private Result DetectBarcodeAsync() {
            var width = (int)resolution.Width;
            var height = (int)resolution.Height;
            var previewBuffer = new byte[width * height];

            device.GetPreviewBufferY(previewBuffer);

            var barcodeReader = new BarcodeReader();
            barcodeReader.Options.TryHarder = true;
            //barcodeReader.TryInverted = true;
            //barcodeReader.AutoRotate = true;

            var result = barcodeReader.Decode(previewBuffer, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
            return result;
        }
        private async Task StartCapturingAsync() {
            CameraCaptureSequence sequence = device.CreateCaptureSequence(1);
            var memoryStream = new MemoryStream();
            sequence.Frames[0].CaptureStream = memoryStream.AsOutputStream();

            device.SetProperty(KnownCameraPhotoProperties.FlashMode, FlashState.Off);
            device.SetProperty(KnownCameraPhotoProperties.SceneMode, CameraSceneMode.Macro);

            await device.PrepareCaptureSequenceAsync(sequence);
        }



    }
}
