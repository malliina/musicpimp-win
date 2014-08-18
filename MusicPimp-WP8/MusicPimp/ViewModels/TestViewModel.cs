using Mle.ViewModels;
using System.Collections.Generic;

namespace Mle.MusicPimp.ViewModels {
    public class TestViewModel : ViewModelBase {
        private bool _isChecked = true;
        public bool IsChecked {
            get { return _isChecked; }
            set { this.SetProperty(ref this._isChecked, value, "IsChecked"); }
        }
        private string _selected = "a";
        public string Selected {
            get { return _selected; }
            set { this.SetProperty(ref this._selected, value, "Selected"); }
        }
        private IEnumerable<string> items = new List<string> { "a", "b", "c" };
        public IEnumerable<string> ListItems { get { return items; } }
        public void test() {
            //WebUtility.
        }
    }
}
