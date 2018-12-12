using System;
using System.Collections.Specialized;
using System.Collections;
namespace HackneyRepairs.ExtensionMethods
{
    public static class Extensions
    {
        public static NameValueCollection ToNameValueCollection(this Hashtable entries)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (DictionaryEntry entry in entries)
            {
                string value = null;
                if (entry.Key != null)
                    value = entry.Value.ToString();

                nameValueCollection.Add(entry.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}
