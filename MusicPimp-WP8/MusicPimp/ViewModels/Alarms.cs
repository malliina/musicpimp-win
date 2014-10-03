using Mle.Concurrent;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class Alarms : WebAwareLoading {
        private static Alarms instance = null;
        public static Alarms Instance {
            get {
                if(instance == null)
                    instance = new Alarms();
                return instance;
            }
        }
        private static readonly int NO_SELECTION = -1;
        private static readonly string
            AtLeastOneMusicPimpServerRequired = "To manage remote alarms, add at least one MusicPimp server.",
            MusicPimpOldVersion = "Remote alarms are supported in version " + PimpSessionBase.AlarmsSupportingVersion + " and later of the MusicPimp server. Please upgrade to the latest release from www.musicpimp.org.";
        private ISettingsManager settings;
        private List<MusicEndpoint> pimpEndpoints;
        public List<MusicEndpoint> PimpEndpoints {
            get { return pimpEndpoints; }
            set { SetProperty(ref pimpEndpoints, value); }
        }
        /// <summary>
        /// ListPicker.SelectedIndex is bound to this property. It appears the index can be -1 iff the ItemsSource of the ListPicker is empty. 
        /// In all other cases, an InvalidOperationException ("SelectedIndex must always be bound to a valid value"...) is thrown if this property 
        /// returns -1. It follows that if a ListPicker has items, one item must always be selected. 
        /// </summary>
        private int pimpEndpointIndex = NO_SELECTION;
        public int PimpEndpointIndex {
            get {
                var endCount = PimpEndpoints.Count;
                if(endCount == 0) {
                    pimpEndpointIndex = NO_SELECTION;
                } else if(endCount <= pimpEndpointIndex || pimpEndpointIndex == NO_SELECTION) {
                    pimpEndpointIndex = 0;
                }
                return pimpEndpointIndex;
            }
            set {
                if(value >= 0 && PimpEndpoints.Count > value) {
                    if(SetProperty(ref pimpEndpointIndex, value)) {
                        UpdateUI2(PimpEndpoints[value]);
                    }
                }
            }
        }
        public MusicEndpoint Endpoint { get; private set; }
        private ObservableCollection<AlarmModel> alarmList;
        public ObservableCollection<AlarmModel> AlarmList {
            get { return alarmList; }
            set {
                if(SetProperty(ref alarmList, value)) {
                    OnPropertyChanged("IsEmpty");
                }
            }
        }
        public bool IsEmpty {
            get { return AlarmList != null && AlarmList.Count == 0; }
        }
        public bool EnableEndpoints {
            get { return !IsLoading && Endpoint != null; }
        }
        private bool isPushEnabled;
        public bool IsPushEnabled {
            get { return isPushEnabled; }
            set {
                if(SetProperty(ref isPushEnabled, value)) {
                    ToggleChanged(value);
                }
            }
        }
        private static readonly string DefaultLoadingText = "Loading...";
        private string loadingText = DefaultLoadingText;
        public string LoadingText {
            get { return loadingText; }
            set { this.SetProperty(ref loadingText, value); }
        }
        private ProviderService provider = ProviderService.Instance;

        private IAlarmClient client;
        protected Alarms() {
            settings = provider.SettingsManager;
            UpdateEndpoints();
            if(PimpEndpoints.Count == 0) {
                FeedbackMessage = AtLeastOneMusicPimpServerRequired;
            }
        }
        public void UpdateEndpoints() {
            PimpEndpoints = provider.EndpointsData.Endpoints
                    .Where(e => e.EndpointType == EndpointTypes.MusicPimp || e.EndpointType == EndpointTypes.PimpCloud)
                    .ToList();
        }
        private async void UpdateUI2(MusicEndpoint endpoint) {
            await UpdateUI(endpoint);
        }
        public async Task UpdateUI(MusicEndpoint pimpEndpoint) {
            var index = PimpEndpoints.FindIndex(e => e.Name == pimpEndpoint.Name);
            if(index >= 0) {
                //pimpEndpointIndex = index;
                ResetClient(pimpEndpoint);
                UpdateToggleSilently(Push.IsChannelOpen());
                if(IsPushEnabled) {
                    try {
                        await SubscribeToPush(silent: true);
                    } catch(Exception) { }
                }
                await UpdateAlarmList(client);
                OnPropertyChanged("EnableEndpoints");
                PimpEndpointIndex = index;
                //OnPropertyChanged("PimpEndpointIndex");
            } else {
                await UpdateUI();
            }
        }
        public async Task UpdateUI() {
            var endpoints = PimpEndpoints;
            if(endpoints.Count > 0) {
                await UpdateUI(endpoints[0]);
            } else {
                Endpoint = null;
                FeedbackMessage = AtLeastOneMusicPimpServerRequired;
                OnPropertyChanged("EnableEndpoints");
            }
        }
        private void ResetClient(MusicEndpoint endpoint) {
            Endpoint = endpoint;
            if(client != null) {
                Utils.Suppress<Exception>(client.Session.Dispose);
                client = null;
            }
            if(endpoint != null) {
                client = new SimpleAlarmClient(endpoint, DateTimeHelper.Instance);
            }
        }
        private void UpdateToggleSilently(bool value) {
            // We don't set IsPushEnabled because that would trigger ToggleChanged, instead, 
            // we use the private isPushEnabled and ensure the UI is updated accordingly.
            isPushEnabled = value;
            OnPropertyChanged("IsPushEnabled");
        }
        private async Task UpdateAlarmList(IAlarmClient client) {
            if(client != null) {
                await WebAware(async () => {
                    var version = await client.Session.PingAuth();
                    var serverVersion = new Version(version.version);
                    if(serverVersion < PimpSessionBase.AlarmsSupportingVersion) {
                        FeedbackMessage = MusicPimpOldVersion;
                    } else {
                        var alarms = await client.Alarms();
                        if(alarms.Count() == 0) {
                            FeedbackMessage = "No alarms have been added.";
                        }
                        AlarmList = new ObservableCollection<AlarmModel>(alarms);
                    }
                });
            } else {
                FeedbackMessage = AtLeastOneMusicPimpServerRequired;
            }
        }
        public Task StopAlarmPlayback() {
            return WebAware(client.StopPlayback);
        }
        /// <summary>
        /// Called when the user has toggled push notifications on/off. Negotiates with
        /// the MusicPimp servers to register/deregister a push notification URI.
        /// 
        /// If enabling succeeds, the enabled value is saved. However, when disabling, 
        /// the disabled value is stored regardless of success. We can always kill the 
        /// notification channel locally, therefore disabling always succeeds to a sufficient 
        /// degree.
        /// </summary>
        /// <param name="toggleValue"></param>
        private async void ToggleChanged(bool toggleValue) {
            await WebAware(async () => {
                if(toggleValue) {
                    await SubscribeToPush();
                } else {
                    try {
                        if(Endpoint != null) {
                            await UnsubscribeFromPush(Endpoint);
                        }
                    } finally {
                        Push.Close();
                    }
                }
            });
            LoadingText = DefaultLoadingText;
        }
        private async Task SubscribeToPush(bool silent = false) {
            Uri uri = (await Push.Open()).ChannelUri;
            if(Endpoint != null) {
                await WithClient(Endpoint, client => TryEnablePush(client, uri, silent));
            }
        }
        private Task UnsubscribeFromPush(MusicEndpoint endpoint) {
            return UnsubscribeFromPush(new List<MusicEndpoint>() { Endpoint });
        }
        private async Task UnsubscribeFromPush(IEnumerable<MusicEndpoint> endpoints) {
            var channelOpt = Push.GetChannelOpt();
            if(channelOpt != null) {
                var uriOpt = channelOpt.ChannelUri;
                if(uriOpt != null) {
                    foreach(var endpoint in endpoints) {
                        await WithClient(endpoint, client => DisablePush(client, uriOpt));
                    }
                }
            }
        }
        private Task WithClient(MusicEndpoint endpoint, Func<PushClient, Task> f) {
            var client = new PushClient(endpoint);
            var task = f(client);
            var t = task.ContinueWith(t2 => client.Dispose());
            return task;
        }
        private async Task TryEnablePush(PushClient helper, Uri uri, bool silent = false) {
            if(!silent) {
                LoadingText = "Enabling...";
            }
            await AsyncTasks.Within2(helper.Register(uri), TimeSpan.FromSeconds(10));
        }
        private async Task DisablePush(PushClient helper, Uri uri) {
            LoadingText = "Disabling...";
            await Utils.SuppressAsync<TimeoutException>(async () => {
                await AsyncTasks.Within2(helper.Deregister(uri), TimeSpan.FromSeconds(5));
            });
        }

    }
}
