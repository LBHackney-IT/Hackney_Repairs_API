using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrsAppointmentsService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
    public class FakeAppointmentService : IHackneyAppointmentsService
    {
        public Task<checkAvailabilityResponse> GetAppointmentsForWorkOrderReference(xmbCheckAvailability request)
        {
            var xmbCheckAvaialabilityResponse = new xmbCheckAvailabilityResponse
            {
                status = responseStatus.success,
                theSlots = new List<daySlotsInfo>
                {
                    new daySlotsInfo
                    {
                        day = new DateTime(2017, 10, 18, 00, 00, 00),
                        daySpecified = false,
                        nonWorkingDay = false,
                        weeklyDayOff = false,
                        slotsForDay = new List<slotInfo>
                        {
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = new DateTime(2017, 10, 18, 10, 00, 00),
                                endDate = new DateTime(2017, 10, 18, 12, 00, 00),
                                bestSlot = true
                            },
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = new DateTime(2017, 10, 18, 12, 00, 00),
                                endDate = new DateTime(2017, 10, 18, 14, 00, 00),
                                bestSlot = false
                            },
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = new DateTime(2017, 10, 18, 14, 00, 00),
                                endDate = new DateTime(2017, 10, 18, 16, 00, 00),
                                bestSlot = false
                            }
                        }.ToArray()
                    }
                }.ToArray()
            };
            var response = new checkAvailabilityResponse(xmbCheckAvaialabilityResponse);
            switch (request.theOrder.primaryOrderNumber)
            {
                case "01550854":
                    return Task.Run(() => response);
                case "01550853":
                    return Task.Run(() => new checkAvailabilityResponse(new xmbCheckAvailabilityResponse
                    {
                        status = responseStatus.error,
                        theSlots = new List<daySlotsInfo>().ToArray()
                    }));
                default:
                    return Task.Run(() => new checkAvailabilityResponse(new xmbCheckAvailabilityResponse
                    {
                        status = responseStatus.success,
                        theSlots = new List<daySlotsInfo>().ToArray()
                    }));
            }
        }

        public Task<openSessionResponse> OpenSessionAsync(xmbOpenSession openSession)
        {
            return Task.Run(() => new openSessionResponse
            {
                @return = new xmbOpenSessionResponse
                {
                    status = responseStatus.success,
                    sessionId = "123456"
                }
            });
        }

        public Task<closeSessionResponse> CloseSessionAsync(xmbCloseSession closeSession)
        {
            return Task.Run(() => new closeSessionResponse(
                new xmbCloseSessionResponse
                {
                    status = responseStatus.success
                }));
        }

        public Task<createOrderResponse> CreateWorkOrderAsync(xmbCreateOrder createOrder)
        {
            var createOrderResponse = new createOrderResponse
            {
                @return = new xmbCreateOrderResponse
                {
                    status = responseStatus.success,
                    theOrder = new order { primaryOrderNumber = createOrder.theOrder.primaryOrderNumber }
                }
            };

            return Task.Run(() => createOrderResponse);
        }

        public Task<scheduleBookingResponse> ScheduleBookingAsync(xmbScheduleBooking scheduleBooking)
        {
            switch (scheduleBooking.theBooking.primaryOrderNumber)
            {
                case "01550853":
                   return Task.Run(() =>
                        new scheduleBookingResponse(new xmbScheduleBookingResponse { status = responseStatus.error }));
                default:
                    return Task.Run(() =>
                        new scheduleBookingResponse(new xmbScheduleBookingResponse { status = responseStatus.success }));
            }
        }

        public Task<selectOrderResponse> SelectOrderAsync(xmbSelectOrder selectOrder)
        {
            return Task.Run(() => new selectOrderResponse(new xmbSelectOrderResponse
            {
                status = responseStatus.success,
                theOrders = new List<order>
                {
                    new order
                    {
                        orderId = 123,
                        theBookings = new List<booking>
                        {
                            new booking
                            {
                                bookingId = 123456,
                                assignedStart = new DateTime(2017, 10, 18, 10, 00, 00),
                                assignedEnd = new DateTime(2017, 10, 18, 12, 00, 00)
                            }
                        }.ToArray()
                    }
                }.ToArray()
            }));
        }

        public Task<selectBookingResponse> SelectBookingAsync(xmbSelectBooking selectBooking)
        {
            return Task.Run(() =>
                new selectBookingResponse(new xmbSelectBookingResponse { status = responseStatus.success }));
        }

		public Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
		{
			if (string.Equals(workOrderReference, "99999999"))
            {
                return Task.Run(() => (IEnumerable<DetailedAppointment>)new List<DetailedAppointment>
                {
                    new DetailedAppointment()
                });
            }

			if (string.Equals(workOrderReference, "888888888"))
			{
                return Task.Run(() => ((IEnumerable<DetailedAppointment>)new List<DetailedAppointment>()));
			}

			var appointmentEntitites = new List<DetailedAppointment>
			{
				new DetailedAppointment 
                {
					BeginDate = DateTime.Today
                }
            };
			return Task.Run(() => (IEnumerable<DetailedAppointment>)appointmentEntitites);
		}

		public Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            if (string.Equals(workOrderReference, "99999999"))
            {
				return Task.Run(() => new DetailedAppointment()
				{
					BeginDate = null
				});
            }

            if (string.Equals(workOrderReference, "888888888"))
            {
				return Task.Run(() => (DetailedAppointment)null);
            }

            var appointment = new DetailedAppointment()
            {
				BeginDate = DateTime.Today,
                AssignedWorker = "Ben Big"
            };
            return Task.Run(() => appointment);
        }
    }
}
