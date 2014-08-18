using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    /// <summary>
    /// Manages remote alarm clocks.
    ///
    /// Alarm toast notifications, when clicked, also open this page. We know
    /// it's opened thru a toast if query parameters "cmd" and "tag" exist.
    /// (The MusicPimp server sets those parameters when sending a toast.)
    /// The tag is defined by the app and sent to the server when push notifications
    /// are enabled for the endpoint; our protocol says that the tag must equal 
    /// the endpoint ID.
    /// </summary>
    public partial class AlarmClock : BasePhonePage {
        private static readonly string
            STOP = "stop";

        private Alarms vm;
        private ProviderService provider = ProviderService.Instance;
        public AlarmClock() {
            InitializeComponent();
            vm = Alarms.Instance;
            vm.UpdateEndpoints();
            DataContext = vm;
        }

        //void AlarmClock_Loaded(object sender, RoutedEventArgs e) {
        //    AddAppBarButton.IsEnabled = vm.Endpoint != null;
        //}
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            // push notifications have query parameters
            MusicEndpoint endpoint = ResolveEndpoint(PageParams.TAG, def: provider.PlayerManager.ActiveEndpoint);
            var shouldStop = Query(PageParams.CMD) == STOP && endpoint != null;
            if(endpoint != null && endpoint.EndpointType == EndpointTypes.MusicPimp) {
                await vm.UpdateUI(endpoint);
            } else {
                var item = PlayerList.SelectedItem;
                if(item != null) {
                    var e2 = item as MusicEndpoint;
                    await vm.UpdateUI(e2);
                } else {
                    var previous = vm.Endpoint;
                    if(previous != null) {
                        await vm.UpdateUI(previous);
                    } else {
                        await vm.UpdateUI();
                    }
                }
            }
            if(shouldStop) {
                await vm.StopAlarmPlayback();
            }
            var btns = ApplicationBar.Buttons;
            if(btns.Count > 0) {
                var btn = btns[0] as ApplicationBarIconButton;
                btn.IsEnabled = vm.Endpoint != null;
            }
        }
        private void Add_Click(object sender, EventArgs e) {
            var queryParams = new Dictionary<string, string>();
            var end = vm.Endpoint;
            if(end != null) {
                queryParams.Add(PageParams.ENDPOINT, end.Id);
                PageNavigationService.Instance.NavigateWithQuery(PageNames.ALARM_EDIT, queryParams);
            } else {
                MessageBox.Show("Please go back and add a MusicPimp server first, then you can add alarms.");
            }

        }
        private async void alarmToggle_Click(object sender, System.Windows.RoutedEventArgs e) {
            // We don't implement this through the IsChecked property binding because we only
            // want to run this if the user clicks the switch; IsChecked fires during 
            // initialization.
            var b = e.OriginalSource as ToggleSwitchButton;
            var dc = b.DataContext as AlarmModel;
            await dc.Save();
        }
        /// <summary>
        /// If the query parameter key contains an endpoint ID, returns that endpoint, otherwise
        /// returns the active playback endpoint.
        /// </summary>
        /// <returns></returns>
        protected MusicEndpoint ResolveEndpoint(string queryKey, MusicEndpoint def = null) {
            var endpointId = Query(queryKey);
            if(endpointId != null) {
                var end = provider.EndpointsData.Endpoints.FirstOrDefault(ep => ep.Id == endpointId);
                if(end != null) {
                    return end;
                }
            }
            return def;
        }
    }
}