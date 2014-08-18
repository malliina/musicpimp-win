using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class ExceptionAwareTransferModel : DownloadsViewModel {
        private static readonly int MAX_MESSAGES = 10;
        private static readonly string IsEmptyPropertyName = "IsMessagesEmpty";

        private ObservableCollection<string> messages;
        public ObservableCollection<string> Messages {
            get { return messages; }
            private set {
                this.SetProperty(ref this.messages, value, "Messages");
                OnPropertyChanged(IsEmptyPropertyName);
            }
        }
        public bool IsMessagesEmpty {
            get { return Messages.Count() == 0; }
        }
        public bool IsNotWaitingAndNoMessages { get { return IsMessagesEmpty && !WaitingStatus.IsWaiting; } }
        public ExceptionAwareTransferModel() {
            Messages = new ObservableCollection<string>();
        }
        public void UpdateProps() {
            WaitingStatus.UpdateProps();
            OnPropertyChanged("IsNotWaitingAndNoMessages");
        }
        public void AddMessage(string msg) {
            Messages.Add(msg);
            var overflow = Messages.Count() - MAX_MESSAGES;
            if (overflow > 0) {
                Messages = new ObservableCollection<string>(Messages.Skip(overflow));
            } else {
                OnPropertyChanged(IsEmptyPropertyName);
            }
        }
        public override Task RemoveTransfer(string transferID) {
            return WithExceptionLogging(async () => await base.RemoveTransfer(transferID));
        }
        public override Task RemoveAllTransfers() {
            return WithMessageBox(async () => await base.RemoveAllTransfers());
        }
        public Task WithExceptionLogging(Action code) {
            return WithExceptionHandling(TaskEx.Run(code), e => AddMessage("Failed: " + e.Message));
        }
        public Task WithMessageBox(Action code) {
            return WithExceptionHandling(TaskEx.Run(code), e => Send("Failed: " + e.Message));
        }
        public async Task WithExceptionHandling(Task code, Action<Exception> onException) {
            try {
                await code;
            } catch (Exception e) {
                onException(e);
            }
        }
    }
}
