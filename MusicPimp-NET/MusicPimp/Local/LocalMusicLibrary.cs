using Mle.Concurrent;
using Mle.IO;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public abstract class LocalMusicLibrary : MusicLibrary {
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <returns>the local URI to the given track, or null if this library does not contain the track</returns>
        public abstract Task<Uri> LocalUriIfExists(MusicItem track);
        public abstract Task<T> WithStream<T>(MusicItem track, Func<Stream, Task<T>> op);
        public abstract Task Delete(List<string> songs);
        public void Delete(string songRelativePath) {
            Delete(new List<string>() { songRelativePath });
        }
        public override Task Ping() {
            return AsyncTasks.Noop();
        }
        public virtual Task DeleteAll() {
            var rootMusicItem = new MusicItem() {
                Id = RootFolderKey,
                Path = "",
                IsDir = true
            };
            return Delete(rootMusicItem);
        }
        public async Task Delete(MusicItem item) {
            if(item.IsDir) {
                var tracks = await SongsInFolder(item);
                await Delete(tracks.Select(t => t.Path).ToList());
            } else {
                await Delete(new List<string>() { item.Path });
            }
        }
        public override Task Upload(MusicItem song, string resource, PimpSession destSession) {
            return destSession.Upload(song, resource);
        }
        public override Task<IEnumerable<DataTrack>> Search(string term) {
            return TaskEx.FromResult<IEnumerable<DataTrack>>(new List<DataTrack>());
        }
    }
}
