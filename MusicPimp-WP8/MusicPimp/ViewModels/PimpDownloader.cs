using Microsoft.Phone.BackgroundTransfer;
using Mle.Collections;
using Mle.Concurrent;
using Mle.IO;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.Network;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class PimpDownloader : ExceptionAwareTransferModel, IDownloader {
        private PhoneLocalLibrary LocalLibrary {
            get { return PhoneLocalLibrary.Instance; }
        }
        private MusicLibrary MusicProvider {
            get { return PimpViewModel.Instance.MusicProvider; }
        }

        public ICommand DownloadMusicItem { get; private set; }

        public PimpDownloader() {
            DownloadMusicItem = new AsyncDelegateCommand<MusicItem>(ValidateThenSubmitDownload);
            var t = Init();
        }
        public Task SubmitDownloads(IEnumerable<MusicItem> tracks, string username, string password) {
            return SubmitDownloads(tracks);
        }
        public Task SubmitDownload(MusicItem track, string username, string password) {
            return SubmitDownload(track);
        }
        /// <summary>
        /// Downloads the song in the background if it's not already available offline.
        /// </summary>
        /// <param name="track"></param>
        public async Task SubmitDownload(MusicItem track) {
            await BeforeDownload(track);
            try {
                var maybeLocalUri = await PhoneLocalLibrary.Instance.LocalUriIfExists(track);
                if(maybeLocalUri == null) {
                    // For Subsonic, the response may be transcoded audio, in which case the 
                    // path to the track, which has the original file extension as stored on
                    // the server, may be incorrect (example: response is transcoded to .mp3, 
                    // path is .flac).

                    // TODO: Ensure that the file is stored locally with the correct extension, 
                    // that is, find out whether the response is transcoded.
                    var destination = LocalLibrary.AbsolutePathTo(track);
                    var downloadUri = MusicProvider.DownloadUriFor(track);
                    // Only downloads tracks that are stored as MP3s, because this app does
                    // not support other local file formats.
                    if(destination.EndsWith("mp3")) {
                        var downloadable = new Downloadable(downloadUri, destination);
                        if(LoadTransfersCount() < 3) {
                            AddTransfer(downloadUri, destination);
                        } else {
                            // add the download to persistent storage from which it will be taken
                            // later when there are fewer concurrent downloads
                            DownloadDataContext.Add(downloadable);
                        }
                    }
                }
            } catch(PathTooLongException) {
                // Thrown if track.Path is about over 190 characters long, but I'm not sure what
                // the limit is and I don't want to be too defensive with this so I catch and 
                // suppress the exception when it occurs.

                // The exception says "The specified path, file name, or both are too long. 
                // The fully qualified file name must be less than 260 characters, and the 
                // directory name must be less than 248 characters.", however, I don't know 
                // the length of the fully qualified path name, so 190 chars is an estimate.
                AddMessage("Download of " + track.Name + " failed. The path is too long: " + track.Path);
            }
        }
        public async Task SubmitDownloads(IEnumerable<MusicItem> tracks) {
            await BeforeDownload(tracks);
            try {
                var remoteTracks = await tracks.FilterAsync(async track => await PhoneLocalLibrary.Instance.LocalUriIfExists(track) == null);
                var downloadables = remoteTracks
                    .Select(track => new Downloadable(MusicProvider.DownloadUriFor(track), LocalLibrary.AbsolutePathTo(track)))
                    .Where(d => d.Destination.EndsWith("mp3"))
                    .ToList();
                var transfers = LoadTransfersCount();
                var queueables = (transfers < 3 ? downloadables.Skip(3) : downloadables).ToList();
                if(transfers < 3) {
                    foreach(var dl in downloadables.Take(3).ToList()) {
                        AddTransfer(dl.Source, dl.Destination);
                    }
                }
                if(queueables.Count > 0) {
                    // add the download to persistent storage from which it will be taken
                    // later when there are fewer concurrent downloads
                    DownloadDataContext.AddAll(queueables);
                }
            } catch(PathTooLongException) {
                // Thrown if track.Path is about over 190 characters long, but I'm not sure what
                // the limit is and I don't want to be too defensive with this so I catch and 
                // suppress the exception when it occurs.

                // The exception says "The specified path, file name, or both are too long. 
                // The fully qualified file name must be less than 260 characters, and the 
                // directory name must be less than 248 characters.", however, I don't know 
                // the length of the fully qualified path name, so 190 chars is an estimate.
                AddMessage("Unable to submit all tracks for download. A file path was too long.");
            }
        }

        /// <summary>
        /// Keeps the download progress properties of the music items up-to-date
        /// so that the progress can be displayed under each item in the UI.
        /// 
        /// The music items in the library and the playlist do not point to the
        /// same object even if it's the same track so we need to update both places separately.
        /// </summary>
        /// <param name="transfer"></param>
        protected override void OnDownloadStatusUpdate(BackgroundTransferRequest transfer) {
            base.OnDownloadStatusUpdate(transfer);
            var path = FileUtilsBase.UnixSeparators(transfer.Tag.Substring(PhoneLocalLibrary.Instance.BaseMusicPath.Length));
            var musicItem = MusicProvider.SearchItem(path);
            var isDownloading = transfer.TransferStatus == TransferStatus.Transferring;
            var bytesReceived = (ulong)transfer.BytesReceived;
            // updates track in the library
            if(musicItem != null) {
                MusicItem.SetDownloadStatus(musicItem, bytesReceived);
                musicItem.IsDownloading = isDownloading;
            }
            // updates the track in the playlist
            var playlistTracks = PimpViewModel.Instance.MusicPlayer.Playlist.Songs
                .Where(item => item.Song.Path == path)
                .ToList(); // beautiful!!!
            BasePlaylist.SetDownloadStatus(playlistTracks, bytesReceived);
            foreach(var item in playlistTracks) {
                item.Song.IsDownloading = isDownloading;
            }
        }

        public override void AddTransfer(Uri remoteUri, string destination) {
            try {
                base.AddTransfer(remoteUri, destination);
            } catch(Exception e) {
                AddMessage("Unable to add transfer: " + e.Message);
            }
        }
        protected override async Task OnTransferComplete(BackgroundTransferRequest transfer) {
            try {
                await base.OnTransferComplete(transfer);
            } catch(Exception e) {
                AddMessage("Error while processing completed transfer. " + e.Message);
            }
        }

        protected override void OnTransferCompletedWithErrors(BackgroundTransferRequest transfer) {
            var destFileName = Path.GetFileName(transfer.Tag);
            AddMessage("Download of " + destFileName + " failed: " + transfer.TransferError.Message);
        }

        public async Task ValidateThenSubmitDownload(MusicItem item) {
            try {
                if(item.IsDir) {
                    await DownloadFolder(item);
                } else {
                    await DownloadSong(item);
                }
            } catch(Exception e) {
                AddMessage("Unable to download " + item.Name + ". " + e.Message);
            }
        }
        public async Task DownloadFolder(MusicItem folder) {
            var tracks = await MusicProvider.SongsInFolder(folder);
            foreach(var t in tracks) {
                await SubmitDownload(t);
            }
        }
        public async Task DownloadSong(MusicItem song) {
            if(PhoneLibraryManager.Instance.ActiveEndpoint.EndpointType == EndpointTypes.Subsonic &&
                song.Size > 5242880) {
                AddMessage("Files over 5MB cannot currently be downloaded from Subsonic due to a technical limitation. The download of " + song.Name + " is likely to fail.");
            }
            await SubmitDownload(song);
        }
        /// <summary>
        /// Downloads the song if it is not already available offline.
        /// </summary>
        /// <param name="track"></param>
        /// <returns>the local uri of the downloaded track</returns>
        public async Task<Uri> DownloadAsync(MusicItem track) {
            await BeforeDownload(track);
            var maybeLocalUri = await PhoneLocalLibrary.Instance.LocalUriIfExists(track);
            if(maybeLocalUri != null) {
                return maybeLocalUri;
            }
            return await AddTransferAsync(track.Source, LocalLibrary.AbsolutePathTo(track));
        }

        // TODO: refactor
        private Task BeforeDownload(MusicItem track) {
            return AsyncTasks.Noop();
        }
        private Task BeforeDownload(IEnumerable<MusicItem> tracks) {
            return AsyncTasks.Noop();
        }

        // rely on the credentials in the query string because the background downloader
        // in WP doesn't support custom HTTP headers

        public Task<Uri> DownloadAsync(MusicItem track, string username, string password) {
            return DownloadAsync(track);
        }


    }
}
