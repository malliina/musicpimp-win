using Mle.Devices;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;

namespace Mle.MusicPimp.ViewModels {
    public class StoreBarcodeReader : BarcodeReaderBase {

        protected MediaCapture MediaCapture { get; private set; }

        private string helpText = "On another PC, go to https://beam.musicpimp.org";
        public string HelpText {
            get { return helpText; }
            set { SetProperty(ref helpText, value); }
        }
        private bool isPreviewing = false;
        public bool IsPreviewing {
            get { return isPreviewing; }
            set { SetProperty(ref isPreviewing, value); }
        }
        private bool hasCamera = false;
        public bool HasCamera {
            get { return hasCamera; }
            set {
                if(SetProperty(ref hasCamera, value)) {
                    OnPropertyChanged("ShowCameraView");
                }
            }
        }
        private bool cameraAccessGranted = false;
        public bool CameraAccessGranted {
            get { return cameraAccessGranted; }
            set {
                if(SetProperty(ref cameraAccessGranted, value)) {
                    OnPropertyChanged("ShowCameraView");
                }
            }
        }
        public bool ShowCameraView {
            get { return HasCamera && CameraAccessGranted; }
        }
        public StoreBarcodeReader() {
            MediaCapture = new MediaCapture();
        }

        public async Task StartPreview(CaptureElement videoCapture) {
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var cameraCount = cameras.Count;
            HasCamera = cameraCount > 0;
            if(!HasCamera) {
                HelpText = "No camera was found. This app feature requires a camera to function.";
            } else {
                try {
                    // prefers back camera
                    var cameraDeviceIndex = cameras.Count == 1 ? 0 : 1; // 0 => front, 1 => back if there are two cameras, I think
                    var mediaCaptureInitSettings = new MediaCaptureInitializationSettings {
                        VideoDeviceId = cameras[cameraDeviceIndex].Id
                    };
                    // throws UnauthorizedAccessException if the user does not grant access to camera & microphone
                    await MediaCapture.InitializeAsync(mediaCaptureInitSettings);
                    videoCapture.Source = MediaCapture;
                    await MediaCapture.StartPreviewAsync();
                    IsPreviewing = true;
                    CameraAccessGranted = true;
                    var barcode = await ScanAndDecodeImage();
                    await StopPreview();
                    OnCodeAvailable(barcode);
                } catch(UnauthorizedAccessException) {
                    CameraAccessGranted = false;
                    HelpText = "It appears this app is not authorized to use the camera. This app feature requires the camera. To allow access, open the Settings charm (swipe top-right and select Settings), then select Permissions, and ensure that this app is allowed to use the webcam/camera and microphone. Then revisit this page.";
                } catch(Exception) {
                    HelpText = "A camera error occurred. Please check your settings and try again.";
                }
            }
        }
        public async Task StopPreview() {
            if(IsPreviewing) {
                await MediaCapture.StopPreviewAsync();
                IsPreviewing = false;
            }
        }
        private async Task<string> ScanAndDecodeImage() {
            Result result = null;
            while(result == null && IsPreviewing) {
                // TODO capture to stream instead
                //using (var stream = new InMemoryRandomAccessStream()) {
                //await MediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                var photoStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("beam.jpg", CreationCollisionOption.GenerateUniqueName);
                await MediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), photoStorageFile);
                using(var stream = await photoStorageFile.OpenReadAsync()) {
                    // initialize with 1,1 to get the current size of the image
                    var writeableBmp = new WriteableBitmap(1, 1);
                    writeableBmp.SetSource(stream);
                    // and create it again because otherwise the WB isn't fully initialized and decoding
                    // results in a IndexOutOfRange
                    writeableBmp = new WriteableBitmap(writeableBmp.PixelWidth, writeableBmp.PixelHeight);
                    stream.Seek(0);
                    await writeableBmp.SetSourceAsync(stream);
                    result = DecodeBitmap(writeableBmp);
                }
                await photoStorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                //}
            }
            return result != null ? result.Text : null;
        }
        private Result DecodeBitmap(WriteableBitmap writeableBmp) {
            var barcodeReader = new BarcodeReader {
                AutoRotate = true
            };
            barcodeReader.Options.TryHarder = true;
            try {
                return barcodeReader.Decode(writeableBmp);
            } catch(NullReferenceException) {
                // crappy library
                return null;
            }
        }
    }
}
