using System;
using System.Collections.Generic;
using System.Linq;
using HackneyRepairs.Models;

namespace HackneyRepairs.Formatters
{
    public static class AppointmentDaySlotsFormatter
    {
        public static object FormatAppointmentsDaySlots(this List<Slot> slots)
        {
            return slots.Select(s => new 
            {
              beginDate = DateTimeFormatter.FormatDateTimeToUtc(s.BeginDate),
              endDate = DateTimeFormatter.FormatDateTimeToUtc(s.EndDate),
              bestSlot = s.BestSlot
            }).ToArray();
        }
    }
}
