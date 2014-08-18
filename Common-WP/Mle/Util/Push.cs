using Microsoft.Phone.Notification;
using Mle.Exceptions;
using System;
using System.Threading.Tasks;

namespace Mle.Util {
    public class Push {
        private static readonly string
            ChannelName = "PushChannel",
            IsPushEnabledKey = "push_enabled_setting";
        public static HttpNotificationChannel GetChannelOpt() {
            return HttpNotificationChannel.Find(ChannelName);
        }
        private static ISettingsManager settings {
            get { return Settings.Instance; }
        }
        
        /// <summary>
        /// There must only be one notification channel per device per app.
        /// 
        /// http://msdn.microsoft.com/en-us/library/ff967566%28v=vs.92%29.aspx
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static Task<HttpNotificationChannel> Open() {
            var tcs = new TaskCompletionSource<HttpNotificationChannel>();
            HttpNotificationChannel channel = GetChannelOpt();
            if(channel == null) {
                channel = new HttpNotificationChannel(ChannelName);
                EventHandler<NotificationChannelUriEventArgs> handler = null;
                EventHandler<NotificationChannelErrorEventArgs> errorHandler = null;
                handler = (s, e) => {
                    channel.ChannelUriUpdated -= handler;
                    channel.ErrorOccurred -= errorHandler;
                    SaveOpen(true);
                    tcs.TrySetResult(channel);
                };
                errorHandler = (s, e) => {
                    channel.ErrorOccurred -= errorHandler;
                    channel.ChannelUriUpdated -= handler;
                    tcs.TrySetException(new NotificationException(e.Message));
                };
                channel.ChannelUriUpdated += handler;
                channel.ErrorOccurred += errorHandler;
                channel.Open();
                channel.BindToShellToast();
            } else {
                if(channel.ConnectionStatus == ChannelConnectionStatus.Disconnected) {
                    tcs.TrySetException(new NotificationException("Not connected. Check your network connectivity."));
                } else if(channel.ChannelUri == null) {
                    tcs.TrySetException(new NotificationException("Unable to obtain push notification URI."));
                } else {
                    SaveOpen(true);
                    tcs.TrySetResult(channel);
                }
            }
            return tcs.Task;
        }

        public static bool IsChannelOpen() {
            return settings.Load<bool>(IsPushEnabledKey, def: false);
        }
        /// <summary>
        /// Disables push notifications for this app.
        /// 
        /// In addition to calling this method, the user should also unregister any 
        /// push URLs from the cloud server when turning off push notifications.
        /// </summary>
        public static void Close() {
            CloseChannel();
            SaveOpen(false);
        }
        /// <summary>
        /// Unreliable. Don't use.
        /// </summary>
        /// <returns></returns>
        public static bool ChannelExists() {
            return GetChannelOpt() != null;
        }
        private static void SaveOpen(bool value) {
            settings.Save<bool>(IsPushEnabledKey, value);
        }
        private static void CloseChannel() {
            var channelOpt = GetChannelOpt();
            if(channelOpt != null) {
                Utils.Suppress<Exception>(channelOpt.Close);
            }
        }
    }
}
