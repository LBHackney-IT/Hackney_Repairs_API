using HackneyRepairs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyCautionaryContactService
    {
        Task<CautionaryContactLevelModel[]> GetCautionaryContactByFirstLineOfAddress(string firstLineOfAddress);
    }
}
