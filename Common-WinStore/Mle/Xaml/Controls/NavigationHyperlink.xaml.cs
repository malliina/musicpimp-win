using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mle.Xaml.Controls {
    public sealed partial class NavigationHyperlink : UserControl {
        public readonly DependencyProperty NavTapCommandProperty =
            DependencyProperty.Register(
                "NavTapCommand",
                typeof(ICommand),
                typeof(NavigationHyperlink),
                new PropertyMetadata(null));

        public readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(string),
                typeof(Image),
                new PropertyMetadata(null));

        public readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register(
                "ButtonContent",
                typeof(string),
                typeof(HyperlinkButton),
                new PropertyMetadata(null));

        public NavigationHyperlink() {
            this.InitializeComponent();
        }
        
        public string ImageSource {
            get { return (string)GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }
        public string ButtonContent {
            get { return (string)GetValue(ButtonContentProperty); }
            set { this.SetValue(ButtonContentProperty, value); }
        }
        public ICommand NavTapCommand {
            get { return (ICommand)GetValue(NavTapCommandProperty); }
            set { this.SetValue(NavTapCommandProperty, value); }
        }

        private void nav_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            if (this.NavTapCommand != null) {
                this.NavTapCommand.Execute(null);
            }
        }
    }
}
