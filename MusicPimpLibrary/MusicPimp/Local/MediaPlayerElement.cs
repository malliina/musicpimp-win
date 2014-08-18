using Mle.IO;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Mle.MusicPimp.Local {
    public class MediaPlayerElement : PimpPlayerElement {
        private static MediaPlayerElement instance = null;
        public static MediaPlayerElement Instance {
            get {
                if(instance == null)
                    instance = new MediaPlayerElement();
                return instance;
            }
        }

        public MediaElement Player { get; private set; }
        private MusicItem latestTrack = null;
        private IRandomAccessStream sourceStream = null;

        protected MediaPlayerElement() {
        }
        /// <summary>
        /// Might only be safe to call after the first page Loaded event.
        /// 
        /// The constructor may be called before that however, so we need to call this separately.
        /// </summary>
        public void Init() {
            // Gets a reference to the MediaElement included in the custom style of this app's Frame
            var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
            if(rootGrid == null) {
                throw new PimpException("Unable to resolve root grid that contains MediaElement. Most likely it is illegal to use VisualTreeHelper at this point in time.");
            }
            var mediaElement = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
            Player = mediaElement;
            Player.MediaOpened += (s, e) => UpdateDuration();
            // currently in xaml
            //Player.AudioCategory = AudioCategory.BackgroundCapableMedia;
            //Player.AutoPlay = true;
        }

        private void UpdateDuration() {
            var duration = Player.NaturalDuration;
            if(duration.HasTimeSpan) {
                var timeSpan = duration.TimeSpan;
                // Workaround: Sometimes the MediaElement claims that the duration of the track is about 10 million hours.
                if(timeSpan.TotalSeconds < 36000) {
                    latestTrack.Duration = timeSpan;
                    // bugfix, but why is MusicPlayer.NowPlaying not the same object as latestTrack?
                    // speculation: the NowPlaying is set by a status update, i.e. an object created from the local player info
                    var playerTrack = MusicItemsModel.Instance.MusicPlayer.NowPlaying;
                    if(playerTrack != null) { // should never be null anyway...
                        playerTrack.Duration = timeSpan;
                    }
                }
            }
        }

        public TimeSpan Position {
            get { return Player.Position; }
            set { Player.Position = value; }
        }
        public double Volume {
            get { return Player.Volume; }
            set { Player.Volume = value; }
        }
        public Task Play() {
            return StoreUtil.OnUiThread(Player.Play);
        }

        public Task Pause() {
            return StoreUtil.OnUiThread(Player.Pause);
        }

        public Task Stop() {
            return StoreUtil.OnUiThread(Player.Stop);
        }

        public Task<bool> IsPlaying() {
            return StoreUtil.OnUiCompute<bool>(() => {
                return Player.CurrentState == MediaElementState.Playing;
            });
        }
        public Task<bool> HasTrack() {
            // MediaElementState.Closed === No media
            return StoreUtil.OnUiCompute<bool>(() => {
                return Player.CurrentState != MediaElementState.Closed;
            });
        }
        /// <summary>
        /// Starts playback based on the preferred URI of the supplied track.
        /// 
        /// Note that only remote or app-local URIs can directly be used as 
        /// the Source of the underlying MediaElement. If the URI points to
        /// an non-app-local file path, we need to supply a FileStream as the
        /// MediaElement Source instead of a URI.
        /// 
        /// The app furthermore needs to have been assigned permissions
        /// to access the local source of the track, if any. An exception 
        /// is thrown if that is not the case.
        /// 
        /// </summary>
        /// <param name="track">track to play</param>
        public async Task SetTrack(MusicItem track) {
            DisposeNullifyAndSuppress(sourceStream);
            // check local file availability
            latestTrack = track;
            var maybeLocalUri = await MultiFolderLibrary.Instance.LocalUriIfExists(track);
            Uri sourceUri = maybeLocalUri != null ? maybeLocalUri : track.Source;
            try {
                var file = await StoreFileUtils.GetFile(sourceUri);
                if(file != null) {
                    // Bugfix: setting an ms-appdata URI directly to the Source when 
                    // the music source is remote sometimes throws an exception about Metadata, so we avoid that.
                    // The identical ms-appdata URI seems to work if the music source is local.
                    // The root cause is unresolved atm. 
                    await SetSource(file);
                } else {
                    Player.Source = sourceUri;
                }
            } catch(FileNotFoundException) {
                // StoreFileUtils.GetFile throws FNFE if the file has special characters such as scandics (...) 
                // even if it exists, so this is a bugfix for now
                Player.Source = track.Source;
            }

            // sets universal media control meta data
            MediaControl.TrackName = Utils.EmptyIfNull(track.Name);
            MediaControl.ArtistName = Utils.EmptyIfNull(track.Artist);
        }

        /// <summary>
        /// Resource management is not proper here. Suggest an improvement.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task SetSource(IStorageFile file) {
            var stream = await file.OpenReadAsync();
            sourceStream = stream;
            await StoreUtil.OnUiThread(() => {
                Player.SetSource(stream, stream.ContentType);
            });
        }
        private void DisposeNullifyAndSuppress(IDisposable victim) {
            if(victim != null) {
                try {
                    victim.Dispose();
                } catch(Exception) {

                } finally {
                    victim = null;
                }
            }
        }

        public async Task<MusicItem> CurrentTrack() {
            if(await HasTrack()) {
                return latestTrack;
            } else {
                return null;
            }
        }
    }
}
