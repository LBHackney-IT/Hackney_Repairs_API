using HackneyRepairs.Models;
using HackneyRepairs.Formatters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HackneyRepairs.Tests.Formatters
{
    public class AppointmentFormatterTests
    {
        [Fact]
        public void returns_formatted_appointment()
        {
            var anAppointment = new Appointment
            {
                BeginDate = new DateTime(2017, 10, 18, 10, 00, 00),
                EndDate = new DateTime(2017, 10, 18, 12, 00, 00),
            };
            var formattedAppointment = anAppointment.FormatAppointment();
            var appointment = new
            {
                beginDate = "2017-10-18T10:00:00Z",
                endDate = "2017-10-18T12:00:00Z",
            };
            Assert.Equal(JsonConvert.SerializeObject(appointment), JsonConvert.SerializeObject(formattedAppointment));
        }
    }
}
