using Callisto.Controls.SettingsManagement;
using Mle.Concurrent;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.Xaml;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Mle {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application {

        private static readonly string DEFAULT_NAVIGATION_PARAMETER = String.Empty;
        // to actually change the first page, edit RootPage.xaml.cs
        //private static Type startPage = typeof(RootPage);
        private static Type startPage = typeof(RootPage);
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            // Consider calling this from elsewhere, for example from OnLaunched 
            InitPimp();
        }
        private async void InitPimp() {
            await LifeCycleManager.Init();
            Initializer.Init();
            await LifeCycleManager.Start();
        }
        private void InitSettingsCharm() {
            var appSettings = AppSettings.Current;
            appSettings.AddCommand<Settings>("Options");
            appSettings.AddCommand<PrivacyPolicy>("Privacy policy");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if(rootFrame == null) {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // defined in AppResDict.xaml
                rootFrame.Style = Resources["MediaElementRootFrameStyle"] as Style;
                LifeCycleManager.SuspensionManagerRegister(rootFrame, "AppFrame");

                if(args.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    await LifeCycleManager.SuspensionManagerRestore();
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if(rootFrame.Content == null) {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if(!rootFrame.Navigate(startPage, args.Arguments)) {
                    throw new Exception("Failed to create initial page");
                }
            }
            if(args.Kind == ActivationKind.Launch) {
                InitSettingsCharm();
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            await LifeCycleManager.SuspensionManagerSave();
            deferral.Complete();
        }
    }
}
