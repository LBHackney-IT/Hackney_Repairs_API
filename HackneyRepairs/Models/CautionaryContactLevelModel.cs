using System;
using System.Collections.Generic;

namespace HackneyRepairs.Models
{
    public class CautionaryContactLevelModel
    {
        public CautionaryContactLevelModel()
        { }

        public IList<AddressAlert> AddressAlerts { get; set; }
        public IList<ContactAlert> ContactAlerts { get; set; }
        public IList<CallerNote> CallerNotes { get; set; }
    }

    public class CallerNote
    {
        public string UHUserName { get; set; }
        public string UHUserFullName { get; set; }
        public string NoteText { get; set; }
        public DateTime DateCreated { get; set;  }
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
