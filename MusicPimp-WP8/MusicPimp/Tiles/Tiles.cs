using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mle.Collections;

namespace Mle.MusicPimp.Tiles {

    public class Tiles : TileAndToastManager {
        private static Tiles instance = null;
        public static Tiles Instance {
            get {
                if(instance == null)
                    instance = new Tiles();
                return instance;
            }
        }

        // true or false?
        private readonly static Mutex mutex = new Mutex(false, "tileMutex");

        private Uri defaultTileUri;

        protected Tiles()
            : base(PhonePlayerManager.Instance,PhoneCoverService.Instance, new Uri("Assets/guitar-1080x1440-white.png", UriKind.RelativeOrAbsolute)) {
            defaultTileUri = new Uri("/Assets/Tiles/guitar-336x336-medium.png", UriKind.Relative);
        }
        public override Task SetBackground(Uri uri) {
            return PimpViewModel.Instance.SetBackground(uri);
        }
        protected async Task<Uri> GetCoverUriOrDefault(MusicItem track) {
            mutex.WaitOne();
            Uri maybeCover = null;
            try {
                maybeCover = await Covers.TryGetCover(track);
            } finally {
                mutex.ReleaseMutex();
            }
            return maybeCover != null ? PhoneTileUtil.TileUri(maybeCover) : defaultTileUri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns>"track by artist" or just "track" if the artist is null or empty</returns>
        protected string GetLongContent(MusicItem track) {
            var artist = track.Artist;
            if(artist == null || artist == String.Empty) {
                return track.Name;
            } else {
                return track.Name + " by " + track.Artist;
            }
        }
        public override async Task Update(MusicItem track) {
            var imageUri = await GetCoverUriOrDefault(track);
            PhoneTileUtil.UpdateTileCyclic(GetLongContent(track), new List<Uri> { imageUri });
        }
        public override Task UpdateNoTrack() {
            return PhoneTileUtil.UpdateNoTrack();
        }
        
    }
}
