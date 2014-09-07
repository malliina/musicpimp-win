using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.Network {
    /// <summary>
    /// The Submit* methods do not await the completion of the download while the DownloadAsync* methods do.
    /// </summary>
    public interface IDownloader {
        ICommand Download { get; }
        /// <summary>
        /// Submits item for download, whether it's a track or a folder.
        /// 
        /// The returned task is complete once the submission is; it does not wait for the download(s) to complete.
        /// 
        /// Does not download anything if the items are already locally available.
        /// </summary>
        /// <param name="item">track or folder to download</param>
        /// <returns></returns>
        Task SubmitDownload(MusicItem item);
        Task SubmitDownloads(IEnumerable<MusicItem> items);
        /// <summary>
        /// Downloads the specified track, returning the local URI to the downloaded track once it's complete.
        /// 
        /// Does not download anything if the track is already locally available.
        /// </summary>
        /// <param name="track">track to download</param>
        /// <returns>the local URI to the track</returns>
        Task<Uri> DownloadAsync(MusicItem track);
        /// <summary>
        /// Submits the track for download as a background task.
        /// 
 
        /// </summary>
        /// <param name="track">track to download</param>
        /// <param name="username">basic auth user</param>
        /// <param name="password">basic auth pass</param>
        /// <returns></returns>
        //Task SubmitDownload(MusicItem track, string username, string password);
        //Task SubmitDownloads(IEnumerable<MusicItem> tracks, string username, string password);
    }
}
