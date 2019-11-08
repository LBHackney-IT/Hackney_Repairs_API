using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Services
{
    public class FakeCautionaryContactService : IHackneyCautionaryContactService
    {
        public Task<CautionaryContactLevelModel> GetCautionaryContactByRef(string reference)
        {
            IList<CallerNote> callerNotes = new List<CallerNote>
            {
                new CallerNote
                {
                    DateCreated = new System.DateTime(637088021420000000),
                    UHUserFullName = "Cyril Vica",
                    UHUserName = "CVICA",
                    NoteText = "This is a note"
                }
            };
            IList<AddressAlert> addressAlerts = new List<AddressAlert>
            {
                new AddressAlert { AlertCode = "VA", AlertDescription = "This is a description" },
                new AddressAlert { AlertCode = "DIS", AlertDescription = "This is a description" }
            };
            IList<ContactAlert> contactAlerts = new List<ContactAlert>
            {
                new ContactAlert { AlertCode = "VA", AlertDescription = "This is a description" }
            };

            var cautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = callerNotes,
                AddressAlerts = addressAlerts,
                ContactAlerts = contactAlerts
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
