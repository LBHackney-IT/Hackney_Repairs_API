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
    public class CautionaryContactController : Controller
    {
        private IHackneyCautionaryContactService _cautionaryContactService;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private readonly IExceptionLogger _exceptionLogger;

        public CautionaryContactController(IHackneyCautionaryContactService cautionaryContactService, ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository, IExceptionLogger exceptionLogger)
        {
            HackneyCautionaryContactServiceFactory cautionaryContactFactory = new HackneyCautionaryContactServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _cautionaryContactService = cautionaryContactFactory.build(uhtRepository, uHWWarehouseRepository, cautionaryContactLoggerAdapter);
            _cautionaryContactLoggerAdapter = cautionaryContactLoggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        // GET properties
        /// <summary>
        /// Gets a property or properties for a particular postcode
        /// </summary>
        /// <param name="firstLineOfAddress">First line of the propterty address being requested</param>
        /// <returns>A list of properties matching the specified first line of address</returns>
        /// <response code="200">Returns the list of properties</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response>
        [HttpGet("firstLineOfAddress")]
        public async Task<JsonResult> GetCautionaryContactByFirstLineOfAddress(string firstLineOfAddress)
        {
            CautionaryContactActions actions = new CautionaryContactActions(_cautionaryContactService, _cautionaryContactLoggerAdapter);
            var result = await actions.GetCautionaryContactByFirstLineOfAddress(firstLineOfAddress);
            return ResponseBuilder.Ok(result);
        }
    }
}