using Mle.Devices;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Mle.MusicPimp.Beam {
    public class BeamBarcodeHandler {

        private BarcodeReaderBase reader;

        private PlayerManager PlayerManager {
            get { return ProviderService.Instance.PlayerManager; }
        }
        private UiUtils uiUtils;
        private Action<string> codeHandler = null;

        public BeamBarcodeHandler(BarcodeReaderBase reader, UiUtils uiUtils) {
            this.reader = reader;
            this.uiUtils = uiUtils;
            codeHandler = BeamQRReader_CodeAvailable;
            reader.CodeAvailable += codeHandler;
        }

        private async void BeamQRReader_CodeAvailable(string code) {
            reader.CodeAvailable -= codeHandler;
            try {
                var beamJson = Json.Deserialize<BeamJson>(code);
                var endpointsManager = PlayerManager.EndpointsData;
                var currentBeam = endpointsManager.Endpoints.FirstOrDefault(e => e.Name == BeamEndpoint.BeamName);
                Dispose();
                await uiUtils.OnUiThreadAsync(async () => {
                    // there's at most one MusicBeamer endpoint at a time; mutates the existing one if any, otherwise add
                    if(currentBeam != null) {
                        FillIn(beamJson, currentBeam);
                        await endpointsManager.SaveChanges(currentBeam);
                        // SaveChanges will trigger EndpointModified, which will be caught by EndManagerBase, which will reactivate the endpoint
                        if(PlayerManager.ActiveEndpoint.Name != currentBeam.Name) {
                            PlayerManager.SetActive(currentBeam);
                        }
                    } else {
                        var endpoint = CreateBeamEndpoint(beamJson);
                        await endpointsManager.AddAndSave(endpoint);
                        PlayerManager.SetActive(endpoint);
                    }
                    PageNavigationService.Instance.Navigate(PageNames.LIBRARY);
                });
            } catch(JsonReaderException) {
                MessagingService.Instance.Send("The barcode does not seem to be a valid MusicBeamer barcode.");
                reader.CodeAvailable += codeHandler;
            } catch(Exception) {
                // might throw if json parsing fails
                reader.CodeAvailable += codeHandler;
            }
        }
        private MusicEndpoint FillIn(BeamJson src, MusicEndpoint dest) {
            dest.Username = src.user;
            dest.Server = src.host;
            dest.Port = src.supports_tls ? src.ssl_port : src.port;
            dest.Protocol = src.supports_tls ? Protocols.https : Protocols.http;
            return dest;
        }
        private MusicEndpoint CreateBeamEndpoint(BeamJson details) {
            return FillIn(details, new BeamEndpoint("", 80, "", false));
        }
        public void Dispose() {
            Utils.Suppress<Exception>(() => {
                reader.CodeAvailable -= BeamQRReader_CodeAvailable;
                reader.Dispose();
            });
        }
    }
    public class BeamJson {
        public string host { get; set; }
        public string user { get; set; }
        public int port { get; set; }
        public int ssl_port { get; set; }
        public bool supports_plaintext { get; set; }
        public bool supports_tls { get; set; }
    }
}
