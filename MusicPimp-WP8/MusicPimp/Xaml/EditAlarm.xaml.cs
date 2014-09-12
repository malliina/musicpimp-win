using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Pages;
using Mle.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    public partial class EditAlarm : BasePhonePage {
        private ProviderService provider = ProviderService.Instance;
        private IDateTimeHelper timeHelper = DateTimeHelper.Instance;
        private AlarmModel viewModel;
        public EditAlarm() {
            InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var endpointId = Query(PageParams.ENDPOINT);
            var alarmId = Query(PageParams.ID);
            var isDoNothing = endpointId == null && alarmId == null;
            var isAddNew = endpointId != null && alarmId == null;
            var isUpdate = alarmId != null;
            if(isDoNothing) {

            } else {
                // After updating the time (on a separate page), the user is
                // navigated back here, then the alarm is loaded from the backend
                // and the old time is reinstated. Clearing the navigation query
                // string fixes this. The alarm is then only loaded when clicked
                // from the Alarms page.
                MusicEndpoint endpoint = DetermineEndpointOrActive(PageParams.ENDPOINT);
                NavigationContext.QueryString.Clear();
                viewModel = AlarmModel.Build(endpoint, timeHelper);
                DataContext = viewModel;
                if(isUpdate) {
                    await viewModel.Fill(alarmId);
                }
                await viewModel.InstallPlayer();
                //await viewModel.LoadTracks();
            }
        }
        protected async override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            await viewModel.UninstallPlayer();
        }
        private async void Save_Click(object sender, EventArgs e) {
            if(await viewModel.Save()) {
                TryGoBack();
            }
        }
        private async void Delete_Click(object sender, EventArgs e) {
            if(viewModel.Id != null) {
                await viewModel.Remove();
            }
            TryGoBack();
        }
        private void TryGoBack() {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
        /// <summary>
        /// If the query parameter key contains an endpoint ID, returns that endpoint, otherwise
        /// returns the active playback endpoint.
        /// </summary>
        /// <returns></returns>
        protected MusicEndpoint DetermineEndpointOrActive(string queryKey) {
            var endpointId = Query(queryKey);
            if(endpointId != null) {
                var end = ProviderService.Instance.EndpointsData.Endpoints.FirstOrDefault(ep => ep.Id == endpointId);
                if(end != null) {
                    return end;
                }
            }
            return PhonePlayerManager.Instance.ActiveEndpoint;
        }
        /// <summary>
        /// The "Populating" event is called at each key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AutoComp_Populating(object sender, Microsoft.Phone.Controls.PopulatingEventArgs e) {
            //Debug.WriteLine("Searching: " + viewModel.TrackName);
            e.Cancel = true;
            await viewModel.PerformSearch();
            AutoComp.PopulateComplete();
        }
    }
}