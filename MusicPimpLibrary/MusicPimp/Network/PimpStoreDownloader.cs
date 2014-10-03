using Mle.Concurrent;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Mle.MusicPimp.Network {
    public class DownloadParameters {
        public Uri Source { get; private set; }
        public StorageFile Destination { get; private set; }
        public DownloadParameters(Uri source, StorageFile dest) {
            Source = source;
            Destination = dest;
        }
    }

    public class PimpStoreDownloader : BaseDownloader {
        private static PimpStoreDownloader instance = null;
        public static PimpStoreDownloader Instance {
            get {
                if(instance == null)
                    instance = new PimpStoreDownloader();
                return instance;
            }
        }

        private MultiFolderLibrary LocalLibrary {
            get { return MultiFolderLibrary.Instance; }
        }
        private MusicLibrary MusicProvider {
            get { return MusicItemsModel.Instance.MusicProvider; }
        }

        public AppLocalDownloader Utils { get; private set; }

        protected PimpStoreDownloader() {
            Utils = new AppLocalDownloader();
        }
        public override async Task SubmitDownloads(IEnumerable<MusicItem> tracks) {
            foreach(var track in tracks) {
                await SubmitDownload(track);
            }
        }
        public override Task SubmitDownload(MusicItem track) {
            // does intentionally not await completion of download
            var t = DownloadAsync(track);
            return AsyncTasks.Noop();
        }

        public override async Task<Uri> DownloadAsync(MusicItem track) {
            var info = await GetDownloadInfo(track);
            if(info != null) {
                return await StartDownload(track, info);
            } else {
                return null;
            }
        }
        private async Task<Uri> StartDownload(MusicItem track, DownloadParameters info) {
            var op = ToDownloader(track).CreateDownload(info.Source, info.Destination);
            return await Utils.StartDownload(op);
        }
        private BackgroundDownloader ToDownloader(MusicItem track) {
            var downloader = new BackgroundDownloader();
            downloader.SetRequestHeader(HttpUtil.Authorization, HttpUtil.BasicAuthHeader(track.Username, track.Password));
            if(track.CloudServer != null) {
                downloader.SetRequestHeader(CloudSession.SERVER_KEY, track.CloudServer);
            }
            return downloader;
        }
        public async Task SubmitAll(IEnumerable<MusicItem> items) {
            var e = StoreLibraryManager.Instance.ActiveEndpoint;
            // the foreach throws InvalidOperationException if the collection is "modified" so 
            // perhaps this is a workaround, then 
            var copiedItems = items.ToList();
            foreach(var item in copiedItems) {
                if(item.IsDir) {
                    await SubmitFolder(item);
                } else {
                    await SubmitDownload(item);
                }
            }
        }
        private async Task SubmitFolder(MusicItem folder) {
            var tracks = await MusicProvider.SongsInFolder(folder);
            foreach(var item in tracks) {
                await SubmitDownload(item);
            }
        }

        //private async Task<Uri> DownloadIfNotExists(MusicItem track, Func<Task<Uri>> performDownload) {
        //    var maybeLocalUri = await LocalLibrary.LocalUriIfExists(track);
        //    if(maybeLocalUri != null) {
        //        return maybeLocalUri;
        //    }
        //    return await performDownload();
        //}
        /// <summary>
        /// Prepares a download. A return value of null indicates that the download shall
        /// not proceed. This may be the case if the track already exists locally, or some
        /// IO error prevents the download, for example if the intended destination path 
        /// is too long.
        /// </summary>
        /// <param name="track"></param>
        /// <returns>download parameters, or null if the track shall not be downloaded</returns>
        private async Task<DownloadParameters> GetDownloadInfo(MusicItem track) {
            var maybeLocalUri = await LocalLibrary.LocalUriIfExists(track);
            if(maybeLocalUri == null) {
                try {
                    var source = RemoveCredentialsFromQueryParams(track.Source);
                    var destFile = await Utils.FileTo(PathTo(track));
                    return new DownloadParameters(source, destFile);
                } catch(IOException) {
                    // may have thrown PathTooLongException
                    // does not proceed with the download
                    return null;
                }
            } else {
                return null;
            }
        }

        /// <summary>
        /// This is a hack and can fail in so many ways, do not reuse outside of this class
        /// 
        /// Removes 'u' and 'p' query params.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private Uri RemoveCredentialsFromQueryParams(Uri uri) {
            var uriString = uri.OriginalString;
            var qStart = uriString.IndexOf('?');
            if(qStart < 0) {
                return uri;
            }
            var q = uri.Query;
            if(q.StartsWith("?")) {
                q = q.Substring(1);
            }
            var kvs = q.Split('&');
            var otherKvs = kvs.Where(kv => kv.Length > 1 && !kv.StartsWith("u=") && !kv.StartsWith("p=")).ToList();
            var newQuery = string.Join("&", otherKvs);
            var uriPrefix = uriString.Substring(0, qStart);
            var newQueryString = newQuery.Length > 0 ? "?" + newQuery : "";
            return new Uri(uriPrefix + newQueryString, UriKind.Absolute);
        }
        private string PathTo(MusicItem track) {
            // todo remove dep on applocalfolderlibrary
            return AppLocalFolderLibrary.Instance.AbsolutePathTo(track);
        }
    }
}
