using Microsoft.Phone.Controls;
using Mle.Util;
using System;
using System.Threading.Tasks;

namespace Mle.Pages {
    public class BasePhonePage : PhoneApplicationPage {
        protected async Task WithPivotIndex(Pivot pivot, Func<int, Task> code) {
            if(pivot.SelectedItem != null) {
                await code(pivot.SelectedIndex);
            }
        }
        protected void WithPivotIndex2(Pivot pivot, Action<int> code) {
            if(pivot.SelectedItem != null) {
                code(pivot.SelectedIndex);
            }
        }
        protected void TryGoBack() {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
        protected T DeserializeQuery<T>(string key, T def) {
            var json = DecodedQuery(key);
            if(json != null) {
                return Json.Deserialize<T>(json);
            } else {
                return def;
            }
        }
        protected string DecodedQuery(string key) {
            var encoded = Query(key);
            if(encoded != null) {
                return Strings.decode(encoded);
            } else {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">query paremeter key</param>
        /// <returns>the value of the query parameter or null if it does not exist</returns>
        protected string Query(string key) {
            string value;
            NavigationContext.QueryString.TryGetValue(key, out value);
            return value;
        }
        
    }
}
