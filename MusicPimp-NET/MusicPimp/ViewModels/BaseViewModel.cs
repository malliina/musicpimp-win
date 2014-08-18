using Mle.MusicPimp.Iap;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class BaseViewModel : WebAwareLoading {
        public async Task UsageControlled(Func<Task> code) {
            if(UsageController.Instance.IsPlaybackAllowed()) {
                await WithExceptionEvents(code);
            } else {
                await ProviderService.Instance.IapHelper.SuggestPremium();
            }
        }
    }
}
