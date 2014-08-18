using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Mle.ViewModels;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Mle.MusicPimp.Network;

namespace Mle {
    public partial class PivotPage1 : PhoneApplicationPage {
        public PivotPage1() {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (DataContext == null) {
                DataContext = TestModel.Instance;
            }
        }

        
    }
}