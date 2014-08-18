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
        /// 
        /// Cannot add dependency to PCL so this is duplicated between WP and Windows Store.
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
                        // all ID3 libraries for WP suck; no way to get duration reliably prior to opening in a player
                        // so set duration to 0 for now and obtain it when needed for playback
                        var artist = firstTag.Artists.Value;
                        artist = artist != null ? artist.Replace("\0", String.Empty) : String.Empty;
                        var album = firstTag.Album.Value;
                        album = album != null ? album.Replace("\0", String.Empty) : String.Empty;
                        return new SongInfo(title, artist, album, TimeSpan.FromSeconds(0));
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
