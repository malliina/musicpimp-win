using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public interface MlePlayer {
        BasePlaylist Playlist { get; }
        /// <summary>
        /// Called after the user has skipped to an index in the playlist.
        /// 
        /// If the skip implementation doesn't start playback automatically, do so here.
        /// </summary>
        /// <returns></returns>
        Task playPlaylist();
        /// <summary>
        /// plays the currently selected track
        /// </summary>
        Task play();
        /// <summary>
        /// pauses the track
        /// </summary>
        Task pause();
        /// <summary>
        /// Starts playing the next track
        /// </summary>
        Task next();
        /// <summary>
        /// Starts playing the previous track
        /// </summary>
        Task previous();
        /// <summary>
        /// Seeks.
        /// </summary>
        /// <param name="pos">the position to seek to, in seconds</param>
        /// <returns></returns>
        Task seek(double pos);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newVolume">between 0 and 100</param>
        /// <returns></returns>
        Task SetVolume(int newVolume);
        Task<PlaybackStatus> Status();
    }
}
