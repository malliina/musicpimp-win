using Mle.Common;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Mle.MusicPimp.Xaml {
    public abstract class BasePage : LayoutAwarePage {

        protected void WithLast<T>(ListViewBase selector, Action<T> code) {
            try {
                var items = selector.SelectedItems;
                if (items != null && items.Count > 0) {
                    code((T)items.Last());
                }
            } finally {
                selector.SelectedItem = null;
            }
        }
    }
}
