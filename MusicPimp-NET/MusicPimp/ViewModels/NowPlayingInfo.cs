using Mle.MusicPimp.Audio;
using Mle.ViewModels;

namespace Mle.MusicPimp.ViewModels {
    public abstract class NowPlayingInfo : WebAwareLoading {

        public abstract PlayerManager PlayerManager { get; }

        public BasePlayer Player {
            get { return PlayerManager.Player; }
        }

        public void StartPollingIfNeeded() {
            if (Player != null && !Player.IsEventBased) {
                StartPolling();
            }
        }
        public void StopPollingIfNeeded() {
            if (Player != null && !Player.IsEventBased) {
                StopPolling();
            }
        }
        public abstract void StartPolling();
        public abstract void StopPolling();

        protected async void Timer_Tick(object sender, object e) {
            await PlayerManager.Player.UpdateNowPlaying();
        }
    }
}
