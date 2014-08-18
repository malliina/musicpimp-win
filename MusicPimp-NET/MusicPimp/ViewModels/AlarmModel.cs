using Mle.Concurrent;
using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class AlarmModel : WebAwareLoading {
        public static readonly string[] DayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

        private IAlarmClient client;
        public IAlarmClient Client {
            get { return this.client; }
            set { this.SetProperty(ref client, value); }
        }
        private string id;
        public string Id {
            get { return this.id; }
            set { this.SetProperty(ref id, value); }
        }
        private string trackName;
        public string TrackName {
            get { return this.trackName; }
            set { this.SetProperty(ref trackName, value); }
        }
        private MusicItem track;
        public MusicItem Track {
            get { return this.track; }
            set {
                if(SetProperty(ref this.track, value)) {
                    TrackName = value.Name;
                    OnPropertyChanged("TrackName");
                }
            }
        }
        private bool isOn;
        public bool IsOn {
            get { return this.isOn; }
            set {
                if(this.SetProperty(ref this.isOn, value)) {
                    OnPropertyChanged("OnOrOffText");
                };
            }
        }
        public string OnOrOffText {
            get { if(IsOn) { return "On"; } else { return "Off"; } }
        }
        private DateTime time = DateTime.Now;
        public DateTime Time {
            get { return this.time; }
            set {
                if(this.SetProperty(ref this.time, value)) {
                    OnPropertyChanged("TimeOnly");
                }
            }
        }
        public string TimeOnly {
            get { return Client.TimeHelper.FormatTimeOnly(Time); }
        }
        private ObservableCollection<object> enabledDays = new ObservableCollection<object>();
        public ObservableCollection<object> EnabledDays {
            get { return enabledDays; }
            set {
                if(this.SetProperty(ref this.enabledDays, value)) {
                    OnPropertyChanged("DaysReadable");
                }
            }
        }
        private IEnumerable<MusicItem> tracks;
        public IEnumerable<MusicItem> Tracks {
            get { return tracks; }
            set { SetProperty(ref tracks, value); }
        }
        public string DaysReadable {
            get { return Client.TimeHelper.SummarizeDaysOfWeek(EnabledDays); }
        }
        private bool isPlaying = false;
        public bool IsPlaying {
            get { return isPlaying; }
            set {
                if(SetProperty(ref isPlaying, value)) {
                    OnPropertyChanged("PlayStopText");
                }
            }
        }
        public string PlayStopText {
            get {
                if(IsPlaying) {
                    return "Stop playback";
                } else {
                    return "Play now";
                };
            }
        }
        public string inputFeedback;
        public string InputFeedback {
            get { return inputFeedback; }
            set { SetProperty(ref inputFeedback, value); }
        }
        public ICommand NavigateToAlarm { get; private set; }
        public ICommand PlayOrStop { get; private set; }
        public ICommand SaveAlarm { get; private set; }
        public ICommand RemoveAlarm { get; private set; }

        private BasePlayer player;
        private ProviderService provider = ProviderService.Instance;
        public AlarmModel(IAlarmClient client) {
            Client = client;
            player = provider.PlayerManager.NoUploadsPimpPlayer(Client.Session);
            EnabledDays.CollectionChanged += (s, e) => {
                OnPropertyChanged("DaysReadable");
            };
            NavigateToAlarm = new DelegateCommand<AlarmModel>(alarm => {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add(PageParams.ENDPOINT, Client.Session.Endpoint.Id);
                queryParams.Add(PageParams.ID, alarm.Id);
                PageNavigationService.Instance.NavigateWithQuery(PageNames.ALARM_EDIT, queryParams);
            });
            PlayOrStop = new AsyncDelegateCommand<AlarmModel>(alarm => alarm.PlayOrStopAlarm());
            SaveAlarm = new AsyncDelegateCommand<AlarmModel>(alarm => alarm.Save());
        }

        public Task PlayOrStopAlarm() {
            return WebAware(async () => {
                EnsureTrackIsUpdated();
                if(!IsPlaying) {
                    if(Track != null) {
                        InputFeedback = String.Empty;
                        await player.PlaySong(Track);
                    } else {
                        InputFeedback = "Please enter a valid track.";
                    }
                } else {
                    await player.pause();
                }
            });
        }
        public Task InstallPlayer() {
            return WebAware(async () => {
                player.PlayerStateChanged += p_PlayerStateChanged;
                EnsureTrackIsUpdated();
                await player.Subscribe();
            });
        }
        public Task UninstallPlayer() {
            return WebAware(async () => {
                if(player != null) {
                    player.Unsubscribe();
                    player.PlayerStateChanged -= p_PlayerStateChanged;
                }
                await AsyncTasks.Noop();
            });
        }

        private void p_PlayerStateChanged(PlayerState state) {
            IsPlaying = state == PlayerState.Playing;
        }
        public Task Fill(string alarmId) {
            return WebAware(async () => {
                var src = (await Client.Alarms()).First(a => a.Id == alarmId);
                Id = src.Id;
                IsOn = src.IsOn;
                Time = src.Time;
                EnabledDays = src.EnabledDays;
                Track = src.Track;
            });
        }
        public static AlarmModel Build(MusicEndpoint endpoint, IDateTimeHelper helper) {
            return new AlarmModel(BuildClient(endpoint, helper));
        }
        public static IAlarmClient BuildClient(MusicEndpoint endpoint, IDateTimeHelper timeHelper) {
            var session = new SimplePimpSession(endpoint);
            return new SimpleAlarmClient(session, timeHelper);
        }
        public Task LoadTracks() {
            return Utils.SuppressAsync<Exception>(async () => {
                var ts = await client.Tracks();
                Tracks = ts;
            });
        }
        public async Task<bool> Save() {
            var wasSaved = false;
            await WebAware(async () => {
                // The autocompletebox does not update property Track, but TrackName.
                // So if TrackName has changed, we manually update Track before saving.
                EnsureTrackIsUpdated();
                try {
                    await Client.Save(this);
                    wasSaved = true;
                } catch(PimpException pe) {
                    InputFeedback = pe.Message;
                    wasSaved = false;
                }
            });
            return wasSaved;
        }
        private async Task<bool> TryUpdateTrackThenSave() {
            if(EnsureTrackIsUpdated()) {
                await Client.Save(this);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if property Track was updated; false otherwise; note that Track may or may not be null</returns>
        private bool EnsureTrackIsUpdated() {
            if(!string.IsNullOrWhiteSpace(TrackName) && (Track == null || Track.Name != TrackName) && Tracks != null) {
                var newTrack = Tracks.FirstOrDefault(t => t.Name == TrackName);
                if(newTrack != null) {
                    Track = newTrack;
                    return true;
                }
            }
            return false;
        }
        public Task Remove() {
            return Client.Remove(Id);
        }
    }
}
