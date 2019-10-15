using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Formatters
{
    public static class DateTimeFormatter
    {
        public static string FormatDateTimeToGMT(DateTime date)
        {
            TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
            TimeSpan bts = TimeZoneInfo.Local.BaseUtcOffset;

            //compare the UTC offsets
            return (ts > bts) ? date.ToString("s") + "Z" : date.Add(ts).ToString("s") + "Z";
        }
    }
}
