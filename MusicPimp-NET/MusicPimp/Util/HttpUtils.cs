using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Mle.MusicPimp.Util {
    public class HttpUtils {
        public static HttpClient BuildClient(MusicEndpoint e) {
            var handler = new HttpClientHandler();
            if(handler.SupportsAutomaticDecompression) {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            } else {
                handler.AutomaticDecompression = DecompressionMethods.None;
            }
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(e.Protocol + "://" + e.Server + ":" + e.Port);
            var headers = client.DefaultRequestHeaders;
            headers.Authorization = HttpUtil.BasicAuthHeaderValue(e.Username, e.Password);
            return client;
        }
        public static HttpClient BuildJsonClient(MusicEndpoint e) {
            var client = BuildClient(e);
            client.DefaultRequestHeaders.Accept.ParseAdd(HttpUtil.Json);
            return client;
        }
    }
}
