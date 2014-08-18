using System;

namespace Mle.Xaml.Commands {
    public abstract class CommandBase {
        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        #region Protected Methods

        protected virtual void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        #endregion Protected Methods
    }
}
