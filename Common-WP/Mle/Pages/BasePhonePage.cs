using Microsoft.Phone.Controls;
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
