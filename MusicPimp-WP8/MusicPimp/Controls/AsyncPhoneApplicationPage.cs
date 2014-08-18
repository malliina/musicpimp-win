using Mle.MusicPimp.Exceptions;
using Mle.Pages;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mle.MusicPimp.Controls {
    public partial class AsyncPhoneApplicationPage : BasePhonePage {
        private static readonly string NetworkErrorMessage = "A network error occurred. Please check your settings.";
        private static readonly string GeneralErrorMessage = "A general error occurred.";
        private static readonly string PageFolderPath = "/MusicPimp/Xaml/";

        protected void GoToPage(string pagePath, string pageParams = "") {
            var uriString = Uri.EscapeUriString("/" + pagePath + ".xaml" + pageParams);
            var uri = new Uri(uriString, UriKind.Relative);
            NavigationService.Navigate(uri);
        }
        protected void GoToPageName(string pageName, string pageParams = "") {
            GoToPage(PageFolderPath + pageName, pageParams);
        }
        protected void GoToProjectPage(string projectName, string pageName, string pageParams = "") {
            GoToPage(projectName + ";component" + PageFolderPath + pageName, pageParams);
        }
        #region lambda support
        protected async Task WithProgressPanel(UIElement progressElement, Func<Task> code) {
            progressElement.Visibility = Visibility.Visible;
            try {
                await code();
            } finally {
                progressElement.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Makes the supplied progress element visible as long as the given code is executing.
        /// </summary>
        /// <param name="progressElement">progress panel or similar</param>
        /// <param name="code">web code to run</param>
        /// <returns></returns>
        protected async Task ProgressAware(UIElement progressElement, Func<Task> code) {
            await WithProgressPanel(progressElement, code);
        }
        /// <summary>
        /// Makes the loading element visible as long as the supplied code is executing.
        /// Hides the feedback component initially, but upon a WebException, makes it visible.
        /// </summary>
        /// <param name="loadingElement">UI element to show while code is loading</param>
        /// <param name="feedbackComponent">UI element to show if a WebException occurs while executing code</param>
        /// <param name="code">code to execute</param>
        /// <returns></returns>
        protected async Task WebAware(UIElement loadingElement, TextBlock feedbackComponent, Func<Task> code) {
            await WebAwareBase(WithProgressPanel(loadingElement, code), feedbackComponent);
        }
        /// <summary>
        /// Variation of WebAware(UIElement,TextBlock,Attempt) using delegates instead.
        /// </summary>
        /// <param name="visibilitySetter"></param>
        /// <param name="feedbackComponent"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        //protected async Task WebAware2(Action<bool> visibilitySetter, TextBlock feedbackComponent, Func<Task> code) {
        //    await WebAwareBase(WithProgressPanel(visibilitySetter, code), feedbackComponent);
        //}
        protected async Task WebAwareBase(Task t, TextBlock feedbackComponent) {
            feedbackComponent.Visibility = Visibility.Collapsed;
            try {
                await t;
            } catch(WebException) {
                feedbackComponent.Text = NetworkErrorMessage;
                feedbackComponent.Visibility = Visibility.Visible;
            } catch(PimpException e) {
                feedbackComponent.Text = e.Message;
                feedbackComponent.Visibility = Visibility.Visible;
            } catch(Exception) {
                feedbackComponent.Text = GeneralErrorMessage;
                feedbackComponent.Visibility = Visibility.Visible;
            }
        }

        protected async Task ExceptionAware(Func<Task> code, Action<string> onExceptionMessage) {
            try {
                await code();
            } catch(WebException) {
                onExceptionMessage(NetworkErrorMessage);
            } catch(HttpRequestException e) {
                onExceptionMessage(e.Message);
            } catch(PimpException e) {
                onExceptionMessage(e.Message);
            } catch(Exception) {
                onExceptionMessage(GeneralErrorMessage);
            }
        }
        [Obsolete]
        protected async Task ExceptionAware(Func<Task> code) {
            await ExceptionAware(code, msg => {
                MessageBox.Show(msg);
                return;
            });
        }
        #endregion
    }
}
