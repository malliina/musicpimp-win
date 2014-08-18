using System;
using System.Collections.Generic;

namespace Mle.Messaging {
    public class PageNavigationService {
        private static PageNavigationService instance = null;
        public static PageNavigationService Instance {
            get {
                if(instance == null)
                    instance = new PageNavigationService();
                return instance;
            }
        }

        public event Func<Type, bool> OnNavigateToPage;
        public event Func<string, bool> OnSamePageNavigate;
        public event Func<string, bool> OnNavigate;
        public event Func<string, string, bool> OnNavigateWithId;
        public event Func<string, IDictionary<string, string>, bool> OnNavigateWithQuery;

        protected PageNavigationService() {

        }

        public void Register(INavigationHandler handler) {
            OnNavigateToPage += handler.NavigateToPage;
            OnSamePageNavigate += handler.NavigateWithParam;
            OnNavigate += handler.Navigate;
            OnNavigateWithId += handler.NavigateWithId;
            OnNavigateWithQuery += handler.NavigateWithQuery;
        }

        public void NavigateToPage(Type pageType) {
            if(OnNavigateToPage != null) {
                OnNavigateToPage(pageType);
            }
        }
        public void NavigateWithinSamePage(string pageParameter) {
            if(OnSamePageNavigate != null) {
                OnSamePageNavigate(pageParameter);
            }
        }
        public void Navigate(string pageId) {
            if(OnNavigate != null) {
                OnNavigate(pageId);
            }
        }
        public void Navigate(string pageId, string paramId) {
            if(OnNavigateWithId != null) {
                OnNavigateWithId(pageId, paramId);
            }
        }
        public void NavigateWithQuery(string pageId, IDictionary<string, string> queryParams) {
            if(OnNavigateWithQuery != null) {
                OnNavigateWithQuery(pageId, queryParams);
            }
        }
    }
}
