using System;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Builders;
using System.Threading.Tasks;
using HackneyRepairs.Factories;
using HackneyRepairs.Services;
using System.Xml;
using Newtonsoft.Json;
using System.Collections;
using System.Configuration;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/keyfax")]
    public class KeyFaxController : Controller
    {
        private readonly IExceptionLogger _exceptionLogger;
        private ILoggerAdapter<KeyFaxActions> _loggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private IHackneyKeyFaxService _keyfaxService;
        private IHackneyKeyFaxServiceRequestBuilder _requestBuilder;

        public KeyFaxController(ILoggerAdapter<KeyFaxActions> loggerAdapter, IExceptionLogger exceptionLogger)
        {
            var factory = new HackneyKeyFaxServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _requestBuilder = new HackneyKeyFaxServiceRequestBuilder(_configBuilder.getConfiguration());
            _keyfaxService = factory.build(loggerAdapter);
            _loggerAdapter = loggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        [HttpGet("get_startup_url")]
        public async Task<JsonResult> GetKeyFaxStartUpURLAsync(string returnURL = "http://")
        {
            try
            {
                KeyFaxActions actions = new KeyFaxActions(_keyfaxService, _requestBuilder, _loggerAdapter);
                
                //Keyfax return type is KeyFaxService.StartupResponse
                var result = await actions.GetStartUpURLAsync(returnURL);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We were unable to start the Keyfax Service. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair or property and a screenshot. ", ex.Message);
            }
        }

        // GET Parsed keyfax results
        /// <summary>
        /// Returns FaultText, RepairCode, RepairCode-Description and Priority
        /// </summary>
        /// <param name="resultID">Keyfax GUID</param>
        /// <returns>Parsed keyfaxdata object result</returns>
        [HttpGet("kf_result/{resultID}")]
        public async Task<JsonResult> GetKeyFaxResultsAsync(string resultID)
        {
            try
            {
                KeyFaxActions actions = new KeyFaxActions(_keyfaxService, _requestBuilder, _loggerAdapter);

                //Keyfax return type is KeyFaxService.GetResultsResponse
                var result = await actions.GetResultsAsync(resultID);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }
    }
}