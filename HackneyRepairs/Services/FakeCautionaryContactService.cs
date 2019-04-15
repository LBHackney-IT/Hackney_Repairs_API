using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Services
{
    public class FakeCautionaryContactService : IHackneyCautionaryContactService
    {
        public Task<CautionaryContactLevelModel[]> GetCautionaryContactByRef(string firstLineOfAddress)
        {
            throw new NotImplementedException();
        }
    }
}
