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
            _logger.LogInformation($"ActionLevel: Getting cautionary contact by first line of address: {reference}");
            try
            {
                var response = await _cautionaryContactService.GetCautionaryContactByRef(reference);
                return new { results = response };
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding property by address: {reference} returned an error: {e.Message}");
                throw new CautionaryContactServiceException();
            }
        }
    }

    public class CautionaryContactServiceException : Exception { }
    public class MissingCautionaryContactException : Exception { }
}
