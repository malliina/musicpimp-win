using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;
using Mle.Collections;

namespace Mle.MusicPimp.Util {
    /// <summary>
    /// Background image logic:
    /// App start with track -> search image, if found, set, otherwise, do nothing
    /// App start without track -> set random
    /// Track changes -> search image, if found, set, otherwise, do nothing 
    /// </summary>
    public abstract class TileAndToastManager {

        public abstract Task Update(MusicItem track);
        public abstract Task UpdateNoTrack();
        /// <summary>
        /// Sets the app background image to the one available at the given uri.
        /// 
        /// The image should be set with an Opacity (~transparency) of 0.1 so as not to
        /// compromise the visibility of UI controls.
        /// </summary>
        /// <param name="uri">Uri to image</param>
        /// <returns></returns>
        public abstract Task SetBackground(Uri uri);
        public CoverService Covers { get; private set; }
        private PlayerManager playerManager;
        public bool IsInitialized { get; private set; }
        protected Uri DefaultBackgroundUri { get; private set; }

        public TileAndToastManager(PlayerManager playerManager, CoverService coverService,Uri defaultBackground) {
            this.playerManager = playerManager;
            Covers = coverService;
            DefaultBackgroundUri = defaultBackground;
            IsInitialized = false;
        }
        /// <summary>
        /// Updates the tile. Called when the app is initialized and whenever the track changes.
        /// 
        /// </summary>
        /// <param name="track">the track, which may be null</param>
        /// <param name="init">true if this method is called because the app is being initialized, false otherwise</param>
        /// <returns></returns>
        public async Task UpdateTileAndBackground(MusicItem track, bool init = false) {
            try {
                if(track != null) {
                    await Update(track);
                    var maybeCover = await Covers.TryGetCover(track);
                    if(maybeCover != null) {
                        await SetBackground(maybeCover);
                    }
                } else {
                    await UpdateNoTrack();
                    if(init) {
                        var uri = await SetRandomBackground();
                        //if(uri == null) {
                        //    await SetBackground(DefaultBackgroundUri);
                        //}
                    }
                }
            } catch(Exception) {
                // suppresses the exception intentionally, what is the user supposed to do anyway if this fails?
                // it fails occasionally with some absurd COM Exception message
                //MessagingService.Instance.Send("Failed to update tile or unable to send toast. " + e.Message);
            }
        }
        /// <summary>
        /// Installs listeners for events that may trigger tile updates.
        /// </summary>
        public async Task Init() {
            if(!IsInitialized) {
                playerManager.PlayerActivated += PlayerActivated;
                playerManager.PlayerDeactivated += PlayerDeactivated;
                var player = playerManager.Player;
                player.TrackChanged += Player_TrackChanged;
                await UpdateTileAndBackground(player.NowPlaying, init: true);
                IsInitialized = true;
            }
        }
        /// <summary>
        /// Sets a random album cover as the app background. Opacity 0.1. Does nothing if
        /// an album cover cannot be found.
        /// </summary>
        /// <returns>a Uri to the background which was set, if any, otherwise null</returns>
        public async Task<Uri> SetRandomBackground() {
            var covers = await Covers.GetCoverCollection(20);
            if(covers.Count > 0) {
                var uri = covers.Head();
                await SetBackground(uri);
                return uri;
            }
            return null;
        }
        private void PlayerActivated(BasePlayer player) {
            player.TrackChanged += Player_TrackChanged;
        }
        private void PlayerDeactivated(BasePlayer player) {
            player.TrackChanged -= Player_TrackChanged;
        }
        private async void Player_TrackChanged(MusicItem track) {
            await UpdateTileAndBackground(track);
        }
    }
}
