using System;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Builders;
using System.Threading.Tasks;
using HackneyRepairs.Factories;
using HackneyRepairs.Services;
using System.Xml;
using Newtonsoft.Json;
using System.Collections;
using System.Configuration;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/repairscache")]
    public class CacheController : Controller
    {
        private readonly IExceptionLogger _exceptionLogger;
        private ILoggerAdapter<CacheActions> _loggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private ICacheService _cacheService;

        public CacheController(ILoggerAdapter<CacheActions> loggerAdapter, IExceptionLogger exceptionLogger, ICacheRepository cacheRepository)
        {
            var factory = new HackneyCacheServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _cacheService = factory.build(loggerAdapter, cacheRepository);
            _loggerAdapter = loggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        [HttpPut]
        public async Task<JsonResult> PutCacheItem([FromBody]string bodyToCache, string key)
        {
            try
            {
                CacheActions actions = new CacheActions(_cacheService, _loggerAdapter);
                var result = actions.PutCachedItem(bodyToCache, key);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, string.Format("We were unable to put the cache item at {0}", key), ex.Message);
            }
        }

        [HttpGet]
        [Route("v1/cache/cacheitem/{key}")]
        public async Task<JsonResult> GetCacheItem(string key)
        {
            try
            {
                CacheActions actions = new CacheActions(_cacheService, _loggerAdapter);
                var result = actions.GetCacheItem(key);
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, string.Format("We were unable to retrieve the cache item at {0}", key), ex.Message);
            }
        }

        [HttpDelete]
        [Route("v1/cache/cacheitem/{key}")]
        public async Task<JsonResult> DeleteCacheItem(string key)
        {
            try
            {
                CacheActions actions = new CacheActions(_cacheService, _loggerAdapter);
                var result = actions.DeleteCacheItem(key);
                if (result)
                {
                    return ResponseBuilder.OkEmpty(result);
                }
                else
                {
                    return ResponseBuilder.NoItem(result);
                }
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, string.Format("We were unable to delete the cache item at {0}", key), ex.Message);
            }
        }
    }
}