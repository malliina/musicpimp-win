using Mle.Roaming.Network;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Mle.MusicPimp.ViewModels {

    public class StoreEndpoints : EndpointsData {
        private static readonly string CredPrefix = "endpoint-";
        private static StoreEndpoints instance = null;
        public static StoreEndpoints Instance {
            get {
                if (instance == null) {
                    instance = BuildEndpoints();
                }
                return instance;
            }
        }
        private static StoreEndpoints BuildEndpoints() {
            var e = new StoreEndpoints();
            e.Init();
            return e;
        }

        private static readonly string RoamingSettingKey = "endpoints";
        private RoamingSettings roamingSettings;

        protected StoreEndpoints() {
            roamingSettings = new RoamingSettings();
        }
        protected override ObservableCollection<MusicEndpoint> LoadEndpoints() {
            var endpoints = roamingSettings.LoadJson<ObservableCollection<MusicEndpoint>>(
                RoamingSettingKey,
                def: new ObservableCollection<MusicEndpoint>());
            LoadCredentials(endpoints);
            return endpoints;
        }
        public override async Task RemoveAndSave(MusicEndpoint endpoint) {
            // removes credential from credential store
            var vault = new PasswordVault();
            var cred = GetCredential(vault.RetrieveAll(), endpoint);
            if (cred != null) {
                vault.Remove(cred);
            }
            await base.RemoveAndSave(endpoint);
        }
        /// <summary>
        /// Saves the settings to roaming application data.
        /// </summary>
        /// <param name="data"></param>
        protected override void SaveToStorage(IEnumerable<MusicEndpoint> data) {
            var withoutCreds = SaveCredentials(data);
            roamingSettings.SaveJson(RoamingSettingKey, withoutCreds);
        }
        private void LoadCredentials(IEnumerable<MusicEndpoint> data) {
            var vault = new PasswordVault();
            // does not retrieve the passwords! funny one!
            var credentials = vault.RetrieveAll();
            foreach (var e in data) {
                try {
                    var cred = GetCredential(credentials, e);
                    if (cred != null) {
                        e.Username = cred.UserName;
                        e.Password = vault.Retrieve(cred.Resource, cred.UserName).Password;
                    } else {
                        ClearCredentials(e);
                    }
                } catch (Exception) {
                    ClearCredentials(e);
                }
            }
        }
        private PasswordCredential GetCredential(IReadOnlyList<PasswordCredential> allCreds, MusicEndpoint endpoint) {
            return allCreds.FirstOrDefault(c => c.Resource == CredPrefix + endpoint.Name);
        }
        private void ClearCredentials(MusicEndpoint e) {
            e.Username = string.Empty;
            e.Password = string.Empty;
        }
        private IEnumerable<MusicEndpoint> SaveCredentials(IEnumerable<MusicEndpoint> data) {
            var ret = new List<MusicEndpoint>();
            var vault = new PasswordVault();
            foreach (var e in data) {
                if (e.Name != string.Empty && e.Username != string.Empty && e.Password != string.Empty) {
                    vault.Add(new PasswordCredential(CredPrefix + e.Name, e.Username, e.Password));
                    ret.Add(new MusicEndpoint() {
                        Name = e.Name,
                        Server = e.Server,
                        Port = e.Port,
                        EndpointType = e.EndpointType,
                        Protocol = e.Protocol
                    });
                }
            }
            return ret;
        }
        public override string Encrypt(string plain) {
            return SecurityUtil.Encrypt(plain).Result;
        }
        public override string Decrypt(string encrypted) {
            return SecurityUtil.Decrypt(encrypted).Result;
        }
    }
}
