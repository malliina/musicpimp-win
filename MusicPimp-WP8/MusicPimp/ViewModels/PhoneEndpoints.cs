using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Mle.Exceptions;
using Mle.IO;
using Mle.Messaging;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class PhoneEndpoints : EndpointsData {
        private static string PersistentFile = "Endpoints7.json";

        private static PhoneEndpoints instance = null;
        public static PhoneEndpoints Instance {
            get {
                if(instance == null) {
                    instance = BuildEndpoints();
                }
                return instance;
            }
        }
        private static PhoneEndpoints BuildEndpoints() {
            var e = new PhoneEndpoints();
            // prevents subclass invocation from superclass
            e.Init();
            return e;
        }
        public override async Task RemoveAndSave(MusicEndpoint endpoint) {
            //var pushHelper = new PushClient(endpoint);
            //var t = pushHelper.TryDeregister();
            await base.RemoveAndSave(endpoint);
        }
        private bool isSearching = false;
        public bool IsSearching {
            get { return isSearching; }
            set { SetProperty(ref this.isSearching, value); }
        }
        public ICommand SearchAnyServer { get; private set; }
        protected PhoneEndpoints() {
            SearchAnyServer = new AsyncUnitCommand(SearchServer);
        }
        public async Task SearchServer() {
            await WithExceptionEvents(async () => {
                try {
                    if(!DeviceNetworkInformation.IsWiFiEnabled) {
                        ShowBox("WiFi is not enabled. Please enable WiFi or add the endpoint manually instead.");
                    } else {
                        IsSearching = true;
                        var endpoint = await EndpointScanner.Instance.SearchAnyServer(NetworkDevice.Instance.Addresses());
                        AskForPasswordThenAdd(endpoint);
                    }
                } catch(NoResultsException) {
                    ShowBox("No endpoint was found. Please add the endpoint manually instead.");
                } catch(TimeoutException) {
                    ShowBox("Search timed out. Please add the endpoint manually instead.");
                } finally {
                    IsSearching = false;
                }
            });
        }
        private void ShowBox(string msg) {
            MessagingService.Instance.Send(msg);
        }
        protected override ObservableCollection<MusicEndpoint> LoadEndpoints() {
            return FileUtils.WithStorage(s => {
                if(s.FileExists(PersistentFile)) {
                    var endpoints = JsonUtils.DeserializeFile<ObservableCollection<MusicEndpoint>>(PersistentFile);
                    decrypt(endpoints);
                    return endpoints;
                } else {
                    return new ObservableCollection<MusicEndpoint>();
                }
            });
        }
        public void AskForPasswordThenAdd(MusicEndpoint endpoint) {
            var passwordBox = new PasswordBox();
            var passwordMessageBox = new CustomMessageBox() {
                Caption = "Password required",
                Message = "Found a MusicPimp server at " + endpoint.Server + ":" + endpoint.Port + ". Please enter the password.",
                Content = passwordBox,
                LeftButtonContent = "submit",
                RightButtonContent = "cancel"
            };
            passwordMessageBox.Dismissed += async (s, e) => {
                switch(e.Result) {
                    case CustomMessageBoxResult.LeftButton:
                        var pass = passwordBox.Password;
                        endpoint.Password = pass;
                        try {
                            await new PhonePimpSession(endpoint).PingAuth();
                            await AddAndSave(endpoint);
                            PhoneLibraryManager.Instance.SetActive(endpoint);
                            //Debug.WriteLine("Added endpoint: " + endpoint);
                        } catch(UnauthorizedException) {
                            ShowBox("Invalid password. No endpoint was added.");
                        } catch(PimpException pe) {
                            ShowBox(pe.Message);
                        } catch(Exception) {
                            ShowBox("An error occurred. Please try adding the endpoint manually.");
                        }
                        break;
                    case CustomMessageBoxResult.RightButton:
                        // user clicked cancel
                        break;
                    case CustomMessageBoxResult.None:
                        break;
                }
            };
            passwordMessageBox.Show();
        }

        protected override void SaveToStorage(IEnumerable<MusicEndpoint> data) {
            var encrypted = encrypt(data);
            JsonUtils.Instance.SerializeToFileSync(encrypted, PersistentFile);
        }
        public override string Encrypt(string plain) {
            return StringUtil.protectToBase64(plain);
        }
        public override string Decrypt(string encrypted) {
            return StringUtil.unprotectFromBase64(encrypted);
        }
    }
}
