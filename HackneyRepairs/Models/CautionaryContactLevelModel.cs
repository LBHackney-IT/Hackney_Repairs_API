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
        public string CallerNotes { get; set; }
    }

    //public class CallerContactNotes
    //{
    //    public string CallerNotes { get; set; }
    //}
}
