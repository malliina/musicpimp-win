﻿using Mle.Util;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mle.Network {
    /// <summary>
    /// Wraps a WebSocket library, as there's nothing as a PCL.
    /// 
    /// Intended to wrap MessageWebSocket for WinRT and WebSocket4Net for WP.
    /// </summary>
    public abstract class WebSocketBase {

        public Uri ServerUri { get; private set; }
        public AuthenticationHeaderValue AuthHeader { get; private set; }
        public string MediaType { get; private set; }
        public bool IsConnected { get; set; }
        protected IList<KeyValuePair<string, string>> headers;

        public WebSocketBase(Uri uri, AuthenticationHeaderValue authHeader, string mediaType) {
            ServerUri = uri;
            AuthHeader = authHeader;
            MediaType = mediaType;
            IsConnected = false;
        }

        public event Action Opened;
        public event Action<string> Closed;
        public event Action Error;
        public event Action<string> MessageReceived;

        // We don't set IsConnected here because of the Welcome-message protocol
        protected void OnOpened() {
            if(Opened != null) {
                Opened();
            }
        }
        protected void OnClosed(string reason) {
            if(Closed != null) {
                Closed(reason);
            }
            IsConnected = false;
        }
        protected void OnError() {
            if(Error != null) {
                Error();
            }
        }
        protected void OnMessageReceived(string message) {
            if(MessageReceived != null) {
                MessageReceived(message);
            }
        }
        /// <summary>
        /// Opens a websocket connection to the server.
        /// 
        /// If authentication fails, this method may still return normally,
        /// however the server may immediately send an eof and not handle
        /// any client messages. Failed auth cases are therefore 
        /// handled in the message handler, not here.
        /// 
        /// TODO: Make this throw an exception if the authentication fails,
        /// if possible.
        /// </summary>
        /// <returns></returns>
        public abstract Task Connect();
        public abstract Task Send(string content);
        public void Close() {
            Utils.Suppress<Exception>(CloseSocket);
        }
        protected abstract void CloseSocket();
    }
}
