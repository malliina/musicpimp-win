using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.Util;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mle.MusicPimp.ViewModels {
    public abstract class LibraryManager : EndManagerBase {
        public LocalMusicLibrary LocalLibrary {
            get { return Provider.LocalLibrary; }
        }
        // TODO set this
        public MusicLibrary MusicProvider { get; private set; }

        public IList<MusicEndpoint> Endpoints {
            get { return EndpointsData.Endpoints.Where(e => e.SupportsLibrary).ToList(); }
        }
        protected override MusicEndpoint TryGetEndpointWithIndex(int index) {
            return TryGetEndpointWithIndex(index, Endpoints);
        }

        public LibraryManager(ISettingsManager settings)
            : base(settings, SettingKey.audioSourceIndex) {

            EndpointsData.Endpoints.CollectionChanged += (s, e) => {
                OnPropertyChanged("Endpoints");
                // if the active endpoint was deleted, it is already set to the local device right now
                if(ActiveEndpoint != null) {
                    SetActive(ActiveEndpoint);
                }
            };
        }
        public override void SetActive(MusicEndpoint endpoint) {
            Index = Endpoints.IndexOf(endpoint);
        }
        public MusicLibrary BuildMusicLibrary(MusicEndpoint source) {
            switch(source.EndpointType) {
                case EndpointTypes.Local:
                    return LocalLibrary;
                case EndpointTypes.MusicPimp:
                case EndpointTypes.MusicPimpWeb:
                    return new MasterChildLibrary(Provider.NewPimpLibrary(source), Provider.LocalLibrary);
                case EndpointTypes.Subsonic:
                    return new SubsonicLibrary(new SubsonicSession(source));
                default:
                    throw new PimpException("Unsupported music library type: " + source.EndpointType);
            }
        }
        protected override void ActivateEndpoint(MusicEndpoint newAudioSource) {
            MusicProvider = BuildMusicLibrary(newAudioSource);
        }
    }
}
