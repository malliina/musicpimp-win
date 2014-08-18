
using System.Threading.Tasks;
namespace Mle.MusicPimp.ViewModels {
    public class NewEndpoint : EndpointEditorViewModel {
        public NewEndpoint() {
            EndpointItem = new MusicEndpoint();
            MakeActiveLibrary = true;
        }
        public override Task AddOrUpdate(MusicEndpoint endpoint) {
            return Endpoints.AddAndSave(endpoint);
        }
    }
}
