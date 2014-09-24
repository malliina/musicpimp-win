using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Iap;
using Mle.MusicPimp.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.Audio {
    public abstract class BasePlayer : BaseViewModel, MlePlayer {

        public bool IsEventBased { get; protected set; }
        public bool IsSkipAndSeekSupported { get; protected set; }

        public virtual Task Subscribe() {
            return TaskEx.FromResult(0);
        }
        public virtual void Unsubscribe() {

        }
        //public event Action<double> TrackProgressUpdated;
        public event Action<MusicItem> TrackChanged;
        public event Action<PlayerState> PlayerStateChanged;
        public event Action<int> VolumeChanged;
        public event Action<bool> MuteToggled;
        public event Action<bool> IsPlayingChanged;

        private PlayerState currentPlayerState = PlayerState.Other;
        public PlayerState CurrentPlayerState {
            get { return currentPlayerState; }
            set {
                if(this.SetProperty(ref this.currentPlayerState, value)) {
                    OnPlayerStateChanged(value);
                    if(value == PlayerState.Playing) {
                        IsPlaying = true;
                    }
                    if(value == PlayerState.Closed || value == PlayerState.Paused || value == PlayerState.Stopped || value == PlayerState.Ended) {
                        IsPlaying = false;
                    }
                }
            }
        }
        private bool isPlaying;
        public bool IsPlaying {
            get { return isPlaying; }
            set {
                if(SetProperty(ref isPlaying, value)) {
                    OnIsPlayingChanged(value);
                }
            }
        }
        private bool isMute;
        public bool IsMute {
            get { return isMute; }
            set {
                if(SetProperty(ref this.isMute, value)) {
                    OnMuteToggled(value);
                }
            }
        }
        private int volumeBeforeMute = 40;
        private int volume;
        public int Volume {
            get { return volume; }
            set {
                if(SetProperty(ref this.volume, value)) {
                    OnVolumeChanged(value);
                }
            }
        }
        private MusicItem nowPlaying;
        public MusicItem NowPlaying {
            get { return nowPlaying; }
            set {
                if(SetProperty(ref this.nowPlaying, value)) {
                    OnPropertyChanged("IsTrackAvailable");
                    //Debug.WriteLine("Track changed to: " + value.Name + " with duration: " + value.Duration);
                    if(NowPlaying != null) {
                        FeedbackMessage = null;
                    }
                    OnTrackChanged(NowPlaying);
                    //PlaybackStarted();
                }
            }
        }
        public bool IsTrackAvailable {
            get { return NowPlaying != null; }
        }
        private TimeSpan trackPosition;
        public TimeSpan TrackPosition {
            get { return trackPosition; }
            set { SetProperty(ref this.trackPosition, value); }
        }

        public ICommand ToggleMute { get; private set; }
        public ICommand SkipPrevious { get; private set; }
        public ICommand PlayOrPause { get; private set; }
        public ICommand SkipNext { get; private set; }
        public ICommand PlayPlaylistItem { get; private set; }
        public ICommand PlayTrack { get; private set; }

        public BasePlayer() {
            IsEventBased = false;
            IsSkipAndSeekSupported = true;
            PlayOrPause = new AsyncUnitCommand(OnPlayOrPause);
            SkipPrevious = new AsyncUnitCommand(OnPrev);
            SkipNext = new AsyncUnitCommand(OnNext);
            PlayPlaylistItem = new AsyncDelegateCommand<PlaylistMusicItem>(async item => {
                await WithExceptionEvents2(PlaybackStarted);
                await PlayIndex(item.Index);
            });
            ToggleMute = new AsyncUnitCommand(async () => await HandleToggleMute(!IsMute));
            PlayTrack = new AsyncDelegateCommand<MusicItem>(PlaySong);
        }
        public virtual async Task TryToConnect() {
            try {
                await Subscribe();
                IsOnline = true;
            } catch(Exception) {
                IsOnline = false;
            }
        }
        private void PlaybackStarted() {
            UsageController.Instance.PlaybackStarted();
        }

        public virtual async Task HandleToggleMute(bool newMuteValue) {
            // simulates mute toggle with 0-volume
            if(newMuteValue) {
                volumeBeforeMute = Volume;
                await SetVolume(0);
            } else {
                await SetVolume(volumeBeforeMute);
            }
            IsMute = newMuteValue;
        }
        private async Task ExecutePlayOrPause() {
            if(IsPlaying) {
                await pause();
            } else {
                await play();
            }
        }
        public Task OnPlayOrPause() {
            return WithExceptionEvents(ExecutePlayOrPause);
        }
        public Task OnPrev() {
            return UsageControlled(async () => {
                PlaybackStarted();
                await Suppress<BadRequestException>(previous);
            });
        }
        public Task OnNext() {
            return UsageControlled(async () => {
                PlaybackStarted();
                // BRE may be thrown if there's no next track
                await Suppress<BadRequestException>(next);
            });
        }

        public Task PlayPlaylist(List<MusicItem> tracks) {
            return UsageControlled(async () => {
                var trackCount = tracks.Count;
                if(trackCount > 0) {
                    await PlaySong(tracks[0]);
                    var tail = tracks.Skip(1).ToList();
                    await Playlist.AddSongs(tail);
                }
            });
        }
        private Task PlayIndex(int playlistIndex) {
            return UsageControlled(async () => {
                await Playlist.SkipTo(playlistIndex);
                await playPlaylist();
            });
        }
        // TODO clarify difference to playPlaylist
        public Task PlaySong(MusicItem song) {
            return UsageControlled(async () => {
                try {
                    await Playlist.SetPlaylist(song);
                    if(!Playlist.AutoPlay) {
                        await PlayIndex(0);
                    }
                    PlaybackStarted();
                } catch(InvalidOperationException ioe) {
                    if(ioe.Message != "The request has already been canceled") {
                        throw;
                    } else {
                        // Suppresses the exception thrown when the user manually cancels a download.
                        // SetPlaylist may throw this if a download is required before the playlist
                        // can be initialized... TODO: obviously fix this bs code.
                    }
                }
            });
        }
        protected async Task Suppress<T>(Func<Task> code) where T : Exception {
            try {
                await code();
            } catch(T) {

            }
        }

        public abstract BasePlaylist Playlist { get; protected set; }

        public virtual Task playPlaylist() {
            return AsyncTasks.Noop();
        }

        public abstract Task play();

        public abstract Task pause();

        /// <summary>
        /// Skips to the next track AND starts playback asap,
        /// that is, when the track is opened.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="BadRequestException">if there is no next track</exception>
        public abstract Task next();

        /// <summary>
        /// Skips to the previous track and starts playback asap,
        /// that is, when the track is opened.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="BadRequestException">if there is no previous track</exception>
        public abstract Task previous();

        //public abstract Task<bool> playing();

        public abstract Task seek(double pos);

        public abstract Task SetVolume(int newVolume);

        public abstract Task<PlaybackStatus> Status();

        public async Task UpdateNowPlaying() {
            await WebAware(async () => {
                var status = await Status();
                UpdateStatus(status);
            });
        }
        public virtual void UpdateStatus(PlaybackStatus status) {
            Playlist.Sync(status.Playlist, status.PlaylistIndex);
            var previousTrack = NowPlaying;
            var track = status.Track;
            // avoids triggering unnecessary TrackChanged events
            // TODO so if the same track is queued in the playlist...
            if(track == null || previousTrack == null || track.Name != previousTrack.Name) {
                NowPlaying = track;
            }
            Volume = status.Volume;
            IsMute = status.IsMute;
            if(NowPlaying != null) {
                TrackPosition = status.TrackPosition;
                var plist = status.Playlist;
                var playlistIndex = status.PlaylistIndex;
                var songCount = plist.Count();
                var nextIndex = playlistIndex + 1;
                MusicItem nextTrack = null;
                if(nextIndex >= 0 && nextIndex < songCount) {
                    nextTrack = plist.ElementAt(nextIndex);
                }
                CurrentPlayerState = status.State;
                FeedbackMessage = null;
            } else {
                CurrentPlayerState = PlayerState.Closed;
            }
        }
        private void OnPlayerStateChanged(PlayerState state) {
            if(PlayerStateChanged != null) {
                PlayerStateChanged(state);
            }
        }
        private void OnVolumeChanged(int newVolume) {
            if(VolumeChanged != null) {
                VolumeChanged(newVolume);
            }
        }
        private void OnMuteToggled(bool newValue) {
            if(MuteToggled != null) {
                MuteToggled(newValue);
            }
        }
        private void OnIsPlayingChanged(bool isPlaying) {
            if(IsPlayingChanged != null) {
                IsPlayingChanged(isPlaying);
            }
        }
        private void OnTrackChanged(MusicItem newTrack) {
            if(TrackChanged != null) {
                TrackChanged(newTrack);
            }
        }
    }
}
