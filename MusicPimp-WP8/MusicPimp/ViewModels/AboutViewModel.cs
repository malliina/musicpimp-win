using Microsoft.Phone.Tasks;
using Mle.MusicPimp.Iap;
using Mle.Xaml.Commands;
using System;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class AboutViewModel {
        private static AboutViewModel instance;
        public static AboutViewModel Instance {
            get {
                if(instance == null) {
                    instance = new AboutViewModel();
                }
                return instance;
            }
        }
        public bool IsIapEnabled { get { return UsageController.Instance.IsIapEnabled; } }
        public ICommand OpenEmail { get; private set; }
        public ICommand OpenWebsite { get; private set; }
        public ICommand OpenMarketPlace { get; private set; }
        public string NameAndVersion { get { return "MusicPimp 2.6.4"; } }

        public AboutViewModel() {
            OpenEmail = new DelegateCommand<string>(email => {
                var emailTask = new EmailComposeTask() {
                    Subject = "MusicPimp Feedback",
                    Body = "Great app! I'm using " + NameAndVersion + " for Windows Phone.",
                    To = email
                };
                emailTask.Show();
            });
            OpenWebsite = new DelegateCommand<string>(url => {
                var browserTask = new WebBrowserTask();
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                browserTask.Uri = uri;
                browserTask.Show();
            });
            OpenMarketPlace = new UnitCommand(() => {
                var reviewTask = new MarketplaceReviewTask();
                reviewTask.Show();
                //var marketplaceDetailTask = new MarketplaceDetailTask();
                //marketplaceDetailTask.ContentIdentifier = "d31b505b-ac9f-4d93-8812-6b649734a5a6";
                //marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
                //marketplaceDetailTask.Show();
            });
        }
    }
}
