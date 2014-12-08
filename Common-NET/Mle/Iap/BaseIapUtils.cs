using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.Iap {
    public abstract class BaseIapUtils : IIapUtils {
        public abstract Task<IEnumerable<ProductInfo>> AvailableProducts();
        public abstract IEnumerable<string> OwnedProductIDs();
        public abstract Task SuggestPremium();
        public abstract Task PurchaseDurable(string productId);
        public async Task<ProductInfo> ProductInfo(string productId) {
            return (await AvailableProducts()).FirstOrDefault(p => p.ProductId == productId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId">ID of the IAP item</param>
        /// <returns>the details of the IAP item with the given product ID</returns>
        public async Task<bool> IsProductAvailable(string productId) {
            var listing = await AvailableProducts();
            return listing.Any(prod => prod.ProductId == productId);
        }
        public bool OwnsProduct(string productId) {
            return OwnedProductIDs().Contains(productId);
        }
    }
}
