using Mle.Messaging;
using System;
using System.Threading.Tasks;

namespace Mle.ViewModels {
    public class MessagingViewModel : LoadingViewModel {
        private MessagingService Messaging { get { return MessagingService.Instance; } }
        /// <summary>
        /// Sends a message to the messaging service, to be displayed in the UI.
        /// 
        /// UI classes can register event handlers on the MessagingService instance
        /// to capture messages and act accordingly, typically displaying a 
        /// popup or feedback message to the user.
        /// </summary>
        /// <param name="message">message content</param>
        protected void Send(string message) {
            Send(new SimpleMessage(message));
        }
        /// <summary>
        /// Sends a message to the messaging service, including a message header.
        /// </summary>
        /// <param name="header">message header (for example in a popup)</param>
        /// <param name="message">message content</param>
        protected void Send(string header, string message) {
            Send(new HeaderMessage(header, message));
        }
        protected void Send(SimpleMessage message) {
            MessagingService.Instance.Send(message);
        }
        protected Task WithExceptionEvents2(Action code) {
            return WithExceptionEvents(() => TaskEx.Run(code));
        }
        /// <summary>
        /// Implementation note: By requiring a Func<Task> as opposed to an Action, any exceptions caught in the async lambda 
        /// are caught by the try-catch.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected async Task WithExceptionEvents(Func<Task> code) {
            try {
                await code();
            } catch(Exception e) {
                OnException(e);
            }
        }
        protected virtual void OnException(Exception e) {
            Send("An error occurred. " + e.Message);
        }

        protected PageNavigationService Navigator {
            get { return PageNavigationService.Instance; }
        }
    }
}
