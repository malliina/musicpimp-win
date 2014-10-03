using Mle.Exceptions;
using Mle.Messaging;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public class MasterChildLibrary : MultiLibrary {
        public override ObservableCollection<MusicLibrary> Libraries { get; protected set; }
        private MusicLibrary master;
        private MusicLibrary child;
        public MasterChildLibrary(MusicLibrary master, MusicLibrary child) {
            this.master = master;
            this.child = child;
            master.NewItemsLoaded += OnSubLibraryItemsLoaded;
            child.NewItemsLoaded += OnSubLibraryItemsLoaded;
            PrepareLibraries();
        }

        private void OnSubLibraryItemsLoaded(IEnumerable<MusicItem> items) {
            OnNewItemsLoaded(items);
        }
        public override async Task Ping() {
            await master.Ping();
            IsOnline = true;
        }
        private void PrepareLibraries() {
            Libraries = new ObservableCollection<MusicLibrary> { child, master };
            var t = RemoveMasterIfPingFails();
        }
        /// <summary>
        /// Checks that the master answers to ping, syncing it if necessary, but disables it if it does not seem to work. 
        /// </summary>
        /// <returns></returns>
        private async Task RemoveMasterIfPingFails() {
            var isUnreachable = false;
            try {
                await master.Ping();
                IsOnline = true;
            } catch(NotFoundException) {
                isUnreachable = true;
            } catch(ConnectivityException) {
                isUnreachable = true;
            } catch(Exception) {
                DisableMaster();
            }
            if(isUnreachable) {
                await DisableIfSyncFails();
            }
        }
        private async Task DisableIfSyncFails() {
            var libraryManager = ProviderService.Instance.LibraryManager;
            var activeLibraryEndpoint = libraryManager.ActiveEndpoint;
            if(activeLibraryEndpoint.EndpointType == EndpointTypes.MusicPimp) {
                var syncWorked = await EndpointScanner.Instance.SyncEndpoint(activeLibraryEndpoint);
                if(syncWorked) {
                    IsOnline = true;
                } else {
                    DisableMaster();
                }
            }
        }
        private void DisableMaster() {
            Libraries = new ObservableCollection<MusicLibrary> { child };
            IsOnline = false;
        }
    }
}
