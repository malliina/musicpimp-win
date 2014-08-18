using Mle.MusicPimp.ViewModels;
using Mle.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mle.MusicPimp.Xaml {
    public sealed partial class AddEndpointPopup : PopupUserControl {
        public AddEndpointPopup() : this(new NewEndpoint()) { }
        public AddEndpointPopup(EndpointEditorViewModel e) {
            var vm = e;
            this.DataContext = vm;
            vm.CloseRequested += () => {
                CloseThisPopup();
            };
            this.InitializeComponent();
        }
    }
}
