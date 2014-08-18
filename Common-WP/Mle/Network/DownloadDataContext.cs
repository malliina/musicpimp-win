using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace Mle.Network {

    public class DownloadDataContext : DataContext {
        public static readonly string ConnectionString = "Data Source=isostore:/downloads.sdf";

        public DownloadDataContext() : base(ConnectionString) { }
        public Table<DownloadableRow> DownloadsTable;

        public static void CreateIfNotExists() {
            // Creates the database if it does not yet exist.
            using(var db = new DownloadDataContext()) {
                if(db.DatabaseExists() == false) {
                    db.CreateDatabase();
                }
            }
        }
        [Table]
        public class DownloadableRow {
            [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "nvarchar(1024) not null", CanBeNull = false)]
            public string Uri { get; set; }
            [Column(IsPrimaryKey = false, IsDbGenerated = false, DbType = "nvarchar(1024) not null", CanBeNull = false)]
            public string Destination { get; set; }
        }
        public static T WithConnection<T>(Func<DownloadDataContext, T> op) {
            using(var db = new DownloadDataContext()) {
                return op(db);
            }
        }
        public static void WithSubmit(Action<DownloadDataContext> op) {
            using(var db = new DownloadDataContext()) {
                op(db);
                db.SubmitChanges();
            }
        }
        public static List<Downloadable> Downloadables() {
            return WithConnection<List<Downloadable>>(db => {
                var items = from DownloadableRow item in db.DownloadsTable select item;
                if(items.Any()) {
                    return items.Select(row2downloadable).ToList();
                } else {
                    return new List<Downloadable>();
                }
            });
        }

        public static void Add(Downloadable item) {
            AddAll(new List<Downloadable>() { item });
        }
        public static void AddAll(IEnumerable<Downloadable> items) {
            WithSubmit(db => {
                var newRows = items.Select(downloadable2row).ToList();
                var newUris = newRows.Select(r => r.Uri).ToList();
                var alreadyAddedRows = (from DownloadableRow row in db.DownloadsTable where newUris.Contains(row.Uri) select row.Uri).ToList();
                var distinctNewRows = newRows.Where(newRow => !alreadyAddedRows.Any(uri => uri == newRow.Uri)).ToList();
                if(distinctNewRows.Count > 0)
                    db.DownloadsTable.InsertAllOnSubmit(distinctNewRows);
            });
        }
        /// <summary>
        /// Pops the next downloadables from persistent storage.
        /// </summary>
        /// <returns>the next downloadables, or an empty list if the downloads list is empty</returns>
        public static List<Downloadable> Pop(int count = 10) {
            return WithConnection(db => {
                var rows = from DownloadableRow item in db.DownloadsTable select item;
                if(rows.Any()) {
                    var popped = rows.Take(count);
                    var rowCount = rows.Count();
                    //var poppedCount = rows.Count();
                    //Debug.WriteLine("Popped " + poppedCount + " downloadables, " + (rowCount - poppedCount) + " remain in queue.");
                    db.DownloadsTable.DeleteAllOnSubmit(popped);
                    db.SubmitChanges();
                    return popped.Select(row2downloadable).ToList();
                } else {
                    return new List<Downloadable>();
                }
            });
        }
        public static void Clear() {
            WithSubmit(db => {
                var allRows = (from DownloadableRow row in db.DownloadsTable select row);
                db.DownloadsTable.DeleteAllOnSubmit(allRows);
            });
        }
        private static DownloadableRow downloadable2row(Downloadable item) {
            return new DownloadableRow {
                Uri = item.Source.OriginalString,
                Destination = item.Destination
            };
        }
        private static Downloadable row2downloadable(DownloadableRow row) {
            return new Downloadable(new Uri(row.Uri, UriKind.Absolute), row.Destination);
        }

    }
}
