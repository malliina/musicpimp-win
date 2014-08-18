using Mle.MusicPimp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mle.MusicPimp.Xaml.Controls {
    public partial class SearchAndAddEndpointsGrid : UserControl {
        public SearchAndAddEndpointsGrid() {
            InitializeComponent();
            // PhoneEndpoints uses isolated storage, which fucks up visual studio
            // during design time, but this condition makes the errors go away.
            if(!System.ComponentModel.DesignerProperties.IsInDesignTool) {
                (this.Content as FrameworkElement).DataContext = PhoneEndpoints.Instance;
            }
        }
    }
}
