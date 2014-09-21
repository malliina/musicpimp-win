using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

namespace Mle.Iap {
    public abstract class IapUtils : BaseIapUtils {
        public abstract void EnableLicense(string productId);
        public override IEnumerable<string> OwnedProductIDs() {
            return CurrentApp.LicenseInformation.ProductLicenses.Values
                .Where(license => license.IsActive)
                .Select(l => l.ProductId).ToList();
        }
        public override async Task PurchaseDurable(string productId) {
            try {
                await CurrentApp.RequestProductPurchaseAsync(productId, includeReceipt: false);
                Fulfill(productId);
            } catch(Exception e) {
                throw new IapException("The purchase was not completed. Try again later!", e);
            }
        }
        public override async Task<IEnumerable<ProductInfo>> AvailableProducts() {
            var products = await GetProducts();
            return products.Select(ToProductInfo).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the IAP products available for purchase</returns>
        public async Task<IEnumerable<ProductListing>> GetProducts() {
            var listing = await CurrentApp.LoadListingInformationAsync();
            return listing.ProductListings.Values;
        }
        private void Fulfill(string expectedProductId) {
            var licenses = CurrentApp.LicenseInformation.ProductLicenses;
            if(licenses.ContainsKey(expectedProductId)) {
                var license = licenses[expectedProductId];
                if(license.IsActive) {
                    EnableLicense(expectedProductId);
                    //CurrentApp.ReportProductFulfillment(expectedProductId);
                } else {
                    throw new IapException("Inactive license. Product ID: " + expectedProductId + ".");
                }
            } else {
                throw new IapException("User does not own product ID: " + expectedProductId + ".");
            }
        }
        private ProductInfo ToProductInfo(ProductListing listing) {
            return new ProductInfo(listing.Name, listing.ProductId, listing.FormattedPrice);
        }
    }
}
