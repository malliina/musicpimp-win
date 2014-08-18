using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Beam;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Mle.MusicPimp.ViewModels {
    public class PhonePlayerManager : PlayerManager {
        private static PhonePlayerManager instance = null;
        public static PhonePlayerManager Instance {
            get {
                if (instance == null)
                    instance = new PhonePlayerManager();
                return instance;
            }
        }

        protected PhonePlayerManager()
            : base(Settings.Instance) {
        }

    }
}
