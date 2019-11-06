using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Models
{
    public class CautionaryContactLevelModel
    {
        public CautionaryContactLevelModel()
        { }

        public IList<AddressAlert> AddressAlerts { get; set; }
        public IList<ContactAlert> ContactAlerts { get; set; }
        public IList<string> CallerNotes { get; set; }
    }

    public class AddressAlert
    {
        public string AlertCode { get; set; }
        public string AlertDescription { get; set; }
    }

    public class ContactAlert
    {
        public string AlertCode { get; set; }
        public string AlertDescription { get; set; }
    }
}
