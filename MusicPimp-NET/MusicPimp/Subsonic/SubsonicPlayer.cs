using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public class SubsonicPlayer : BasePlayer {
        public override BasePlaylist Playlist { get; protected set; }
        private SubsonicSession session;
        //public SubsonicConverter Converter { get; protected set; }

        public SubsonicPlayer(SubsonicSession s) {
            session = s;
            Playlist = new SubsonicPlaylist(session);
        }
        public override Task play() {
            return session.serverPlayAsync();
        }
        public override Task next() {
            return Playlist.SkipTo(Playlist.Index + 1);
        }
        public override Task previous() {
            return Playlist.SkipTo(Playlist.Index - 1);
        }
        public override Task pause() {
            return session.serverStopAsync();
        }
        private async Task<JukeboxPlaylist> GetPlaylist() {
            var jukebox = await session.serverGetPlaylistAsync();
            return jukebox.jukeboxPlaylist;
        }
        private MusicItem currentPlaylistTrack(JukeboxPlaylist playlist) {
            var index = playlist.currentIndex;
            if(index < 0)
                return null;
            var track = playlist.entry[index];
            return AudioConversions.EntryToMusicItem(track, session.StreamUriFor(track.id), session.Username, session.Password);
        }
        private TimeSpan position(JukeboxPlaylist playlist) {
            return TimeSpan.FromSeconds(playlist.position);
        }
        public override async Task seek(double pos) {
            var playlist = await GetPlaylist();
            var songIndex = playlist.currentIndex;
            await session.serverSkipToPlaylistIndexAsync(songIndex, pos);
        }
        private int volume(JukeboxStatusResponse status) {
            return (int)(100.0 * status.jukeboxStatus.gain);
        }
        public override Task SetVolume(int newVolume) {
            return session.serverSetVolumeAsync(1.0 * newVolume / 100);
        }
        public PlayerState playerState(JukeboxStatusResponse status) {
            return status.jukeboxStatus.playing ? PlayerState.Playing : PlayerState.Stopped;
        }
        public override async Task<PlaybackStatus> Status() {
            var status = await session.serverStatus();
            var playlist = await GetPlaylist();
            var musicEntries = playlist.entry;
            var playlistTracks = musicEntries != null ? playlist.entry.Select(e => AudioConversions.SongEntryToMusicItem(e, session.StreamUriFor(e.id), session.Username, session.Password)).ToList() : new List<MusicItem>();
            return new PlaybackStatus(
                currentPlaylistTrack(playlist),
                position(playlist),
                playlist.currentIndex,
                playlistTracks,
                volume(status),
                playerState(status),
                IsMute);
        }
    }
}
