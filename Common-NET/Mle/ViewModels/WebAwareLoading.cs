using Mle.Exceptions;
using Mle.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mle.ViewModels {
    abstract public class WebAwareLoading : MessagingViewModel {
        private static readonly string NetworkErrorMessage = "A network error occurred. Please check your settings.";
        private static readonly string GeneralErrorMessage = "A general error occurred.";

        protected Task OnUiThread(Action uiCode) {
            return UiService.Instance.OnUiThreadAsync(uiCode);
        }

        protected async Task WebAware(Func<Task> code) {
            await WebAware<int>(
                async progressState => await OnUiThread(() => IsLoading = progressState),
                feedback => FeedbackMessage = feedback,
                async () => { await code(); return 42; });
        }
        protected async Task<T> WebAware<T>(Func<Task<T>> code) {
            return await WebAware(
                async progressState => await OnUiThread(() => IsLoading = progressState),
                feedback => FeedbackMessage = feedback,
                code);
        }

        protected async Task<T> WebAware<T>(
            Action<bool> progressUpdateHandler,
            Action<string> exceptionMessageHandler,
            Func<Task<T>> code) {
            progressUpdateHandler(true);
            try {
                return await WebAwareBase(exceptionMessageHandler, code);
            } finally {
                progressUpdateHandler(false);
            }
        }

        protected async Task<T> WebAwareBase<T>(Action<string> onFeedback, Func<Task<T>> code) {
            onFeedback(null);
            try {
                return await code();
            } catch(WebException) {
                onFeedback(NetworkErrorMessage);
            } catch(NotFoundException) {
                onFeedback("Unable to connect.");
            } catch(UnauthorizedException) {
                onFeedback("Check your credentials.");
            } catch(ServerResponseException sre) {
                var errorMessage = sre.Message;
                if(errorMessage == String.Empty) {
                    onFeedback("Server error.");
                } else {
                    try {
                        var json = JObject.Parse(errorMessage);
                        var reason = json["reason"];
                        var msg = reason != null ? reason.Value<string>() : errorMessage;
                        onFeedback(msg);
                    } catch(JsonReaderException) {
                        onFeedback(errorMessage);
                    }

                }
            } catch(HttpRequestException) {
                onFeedback(NetworkErrorMessage);
            } catch(TimeoutException toe) {
                onFeedback("Timed out. " + toe.Message);
            } catch(FriendlyException fe) {
                onFeedback(fe.Message);
            } catch(Exception e) {
                onFeedback(GeneralErrorMessage + " " + e.Message);
            }
            return default(T);
        }
    }
}
