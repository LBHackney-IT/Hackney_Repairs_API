using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Infrastructure;
using Newtonsoft.Json;

namespace HackneyRepairs.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private CacheManager _cacheManager;
        private ILoggerAdapter<CacheRepository> _logger;        

        public CacheRepository(CacheManager cacheManager, ILoggerAdapter<CacheRepository> logger)
        {
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public bool DeleteCachedItem(string key)
        {
            var cache = CacheManager.Cache;
            return cache.KeyDelete(key);
        }

        public T GetCachedItemByKey<T>(string key)
            where T : class
        {
            var objectType = typeof(T);
            _logger.LogInformation($"Getting item type {objectType.Name} from cache for {key}");
            var cache = CacheManager.Cache;
            var cachedItem = cache.StringGet(key);
            if (cachedItem.IsNull)
            {
                _logger.LogInformation($"Cache miss for {key}");
                return null;
            }
            else
            {
                _logger.LogInformation($"Cache hit for {key}");
                return JsonConvert.DeserializeObject<T>(cachedItem);
            }
        }             

        public void SetCache<T>(T objectToBeCached, string key)
        {
            try
            {
                var jAppointment = JsonConvert.SerializeObject(objectToBeCached);                
                var cache = CacheManager.Cache;
                var objectType = objectToBeCached.GetType();
                _logger.LogInformation($"Setting {key} for {objectType.Name} in cache");
                cache.StringSet(key, jAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new CacheRepositoryException(ex.Message);
            }
        }
    }

    public class CacheRepositoryException : Exception
    {
        public CacheRepositoryException() { }
        public CacheRepositoryException(string message) : base(message)
        { }
    }
}
