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
            TimeZoneInfo gmtZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(date, gmtZone).ToString("s") + "Z";
            //return date.ToString("s") + "Z";
        }
    }
}
