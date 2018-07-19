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

        public RepairsController(ILoggerAdapter<RepairsActions> loggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository)
        {
            var factory = new HackneyRepairsServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _repairsService = factory.build(uhtRepository, uhwRepository, loggerAdapter);
            _requestBuilder = new HackneyRepairsServiceRequestBuilder(_configBuilder.getConfiguration());
            _repairRequestValidator = new RepairRequestValidator();
            _loggerAdapter = loggerAdapter;
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
                    var jsonResponse = Json(result);
                    jsonResponse.StatusCode = 200;
                    return jsonResponse;
                }
                else
                {
                    var errors = validationResult.ErrorMessages.Select(error => new ApiErrorMessage
                    {
                        developerMessage = error,
                        userMessage = error
                    }).ToList();
                    var jsonResponse = Json(errors);
                    jsonResponse.StatusCode = 400;
                    return jsonResponse;
                }

            }
            catch (Exception ex)
            {
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {
                        developerMessage = ex.Message,
                        userMessage = "We had some problems processing your request"
                    }
                };
                var json = Json(errors);
                json.StatusCode = 500;
                return json;
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
                var json = Json(await repairActions.GetRepairByReference(repairRequestReference));
                json.StatusCode = 200;
                return json;
            }
            catch (MissingRepairException ex)
            {
                //var json = Json(new object());
                //json.StatusCode = 404;
                //return json;

                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {
                        developerMessage = ex.Message,
                        userMessage = @"Cannot find repair."
                    }
                };
                var json = Json(errors);
                json.StatusCode = 404;
                return json;

            }
            catch (Exception ex)
            {
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {

                        developerMessage = ex.Message,
                        userMessage = "We had some problems processing your request"
                    }
                };
                var json = Json(errors);
                json.StatusCode = 500;
                return json;
            }
        }
    }
}