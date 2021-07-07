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
            //AWS Cloud TimeZone
            DateTime dayLightSavingDate = new DateTime(DateTime.Now.Year, 10, 27);
            if (TimeZoneInfo.Local.Id.Equals("UTC") && (DateTime.Now < dayLightSavingDate))
                return date.AddHours(1).ToString("s") + "Z";

            return date.ToString("s") + "Z";
        }
    }
}
