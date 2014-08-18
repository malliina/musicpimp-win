using System;
using System.Threading.Tasks;

namespace Mle.Util {
    public class Utils {
        public static void Suppress<T>(Action f) where T : Exception {
            try {
                f();
            } catch (T) {

            }
        }
        public static async Task SuppressAsync<T>(Func<Task> f) where T : Exception {
            try {
                await f();
            } catch (T) {

            }
        }
        public static string EmptyIfNull(string nullable){
            return nullable == null ? "" : nullable;
        }
    }
}
