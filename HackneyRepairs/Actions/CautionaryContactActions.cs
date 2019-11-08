using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Actions
{
    public class CautionaryContactActions
    {
        private IHackneyCautionaryContactService _cautionaryContactService;
        private readonly ILoggerAdapter<CautionaryContactActions> _logger;

        public CautionaryContactActions(IHackneyCautionaryContactService cautionaryContactService, ILoggerAdapter<CautionaryContactActions> logger)
        {
            _cautionaryContactService = cautionaryContactService;
            _logger = logger;
        }

        public async Task<object> GetCautionaryContactByRef(string reference)
        {
            _logger.LogInformation($"ActionLevel: Getting cautionary contact by property reference: {reference}");
            var response = await _cautionaryContactService.GetCautionaryContactByRef(reference);
            return response;
        }
    }

    public class CautionaryContactServiceException : Exception { }
    //Missing cautionary contact not an axception? Never thrown
    //public class MissingCautionaryContactException : Exception { }
}
