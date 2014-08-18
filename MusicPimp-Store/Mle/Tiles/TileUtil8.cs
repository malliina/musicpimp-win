using Windows.UI.Notifications;

namespace Mle.Tiles {
    public class TileUtil8 : TileUtil {
        private static TileUtil8 instance = null;
        public static TileUtil8 Instance {
            get {
                if(instance == null)
                    instance = new TileUtil8();
                return instance;
            }
        }
        protected TileUtil8() : base(TileTemplateType.TileSquareImage, TileTemplateType.TileWideImage, TileTemplateType.TileWideImageCollection) { }
    }
}
