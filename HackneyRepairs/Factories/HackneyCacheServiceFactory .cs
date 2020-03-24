using System;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Services;
using HackneyRepairs.Tests;

namespace HackneyRepairs.Factories
{
    public class HackneyCacheServiceFactory
    {
        public ICacheService build(ILoggerAdapter<CacheActions> logger, ICacheRepository cacheRepository)
        {
            if (TestStatus.IsRunningInTests == false)
            {
                return new CacheService(logger, cacheRepository);
            }
            else
            {
                return new FakeCacheService();
            }
        }
    }
}
