using System;
using System.Collections.Generic;
using System.Linq;

namespace Mle.Network {
    public class NetworkUtils {
        public static bool IsNumericalIP(string host) {
            return host.ToCharArray().All(c => Char.IsDigit(c) || c == '.') && host.Length >= 7 && host.Length <= 15;
        }
        public static List<string> AdjacentIPs(string sampleIP, int radius = 10) {
            var ipInfo = IpSplit(sampleIP);
            var rangeStart = Math.Max(1, ipInfo.LastOctet - radius);
            var rangeStop = Math.Min(254, ipInfo.LastOctet + radius);
            // the second param to Range is count, + 1 is because the IP of the device is discarded
            return Enumerable.Range(rangeStart, rangeStop - rangeStart + 1)
                .Select(octet => ipInfo.Network + "." + octet)
                .Where(ip => ip != sampleIP)
                .ToList();
        }
        public static IpInfo IpSplit(string ip) {
            var lastDotIndex = ip.LastIndexOf('.');
            var nw = ip.Substring(0, lastDotIndex);
            var lastOctet = Convert.ToInt32(ip.Substring(lastDotIndex + 1, ip.Length - (lastDotIndex + 1)));
            return new IpInfo(nw, lastOctet);
        }
        public class IpInfo {
            public string Network { get; private set; }
            public int LastOctet { get; private set; }
            public IpInfo(string nw, int lastOctet) {
                Network = nw;
                LastOctet = lastOctet;
            }
        }
    }
}
