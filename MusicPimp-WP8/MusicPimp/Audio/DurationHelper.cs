using Mle.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mle.MusicPimp.Audio {
    public class DurationHelper {

        // The MediaElement is not used for playback but only to determine the duration of songs
        // defined in App.xaml
        protected static MediaElement MediaElement {
            get { return Application.Current.Resources["mediaElement"] as MediaElement; }
        }
        public static async Task SetDurationIfZero(MusicItem track) {
            if(track.Duration.TotalSeconds < 1) {
                await SetDuration(track);
            }
        }
        public static async Task SetDuration(MusicItem track) {
            track.Duration = await GetDuration(track);
        }
        public static async Task<TimeSpan> GetDuration(MusicItem track) {
            var filePath = PhoneLocalLibrary.Instance.AbsolutePathTo(track);
            return await MediaElement.TrackDuration(filePath);
        }

    }
}
