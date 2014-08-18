using Mle.Collections;
using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Network {
    public class EndpointScanner {
        private static EndpointScanner instance = null;
        public static EndpointScanner Instance {
            get {
                if(instance == null)
                    instance = new EndpointScanner();
                return instance;
            }
        }
        private ProviderService Provider { get { return ProviderService.Instance; } }
        private EndpointsData EndpointsInfo { get { return Provider.EndpointsData; } }
        public Task<MusicEndpoint> SearchAnyServer(IEnumerable<string> baseIPs) {
            //Debug.WriteLine("Searching for servers, given the following initial IPs: " + baseIPs.MkString(", "));
            var scanRange = baseIPs.SelectMany(ScanRange).ToList();
            //Debug.WriteLine("Scanning " + scanRange.Count() + " IP addresses: " + scanRange.MkString(", "));
            return TestIPs<MusicEndpoint>(scanRange, ip => token => PingNoAuth(ip, token));
        }
        public async Task SyncActiveEndpoints() {
            var provider = ProviderService.Instance;
            var libraryEndpoint = provider.LibraryManager.ActiveEndpoint;
            var playerEndpoint = provider.PlayerManager.ActiveEndpoint;
            await SyncIfUnreachable(libraryEndpoint);
            if(playerEndpoint.Name != libraryEndpoint.Name) {
                await SyncIfUnreachable(playerEndpoint);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>true if the endpoint was modified, false otherwise</returns>
        public async Task<bool> SyncIfUnreachable(MusicEndpoint endpoint) {
            if(await IsUnreachable(endpoint)) {
                return await SyncEndpoint(endpoint);
            }
            return false;
        }
        public async Task<bool> SyncEndpoint(MusicEndpoint endpoint) {
            try {
                if(NetworkUtils.IsNumericalIP(endpoint.Server)) {
                    var working = await SearchWorking(endpoint);
                    // submits changes
                    var current = EndpointsInfo.Endpoints.FirstOrDefault(e => e.Name == working.Name);
                    if(current != null) {
                        current.Server = working.Server;
                        await EndpointsInfo.SaveChanges(current);
                        return true;
                    }
                }
            } catch(Exception) {
            }
            return false;
        }
        private async Task<bool> IsUnreachable(MusicEndpoint e) {
            var isUnreachable = false;
            if(e.EndpointType == EndpointTypes.MusicPimp) {
                try {
                    var s = Provider.NewPimpSession(e);
                    await s.PingAuth();
                } catch(NotFoundException) {
                    // HttpClient returns a 404 status code even if the server is unreachable,
                    // which seems wrong, but that's why we also need to catch NotFoundExceptions.
                    isUnreachable = true;
                } catch(ConnectivityException) {
                    isUnreachable = true;
                } catch(Exception) {

                }
            }
            return isUnreachable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notWorking"></param>
        /// <returns>a working endpoint if any</returns>
        private async Task<MusicEndpoint> SearchWorking(MusicEndpoint notWorking) {
            if(NetworkUtils.IsNumericalIP(notWorking.Server)) {
                var scanRange = ScanRange(notWorking.Server);
                return await TestIPs<MusicEndpoint>(scanRange, addr => token => AuthTest(addr, notWorking));
            } else {
                throw new Exception("Not a numerical IP: " + notWorking.Server);
            }
        }
        private Task<T> TestIPs<T>(IEnumerable<string> ips, Func<string, Func<CancellationToken, Task<T>>> test) {
            var tests = ips.Select(test).ToList();
            return AsyncTasks.FirstSuccessfulWithin<T>(tests, TimeSpan.FromSeconds(8));
        }
        private List<string> ScanRange(string ip) {
            var adjacent = NetworkUtils.AdjacentIPs(ip);
            //Debug.WriteLine("Got IPs: " + adjacent.MkString(", "));
            var middle = adjacent.Count / 2;
            var left = adjacent.Take(middle);
            var right = adjacent.Skip(middle).ToList();
            return Lists.Interleave<string>(left.Reverse().ToList(), right);
        }
        private Task<MusicEndpoint> PingNoAuth(string ip, CancellationToken token) {
            return BasicTest(ip, addr => new MusicEndpoint(addr, "unused"), async session => await session.Ping(token));
        }
        private Task<MusicEndpoint> AuthTest(string ip, MusicEndpoint notWorking) {
            return BasicTest(
                ip,
                addr => {
                    var e = new MusicEndpoint(notWorking);
                    e.Server = addr;
                    return e;
                },
                session => session.PingAuth());
        }
        private async Task<MusicEndpoint> BasicTest(string ip, Func<string, MusicEndpoint> endBuilder, Func<PimpSessionBase, Task> test) {
            var endpoint = endBuilder(ip);
            var session = Provider.NewPimpSession(endpoint);
            await test(session);
            return endpoint;
        }
    }
}
