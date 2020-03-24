using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Infrastructure
{
    public class CacheManager
    {
        private static ConfigurationOptions _configurationOptions;
        
        public CacheManager(ConfigurationOptions configurationOptions)
        {
            _configurationOptions = configurationOptions;
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configurationOptions));

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase Cache => Connection.GetDatabase();
    }
}
