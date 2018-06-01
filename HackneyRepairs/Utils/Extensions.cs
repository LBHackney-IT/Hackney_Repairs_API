﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
namespace HackneyRepairs.ExtensionMethods
{
    public static class Extensions
    {
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                string value = null;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }
            return nameValueCollection;
        }
    }
}
