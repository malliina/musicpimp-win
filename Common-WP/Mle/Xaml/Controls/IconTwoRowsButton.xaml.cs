using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mle.Xaml.Controls {
    public partial class IconTwoRowsButton : UserControl {
        public IconTwoRowsButton() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }
        public string FirstRow {
            get { return (string)GetValue(FirstRowProperty); }
            set { SetValue(FirstRowProperty, value); }
        }
        public static readonly DependencyProperty FirstRowProperty =
            DependencyProperty.Register("FirstRow", typeof(string), typeof(IconTwoRowsButton), new PropertyMetadata("first row here"));
        public string SecondRow {
            get { return (string)GetValue(SecondRowProperty); }
            set { SetValue(SecondRowProperty, value); }
        }
        public static readonly DependencyProperty SecondRowProperty =
            DependencyProperty.Register("SecondRow", typeof(string), typeof(IconTwoRowsButton), new PropertyMetadata("second row here"));
        public ImageSource LeftImage {
            get { return (ImageSource)GetValue(LeftImageProperty); }
            set { SetValue(LeftImageProperty, value); }
        }
        public static readonly DependencyProperty LeftImageProperty =
            DependencyProperty.Register("LeftImage", typeof(ImageSource), typeof(IconTwoRowsButton), new PropertyMetadata(null));
    }
}
