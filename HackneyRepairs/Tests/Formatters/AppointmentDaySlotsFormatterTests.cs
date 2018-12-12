using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Formatters;
using HackneyRepairs.Models;
using Newtonsoft.Json;
using Xunit;

namespace HackneyRepairs.Tests.Formatters
{
    public class AppointmentDaySlotsFormatterTests
    {
        [Fact]
        public void returns_formatted_dayslots()
        {
            var daySlots = new List<Slot>
            {
                new Slot
                {
                    BeginDate = new DateTime(2017, 10, 18, 10, 00, 00),
                    EndDate = new DateTime(2017, 10, 18, 12, 00, 00),
                    BestSlot = true
                },
                new Slot
                {
                    BeginDate = new DateTime(2017, 10, 18, 12, 00, 00),
                    EndDate = new DateTime(2017, 10, 18, 14, 00, 00),
                    BestSlot = false
                },new Slot
                {
                    BeginDate = new DateTime(2017, 10, 18, 14, 00, 00),
                    EndDate = new DateTime(2017, 10, 18, 16, 00, 00),
                    BestSlot = false
                }
            };
            var formattedDaySlots = daySlots.FormatAppointmentsDaySlots();
            var slots = new object[3];
            var slot1 = new
            {
                beginDate = "2017-10-18T10:00:00Z",
                endDate = "2017-10-18T12:00:00Z",
                bestSlot = true
            };
            var slot2 = new
            {
                beginDate = "2017-10-18T12:00:00Z",
                endDate = "2017-10-18T14:00:00Z",
                bestSlot = false
            };
            var slot3 = new
            {
                beginDate = "2017-10-18T14:00:00Z",
                endDate = "2017-10-18T16:00:00Z",
                bestSlot = false
            };
            slots[0] = slot1;
            slots[1] = slot2;
            slots[2] = slot3;
            Assert.Equal(JsonConvert.SerializeObject(slots), JsonConvert.SerializeObject(formattedDaySlots));
        }
    }
}
