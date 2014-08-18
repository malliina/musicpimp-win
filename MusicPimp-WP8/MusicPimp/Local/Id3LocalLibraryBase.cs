using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.Util;
using System.IO;

namespace Mle.MusicPimp.Local {
    public abstract class Id3LocalLibraryBase : LocalLibraryBase {
        public Id3LocalLibraryBase(ISettingsManager settings)
            : base(settings) {

        }
        /// <summary>
        /// Uses ID3.NET.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override SongInfo ReadId3(string filePath, Stream stream) {
            var songInfo = Id3Reader.ReadId3(filePath, stream);
            if (songInfo == null) {
                songInfo = FromFile(filePath);
            }
            return songInfo;
        }
    }
}
