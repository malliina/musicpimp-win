using System;
using System.Windows.Input;

namespace Mle.Xaml.Commands {
    /// <summary>
    /// http://stackoverflow.com/questions/11960488/any-winrt-icommand-commandbinding-implementaiton-samples-out-there/11964495#11964495
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnitCommand : CommandBase, ICommand {
        private readonly Func<bool> _canExecuteMethod;
        private readonly Action _executeMethod;

        #region Constructors

        public UnitCommand(Action executeMethod)
            : this(executeMethod, null) {
        }

        public UnitCommand(Action executeMethod, Func<bool> canExecuteMethod) {
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

        void ICommand.Execute(object parameter) {
            Execute();
        }

        #endregion ICommand Members

        #region Public Methods

        public bool CanExecute() {
            return ((_canExecuteMethod == null) || _canExecuteMethod());
        }

        public void Execute() {
            if(_executeMethod != null) {
                _executeMethod();
            }
        }

        #endregion Public Methods

    }

}
