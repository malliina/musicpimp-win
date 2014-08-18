using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Tiles {
    public class PhoneTileUtil {
        public static async Task UpdateNoTrack() {
            var covers = await PhoneCoverService.Instance.GetCoverCollection(9);
            if(covers.Count > 0) {
                UpdateTileCyclic("MusicPimp", covers.Select(TileUri).ToList());
            }
        }
        public static Uri TileUri(Uri uri) {
            return new Uri("isostore:/" + uri.OriginalString);
        }
        public static void UpdateTile(ShellTileData tileUpdateData) {
            // there's always at least one tile, the main tile, which is first
            var mainTile = ShellTile.ActiveTiles.ElementAt(0);
            mainTile.Update(tileUpdateData);
        }
        public static void UpdateTileCyclic(string title, IList<Uri> isoStorePrefixedImageUris) {
            if(isoStorePrefixedImageUris.Count > 0) {
                var tileData = new CycleTileData() {
                    Title = title,
                    SmallBackgroundImage = isoStorePrefixedImageUris[0],
                    CycleImages = isoStorePrefixedImageUris
                };
                UpdateTile(tileData);
            }
        }
    }
}
