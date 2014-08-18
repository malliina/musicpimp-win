using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mle.Util {
    public interface IDateTimeHelper {
        string SummarizeDaysOfWeek(IList days);
        string FormatTimeOnly(DateTime time);
    }
}
