using Microsoft.Phone.BackgroundAudio;
using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public class PhonePlayerElement : PimpPlayerElement {
        private static PhonePlayerElement instance = null;
        public static PhonePlayerElement Instance {
            get {
                if(instance == null)
                    instance = new PhonePlayerElement();
                return instance;
            }
        }
        private BackgroundAudioPlayer Player {
            get { return BackgroundAudioPlayer.Instance; }
        }

        protected PhonePlayerElement() { }

        // SystemException is thrown for some tracks when streamed from a Subsonic server.
        private T WithErrorHandling<T>(Func<T> code) {
            try {
                return code();
            } catch(SystemException) {
                var trackInfo = "";
                var track = Player.Track;
                if(track != null) {
                    var title = track.Title;
                    if(!string.IsNullOrWhiteSpace(title)) {
                        trackInfo = " while playing " + title;
                    }
                }
                throw new FriendlyException("The audio player encountered an error" + trackInfo + ". Please try another track.");
            }
        }

        public TimeSpan Position {
            get { return WithErrorHandling(() => Player.Position); }
            set { Player.Position = value; }
        }
        public double Volume {
            get { return WithErrorHandling<double>(() => Player.Volume); }
            set { Player.Volume = value; }
        }
        public Task Play() {
            Player.Play();
            return AsyncTasks.Noop();
        }
        public Task Pause() {
            Player.Pause();
            return AsyncTasks.Noop();
        }
        public Task Stop() {
            Player.Stop();
            return AsyncTasks.Noop();
        }
        public Task<bool> IsPlaying() {
            return TaskEx.FromResult(Player.PlayerState == PlayState.Playing);
        }
        public Task<bool> HasTrack() {
            return TaskEx.FromResult(Player.Track != null);
        }
        public async Task SetTrack(MusicItem track) {
            Uri localUri = await PhoneLocalLibrary.Instance.LocalUriIfExists(track);
            Uri source = localUri != null ? localUri : track.Source;
            Player.Track = AudioItemConversions.item2track(track, source);
        }
        public async Task<MusicItem> CurrentTrack() {
            var t = Player.Track;
            if(t != null) {
                return await TaskEx.FromResult(AudioItemConversions.track2item(t));
            } else {
                return null;
            }
        }
    }
}
