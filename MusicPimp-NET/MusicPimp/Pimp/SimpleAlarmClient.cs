using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class SimpleAlarmClient : IAlarmClient {
        private static readonly string
            alarmsResource = "/alarms",
            tracksResource = "/tracks",
            searchResource = "/search",
            save = "save",
            delete = "delete",
            start = "start",
            stop = "stop";
        public SimplePimpSession Session { get; protected set; }
        public IDateTimeHelper TimeHelper { get; private set; }
        public SimpleAlarmClient(SimplePimpSession session, IDateTimeHelper timeHelper) {
            Session = session;
            TimeHelper = timeHelper;
        }
        public SimpleAlarmClient(MusicEndpoint pimpEndpoint, IDateTimeHelper timeHelper) :
            this(BuildSession(pimpEndpoint), timeHelper) { }

        public static SimplePimpSession BuildSession(MusicEndpoint endpoint) {
            if(endpoint.EndpointType == EndpointTypes.PimpCloud) {
                return new CloudSession(endpoint);
            } else {
                return new SimplePimpSession(endpoint);
            }
        }
        public Task<IEnumerable<AlarmModel>> Alarms() {
            return MapList<AlarmModel, MusicAlarm>(alarmsResource, ToModel);
        }
        public Task<IEnumerable<MusicItem>> Tracks() {
            return MapList<MusicItem, PimpTrack>(tracksResource, item => AudioConversions.PimpTrackToMusicItem(item, null, Session.Username, Session.Password, Session.CloudServerID));
        }
        public Task<IEnumerable<MusicItem>> Search(string term) {
            return MapList<MusicItem, PimpTrack>(searchResource + "?term=" + term, item => AudioConversions.PimpTrackToMusicItem(item, null, Session.Username, Session.Password, Session.CloudServerID));
        }
        private async Task<IEnumerable<T>> MapList<T, U>(string resource, Func<U, T> mapper) {
            IEnumerable<U> items = await Session.ToJson<IEnumerable<U>>(resource);
            return items.Select(mapper).ToList();
        }
        public Task StartPlayback(string alarmId) {
            return PostId(start, alarmId);
        }
        public Task StopPlayback() {
            return Post(new SimpleCommand(stop));
        }
        public Task Save(AlarmModel schedule) {
            Validate(schedule);
            var json = ToShortJson(schedule);
            return Session.PostCommand(new AlarmCommand(save, json), alarmsResource);
        }
        public void Validate(AlarmModel schedule) {
            if(schedule.Track == null) {
                throw new PimpException("Please supply a track.");
            }
            if(schedule.EnabledDays.Count == 0) {
                throw new PimpException("Please choose at least one day.");
            }
        }
        public Task Remove(string id) {
            return PostId(delete, id);
        }
        private Task PostId(string cmdName, string id) {
            return Post(new IdCommand(cmdName, id));
        }
        private Task Post(JsonContent cmd) {
            return Session.PostCommand(cmd, alarmsResource);
        }
        private AlarmModel ToModel(MusicAlarm json) {
            var when = json.when;
            var time = new DateTime(2013, 1, 1, when.hour, when.minute, 0);
            var days = when.days.Select(DayName).ToList();

            return new AlarmModel(this) {
                Id = json.id,
                IsOn = json.enabled,
                Time = time,
                EnabledDays = new ObservableCollection<object>(days),
                Track = AudioConversions.PimpTrackToMusicItem(json.job.track, null, Session.Username, Session.Password, Session.CloudServerID)
            };
        }
        private MusicAlarm ToJson(AlarmModel model) {
            var job = new PlaybackJob() {
                track = new PimpTrack(model.Track)
            };
            var when = new When() {
                hour = model.Time.Hour,
                minute = model.Time.Minute,
                days = model.EnabledDays.Select(o => ShortName((string)o)).ToList()
            };
            return new MusicAlarm() {
                id = model.Id,
                enabled = model.IsOn,
                job = job,
                when = when
            };
        }
        private ShortMusicAlarm ToShortJson(AlarmModel model) {
            var job = new ShortPlaybackJob() {
                track = model.Track.Id
            };
            var when = new When() {
                hour = model.Time.Hour,
                minute = model.Time.Minute,
                days = model.EnabledDays.Select(o => ShortName((string)o)).ToList()
            };
            return new ShortMusicAlarm() {
                id = model.Id,
                enabled = model.IsOn,
                job = job,
                when = when
            };
        }
        // My MusicPimp JSON API uses names "mon", ..., "sun", but the day names in .NET
        // depend on culture and whatnot. I need a way to convert from my representation
        // to the .NET representation and I believe these two functions do just that.
        private string DayName(string shortName) {
            return FindPair<string>(shortName, PimpJson.DayNames, AlarmModel.DayNames);
        }
        private string ShortName(string dayName) {
            return FindPair<string>(dayName, AlarmModel.DayNames, PimpJson.DayNames);
        }
        /// <summary>
        /// Looks up the index of item in sourceItems, and returns the item in targetItems with that index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="sourceItems"></param>
        /// <param name="targetItems"></param>
        /// <returns>the item from targetItems whose index equals sourceItems.indexOf(item)</returns>
        private T FindPair<T>(T item, IList<T> sourceItems, IList<T> targetItems) {
            var count = sourceItems.Count;
            for(int i = 0; i < count; ++i) {
                if(item.Equals(sourceItems[i])) {
                    return targetItems[i];
                }
            }
            throw new ArgumentException("Unknown item: " + item);
        }
    }
}
