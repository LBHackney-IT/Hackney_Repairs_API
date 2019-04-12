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

        public async Task<CautionaryContactLevelModel[]> GetCautionaryContactByFirstLineOfAddress(string firstLineOfAddress)
        {
            var response = await _uhWarehouseRepository.GetCautionaryContactByFirstLineOfAddress(firstLineOfAddress);
            return response;
        }
    }
}
