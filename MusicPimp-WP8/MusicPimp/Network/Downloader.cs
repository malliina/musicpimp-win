using Mle.IO;
using Mle.MusicPimp.Database;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Phone.Network {
    public class DownloadTask {
        public Uri RemoteUri { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
        public DownloadTask(Uri uri, string path, long size) {
            this.RemoteUri = uri;
            Path = path;
            Size = size;
        }
    }
    public delegate void DownloadCompleteHandler(string filePath);
    /// <summary>
    /// dear god fix this synchronization mess
    /// </summary>
    public class Downloader {

        private static PhoneLocalLibrary LocalLibrary {
            get { return PhoneLocalLibrary.Instance; }
        }
        public static event DownloadCompleteHandler DownloadCompleteEvent;

        public static List<DownloadTask> Tasks = new List<DownloadTask>();

        static Downloader() {
            DownloadCompleteEvent += x => { };
        }
        public static async Task<Uri> Download(DownloadTask task) {
            if (IsDownloading(task))
                return null;
            Tasks.Add(task);
            try {
                var ret = await FileUtils.WithStorage<Task<Uri>>(async isoStore => {
                    var dir = Path.GetDirectoryName(task.Path);
                    bool shouldDownload = false;
                    if (!isoStore.DirectoryExists(dir)) {
                        isoStore.CreateDirectory(dir);
                        shouldDownload = true;
                    } else {
                        if (isoStore.FileExists(task.Path)) {
                            using (var readFile = isoStore.OpenFile(task.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)) {
                                if (readFile.Length != task.Size) {
                                    shouldDownload = true;
                                }
                            }
                        } else {
                            shouldDownload = true;
                        }
                    }
                    if (shouldDownload) {
                        using (var isoDFile = isoStore.OpenFile(task.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)) {
                            if (isoDFile.Length != task.Size) {
                                var webClient = new WebClient();
                                var stream = await webClient.OpenReadTaskAsync(task.RemoteUri);
                                await stream.CopyToAsync(isoDFile);
                            }
                        }
                    }
                    return new Uri(task.Path, UriKind.Relative);
                });
                return ret;
            } finally {
                Tasks.Remove(task);
                DownloadCompleteEvent(task.Path);
            }
        }
        public static bool IsDownloading(DownloadTask downloadInfo) {
            return Tasks.Any(task => task.Path == downloadInfo.Path);
        }

        [Obsolete("Use BackgroundTransferService")]
        public static Task<Uri> CompletionOfDownload(MusicItem song) {
            var downloadInfo = song2task(song);
            if (IsDownloading(downloadInfo)) {
                var tcs = new TaskCompletionSource<Uri>();
                DownloadCompleteHandler handler = null;
                handler = filePath => {
                    if (filePath == LocalLibrary.AbsolutePathTo(song)) {
                        DownloadCompleteEvent -= handler;
                        tcs.SetResult(new Uri(filePath, UriKind.Relative));
                    }
                };
                DownloadCompleteEvent += handler;
                return tcs.Task;
            } else {
                return Download(song);
            }
        }

        public static async Task<Uri> Download(MusicItem song) {
            await PlaybackHistorian.Instance.AddPlayCount(song.Path);
            var maybeLocalUri = await LocalLibrary.LocalUriIfExists(song);
            if (maybeLocalUri != null) {
                return maybeLocalUri;
            }
            var task = song2task(song);
            return await Download(task);
        }
        public static DownloadTask song2task(MusicItem song) {
            return new DownloadTask(song.Source, LocalLibrary.BaseMusicPath + song.Path, song.Size);
        }
    }

}
