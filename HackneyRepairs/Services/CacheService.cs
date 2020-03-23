using HackneyRepairs.Interfaces;
using HackneyRepairs.Actions;
using KeyFaxService;
using System.Threading.Tasks;
using System;

namespace HackneyRepairs.Services
{
    public class CacheService : ICacheService
    {
        private ILoggerAdapter<CacheActions> _logger;
        private ICacheRepository _cacheRepository;

        public CacheService(ILoggerAdapter<CacheActions> logger, ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
            _logger = logger;
        }

        public T GetCacheItem<T>(string key) where T : class
        {
            _logger.LogInformation($"CacheService/GetCacheItem(): Retrieving cache item for {key}");
            return _cacheRepository.GetCachedItemByKey<T>(key);
        }

        public bool PutCachedItem<T>(T objectToBeCached, string key, TimeSpan ttl)
        {
            _logger.LogInformation($"CacheService/PutCachedItem(): Sent PUT request for cache item {key}");
            return _cacheRepository.PutCachedItem<T>(objectToBeCached, key, ttl);
        }

        public bool DeleteCacheItem(string key)
        {
            _logger.LogInformation($"CacheService/DeleteCacheItem(): Sent delete request for cache item {key}");
            var response = _cacheRepository.DeleteCachedItem(key);
            _logger.LogInformation($"CacheServiceCacheService/DeleteCacheItem(): Delete response for cache item {key} = {response}");
            return response;
        }
    }
}
