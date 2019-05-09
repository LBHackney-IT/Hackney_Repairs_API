using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Services
{
    public class HackneyCautionaryContactService : IHackneyCautionaryContactService
    {
        private IUhtRepository _uhtRepository;
        private ILoggerAdapter<CautionaryContactActions> _logger;
        public HackneyCautionaryContactService(IUhtRepository uhtRepository, ILoggerAdapter<CautionaryContactActions> logger)
        {
            _uhtRepository = uhtRepository;
            _logger = logger;
        }

        public async Task<CautionaryContactLevelModel[]> GetCautionaryContactByRef(string reference)
        {
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Sent request to upstream data warehouse (FirstLineOfAddress: {reference})");
            var response = await _uhtRepository.GetCautionaryContactByRef(reference);
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Received response from upstream data warehouse (FirstLineOfAddress: {reference})");
            return response;
        }
    }
}
