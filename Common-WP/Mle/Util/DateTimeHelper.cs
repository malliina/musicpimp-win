using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.LocalizedResources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mle.Util {
    /// <summary>
    /// Ripped from Windows Phone Toolkit's RecurringDaysPicker.
    /// </summary>
    public class DateTimeHelper : IDateTimeHelper {
        private static DateTimeHelper instance = null;
        public static DateTimeHelper Instance {
            get {
                if(instance == null)
                    instance = new DateTimeHelper();
                return instance;
            }
        }
        public static readonly string[] DayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
        private const string CommaSpace = ", ";
        private const string EnglishLanguage = "en";
        private string[] ShortestDayNames = CultureInfo.CurrentCulture.DateTimeFormat.ShortestDayNames;
        private string _fallbackValueStringFormat;

        protected DateTimeHelper() {
            if(CultureInfo.CurrentCulture.Name.StartsWith(EnglishLanguage, StringComparison.OrdinalIgnoreCase)) {
                // The shortestDayNames array shortens English weekdays to two letters.
                // The native experience has 3 letters, so we initialize it correclty here.
                ShortestDayNames = new string[] { "Sun",
                                                  "Mon",
                                                  "Tue",
                                                  "Wed",
                                                  "Thu",
                                                  "Fri",
                                                  "Sat" };
            }
        }

        public string FormatTimeOnly(DateTime time) {
            return string.Format(CultureInfo.CurrentCulture, ValueStringFormatFallback, time);
        }

        ///// <summary>
        ///// Sumarizes a list of days into a shortened string representation.
        ///// If all days, all weekdays, or all weekends are in the list, then the string includes 
        /////     the corresponding name rather than listing out all of those days separately.
        ///// If individual days are listed, they are abreviated.
        ///// If the list is null or empty, "only once" is returned.
        ///// </summary>
        ///// <param name="selection">The list of days. Can be empty or null.</param>
        ///// <returns>A string representation of the list of days.</returns>
        public string SummarizeDaysOfWeek(IList selection) {
            string str = ControlResources.RepeatsOnlyOnce;

            if(null != selection) {
                List<string> contents = new List<string>();
                foreach(object o in selection) {
                    contents.Add((string)o);
                }
                str = DaysOfWeekToString(contents);
            }
            return str;
        }

        /// <summary>
        /// Sumarizes a list of days into a shortened string representation.
        /// If all days, all weekdays, or all weekends are in the list, then the string includes 
        ///     the corresponding name rather than listing out all of those days separately.
        /// If individual days are listed, they are abreviated.
        /// If the list is empty, "only once" is returned.
        /// </summary>
        /// <param name="daysList">The list of days. Can be empty.</param>
        /// <returns>A string representation of the list of days.</returns>
        public string DaysOfWeekToString(List<string> daysList) {
            List<string> days = new List<string>();

            foreach(string day in daysList) {
                // Only include unique days of the week. 
                // Though a list *should* never have duplicate days, protect against it anyways.
                if(!days.Contains(day)) {
                    days.Add(day);
                }
            }

            // No days chosen, return the 'only once' string
            if(days.Count == 0) {
                return ControlResources.RepeatsOnlyOnce;
            }

            StringBuilder builder = new StringBuilder();

            IEnumerable<string> unhandledDays;

            builder.Append(HandleGroups(days, out unhandledDays));

            if(builder.Length > 0) {
                builder.Append(CommaSpace);
            }

            DayOfWeek dow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for(int i = 0; i < DayNames.Count(); i++) {
                int index = ((int)dow + i) % DayNames.Count();
                string day = DayNames[index];

                if(unhandledDays.Contains(day)) {
                    builder.Append(ShortestDayNames[index]);
                    builder.Append(CommaSpace);
                }
            }

            // trim off the remaining ", " characters, as it was the last day
            builder.Length -= CommaSpace.Length;
            return builder.ToString();
        }
        /// <summary>
        /// Finds a group (weekends, weekdays, every day) within a list of days and returns a string representing that group.
        /// Days that are not in a group are set in the unhandledDays out parameter.
        /// </summary>
        /// <param name="days">List of days</param>
        /// <param name="unhandledDays">Out parameter which will be written to with the list of days that were not in a group.</param>
        /// <returns>String of any group found.</returns>
        private static string HandleGroups(List<string> days, out IEnumerable<string> unhandledDays) {
            // First do a check for all of the days of the week, and replace it with the 'every day' string
            if(days.Count == 7) {
                unhandledDays = new List<string>();
                return ControlResources.RepeatsEveryDay;
            }

            var weekdays = CultureInfo.CurrentCulture.Weekdays();
            var weekends = CultureInfo.CurrentCulture.Weekends();

            if(days.Intersect(weekdays).Count() == weekdays.Count) {
                unhandledDays = days.Where(day => !weekdays.Contains(day));
                return ControlResources.RepeatsOnWeekdays;
            } else if(days.Intersect(weekends).Count() == weekends.Count) {
                unhandledDays = days.Where(day => !weekends.Contains(day));
                return ControlResources.RepeatsOnWeekends;
            } else {
                unhandledDays = days;
                return string.Empty;
            }
        }
        /// <summary>
        /// Ripped from Windows Phone Toolkit TimePicker.cs.
        /// </summary>
        private string ValueStringFormatFallback {
            get {
                if(null == _fallbackValueStringFormat) {
                    // Need to convert LongTimePattern into ShortTimePattern to work around a platform bug
                    // such that only LongTimePattern respects the "24-hour clock" override setting.
                    // This technique is not perfect, but works for all the initially-supported languages.
                    string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Replace(":ss", "");
                    string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                    if(lang == "ar" || lang == "fa") {
                        // For arabic and persian, we want the am/pm designator to be displayed at the left.
                        pattern = "\u200F" + pattern;
                    } else {
                        // For LTR languages and Hebrew, we want the am/pm designator to be displayed at the right.
                        pattern = "\u200E" + pattern;
                    }

                    _fallbackValueStringFormat = "{0:" + pattern + "}";
                }
                return _fallbackValueStringFormat;
            }
        }
    }
}
