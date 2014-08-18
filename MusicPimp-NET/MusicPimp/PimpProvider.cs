
namespace Mle.MusicPimp {
    public class PimpProvider {
        private static PimpProvider instance = null;
        public static PimpProvider Instance {
            get {
                if (instance == null)
                    instance = new PimpProvider();
                return instance;
            }
        }

        //Func<MusicEndpoint, PimpPlayer> NewPlayer;

    }
}
