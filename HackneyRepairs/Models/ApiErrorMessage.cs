using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Models
{
    public class ApiErrorMessage
    {
        public string DeveloperMessage { get; set; }
        public string UserMessage { get; set; }
    }

    public class JsonApiErrorMessage
    {
        public int Code { get; set; }
        public string Source { get; set; }
        public string DeveloperMessage { get; set; }
        public string UserMessage { get; set; }
    }
}
