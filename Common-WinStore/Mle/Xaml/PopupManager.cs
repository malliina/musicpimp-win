using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mle.Xaml {
    /// <summary>
    /// Instances of this class manage one popup. Usage: PopupManager.Show(...); 
    /// </summary>
    public class PopupManager {
        public static void Show(UserControl control) {
            var popup = new PopupManager(control);
            popup.ShowPopup();
        }
        protected Popup popup;
        protected UserControl popupContent;
        protected Rect bounds;

        protected PopupManager(UserControl popupContent) {
            this.popupContent = popupContent;
            bounds = Window.Current.Bounds;
            popup = new Popup();
            popup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            popup.Child = popupContent;
        }
        // Makes HorizontalAlignment and VerticalAlignment of the control relative to the whole page
        protected virtual void BeforeOpen() {
            popupContent.Width = bounds.Width;
            popupContent.Height = bounds.Height;
        }
        /// <summary>
        /// Show the Popup with the UserControl as content
        /// </summary>
        /// <param name="control"></param>
        public void ShowPopup() {
            BeforeOpen();
            popup.IsOpen = true;
        }
        protected void OnPopupClosed(object sender, object e) {
            Window.Current.Activated -= OnWindowActivated;
        }
        protected void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e) {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated) {
                popup.IsOpen = false;
            }
        }
    }
}
