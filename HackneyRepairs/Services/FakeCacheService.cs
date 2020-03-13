using HackneyRepairs.Interfaces;
using System.Threading.Tasks;
using KeyFaxService;
using System;

namespace HackneyRepairs.Services
{
    public class FakeCacheService : ICacheService
    {
        public bool DeleteCacheItem(string key)
        {
            switch (key)
            {
                case "success":
                    return true;
                case "failure":
                    return false;
                default:
                    return false;
            }
        }

        public T GetCacheItem<T>(string key) where T : class
        {
            switch (key)
            {
                case "null":
                    return null;
                case "string":
                    return (T)Convert.ChangeType("This is the cached value for 54321", typeof(T));
                case "int":
                    return (T)Convert.ChangeType(1234, typeof(T));
                default:
                    return (T)Convert.ChangeType("This is the cached value", typeof(T)); 
            }
        }

        public bool PutCachedItem<T>(T objectToBeCached, string key)
        {
            switch (key)
            {
                case "success":
                    return true;
                case "failure":
                    return false;
                default:
                    return false;
            }
        }
    }
}
