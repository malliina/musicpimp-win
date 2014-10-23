using Mle.Exceptions;
using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class PimpLibrary : MusicLibrary {
        private PimpSessionBase session;
        public PimpLibrary(PimpSessionBase s) {
            session = s;
            RootEmptyMessage = "Successfully connected to MusicPimp at " + session.Describe + ", however the server-side music library is empty. To use it as a music source, you need to add folders containing MP3s in the Manage section of MusicPimp for your PC. Refresh the library of this app when you're done.";
        }

        public override Task Ping() {
            return session.PingAuth();
        }
        public override Uri DownloadUriFor(MusicItem track) {
            return session.DownloadUriFor(track);
        }
        //public override string DirectoryIdentifier(MusicItem musicDir) {
        //    return "" + musicDir.Id;
        //}
        public async override Task<IEnumerable<MusicItem>> Reload(string folder) {
            FoldersPimpResponse response = null;
            if(folder == RootFolderKey) {
                response = await session.RootContentAsync();
            } else {
                response = await session.ContentsIn(folder);
            }
            return Itemize(response);
        }
        public override Task<IEnumerable<MusicItem>> Search(string term) {
            return session.Search(term);
        }
        /// <summary>
        /// maps a json response to a list of music items
        /// </summary>
        /// <param name="response">the fetched data</param>
        private List<MusicItem> Itemize(FoldersPimpResponse response) {
            var ret = new List<MusicItem>();
            foreach(var folder in response.folders) {
                ret.Add(AudioConversions.FolderToMusicItem(folder));
            }
            foreach(var track in response.tracks) {
                var uri = session.PlaybackUriFor(track.id);
                ret.Add(TrackToMusicItem(track, uri));
            }
            return ret;
        }

        public MusicItem TrackToMusicItem(PimpTrack track, Uri uri) {
            return AudioConversions.PimpTrackToMusicItem(track, uri, session.Username, session.Password, session.IsCloud ? session.CloudServerID : null);
        }
        public override async Task Upload(MusicItem song, string resource, PimpSession destSession) {
            var targetUri = destSession.BaseUri + resource;
            var cmd = new BeamCommand(song.Id, targetUri, destSession.Username, destSession.Password);
            // Bug: If the upload takes about over one minute, the client-side
            // response of HttpClient says 404 even though the server has returned
            // 200 OK. Increasing HttpClient.Timeout has no effect.
            // Displaying an error when all is fine is confusing so we suppress the
            // exception until this is resolved properly. This might of course also
            // suppress 404s that are actually real.
            await Utils.SuppressAsync<NotFoundException>(async () => {
                var versionResponse = await session.PingAuth();
                var serverVersion = new Version(versionResponse.version);
                try {
                    // throws BadRequestException if adding fails
                    await session.PostCommand(cmd, "/playback/stream");
                } catch(InternalServerErrorException) {
                    // MusicPimp server 2.0.0 and lower throw InternalServerError if the MusicBeamer endpoint uses https because 
                    // StartSSL certs are not trusted. Fixed in MusicPimp server 2.1.0.
                    if(destSession.BaseUri.StartsWith("https") && serverVersion < PimpSessionBase.HttpsSupportingVersion) {
                        MessagingService.Instance.Send("An error occurred. Please upgrade the MusicPimp server to version 2.1.1 or later. Get it from www.musicpimp.org. Thanks!");
                    } else {
                        throw;
                    }
                }
            });
        }
    }
}
