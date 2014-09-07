using System;
using System.Collections.Generic;

namespace Mle.Messaging {
    public interface INavigationHandler {
        //void GoBack();
        //void GoForward();
        bool NavigateToPage(Type pageType);
        //bool Navigate(Type source, object parameter = null);
        //bool Navigate(string longPageClassName, object parameter = null);
        bool NavigateWithParam(string parameter);
        // TODO: consider supplying the viewmodel type instead of the target page, then implementations
        // look up the corresponding view for each viewmodel from a dictionary, with query string hack for wp
        //bool Navigate<TViewModelType>(object parameter = null);
        bool Navigate(string pageId);
        /// <summary>
        /// Navigates to pageId and sets the id query parameter to idParam.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="idParam"></param>
        /// <returns></returns>
        bool NavigateWithId(string pageId, string idParam);
        bool NavigateWithParam(string pageId, string navParam);
        bool NavigateWithQuery(string pageId, IDictionary<string, string> queryParams);
    }
}
