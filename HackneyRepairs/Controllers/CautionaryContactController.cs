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
        private IHackneyCautionaryContactServiceRequestBuilder _cautionaryContactRequestBuilder;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;

        public CautionaryContactController(IHackneyCautionaryContactService cautionaryContactService, IHackneyCautionaryContactServiceRequestBuilder cautionaryContactRequestBuilder, ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository)
        {
            HackneyCautionaryContactServiceFactory cautionaryContactFactory = new HackneyCautionaryContactServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _cautionaryContactService = cautionaryContactFactory.build(uhtRepository, uHWWarehouseRepository, cautionaryContactLoggerAdapter);
            _cautionaryContactRequestBuilder = HackneyCautionaryContactServiceRequestBuilder(_configBuilder.getConfiguration());
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