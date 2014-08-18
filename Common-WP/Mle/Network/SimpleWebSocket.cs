using Mle.Concurrent;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Mle.Network {
    public class SimpleWebSocket : WebSocketBase {

        private WebSocket ws;

        public SimpleWebSocket(Uri uri, string userName, string password, string mediaType)
            : base(uri, userName, password, mediaType) {
        }

        public override async Task Connect() {
            // ServerCredentials property does not work
            var authHeader = new KeyValuePair<string, string>(HttpUtil.Authorization, HttpUtil.BasicAuthHeader(UserName, Password));
            var acceptHeader = new KeyValuePair<string, string>(HttpUtil.Accept, MediaType);
            var headers = new List<KeyValuePair<string, string>>() { authHeader, acceptHeader };
            ws = new WebSocket(ServerUri.OriginalString, customHeaderItems: headers);
            ws.MessageReceived += async (s, args) => {
                await PhoneUtil.OnUiThread(() => OnMessageReceived(args.Message));
            };
            ws.Closed += (s, e) => OnClosed(String.Empty);
            ws.Error += (s, e) => OnError();
            //Debug.WriteLine("Attempting to connect to: " + ServerUri + " as user: " + UserName + " with password: " + Password);
            await ws.OpenAsync();
            OnOpened();
        }

        public override Task Send(string content) {
            if(ws != null && IsConnected && ws.State == WebSocketState.Open) {
                ws.Send(content);
            }
            return AsyncTasks.Noop();
        }

        protected override void CloseSocket() {
            IsConnected = false;
            try {
                if(ws != null) {
                    //ws.Close(1000, "Closed by client.");
                    //await ws.CloseAsync();
                    Utils.Suppress<Exception>(ws.Close);
                }
            } catch(Exception) { }
        }
    }
}
