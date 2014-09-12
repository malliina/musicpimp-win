using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public interface IAlarmClient {
        SimplePimpSession Session { get; }
        IDateTimeHelper TimeHelper { get; }
        /// <summary>
        /// Useful for testing purposes.
        /// </summary>
        /// <param name="alarmId"></param>
        /// <returns></returns>
        Task StartPlayback(string alarmId);
        Task StopPlayback();
        Task<IEnumerable<AlarmModel>> Alarms();
        Task<IEnumerable<MusicItem>> Tracks();
        Task<IEnumerable<MusicItem>> Search(string term);
        /// <summary>
        /// Adds or updates the supplied schedule. The server will interpret
        /// a schedule with a non-null id member as an update, otherwise add.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        Task Save(AlarmModel schedule);
        Task Remove(string alarmId);
    }
    public abstract class BaseMusicAlarm : JsonContent {
        public string id { get; set; }
        public When when { get; set; }
        public bool enabled { get; set; }
    }
    public class MusicAlarm : BaseMusicAlarm {
        public PlaybackJob job { get; set; }
    }
    public class ShortMusicAlarm : BaseMusicAlarm {
        public ShortPlaybackJob job { get; set; }
    }
    public class PlaybackJob : JsonContent {
        public PimpTrack track { get; set; }
    }
    public class ShortPlaybackJob : JsonContent {
        public string track { get; set; }
    }
    public class When : JsonContent {
        public int hour { get; set; }
        public int minute { get; set; }
        public List<string> days { get; set; }
    }
    public class AlarmCommand : SimpleCommand {
        public JsonContent ap { get; set; }
        public AlarmCommand(string command, JsonContent alarm)
            : base(command) {
            this.ap = alarm;
        }
    }

}
