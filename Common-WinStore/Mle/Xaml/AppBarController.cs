using Mle.ViewModels;
using System.Collections;

namespace Mle.Xaml {
    /// <summary>
    /// Facilitates cases where the appbar is opened/closed
    /// based on user selections in a list view. Typically, 
    /// no selection = closed, one or more selections = open.
    /// 
    /// The appbar might be open even if the selection is empty,
    /// so these properties are not always up to date, but
    /// <c>Update(ListViewBase)</c> synchronizes the appbar state.
    /// </summary>
    public class AppBarController : ViewModelBase {
        private bool isSelectionEmpty = true;
        public bool IsSelectionEmpty {
            get { return isSelectionEmpty; }
            set {
                if (SetProperty(ref this.isSelectionEmpty, value)) {
                    IsAppBarOpen = !value;
                }
            }
        }
        private bool isAppBarOpen = false;
        public bool IsAppBarOpen {
            get { return isAppBarOpen; }
            set { SetProperty(ref this.isAppBarOpen, value); }
        }
        
        /// <summary>
        /// The appbar might be opened/closed by forces 
        /// that don't know about these properties.
        /// 
        /// Calling this ensures that the appbar is in
        /// the correct state as dictated by the
        /// selection.
        /// 
        /// The correct usage is to call this in response
        /// to SelectionChanged events of the selector.
        /// </summary>
        /// <param name="selector">selector that requires an appbar upon selection</param>
        public void Update(IList items) {
            //var items = selector.SelectedItems;
            IsSelectionEmpty = items == null || items.Count == 0;
            OnPropertyChanged("IsAppBarOpen");
        }
    }
}
