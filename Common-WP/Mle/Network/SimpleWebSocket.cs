using Mle.Concurrent;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Mle.Network {
    public class SimpleWebSocket : WebSocketBase {

        private WebSocket ws;

        public SimpleWebSocket(Uri uri, AuthenticationHeaderValue authHeader, string mediaType)
            : base(uri, authHeader, mediaType) {
            // ServerCredentials property does not work
            var authHeaderPair = new KeyValuePair<string, string>(HttpUtil.Authorization, authHeader.Scheme + " " + authHeader.Parameter);
            var acceptHeader = new KeyValuePair<string, string>(HttpUtil.Accept, MediaType);
            var headers = new List<KeyValuePair<string, string>>() { authHeaderPair, acceptHeader };
            ws = new WebSocket(ServerUri.OriginalString, customHeaderItems: headers);
            ws.Closed += (s, e) => OnClosed(String.Empty);
            ws.Error += (s, e) => OnError();
            ws.MessageReceived += async (s, args) => {
                // Why UI thread?
                await PhoneUtil.OnUiThread(() => OnMessageReceived(args.Message));
            };
        }
        public override async Task Connect() {
            await ws.OpenAsync();
            OnOpened();
        }

        public override Task Send(string content) {
            if(IsConnected && ws.State == WebSocketState.Open) {
                ws.Send(content);
            }
            return AsyncTasks.Noop();
        }

        protected override void CloseSocket() {
            //IsConnected = false;
            Utils.Suppress<Exception>(ws.Close);
        }
    }
}
