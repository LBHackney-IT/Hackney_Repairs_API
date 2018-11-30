using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Actions;
using HackneyRepairs.Formatters;
using HackneyRepairs.Services;
using HackneyRepairs.Validators;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using HackneyRepairs.Repository;
using HackneyRepairs.Builders;

namespace HackneyRepairs.Controllers
{
	[Produces("application/json")]
	public class AppointmentsController : Controller
	{
		private IHackneyAppointmentsService _appointmentsService;
		private IHackneyRepairsService _repairsService;
		private ILoggerAdapter<AppointmentActions> _loggerAdapter;
		private IHackneyAppointmentsServiceRequestBuilder _serviceRequestBuilder;
		private IHackneyRepairsServiceRequestBuilder _repairsServiceRequestBuilder;
		private IScheduleBookingRequestValidator _scheduleBookingRequestValidator;
		private HackneyConfigurationBuilder _configBuilder;
		private readonly IExceptionLogger _exceptionLogger;

		public AppointmentsController(ILoggerAdapter<AppointmentActions> loggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository,
			ILoggerAdapter<HackneyAppointmentsServiceRequestBuilder> requestBuildLoggerAdapter, ILoggerAdapter<RepairsActions> repairsLoggerAdapter,
                                      IDRSRepository drsRepository, IUHWWarehouseRepository uHWWarehouseRepository, IExceptionLogger exceptionLogger = null)
		{
			var serviceFactory = new HackneyAppointmentServiceFactory();
			_configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _appointmentsService = serviceFactory.build(loggerAdapter, uhtRepository, drsRepository);
			var factory = new HackneyRepairsServiceFactory();
            _repairsService = factory.build(uhtRepository, uhwRepository, uHWWarehouseRepository, repairsLoggerAdapter);
			_loggerAdapter = loggerAdapter;
			_serviceRequestBuilder = new HackneyAppointmentsServiceRequestBuilder(_configBuilder.getConfiguration(), requestBuildLoggerAdapter);
			_scheduleBookingRequestValidator = new ScheduleBookingRequestValidator(_repairsService);
			_repairsServiceRequestBuilder = new HackneyRepairsServiceRequestBuilder(_configBuilder.getConfiguration());
			_exceptionLogger = exceptionLogger;
		}

		// GET available appointments for a Universal Housing work order
		/// <summary>
		/// Returns available appointments for a Universal Housing work order
		/// </summary>
		/// <param name="workorderreference">The work order reference for which to provide available appointments</param>
		/// <returns>A list of available appointments</returns>
		/// <response code="200">Returns the list of available appointments</response>
		/// <response code="400">If no valid work order reference is provided</response>   
		/// <response code="500">If any errors are encountered</response>   
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
        [ProducesResponseType(404)]
		[ProducesResponseType(500)]
		[Route("v1/work_orders/{workOrderReference}/available_appointments")]
		public async Task<JsonResult> Get(string workOrderReference)
		{
			try
			{
                if (string.IsNullOrWhiteSpace(workOrderReference))
                {
                    return ResponseBuilder.Error(400, "Please provide a valid work order reference", "Invalid parameter - workorderreference");
                }

                var appointmentsActions = new AppointmentActions(_loggerAdapter, _appointmentsService, _serviceRequestBuilder, _repairsService, _repairsServiceRequestBuilder, _configBuilder.getConfiguration());
                var response = await appointmentsActions.GetAppointments(workOrderReference);
                return ResponseBuilder.Ok(new { results = response.ToList().FormatAppointmentsDaySlots() });
            }
			catch (NoAvailableAppointmentsException ex)
			{
				_exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Ok(new { results = new List<string>() });
			}
            catch (InvalidWorkOrderInUHException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(404, "WorkOrderReference not found", ex.Message);
            }
			catch (Exception ex)
			{
				_exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
			}
		}

		/// <summary>
		/// Creates an appointment
		/// </summary>
		/// <param name="workOrderReference">The reference number of the work order for the appointment</param>
		/// <param name="appointment">Details of the appointment to be booked</param>
		/// <returns>A JSON object for a successfully created appointment</returns>
		/// <response code="200">A successfully created repair request</response>
		[HttpPost]
		[Route("v1/work_orders/{workOrderReference}/appointments")]
		public async Task<JsonResult> Post(string workOrderReference, [FromBody]ScheduleAppointmentRequest request)
		{
			try
			{
				var validationResult = _scheduleBookingRequestValidator.Validate(workOrderReference, request);
				if (validationResult.Valid)
				{
					var appointmentsActions = new AppointmentActions(_loggerAdapter, _appointmentsService,
																	 _serviceRequestBuilder, _repairsService, _repairsServiceRequestBuilder, _configBuilder.getConfiguration());
					var result = await appointmentsActions.BookAppointment(workOrderReference,
						DateTime.Parse(request.BeginDate),
						DateTime.Parse(request.EndDate));
                    return ResponseBuilder.Ok(result);
				}
				else
				{
					var errors = validationResult.ErrorMessages.Select(error => new ApiErrorMessage
					{
						DeveloperMessage = error,
						UserMessage = error
					}).ToList();
                    return ResponseBuilder.ErrorFromList(400, errors);
				}
			}
			catch (Exception ex)
			{
				_exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
			}
		}

		// GET all appointments booked appointments by work order reference 
        /// <summary>
        /// Returns all appointments for a work order
        /// </summary>
        /// <param name="workOrderReference">UH work order reference</param>
        /// <returns>A list of UHT appointment entities</returns>
        /// <response code="200">Returns a list of appointments for a work order reference</response>
        /// <response code="404">If there are no appointments found for the work orders reference</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("v1/work_orders/{workOrderReference}/appointments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetAppointmentsByWorkOrderReference(string workOrderReference)
        {
            var appointmentsActions = new AppointmentActions(_loggerAdapter, _appointmentsService, _serviceRequestBuilder, _repairsService, _repairsServiceRequestBuilder, _configBuilder.getConfiguration());
            IEnumerable<DetailedAppointment> result;
            try
            {
                result = await appointmentsActions.GetAppointmentsByWorkOrderReference(workOrderReference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingAppointmentsException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Ok(new string[0]);
            }
            catch (InvalidWorkOrderInUHException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(404, "workOrderReference not found", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues with connecting to the data source.", ex.Message);
            }
            catch (Exception ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
            }
        }

		// GET the latest appointment by work order reference from UH or DRS
        /// <summary>
        /// Returns the latest apointment for a work order
        /// </summary>
        /// <param name="workOrderReference">UH work order reference</param>
        /// <returns>An appointment</returns>
        /// <response code="200">Returns an appointment for a work order reference</response>
        /// <response code="404">If there is no appointment found for the work order reference</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("v1/work_orders/{workOrderReference}/appointments/latest")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            var appointmentsActions = new AppointmentActions(_loggerAdapter, _appointmentsService, _serviceRequestBuilder, _repairsService, _repairsServiceRequestBuilder, _configBuilder.getConfiguration());
            DetailedAppointment result;
            try
            {
                result = await appointmentsActions.GetLatestAppointmentByWorkOrderReference(workOrderReference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingAppointmentException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Ok(new string[0]);
            }
            catch (InvalidWorkOrderInUHException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(404, "workOrderReference not found", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues with connecting to the data source.", ex.Message);
            }
            catch (Exception ex)
            {
	            _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
            }
        }
	}
}
