using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mle.Util {
    public class PhoneUtil : UiUtils {
        private static PhoneUtil instance = null;
        public static PhoneUtil Instance {
            get {
                if (instance == null)
                    instance = new PhoneUtil();
                return instance;
            }
        }
        private static Dispatcher UiDispatcher {
            get { return Deployment.Current.Dispatcher; }
        }
        public Task OnUiThreadAsync(Action uiCode) {
            return UiDispatcher.InvokeAsync(uiCode);
        }
        public static Task OnUiThread(Action uiCode) {
            return UiDispatcher.InvokeAsync(uiCode);
        }
        public static Task<T> OnUiThreadAsync2<T>(Func<T> uiCode) {
            return UiDispatcher.InvokeAsync<T>(uiCode);
        }
        public static async Task<T> OnUi<T>(Func<Task<T>> f) {
            var taskOfTask = await UiDispatcher.InvokeAsync<Task<T>>(() => {
                return f();
            });
            return await taskOfTask;
        }
    }
}
