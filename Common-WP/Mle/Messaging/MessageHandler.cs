using Mle.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Mle.Messaging {
    public class MessageHandler : IMessageHandler {

        public Task Handle(SimpleMessage msg) {
            return OnUiThread(() => MessageBox.Show(msg.Content));
        }

        public Task Handle(HeaderMessage msg) {
            return OnUiThread(() => MessageBox.Show(msg.Content, msg.Title, MessageBoxButton.OK));
        }
        
        private Task OnUiThread(Action code) {
            return PhoneUtil.OnUiThread(code);
        }
    }
}
