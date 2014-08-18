
using Mle.MusicPimp.ViewModels;
using System;
namespace Mle.ViewModels {
    /// <summary>
    /// View model for the case where you want to display one of three things:
    /// a) a "loading..." message
    /// b) a feedback message
    /// c) a successfully retrieved result
    /// 
    /// And you intend for only one of the above to be shown at any given time: 
    /// if the content is loading, show the "loading..." item
    /// if it's not loading and there's feedback, show the feedback
    /// if it's not loading and there's no feedback, show the result
    /// 
    /// Client code should then manage the IsLoading/FeedbackMessage as appropriate and bind UI controls to the properties.
    /// 
    /// </summary>
    public abstract class LoadingViewModel : NetworkedViewModel {
        private bool isLoading = false;
        public bool IsLoading {
            get { return isLoading; }
            set {
                this.SetProperty(ref this.isLoading, value);
                OnPropertyChanged("ShowFeedback");
                OnPropertyChanged("ShowResult");
                OnIsLoadingChanged(value);
            }
        }
        protected virtual void OnIsLoadingChanged(bool loading) {

        }

        private string feedbackMessage = null;
        public string FeedbackMessage {
            get { return feedbackMessage; }
            set {
                if (SetProperty(ref this.feedbackMessage, value)) {
                    OnPropertyChanged("HasFeedback");
                    OnPropertyChanged("ShowFeedback");
                    OnPropertyChanged("ShowResult");
                    OnFeedbackMessageChanged(value);
                }
            }
        }
        protected virtual void OnFeedbackMessageChanged(string msg) {

        }

        public virtual bool HasFeedback {
            get { return !string.IsNullOrEmpty(FeedbackMessage); }
        }

        public virtual bool ShowFeedback {
            get { return !IsLoading && HasFeedback; }
        }

        public virtual bool ShowResult {
            get { return !IsLoading && !HasFeedback; }
        }

        protected void WithLoading(Action code) {
            IsLoading = true;
            try {
                code();
            } finally {
                IsLoading = false;
            }
        }
    }
}
