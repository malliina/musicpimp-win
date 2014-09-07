using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// Player initialization protocol:
    /// 
    /// Client          Server
    ///   | subscribe ->  |
    ///   | <- opened     |
    ///   | <- welcome    |
    ///   | get status -> | 
    ///   | <- status     |
    ///   
    /// Opened is always sent, but if authentication failed it is immediately followed by
    /// an EOF from the server, closing the connection.
    /// The welcome message is only sent if authentication succeeded.
    /// </summary>
    public abstract class WebSocketPlayer : BasePlayer {
        protected PimpWebSocket webSocket;
        protected SimplePimpSession session;
        public SimplePimpSession Session { get; private set; }

        public WebSocketPlayer(SimplePimpSession session, PimpWebSocket webSocket) {
            Session = session;
            this.webSocket = webSocket;
            this.session = session;
            // the lifetime of this player is the same as the websocket
            // so we don't care about deregistering event handlers
            webSocket.MuteToggled += async muteOrNot => await OnUiThread(() => IsMute = muteOrNot);
            webSocket.VolumeChanged += async volume => await OnUiThread(() => Volume = volume);
            webSocket.TimeUpdated += OnTimeUpdated;
            webSocket.TrackChanged += async track => {
                // TODO do better
                FeedbackMessage = null;
                await OnUiThread(() => NowPlaying = track);
            };
            webSocket.PlayStateChanged += async state => await OnUiThread(() => CurrentPlayerState = state);
            webSocket.StatusUpdateReceived += UpdateStatus;
            webSocket.ShortStatusUpdateReceived += async shortStatus => await UpdateShortStatus(shortStatus);
            webSocket.Welcomed += async () => {
                await OnUiThread(() => IsOnline = true);
                await PostSimple("status");
            };
            //webSocket.Disconnected += str => IsOnline = false;
            webSocket.SocketClosed += async str => await OnUiThread(() => IsOnline = false);
            webSocket.ErrorOccurred += async () => await OnUiThread(() => IsOnline = false);
            IsEventBased = true;
        }
        public override Task Subscribe() {
            return OpenWebSocketAsync();
        }
        private async Task OpenWebSocketAsync() {
            // only completes the task after the Welcome message has been received
            var tcs = new TaskCompletionSource<bool>();
            Action welcomeHandler = null;
            Action<string> disconnectedHandler = null;
            welcomeHandler = () => {
                webSocket.Welcomed -= welcomeHandler;
                webSocket.Disconnected -= disconnectedHandler;
                tcs.TrySetResult(true);
            };
            disconnectedHandler = msg => {
                webSocket.Disconnected -= disconnectedHandler;
                webSocket.Welcomed -= welcomeHandler;
                tcs.TrySetException(new ConnectivityException(msg));
            };
            webSocket.Welcomed += welcomeHandler;
            webSocket.Disconnected += disconnectedHandler;
            await webSocket.Socket.Connect();
            await tcs.Task;
        }
        public override void Unsubscribe() {
            webSocket.Socket.Close();
            IsOnline = false;
        }
        private Task UpdateShortStatus(ShortStatusJsonEvent ev) {
            return OnUiThread(() => {
                CurrentPlayerState = PimpWebSocket.FromName(ev.state);
                TrackPosition = TimeSpan.FromSeconds(ev.position);
                Volume = ev.volume;
                IsMute = ev.mute;
            });
        }
        public virtual async void OnTimeUpdated(double time) {
            await OnUiThread(() => TrackPosition = TimeSpan.FromSeconds(time));
        }
        public override Task pause() {
            return PostSimple("stop");
        }
        public override Task seek(double pos) {
            return Post("seek", (int)pos);
        }
        public override Task SetVolume(int newVolume) {
            return Post("volume", newVolume);
        }

        protected Task PostSimple(string cmd) {
            var command = new SimpleCommand(cmd);
            return webSocket.Send(command);
        }

        protected Task Post<T>(string cmd, T value) {
            var command = new GenericCommand<T>(cmd, value);
            return webSocket.Send(command);
        }
        protected Task Send(JsonContent content) {
            return webSocket.Send(content);
        }
    }
}
