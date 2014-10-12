using Mle.MusicPimp.Local;
using Mle.MusicPimp.Util;
using Mle.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class LimitsViewModel : MessagingViewModel {
        public static readonly int DEFAULT_CACHE_SIZE_GB = 1;

        protected LocalLibraryBase LocalLibrary { get; private set; }
        protected ISettingsManager Settings { get; private set; }

        public ICommand DeleteLocalCache { get; private set; }

        public int MaxCacheSizeGb {
            get {
                return Settings.Load<int>(SettingKey.maxCacheSizeGb.ToString(), DEFAULT_CACHE_SIZE_GB);
            }
            set {
                Settings.Save<int>(SettingKey.maxCacheSizeGb.ToString(), value);
                OnPropertyChanged("MaxCacheSizeGb");
            }
        }
        private double consumedGb = 0;
        public double ConsumedGb {
            get { return consumedGb; }
            set {
                if(SetProperty(ref this.consumedGb, value)) {
                    OnPropertyChanged("ConsumedGbFormatted");
                    OnPropertyChanged("LimitsExplanation");
                    OnPropertyChanged("ConsumedSummary");
                };
            }
        }
        public string ConsumedGbFormatted {
            get { return String.Format("{0:0.###}", ConsumedGb); }
        }
        public string LimitsExplanation {
            get {
                return "Currently using " + ConsumedGbFormatted + " GB. When the limit is reached, tracks in the cache are automatically removed.";
            }
        }
        public async Task CalculateConsumedGb() {
            var bytes = await LocalLibrary.ConsumedDiskSpaceBytes();
            ConsumedGb = 1.0D * bytes / 1024 / 1024 / 1024;
        }
        public string ConsumedSummary { get { return "using " + ConsumedGbFormatted + " / " + MaxCacheSizeGb + " GB"; } }
        public LimitsViewModel(LocalLibraryBase localLibrary, ISettingsManager settings) {
            LocalLibrary = localLibrary;
            Settings = settings;
            DeleteLocalCache = new AsyncUnitCommand(async () => {
                try {
                    await LocalLibrary.DeleteAll();
                    await Utils.SuppressAsync<Exception>(ProviderService.Instance.MusicItemsBase.ResetAndRefreshRoot);
                } catch(Exception e) {
                    Send("Unable to delete everything. Some track may be in use. Try again later. " + e.Message);
                }
                var t1 = CalculateConsumedGb();
            });

        }
    }
}
