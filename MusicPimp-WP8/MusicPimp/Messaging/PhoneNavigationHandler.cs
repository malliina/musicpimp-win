using Mle.Collections;
using Mle.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Mle.MusicPimp.Messaging {
    public class PhoneNavigationHandler : INavigationHandler {
        private Frame frame;
        private Dictionary<string, Uri> pageIdResolver;
        private Dictionary<Type, Uri> viewModelResolver;
        private static long counter = 0L;

        public PhoneNavigationHandler(Frame rootFrame) {
            frame = rootFrame;
            pageIdResolver = new Dictionary<string, Uri>();
            // TODO remove fragility
            AddRoute(PageNames.LIBRARY, "/MusicPimp/Xaml/MusicFiles.xaml");
            AddRoute(PageNames.IAP, "/MusicPimp-WP8;component/MusicPimp/Xaml/IapPage.xaml");
            AddRoute(PageNames.ALARM_EDIT, "/MusicPimp/Xaml/EditAlarm.xaml");
            AddRoute(PageNames.ALARMS, "/MusicPimp/Xaml/AlarmClock.xaml");
            viewModelResolver = new Dictionary<Type, Uri>();
        }
        private void AddRoute(string id, string uriString) {
            pageIdResolver.Add(id, new Uri(uriString, UriKind.Relative));
        }

        public bool NavigateWithParam(string parameter) {
            var uriString = frame.CurrentSource.OriginalString;
            var cutOff = uriString.IndexOf('?');
            if(cutOff == -1) {
                cutOff = uriString.Length;
            }
            // When the library changes, the user is shown the root library view. However,
            // no navigation takes place and therefore frame.CurrentSource points to 
            // whatever folder URI the user was at before the library change.
            // When navigating to a new page, if frame.CurrentSource of the previous page
            // equals the new navigation URI, no navigation will take place.
            // Thus after a library change, navigating to the same folder as before will
            // not work unless the URI is distinct from the previous one. So we artificially
            // create distinct URIs for each page navigation by appending an increasing counter
            // value to each URI.
            var pageUri = new Uri(uriString.Substring(0, cutOff) + "?folder=" + parameter + "&count=" + (++counter), UriKind.Relative);
            return frame.Navigate(pageUri);
        }

        public bool NavigateToPage(Type pageType) {
            return false;
        }
        public bool Navigate(string pageId) {
            return frame.Navigate(pageIdResolver[pageId]);
        }
        public bool NavigateWithId(string pageId, string id) {
            var dict = new Dictionary<string, string>();
            dict.Add(PageParams.ID, id);
            return NavigateWithQuery(pageId, dict);
        }
        public bool NavigateWithParam(string pageId, string navParam) {
            var dict = new Dictionary<string, string>();
            dict.Add(PageParams.P, navParam);
            return NavigateWithQuery(pageId, dict);
        }
        public bool NavigateWithQuery(string pageId, IDictionary<string, string> queryParams) {
            string queryString = queryParams.Select(kv => kv.Key + "=" + kv.Value).MkString("&");
            Uri uri = new Uri(pageIdResolver[pageId].OriginalString + "?" + queryString, UriKind.Relative);
            return frame.Navigate(uri);
        }
    }
}
