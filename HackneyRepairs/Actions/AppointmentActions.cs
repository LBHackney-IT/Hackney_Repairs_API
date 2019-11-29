using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrsAppointmentsService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System.Collections.Specialized;
using HackneyRepairs.Formatters;

namespace HackneyRepairs.Actions
{
    public class AppointmentActions
    {
        private readonly ILoggerAdapter<AppointmentActions> _logger;
        private readonly IHackneyAppointmentsService _appointmentsService;
        private readonly IHackneyRepairsService _repairsService;
        private readonly IHackneyAppointmentsServiceRequestBuilder _appointmentsServiceRequestBuilder;
        private readonly IHackneyRepairsServiceRequestBuilder _repairsServiceRequestBuilder;
        private readonly NameValueCollection _configuration;

        public AppointmentActions(ILoggerAdapter<AppointmentActions> logger, IHackneyAppointmentsService appointmentsService, IHackneyAppointmentsServiceRequestBuilder requestBuilder, IHackneyRepairsService repairsService, IHackneyRepairsServiceRequestBuilder repairsServiceRequestBuilder, NameValueCollection configuration)
        {
            _logger = logger;
            _appointmentsService = appointmentsService;
            _appointmentsServiceRequestBuilder = requestBuilder;
            _repairsService = repairsService;
            _repairsServiceRequestBuilder = repairsServiceRequestBuilder;
            _configuration = configuration;
        }

        public async Task<IList<Slot>> GetAppointments(string workOrderReference)
        {
            _logger.LogInformation($"Getting appointments from DRS for work order reference {workOrderReference}");
            // Get DRS sessionId
            var sessionId = await OpenDrsServiceSession();

            // get the work order details and pass it to the request builder
            var workOrder = await _repairsService.GetWorkOrderDetails(workOrderReference);
            if (workOrder == null)
            {
                _logger.LogError($"could not find the work order in UH with reference {workOrderReference}");
                throw new InvalidWorkOrderInUHException($"could not find the work order in UH with reference {workOrderReference}");
            }

            // Trim work order properties - to be moved to a separate method
            workOrder.priority = workOrder.priority.Trim();
            // Create the work order in DRS
            var drsCreateResponse = await CreateWorkOrderInDrs(workOrderReference, sessionId, workOrder);

            if (drsCreateResponse.@return.status != responseStatus.success)
            {
              _logger.LogError(drsCreateResponse.@return.errorMsg);
              throw new AppointmentServiceException(drsCreateResponse.@return.errorMsg);
            }

            _logger.LogInformation($"Successfully created order in DRS with order reference {workOrderReference}");
            var slotList = new List<Slot>();
            int count = 0;
            while (!slotList.Any() && count < 4)
            {
                DateTime startDay = DateTime.Now.AddDays(1 + (count * 7));
                DateTime endDay = startDay.AddDays(7);
                var start = new DateTime(startDay.Year, startDay.Month, startDay.Day, 01, 0, 0, 0);
                var end = new DateTime(endDay.Year, endDay.Month, endDay.Day, 01, 0, 0, 0);
                slotList = await getAppointmentSlots(workOrderReference, sessionId, workOrder, start, end);
                count += 1;
            }

            // close session
            await CloseDrsServiceSession(sessionId);
            if (slotList.Any())
            {
                return slotList;
            }
            else
            {
                throw new NoAvailableAppointmentsException($"There are no available appointments for order reference {workOrderReference}");
            }
        }

        public async Task<object> BookAppointment(string workOrderReference, DateTime beginDate, DateTime endDate)
        {
            _logger.LogInformation($"Booking appointment for work order reference {workOrderReference} with {beginDate} and {endDate}");
            // Get DRS sessionId
            var sessionId = await OpenDrsServiceSession();
            // get the work order details and pass it to the request builder
            var workOrder = await _repairsService.GetWorkOrderDetails(workOrderReference);
            if (string.IsNullOrEmpty(workOrder.wo_ref))
            {
                _logger.LogError($"could not find the work order in UH with reference {workOrderReference}");
                throw new InvalidWorkOrderInUHException($"could not find the work order in UH with reference {workOrderReference}");
            }

            var request = _appointmentsServiceRequestBuilder.BuildXmbScheduleBookingRequest(workOrderReference, sessionId, beginDate, endDate, workOrder);

            // Get booking id & order id for the primary order reference
            var orderResponse = await GetOrderFromDrs(workOrderReference, sessionId);
            request.theBooking.orderId = orderResponse.@return.theOrders[0].orderId;
            request.theBooking.bookingId = orderResponse.@return.theOrders[0].theBookings[0].bookingId;
            var response = await _appointmentsService.ScheduleBookingAsync(request);
            // close session
            await CloseDrsServiceSession(sessionId);
            var returnResponse = response.@return;
            if (response.@return.status != responseStatus.success)
            {
                _logger.LogError(returnResponse.errorMsg);
                throw new AppointmentServiceException(returnResponse.errorMsg);
            }

            // update UHT with the order and populate the u_sentToAppointmentSys table
            var order_id = await _repairsService.UpdateUHTVisitAndBlockTrigger(workOrderReference, beginDate, endDate,
                request.theBooking.orderId, request.theBooking.bookingId, BuildSlotDetail(beginDate, endDate));
            // attach the process (running Andrey's stored procedure)
            _logger.LogInformation($"Updating UH documents for workorder {workOrderReference}");
            if (order_id != null)
            {
                await _repairsService.AddOrderDocumentAsync(_configuration.Get("RepairRequestDocTypeCode"),
                                                            workOrderReference, order_id.Value, _configuration.Get("UHDocUploadResponseMessage"));
            }

            // Issue Order
            _logger.LogInformation($"Issuing order for workorder {workOrderReference}");
            var worksOrderRequest = _repairsServiceRequestBuilder.BuildWorksOrderRequest(workOrderReference);
            var issueOrderResponse = await _repairsService.IssueOrderAsync(worksOrderRequest);
            if (!issueOrderResponse.Success)
            {
                _logger.LogError(issueOrderResponse.ErrorMessage);
                throw new AppointmentServiceException(issueOrderResponse.ErrorMessage);
            }

            _logger.LogInformation($"Successfully issued workorder {workOrderReference}");
            // End Issue Order
            var json = new
            {
                beginDate = DateTimeFormatter.FormatDateTimeToUtc(beginDate),
                endDate = DateTimeFormatter.FormatDateTimeToUtc(endDate)
            };
            return json;
        }

        public async Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($"Getting all apointments for workOrderReference: {workOrderReference}");
            var result = await _appointmentsService.GetAppointmentsByWorkOrderReference(workOrderReference);
            if (!result.Any())
            {
                _logger.LogError($"No appointments returned due workOrderReference not being found: {workOrderReference}");
                throw new InvalidWorkOrderInUHException($"No appointments returned due workOrderReference not being found: {workOrderReference}");
            }

            if (result.FirstOrDefault().BeginDate == null)
            {
                _logger.LogError($"No appointments found for : {workOrderReference}");
                throw new MissingAppointmentsException($"No appointments found for : {workOrderReference}");
            }

            _logger.LogInformation($"Appointments returned for workOrderReference: {workOrderReference}");
            GenericFormatter.TrimStringAttributesInEnumerable(result);
            return result;
        }

        public async Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($"Getting current apointment for workOrderReference: {workOrderReference}");
            var result = await _appointmentsService.GetLatestAppointmentByWorkOrderReference(workOrderReference);
            if (result == null)
            {
                _logger.LogError($"No appointment returned due workOrderReference not being found: {workOrderReference}");
                throw new InvalidWorkOrderInUHException($"No appointment returned due workOrderReference not being found: {workOrderReference}");
            }

            if (result.BeginDate == null)
            {
                _logger.LogError($"No appointment found for : {workOrderReference}");
                throw new MissingAppointmentException($"No appointment found for : {workOrderReference}");
            }

            _logger.LogInformation($"Appointment returned for workOrderReference: {workOrderReference}");
            GenericFormatter.TrimStringAttributes(result);
            return result;
        }

        // Currently not used, but it might be in the future
        internal async Task<object> GetAppointmentForWorksOrder(string workOrderReference)
        {
            _logger.LogInformation($"Getting booked appointment for work order reference {workOrderReference}");
            // Get DRS sessionId
            var sessionId = await OpenDrsServiceSession();
            // get the work order details and pass it to the request builder
            var workOrder = await _repairsService.GetWorkOrderDetails(workOrderReference);
            if (string.IsNullOrEmpty(workOrder.wo_ref))
            {
                _logger.LogError($"could not find the work order in UH with reference {workOrderReference}");
                throw new InvalidWorkOrderInUHException($"could not find the work order in UH with reference {workOrderReference}");
            }

            // Get booking id & order id for the primary order reference
            var orderResponse = await GetOrderFromDrs(workOrderReference, sessionId);
            // close session
            await CloseDrsServiceSession(sessionId);
            var returnResponse = orderResponse.@return;
            if (orderResponse.@return.status != responseStatus.success)
            {
                _logger.LogError(returnResponse.errorMsg);
                throw new AppointmentServiceException(returnResponse.errorMsg);
            }

            return new
            {
                beginDate = DateTimeFormatter.FormatDateTimeToUtc(orderResponse.@return.theOrders[0].theBookings[0].assignedStart),
                endDate = DateTimeFormatter.FormatDateTimeToUtc(orderResponse.@return.theOrders[0].theBookings[0].assignedEnd)
            };
        }

        private async Task<string> OpenDrsServiceSession()
        {
            _logger.LogInformation("Opening the DRS Session");
            var sessionRequest = _appointmentsServiceRequestBuilder.BuildXmbOpenSessionRequest();
            var sessionResponse = await _appointmentsService.OpenSessionAsync(sessionRequest);
            var sessionResponseReturn = sessionResponse.@return;
            if (sessionResponseReturn.status != responseStatus.success)
            {
                _logger.LogError(sessionResponseReturn.errorMsg);
                throw new AppointmentServiceException(sessionResponseReturn.errorMsg);
            }

            _logger.LogInformation($"Succesfully opened the session {sessionResponseReturn.sessionId}");
            return sessionResponseReturn.sessionId;
        }

        private async Task CloseDrsServiceSession(string sessionId)
        {
            _logger.LogInformation($"Closing the DRS Session {sessionId}");
            var closeSessionRequest = _appointmentsServiceRequestBuilder.BuildXmbCloseSessionRequest(sessionId);
            var sessionResponse = await _appointmentsService.CloseSessionAsync(closeSessionRequest);
            var sessionResponseReturn = sessionResponse.@return;
            if (sessionResponseReturn.status != responseStatus.success)
            {
                _logger.LogError(sessionResponseReturn.errorMsg);
                throw new AppointmentServiceException();
            }

            _logger.LogInformation($"Succesfully closed the session {sessionId}");
        }

        private List<Slot> buildSlot(daySlotsInfo daySlot)
        {
            _logger.LogInformation($"Getting the Slots info from the daySlot {daySlot}");

          var slots = new List<Slot>();
          try
            {
                if (daySlot.slotsForDay != null)
                {
                    slots = daySlot.slotsForDay.Select(x => new Slot
                    {
                        BeginDate = x.beginDate,
                        EndDate = x.endDate,
                        BestSlot = x.bestSlot,
                        Available = x.available == availableValue.YES
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

          return slots;
        }

        private async Task<createOrderResponse> CreateWorkOrderInDrs(string workOrderReference, string sessionId, DrsOrder drsOrder)
        {
            // build the request
            var request = _appointmentsServiceRequestBuilder.BuildXmbCreateOrderRequest(workOrderReference, sessionId, drsOrder);
            // create the work order
            var response = await _appointmentsService.CreateWorkOrderAsync(request);
            var returnResponse = response.@return;
            if (returnResponse.status == responseStatus.error && returnResponse.errorMsg.Contains("order already exists"))
            {
                _logger.LogInformation($"Unable to create order in DRS, an order already exists with order reference {workOrderReference}");
                response.@return.status = responseStatus.success;
            }

            return response;
        }

        private async Task<selectOrderResponse> GetOrderFromDrs(string workOrderReference, string sessionId)
        {
            var request = _appointmentsServiceRequestBuilder.BuildXmbSelectOrderRequest(workOrderReference, sessionId);

            // get the order
            var response = await _appointmentsService.SelectOrderAsync(request);
            var returnResponse = response.@return;
            if (returnResponse.status != responseStatus.success)
            {
                _logger.LogError(returnResponse.errorMsg);
                throw new AppointmentServiceException(returnResponse.errorMsg);
            }

            _logger.LogInformation($"Succesful getting the order details from Drs for {workOrderReference}");
            return response;
        }

        private async Task<List<Slot>> getAppointmentSlots(string workOrderReference, string sessionId, DrsOrder drsOrder, DateTime startPeriod, DateTime endPeriod)
        {
            var request =
            _appointmentsServiceRequestBuilder.BuildXmbCheckAvailabilityRequest(workOrderReference, sessionId, drsOrder, startPeriod, endPeriod);
            // get the appointments
            var response = await _appointmentsService.GetAppointmentsForWorkOrderReference(request);
            var responseString = response.@return;
            if (responseString.status != responseStatus.success)
            {
                _logger.LogError(responseString.errorMsg);
                throw new AppointmentServiceException(responseString.errorMsg);
            }

            var slots = responseString.theSlots;
            if (slots == null)
            {
                _logger.LogError($"Missing the slots from the response string {responseString}");
                throw new MissingSlotsException($"Missing the slots from the response string {responseString}");
            }

            var slotList = new List<Slot>();
            foreach (var slot in slots)
            {
                slotList.AddRange(buildSlot(slot));
            }

            return slotList.Where(slot => slot.Available).ToList();
        }

        private string BuildSlotDetail(DateTime beginDate, DateTime endDate)
        {
            int hrs = endDate.Subtract(beginDate).Hours;

            string slotName = string.Empty;

            if (hrs < 5 && beginDate.Hour < 9)
                slotName = "Morning";
            else if (hrs < 5 && beginDate.Hour >= 12)
                slotName = "Afternoon";
            else if (beginDate.Hour > 8 && endDate.Hour < 15)
                slotName = "Avoid School Run";
            else if (beginDate.Hour >= 16)
                slotName = "Evening";
            else if (beginDate.DayOfWeek == DayOfWeek.Saturday || beginDate.DayOfWeek == DayOfWeek.Sunday)
                slotName = "Weekend";
            else if (hrs > 5 && beginDate.Hour >= 8)
                slotName = "All Day";

            return slotName;
        }
    }

    public class MissingSlotsException : System.Exception
    {
        public MissingSlotsException()
        {
        }

        public MissingSlotsException(string message) : base(message)
        {
        }
    }

    public class MissingSlotsForDayException : System.Exception { }
    public class AppointmentServiceException : Exception
    {
        public AppointmentServiceException()
        {
        }

        public AppointmentServiceException(string message) : base(message)
        {
        }
    }

    public class InvalidWorkOrderInUHException : System.Exception
    {
        public InvalidWorkOrderInUHException()
        {
        }

        public InvalidWorkOrderInUHException(string message) : base(message)
        {
        }
    }

    public class NoAvailableAppointmentsException : System.Exception
    {
        public NoAvailableAppointmentsException()
        {
        }

        public NoAvailableAppointmentsException(string message) : base(message)
        {
        }
    }

    public class MissingAppointmentsException : Exception
    {
        public MissingAppointmentsException()
        {
        }

        public MissingAppointmentsException(string message) : base(message)
        {
        }
    }

    public class MissingAppointmentException : Exception
    {
        public MissingAppointmentException()
        {
        }

        public MissingAppointmentException(string message) : base(message)
        {
        }
    }
}
