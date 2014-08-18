using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mle.Xaml.Controls {
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

        public void ControlTapped(object sender, GestureEventArgs e) {
            if (TapCommand != null) {
                TapCommand.Execute(TapCommandParameter);
            }
        }

    }
}
