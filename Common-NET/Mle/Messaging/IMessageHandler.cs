using System.Threading.Tasks;

namespace Mle.Messaging {
    public interface IMessageHandler {
        Task Handle(SimpleMessage msg);
        Task Handle(HeaderMessage msg);
    }
    
}
