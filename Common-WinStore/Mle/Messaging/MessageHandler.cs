using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Mle.Messaging {
    public class MessageHandler : IMessageHandler {
        public async Task Handle(SimpleMessage msg) {
            var diag = new MessageDialog(msg.Content);
            // throws UnauthorizedAccessException
            await diag.ShowAsync();
        }

        public async Task Handle(HeaderMessage msg) {
            var diag = new MessageDialog(msg.Content, msg.Title);
            await diag.ShowAsync();
        }
    }
}
