using Mle.Exceptions;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public abstract class EndManagerBase : MessagingViewModel {

        /// <summary>
        /// Triggered when this endpoint manager has been fully initialized.
        /// 
        /// Initialization occurs asynchronously; the object is only initialized
        /// after Init() completes; simply constructing an object does not cut it.
        /// </summary>
        public event Action Initialized;
        public event Action<MusicEndpoint> ActiveEndpointChanged;
        public Provider Provider {
            get { return ProviderService.Instance; }
        }

        public EndpointsData EndpointsData {
            get { return Provider.EndpointsData; }
        }

        private MusicEndpoint musicEndpoint;
        public MusicEndpoint ActiveEndpoint {
            get { return musicEndpoint; }
            set { this.SetProperty(ref this.musicEndpoint, value); }
        }
        private int index = EndpointsData.THIS_DEVICE;
        public int Index {
            get {
                return index;
            }
            set {
                var nonNegativeValue = value < 0 ? EndpointsData.THIS_DEVICE : value;
                if(SetProperty(ref index, nonNegativeValue)) {
                    SaveIndex(nonNegativeValue);
                }
                ActiveEndpoint = GetEndpointWithIndex(Index);
                SetEndpoint(ActiveEndpoint);
            }
        }

        private SettingKey key;
        private ISettingsManager settings;

        public EndManagerBase(ISettingsManager settings, SettingKey endpointKey) {
            this.settings = settings;
            this.key = endpointKey;
            var savedIndex = LoadIndex();
            Index = savedIndex < 0 ? EndpointsData.THIS_DEVICE : savedIndex;
            EndpointsData.BeforeRemoval += e => {
                if(ActiveEndpoint == e) {
                    Index = EndpointsData.THIS_DEVICE;
                }
            };
            EndpointsData.EndpointModified += e => {
                if(ActiveEndpoint == e) {
                    ActivateEndpoint(e);
                }
            };
            ActiveEndpointChanged += async e => {
                try {
                    await CheckServerVersion(e);
                } catch(Exception) {

                }
            };
        }
        /// <summary>
        /// Sets the active endpoint to the supplied one. Explodes if the 
        /// endpoint does not exist or is not valid for this endpoint manager.
        /// 
        /// This method looks up the index of the supplied endpoint and sets
        /// the Index property.
        /// </summary>
        /// <param name="endpoint"></param>
        public abstract void SetActive(MusicEndpoint endpoint);
        protected virtual MusicEndpoint GetEndpointWithIndex(int index) {
            var attempt = TryGetEndpointWithIndex(index);
            return attempt != null ? attempt : EndpointsData.Endpoints.ElementAt(0);
        }
        protected virtual MusicEndpoint TryGetEndpointWithIndex(int index) {
            return TryGetEndpointWithIndex(index, EndpointsData.Endpoints);
        }
        protected MusicEndpoint TryGetEndpointWithIndex(int index, IList<MusicEndpoint> endpoints) {
            return (index >= 0 && endpoints.Count > index) ? endpoints.ElementAt(index) : null;
        }
        protected abstract void ActivateEndpoint(MusicEndpoint endpoint);
        protected void SetEndpoint(MusicEndpoint newEndpoint) {
            try {
                ActivateEndpoint(newEndpoint);
                OnActiveEndpointChanged(newEndpoint);
            } catch(NotFoundException) {
                Send("Unable to connect to endpoint " + newEndpoint.Name);
            } catch(ServerResponseException sre) {
                Send(sre.Message);
            } catch(Exception ex) {
                Send("There's a problem with music endpoint " + newEndpoint.Name + ". Please check your settings. " + ex.Message);
            }
        }
        protected async Task CheckServerVersion(MusicEndpoint endpoint) {
            if(await IsServerOld(endpoint)) {
                Send("New version available", "A new version of the MusicPimp server is available. The current version might not work well with this app. Get the new version from www.musicpimp.org. Enjoy!");
            }
        }
        private async Task<bool> IsServerOld(MusicEndpoint endpoint) {
            if(endpoint.EndpointType == EndpointTypes.MusicPimp) {
                var s = new PimpSessionBase(endpoint);
                // throws if expired
                var pingResponse = await s.GetPingAuth();
                var serverVersionStr = pingResponse.version;
                if(serverVersionStr == null) {
                    return true;
                } else {
                    Version version;
                    Version.TryParse(serverVersionStr, out version);
                    if(version < PimpSessionBase.SearchSupportingVersion) {
                        return true;
                    }
                }
            }
            return false;
        }

        private int LoadIndex() {
            return settings.Load<int>(key.ToString());
        }
        private void SaveIndex(int index) {
            settings.Save(key.ToString(), index);
        }
        private void OnActiveEndpointChanged(MusicEndpoint newEndpoint) {
            if(ActiveEndpointChanged != null) {
                ActiveEndpointChanged(newEndpoint);
            }
        }
        protected void OnInitialized() {
            if(Initialized != null) {
                Initialized();
            }
        }
    }
}
