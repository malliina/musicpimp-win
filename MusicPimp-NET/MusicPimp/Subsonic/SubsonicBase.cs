using Mle.Exceptions;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Newtonsoft.Json;
using System;

namespace Mle.MusicPimp.Subsonic {
    abstract public class SubsonicBase : SessionBase {

        private const string
            GenericSubsonicErrorMessage = "A Subsonic error occurred.",
            EmptyResponseErrorMessage = "A Subsonic network error occurred. An empty response was unexpectedly received.",
            CheckCredentialsMessage = "Check your credentials.",
            InvalidResponseMessage = "An invalid Subsonic response was received.",
            Ok = "ok";

        private string uriBase;
        public SubsonicBase(MusicEndpoint settings)
            : base(settings, HttpUtil.Json) {
            uriBase = BaseUri + "/rest/{0}.view?v=1.8.0&c=pimp&f=json";
        }
        protected string UriTemplate() {
            // The credentials are needed in the query parameters because
            // the MediaElement control does not provide a way to set 
            // HTTP Headers when supplied with a Uri as media Source
            return uriBase + "&u=" + Username + "&p=" + Password;
        }
        protected Uri UriFor(string resource, int id) {
            return UriFor(resource, "&id=" + id);
        }
        protected string UriString(string resource) {
            return String.Format(UriTemplate(), resource);
        }
        protected Uri UriFor(string resource, string parameters) {
            return ToUri(UriString(resource) + parameters);
        }
        public Uri DownloadUriFor(int songId) {
            return UriFor("download", songId);
        }
        public Uri StreamUriFor(int songId) {
            return UriFor("stream", songId);
        }
        protected void validateResponse<T>(T response) where T : SubsonicResponse {
            if(response.status != Ok) {
                var errorMessage = GenericSubsonicErrorMessage;
                var errorOpt = response.error;
                if(errorOpt != null) {
                    errorMessage = errorOpt.message;
                }
                throw new ServerResponseException(InvalidResponseMessage + " The error message is: " + errorMessage);
            }
        }
        protected T parseResponse<T, U>(string json)
            where U : ISubsonicResponseContainer<T>
            where T : SubsonicResponse {
            if(json == String.Empty) {
                throw new ServerResponseException(EmptyResponseErrorMessage);
            }
            if(json.StartsWith("<html>")) {
                // Document this masterpiece of code.
                // Incorrect creds always give an HTML response? Ok then.
                throw new ServerResponseException(CheckCredentialsMessage);
            }
            T response = deserialize<T, U>(json);
            validateResponse(response);
            return response;
        }
        protected T deserialize<T, U>(string json)
            where U : ISubsonicResponseContainer<T>
            where T : SubsonicResponse {
            // TODO: only deserialize responses with "status": "ok"
            return JsonConvert.DeserializeObject<U>(json).subsonicResponse;
        }
    }
}
