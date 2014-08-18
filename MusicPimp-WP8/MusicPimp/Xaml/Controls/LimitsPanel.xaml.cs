using Mle.MusicPimp.ViewModels;
using System.Windows.Controls;

namespace Mle.MusicPimp.Controls {
    public partial class LimitsPanel : UserControl {
        public LimitsPanel() {
            InitializeComponent();
            DataContext = SettingsOverview.Instance.Limits;
        }
    }
}
