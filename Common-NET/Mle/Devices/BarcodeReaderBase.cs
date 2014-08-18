using Mle.ViewModels;
using System;

namespace Mle.Devices {
    public abstract class BarcodeReaderBase : ViewModelBase {
        /// <summary>
        /// Called whenever a barcode has been successfully read.
        /// </summary>
        public event Action<string> CodeAvailable;

        protected void OnCodeAvailable(string code) {
            if (CodeAvailable != null) {
                CodeAvailable(code);
            }
        }
        public virtual void Dispose() { }
    }
}
