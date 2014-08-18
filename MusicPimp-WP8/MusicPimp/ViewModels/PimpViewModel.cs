using Mle.IO;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage;

namespace Mle.MusicPimp.ViewModels {
    public class PimpViewModel : MusicItemsBase {
        private static PimpViewModel instance = null;
        public static PimpViewModel Instance {
            get {
                if(instance == null)
                    instance = new PimpViewModel();
                return instance;
            }
        }
        public EndpointsData EndpointsData {
            get { return PhoneEndpoints.Instance; }
        }
        public LibraryManager AudioSources {
            get { return PhoneLibraryManager.Instance; }
        }
        public PlayerManager PlaybackDevices {
            get { return PhonePlayerManager.Instance; }
        }
        public NowPlayingInfo NowPlayingModel { get; private set; }

        public PimpDownloader Downloader { get; private set; }

        public override BasePlayer MusicPlayer {
            get { return PlaybackDevices.Player; }
        }
        public override MusicLibrary MusicProvider {
            get { return AudioSources.MusicProvider; }
        }
        private string currentPicturePath = null;
        private ImageSource pictureBackground;
        public ImageSource PictureBackground {
            get { return pictureBackground; }
            set {
                if(SetProperty(ref pictureBackground, value)) {
                    var brush = new ImageBrush();
                    brush.ImageSource = value;
                    // transparency so it doesn't disturbe visibility of UI controls
                    brush.Opacity = 0.1;
                    brush.Stretch = Stretch.UniformToFill;
                    App.RootFrame.Background = brush;
                }
            }
        }

        public ICommand GoToAboutFeedback { get; private set; }

        protected PimpViewModel()
            : base(PhoneLocalLibrary.Instance) {

            NowPlayingModel = PhoneNowPlaying.Instance;
            Downloader = new PimpDownloader();
            PhoneLibraryManager.Instance.ActiveEndpointChanged += async e => {
                await ResetAndRefreshRoot();
                OnPropertyChanged("IsLibraryLocal");
            };
        }
        /// <summary>
        /// Sets the app background image to the one specified by the given Uri.
        /// 
        /// I was unable to successfully use an Uri as the data source for the BitmapImage used
        /// as the background image, but it works if I convert the Uri to a StorageFile and then
        /// supply a Stream to the BitmapImage instead of an Uri. Improvements are welcome; is 
        /// the stream disposed of correctly?
        /// </summary>
        /// <param name="imageUri"></param>
        /// <returns></returns>
        public async Task SetBackground(Uri imageUri) {
            var uriString = imageUri.OriginalString;
            // TODO improve predicate
            if(uriString.ToLower().Contains("shellcontent")) {
                await SetBackgroundFromLocalStorage(ToRelativeLocalFolderPath(imageUri));
            } else {
                UpdateBackgroundIfChanged(new BitmapImage(imageUri), uriString);
            }
        }
        private string ToRelativeLocalFolderPath(Uri uri) {
            var localFolderFilePath = uri.OriginalString;
            localFolderFilePath = removePrefixes(localFolderFilePath, "isostore:/", "ms-appdata:///local/");
            localFolderFilePath = localFolderFilePath
                .Replace("shared", "Shared")
                .Replace("shellcontent", "ShellContent");
            return localFolderFilePath;
        }
        private async Task SetBackgroundFromLocalStorage(string localFolderFilePath) {
            if(localFolderFilePath != currentPicturePath) {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.GetFileIfExists(localFolderFilePath);
                if(file != null) {
                    using(var stream = await file.OpenStreamForReadAsync()) {
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        UpdateBackgroundIfChanged(image, localFolderFilePath);
                    }
                }
            }
        }
        private void UpdateBackgroundIfChanged(BitmapImage image, string path) {
            if(currentPicturePath != path) {
                PictureBackground = image;
                currentPicturePath = path;
            }
        }
        private string removePrefixes(string str, params string[] prefixes) {
            var result = str;
            foreach(var prefix in prefixes) {
                if(str.StartsWith(prefix)) {
                    result = result.Substring(prefix.Length);
                }
            }
            return result;
        }
    }
}