using Mle.Exceptions;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mle.Audio {
    public static class MediaElementExtensions {
        public static async Task<TimeSpan> TrackDuration(this MediaElement element, string trackPath) {
            using(var storage = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = storage.OpenFile(trackPath, FileMode.Open, FileAccess.Read)) {
                return await element.StreamDuration(stream);
            }
        }
        /// <summary>
        /// Determines the duration of the stream, which is presumably a media source.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="stream"></param>
        /// <returns>the duration</returns>
        public static Task<TimeSpan> StreamDuration(this MediaElement element, Stream stream) {
            var tcs = new TaskCompletionSource<TimeSpan>();
            RoutedEventHandler handler = null;
            handler = (s, e) => {
                element.MediaOpened -= handler;
                var duration = element.NaturalDuration;
                if (duration.HasTimeSpan) {
                    tcs.SetResult(duration.TimeSpan);
                } else {
                    tcs.SetException(new AudioException("The media element duration is not a TimeSpan."));
                }
            };
            EventHandler<ExceptionRoutedEventArgs> errorHandler = null;
            errorHandler = (s, e) => {
                element.MediaFailed -= errorHandler;
                tcs.SetException(e.ErrorException);
            };
            element.MediaOpened += handler;
            element.MediaFailed += errorHandler;
            element.SetSource(stream);
            return tcs.Task;
        }
    }
}
