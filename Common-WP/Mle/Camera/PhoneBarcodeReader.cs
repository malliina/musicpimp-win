using Microsoft.Devices;
using Mle.Devices;
using System;
using System.Windows.Media;
using ZXing;

namespace Mle.ViewModels {
    /// <summary>
    /// A barcode reader.
    /// 
    /// Usage: listen to the CodeAvailable event.
    /// 
    /// The user should dispose of the camera object when it's no longer needed.
    /// 
    /// The user need not press any camera buttons; the barcode is
    /// automatically read.
    /// 
    /// </summary>
    public class PhoneBarcodeReader : BarcodeReaderBase {
        private VideoBrush videoBrush;
        public VideoBrush VideoBrush {
            get { return videoBrush; }
            set { SetProperty(ref videoBrush, value); }
        }

        public PhotoCamera Camera { get; private set; }
        public BarcodeReader Reader { get; private set; }

        private int width;
        private int height;
        private PhotoCameraLuminanceSource luminance;

        public PhoneBarcodeReader() {
            Camera = InitCamera();
            // the camera's "autofocus completed" event will use the barcode reader
            Reader = InitBarCodeReader();
            // shows what the camera sees on the screen
            VideoBrush = InitVideoBrush(Camera);
        }
        private PhotoCamera InitCamera() {
            var camera = new PhotoCamera();
            camera.Initialized += Camera_Initialized;
            camera.AutoFocusCompleted += Camera_AutoFocusCompleted;
            return camera;
        }
        private BarcodeReader InitBarCodeReader() {
            var reader = new BarcodeReader() {
                AutoRotate = true
            };
            reader.Options.TryHarder = true;
            reader.ResultFound += reader_ResultFound;
            return reader;
        }
        private VideoBrush InitVideoBrush(PhotoCamera camera) {
            var rotateTransform = new RotateTransform() {
                CenterX = .5,
                CenterY = .5,
                Angle = 90
            };
            var brush = new VideoBrush() {
                Stretch = Stretch.Fill,
                RelativeTransform = rotateTransform
            };
            try {
                brush.SetSource(camera);
            } catch(NotSupportedException) {
                // thrown if the camera is not available
            } catch(Exception) { }
            return brush;
        }
        private void Camera_Initialized(object sender, CameraOperationCompletedEventArgs e) {
            width = Convert.ToInt32(Camera.PreviewResolution.Width);
            height = Convert.ToInt32(Camera.PreviewResolution.Height);
            luminance = new PhotoCameraLuminanceSource(width, height);
            Camera.Focus();
        }
        private void Camera_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e) {
            try {
                if(Camera != null) {
                    TryDecode();
                }
                if(Camera != null) {
                    Camera.Focus();
                }
            } catch(Exception) { }
        }
        /// <summary>
        /// Reads the photo and feeds it to the barcode reader.
        /// </summary>
        private void TryDecode() {
            var previewBuffer = new byte[width * height];
            Camera.GetPreviewBufferY(previewBuffer);
            Reader.Decode(previewBuffer, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
        }
        /// <summary>
        /// Fired by the barcode reader upon a successful read.
        /// 
        /// </summary>
        /// <param name="result"></param>
        private void reader_ResultFound(Result result) {
            OnCodeAvailable(result.Text);
        }
        public override void Dispose() {
            try {
                if(Camera != null) {
                    Camera.Initialized -= Camera_Initialized;
                    Camera.AutoFocusCompleted -= Camera_AutoFocusCompleted;
                    Camera.Dispose();
                    Camera = null;
                }
            } catch(Exception) { }
        }

        /// <summary>
        /// https://www.codeplex.com/Download?ProjectName=zxingnet&DownloadId=696268
        /// </summary>
        public class PhotoCameraLuminanceSource : BaseLuminanceSource {
            public PhotoCameraLuminanceSource(int width, int height)
                : base(width, height) {
                luminances = new byte[width * height];
            }

            internal PhotoCameraLuminanceSource(int width, int height, byte[] newLuminances)
                : base(width, height) {
                luminances = newLuminances;
            }

            public byte[] PreviewBufferY {
                get { return luminances; }
            }

            protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height) {
                return new PhotoCameraLuminanceSource(width, height, newLuminances);
            }
        }

    }
}
