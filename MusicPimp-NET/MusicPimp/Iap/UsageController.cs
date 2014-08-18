using Mle.MusicPimp.Util;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mle.MusicPimp.Iap {
    public class UsageController {
        private static UsageController instance = null;
        public static UsageController Instance {
            get {
                if(instance == null)
                    instance = new UsageController();
                return instance;
            }
        }
        public bool IsIapEnabled { get { return true; } }
        public bool IsUnlimitedPlaybackAllowed { get; set; }

        private static readonly string
            savedStackKey = "usage_stack_setting",
            playbackCountKey = "usage_tracks_played",
            premiumEnabledKey = "usage_premium";
        private static readonly DateTime StartOfEpoch = new DateTime(1970, 1, 1);
        private int upFrontAllowance = 10;
        private int tracksPlayedInTotal;
        private int oneDayLimit = 3;
        private Stack<double> stack;
        private int maxStackSize = 100;
        private ISettingsManager settings = ProviderService.Instance.SettingsManager;

        protected UsageController() {
            var stackJson = settings.Load<string>(savedStackKey, null);
            if(stackJson != null) {
                stack = Json.Deserialize<Stack<double>>(stackJson);
            } else {
                stack = new Stack<double>();
            }
            tracksPlayedInTotal = settings.Load<int>(playbackCountKey, 0);
            IsUnlimitedPlaybackAllowed = settings.Load<bool>(premiumEnabledKey, false) || PremiumHelper.Instance.HasPremium();
            //IsUnlimitedPlaybackAllowed = PremiumHelper.Instance.HasPremium();
        }
        public void PlaybackStarted() {
            stack.Push((DateTime.UtcNow - StartOfEpoch).TotalMilliseconds);
            if(stack.Count > maxStackSize) {
                stack = new Stack<double>(stack.Take(maxStackSize));
            }
            settings.Save(savedStackKey, Json.SerializeToString(stack));

            tracksPlayedInTotal += 1;
            settings.Save(playbackCountKey, tracksPlayedInTotal);
        }
        public bool IsPlaybackAllowed() {
            return IsPlaybackAllowed(stack);
        }
        private bool IsPlaybackAllowed(IEnumerable<double> times) {
            return !IsIapEnabled || IsUnlimitedPlaybackAllowed || tracksPlayedInTotal < upFrontAllowance || times.Count() < oneDayLimit || !IsWithinOneDay(times.Take(oneDayLimit));
        }
        private bool IsWithinOneDay(IEnumerable<double> times) {
            var now = DateTime.UtcNow;
            return times.All(time => (now - StartOfEpoch.AddMilliseconds(time)).TotalDays < 1);
        }
        public void EnablePremium() {
            IsUnlimitedPlaybackAllowed = true;
            settings.Save(premiumEnabledKey, true);
        }
    }
}
