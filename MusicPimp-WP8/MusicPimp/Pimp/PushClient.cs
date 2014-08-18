using Microsoft.Phone.Notification;
using Mle.Concurrent;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class PushClient : IDisposable {
        // http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj662938%28v=vs.105%29.aspx#BKMK_Toastproperties
        private static Version ToastSoundSupportingVersion = new Version(8, 0, 10492);
        public static bool SupportsToastsWithCustomSound {
            get { return Environment.OSVersion.Version >= ToastSoundSupportingVersion; }
        }
        public MusicEndpoint Endpoint { get; private set; }

        private static readonly string pushAdd = "push_add", pushRemove = "push_remove", resource = "/alarms";
        private SimplePimpSession session;
        private string registrationId;
        private TimeSpan DefaultDeregisterTimeout = TimeSpan.FromSeconds(3);
        public PushClient(MusicEndpoint e) {
            this.registrationId = e.Id;
            this.session = new SimplePimpSession(e);
            Endpoint = e;
        }
        public Task Register(Uri uri) {
            return PostUrl(pushAdd, uri, registrationId);
        }
        public  Task Deregister(Uri uri) {
            return PostUrl(pushRemove, uri, registrationId);
        }
        private Task PostUrl(string cmd, Uri uri, string tag) {
            return Post(new PushCommand(cmd, uri.ToString(), SupportsToastsWithCustomSound, tag));
        }
        private Task Post(JsonContent content) {
            return session.PostCommand(content, resource);
        }
        public void Dispose() {
            session.Dispose();
        }
    }
    public class PushCommand : SimpleCommand {
        public string url { get; set; }
        public bool silent { get; set; }
        public string tag { get; set; }
        public PushCommand(string cmd, string url, bool silent, string tag)
            : base(cmd) {
            this.url = url;
            this.silent = silent;
            this.tag = tag;
        }
    }
}
