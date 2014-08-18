using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mle.ViewModels {
    abstract public class ChangingViewModelBase : ViewModelBase, INotifyPropertyChanging {

        public event PropertyChangingEventHandler PropertyChanging;

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            if (object.Equals(storage, value)) return false;
            this.OnPropertyChanging(propertyName);
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanging([CallerMemberName] string propertyName = null) {
            var eventHandler = this.PropertyChanging;
            if (eventHandler != null) {
                eventHandler(this, new PropertyChangingEventArgs(propertyName));
            }
        }
    }
}
