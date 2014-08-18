using Mle.Exceptions;
using Mle.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mle.Audio {
    public static class MediaElementExtensions {
        public static async Task<TimeSpan> FileDuration(this MediaElement element, StorageFile file) {
            using (var stream = await file.OpenReadAsync()) {
                return await element.Duration(stream, (e, s) => e.SetSource(s, s.ContentType));
            }
        }
        //public static Task<TimeSpan> StreamDuration(this MediaElement element, Stream stream) {
        //    using(var stream = await file.OpenReadAsync()) {
        //        return element.Duration(stream, (e, s) => e.SetSource(s, s.ContentType));
        //    }
        //}
        public static async Task<TimeSpan> UriDuration(this MediaElement element, Uri uri) {
            var file = await StoreFileUtils.GetFile(uri);
            if (file != null) {
                return await element.FileDuration(file);
            } else {
                return await element.Duration(uri, (e, u) => e.Source = u);
            }
        }
        /// <summary>
        /// Determines the duration of the media source.
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="stream"></param>
        /// <returns>the duration</returns>
        private static Task<TimeSpan> Duration<T>(this MediaElement element, T source, Action<MediaElement, T> setSource) {
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
            ExceptionRoutedEventHandler errorHandler = null;
            errorHandler = (s, e) => {
                element.MediaFailed -= errorHandler;
                tcs.SetException(new AudioException(e.ErrorMessage));
            };
            element.MediaOpened += handler;
            element.MediaFailed += errorHandler;
            setSource(element, source);
            return tcs.Task;
        }
    }
}
