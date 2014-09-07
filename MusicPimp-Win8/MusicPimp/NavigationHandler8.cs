using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mle.MusicPimp {
    public class NavigationHandler8 : NavigationHandler {
        public NavigationHandler8(Frame frame)
            : base(frame) {
            pageIdResolver.Add(PageNames.LIBRARY, typeof(MusicItems));
            pageIdResolver.Add(PageNames.PLAYER, typeof(Player));
        }
    }
}
