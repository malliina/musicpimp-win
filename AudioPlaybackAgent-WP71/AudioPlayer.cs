using Microsoft.Phone.BackgroundAudio;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Database;
using System;
using System.Diagnostics;
using System.Windows;

namespace AudioPlaybackAgent {
    public class AudioPlayer : AudioPlayerAgent {
        /// <remarks>
        /// AudioPlayer instances can share the same process.
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        static AudioPlayer() {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate {
                Application.Current.UnhandledException += UnhandledException;
            });
        }
        public static AudioTrack track { get; set; }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            //AudioUtils.HandleException(BackgroundAudioPlayer.Instance, BackgroundAudioPlayer.Instance.Track, e.ExceptionObject);
            if (Debugger.IsAttached) {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        ///
        /// Notable playstate events:
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        ///
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState) {
            switch (playState) {
                case PlayState.TrackEnded:
                    player.Track = GetNextTrack();
                    player.Play();
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        ///
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param) {
            try {
                switch (action) {
                    case UserAction.Play:
                        player.Play();
                        break;
                    case UserAction.Stop:
                        player.Stop();
                        break;
                    case UserAction.Pause:
                        player.Pause();
                        break;
                    case UserAction.FastForward:
                        player.FastForward();
                        break;
                    case UserAction.Rewind:
                        player.Rewind();
                        break;
                    case UserAction.Seek:
                        try {
                            player.Position = (TimeSpan)param;
                        } catch (InvalidOperationException) {
                            // thrown occasionally. what to do?
                        }
                        break;
                    case UserAction.SkipNext:
                        var maybeNext = GetNextTrack();
                        if (maybeNext != null) {
                            player.Track = maybeNext;
                            // no need to play as playback is started when the track is "ready"
                            //player.Play();
                        }
                        break;
                    case UserAction.SkipPrevious:
                        var maybePrevious = GetPreviousTrack();
                        if (maybePrevious != null) {
                            player.Track = maybePrevious;
                            //player.Play();
                        }
                        break;
                }
            } catch (Exception) {
                // Might throw SystemException on some rare occasions? 
                // TODO: reproduce, handle exception
                var tmp = 0;
            }
            NotifyComplete();
        }

        /// <summary>
        /// Implements the logic to get the next AudioTrack instance.
        /// In a playlist, the source can be from a file, a web request, etc.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if the playback is completed</returns>
        private AudioTrack GetNextTrack() {
            return ToAudioTrack(PlaylistDatabase.Next());
        }

        /// <summary>
        /// Implements the logic to get the previous AudioTrack instance.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if previous track is not allowed</returns>
        private AudioTrack GetPreviousTrack() {
            return ToAudioTrack(PlaylistDatabase.Previous());
        }
        private AudioTrack ToAudioTrack(PlaylistTrack track) {
            if (track != null)
                return track.ToTrack();
            else
                return null;
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal) {
            var tmp = 0;
            if (isFatal) {
                Abort();
            } else {
                NotifyComplete();
            }
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel() {
            var tmp = 0;
        }
    }
}