using Mle.Messaging;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mle.MusicPimp.Xaml {
    /// <summary>
    /// A root page that hosts other pages. This way the top appbar code is shared between all hosted pages.
    /// </summary>
    public sealed partial class RootPage : Page {
        
     
        private static Type GetStartPage() {
            var pageEnum = StartSettings.Instance.StartPage;
            switch (pageEnum) {
                case Pages.Home:
                    return typeof(SplashPage);
                case Pages.Library:
                    return typeof(MusicItems);
                case Pages.Player:
                    return typeof(Player);
                default:
                    return typeof(SplashPage);
            }
        }
        private Type firstPage = GetStartPage();

        public RootPage() {
            this.InitializeComponent();
            DataContext = RootPageViewModel.Instance;
            PageNavigationService.Instance.Register(new NavigationHandler(ContentFrame));
            Loaded += async (s, e) => await LifeCycleManager.InitOnLoaded();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached. The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            await LifeCycleManager.InitOnNavigated();
            ContentFrame.Navigate(firstPage);
        }

        private void ItemClicked(object sender, ItemClickEventArgs e) {
            var item = (TitledImageItem)e.ClickedItem;
            item.OnClicked();
        }
    }
}
