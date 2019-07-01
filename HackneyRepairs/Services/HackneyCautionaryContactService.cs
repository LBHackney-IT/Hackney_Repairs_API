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
        private IUhwRepository _uhwRepository;
        private ILoggerAdapter<CautionaryContactActions> _logger;
        public HackneyCautionaryContactService(IUhwRepository uhwRepository, ILoggerAdapter<CautionaryContactActions> logger)
        {
            _uhwRepository = uhwRepository;
            _logger = logger;
        }

        public async Task<CautionaryContactLevelModel[]> GetCautionaryContactByRefA(string reference)
        {
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Sent request to upstream data warehouse (FirstLineOfAddress: {reference})");
            var response = await _uhwRepository.GetCautionaryContactByRef(reference);
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Received response from upstream data warehouse (FirstLineOfAddress: {reference})");
            return null;
        }

        public async Task<CautionaryContactLevelModel> GetCautionaryContactByRef(string reference)
        {
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Sent request to upstream data warehouse (property ref: {reference})");
            var response = await _uhwRepository.GetCautionaryContactByRef(reference);
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByRef(): Received response from upstream data warehouse (property ref: {reference})");
            return response;
        }
    }
}
