using System.Windows;
using System.Windows.Controls;

namespace Mle.Xaml.Controls {
    public partial class LabeledProgressBar : UserControl {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LabeledProgressBar), new PropertyMetadata("Loading..."));

        public LabeledProgressBar() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }
        public string Label {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

    }
}
