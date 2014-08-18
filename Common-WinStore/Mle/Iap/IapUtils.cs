using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;

namespace Mle.Iap {
    /// <summary>
    /// When testing IAP, use CurrentAppSimulator instead of CurrentApp, and run the following in App.xaml.cs upon app startup:
    /// 
    /// var proxyFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WindowsStoreProxy.xml", UriKind.Absolute));
    //  await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
    /// </summary>
    public abstract class IapUtils : BaseIapUtils {
        public event Action<string> ProductPurchased;
        public abstract void EnableLicense(string productId);

        private IReadOnlyDictionary<string, ProductLicense> ProductLicenses {
            get { return CurrentApp.LicenseInformation.ProductLicenses; }
            //get { return CurrentAppSimulator.LicenseInformation.ProductLicenses; }
        }
        private IAsyncOperation<ListingInformation> LoadListings() {
            return CurrentApp.LoadListingInformationAsync();
            //return CurrentAppSimulator.LoadListingInformationAsync();
        }
        private IAsyncOperation<string> RequestProductPurchase(string productId) {
            return CurrentApp.RequestProductPurchaseAsync(productId, includeReceipt: false);
            //return CurrentAppSimulator.RequestProductPurchaseAsync(productId, includeReceipt: false);
        }
        public override async Task<IEnumerable<ProductInfo>> AvailableProducts() {
            var listing = await LoadListings();
            return listing.ProductListings.Values.Select(ToProductInfo).ToList();
        }
        public override IEnumerable<string> OwnedProductIDs() {
            return ProductLicenses.Values
                .Where(license => license.IsActive)
                .Select(l => l.ProductId).ToList();
        }
        public override async Task PurchaseDurable(string productId) {
            try {
                await RequestProductPurchase(productId);
                Fulfill(productId);
            } catch(Exception e) {
                throw new IapException("The purchase was not completed. Try again later!", e);
            }
        }
        private void Fulfill(string expectedProductId) {
            var licenses = ProductLicenses;
            var licenseCount = licenses.Count;
            var licenseKeys = licenses.Keys;

            if(licenses.ContainsKey(expectedProductId)) {
                var license = licenses[expectedProductId];
                if(license.IsActive) {
                    EnableLicense(expectedProductId);
                    OnProductPurchased(expectedProductId);
                    // the following is only for consumables
                    //CurrentApp.ReportProductFulfillment(expectedProductId);
                } else {
                    throw new IapException("Inactive license. Product ID: " + expectedProductId + ".");
                }
            } else {
                throw new IapException("User does not own product ID: " + expectedProductId + ".");
            }
        }
        private void OnProductPurchased(string productId) {
            if(ProductPurchased != null) {
                ProductPurchased(productId);
            }
        }
        private ProductInfo ToProductInfo(ProductListing listing) {
            return new ProductInfo(listing.Name, listing.ProductId, listing.FormattedPrice);
        }
    }
}
