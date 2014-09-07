using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System.Linq;
using Windows.Networking.BackgroundTransfer;

namespace Mle.MusicPimp.Network {
    public class AppLocalDownloader : StoreDownloader {
        private MusicItemsModel AppModel {
            get { return MusicItemsModel.Instance; }
        }
        private MusicLibrary MusicProvider {
            get { return AppModel.MusicProvider; }
        }
        private BasePlaylist Playlist {
            get { return AppModel.MusicPlayer.Playlist; }
        }
        public AppLocalDownloader() : base(AppLocalMusicFolderFileUtils.Instance) { }
        protected override void OnDownloadStatusUpdate(DownloadOperation download) {
            base.OnDownloadStatusUpdate(download);
            // Updates the BytesReceived property of the possibly loaded MusicItem
            // so that a progress bar may be displayed under the item
            var musicPath = AppLocalFolderLibrary.MusicPath(download.ResultFile);
            var musicItem = MusicProvider.SearchItem(musicPath);
            var progress = download.Progress;
            var bytesReceived = progress.BytesReceived;
            if(musicItem != null) {
                MusicItem.SetDownloadStatus(musicItem, bytesReceived);
            }
            var playlistTracks = Playlist.Songs.Where(item => item.Song.Path == musicPath).ToList();
            BasePlaylist.SetDownloadStatus(playlistTracks, bytesReceived);
        }
    }
}
