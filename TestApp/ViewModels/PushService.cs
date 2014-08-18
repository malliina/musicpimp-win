using Microsoft.Phone.Notification;
using Mle.MusicPimp.Pimp;
using Mle.Network;
using Mle.Util;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mle.ViewModels {
    public class PushService {
        private const string pushUriKey = "setting_push_uri";
        private static string pushAdd = "push_add", pushRemove = "pushRemove", stop = "stop";
        private string channelName;
        private string resource;
        private HttpClient client;
        private ISettingsManager settings;
        

        public PushService(string channelName, Uri baseAddress, string resource) {
            this.channelName = channelName;
            this.client = new HttpClient();
            client.BaseAddress = baseAddress;
            this.resource = resource;
            settings = Settings.Instance;
        }
        /// <summary>
        /// Call this on application startup.
        /// </summary>
        /// <returns></returns>
        public Task Init() {
            return InitPushChannel(channelName);
        }
        /// <summary>
        /// Ensures that the supplied URI is registered with the push notification server. 
        /// Following a successful registration, it's stored locally.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task RegisterUriIfNeeded(Uri uri) {
            var uriString = uri.ToString();
            var savedUri = settings.Load<string>(pushUriKey, def: String.Empty);
            try {
                //if(uriString != savedUri) {
                await RegisterUri(uri);
                //}
            } catch(Exception) {

            }
        }
        private async Task RegisterUri(Uri uri) {
            await PostUrl(pushAdd, uri, channelName);
            settings.Save<string>(pushUriKey, uri.ToString());
        }
        public Task DeregisterUri(Uri uri) {
            return PostUrl(pushRemove, uri, channelName);
        }
        public Task SendStop() {
            return Post(new SimpleCommand(stop));
        }
        private async Task<HttpNotificationChannel> InitPushChannel(string channelName) {
            HttpNotificationChannel channel = HttpNotificationChannel.Find(channelName);
            if(channel == null) {
                channel = new HttpNotificationChannel(channelName);
                RegisterHandlers(channel);
                channel.Open();
                // I think this can be omitted if we don't plan on intercepting toasts in the app.
                channel.BindToShellToast();
            } else {
                RegisterHandlers(channel);
                var uri = channel.ChannelUri;
                if(uri != null) {
                    await RegisterUriIfNeeded(uri);
                }
            }
            return channel;
        }
        private void RegisterHandlers(HttpNotificationChannel channel) {
            channel.ChannelUriUpdated += async (s, e) => await RegisterUriIfNeeded(e.ChannelUri);
        }
        private Task PostUrl(string cmd, Uri uri, string tag) {
            return Post(new PushCommand(cmd, uri.ToString(), PushClient.SupportsToastsWithCustomSound, tag));
        }
        private Task Post(JsonContent content) {
            return client.PostJson(content.Json(), resource);
        }
    }
    
}
