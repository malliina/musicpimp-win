using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mle.MusicPimp.Controls {
    public partial class PlaylistItemPanel : UserControl {
        public readonly DependencyProperty PlaylistContextProperty =
            DependencyProperty.Register(
                "PlaylistContext",
                typeof(object),
                typeof(PlaylistItemPanel),
                new PropertyMetadata(null));

        public PlaylistItemPanel() {
            InitializeComponent();
        }
        public PimpViewModel AppModel {
            get { return PimpViewModel.Instance; }
        }

        public object PlaylistContext {
            get { return GetValue(PlaylistContextProperty); }
            set { this.SetValue(PlaylistContextProperty, value); }
        }
    }
}
