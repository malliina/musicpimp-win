using Mle.Xaml.Commands;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class StoreEditEndpoint : EditEndpoint {

        public ICommand Remove { get; private set; }

        public StoreEditEndpoint(string endpointName)
            : base(endpointName) {
            Remove = new AsyncDelegateCommand<MusicEndpoint>(async e => {
                Close();
                await Endpoints.RemoveAndSave(e);
            });
        }
    }
}
