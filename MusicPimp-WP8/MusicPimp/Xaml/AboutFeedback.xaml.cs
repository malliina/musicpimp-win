using Microsoft.Phone.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    public partial class AboutFeedback : PhoneApplicationPage {
        public AboutFeedback() {
            InitializeComponent();
            DataContext = AboutViewModel.Instance;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
        }
    }
}