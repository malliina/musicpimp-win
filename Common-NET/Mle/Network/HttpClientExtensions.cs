using Mle.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.Network {
    /// <summary>
    /// TODO fix: If httpclient extension methods are used, a ServerResponseException may be 
    /// thrown for an error, but if the inbuilt methods are used, the same failure may throw 
    /// an HttpRequestException instead.
    /// </summary>
    public static class HttpClientExtensions {
        /// <summary>
        /// Throws an exception if the status code does not indicate success.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>the response content if the response had a successful status code</returns>
        /// <exception cref="BadRequestException">if the server responds with an HTTP bad request</exception>
        /// <exception cref="NotFoundException">if the server is unreachable or there's actually a 404</exception>
        /// <exception cref="ConnectivityException"if the server is unreachable</exception>
        /// <exception cref="UnauthorizedException">if the server responds with HTTP Unauthorized</exception>
        /// <exception cref="IntenralServerError">HTTP status code 500</exception>
        /// <exception cref="ServerResponseException">for other erroneous status codes</exception>
        public static async Task<string> TakeContent(this HttpResponseMessage response) {
            if(response.IsSuccessStatusCode) {
                return await ReadContent(response);
            } else {
                var statusCode = response.StatusCode;
                switch(statusCode) {
                    case HttpStatusCode.BadRequest:
                        // The server may respond with feedback in the content of the bad request response.
                        var content = await ReadContent(response);
                        throw new BadRequestException("Bad request", content);
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException();
                    case HttpStatusCode.NotFound:
                        // HttpClient returns NotFound even if the server does not exist 
                        // So this may or may not also mean a "ConnectivityException"
                        throw new NotFoundException();
                    case HttpStatusCode.InternalServerError:
                        throw new InternalServerErrorException();
                    default:
                        throw new ServerResponseException("Unexpected status code in response: " + statusCode);
                }
            }
        }
        public static async Task DownloadToStream(this HttpClient client, Uri remoteUri, Stream stream) {
            var response = await client.GetAsync(remoteUri);
            response.EnsureSuccessStatusCode();
            await response.Content.CopyToAsync(stream);
        }
        public static async Task<T> GetJson<T>(this HttpClient client, string resource, CancellationToken cancellationToken) {
            var responseContent = await client.GetString(resource, cancellationToken);
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
        public static async Task<T> GetJson<T>(this HttpClient client, string resource) {
            var responseContent = await client.GetString(resource);
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
        public static async Task<string> GetString(this HttpClient client, string resource, CancellationToken cancellationToken) {
            var response = await client.GetAsync(resource, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await response.TakeContent();
        }
        public static async Task<string> GetString(this HttpClient client, string resource) {
            var response = await client.GetAsync(resource, HttpCompletionOption.ResponseHeadersRead);
            return await response.TakeContent();
        }
        public static async Task<string> GetString(this HttpClient client, Uri resource) {
            var response = await client.GetAsync(resource, HttpCompletionOption.ResponseHeadersRead);
            return await response.TakeContent();
        }
        public static async Task<string> PostJson(this HttpClient client, string json, string resource) {
            var content = new StringContent(json, Encoding.UTF8, HttpUtil.Json);
            var response = await client.PostAsync(resource, content);
            return await response.TakeContent();
        }
        /// <summary>
        /// Reads the response content on another thread.
        /// 
        /// This hack prevents NotSupportedException on WP7 with error message:
        /// "Read is not supported on the main thread when buffering is disabled".
        /// 
        /// Don't remove TaskEx and think await'll just work, test WP7 first.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task<string> ReadContent(HttpResponseMessage response) {
            return await TaskEx.Run(() => response.Content.ReadAsStringAsync());
        }
        private static async Task<T> ReadContentAsJson<T>(HttpResponseMessage response) {
            var content = await ReadContent(response);
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
