using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class EditEndpoint : EndpointEditorViewModel {
        public EditEndpoint(string endpointName) {
            EndpointItem = Endpoints.Endpoints.FirstOrDefault(item => item.Name == endpointName);
            if(EndpointItem == null) {
                EndpointItem = new MusicEndpoint();
            }
            MakeActiveLibrary = false;
            Update();
        }
        public override Task AddOrUpdate(MusicEndpoint endpoint) {
            return Endpoints.SaveChanges(endpoint);
        }
    }
}
