using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;

namespace Mle.MusicPimp.Pimp {
    /// <summary>
    /// Syncs playlist based on websocket events from the player.
    /// </summary>
    public abstract class WebSocketPlaylistBase : BasePlaylist {
        protected PimpWebSocket webSocket;

        public WebSocketPlaylistBase(PimpWebSocket webSocket) {
            this.webSocket = webSocket;
            IsEventBased = true;
            AutoPlay = true;
            webSocket.PlaylistModified += Sync;
            webSocket.PlaylistIndexChanged += index => Index = index;
        }
    }
}
