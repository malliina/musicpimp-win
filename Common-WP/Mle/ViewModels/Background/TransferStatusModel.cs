using Microsoft.Phone.BackgroundTransfer;
using System;

namespace Mle.ViewModels.Background {
    public class TransferStatusModel : ViewModelBase {
        private bool waitingForExternalPower;
        public bool WaitingForExternalPower {
            get { return waitingForExternalPower; }
            set { this.SetProperty(ref this.waitingForExternalPower, value); }
        }
        public string WaitingForExternalPowerMessage {
            get { return "You have one or more file transfers waiting for external power. Connect your device to external power to continue transferring. "; }
        }
        private bool waitingForExternalPowerDueToBatterySaverMode;
        public bool WaitingForExternalPowerDueToBatterySaverMode {
            get { return waitingForExternalPowerDueToBatterySaverMode; }
            set { this.SetProperty(ref this.waitingForExternalPowerDueToBatterySaverMode, value); }
        }
        public string WaitingForExternalPowerDueToBatterySaverModeMessage {
            get { return "You have one or more file transfers waiting for external power. Connect your device to external power or disable Battery Saver Mode to continue transferring. "; }
        }
        private bool waitingForNonVoiceBlockingNetwork;
        public bool WaitingForNonVoiceBlockingNetwork {
            get { return waitingForNonVoiceBlockingNetwork; }
            set { this.SetProperty(ref this.waitingForNonVoiceBlockingNetwork, value); }
        }
        public string WaitingForNonVoiceBlockingNetworkMessage {
            get { return "You have one or more file transfers waiting for a network that supports simultaneous voice and data. "; }
        }
        private bool waitingForWiFi;
        public bool WaitingForWiFi {
            get { return waitingForWiFi; }
            set { this.SetProperty(ref this.waitingForWiFi, value); }
        }
        public string WaitingForWiFiMessage {
            get { return "You have one or more file transfers waiting for a WiFi connection. Connect your device to a WiFi network to continue transferring. "; }
        }
        public string CombinedMessage {
            get {
                var msg = String.Empty;
                if(WaitingForExternalPower) {
                    msg += WaitingForExternalPowerMessage;
                }
                if(WaitingForExternalPowerDueToBatterySaverMode) {
                    msg += WaitingForExternalPowerDueToBatterySaverModeMessage;
                }
                if(WaitingForNonVoiceBlockingNetwork) {
                    msg += WaitingForNonVoiceBlockingNetworkMessage;
                }
                if(WaitingForWiFi) {
                    msg += WaitingForWiFiMessage;
                }
                return msg;
            }
        }
        public bool IsWaiting { get { return CombinedMessage != String.Empty; } }
        public void Reset() {
            WaitingForExternalPower = false;
            WaitingForExternalPowerDueToBatterySaverMode = false;
            WaitingForNonVoiceBlockingNetwork = false;
            WaitingForWiFi = false;
            UpdateProps();
        }
        public void UpdateProps() {
            OnPropertyChanged("CombinedMessage");
            OnPropertyChanged("IsWaiting");
        }
        public void Update(TransferStatus status) {
            switch(status) {
                case TransferStatus.WaitingForExternalPower:
                    WaitingForExternalPower = true;
                    break;
                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    WaitingForExternalPowerDueToBatterySaverMode = true;
                    break;
                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    WaitingForNonVoiceBlockingNetwork = true;
                    break;
                case TransferStatus.WaitingForWiFi:
                    WaitingForWiFi = true;
                    break;
            }
        }

        private string DetermineFeedback(TransferStatus status) {
            switch(status) {
                case TransferStatus.WaitingForExternalPower:
                    return WaitingForExternalPowerMessage;
                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    return WaitingForExternalPowerDueToBatterySaverModeMessage;
                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    return WaitingForNonVoiceBlockingNetworkMessage;
                case TransferStatus.WaitingForWiFi:
                    return WaitingForWiFiMessage;
                default:
                    return null;
            }
        }
    }
}
