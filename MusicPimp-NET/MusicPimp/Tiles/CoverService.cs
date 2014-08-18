using Mle.Exceptions;
using Mle.IO;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using PCLWebUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Tiles {
    // discogs json
    public class UriWrapper {
        [JsonProperty(PropertyName = "thumb")]
        public string UriString { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string AlbumId { get; set; }
    }
    public class ImageInfo {
        [JsonProperty(PropertyName = "uri")]
        public string UriString { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        [JsonProperty(PropertyName = "uri150")]
        public string Uri150 { get; set; }
        [JsonProperty(PropertyName = "resource_url")]
        public string ResourceUrl { get; set; }
    }

    public class CoverService : IDisposable {
        private static readonly ulong ILoveDiscoGsFakeCoverSize = 15378;

        private static AsyncSemaphore sem = new AsyncSemaphore(1);

        private FileUtilsBase fileUtils;
        private string coverArtFolderPath;
        private HttpClient httpClient;

        protected CoverService(FileUtilsBase fileUtils, string coverArtFolderPath) {
            this.fileUtils = fileUtils;
            this.coverArtFolderPath = coverArtFolderPath;
            httpClient = new HttpClient();
        }

        /// <summary>
        /// Attempts to obtain a local URI to the album cover image of the supplied track. Downloads
        /// the cover from DiscoGs if necessary, after which the cover is cached on disk.
        /// 
        /// Never throws; exceptions are suppressed and null is instead returned if any are thrown.
        /// This is because fetching album art is a low-prio, secondary, best-effort, expendable service.
        /// </summary>
        /// <param name="track">track for which to get album art</param>
        /// <returns>a local URI to the cover, or null if none is found or exceptions were thrown</returns>
        public async Task<Uri> TryGetCover(MusicItem track) {
            try {
                return await GetOrDownloadCover(track);
            } catch(HttpRequestException) {
                return null;
            } catch(ServerResponseException) {
                return null;
            } catch(JsonReaderException) {
                // parsing discogs might fail; sometimes the api seems to return nonsense; maybe due to throttling
                return null;
            } catch(Exception) {
                // caught because might throw IsolatedStorageException for some fucking reason which I don't want to propagate
                // TODO improve error handling
                return null;
            }
        }
        /// <summary>
        /// Attempts to obtain a URI to cover art for the supplied track.
        /// 
        /// First the local cover storage is inspected; if no cover is found,
        /// it is searched for online and downloaded if found.
        /// 
        /// If no cover is found locally nor online, null is returned.
        /// </summary>
        /// <param name="track">the track for which we want cover art</param>
        /// <returns>the local URI to the cover art, or null if no cover is found for the track</returns>
        public Task<Uri> GetOrDownloadCover(MusicItem track) {
            return GetOrDownloadCover(track.Artist, track.Album);
        }
        public async Task<Uri> GetOrDownloadCover(string artist, string album) {
            if(NullOrEmptyOrSuspect(artist, album)) {
                return null;
            }
            var fileName = CoverFileName(artist, album);
            var coverFilePath = coverArtFolderPath + fileName;
            await DeleteCoverIfFake(coverFilePath);
            var localUri = await GetCoverUri(fileName);
            if(localUri != null) {
                return localUri;
            } else {
                var remoteUri = CoverUri(artist, album);
                if(remoteUri != null) {
                    var uri = await Download(remoteUri, coverFilePath);
                    await DeleteCoverIfFake(coverFilePath);
                    return await GetCoverUri(fileName);
                } else {
                    return null;
                }
            }
        }
        private Uri CoverUri(string artist, string album) {
            return new Uri("https://api.musicpimp.org/covers?artist=" + WebUtility.UrlEncode(artist) + "&album=" + WebUtility.UrlEncode(album), UriKind.Absolute);
        }
        /// <summary>
        /// Attempts to return count random covers from the ones available in isolated storage.
        /// 
        /// If there are less than count covers available, returns as many as there are available.
        /// 
        /// Note: the returned URIs are relative to the local app iso storage root "ms-appdata:///local/".
        /// </summary>
        /// <param name="count">number of covers.</param>
        /// <returns>a list with at most count covers</returns>
        public async Task<IList<Uri>> GetCoverCollection(int count = 5) {
            return (await fileUtils.ListFilesAsUris(coverArtFolderPath))
                .OrderBy(uri => Guid.NewGuid()) // randomizes
                .Take(count)
                .ToList();
        }
        private async Task DeleteCoverIfFake(string coverFilePath) {
            var uriInfo = await fileUtils.UriInfoIfExists(coverFilePath);
            if(uriInfo != null && uriInfo.Size == ILoveDiscoGsFakeCoverSize) {
                await fileUtils.Delete(coverFilePath);
            }
        }
        protected Task<Uri> GetCoverUri(string fileName) {
            return UriFromPath(coverArtFolderPath + fileName);
        }
        protected Task<Uri> UriFromPath(string filePath) {
            return fileUtils.UriIfExistsAndPositiveSize(FileUtilsBase.UnixSeparators(filePath));
        }
        private bool NullOrEmptyOrSuspect(params string[] input) {
            // \0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0
            return input.Any(inp => String.IsNullOrWhiteSpace(inp) || inp.Contains("\0"));
        }
        /// <summary>
        /// Downloads remoteUri to dest. This method is solely used for downloading 
        /// covers from discogs, which requires OAuth Authentication. Therefore, note
        /// the use of oAuthHttpClient.
        /// </summary>
        /// <param name="remoteUri">remote URI to download</param>
        /// <returns>a local URI to the downloaded file</returns>
        public async Task<Uri> Download(Uri remoteUri, string dest) {
            // might throw at least IsolatedStorageException on WP
            await sem.WaitAsync();
            try {
                await fileUtils.WithFileWriteAsync(dest, stream => {
                    return httpClient.DownloadToStream(remoteUri, stream);
                });
            } finally {
                sem.Release();
            }
            return await UriFromPath(dest);
        }
        private string CoverFileName(MusicItem item) {
            return CoverFileName(item.Artist, item.Album);
        }
        private string CoverFileName(string artist, string album) {
            // some id3 tags contain what appear to be nulls stringified, thanks id3.net
            var properAlbumName = album.Replace("\\0", String.Empty);
            // TODO: is it safe that all covers have the .jpg extension even though I
            // suppose they might be .png, .jpeg, .gif...?
            // I force the extension just so I can search them with an exact file name later.
            return artist + "-" + properAlbumName + ".jpg";
        }
        public void Dispose() {
            httpClient.Dispose();
        }
    }
}
