using Mle.Messaging;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Util {
    public interface IPimpMessageHandler : IMessageHandler {
        Task HandleConnectivityStatus(bool connected);
    }
}
