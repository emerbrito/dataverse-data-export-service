using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class DateUtility
    {

        static TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public static TimeZoneInfo LocalTimeZone { get => localTimeZone; }
        public static DateTime LocalNow { get => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, LocalTimeZone); }

    }
}
