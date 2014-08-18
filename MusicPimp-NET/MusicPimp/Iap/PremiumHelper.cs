using Mle.Iap;
using Mle.MusicPimp.Util;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Iap {
    public class PremiumHelper {
        private static PremiumHelper instance = null;
        public static PremiumHelper Instance {
            get {
                if(instance == null)
                    instance = new PremiumHelper();
                return instance;
            }
        }
        public static readonly string premiumProductId = "org.musicpimp.premium";
        private IIapUtils iapUtils;

        protected PremiumHelper() {
            this.iapUtils = ProviderService.Instance.IapHelper;
        }
        public Task<ProductInfo> PremiumInfo() {
            return iapUtils.ProductInfo(premiumProductId);
        }

        public Task<bool> IsPremiumAvailable() {
           return iapUtils.IsProductAvailable(premiumProductId);
        }
        public bool HasPremium() {
            return iapUtils.OwnsProduct(premiumProductId);
        }
        public Task PurchasePremium() {
            return iapUtils.PurchaseDurable(premiumProductId);
        }
    }
}
