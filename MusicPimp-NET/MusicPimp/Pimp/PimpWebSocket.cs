using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Network {
    public class PimpWebSocket {
        public WebSocketBase Socket { get; private set; }

        private const string EVENT = "event";
        private const string STATUS = "status";
        private const string SHORT_STATUS = "short_status";
        private const string TIME_UPDATED = "time_updated";
        private const string TRACK_CHANGED = "track_changed";
        private const string VOLUME_CHANGED = "volume_changed";
        private const string MUTE_TOGGLED = "mute_toggled";
        private const string PLAYLIST_MODIFIED = "playlist_modified";
        private const string PLAYLIST_INDEX_CHANGED = "playlist_index_changed";
        private const string PLAYSTATE_CHANGED = "playstate_changed";
        private const string WELCOME = "welcome";
        private const string DISCONNECTED = "disconnected";
        private const string PING = "ping";

        public event Action<double> TimeUpdated;
        public event Action<PlayerState> PlayStateChanged;
        public event Action<MusicItem> TrackChanged;
        public event Action<int> VolumeChanged;
        public event Action<bool> MuteToggled;
        public event Action<IEnumerable<MusicItem>> PlaylistModified;
        public event Action<int> PlaylistIndexChanged;
        public event Action<PlaybackStatus> StatusUpdateReceived;
        public event Action<ShortStatusJsonEvent> ShortStatusUpdateReceived;
        public event Action Welcomed;
        public event Action<string> Disconnected;
        public event Action<string> SocketClosed;
        public event Action ErrorOccurred;

        public PimpWebSocket(WebSocketBase socket) {
            Socket = socket;
            socket.MessageReceived += socket_MessageReceived;
            socket.Error += OnErrorOccurred;
            socket.Closed += OnSocketClosed;
        }

        private void socket_Closed(string reason) {
            //SendMessage("WebSocket closed");
        }

        private void socket_Error() {
            //SendMessage("WebSocket error");
        }
        private void SendMessage(string msg) {
            MessagingService.Instance.Send(msg);
        }
        public Task Send(JsonContent cmd) {
            return Socket.Send(cmd.Json());
        }
        private T Deserialize<T>(string json) {
            return Json.Deserialize<T>(json);
        }
        /// <summary>
        /// Parses JSON message: determines event type and calls appropriate event handler.
        /// </summary>
        /// <param name="msg">JSON message</param>
        private void socket_MessageReceived(string msg) {
            var json = JObject.Parse(msg);
            var eventTypeToken = json[EVENT];
            if(eventTypeToken != null) {
                string eventType = eventTypeToken.Value<string>();
                switch(eventType) {
                    case STATUS:
                        var pimpStatus = Deserialize<StatusPimpResponse>(msg);
                        var status = PimpBasePlayer.ToPlaybackStatus(pimpStatus);
                        OnStatusUpdateReceived(status);
                        break;
                    case SHORT_STATUS:
                        var shortStatus = Deserialize<ShortStatusJsonEvent>(msg);
                        OnShortStatusUpdateReceived(shortStatus);
                        break;
                    case TIME_UPDATED:
                        var e = Deserialize<TimeUpdatedEvent>(msg);
                        OnTimeUpdated(e.position);
                        break;
                    case PLAYSTATE_CHANGED:
                        var psce = Deserialize<PlayStateChangedEvent>(msg);
                        PlayerState state = FromName(psce.state);
                        OnPlayStateChanged(state);
                        break;
                    case TRACK_CHANGED:
                        var tce = Deserialize<TrackChangedEvent>(msg);
                        // todo set track source
                        MusicItem item = ToItem(tce.track);
                        OnTrackChanged(item);
                        break;
                    case VOLUME_CHANGED:
                        var vce = Deserialize<VolumeChangedEvent>(msg);
                        OnVolumeChanged(vce.volume);
                        break;
                    case MUTE_TOGGLED:
                        var mte = Deserialize<MuteToggledEvent>(msg);
                        OnMuteToggled(mte.mute);
                        break;
                    case PLAYLIST_MODIFIED:
                        var pme = Deserialize<PlaylistModifiedEvent>(msg);
                        var playlist = pme.playlist.Select(ToItem).ToList();
                        OnPlaylistModified(playlist);
                        break;
                    case PLAYLIST_INDEX_CHANGED:
                        var pic = Deserialize<PlaylistIndexChangedEvent>(msg);
                        OnPlaylistIndexChanged(pic.index);
                        break;
                    case WELCOME:
                        Socket.IsConnected = true;
                        OnWelcomed();
                        break;
                    case DISCONNECTED:
                        var de = Deserialize<DisconnectedEvent>(msg);
                        OnDisconnected(de.user);
                        break;
                    case PING:
                        break;
                    default:
                        //Debug.WriteLine("Received unknown WebSocket message: " + msg);
                        break;
                }
            }
        }
        private MusicItem ToItem(PimpTrack track) {
            return AudioConversions.PimpTrackToMusicItem(track, null, null, null, null);
        }
        public static PlayerState FromName(string stateName) {
            switch(stateName) {
                case "Started":
                case "Playing":
                    return PlayerState.Playing;
                case "Paused":
                    return PlayerState.Paused;
                case "Stopped":
                    return PlayerState.Stopped;
                case "Closed":
                    return PlayerState.Closed;
                case "Ended":
                    return PlayerState.Closed;
                case "Buffering":
                    return PlayerState.Buffering;
                case "Seeking":
                    return PlayerState.Seeking;
                case "Seeked":
                    return PlayerState.Seeked;
                case "Emptied":
                    return PlayerState.Closed;
                default:
                    return PlayerState.Other;
            }
        }

        private void OnTimeUpdated(double newTime) {
            if(TimeUpdated != null) {
                TimeUpdated(newTime);
            }
        }
        private void OnPlayStateChanged(PlayerState state) {
            if(PlayStateChanged != null) {
                PlayStateChanged(state);
            }
        }
        private void OnTrackChanged(MusicItem track) {
            if(TrackChanged != null) {
                TrackChanged(track);
            }
        }
        private void OnVolumeChanged(int newVolume) {
            if(VolumeChanged != null) {
                VolumeChanged(newVolume);
            }
        }
        private void OnMuteToggled(bool isMute) {
            if(MuteToggled != null) {
                MuteToggled(isMute);
            }
        }
        private void OnPlaylistModified(IEnumerable<MusicItem> playlist) {
            if(PlaylistModified != null) {
                PlaylistModified(playlist);
            }
        }
        private void OnPlaylistIndexChanged(int index) {
            if(PlaylistIndexChanged != null) {
                PlaylistIndexChanged(index);
            }
        }
        private void OnStatusUpdateReceived(PlaybackStatus status) {
            if(StatusUpdateReceived != null) {
                StatusUpdateReceived(status);
            }
        }
        private void OnShortStatusUpdateReceived(ShortStatusJsonEvent status) {
            if(ShortStatusUpdateReceived != null) {
                ShortStatusUpdateReceived(status);
            }
        }
        private void OnWelcomed() {
            if(Welcomed != null) {
                Welcomed();
            }
        }
        private void OnDisconnected(string user) {
            if(Disconnected != null) {
                Disconnected(user);
            }
        }
        private void OnErrorOccurred() {
            if(ErrorOccurred != null) {
                ErrorOccurred();
            }
        }
        private void OnSocketClosed(string content) {
            if(SocketClosed != null) {
                SocketClosed(content);
            }
        }
    }
    public abstract class PimpEvent : JsonContent {
        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }
    }
    public class TrackChangedEvent : PimpEvent {
        public PimpTrack track { get; set; }
    }
    public class TimeUpdatedEvent : PimpEvent {
        public double position { get; set; }
    }
    public class PlayStateChangedEvent : PimpEvent {
        public string state { get; set; }
    }
    public class VolumeChangedEvent : PimpEvent {
        public int volume { get; set; }
    }
    public class MuteToggledEvent : PimpEvent {
        public bool mute { get; set; }
    }
    public class PlaylistModifiedEvent : PimpEvent {
        public List<PimpTrack> playlist { get; set; }
    }
    public class PlaylistIndexChangedEvent : PimpEvent {
        public int index { get; set; }
    }
    public class WelcomeEvent : PimpEvent { }
    public class DisconnectedEvent : PimpEvent {
        public string user { get; set; }
    }
    public class ShortStatusJsonEvent : PimpEvent {
        public string state { get; set; }
        public double position { get; set; }
        public int volume { get; set; }
        public bool mute { get; set; }
    }
    // not used atm, too verbose, too small profit
    //public class ShortStatusEvent {
    //    public PlayerState PlayerState { get; private set; }
    //    public TimeSpan TrackPosition { get; private set; }
    //    public int Volume { get; private set; }
    //    public double IsMute { get; private set; }

    //    public ShortStatusEvent(PlayerState state, TimeSpan pos, int volume, double mute) {
    //        PlayerState = state;
    //        TrackPosition = pos;
    //        Volume = volume;
    //        IsMute = mute;
    //    }

    //}
}
