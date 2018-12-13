using System;
using System.Collections;
using System.Collections.Specialized;
using HackneyRepairs.ExtensionMethods;

namespace HackneyRepairs.Services
{
    public class HackneyConfigurationBuilder
    {
        private NameValueCollection _configuration;
        public HackneyConfigurationBuilder(Hashtable env_variables, NameValueCollection config_items)
        {
            if (env_variables != null)
            {
                _configuration = env_variables.ToNameValueCollection();
            }
            else
            {
                _configuration = config_items;
            }
        }

        public NameValueCollection getConfiguration()
        {
            return _configuration;
        }
    }
}
