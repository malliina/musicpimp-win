using Mle.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.Tiles {
    public class PimpTiles {

        public static readonly string CoverArtFolderPath = "CoverArt\\";

        public static async Task EnsureCoverFolderExists() {
            var coverParentFolder = ApplicationData.Current.LocalFolder;
            await coverParentFolder.CreateFolderAsync(
               CoverArtFolderPath, CreationCollisionOption.OpenIfExists);
        }

        public static async Task UpdateLiveTiles(TileUtil util) {
            await EnsureCoverFolderExists();
            var fileUtils = new AppLocalFolderFileUtils();
            var images = (await fileUtils.ListFilesAsUris(CoverArtFolderPath))
                .OrderBy(uri => Guid.NewGuid()) // randomizes
                .Take(5) // at most 5 images are needed
                .ToList();
            util.UpdateLiveTiles(images);
        }
    }
}
