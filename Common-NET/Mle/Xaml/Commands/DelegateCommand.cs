using System;
using System.Windows.Input;

namespace Mle.Xaml.Commands {
    /// <summary>
    /// http://stackoverflow.com/questions/11960488/any-winrt-icommand-commandbinding-implementaiton-samples-out-there/11964495#11964495
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateCommand<T> : CommandBase, ICommand {
        private readonly Func<T, bool> _canExecuteMethod;
        private readonly Action<T> _executeMethod;

        #region Constructors

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null) {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
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

        void ICommand.Execute(object parameter) {
            Execute((T)parameter);
        }

        #endregion ICommand Members

        #region Public Methods

        public bool CanExecute(T parameter) {
            return ((_canExecuteMethod == null) || _canExecuteMethod(parameter));
        }

        public void Execute(T parameter) {
            if (_executeMethod != null) {
                _executeMethod(parameter);
            }
        }
        #endregion Public Methods
    }
    

}
