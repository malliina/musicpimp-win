using System;
using System.Windows.Threading;

namespace Mle.Util {
    public class TickTimer {

        public DispatcherTimer Timer { get; private set; }

        public TickTimer(TimeSpan interval) {
            Timer = new DispatcherTimer();
            Timer.Interval = interval;
        }
        public TickTimer()
            : this(TimeSpan.FromSeconds(1)) {
        }

        public void Start() {
            if (!Timer.IsEnabled)
                Timer.Start();
        }
        public void Stop() {
            if (Timer.IsEnabled)
                Timer.Stop();
        }
    }
}
