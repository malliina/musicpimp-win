using Microsoft.Phone.BackgroundAudio;
using Mle.Background;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Database;
using Mle.MusicPimp.Iap;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Mle.Concurrent;

namespace Mle.MusicPimp.Local {
    /// <summary>
    /// Manages a playlist in persistent storage so that the BackgroundAudioPlayer, running in another process, can access it.
    /// </summary>
    public class PhoneLocalPlaylist : LocalBasePlaylist {
        private IDownloader Downloader {
            get { return PimpViewModel.Instance.Downloader; }
        }
        private BackgroundAudioPlayer BackgroundPlayer { get { return BackgroundAudioPlayer.Instance; } }
        public PhoneLocalPlaylist() {
            BackgroundPlayer.PlayStateChanged += async (s, e) => {
                switch(BackgroundPlayer.PlayerState) {
                    case PlayState.Playing:
                        await Load();
                        break;
                    case PlayState.Stopped:
                        // This code is not used because by the time this Stopped event is
                        // received, the background player has already installed the next track.

                        // Instead, TODO: the background audio player must read the file or 
                        // database to find out whether playback is allowed following a TrackEnded
                        // event. This must occur without concurrency issues.

                        // The playstate jumps to the Stopped state when a track ends,
                        // then, the next one from the playlist is chosen automatically,
                        // however this stops playback if no further playback is allowed.
                        //if(!UsageController.Instance.IsPlaybackAllowed()) {
                        //    BackgroundPlayer.Stop();
                        //}
                        break;
                }
            };
        }
        public override async Task AddSong(MusicItem song) {
            await TrySubmitDownload(song);
            PlaylistDatabase.Add(AudioConversions.ToPlaylistTrack(song));
        }
        public override async Task AddSongs(IEnumerable<MusicItem> songs) {
            await TrySubmitDownload(songs);
            PlaylistDatabase.AddAll(songs.Select(AudioConversions.ToPlaylistTrack).ToList());
        }
        protected override Task RemoveSongInternal(int index) {
            return TaskEx.Run(() => {
                PlaylistDatabase.Delete(index);
                if(index == Index) {
                    PlaylistDatabase.SetIndex(NO_POSITION);
                } else if(index >= 0 && index < Index && Index > 0) {
                    // A track has been removed from "above" the currently playing track,
                    // so the Index needs to adapt and move one step up, so that it continues 
                    // to point at the same track.
                    PlaylistDatabase.SetIndex(Index - 1);
                }
            });
        }
        protected override Task SendSkipCommand(int index) {
            PlaylistDatabase.SetIndex(index);
            return AsyncTasks.Noop();
        }
        public override async Task SetPlaylist(MusicItem song) {
            await TrySubmitDownload(song);
            PlaylistDatabase.SetPlaylist(AudioConversions.ToPlaylistTrack(song));
        }
        private Task TrySubmitDownload(MusicItem song) {
            return TrySubmitDownload(new List<MusicItem>() { song });
        }
        private async Task TrySubmitDownload(IEnumerable<MusicItem> songs) {
            try {
                var source = PhoneLibraryManager.Instance.ActiveEndpoint;
                // awaits completion of submission, not download
                // even submitting a download takes fucking long,
                // so TaskEx.Run hopefully runs the stuff on another thread
                await TaskEx.Run(async () => await Downloader.SubmitDownloads(songs, source.Username, source.Password));
            } catch(BackgroundTransferException) {
                // thrown if the transfer request cannot be added, for example if there are 25 other requests already pending
                // TODO fix: add request to persistent storage, make transfer service load from storage when capable
            } catch(DirectoryNotFoundException) {
                // I should have fixed this bug by now
            }
        }
        public override Task<PlaylistInfo> LoadPlaylist() {
            return TaskEx.FromResult(PlaylistDatabase.TracksAndIndex());
        }
    }
}
