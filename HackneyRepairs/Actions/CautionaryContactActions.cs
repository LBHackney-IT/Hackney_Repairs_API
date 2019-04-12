﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Actions
{
    public class CautionaryContactActions
    {
        private IHackneyCautionaryContactService _cautionaryContactService;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;
        private readonly ILoggerAdapter<CautionaryContactActions> _logger;

        public CautionaryContactActions(IHackneyCautionaryContactService cautionaryContactService, ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter)
        {
            _cautionaryContactService = cautionaryContactService;
            _cautionaryContactLoggerAdapter = cautionaryContactLoggerAdapter;
        }

        public async Task<object> GetCautionaryContactByFirstLineOfAddress(string firstLineOfAddress)
        {
            _logger.LogInformation($"ActionLevel: Getting cautionary contact by first line of address: {firstLineOfAddress}");
            try
            {
                var response = await _cautionaryContactService.GetCautionaryContactByFirstLineOfAddress(firstLineOfAddress);
                return new object { results = response };

            }
            catch (Exception e)
            {

            }
        }
    }
}
