using Mle.MusicPimp.ViewModels;
using Mle.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mle.MusicPimp.Xaml {
    public sealed partial class AddEndpointPopup : PopupUserControl {
        private EndpointEditorViewModel vm;
        public AddEndpointPopup() : this(new NewEndpoint()) { }
        public AddEndpointPopup(EndpointEditorViewModel e) {
            vm = e;
            this.DataContext = vm;
            vm.CloseRequested += () => {
                CloseThisPopup();
            };
            this.InitializeComponent();
        }

        private void RadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            vm.Update();
        }

        private void CloudTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e) {
            vm.SyncDescriptionWithCloud(CloudTextBox.Text);
        }
    }
}
