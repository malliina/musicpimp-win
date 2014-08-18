using Id3;
using Mle.IO;
using Mle.MusicPimp.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mle.MusicPimp.Local {
    public class Id3Reader {
        /// <summary>
        /// Uses ID3.NET.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static SongInfo ReadId3(string filePath, Stream stream) {
            try {
                using (var mp3 = new Mp3Stream(stream)) {
                    var tags = new List<Id3Tag>(mp3.GetAllTags());
                    if (tags.Count > 0) {
                        var firstTag = tags.First();
                        var title = firstTag.Title.Value;
                        if (String.IsNullOrWhiteSpace(title))
                            title = PathHelper.Instance.FileNameWithoutExtension(filePath);
                        // All ID3 libraries for WP suck; no way to get duration reliably prior to opening in a player,
                        // so sets duration to 0 for now and obtain it when needed for playback.
                        return new SongInfo(title, firstTag.Artists.Value, firstTag.Album.Value, TimeSpan.FromSeconds(0));
                    } else {
                        return null;
                    }
                }
            } catch (IndexOutOfRangeException) {
                // maybe thrown if the file is cocked up and id3 tags cannot be read
                return null;
            }
        }
    }
}
