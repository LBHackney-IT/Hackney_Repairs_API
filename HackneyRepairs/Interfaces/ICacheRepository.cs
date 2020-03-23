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

        bool PutCachedItemNoTTL<T>(T objectToBeCached, string key);
        bool PutCachedItem<T>(T objectToBeCached, string key, TimeSpan ttl);
        bool DeleteCachedItem(string key);
    }
}
