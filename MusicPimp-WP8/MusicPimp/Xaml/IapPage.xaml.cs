using Microsoft.Phone.Controls;
using Mle.MusicPimp.ViewModels;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    public partial class IapPage : PhoneApplicationPage {
        public IapPage() {
            InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            var vm = PhoneIAP.Current;
            DataContext = vm;
            await vm.UpdateStatusAsync();
            base.OnNavigatedTo(e);
        }
    }
}