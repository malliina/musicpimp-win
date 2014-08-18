using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.Audio {
    public interface MlePlaylist {
        ObservableCollection<PlaylistMusicItem> Songs { get; }
        bool IsPlaylistEmpty { get; }
        ICommand AddToPlaylistCommand { get; }
        ICommand RemoveFromPlaylistCommand { get; }
        Task AddSong(MusicItem song);
        Task AddSongs(IEnumerable<MusicItem> songs);
        Task RemoveSong(int playlistIndex);
        /// <summary>
        /// Starts playing the song at the specified playlist index.
        /// </summary>
        /// <param name="playlistIndex">index of song to play</param>
        /// <returns></returns>
        Task SkipTo(int playlistIndex);
        Task SetPlaylist(MusicItem song);
        /// <summary>
        /// Loads the playlist from the playback device.
        /// 
        /// Can no-op if the playlist is event-based.
        /// </summary>
        /// <returns></returns>
        Task Load();
    }
}
