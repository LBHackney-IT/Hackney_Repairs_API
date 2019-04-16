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
        public Task<CautionaryContactLevelModel[]> GetCautionaryContactByRef(string reference)
        {
            var CautionaryContactList = new CautionaryContactLevelModel[2];
            CautionaryContactLevelModel[] emptyCautionaryContactList;
            var cautionaryContact1 = new CautionaryContactLevelModel()
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };
            var cautionaryContact2 = new CautionaryContactLevelModel()
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };
            CautionaryContactList[0] = cautionaryContact1;
            CautionaryContactList[1] = cautionaryContact2;
            switch (reference)
            {
                case "Acacia":
                    return Task.Run(() => CautionaryContactList);
                case "Elmbridge":
                    emptyCautionaryContactList = null;
                    return Task.Run(() => emptyCautionaryContactList);
                default:
                    emptyCautionaryContactList = new CautionaryContactLevelModel[0];
                    return Task.Run(() => emptyCautionaryContactList);
            }
        }
    }
}
