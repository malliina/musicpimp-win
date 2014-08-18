using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Mle.Network {
    public class SimpleWebSocket : WebSocketBase {
        private MessageWebSocket ws;
        private DataWriter writer;

        public SimpleWebSocket(Uri uri, string userName, string password, string mediaType)
            : base(uri, userName, password, mediaType) {
            ws = new MessageWebSocket();
            ws.Control.MessageType = SocketMessageType.Utf8;
            ws.SetRequestHeader(HttpUtil.Authorization, HttpUtil.BasicAuthHeader(UserName, Password));
            ws.SetRequestHeader(HttpUtil.Accept, mediaType);
            ws.MessageReceived += ws_MessageReceived;
            ws.Closed += (s, args) => OnClosed(args.Reason);
        }

        private async void ws_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
            try {
                // throws Exception if eof is received, which is the case for example if auth fails
                var reader = args.GetDataReader();
                var message = reader.ReadString(reader.UnconsumedBufferLength);
                await StoreUtil.OnUiThread(() => OnMessageReceived(message));
            } catch(Exception) {
                // assumes eof has been received i.e. the server has closed the connection
                // http://msdn.microsoft.com/en-US/library/windows/apps/hh701366
                ws.Close(1005, String.Empty);
            }
        }
        public override async Task Connect() {
            // TODO throw an exception if the connection was unsuccessful, goddammit
            await ws.ConnectAsync(ServerUri);
            writer = new DataWriter(ws.OutputStream);
            OnOpened();
        }
        public override async Task Send(string content) {
            writer.WriteString(content);
            await writer.StoreAsync();
        }
        /// <summary>
        /// Idempotent.
        /// </summary>
        protected override void CloseSocket() {
            try {
                // the writer is null if the connection has never been successfully opened
                if(writer != null) {
                    writer.Dispose();
                }
                ws.Close(1000, "Closed by client.");
                ws.Dispose();
            } catch(Exception) {

            }

        }
    }
}
