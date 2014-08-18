using Mle.Network;
using Mle.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Mle {
    public class TestViewModel : ViewModelBase {

        private string output;
        public string Output {
            get { return this.output; }
            set { this.SetProperty(ref this.output, value); }
        }
        private static BitmapImage ImageFrom(string path) {
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
        public ImageSource PictureBackground {
            get { return ImageFrom("ms-appx:///Assets/music.png"); }
        } 

        public ICommand RunTest { get; private set; }
        public ICommand CloseCommand { get; private set; }

        //Uri uri = new Uri("ws://desktop:9000/ws/open");
        SimpleWebSocket ws = new SimpleWebSocket(new Uri("ws://desktop:9000/ws/open"), "admin", "test", "application/json");

        public TestViewModel() {
            RunTest = new AsyncUnitCommand(Test);
            CloseCommand = new UnitCommand(CloseTest);
        }
        public async Task Test() {
            await TestWebSocketsWrapper();
        }
        public void CloseTest() {
            ws.Close();
        }
        public async Task TestWebSocketsWrapper() {
            await ws.Connect();
            ws.Opened += () => SetOutput("Opened!");
            ws.MessageReceived += msg => SetOutput("Got msg: " + msg);
            ws.Closed += reason => SetOutput("Closed. " + reason);
            Output = "Connected.";
            //await ws.Send(@"{""cmd"":""status""}");
            //Output = "Sent message";
        }
        private async void SetOutput(string msg) {
            await StoreUtil.OnUiThread(() => Output = msg);
        }
        public async Task TestWebSockets() {
            var webSocket = new MessageWebSocket();
            webSocket.Control.MessageType = SocketMessageType.Utf8;
            webSocket.MessageReceived += webSocket_MessageReceived;
            webSocket.SetRequestHeader(HttpUtil.Authorization, HttpUtil.BasicAuthHeader("admin", "test"));
            var uri = new Uri("ws://desktop:9000/ws/open");
            await webSocket.ConnectAsync(uri);
            Output = "Connected!";
            var writer = new DataWriter(webSocket.OutputStream);
            writer.WriteString(@"{""cmd"":""status""}");
            await writer.StoreAsync();

        }

        async void webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
            var reader = args.GetDataReader();
            var msg = reader.ReadString(reader.UnconsumedBufferLength);
            await StoreUtil.OnUiThread(() => Output = msg);
        }
    }
}
