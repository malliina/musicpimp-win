using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mle.MusicPimp {
    public class NavigationHandler81 : NavigationHandler {
        public NavigationHandler81(Frame frame)
            : base(frame) {
            pageIdResolver.Add(PageNames.LIBRARY, typeof(MusicItems));
            pageIdResolver.Add(PageNames.PLAYER, typeof(Player));
            pageIdResolver.Add(PageNames.SEARCH, typeof(Search));
        }
    }
}
