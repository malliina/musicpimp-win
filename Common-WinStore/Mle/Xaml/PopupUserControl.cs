using Callisto.Controls;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mle.Xaml {
    public class PopupUserControl : UserControl {

        protected void GoBack() {
            CloseThisPopup();
            NavigateBack();
        }

        protected void CloseThisPopup() {
            if (this.Parent != null && this.Parent.GetType() == typeof(Popup)) {
                ((Popup)this.Parent).IsOpen = false;
            }
            if (this.Parent != null && this.Parent.GetType() == typeof(SettingsFlyout)) {
                ((SettingsFlyout)this.Parent).IsOpen = false;
            }
        }
        protected virtual void NavigateBack() {
            SettingsPane.Show();
        }
    }
}
