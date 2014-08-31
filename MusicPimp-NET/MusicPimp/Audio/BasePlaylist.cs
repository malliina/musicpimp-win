using Mle.Messaging;
using Mle.MusicPimp.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.Audio {
    public abstract class BasePlaylist : BaseViewModel, MlePlaylist {
        public static readonly int NO_POSITION = -1;

        //public event Action<MusicItem> TrackAdded;

        public ObservableCollection<PlaylistMusicItem> Songs { get; protected set; }

        private int index;
        /// <summary>
        /// Returns the playlist index. An empty playlist implies the index is NO_POSITION,
        /// but an index of NO_POSITION does not imply an empty playlist.
        /// </summary>
        /// <returns>the playlist index, a value of -1 indicates no selection</returns>
        public int Index {
            get { return index; }
            set {
                var changed = SetProperty(ref this.index, value);
                // wtf?
                if (changed) {
                    OnPropertyChanged("NextTrack");
                    OnPropertyChanged("HasNext");
                }
                SyncIndex(value);
                if (changed) {
                    OnIndexChanged(value);
                }
            }
        }
        private bool autoPlayOnSet = false;
        /// <summary>
        /// True if playback automatically starts when the playlist is set; false otherwise.
        /// </summary>
        public bool AutoPlay {
            get { return autoPlayOnSet; }
            set { this.SetProperty(ref this.autoPlayOnSet, value); }
        }
        public event Action<int> IndexChanged;

        public bool IsPlaylistEmpty {
            get { return Songs.Count == 0; }
        }
        public bool ShowPlaylistEmptyText {
            get { return IsPlaylistEmpty && !ShowFeedback && !IsLoading; }
        }
        public MusicItem NextTrack {
            get {
                return (Index >= 0 && Songs.Count > Index + 1) ? Songs[Index + 1].Song : null;
            }
        }
        public bool HasNext {
            get { return NextTrack != null; }
        }
        public bool IsEventBased { get; protected set; }
        public ICommand AddToPlaylistCommand { get; protected set; }
        public ICommand RemoveFromPlaylistCommand { get; protected set; }

        public BasePlaylist() {
            IsEventBased = false;
            Songs = new ObservableCollection<PlaylistMusicItem>();
            Songs.CollectionChanged += (s, e) => {
                OnPropertyChanged("IsPlaylistEmpty");
                OnPropertyChanged("NextTrack");
                OnPropertyChanged("HasNext");
                OnPropertyChanged("ShowPlaylistEmptyText");
            };
            AddToPlaylistCommand = new AsyncDelegateCommand<MusicItem>(OnAddTrack);
            RemoveFromPlaylistCommand = new AsyncDelegateCommand<PlaylistMusicItem>(song => {
                return OnRemoveSong(song.Index);
            });
        }
        protected override void OnIsLoadingChanged(bool loading) {
            base.OnIsLoadingChanged(loading);
            OnPropertyChanged("ShowPlaylistEmptyText");
        }
        protected override void OnFeedbackMessageChanged(string msg) {
            base.OnFeedbackMessageChanged(msg);
            OnPropertyChanged("ShowPlaylistEmptyText");
        }
        /// <summary>
        /// Skips to the song with the given index in the playlist, starting playback.
        /// 
        /// </summary>
        /// <param name="playlistIndex"></param>
        /// <returns></returns>
        protected abstract Task SendSkipCommand(int playlistIndex);
        /// <summary>
        /// Removes the song with the given index from the playlist.
        /// 
        /// TODO remove internal suffix.
        /// </summary>
        /// <param name="playlistIndex">index of track to remove</param>
        /// <returns></returns>
        protected abstract Task RemoveSongInternal(int playlistIndex);
        public abstract Task SetPlaylist(MusicItem song);
        public abstract Task LoadData();
        public abstract Task AddSong(MusicItem song);
        public virtual async Task AddSongs(IEnumerable<MusicItem> songs) {
            foreach(var song in songs) {
                await AddSong(song);
            }
        }
        public async Task WithOOMGuard(Func<Task> code) {
            try {
                await code();
            } catch(OutOfMemoryException) {
                MessagingService.Instance.Send("An error occurred. The file might be too large.");
            }
        }
        public Task OnAddTrack(MusicItem track) {
            return UsageControlled(() => AddTrack(track));
        }
        public async Task AddTrack(MusicItem track) {
            await AddSong(track);
            //OnTrackAdded(track);
        }

        public Task Load() {
            return WebAware(LoadData);
        }
        public Task OnRemoveSong(int index) {
            return UsageControlled(() => RemoveSong(index));
        }
        public async Task RemoveSong(int playlistIndex) {
            await RemoveSongInternal(playlistIndex);
            if (!IsEventBased) {
                await Load();
            }
        }
        public virtual async Task SkipTo(int playlistIndex) {
            await SendSkipCommand(playlistIndex);
            if (!IsEventBased) {
                Index = playlistIndex;
            }
        }

        private void SyncIndex(int index) {
            foreach (var song in Songs) {
                song.IsSelected = false;
            }
            SetSelected(index);
        }
        private void SetSelected(int index) {
            var songCount = Songs.Count;
            if (index >= 0 && index < songCount) {
                Songs[index].IsSelected = true;
            }
        }
        public void Sync(IEnumerable<MusicItem> tracks, int index) {
            Sync(tracks);
            OnUiThread(() => Index = index);
        }
        public void Sync(IEnumerable<MusicItem> tracks) {
            var playlistItems = new List<PlaylistMusicItem>();
            // don't Select (are you afraid? -M)
            int i = 0;
            foreach (var track in tracks) {
                playlistItems.Add(new PlaylistMusicItem(track, i++));
            }
            SyncPlaylist(playlistItems);
        }
        /// <summary>
        /// Takes a playlist from another source and ensures that the local ObservableCollection of Songs is synchronized.
        /// 
        /// Does not clear-and-replace because that may affect the UI if the user browses the playlist at the same time.
        /// 
        /// </summary>
        /// <param name="actualTracks"></param>
        /// <param name="selectedIndex"></param>
        protected void SyncPlaylist(IEnumerable<PlaylistMusicItem> actualTracks, int selectedIndex) {
            SyncPlaylist(actualTracks);
            Index = selectedIndex;
        }
        protected void SyncPlaylist(IEnumerable<PlaylistMusicItem> tracks) {
            var trackCount = tracks.Count();
            var iterIndex = 0;
            foreach (var song in tracks) {
                if (Songs.Count <= iterIndex) {
                    Songs.Add(song);
                } else {
                    var existingItem = Songs[iterIndex];
                    if (song.Song.Id != existingItem.Song.Id || song.Index != existingItem.Index) {
                        Songs.Insert(iterIndex, song);
                    } else {
                        existingItem.Index = iterIndex;
                    }
                }
                iterIndex++;
            }
            while (Songs.Count > trackCount) {
                Songs.RemoveAt(trackCount);
            }
        }
        //private void OnTrackAdded(MusicItem track) {
        //    if (TrackAdded != null) {
        //        TrackAdded(track);
        //    }
        //}
        private void OnIndexChanged(int index) {
            if (IndexChanged != null) {
                IndexChanged(index);
            }
        }
        public static void SetDownloadStatus(IEnumerable<PlaylistMusicItem> items, ulong bytesReceived) {
            foreach (var item in items) {
                MusicItem.SetDownloadStatus(item.Song, bytesReceived);
            }
        }
    }
}
