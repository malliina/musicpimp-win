using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Mle.Network {
    public class NetworkDevice {
        private static NetworkDevice instance = null;
        public static NetworkDevice Instance {
            get {
                if(instance == null)
                    instance = new NetworkDevice();
                return instance;
            }
        }
        public List<string> Addresses() {
            return NetworkInformation.GetHostNames()
                .Where(h => Convert.ToInt32(h.IPInformation.PrefixLength) == 24)
                .Select(h => h.CanonicalName)
                .ToList();
        }
    }
}
