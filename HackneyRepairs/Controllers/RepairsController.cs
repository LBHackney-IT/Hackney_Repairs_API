using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Actions;
using HackneyRepairs.Models;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Factories;
using HackneyRepairs.Services;
using HackneyRepairs.Validators;
using System.Collections;
using HackneyRepairs.Builders;
using HackneyRepairs.Repository;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/repairs")]
    public class RepairsController : Controller
    {
        private IHackneyRepairsService _repairsService;
        private IHackneyRepairsServiceRequestBuilder _requestBuilder;
        private IRepairRequestValidator _repairRequestValidator;
        private ILoggerAdapter<RepairsActions> _loggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private readonly IExceptionLogger _exceptionLogger;

        public RepairsController(ILoggerAdapter<RepairsActions> loggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository, IExceptionLogger exceptionLogger = null)
        {
            var factory = new HackneyRepairsServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _repairsService = factory.build(uhtRepository, uhwRepository, uHWWarehouseRepository, loggerAdapter);
            _requestBuilder = new HackneyRepairsServiceRequestBuilder(_configBuilder.getConfiguration());
            _repairRequestValidator = new RepairRequestValidator();
            _loggerAdapter = loggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        /// <summary>
        /// Creates a repair request
        /// </summary>
        /// <param name="priority">The priority of the request</param>
        /// <param name="problem">A description of the problem</param>
        /// <param name="propertyref">The reference number of the property the repair request is for</param>
        /// <param name="repairorders">Optionally, a list repair order objects can be included in the request</param>
        /// <returns>A JSON object for a successfully created repair request</returns>
        /// <response code="200">A successfully created repair request</response>
        [HttpPost]
        public async Task<JsonResult> Post([FromBody]RepairRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = _repairRequestValidator.Validate(request);

                if (validationResult.Valid)
                {
                    RepairsActions actions = new RepairsActions(_repairsService, _requestBuilder, _loggerAdapter);
                    var result = await actions.CreateRepair(request);
                    return ResponseBuilder.Ok(result);
                }
                var errors = validationResult.ErrorMessages.Select(error => new ApiErrorMessage
                {
                    DeveloperMessage = error,
                    UserMessage = error
                }).ToList();
                return ResponseBuilder.ErrorFromList(400, errors);

            }
            catch (Exception ex)
            {
                _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET repair by reference
        /// <summary>
        /// Retrieves a repair request
        /// </summary>
        /// <param name="repairRequestReference">The reference number of the repair request</param>
        /// <returns>A repair request</returns>
        /// <response code="200">Returns a repair request</response>
        /// <response code="404">If the request is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{repairRequestReference}")]
        public async Task<JsonResult> GetByReference(string repairRequestReference)
        {
            try
            {
                RepairsActions repairActions = new RepairsActions(_repairsService, _requestBuilder, _loggerAdapter);
                var result = await repairActions.GetRepair(repairRequestReference);
                return ResponseBuilder.Ok(result);
            }
			catch (MissingRepairRequestException ex)
            {
                _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(404, "Cannot find repair", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
                _exceptionLogger?.CaptureException(ex);
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {
                        DeveloperMessage = ex.Message,
                        UserMessage = "We had some problems connecting to the data source"
                    }
                };
                var json = Json(errors);
                json.StatusCode = 500;
                return json;
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger?.CaptureException(ex);
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {

                        DeveloperMessage = ex.Message,
                        UserMessage = "We had some issues connecting to the data source"
                    }
                };
                var json = Json(errors);
                json.StatusCode = 500;
                return json;
            }
            catch (Exception ex)
            {
                _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }

        }

		    // GET Repair Requests by property reference
        /// <summary>
		    /// Returns all Repair Requests for a property, for the work orders and contact details call /v1/repairs/{repairRequestReference}
        /// </summary>
		    /// <param name="propertyReference">Universal Housing property reference</param>
        /// <returns>A list of Repair Requests</returns>
		    /// <response code="200">Returns a list of Repair Requests</response>
        /// <response code="404">If no Repair Request was found for the property</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet]
        public async Task<JsonResult> GetByPropertyReference(string propertyReference)
        {
            if (String.IsNullOrWhiteSpace(propertyReference))
            {
                return ResponseBuilder.Error(400, "Missing parameter - propertyReference", "Missing parameter - propertyReference");
            }

            try
            {
                RepairsActions repairActions = new RepairsActions(_repairsService, _requestBuilder, _loggerAdapter);
                var result = await repairActions.GetRepairByPropertyReference(propertyReference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(404, "Cannot find property", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger?.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }
    }
}