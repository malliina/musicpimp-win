using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.Xaml.Commands {
    public class AsyncDelegateCommand<T> : CommandBase, ICommand {
        private readonly Func<T, bool> _canExecuteMethod;
        private readonly Func<T, Task> _executeMethod;

        #region Constructors

        public AsyncDelegateCommand(Func<T, Task> executeMethod)
            : this(executeMethod, null) {
        }

        public AsyncDelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) {
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion Constructors

        #region ICommand Members


        bool ICommand.CanExecute(object parameter) {
            try {
                return CanExecute((T)parameter);
            } catch { return false; }
        }

        async void ICommand.Execute(object parameter) {
            await Execute((T)parameter);
        }

        #endregion ICommand Members

        #region Public Methods

        public bool CanExecute(T parameter) {
            return ((_canExecuteMethod == null) || _canExecuteMethod(parameter));
        }

        public async Task Execute(T parameter) {
            if (_executeMethod != null) {
                await _executeMethod(parameter);
            }
        }
        #endregion Public Methods
    }
}
