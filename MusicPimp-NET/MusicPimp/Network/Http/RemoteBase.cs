using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.ViewModels;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Network.Http {
    abstract public class RemoteBase {
        public MusicEndpoint Endpoint {get; private set;}
        public string EndpointName { get; private set; }
        public string Server { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string CloudServerID { get; private set; }
        public string BaseUri { get; private set; }
        public string Describe { get; private set; }

        public RemoteBase(MusicEndpoint settings) {
            Endpoint = settings;
            EndpointName = settings.Name;
            Server = settings.Server;
            Port = settings.Port;
            Username = settings.Username;
            Password = settings.Password;
            CloudServerID = settings.CloudServerID;
            BaseUri = settings.Uri().OriginalString;
            Describe = Server + ":" + Port;
        }
        public abstract Task TestConnectivity();
        /// <summary>
        /// Note: If the supplied task "faults" as opposed to "completes", the timeout is always thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public async Task<T> WebTask<T>(Task<T> task, TimeSpan timeout) {
            try {
                var firstTask = await TaskEx.WhenAny(task, TaskEx.Delay(timeout));
                if(firstTask == task) {
                    var maybeException = task.Exception;
                    if(maybeException != null) {
                        var maybeInner = maybeException.InnerException;
                        if(maybeInner != null) {
                            throw maybeInner;
                        } else {
                            throw maybeException;
                        }
                    }
                    return task.Result;
                } else {
                    throw new TimeoutPimpException("Operation timed out after " + timeout + ".");
                }
            } catch(WebException) {
                throw new ConnectivityException("Unable to connect.");
            } catch(AggregateException ae) {
                throw new ConnectivityException(ae.InnerException.Message);
            }
        }
    }
}
