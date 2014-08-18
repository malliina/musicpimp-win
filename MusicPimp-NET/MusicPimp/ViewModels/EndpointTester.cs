using Mle.Exceptions;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Network.Http;
using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.Util;
using Mle.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class EndpointTester : MessagingViewModel {
        public virtual void OnFeedback(string message) {
            FeedbackMessage = message;
        }

        public ICommand TestEndpoint { get; protected set; }

        public EndpointTester() {
            TestEndpoint = new AsyncDelegateCommand<MusicEndpoint>(Test);
        }

        public async Task Test(MusicEndpoint endpoint) {
            string userFeedback = null;
            RemoteBase session = null;
            try {
                switch(endpoint.EndpointType) {
                    case EndpointTypes.Local:
                        break;
                    case EndpointTypes.MusicPimp:
                        session = ProviderService.Instance.NewPimpSession(endpoint);
                        break;
                    case EndpointTypes.Subsonic:
                        session = new SubsonicSession(endpoint);
                        break;
                }
                if(session != null) {
                    try {
                        FeedbackMessage = String.Empty;
                        IsLoading = true;
                        await session.TestConnectivity();
                        userFeedback = "Connection successfully established.";
                    } finally {
                        IsLoading = false;
                    }
                }
            } catch(UnauthorizedException) {
                userFeedback = "Check your credentials.";
            } catch(ConnectivityException) {
                userFeedback = GetFeedback(endpoint, session);
            } catch(NotFoundException) {
                userFeedback = GetFeedback(endpoint, session);
            } catch(PimpException pe) {
                userFeedback = pe.Message;
            } catch(ServerResponseException sre) {
                userFeedback = sre.Message;
            } catch(HttpRequestException) {
                userFeedback = GetFeedback(endpoint, session);
            } catch(Exception) {
                userFeedback = "Something went wrong. Please check that all the fields are filled in properly.";
            }

            if(userFeedback != null) {
                OnFeedback(userFeedback);
            }
        }
        private string GetFeedback(MusicEndpoint e, RemoteBase s) {
            string userFeedback = s != null ? "Unable to connect to " + s.BaseUri : "Unable to connect";
            var extraInfo = "";
            if(e.Protocol == Protocols.https) {
                extraInfo = ". Perhaps the certificate validation failed? Untrusted SSL certificates are not accepted.";
            }
            userFeedback += extraInfo;
            return userFeedback;
        }
    }
}
