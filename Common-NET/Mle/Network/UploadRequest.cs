using Mle.IO.Local;
using Mle.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Network {
    public class UploadFile {
        public UploadFile() {
            ContentType = HttpUtil.OctetStream;
        }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
    }
    /// <summary>
    /// A file uploader based on HttpWebRequest.
    /// 
    /// RestSharp is not available for Windows Store and HttpClient does not work.
    /// 
    /// Adapted from http://www.bratched.com/en/home/dotnet/69-uploading-multiple-files-with-c.html
    /// 
    /// </summary>
    public abstract class UploadRequest {
        public abstract IPathHelper Paths { get; }
        public abstract Task WithFileRead(string filePath, Func<Stream, Task> op);

        public string BaseUri { get; private set; }
        private string AuthString;
        protected List<string> Files { get; private set; }
        protected Dictionary<string, string> KeyValues { get; private set; }
        protected IDictionary<string, string> HeaderKeyValues { get; private set; }
        private Encoding Enc;

        public UploadRequest(string requestUri, string username, string password) {
            BaseUri = requestUri;
            AuthString = HttpUtil.BasicAuthHeader(username, password);
            Files = new List<string>();
            KeyValues = new Dictionary<string, string>();
            HeaderKeyValues = new Dictionary<string, string>();
            Enc = Encoding.UTF8;
        }

        public void AddFile(string localStorageFilePath) {
            Files.Add(localStorageFilePath);
        }
        public void AddKeyValue(string key, string value) {
            KeyValues[key] = value;
        }
        public void AddHeader(string key, string value) {
            HeaderKeyValues[key] = value;
        }
        private async Task AddFile(Stream requestStream, string boundary, string filePath) {
            await WithFileRead(filePath, async stream => {
                var uploadFile = new UploadFile() {
                    Name = "file",
                    FileName = Paths.FileNameOf(filePath),
                    Stream = stream
                };
                await AddFile(requestStream, boundary, uploadFile);
            });
        }
        private async Task AddFile(Stream requestStream, string boundary, UploadFile uploadFile) {
            var buffer = Enc.GetBytes(boundary + Environment.NewLine);
            Write(requestStream, buffer);
            buffer = Enc.GetBytes(string.Format(HttpUtil.ContentDisposition + ": form-data; name=\"{0}\"; filename=\"{1}\"{2}", uploadFile.Name, uploadFile.FileName, Environment.NewLine));
            Write(requestStream, buffer);
            buffer = Enc.GetBytes(string.Format(HttpUtil.ContentType + ": {0}{1}{1}", uploadFile.ContentType, Environment.NewLine));
            Write(requestStream, buffer);
            await uploadFile.Stream.CopyToAsync(requestStream);
            buffer = Enc.GetBytes(Environment.NewLine);
            Write(requestStream, buffer);
        }
        private void AddKeyValue(Stream requestStream, string boundary, KeyValuePair<string, string> kv) {
            var buffer = Enc.GetBytes(boundary + Environment.NewLine);
            Write(requestStream, buffer);
            buffer = Enc.GetBytes(string.Format(HttpUtil.ContentDisposition + ": form-data; name=\"{0}\"{1}{1}", kv.Key, Environment.NewLine));
            Write(requestStream, buffer);
            buffer = Enc.GetBytes(kv.Value + Environment.NewLine);
            Write(requestStream, buffer);
        }
        public async Task<string> UploadAsync(string resource) {
            var req = WebRequest.CreateHttp(BaseUri + resource);
            req.Method = "POST";
            req.Headers[HttpUtil.Authorization] = AuthString;
            foreach(var kv in HeaderKeyValues) {
                req.Headers[kv.Key] = kv.Value;
            }
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using(var requestStream = await req.GetRequestStreamAsync()) {
                // adds content to stream
                foreach(var keyValue in KeyValues) {
                    AddKeyValue(requestStream, boundary, keyValue);
                }
                foreach(var filePath in Files) {
                    await AddFile(requestStream, boundary, filePath);
                }
                // adds footer
                var boundaryBuffer = Enc.GetBytes(boundary + "--");
                Write(requestStream, boundaryBuffer);
            };
            // makes request
            using(var response = await req.GetResponseAsync())
            using(var responseStream = response.GetResponseStream())
            using(var memStream = new MemoryStream()) {
                var streamReader = new StreamReader(responseStream);
                return await streamReader.ReadToEndAsync();
            }
        }
        private void Write(Stream to, Byte[] data) {
            to.Write(data, 0, data.Length);
        }
    }
}
