using Microsoft.Phone.BackgroundAudio;
using Mle.MusicPimp.ViewModels;
using System;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// Use OO
    /// </summary>
    public class AudioItemConversions {
        public static AudioTrack item2track(MusicItem item, Uri source) {
            return new AudioTrack(
                source,
                item.Name,
                item.Artist,
                item.Album,
                null,
                item.Path,
                EnabledPlayerControls.All
                );
        }
        public static MusicItem track2item(AudioTrack track) {
            return new MusicItem() {
                Name = track.Title,
                Artist = track.Artist,
                Album = track.Album,
                Source = track.Source,
                Path = track.Tag,
                Duration = track.DurationOrElse(TimeSpan.FromSeconds(0))
            };
        }
    }
}
