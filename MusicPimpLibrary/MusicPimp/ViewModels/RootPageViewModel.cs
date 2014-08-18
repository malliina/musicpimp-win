using Mle.MusicPimp.Tiles;
using Mle.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Mle.MusicPimp.ViewModels {
    public class RootPageViewModel : Navigable {
        private static RootPageViewModel instance = null;
        public static RootPageViewModel Instance {
            get {
                if(instance == null) {
                    instance = new RootPageViewModel();
                }
                return instance;
            }
        }
        public ObservableCollection<TitledImageItem> NavItems { get; private set; }
        private static BitmapImage ImageFrom(string path) {
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
        private string currentPictureUri = null;
        private ImageSource pictureBackground;
        public ImageSource PictureBackground {
            get { return pictureBackground; }
            set { SetProperty(ref pictureBackground, value); }
        }
        public RootPageViewModel() {
            NavItems = new ObservableCollection<TitledImageItem>() {
                HomeItem, SmallFolderItem, PlayerItem, DownloadsItem
            };
        }
        public void SetBackgroundUri(Uri uri){
            var uriString = uri.OriginalString;
            if(uriString != currentPictureUri) {
                PictureBackground = new BitmapImage(uri);
                currentPictureUri = uri.OriginalString;
            }
        }
    }
}
