using Id3;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Notification;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebSocket4Net;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Mle.Collections;
using System.Collections.ObjectModel;
using Microsoft.Phone.Info;

namespace Mle.ViewModels {
    public class TestModel : ViewModelBase {
        private static TestModel instance = null;
        public static TestModel Instance {
            get {
                if(instance == null)
                    instance = new TestModel();
                return instance;
            }
        }
        private List<HostName> addresses;
        public List<HostName> Addresses {
            get { return addresses; }
            set { this.SetProperty(ref this.addresses, value); }
        }
        private string output = "Welcome.";
        public string Output {
            get { return this.output; }
            set { this.SetProperty(ref this.output, value); }
        }
        private string output2;
        public string Output2 {
            get { return this.output2; }
            set { this.SetProperty(ref this.output2, value); }
        }
        MusicEndpoint devEndpoint = new MusicEndpoint() {
            Name = "pimp",
            Server = "desktop",
            Port = 9000,
            Username = "admin",
            Password = "test"
        };

        private MusicEndpoint testEndpoint = new MusicEndpoint() {
            Name = "test endpoint",
            Server = "desktop",
            Port = 8456,
            Username = "admin",
            Password = "test"
        };
        public ICommand RunTest { get; private set; }
        public ICommand Close { get; private set; }
        public ICommand WithGzip { get; private set; }
        public ICommand WithoutGzip { get; private set; }
        public ICommand Upload { get; private set; }
        WebSocket4Net.WebSocket webSocket;

        public ObservableCollection<string> Days {
            get { return new ObservableCollection<string>() { "mon", "tue", "wed", "thu", "fri", "sat", "sun" }; }
        }
        

        public TestModel() {
            RunTest = new AsyncUnitCommand(TestAnid2);
        }
        private async Task TestAnid2() {
            var anid2 = UserExtendedProperties.GetValue("ANID2") as string;
            if(anid2 != null) {
                await SetOutput(anid2);
            } else {
                await SetOutput("No ANID2. Running on emulator?");
            }
        }
        private Task TestCommand() {
            return TestMPNS();
        }
        private async Task TestMPNS() {
            var service = new PushService("ToastSampleChannel", new Uri("http://desktop:9000"), "/clocks");
            await service.Init();
        }
        private async Task UpdateChannelUri(Uri uri) {
            var msg = "Channel URI: " + uri.ToString();
            Debug.WriteLine(msg);
            await SetOutput(msg);
        }
        private async Task TestHttpExceptions() {
            var session = new PhonePimpSession(new MusicEndpoint("10.0.0.123", "secret"));
            try {
                Output = "Pinging...";
                await session.Ping();
            } catch(Exception e) {
                Output = "" + e.GetType() + ": " + e.Message;
            }
        }

        private void TestHostInfo() {
            var infos = NetworkInformation.GetHostNames()
                           .Select(n => "Canonical: " + n.CanonicalName +
                               ", Display: " + n.DisplayName +
                               ", Raw: " + n.RawName +
                               ", Type: " + n.Type +
                               ", Prefix length: " + Convert.ToInt32(n.IPInformation.PrefixLength));
            foreach(var info in infos) {
                Debug.WriteLine(info);
            }
        }
        private async Task TestHttpClient() {
            //var uri = "http://www.google.com";
            var uri = "https://beam.musicpimp.org";
            var handler = new HttpClientHandler();
            if(handler.SupportsAutomaticDecompression) {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            } else {
                handler.AutomaticDecompression = DecompressionMethods.None;
            }
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(uri);
            //var responseContent = await client.GetString("/intl/en/about/index.html");
            var responseContent = await client.GetString("/ping");
            Output = responseContent;
        }
        public void TestTagLib() {
            var fileName = "some.mp3";
            var assetFile = "Assets/" + fileName;
            var assetStream = Application.GetResourceStream(new Uri(assetFile, UriKind.Relative)).Stream;
            using(var storage = IsolatedStorageFile.GetUserStoreForApplication()) {
                using(var fileStream = storage.OpenFile(fileName, FileMode.OpenOrCreate)) {
                    assetStream.CopyTo(fileStream);
                }
                using(var fileStream = storage.OpenFile(fileName, FileMode.OpenOrCreate)) {
                    Output = "Bytes: " + fileStream.Length;
                }
            }
        }
        public void ReadID3(Stream stream) {
            try {
                using(var mp3 = new Mp3Stream(stream)) {
                    var tags = new List<Id3Tag>(mp3.GetAllTags());
                    if(tags.Count > 0) {
                        var firstTag = tags.First();
                        var title = firstTag.Title.Value;
                        if(String.IsNullOrWhiteSpace(title))
                            title = "crazy";
                        //return new SongInfo(title, firstTag.Artists.Value, firstTag.Album.Value);
                    }
                }
            } catch(IndexOutOfRangeException iore) {
                // maybe thrown if the file is cocked up and id3 tags cannot be read
                Debug.WriteLine(iore.Message);
            }
        }
        private async Task TestWebSockets() {
            var authHeader = new KeyValuePair<string, string>(HttpUtil.Authorization, HttpUtil.BasicAuthHeader("d30f4e3c-818f-4513-934f-9bc828b42221", "beam"));
            var formatHeader = new KeyValuePair<string, string>(HttpUtil.Accept, HttpUtil.Json);
            var headers = new List<KeyValuePair<string, string>>() { authHeader, formatHeader };
            webSocket = new WebSocket4Net.WebSocket("ws://desktop:9001/ws/control", customHeaderItems: headers);
            webSocket.Opened += webSocket_Opened;
            webSocket.MessageReceived += webSocket_MessageReceived;
            await SetOutput("Opening...");
            webSocket.Open();
        }
        private void TestClose() {
            webSocket.Close();
        }
        private async void ws_MessageReceived(string msg) {
            await SetOutput("Got msg: " + msg);
        }

        private async void ws_Closed(string reason) {
            await SetOutput("Closed. " + reason);
        }

        private async void ws_Error() {
            await SetOutput("Error");
        }
        private async Task SetOutput(string msg) {
            await PhoneUtil.OnUiThread(() => Output = msg);
        }


        private async void webSocket_MessageReceived(object sender, MessageReceivedEventArgs e) {
            await PhoneUtil.OnUiThread(() => Output = e.Message);
        }

        private async void webSocket_Opened(object sender, EventArgs e) {
            await PhoneUtil.OnUiThread(() => Output = "Opened!");
            //var msg = @"{""cmd"":""status""}";
            //webSocket.Send(msg);
        }
        private async Task TestException() {
            var result = await WithExceptionEvents2(async () => {
                await Task.Delay(1000);
                throw new Exception("cockup");
            });
            MessageBox.Show(result);
        }
        private async Task<double> Measured(Func<Task> code) {
            var start = DateTime.UtcNow;
            for(int i = 0; i < 100; ++i) {
                await code();
            }
            var end = DateTime.UtcNow;
            return (end - start).TotalMilliseconds;
        }
        private async Task GzipVsNoGzip(bool compression) {
            var s = new PhonePimpSession(testEndpoint, acceptCompression: compression);

            var duration = await Measured(async () => {
                await s.RootContentAsync();
            });
            string withOrWithout = null;
            if(compression) {
                withOrWithout = "with";
            } else {
                withOrWithout = "without";
            }
            Output = "Ran " + withOrWithout + " compression in " + duration + " ms.";
        }
        private async Task WithCompression() {
            await GzipVsNoGzip(compression: true);
        }
        private async Task WithoutCompression() {
            await GzipVsNoGzip(compression: false);
        }

        protected async Task<string> WithExceptionEvents2(Func<Task> code) {
            try {
                await code();
                return "Completed";
            } catch(Exception) {
                return "Caught";
            }
        }

        private async Task TestHttpClient2() {
            var m = HttpMethod.Post;
            if(m == HttpMethod.Get) {
                MessageBox.Show("GET");
            } else if(m == HttpMethod.Post) {
                MessageBox.Show("POST");
            } else {
                MessageBox.Show("Unknown");
            }
            //var credHandler = new HttpClientHandler() {
            //    Credentials = new NetworkCredential("admin", "test")
            //};
            var credBytes = Encoding.UTF8.GetBytes(String.Format("{0}:{1}", "admin", "test"));
            var cred = Convert.ToBase64String(credBytes);
            // no need to dispose
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var headers = client.DefaultRequestHeaders;
            headers.Accept.ParseAdd("application/json");
            headers.Authorization = new AuthenticationHeaderValue("Basic", cred);
            headers.AcceptEncoding.ParseAdd("gzip");
            client.BaseAddress = new Uri("http://169.254.80.80:9000");
            var result = await client.GetAsync("/folders");
            var str = await result.Content.ReadAsStringAsync();
            Output = str.Trim();
        }
        public void Load() {
            Addresses = new List<HostName>(NetworkInformation.GetHostNames());
            foreach(HostName h in Addresses) {
                int.Parse(h.IPInformation.PrefixLength.ToString());
            }
        }
    }
}
