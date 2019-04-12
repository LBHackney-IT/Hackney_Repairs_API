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
        private IUHWWarehouseRepository _uhWarehouseRepository;
        private ILoggerAdapter<CautionaryContactActions> _logger;
        public HackneyCautionaryContactService(IUHWWarehouseRepository uHWWarehouseRepository, ILoggerAdapter<CautionaryContactActions> logger)
        {
            _uhWarehouseRepository = uHWWarehouseRepository;
            _logger = logger;
        }

        public async Task<CautionaryContactLevelModel[]> GetCautionaryContactByFirstLineOfAddress(string firstLineOfAddress)
        {
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByFirstLineOfAddress(): Sent request to upstream data warehouse (FirstLineOfAddress: {firstLineOfAddress})");
            var response = await _uhWarehouseRepository.GetCautionaryContactByFirstLineOfAddress(firstLineOfAddress);
            _logger.LogInformation($"HackneyCautionaryContactService/GetCautionaryContactByFirstLineOfAddress(): Received response from upstream data warehouse (FirstLineOfAddress: {firstLineOfAddress})");
            return response;
        }
    }
}
