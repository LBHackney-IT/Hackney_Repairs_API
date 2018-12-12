using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Models;
using HackneyRepairs.Validators;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Validations
{
    public class ScheduleBookingRequestValidationTests
    {
        [Fact]
        public void returns_true_if_schedule_booking_request_is_valid()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Z",
                EndDate = "2017-11-10T12:00:00Z"
            };
            var result = validator.Validate("015580954", request);
            Assert.True(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 0);
        }

        [Fact]
        public void returns_false_and_errormessage_if_schedule_booking_request_is_missing_work_order_reference()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Z",
                EndDate = "2017-11-10T12:00:00Z"
            };
            var result = validator.Validate("", request);
            Assert.False(result.Valid); Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("You must provide a work order reference", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_work_order_reference_does_exist_in_UHT()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Z",
                EndDate = "2017-11-10T12:00:00Z"
            };
            var result = validator.Validate("015580954", request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("Please provide a valid work order reference", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_schedule_booking_request_is_missing_begin_date()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "",
                EndDate = "2017-11-10T12:00:00Z"
            };
            var result = validator.Validate("015580954", request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("You must provide a begin Date", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_begin_date_is_not_formatted_correctly()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Zfdrf",
                EndDate = "2017-11-10T12:00:00Z"
            };
            var result = validator.Validate("015580954", request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.True(result.ErrorMessages[0].Contains("Please provide a valid begin date"));
        }

        [Fact]
        public void returns_false_and_errormessage_if_schedule_booking_request_is_missing_end_date()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Z",
                EndDate = ""
            };
            var result = validator.Validate("015580954", request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("You must provide a end Date", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_end_date_is_not_formatted_correctly()
        {
            var fakeRequestBuilder = new Mock<IHackneyRepairsService>();
            fakeRequestBuilder.Setup(service => service.GetWorkOrderDetails("015580954"))
                .ReturnsAsync(new DrsOrder { wo_ref = "015580954" });
            var validator = new ScheduleBookingRequestValidator(fakeRequestBuilder.Object);
            var request = new ScheduleAppointmentRequest
            {
                BeginDate = "2017-11-10T10:00:00Z",
                EndDate = "2017-11-10T12:00:00Zdfdf"
            };
            var result = validator.Validate("015580954", request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.True(result.ErrorMessages[0].Contains("Please provide a valid end date"));
        }
    }
}
