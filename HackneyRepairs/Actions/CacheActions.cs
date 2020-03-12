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

        public Task<object> GetCacheItem(string key)
        {
            _logger.LogInformation($"Retrieving cache item at {key}");
            var response = _cacheService.GetCacheItem<object>(key);
            return response;
        }

        public async Task<object> DeleteCacheItem(string key)
        {
            _logger.LogInformation($"Deleting cache item at {key}");
            var response = await _cacheService.DeleteCacheItem(key);
            return response;
        }        
    }
}