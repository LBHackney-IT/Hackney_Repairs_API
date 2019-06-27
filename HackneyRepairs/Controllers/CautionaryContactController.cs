using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Builders;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/cautionary_contact")]
    public class CautionaryContactController : Controller
    {
        private IHackneyCautionaryContactService _cautionaryContactService;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private readonly IExceptionLogger _exceptionLogger;

        public CautionaryContactController(ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter, IUhwRepository uhwRepository, IExceptionLogger exceptionLogger)
        {
            HackneyCautionaryContactServiceFactory cautionaryContactFactory = new HackneyCautionaryContactServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _cautionaryContactService = cautionaryContactFactory.build(uhwRepository, cautionaryContactLoggerAdapter);
            _cautionaryContactLoggerAdapter = cautionaryContactLoggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        // GET Cautionary Contacts
        /// <summary>
        /// Gets the cautionary contact notes for a address
        /// </summary>
        /// <param name="reference">Use the UH Property reference number to get the alerts</param>
        /// <returns>A list of cautionary contact details and alerts related to the property</returns>
        /// <response code="200">Returns the list of Cautionary notes</response>
        /// <response code="404">If the cautionary contact information is not found</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet]
        public async Task<JsonResult> GetCautionaryContactByRef(string reference)
        {
            try
            {
                CautionaryContactActions actions = new CautionaryContactActions(_cautionaryContactService, _cautionaryContactLoggerAdapter);
                var result = await actions.GetCautionaryContactByRef(reference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingCautionaryContactException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Resource identification error", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }
    }
}