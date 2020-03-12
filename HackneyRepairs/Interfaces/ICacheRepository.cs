using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheRepository
    {
        T GetCachedItemByKey<T>(string key)
            where T : class;
        void SetCache<T>(T objectToBeCached, string key);
        bool DeleteCachedItem(string key);
    }
}
