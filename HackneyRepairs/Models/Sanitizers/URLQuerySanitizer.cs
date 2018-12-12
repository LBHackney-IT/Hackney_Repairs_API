using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Sanitizers
{
    public class URLQuerySanitizer
    {
        private string _URL;
        public string URL 
        {
            get
            {
                return _URL;
            }
            set
            {
                _URL = URL;
            }
        }

        public URLQuerySanitizer(string queryString)
        {
            URL = queryString;
        }
    }
}
