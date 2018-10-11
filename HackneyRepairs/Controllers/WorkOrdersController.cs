using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Controllers
{
	[Produces("application/json")]
	[Route("/v1/work_orders")]
	public class WorkOrdersController : Controller
	{
		private IHackneyWorkOrdersService _workOrdersService;
		private ILoggerAdapter<WorkOrdersActions> _workOrderLoggerAdapter;

		public WorkOrdersController(ILoggerAdapter<WorkOrdersActions> workOrderLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uhWarehouseRepository)
		{
			_workOrderLoggerAdapter = workOrderLoggerAdapter;
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
                var error = new ApiErrorMessage
                {
                    developerMessage = "Bad Request - Missing reference parameter",
                    userMessage = @"Bad Request"
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 400;
                return jsonResponse;
            }

            var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
            var filters = include.Split(',');
            var includeMobileReports = filters.Contains("mobilereports");

            try
            {
                var workOrders = (await workOrdersActions.GetWorkOrders(reference, includeMobileReports)).ToList();
                return Json(workOrders);
            }
            catch (Exception ex)
            {
                JsonResult errorJsonResponse;
                var exceptionError = new ApiErrorMessage
                {
                    developerMessage = ex.Message
                };

                if (ex is UHWWarehouseRepositoryException || ex is UhtRepositoryException || ex is MobileReportsConnectionException)
                {
                    exceptionError.userMessage = "We had issues with connecting to the data source.";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 500;
                }
                else if (ex is MissingWorkOrderException)
                {
                    exceptionError.userMessage = "Could not find one or more of the given work orders";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 404;
                }
                else
                {
                    exceptionError.userMessage = "We had an unknown issue processing your request.";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 500;

                    throw;
                }

                return errorJsonResponse;
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
                JsonResult json;
                if (string.IsNullOrWhiteSpace(include))
                {
                    var workOrderResult = await workOrdersActions.GetWorkOrder(workOrderReference);
                    json = Json(workOrderResult); 
                }
                else if (string.Equals(include.ToLower(), "mobilereports"))
                {
                    var workOrderWithMobileReports = await workOrdersActions.GetWorkOrder(workOrderReference, true);
                    json = Json(workOrderWithMobileReports);
                }
                else
                {
                    var error = new ApiErrorMessage
                    {
                        developerMessage = $"Unknown parameter value: {include}",
                        userMessage = $"Unknown parameter value: {include}"
                    };
                    json = Json(error);
                    json.StatusCode = 400;
                    return json;
                }

				json.StatusCode = 200;
				return json;
			}
            catch (Exception ex)
            {
                var exceptionError = new ApiErrorMessage
                {
                    developerMessage = ex.Message
                };

                JsonResult errorJsonResponse;
                if (ex is UHWWarehouseRepositoryException || ex is UhtRepositoryException || ex is MobileReportsConnectionException)
                {
                    exceptionError.userMessage = "We had issues with connecting to the data source.";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 500;
                }
                else if (ex is MissingWorkOrderException)
                {
                    exceptionError.userMessage = "Cannot find work order";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 404;
                }
                else
                {
                    exceptionError.userMessage = "We had issues processing your request.";
                    errorJsonResponse = Json(exceptionError);
                    errorJsonResponse.StatusCode = 500;
                }
                return errorJsonResponse;
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
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<JsonResult> GetWorkOrderByPropertyReference(string[] propertyReference, string since, string until)
        {
            if (propertyReference == null || propertyReference.Length == 0)
            {
                var error = new ApiErrorMessage
                {
                   developerMessage = "Bad Request - Missing parameter",
                   userMessage = @"Bad Request"
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 400;
                return jsonResponse;
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
                        var error = new ApiErrorMessage
                        {
                            developerMessage = "parameter is not a valid DateTime",
                            userMessage = "Invalid parameter value - since"
                        };
                        var jsonResponse = Json(error);
                        jsonResponse.StatusCode = 400;
                        return jsonResponse;
                    }
                }

                DateTime validUntil = DateTime.Now;
                if (until != null)
                {
                    if (!DateTime.TryParseExact(until, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out validUntil))
                    {
                        var error = new ApiErrorMessage
                        {
                            developerMessage = "parameter is not a valid DateTime",
                            userMessage = "Invalid parameter value - until"
                        };
                        var jsonResponse = Json(error);
                        jsonResponse.StatusCode = 400;
                        return jsonResponse;
                    }
                }

                result = (await workOrdersActions.GetWorkOrdersByPropertyReferences(propertyReference, validSince, validUntil)).ToList();
                var json = Json(result);
                json.StatusCode = 200;
                return json;
            }
            catch (UhtRepositoryException ex)
            {
                var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message,
                    userMessage = @"We had issues with connecting to the data source."
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
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
                var json = Json(result);
                json.StatusCode = 200;
                return json;
            }
			catch (MissingWorkOrderException ex)
            {
                var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message,
                    userMessage = @"Work order not found."
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
            catch (UhtRepositoryException ex)
            {
                var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message,
                    userMessage = @"We had issues with connecting to the data source."
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
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
                var error = new ApiErrorMessage
                {
                    developerMessage = "Missing parameter - startId",
                    userMessage = @"Bad Request"
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 400;
                return jsonResponse;
            }

            try
            {
				var workOrdersActions = new WorkOrdersActions(_workOrdersService, _workOrderLoggerAdapter);
                var result = await workOrdersActions.GetWorkOrdersFeed(startId, resultSize);
                var jsonResponse = Json(result);
                jsonResponse.StatusCode = 200;
                return jsonResponse;
            }
            catch (Exception ex)
            {
                var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message
                };

                JsonResult jsonResponse;
                if (ex is UhtRepositoryException || ex is UHWWarehouseRepositoryException)
                {
                    error.userMessage = "we had issues with connecting to the data source.";
                    jsonResponse = Json(error);
                }
                else
                {
                    error.userMessage = "We had issues processing your request.";
                    jsonResponse = Json(error);
                }

                jsonResponse.StatusCode = 500;
                return jsonResponse;
            }
        }
	}
}
