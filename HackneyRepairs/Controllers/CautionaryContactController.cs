using System;
using System.Collections;
using System.Collections.Generic;
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
        private IHackneyCautionaryContactRequestBuilder _cautionaryContactRequestBuilder;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;

        public CautionaryContactController(IHackneyCautionaryContactService _cautionaryContactService, IHackneyCautionaryContactRequestBuilder _cautionaryContactRequestBuilder, ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter)
        {
            HackneyCautionaryContactServiceFactory cautionaryContactFactory = new HackneyCautionaryContactServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
        }

        [HttpGet]
        public async Task<JsonResult> GetCautionaryContactByFirstLineOfAddress()
        {
            CautionaryContactActions actions = new CautionaryContactActions(_cautionaryContactService, _cautionaryContactRequestBuilder, _cautionaryContactLoggerAdapter);
            var result = await actions.GetCautionaryContactByFirstLineOfAddress();
            return ResponseBuilder.Ok(result);
        }
    }
}