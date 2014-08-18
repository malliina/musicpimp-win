using System;
using System.Threading.Tasks;

namespace Mle.Util {
    public interface UiUtils {
        Task OnUiThreadAsync(Action uiCode);
    }
}
