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
        public Task<CautionaryContactLevelModel> GetCautionaryContactByRef(string reference)
        {
            string[] alertCodes =
            {
                "VA", "PV"
            };

            var cautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = "Don't come its not Healthy",
                AlertCodes = alertCodes.ToList()
            };
           
            switch (reference)
            {
                case "00000123":
                    return Task.Run(() => cautionaryContact); 
                case "00000000":
                    cautionaryContact = new CautionaryContactLevelModel();
                    return Task.Run(() => cautionaryContact);
                default:
                    return Task.Run(() => cautionaryContact);
            }
        }
    }
}
