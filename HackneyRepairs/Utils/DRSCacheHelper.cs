using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace HackneyRepairs.Utils
{
    public class DRSCacheHelper
    {
        private NameValueCollection _configuration;

        public DRSCacheHelper(NameValueCollection configuration)
        {
            _configuration = configuration;
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        public TimeSpan getTTLForStatus(string status)
        {
            string[] drsStatusLookupOptions = _configuration.Get("DRSStatusTTL").Split('|');
            Dictionary<string, string> statusDictionary = new Dictionary<string, string>();
            for (int a = 0; a < drsStatusLookupOptions.Length; a++)
            {
                statusDictionary.Add(drsStatusLookupOptions[a].Split(',')[0], drsStatusLookupOptions[a].Split(',')[1]);
            }

            if (statusDictionary.ContainsKey(status.ToLower()))
            {
                try
                {
                    return TimeSpan.Parse(statusDictionary[status.ToLower()]);
                }
                catch (Exception)
                {
                    throw new InvalidStatusCodeException();
                }                
            }
            else
            {
                throw new InvalidStatusCodeException();
            }
        }    

        public class InvalidStatusCodeException : Exception
        {
        } 
    }
}
