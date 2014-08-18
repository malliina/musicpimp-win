using Mle.ViewModels;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Mle.MusicPimp.Database {
    public class MusicDataContext : DataContext {
        public static readonly string ConnectionString = "Data Source=isostore:/pimp12.sdf";
        public MusicDataContext() : base(ConnectionString) { }
        public Table<PlayFrequency> PlayFrequencies;
        public Table<PlaylistItem> Playlist;
        public Table<PlaylistIndex> PlayIndex;

        public static void CreateIfNotExists() {
            // Creates the database if it does not yet exist.
            using(var db = new MusicDataContext()) {
                if(db.DatabaseExists() == false) {
                    db.CreateDatabase();
                }
            }
        }
        public static void WithSubmit(Action<MusicDataContext> op) {
            using(var db = new MusicDataContext()) {
                op(db);
                db.SubmitChanges();
            }
        }
        public static T WithSubmit<T>(Func<MusicDataContext, T> op) {
            using(var db = new MusicDataContext()) {
                var ret = op(db);
                db.SubmitChanges();
                return ret;
            }
        }
        public static T WithConnection<T>(Func<MusicDataContext, T> op) {
            using(var db = new MusicDataContext()) {
                return op(db);
            }
        }
    }
    [Table]
    public class PlayFrequency : ChangingViewModelBase {
        private string songPath;
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "nvarchar(512) not null", CanBeNull = false)]
        public string SongPath {
            get { return songPath; }
            set { this.SetProperty(ref this.songPath, value); }
        }
        private int playCount;
        [Column(IsPrimaryKey = false, IsDbGenerated = false, DbType = "int not null", CanBeNull = false)]
        public int PlayCount {
            get { return playCount; }
            set { this.SetProperty(ref this.playCount, value); }
        }
    }
    [Table]
    public class PlaylistItem : ChangingViewModelBase {
        private int itemId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "int not null identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ItemId {
            get { return itemId; }
            set { SetProperty(ref this.itemId, value); }
        }
        private string source;
        [Column(DbType = "nvarchar(512) not null", CanBeNull = false)]
        public string Source {
            get { return source; }
            set { this.SetProperty(ref this.source, value); }
        }
        private string title;
        [Column(DbType = "nvarchar(512) not null", CanBeNull = false)]
        public string Title {
            get { return title; }
            set { this.SetProperty(ref this.title, value); }
        }
        private string artist;
        [Column(DbType = "nvarchar(512)")]
        public string Artist {
            get { return artist; }
            set { this.SetProperty(ref this.artist, value); }
        }
        private string album;
        [Column(DbType = "nvarchar(512)")]
        public string Album {
            get { return album; }
            set { this.SetProperty(ref this.album, value); }
        }
        private string path;
        [Column(DbType = "nvarchar(512) not null", CanBeNull = false)]
        public string Path {
            get { return path; }
            set { this.SetProperty(ref this.path, value); }
        }
        private long size;
        [Column(DbType = "bigint not null", CanBeNull = false)]
        public long Size {
            get { return size; }
            set { this.SetProperty(ref this.size, value); }
        }
        private double durationSeconds;
        [Column(DbType = "float not null", CanBeNull = false)]
        public double DurationSeconds {
            get { return durationSeconds; }
            set { this.SetProperty(ref this.durationSeconds, value); }
        }
    }
    [Table]
    public class PlaylistIndex : ChangingViewModelBase {
        private int itemId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "int not null identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ItemId {
            get { return itemId; }
            set { SetProperty(ref this.itemId, value); }
        }
        private int index;
        [Column(DbType = "int not null", CanBeNull = false)]
        public int Index {
            get { return index; }
            set { this.SetProperty(ref this.index, value); }
        }
    }
}
