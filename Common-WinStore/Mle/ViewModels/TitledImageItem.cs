using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Mle.ViewModels {
    public class TitledImageItem {
        public ImageSource Image { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public Action OnClicked { get; private set; }

        public TitledImageItem(string imageAsset, string title, string subtitle, Action onClicked) {
            Image = new BitmapImage(new Uri(imageAsset, UriKind.Absolute));
            Title = title;
            Subtitle = subtitle;
            Description = String.Empty;
            OnClicked = onClicked;
        }
    }
}
