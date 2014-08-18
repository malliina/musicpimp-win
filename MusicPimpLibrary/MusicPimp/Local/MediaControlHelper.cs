using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System.Threading.Tasks;
using Windows.Media;

namespace Mle.MusicPimp.Local {
    public class MediaControlHelper {
        private static BasePlayer Player {
            get { return MusicItemsModel.Instance.MusicPlayer; }
        }
        /// <summary>
        /// Assigns event handlers to user-initiated play state changes. 
        /// 
        /// Assigning handlers to the events is required for background audio, 
        /// regardless of whether we care about the events or not.
        /// </summary>
        public static void InitMediaControls() {
            MediaControl.PlayPauseTogglePressed += async (s, e) => await OnUi(Player.OnPlayOrPause());
            MediaControl.PlayPressed += async (s, e) => await OnUi(Player.play());
            MediaControl.PausePressed += async (s, e) => await OnUi(Player.pause());
            MediaControl.StopPressed += async (s, e) => await OnUi(Player.pause());
            MediaControl.NextTrackPressed += async (s, e) => await OnUi(Player.OnNext());
            MediaControl.PreviousTrackPressed += async (s, e) => await OnUi(Player.OnPrev());
        }

        private static Task OnUi(Task t) {
            return StoreUtil.OnUiThread(async () => await t);
        }
    }
}
