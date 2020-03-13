using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HackneyRepairs.Actions
{
    public class CacheActions
    {
        public ICacheService _cacheService;
        public ILoggerAdapter<CacheActions> _logger;

        public CacheActions(ICacheService cacheService, ILoggerAdapter<CacheActions> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public bool PutCachedItem<T>(T objectToBeCached, string key)
        {
            _logger.LogInformation($"Adding cache item at {key}");
            return _cacheService.PutCachedItem(objectToBeCached, key);
        }

        public object GetCacheItem(string key)
        {
            _logger.LogInformation($"Retrieving cache item at {key}");
            return _cacheService.GetCacheItem<object>(key);
        }

        public bool DeleteCacheItem(string key)
        {
            _logger.LogInformation($"Deleting cache item at {key}");
            return _cacheService.DeleteCacheItem(key);
        }        
    }
}