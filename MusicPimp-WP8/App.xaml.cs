using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.Resources;
using MockIAPLib;
using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Mle {
    public partial class App : Application {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }
        enum LaunchType {
            None, Home, DeepLink
        }
        private LaunchType launchType = LaunchType.None;
        private bool wasRelaunched = false;
        private bool shouldClearPageStack = false;
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private string
            DeactivateSettingKey = "DeactivateTime",
            LaunchTypeKey = "LaunchType",
            DeepLinkHint = "DeepLink=true",
            HomePageHint = "/MusicFiles.xaml";
        // The page stack is cleared after this duration has passed; if the user opens the app again
        // within this time period, the app goes to the top of the previous page stack; otherwise 
        // the previous page stack is cleared and the main page is shown.
        private TimeSpan PageStackResumeAge = TimeSpan.FromHours(12);


        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App() {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if(Debugger.IsAttached) {
                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            LifeCycleManager.Start();
            SetupMockIAP();
        }
        private void SetupMockIAP() {
#if DEBUG
            MockIAP.Init();

            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "Removes all playback limits", "3", "MusicPimp Premium");

            // Add some more items manually.
            ProductListing p = new ProductListing {
                Name = "MusicPimp Premium",
                ImageUri = new Uri("/Assets/Icons/browser.png", UriKind.Relative),
                ProductId = "org.musicpimp.premium",
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new string[] { "premium" },
                Description = "Removes all playback limits. Does not expire.",
                FormattedPrice = "€1.99",
                Tag = string.Empty
            };
            MockIAP.AddProductListing("org.musicpimp.premium", p);
#endif
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e) {
            RemoveCurrentDeactivationSettings();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e) {
            LifeCycleManager.AppWasActivated = true;
            // If some interval has passed since the app was deactivated,
            // then remember to clear the back stack of pages.
            shouldClearPageStack = CheckDeactivationTimeStamp();
            if(!e.IsApplicationInstancePreserved) {
                RestoreLaunchType();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e) {
            LifeCycleManager.Deactivate();
            SaveCurrentDeactivationSettings();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e) {
            // Ensure that required application state is persisted here.
            RemoveCurrentDeactivationSettings();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {
            if(Debugger.IsAttached) {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            var ex = e.ExceptionObject;
            var isWebSocketLibraryCockup = ex.GetType() == typeof(NullReferenceException) && ex.Source == "System.Net";
            if(isWebSocketLibraryCockup) {
                return;
            }
            if(ex.GetType() == typeof(ObjectDisposedException)) {
                // fucking camera
                return;
            }
            if(ex.InnerException != null) {
                var msg = ex.InnerException.Message;
                if(msg != null && msg.StartsWith("The operation was canceled by the user.") || msg.StartsWith("Specified method is not supported.")) {
                    // fucking camera. thrown if the user twice clicks 'back' from the beam scanner page just as it's opening
                    return;
                }
            }
            //if(ex.Inn)
            if(Debugger.IsAttached) {
                Debug.WriteLine("App crashed: " + e.ExceptionObject.GetType().Name + ": " + e.ExceptionObject.StackTrace);
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication() {
            if(phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();

            MessagingService.Instance.Register(new MessageHandler());
            PageNavigationService.Instance.Register(new PhoneNavigationHandler(RootFrame));
            //RootFrame.Background = App.Current.Resources["MainBackground"] as ImageBrush;

            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigated += RootFrame_Navigated;
            // Monitors deep link launching
            RootFrame.Navigating += RootFrame_Navigating;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }
        /// <summary>
        /// For documentation, see the sample code in
        /// http://code.msdn.microsoft.com/wpapps/Fast-app-resume-backstack-f16baaa6.
        /// 
        /// When resuming the app, this method is first called with NavigationMode.Reset, 
        /// followed by a call with NavigationMode.New. Reset implies that the app is being
        /// resumed. The call to New should apply FAR if the previous call was a Reset, 
        /// therefore, Reset sets variable wasRelaunched to indicate to the next New call 
        /// that FAR should take place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e) {
            var uri = e.Uri.ToString();
            if(launchType == LaunchType.None && e.NavigationMode == NavigationMode.New) {
                // initial launch
                if(IsDeepLink(uri)) {
                    launchType = LaunchType.DeepLink;
                } else if(IsMainPage(uri)) {
                    launchType = LaunchType.Home;
                }
            }
            if(e.NavigationMode == NavigationMode.Reset) {
                wasRelaunched = true;
            } else if(e.NavigationMode == NavigationMode.New && wasRelaunched) {
                // the previous navigation was a Reset
                wasRelaunched = false;
                if(IsDeepLink(uri)) {
                    launchType = LaunchType.DeepLink;
                } else if(IsMainPage(uri)) {
                    if(launchType == LaunchType.DeepLink) {
                        launchType = LaunchType.Home;
                    } else if(!shouldClearPageStack) {
                        // fast application resume: cancels navigation to the page, resume the one from the backstack
                        e.Cancel = true;
                        RootFrame.Navigated -= ClearBackStackAfterReset;
                    }
                }
                shouldClearPageStack = false;
            }
        }
        private bool IsDeepLink(string uri) {
            return uri.Contains(DeepLinkHint);
        }
        private bool IsMainPage(string uri) {
            return uri.Contains(HomePageHint);
        }

        /// <summary>
        /// var eventSequence = 
        ///     if(resume to same page) Reset andThen Refresh
        ///     else Reset andThen New
        /// If the app is resumed to the same page as when it was suspended,
        /// a Refresh event is fired after a Reset. No New event is fired.
        /// In this case, we should not cancel the next New event, because 
        /// that will be the next page the user manually navigates to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootFrame_Navigated(object sender, NavigationEventArgs e) {
            if(e.NavigationMode == NavigationMode.Refresh) {
                wasRelaunched = false;
            }
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
            // Set the root visual to allow the application to render
            if(RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e) {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if(e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e) {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if(e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while(RootFrame.RemoveBackEntry() != null) {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage() {
            try {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            } catch {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if(Debugger.IsAttached) {
                    Debugger.Break();
                }

                throw;
            }
        }

        #region fast app resume helpers

        // Called when the app is deactivating. Saves the time of the deactivation and the 
        // session type of the app instance to isolated storage.
        public void SaveCurrentDeactivationSettings() {
            if(AddOrUpdateValue(DeactivateSettingKey, DateTimeOffset.Now)) {
                settings.Save();
            }
            if(AddOrUpdateValue(LaunchTypeKey, launchType)) {
                settings.Save();
            }
        }
        // Called when the app is launched or closed. Removes all deactivation settings from
        // isolated storage
        public void RemoveCurrentDeactivationSettings() {
            RemoveValue(DeactivateSettingKey);
            RemoveValue(LaunchTypeKey);
            settings.Save();
        }

        // Helper method to determine if the interval since the app was deactivated is
        // greater than 30 seconds
        bool CheckDeactivationTimeStamp() {
            if(settings.Contains(DeactivateSettingKey)) {
                DateTimeOffset lastDeactivated = (DateTimeOffset)settings[DeactivateSettingKey];
                var currentDuration = DateTimeOffset.Now.Subtract(lastDeactivated);

                return TimeSpan.FromSeconds(currentDuration.TotalSeconds) > PageStackResumeAge;
            } else {
                return true;
            }
        }
        private void RestoreLaunchType() {
            if(settings.Contains(LaunchTypeKey)) {
                launchType = (LaunchType)settings[LaunchTypeKey];
            }
        }
        private bool AddOrUpdateValue(string key, Object value) {
            bool valueChanged = false;

            if(settings.Contains(key)) {
                // If the value differs from the one stored
                if(settings[key] != value) {
                    // Store the new value
                    settings[key] = value;
                    valueChanged = true;
                }
            } else {
                settings.Add(key, value);
                valueChanged = true;
            }
            return valueChanged;
        }
        public void RemoveValue(string Key) {
            // If the key exists
            if(settings.Contains(Key)) {
                settings.Remove(Key);
            }
        }
        #endregion
    }
}