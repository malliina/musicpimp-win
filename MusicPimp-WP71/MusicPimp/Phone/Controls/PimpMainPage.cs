using Microsoft.Phone.Controls;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Phone.Controls {
    /// <summary>
    /// The goal was to refactor all of this to class PimpBasePage 
    /// which is shared between the WP7 and WP8 projects.
    /// 
    /// However, I get a build error if Pivot is used from the shared code hence 
    /// this class is duplicated in both WP7 and WP8 projects for now.
    /// 
    /// </summary>
    public abstract class PimpMainPage : PimpBasePage {

        protected abstract Pivot MainPivot();
        
        protected override async Task PrepareSelectedPivot() {
            await WithPivotIndex(MainPivot(), async pivotIndex => {
                ManageRefreshMenuItem(pivotIndex);
                stopUpdateTimerIfAway();
                switch (pivotIndex) {
                    case Pivots.Music:
                        await OnMusicItemsNavigatedTo();
                        break;
                    case Pivots.Player:
                        await OnPlayerNavigatedTo();
                        break;
                    case Pivots.Playlist:
                        await OnPlaylistNavigatedTo();
                        break;
                    case Pivots.Settings:
                        SetAppBarButtons(PlayerButtons());
                        break;
                }
            });
        }
        private void stopUpdateTimerIfAway() {
            var pivotIndex = MainPivot().SelectedIndex;
            if (pivotIndex != Pivots.Player || pivotIndex != Pivots.Playlist) {
                AppModel.NowPlayingModel.StopPolling();
            }
        }
    }
}
