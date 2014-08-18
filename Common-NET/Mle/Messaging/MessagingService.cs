using System;
using System.Threading.Tasks;

namespace Mle.Messaging {
    public class MessagingService {

        private static MessagingService instance = null;
        public static MessagingService Instance {
            get {
                if (instance == null)
                    instance = new MessagingService();
                return instance;
            }
        }

        public event Func<SimpleMessage, Task> OnSimpleMessage;
        public event Func<HeaderMessage, Task> OnHeaderMessage;
        public event Func<Task> OnShowPremiumDialog;
        //public event Func<NeverAgainOptionMessage, Task> OnNeverAgainOptionMessage;

        public void Register(IMessageHandler handler) {
            OnSimpleMessage += handler.Handle;
            OnHeaderMessage += handler.Handle;
            //OnShowPremiumDialog += handler.SuggestPremium;
            //OnNeverAgainOptionMessage += handler.Handle;
        }
        public void Send(string msg) {
            Send(new SimpleMessage(msg));
        }
        public void Send(string header, string msg) {
            Send(new HeaderMessage(header, msg));
        }
        //public void MaybeSend(string msg,string key) {
        //    //ApplicationData.Current.RoamingSettings
        //}
        public void Send(SimpleMessage msg) {
            if (OnSimpleMessage != null) {
                OnSimpleMessage(msg);
            }
        }
        public void Send(HeaderMessage msg) {
            if (OnHeaderMessage != null) {
                OnHeaderMessage(msg);
            }
        }
        public void ShowPremiumDialog() {
            if(OnShowPremiumDialog != null) {
                OnShowPremiumDialog();
            }
        }
        //public void Send(NeverAgainOptionMessage msg) {
        //    if(OnNeverAgainOptionMessage != null) {
        //        OnNeverAgainOptionMessage(msg);
        //    }
        //}
    }

}
