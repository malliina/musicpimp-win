using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mle.MusicPimp.Xaml.Controls {
    public sealed partial class EndpointPanel : UserControl {
        public EditEndpoint ViewModel { get; private set; }
        public EndpointPanel() { }
        public EndpointPanel(string endpointName) {
            ViewModel = new StoreEditEndpoint(endpointName);
            DataContext = ViewModel;
            this.InitializeComponent();
        }
        private async void EndpointChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e) {
            await SaveChanges();
        }
        private async void EndpointChanged(object sender, RoutedEventArgs e) {
            await SaveChanges();
        }
        private async void ServerTypeChanged(object sender, RoutedEventArgs e) {
            ViewModel.Update();
            await SaveChanges();
        }
        private async Task SaveChanges() {
            await ViewModel.SubmitChanges();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            ViewModel.Update();
        }

        private void CloudTextChanged(object sender, TextChangedEventArgs e) {
            var updatedText = CloudBox.Text;
            ViewModel.SyncDescriptionWithCloud(updatedText);
        }
    }
}
