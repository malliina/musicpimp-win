using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Beam;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.Util;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Mle.MusicPimp.ViewModels {
    // TODO remove this suspect hack
    public enum PlaybackMode { server, web }

    public abstract class PlayerManager : EndManagerBase {

        public event Action<BasePlayer> PlayerActivated;
        public event Action<BasePlayer> PlayerDeactivated;

        public LibraryManager LibraryManager { get { return Provider.LibraryManager; } }

        private static readonly string webPlayerPrefix = "clients of ";
        // TODO remove musicSource parameter from the player
        protected virtual BasePlayer NewPimpPlayer(MusicEndpoint e, Func<MusicEndpoint> musicSource) {
            var session = Provider.NewPimpSession(e);
            return new PimpPlayer(session, musicSource, NewPimpSocket(session));
        }
        protected virtual BasePlayer NewSubsonicPlayer(MusicEndpoint e) {
            return new SubsonicPlayer(new SubsonicSession(e));
        }
        protected virtual BasePlayer NewCloudPlayer(MusicEndpoint e) {
            return NoUploadsPimpPlayer(new CloudSession(e));
        }
        public BasePlayer NoUploadsPimpPlayer(SimplePimpSession s) {
            var socket = NewPimpSocket(s);
            var playlist = new SimplePimpPlaylist(s, socket);
            return new PimpPlayer(s, socket, playlist);
        }
        // dangerous mutable state, todo better code
        private PlaybackMode playbackMode = PlaybackMode.server;

        /// <summary>
        /// This property is bound to the ItemsSource of a ListPicker, which also has its SelectedIndex bound
        /// to property Index. When this property is changed, for some reason the Index is set to 0. I don't
        /// know why. To workaround, always reassign the previously active endpoint when this property is updated.
        /// </summary>
        public List<string> PlayerEndpoints {
            get {
                var endpoints = EndpointsData.Endpoints;
                var pimps = endpoints.Where(e => e.EndpointType == EndpointTypes.MusicPimp).ToList();
                var endpointNames = endpoints.Select(e => e.Name).ToList();
                var clientNames = pimps.Select(e => webPlayerPrefix + e.Name).ToList();
                endpointNames.AddRange(clientNames);
                return endpointNames;
            }
        }
        private BasePlayer player;
        public BasePlayer Player {
            get { return player; }
            set { SetProperty(ref player, value); }
        }

        protected PlayerManager(ISettingsManager settings)
            : base(settings, SettingKey.playbackDeviceIndex) {
            EndpointsData.Endpoints.CollectionChanged += (s, e) => {
                OnPropertyChanged("PlayerEndpoints");
                if(ActiveEndpoint != null) {
                    SetActive(ActiveEndpoint);
                }
            };
            EndpointsData.EndpointModified += e => {
                var a = ActiveEndpoint;
                OnPropertyChanged("PlayerEndpoints");
                if(a != null) {
                    SetActive(a);
                }
            };
            EndpointsData.EndpointRemoved += e => {
                var a = ActiveEndpoint;
                OnPropertyChanged("PlayerEndpoints");
                if(a != null) {
                    SetActive(a);
                }
            };
            PlayerActivated += async p => {
                OnPropertyChanged("ActiveEndpoint");
                OnPropertyChanged("ActiveEndpoint.CanReceiveMusic");
                // we don't do this in ActivateEndpoint so that we can keep it non-async 
                // and avoid questions about initialization order
                try {
                    await p.TryToConnect();
                } catch(TaskCanceledException) {
                    // Not sure why this is thrown, but the subscription tends to be successful regardless so I suppress.
                    // TODO examine in more detail and fix this.
                } catch(Exception ex) {
                    Send("Unable to connect to player. Try again later. " + ex.Message);
                }
            };
            PlayerDeactivated += async p => {
                // unsubscribing might take ages due to low quality websocket library in wp
                await TaskEx.Run(() => Utils.Suppress<Exception>(p.Unsubscribe));
            };
        }
        /// <summary>
        /// The playbackMode variable determines which MusicPimp endpoint to activate
        /// if the endpoint is a MusicPimp endpoint. Keep this in mind or don't use 
        /// this method otherwise.
        /// </summary>
        /// <param name="endpoint">existing endpoint to activate</param>
        public override void SetActive(MusicEndpoint endpoint) {
            var name = endpoint.Name;
            if(endpoint.EndpointType == EndpointTypes.MusicPimp && playbackMode == PlaybackMode.web) {
                name = webPlayerPrefix + name;
            }
            Index = PlayerEndpoints.IndexOf(name);
        }
        protected PimpWebSocket NewPimpSocket(SimplePimpSession session) {
            var e = session.Endpoint;
            var protocolPart = e.Protocol == Protocols.https ? "wss://" : "ws://";
            var uri = new Uri(protocolPart + e.Server + ":" + e.Port + session.SocketResource);
            var ws = Provider.NewWebSocket(uri, session.AuthHeader(e), SimplePimpSession.JSONv18);
            return new PimpWebSocket(ws);
        }
        /// <summary>
        /// Gets the playback device with the given index.
        /// 
        /// The index may also refer to a "clients of" playback device, in which
        /// case the server endpoint is returned.
        /// 
        /// If no endpoint is found, returns the default endpoint.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override MusicEndpoint GetEndpointWithIndex(int index) {
            playbackMode = PlaybackMode.server;
            var maybe = TryGetEndpointWithIndex(index);
            if(maybe == null) {
                var endpoints = PlayerEndpoints;
                if(index >= 0 && endpoints.Count > index) {
                    var name = endpoints.ElementAt(index);
                    // "clients of" endpoint names start with a prefix
                    if(name.StartsWith(webPlayerPrefix)) {
                        var endpoint = EndpointsData.Endpoints
                            .FirstOrDefault(e => e.Name == name.Substring(webPlayerPrefix.Length));
                        if(endpoint != null) {
                            playbackMode = PlaybackMode.web;
                            return endpoint;
                        }
                        return DefaultEndpoint();
                    } else {
                        return DefaultEndpoint();
                    }
                } else {
                    return DefaultEndpoint();
                }
            } else {
                return maybe;
            }
        }
        private MusicEndpoint DefaultEndpoint() {
            playbackMode = PlaybackMode.server;
            return EndpointsData.Endpoints.ElementAt(0);
        }
        public BasePlayer BuildPlayer(MusicEndpoint e) {
            switch(e.EndpointType) {
                case EndpointTypes.Local:
                    return Provider.LocalPlayer;
                case EndpointTypes.PimpCloud:
                    return NewCloudPlayer(e);
                case EndpointTypes.MusicPimp:
                    switch(playbackMode) {
                        case PlaybackMode.server:
                            return NewPimpPlayer(e, () => LibraryManager.ActiveEndpoint);
                        case PlaybackMode.web:
                            var s = Provider.NewPimpSession(e);
                            return new PimpWebPlayer(s, NewPimpSocket(s));
                        default:
                            throw new NotImplementedException("Invalid playback mode: " + playbackMode);
                    }
                case EndpointTypes.Beam:
                    var s2 = Provider.NewBeamSession(e);
                    return Provider.NewBeamPlayer(s2, NewPimpSocket(s2));
                case EndpointTypes.Subsonic:
                    return NewSubsonicPlayer(e);
                default:
                    throw new NotImplementedException("Create player for: " + e.EndpointType);
            }
        }
        protected override void ActivateEndpoint(MusicEndpoint newPlaybackEndpoint) {
            SetPlayer(BuildPlayer(newPlaybackEndpoint));
            // if the player endpoint cannot receive music, it means it must be the source as well
            if(!newPlaybackEndpoint.CanReceiveMusic) {
                // LibraryManager is null when this is called from the superclass before the constructor is finished.
                // TODO fix this bullshit.
                if(LibraryManager != null) {
                    LibraryManager.SetActive(newPlaybackEndpoint);
                }
            }
        }
        public void SetPlayer(BasePlayer player) {
            var previousPlayer = Player;
            Player = player;
            if(previousPlayer != null) {
                OnPlayerDeactivated(previousPlayer);
            }
            OnPlayerActivated(Player);
        }

        private bool isWebPlayer(MusicEndpoint playbackDevice) {
            return playbackDevice.Name.StartsWith(webPlayerPrefix);
        }
        private void OnPlayerDeactivated(BasePlayer player) {
            if(PlayerDeactivated != null) {
                PlayerDeactivated(player);
            }
        }
        private void OnPlayerActivated(BasePlayer player) {
            if(PlayerActivated != null) {
                PlayerActivated(player);
            }
        }
    }
}
