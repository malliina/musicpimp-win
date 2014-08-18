using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Mle.Util {
    public class StoreUtil : UiUtils {
        private static StoreUtil instance = null;
        public static StoreUtil Instance {
            get {
                if (instance == null)
                    instance = new StoreUtil();
                return instance;
            }
        }
        /// <summary>
        /// Executes the given code on the UI thread.
        /// 
        /// Example use: Changes to the MediaElement (play, pause, stop) must be done on the UI thread.
        /// </summary>
        /// <param name="uiCode"></param>
        /// <returns></returns>
        public async Task OnUiThreadAsync(Action uiCode) {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => uiCode());
        }
        public static Task OnUiThread(Action uiCode) {
            return Instance.OnUiThreadAsync(uiCode);
        }
        public static async Task<T> OnUiCompute<T>(Func<T> job) {
            T ret = default(T);
            await OnUiThread(() => {
                ret = job();
            });
            return ret;
        }
    }
}
