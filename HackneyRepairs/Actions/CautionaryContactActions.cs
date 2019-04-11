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
        private IHackneyCautionaryContactServiceRequestBuilder _cautionaryContactRequestBuilder;
        private ILoggerAdapter<CautionaryContactActions> _cautionaryContactLoggerAdapter;

        public CautionaryContactActions(IHackneyCautionaryContactService cautionaryContactService, IHackneyCautionaryContactServiceRequestBuilder cautionaryContactRequestBuilder, ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter)
        {
            _cautionaryContactService = cautionaryContactService;
            _cautionaryContactRequestBuilder = cautionaryContactRequestBuilder;
            _cautionaryContactLoggerAdapter = cautionaryContactLoggerAdapter;
        }

        public async Task<object> GetCautionaryContactByFirstLineOfAddress()
        {
            throw new NotImplementedException();
        }
    }
}
