using System;

namespace Mle.ViewModels {
    /// <summary>
    /// http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form/2100824#2100824
    /// </summary>
    public class ClosingViewModel : MessagingViewModel {
        public event Action CloseRequested;
        public virtual void Close() {
            if (CloseRequested != null) {
                CloseRequested();
            }
        }
    }
}
