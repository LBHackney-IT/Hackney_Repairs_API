using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Models
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            ErrorMessages = new List<string>();
            Valid = true;
        }

        public bool Valid { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
