using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Builders;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Validators;
using HackneyRepairs.Services;
using System.Collections;
using System.Configuration;

namespace HackneyRepairs.Controllers
{
	[Produces("application/json")]
	[Route("/v1/work_orders")]
	public class WorkOrdersController : Controller
	{
		private IHackneyWorkOrdersService _workOrdersService;
		private ILoggerAdapter<WorkOrdersActions> _workOrderLoggerAdapter;

        private HackneyConfigurationBuilder _configBuilder;
        private IHackneyRepairsService _repairsService;
        private IHackneyRepairsServiceRequestBuilder _requestBuilder;
        private ILoggerAdapter<RepairsActions> _repairsLoggerAdapter;

        private readonly IExceptionLogger _exceptionLogger;

	    public WorkOrdersController(ILoggerAdapter<WorkOrdersActions> workOrderLoggerAdapter, ILoggerAdapter<RepairsActions> repairsLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uhWarehouseRepository, IUhWebRepository uhWebRepository, IExceptionLogger exceptionLogger)
		{
			_workOrderLoggerAdapter = workOrderLoggerAdapter;
		    _exceptionLogger = exceptionLogger;

            _repairsLoggerAdapter = repairsLoggerAdapter;
            var factory = new HackneyRepairsServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);

            _repairsService = factory.build(uhtRepository, uhwRepository, uhWarehouseRepository, uhWebRepository, _repairsLoggerAdapter);
            _requestBuilder = new HackneyRepairsServiceRequestBuilder(_configBuilder.getConfiguration());

            var workOrderServiceFactory = new HackneyWorkOrdersServiceFactory();
			_workOrdersService = workOrderServiceFactory.build(uhtRepository, uhwRepository, uhWarehouseRepository, _workOrderLoggerAdapter);
		}

        // GET Work Orders by work order references
        /// <summary>
        /// Returns all work orders for given work order references
        /// </summary>
        /// <param name="reference">Work order reference</param>
        /// <param name="include">Allows extending the content of the Work Order response. Currently only accepts the value "mobilereports"</param>
        /// <returns>A list of work order entities</returns>
        /// <response code="200">Returns a list of work orders for the work order references</response>
        /// <response code="400">If no work order references are given</response>   
        /// <response code="404">If one or more work orders are missing based on the references given</response>
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("by_references")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetWorkOrdersByWorkOrderReferences(string[] reference, string include = "")
        {  
            if (reference.Length == 0)
            {
                return ResponseBuilder.Error(400, "Bad request", "Bad Request - Missing reference parameter");
            }

            var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
            var filters = include.Split(',');
            var includeMobileReports = filters.Contains("mobilereports");

            try
            {
                var workOrders = (await workOrdersActions.GetWorkOrders(reference, includeMobileReports)).ToList();
                return ResponseBuilder.Ok(workOrders);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);

                if (ex is UHWWarehouseRepositoryException || ex is UhtRepositoryException || ex is MobileReportsConnectionException)
                {
                    return ResponseBuilder.Error(500, "We had issues with connecting to the data source", ex.Message);
                }
                else if (ex is MissingWorkOrderException)
                {
                    return ResponseBuilder.Error(404, "Could not find one or more of the given work orders", ex.Message);
                }
                else
                {
                    return ResponseBuilder.Error(500, "We had an unknown issue processing your request", ex.Message);
                }
            }
        }

		// GET Work Order 
		/// <summary>
		/// Retrieves a work order
		/// </summary>
		/// <param name="workOrderReference">Work order reference</param>
        /// <param name="include">Allows extending the content of the Work Order response. Currently only accepts the value "mobilereports"</param>
		/// <returns>A work order entity</returns>
		/// <response code="200">Returns the work order for the work order reference</response>
		/// <response code="404">If there is no work order for the given reference</response>   
		/// <response code="500">If any errors are encountered</response>
		[HttpGet("{workOrderReference}")]
		[ProducesResponseType(200)]
        [ProducesResponseType(400)]
		[ProducesResponseType(404)]
		[ProducesResponseType(500)]
        public async Task<JsonResult> GetWorkOrder(string workOrderReference, string include = null)
		{
            var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
			try
			{
                if (string.IsNullOrWhiteSpace(include))
                {
                    var workOrderResult = await workOrdersActions.GetWorkOrder(workOrderReference);
                    return ResponseBuilder.Ok(workOrderResult);
                }
                else if (string.Equals(include.ToLower(), "mobilereports"))
                {
                    var workOrderWithMobileReports = await workOrdersActions.GetWorkOrder(workOrderReference, true);
                    return ResponseBuilder.Ok(workOrderWithMobileReports);
                }
                else
                {
                    return ResponseBuilder.Error(400, $"Unknown parameter value: {include}", $"Unknown parameter value: {include}");
                }
			}
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                
                if (ex is UHWWarehouseRepositoryException || ex is UhtRepositoryException || ex is MobileReportsConnectionException)
                {
                    return ResponseBuilder.Error(500, "We had issues with connecting to the data source", ex.Message);
                }
                else if (ex is MissingWorkOrderException)
                {
                    return ResponseBuilder.Error(404, "Cannit find work order", ex.Message);
                }
                else
                {
                    return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
                }
            }
		}

        // GET Work Order by property reference 
        /// <summary>
        /// Returns all work orders for a property
        /// </summary>
        /// <param name="propertyReference">UH Property reference</param>
        /// <param name="since">A string with the format dd-MM-yyyy (Optional).</param>
        /// <param name="until">A string with the format dd-MM-yyyy (Optional).</param>
        /// <returns>A list of work order entities</returns>
        /// <response code="200">Returns a list of work orders for the property reference</response>
        /// <response code="400">If no parameter or a parameter different than propertyReference is passed</response>   
        /// <response code="404">If there are no work orders for the given property reference</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetWorkOrderByPropertyReference(string[] propertyReference, string since, string until)
        {
            if (propertyReference == null || propertyReference.Length == 0)
            {
                return ResponseBuilder.Error(400, "Bad request", "Bad request - Missing parameter");
            }

			var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
			var result = new List<UHWorkOrder>();
            try
            {
                DateTime validSince = DateTime.Now.AddYears(-2);
                if (since != null)
                {
                    if (!DateTime.TryParseExact(since, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out validSince))
                    {
                        return ResponseBuilder.Error(400, "Invalid parameter format - since", "Parameter is not a valid DateTime");
                    }
                }

                DateTime validUntil = DateTime.Now;
                if (until != null)
                {
                    if (!DateTime.TryParseExact(until, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out validUntil))
                    {
                        return ResponseBuilder.Error(400, "Invalid parameter format - since", "Parameter is not a valid DateTime");
                    }

                    validUntil = validUntil.AddDays(1).AddSeconds(-1);
                }

                result = (await workOrdersActions.GetWorkOrdersByPropertyReferences(propertyReference, validSince, validUntil)).ToList();
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                
                if (ex is UHWWarehouseRepositoryException || ex is UhtRepositoryException)
                {
                    return ResponseBuilder.Error(500, "We had issues with connecting to the data source", ex.Message);
                }
                else
                {
                    return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
                }
            }
        }

        // GET Notes for a Work Order 
        /// <summary>
        /// Returns all notes for a work order
        /// </summary>
        /// <param name="workOrderReference">Work order reference</param>
        /// <returns>A list of notes entities</returns>
        /// <response code="200">Returns a list of notes for a work order reference</response>
        /// <response code="404">If there is no notes found for the work order</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("{workOrderReference}/notes")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetNotesForWorkOrder(string workOrderReference)
        {
			var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
            IEnumerable<Note> result = new List<Note>();
            try
            {
                result = await workOrdersActions.GetNotesByWorkOrderReference(workOrderReference);
                return ResponseBuilder.Ok(result);
            }
			catch (MissingWorkOrderException ex)
            {
                _exceptionLogger.CaptureException(ex);

                return ResponseBuilder.Error(404, "Work order not found", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues with connecting to the data source", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
            }
        }

        // GET A feed of work orders
        /// <summary>
        /// Returns a list of work orders with a work order reference greater than the parameter startId
        /// </summary>
        /// <param name="startId">A work order reference, results will have a grater id than this parameter</param>
        /// <param name="resultSize">The maximum number of work orders returned. Default value is 50</param>
        /// <returns>A list of work orders</returns>
        /// <response code="200">Returns a list of work orders</response>
        /// <response code="400">If a parameter is invalid</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("feed")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> getWorkOrderFeed(string startId, int resultSize = 0)
        {
            if (string.IsNullOrWhiteSpace(startId))
            {
                return ResponseBuilder.Error(400, "Bad request", "Missing parameter - startId");
            }

            try
            {
				var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
                var result = await workOrdersActions.GetWorkOrdersFeed(startId, resultSize);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                if (ex is UhtRepositoryException || ex is UHWWarehouseRepositoryException)
                {
                    return ResponseBuilder.Error(500, "we had issues with connecting to the data source.", ex.Message);
                }

                return ResponseBuilder.Error(500, "We had issues processing your request.", ex.Message);
            }
        }

        /// <summary>
        /// Updates 
        /// </summary>
        /// <param name="workOrderReference">The reference number of the work order for the appointment</param>
        /// <param name="lbhEmail"> JSON object with Email address of user making issue_order request</param>
        /// <returns>A JSON object for a successfully created appointment</returns>
        /// <response code="200">A successfully issued order</response>
        [HttpPost("{workOrderReference}/issue")]
        public async Task<JsonResult> IssueOrder(string workOrderReference, [FromBody]RepairRequest request)
        {
            //string email = this.Request.Headers["x-lhbEmail"].ToString();
            if (!EmailValidator.Validate(request.LBHEmail))
                return ResponseBuilder.Error(500, "Please check your email address", "We had some problems processing your request");
            var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
            var workOrderResult = await workOrdersActions.GetWorkOrder(workOrderReference) as UHWorkOrder;

            if (!SupplierRefDLOValidator.Validate(workOrderResult.SupplierRef))
            {
                return ResponseBuilder.Error(500, "Could not issue order. The work order is associated to an external supplier and this " +
                    "process is intended only for orders issued to the DLO.", "We had some problems processing your request");
            }

            try
            {
                RepairsActions repairActions = new RepairsActions(_repairsService, _requestBuilder, _repairsLoggerAdapter);
                var result = await repairActions.IssueOrderAsync(workOrderReference, request.LBHEmail);
                return ResponseBuilder.Ok(result);
            }
            catch (AppointmentServiceException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "Order not authorised. The authorisation workflow is currently not supported " +
                    "in Repairs Hub. Please cancel the order and re-raise in UH." + ex.Message, ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        /// <summary>
        /// Cancel work order 
        /// </summary>
        /// <param name="workOrderReference">The reference number of the work order</param>
        /// <param name="request">JSON object with the Hackney Email address of user making the cancel request</param>
        /// <returns>A JSON object for a successfully cancelled work order</returns>
        /// <response code="200">A successfully cancelled order</response>
        [HttpPost("{workOrderReference}/cancel")]
        public async Task<JsonResult> CancelOrder(string workOrderReference, [FromBody]RepairRequest request)
        {
            //string email = this.Request.Headers["x-lhbEmail"].ToString();
            if (!EmailValidator.Validate(request.LBHEmail))
                return ResponseBuilder.Error(500, "Please check your email address", "Please check your email address");
            
            try
            {
                RepairsActions repairActions = new RepairsActions(_repairsService, _requestBuilder, _repairsLoggerAdapter);
                var result = await repairActions.CancelOrderAsync(workOrderReference, request.LBHEmail);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, ex.Message, ex.Message);
            }
        }

        // GET Tasks and SORs for a Work Order 
        /// <summary>
        /// Returns all tasks and SORs for a work order
        /// </summary>
        /// <param name="workOrderReference">Work order reference</param>
        /// <returns>A list of tasks and SORs entities</returns>
        /// <response code="200">Returns a list of SORs for a work order reference</response>
        /// <response code="404">If there are no SORs found for the work order</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("{workOrderReference}/tasks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetTasksForWorkOrder(string workOrderReference)
        {
            var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
            IEnumerable<UHWorkOrder> result = new List<UHWorkOrder>();
            try
            {
                result = await workOrdersActions.GetTasksForWorkOrder(workOrderReference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingWorkOrderException ex)
            {
                _exceptionLogger.CaptureException(ex);

                return ResponseBuilder.Error(404, "Work order not found", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues with connecting to the data source", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had issues processing your request", ex.Message);
            }
        }
    }
}
