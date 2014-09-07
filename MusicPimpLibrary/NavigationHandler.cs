using Mle.Collections;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Mle {
    public abstract class NavigationHandler : INavigationHandler {
        private readonly Frame frame;
        protected Dictionary<string, Type> pageIdResolver;

        public NavigationHandler(Frame frame) {
            this.frame = frame;
            pageIdResolver = new Dictionary<string, Type>();
            //pageIdResolver.Add(PageNames.LIBRARY, typeof(MusicItems));
            pageIdResolver.Add(PageNames.IAP, typeof(IapPage));
        }

        public bool Navigate<T>(object parameter = null) {
            var type = typeof(T);
            return Navigate(type, parameter);
        }
        public bool NavigateToPage(Type pageType) {
            return Navigate(pageType, null);
        }

        public bool Navigate(Type source, object parameter = null) {
            return frame.Navigate(source, parameter);
        }
        public bool Navigate(string longPageClassName, object parameter = null) {
            return Navigate(Type.GetType(longPageClassName), parameter);
        }
        public bool NavigateWithParam(string parameter) {
            return Navigate(frame.CurrentSourcePageType, parameter);
        }
        public bool Navigate(string pageId) {
            return NavigateToPage(pageIdResolver[pageId]);
        }
        public bool NavigateWithId(string pageId, string idParam) {
            return NavigateWithParam(pageId, "?id=" + idParam);
        }
        public bool NavigateWithParam(string pageId, string navParam) {
            return Navigate(pageIdResolver[pageId], navParam);
        }
        public bool NavigateWithQuery(string pageId, IDictionary<string, string> queryParams) {
            var queryString = queryParams.Select(kv => kv.Key + "=" + kv.Value).MkString("&");
            return Navigate(pageIdResolver[pageId], queryString);
        }
    }

}
