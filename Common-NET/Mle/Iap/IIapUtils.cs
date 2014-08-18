using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.Iap {
    public interface IIapUtils {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the IAP products available for purchase</returns>
        Task<IEnumerable<ProductInfo>> AvailableProducts();
        Task<bool> IsProductAvailable(string productId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>the active product IDs owned by the user</returns>
        IEnumerable<string> OwnedProductIDs();
        bool OwnsProduct(string productId);
        /// <summary>
        /// Shows the purchase screen and fulfills the purchase. If this method returns normally, the purchase succeeded.
        /// </summary>
        /// <param name="productId">product to purchase</param>
        /// <exception cref="IapException">if the purchase is not completed, for example if the user aborts</exception>
        Task PurchaseDurable(string productId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId">ID of the IAP item</param>
        /// <returns>the details of the IAP item with the given product ID, or null if no such item exists</returns>
        Task<ProductInfo> ProductInfo(string productId);
        /// <summary>
        /// Shows a dialog to the user suggesting she upgrades to the premium version.
        /// 
        /// TODO consider moving this UI code elsewhere.
        /// </summary>
        /// <returns></returns>
        Task SuggestPremium();
    }
}
