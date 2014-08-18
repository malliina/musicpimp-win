using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public interface PlayerElement {
        TimeSpan Position { get; set; }
        double Volume { get; set; }
        Task Play();
        Task Pause();
        Task Stop();
        Task<bool> IsPlaying();
        Task<bool> HasTrack();
    }
}
