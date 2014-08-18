using System;
using System.Threading.Tasks;

namespace Mle.Util {
    public class UiService {
        private static UiService instance = null;
        public static UiService Instance {
            get {
                if (instance == null)
                    instance = new UiService();
                return instance;
            }
        }

        private UiUtils UiUtils;

        public void SetUiUtils(UiUtils utils) {
            UiUtils = utils;
        }
        public Task OnUiThreadAsync(Action uiCode) {
            return UiUtils.OnUiThreadAsync(uiCode);
        }
    }
}
