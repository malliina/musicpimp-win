using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mle.Xaml.Controls {
    // adapted for WinRT from http://blog.mrlacey.co.uk/2013/06/tap-click-command-or-select-how-to.html
    public partial class TapCommandControl : UserControl {
        public readonly DependencyProperty TapCommandProperty =
            DependencyProperty.Register(
                "TapCommand",
                typeof(ICommand),
                typeof(TapCommandControl),
                new PropertyMetadata(null));

        public readonly DependencyProperty TapCommandParameterProperty =
            DependencyProperty.Register(
                "TapCommandParameter",
                typeof(object),
                typeof(TapCommandControl),
                new PropertyMetadata(null));

        public TapCommandControl() {
            InitializeComponent();
        }

        public ICommand TapCommand {
            get { return (ICommand)GetValue(TapCommandProperty); }
            set { this.SetValue(TapCommandProperty, value); }
        }

        public object TapCommandParameter {
            get { return GetValue(TapCommandParameterProperty); }
            set { this.SetValue(TapCommandParameterProperty, value); }
        }

        private void UserControl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            if (this.TapCommand != null) {
                this.TapCommand.Execute(this.TapCommandParameter);
            }
        }

    }
}
