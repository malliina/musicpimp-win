using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public interface PimpPlayerElement : PlayerElement {
        Task SetTrack(MusicItem track);
        Task<MusicItem> CurrentTrack();
    }
}
