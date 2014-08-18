using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Local;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    /// <summary>
    /// a manager of music endpoints, one endpoint being active
    /// </summary>
    public abstract class EndpointsData : WebAwareLoading {
        public abstract string Encrypt(string plain);
        public abstract string Decrypt(string encrypted);
        protected abstract void SaveToStorage(IEnumerable<MusicEndpoint> data);
        protected abstract ObservableCollection<MusicEndpoint> LoadEndpoints();

        public static readonly int NO_ENDPOINT = -1;
        public static readonly int THIS_DEVICE = 0;
        private static MusicEndpoint thisDevice;
        public static MusicEndpoint ThisDevice {
            get {
                if(thisDevice == null) {
                    thisDevice = new LocalDeviceEndpoint();
                }
                return thisDevice;
            }
        }
        public ObservableCollection<MusicEndpoint> Endpoints { get; private set; }
        public IEnumerable<MusicEndpoint> RemoteEndpoints {
            get { return Endpoints.Skip(1).ToList(); }
        }
        public ICommand RemoveCommand { get; private set; }

        public event Action<MusicEndpoint> EndpointAdded;
        public event Action<MusicEndpoint> EndpointRemoved;
        public event Action<MusicEndpoint> EndpointModified;
        public event Action<MusicEndpoint> BeforeRemoval;

        public EndpointsData() {
            RemoveCommand = new AsyncDelegateCommand<MusicEndpoint>(RemoveAndSave);
            Endpoints = new ObservableCollection<MusicEndpoint>();
            Endpoints.Insert(0, ThisDevice);
            Endpoints.CollectionChanged += (s, e) => OnPropertyChanged("RemoteEndpoints");
        }
        public void Init() {
            WithLoading(() => {
                var loaded = LoadEndpoints();
                // ensure each endpoint has an ID
                var withIDs = EnsureAllHaveIDs(loaded);
                foreach(var e in withIDs) {
                    Endpoints.Add(e);
                }
            });
        }

        private IEnumerable<MusicEndpoint> EnsureAllHaveIDs(IEnumerable<MusicEndpoint> endpoints) {
            var modifiedAnyId = false;
            // This is for backwards compat.
            var withIDs = endpoints.Select(e => {
                if(String.IsNullOrWhiteSpace(e.Id)) {
                    e.Id = Guid.NewGuid().ToString();
                    modifiedAnyId = true;
                }
                return e;
            }).ToList();
            if(modifiedAnyId) {
                SaveToStorage(withIDs);
            }
            return withIDs;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>the index of the added endpoint</returns>
        public async Task AddAndSave(MusicEndpoint endpoint) {
            var sameName = Endpoints.FirstOrDefault(e => e.Name == endpoint.Name);
            if(sameName != null) {
                ThrowAlreadyExists(endpoint.Name);
            }
            Endpoints.Add(endpoint);
            await persist();
            OnEndpointAdded(endpoint);
        }
        /// <summary>
        /// Saves changes to all endpoints.
        /// </summary>
        /// <param name="endpoint">the modified endpoint</param>
        public async Task SaveChanges(MusicEndpoint endpoint) {
            if(Endpoints.Count(e => e.Name == endpoint.Name) > 1) {
                ThrowAlreadyExists(endpoint.Name);
            }
            await persist();
            OnEndpointModified(endpoint);
        }
        private void ThrowAlreadyExists(string name) {
            throw new PimpException("An endpoint named " + name + " already exists. Please rename.");
        }
        public virtual async Task RemoveAndSave(MusicEndpoint endpoint) {
            OnBeforeRemoval(endpoint);
            Endpoints.Remove(endpoint);
            // TODO remove credential from vault
            await persist();
            OnEndpointRemoved(endpoint);
        }
        public Task persist() {
            return TaskEx.Run(() => {
                var serializeThis = Endpoints.Skip(1);
                SaveToStorage(serializeThis);
            });
        }
        /// <summary>
        /// Creates and returns a new list where each password is encrypted; does not mutate the parameter.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>a copy of items, encrypted</returns>
        protected IEnumerable<MusicEndpoint> encrypt(IEnumerable<MusicEndpoint> items) {
            var copies = items.Select(item => new MusicEndpoint(item)).ToList();
            MapEachPass(copies, Encrypt);
            return copies;
        }
        protected void decrypt(IEnumerable<MusicEndpoint> items) {
            MapEachPass(items, Decrypt);
        }
        private void MapEachPass(IEnumerable<MusicEndpoint> items, Func<string, string> passFunc) {
            foreach(MusicEndpoint e in items) {
                e.Password = passFunc(e.Password);
            }
        }
        public MusicEndpoint withNameOrElse(string name, MusicEndpoint orElse) {
            if(name == null || name == String.Empty) {
                return orElse;
            } else {
                var tmp = Endpoints.FirstOrDefault(item => item.Name == name);
                if(tmp == null)
                    return orElse;
                else
                    return tmp;
            }
        }
        private void OnEndpointModified(MusicEndpoint e) {
            if(EndpointModified != null) {
                EndpointModified(e);
            }
        }
        private void OnEndpointAdded(MusicEndpoint e) {
            if(EndpointAdded != null) {
                EndpointAdded(e);
            }
        }
        private void OnEndpointRemoved(MusicEndpoint e) {
            if(EndpointRemoved != null) {
                EndpointRemoved(e);
            }
        }
        private void OnBeforeRemoval(MusicEndpoint e) {
            if(BeforeRemoval != null) {
                BeforeRemoval(e);
            }
        }
    }
}
