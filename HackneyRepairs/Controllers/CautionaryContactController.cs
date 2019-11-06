using HackneyRepairs.Actions;
using HackneyRepairs.Builders;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Repository;
using HackneyRepairs.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Configuration;
using System.Threading.Tasks;

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
        /// <param name="reference">Use the UH Property reference number to get a list of alerts</param>
        /// <returns>A list of cautionary contact alerts and any caller notes associated with the property</returns>
        /// <response code="200">Returns an object with a list of cautionary alerts and any caller notes</response>
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
            catch (UhwRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to get cautionary contact details. " +
                    "Please raise a ticket at https://support.hackney.gov.uk including the details of this error, the repair or " +
                    "property and a screenshot. ", ex.Message);
            }
        }
    }
}