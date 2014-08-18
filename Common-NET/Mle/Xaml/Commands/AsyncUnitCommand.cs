using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.Xaml.Commands {
    public class AsyncUnitCommand : CommandBase, ICommand {
        private readonly Func<bool> _canExecuteMethod;
        private readonly Func<Task> _executeMethod;

        #region Constructors

        public AsyncUnitCommand(Func<Task> executeMethod)
            : this(executeMethod, null) {
        }

        public AsyncUnitCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod) {
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion Constructors

        #region ICommand Members

        bool ICommand.CanExecute(object parameter) {
            try {
                return CanExecute();
            } catch { return false; }
        }

        async void ICommand.Execute(object parameter) {
            await Execute();
        }

        #endregion ICommand Members

        #region Public Methods

        public bool CanExecute() {
            return ((_canExecuteMethod == null) || _canExecuteMethod());
        }

        public async Task Execute() {
            if (_executeMethod != null) {
                await _executeMethod();
            }
        }

        #endregion Public Methods
    }
}
