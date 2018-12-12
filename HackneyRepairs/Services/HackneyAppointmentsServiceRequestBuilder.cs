using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DrsAppointmentsService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
    public class HackneyAppointmentsServiceRequestBuilder : IHackneyAppointmentsServiceRequestBuilder
    {
        private readonly NameValueCollection _appSettings;
        private readonly ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder> _logger;
        public HackneyAppointmentsServiceRequestBuilder(NameValueCollection appSettings, ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public xmbCheckAvailability BuildXmbCheckAvailabilityRequest(string workOrderReference, string sessionId, DrsOrder drsOrder, DateTime startPeriod, DateTime endPeriod)
        {
            _logger.LogInformation($"Building the xmbCheckAvailability request for {workOrderReference}");
            try
            {
                var xmbCheckAvailability = new xmbCheckAvailability
                {
                    theOrder = buildTheOrderForRequest(workOrderReference, drsOrder),
                    sessionId = sessionId,
                    periodBegin = startPeriod,
                    periodEnd = endPeriod,
                    periodBeginSpecified = true,
                    periodEndSpecified = true,
                    canSelectOtherOrders = false,
                    checkAvailType = checkAvailTypeType.standard,
                    onlyBestSlots = onlyBestSlotsValue.fullPeriod
                };
                return xmbCheckAvailability;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        private order buildTheOrderForRequest(string workOrderReference, DrsOrder drsOrder)
        {
            var bookingCodes = drsOrder.Tasks.Select((bookingCode, i) => new bookingCode
            {
                bookingCodeDescription = bookingCode.comments.Trim(),
                bookingCodeSORCode = bookingCode.job_code.Trim(),
                itemValue = bookingCode.itemValue.ToString().Trim(),
                primaryOrderNumber = workOrderReference.Trim(),
                trade = bookingCode.trade.Trim(),
                standardMinuteValue = bookingCode.smv.ToString().Trim(),
                quantity = bookingCode.itemqty.ToString().Trim(),
                itemNumberWithinBooking = (i + 1).ToString().Trim()
            })
    .ToArray();
            var repairLocation = new location
            {
                locationId = drsOrder.prop_ref.Trim(),
                contract = drsOrder.contract.Trim(),
                name = drsOrder.propname.Trim(),
                address1 = drsOrder.address1.Trim(),
                postCode = drsOrder.postcode.Trim()
            };
            return new order
            {
                primaryOrderNumber = workOrderReference.Trim(),
                contract = drsOrder.contract.Trim(),
                locationID = drsOrder.prop_ref.Trim(),
                // TODO:: need to discuss what the user id will be
                userId = "createARepair",
                theBookingCodes = bookingCodes,
                priority = drsOrder.priority,
                contactName = drsOrder.contactName,
                orderComments = drsOrder.comments,
                phone = drsOrder.phone,
                message = drsOrder.txtMessage,
                targetDate = drsOrder.dueDate,
                theLocation = repairLocation
            };
        }

        public xmbOpenSession BuildXmbOpenSessionRequest()
        {
            return new xmbOpenSession
            {
                login = _appSettings["DRSLogin"],
                password = _appSettings["DRSPassword"]
            }; 
        }

        public xmbCloseSession BuildXmbCloseSessionRequest(string sessionId)
        {
            return new xmbCloseSession
            {
                sessionId = sessionId
            };
        }

        public xmbCreateOrder BuildXmbCreateOrderRequest(string workOrderReference, string sessionId, DrsOrder drsOrder)
        {
            var xmbCreateOrder = new xmbCreateOrder
            {
                sessionId = sessionId,
                theOrder = buildTheOrderForRequest(workOrderReference, drsOrder)
            };
            return xmbCreateOrder;
        }

        public xmbScheduleBooking BuildXmbScheduleBookingRequest(string workOrderReference, string sessionId, DateTime beginDate,
            DateTime endDate, DrsOrder drsOrder)
        {
            var bookingCodes = drsOrder.Tasks.Select((bookingCode, i) => new bookingCode
                {
                    bookingCodeDescription = bookingCode.comments.Trim(),
                    bookingCodeSORCode = bookingCode.job_code.Trim(),
                    itemValue = bookingCode.itemValue.ToString().Trim(),
                    primaryOrderNumber = workOrderReference.Trim(),
                    trade = bookingCode.trade.Trim(),
                    standardMinuteValue = bookingCode.smv.ToString().Trim(),
                    quantity = bookingCode.itemqty.ToString().Trim(),
                    itemNumberWithinBooking = i.ToString().Trim()
                })
                .ToArray();
            var order = new order
            {
                primaryOrderNumber = workOrderReference.Trim(),
                contract = drsOrder.contract.Trim(),
                locationID = drsOrder.prop_ref.Trim(),
                userId = drsOrder.userid,
                theBookingCodes = bookingCodes,
                priority = drsOrder.priority,
                contactName = drsOrder.contactName,
                orderComments = drsOrder.comments,
                phone = drsOrder.phone,
                message = drsOrder.txtMessage,
                targetDate = drsOrder.dueDate
            };
            var xmbSchedulebooking = new xmbScheduleBooking
            {
                theBooking = new booking
                {
                    primaryOrderNumber = workOrderReference.Trim(),
                    contract = drsOrder.contract.Trim(),
                    locationID = drsOrder.prop_ref.Trim(),
                    userId = drsOrder.userid,
                    theBookingCodes = bookingCodes,
                    theOrder = order,
                    planningWindowStart = beginDate,
                    planningWindowEnd = endDate,
                    isEmergency = false
                },
                sessionId = sessionId,
                force = false,
                userId = drsOrder.userid,
            };
            return xmbSchedulebooking;
        }

        public xmbSelectOrder BuildXmbSelectOrderRequest(string workOrderReference, string sessionId)
        {
            string[] workOrders = { workOrderReference };
            var xmsSelectOrder = new xmbSelectOrder
            {
                primaryOrderNumber = workOrders,
                sessionId = sessionId
            };
            return xmsSelectOrder;
        }
    }
}
