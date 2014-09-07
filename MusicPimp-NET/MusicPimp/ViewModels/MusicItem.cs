using Mle.Network;
using Mle.ViewModels;
using System;

namespace Mle.MusicPimp.ViewModels {
    /// <summary>
    /// a folder or a song
    /// TODO separate containers: folder, song.
    /// 
    /// </summary>
    public class MusicItem : ViewModelBase {
        private string _name;
        public string Name {
            get { return _name; }
            set { this.SetProperty(ref this._name, value); }
        }
        private string _id;
        public string Id {
            get { return _id; }
            set { this.SetProperty(ref this._id, value); }
        }
        private bool _isDir;
        public bool IsDir {
            get { return _isDir; }
            set { this.SetProperty(ref this._isDir, value); }
        }
        private string _album;
        public string Album {
            get { return _album; }
            set { this.SetProperty(ref this._album, value); }
        }
        private string _artist;
        public string Artist {
            get { return _artist; }
            set { this.SetProperty(ref this._artist, value); }
        }
        private string _path;
        public string Path {
            get { return _path; }
            set { this.SetProperty(ref this._path, value); }
        }
        private TimeSpan _duration;
        public TimeSpan Duration {
            get { return _duration; }
            set {
                this.SetProperty(ref this._duration, value);
            }
        }
        private Uri source;
        public Uri Source {
            get { return source; }
            set {
                if(this.SetProperty(ref this.source, value)) {
                    OnPropertyChanged("IsSourceLocal");
                    OnPropertyChanged("IsDownloadable");
                }
            }
        }
        private string username;
        public string Username {
            get { return username; }
            set { this.SetProperty(ref this.username, value); }
        }
        private string password;
        public string Password {
            get { return password; }
            set { this.SetProperty(ref this.password, value); }
        }
        public bool IsSourceLocal {
            get { return Source != null && !HttpUtil.IsHttp(Source); }
        }
        private bool isDownloading = false;
        public bool IsDownloading {
            get { return isDownloading; }
            set { this.SetProperty(ref this.isDownloading, value); }
        }
        private ulong bytesReceived;
        public ulong BytesReceived {
            get { return bytesReceived; }
            set { this.SetProperty(ref this.bytesReceived, value); }
        }
        // bytes; todo: change type to ulong
        private long size;
        public long Size {
            get { return size; }
            set { this.SetProperty(ref this.size, value); }
        }
        public static void SetDownloadStatus(MusicItem track, ulong bytesReceived) {
            track.IsDownloading = bytesReceived < (ulong)track.Size;
            track.BytesReceived = bytesReceived;
        }
    }
}