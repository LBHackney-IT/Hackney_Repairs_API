using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DrsAppointmentsService;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Services;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Services
{
    public class HackneyAppointmentsServiceRequestBuilderTests
    {
        [Fact]
        public void build_open_session_request_return_a_built_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            var request = builder.BuildXmbOpenSessionRequest();
            Assert.IsType<xmbOpenSession>(request);
        }


        [Fact]
        public void build_open_session_request_builts_valid_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            var configuration = new NameValueCollection
            {
                {"DRSLogin", "Login" },
                {"DRSPassword", "Password" }
            };
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(configuration, mockLogger.Object);
            var request = builder.BuildXmbOpenSessionRequest();
            Assert.IsType<xmbOpenSession>(request);
            Assert.Equal(request.login, "Login");
            Assert.Equal(request.password, "Password");
        }

        [Fact]
        public void build_xmb_check_availability_request_return_a_built_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            var drsOrder = new DrsOrder
            {
                contract = "H01",
                prop_ref = "12345",
                propname = "The Address",
                address1 = "The Address",
                postcode = "addresspostcode",
                createdDate = DateTime.Today,
                dueDate = DateTime.Today.AddDays(30),
                Tasks = new List<DrsTask> {
                    new DrsTask
                    {
                        job_code = "00210356",
                        comments = "Some comments",
                        itemValue = Decimal.MinValue,
                        itemqty = Decimal.MinValue,
                        trade = "GL",
                        smv = 1
                    }
                }
            };
            var request = builder.BuildXmbCheckAvailabilityRequest("01550854", "123456", drsOrder, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.IsType<xmbCheckAvailability>(request);
        }

        [Fact]
        public void build_xmb_check_availability_request_return_a_valid_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);

            var drsOrder = new DrsOrder
            {
                contract = "H01",
                prop_ref = "12345",
                propname = "The Address",
                address1 = "The Address",
                postcode = "addresspostcode",
                createdDate = DateTime.Today,
                dueDate = DateTime.Today.AddDays(30),
                Tasks = new List<DrsTask> {
                    new DrsTask
                    {
                        job_code = "00210356",
                        comments = "Some comments",
                        itemValue = Decimal.MinValue,
                        itemqty = Decimal.MinValue,
                        trade = "GL",
                        smv = 1
                    } 
                }
            };
            var request = builder.BuildXmbCheckAvailabilityRequest("01550854", "123456", drsOrder, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.Equal(request.sessionId, "123456");
            Assert.Equal(request.theOrder.primaryOrderNumber, "01550854");
            Assert.Equal(request.theOrder.contract, "H01");
            Assert.Equal(request.theOrder.locationID, "12345");
            Assert.Equal(request.theOrder.theBookingCodes[0].bookingCodeSORCode, "00210356");
        }

        [Fact]
        public void build_xmb_close_session_request_return_a_built_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            var request = builder.BuildXmbCloseSessionRequest("123456");
            Assert.IsType<xmbCloseSession>(request);
        }

        [Fact]
        public void build_xmb_close_session_request_return_a_valid_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            var request = builder.BuildXmbCloseSessionRequest("123456");
            Assert.Equal(request.sessionId, "123456");
        }
        [Fact]
        public void build_xmb_create_order_request_return_a_valid_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            
            var bookingCodes = new List<bookingCode>{ new bookingCode
                {
                    bookingCodeDescription = "Some comments",
                    bookingCodeSORCode = "00210356",
                    itemValue = "1.00",
                    primaryOrderNumber = "01550854",
                    trade = "GL",
                    standardMinuteValue = "1",
                    quantity = "1.00",
                    itemNumberWithinBooking = "1"
                }}
                .ToArray();
            var order = new DrsOrder
            {
                contract = "H01",
                prop_ref = "12345",
                propname = "The Address",
                address1 = "The Address",
                postcode = "addresspostcode",
                createdDate = DateTime.Today,
                dueDate = DateTime.Today.AddDays(30),
                Tasks = new List<DrsTask> {
                    new DrsTask
                    {
                        job_code = "00210356",
                        comments = "Some comments",
                        itemValue = Decimal.MinValue,
                        itemqty = Decimal.MinValue,
                        trade = "GL",
                        smv = 1
                    }
                }
            };

            var request = builder.BuildXmbCreateOrderRequest("01550854", "123456", order);
            Assert.Equal(request.sessionId, "123456");
            Assert.Equal(request.theOrder.primaryOrderNumber, "01550854");
            Assert.Equal(request.theOrder.contract, "H01");
            Assert.Equal(request.theOrder.locationID, "12345");
            Assert.Equal(request.theOrder.theBookingCodes[0].bookingCodeSORCode, "00210356");
        }

        [Fact]
        public void build_xmb_schedule_booking_request_returns_a_valid_request_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder>>();
            IHackneyAppointmentsServiceRequestBuilder builder =
                new HackneyAppointmentsServiceRequestBuilder(new NameValueCollection(), mockLogger.Object);
            var drsOrder = new DrsOrder
            {
                contract = "H01",
                prop_ref = "12345",
                createdDate = DateTime.Today,
                dueDate = DateTime.Today.AddDays(30),
                Tasks = new List<DrsTask> {
                    new DrsTask
                    {
                        job_code = "00210356",
                        comments = "Some comments",
                        itemValue = Decimal.MinValue,
                        itemqty = Decimal.MinValue,
                        trade = "GL",
                        smv = 1
                    }
                }
            };
            var request = builder.BuildXmbScheduleBookingRequest("01550854", "123456", new DateTime(2017, 11, 21, 10, 00, 00), new DateTime(2017, 11, 21, 12, 00, 00), drsOrder);
            Assert.Equal(request.sessionId, "123456");
            Assert.Equal(request.theBooking.primaryOrderNumber, "01550854");
            Assert.Equal(request.theBooking.contract, "H01");
            Assert.Equal(request.theBooking.locationID, "12345");
            Assert.Equal(request.theBooking.theBookingCodes[0].bookingCodeSORCode, "00210356");
            Assert.Equal(request.theBooking.planningWindowStart, new DateTime(2017, 11, 21, 10, 00, 00));
            Assert.Equal(request.theBooking.planningWindowEnd, new DateTime(2017, 11, 21, 12, 00, 00));
        }
    }
}
