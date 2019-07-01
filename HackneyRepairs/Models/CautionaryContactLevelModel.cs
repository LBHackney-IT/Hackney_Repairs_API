using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Models
{
    public class CautionaryContactLevelModel
    {
        public CautionaryContactLevelModel()
        { 
            AlertCodes = new List<string>();
        }
       
        public IList<string> AlertCodes { get; set; }
        public IList<string> CallerNotes { get; set; }
    }
}
