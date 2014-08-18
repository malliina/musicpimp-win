using SuperSocket.ClientEngine;
using System;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Mle.Network {
    public static class WebSocketExtensions {
        public static Task OpenAsync(this WebSocket ws) {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            EventHandler<ErrorEventArgs> errorHandler = null;
            handler = (s, e) => {
                ws.Opened -= handler;
                ws.Error -= errorHandler;
                tcs.SetResult(true);
            };
            errorHandler = (s, e) => {
                ws.Error -= errorHandler;
                ws.Opened -= handler;
                tcs.SetException(e.Exception);
            };
            ws.Opened += handler;
            ws.Error += errorHandler;
            ws.Open();
            return tcs.Task;
        }
        public static Task CloseAsync(this WebSocket ws) {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler closedHandler = null;
            EventHandler<ErrorEventArgs> errorHandler = null;
            closedHandler = (s, e) => {
                ws.Closed -= closedHandler;
                tcs.SetResult(true);
            };
            errorHandler = (s, e) => {
                ws.Error -= errorHandler;
                tcs.SetException(e.Exception);
            };
            ws.Closed += closedHandler;
            ws.Error += errorHandler;
            ws.Close();
            return tcs.Task;
        }
    }
}
