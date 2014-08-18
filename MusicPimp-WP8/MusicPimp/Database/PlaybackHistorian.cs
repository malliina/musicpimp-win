using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Database {

    /// <summary>
    /// a dictionary of songs -> playcounts
    /// </summary>
    public class PlaybackHistorian : PlaybackHistorianBase {
        private static PlaybackHistorian instance = null;
        public static PlaybackHistorian Instance {
            get {
                if(instance == null)
                    instance = new PlaybackHistorian();
                return instance;
            }
        }
        public static ObservableCollection<PlayFrequency> MostPlayed(int count = 100) {
            return MusicDataContext.WithConnection(db => {
                var mostPlayed = (from PlayFrequency playFreq in db.PlayFrequencies
                                  orderby playFreq.PlayCount descending
                                  select playFreq).Take(count);
                return new ObservableCollection<PlayFrequency>(mostPlayed);
            });
        }
        public override Task AddPlayCount(string songPath) {
            return TaskEx.Run(() => {
                MusicDataContext.WithSubmit(db => {
                    var currentEntry = from PlayFrequency playFreq in db.PlayFrequencies
                                       where playFreq.SongPath == songPath
                                       select playFreq;
                    if(currentEntry.Any()) {
                        // if the entry already exists
                        foreach(PlayFrequency pf in currentEntry) {
                            pf.PlayCount = pf.PlayCount + 1;
                        }
                    } else {
                        // if it doesn't exist
                        var newEntry = new PlayFrequency() { SongPath = songPath, PlayCount = 1 };
                        db.PlayFrequencies.InsertOnSubmit(newEntry);
                    }
                });
            });
        }
        public override Task DeleteSongEntry(string songPath) {
            return TaskEx.Run(() => {
                MusicDataContext.WithSubmit(db => {
                    var entry = from PlayFrequency playFreq in db.PlayFrequencies
                                where playFreq.SongPath == songPath
                                select playFreq;
                    foreach(PlayFrequency pf in entry) {
                        db.PlayFrequencies.DeleteOnSubmit(pf);
                    }
                });
            });
        }
        public override Task<List<string>> LeastPlayed(int count = 20) {
            return TaskEx.Run(() => {
                return MusicDataContext.WithConnection(db => {
                    var leastPlayed = (from PlayFrequency playFreq in db.PlayFrequencies
                                       orderby playFreq.PlayCount ascending
                                       select playFreq.SongPath).Take(count);
                    return new List<string>(leastPlayed);
                });
            });
        }
    }
}
