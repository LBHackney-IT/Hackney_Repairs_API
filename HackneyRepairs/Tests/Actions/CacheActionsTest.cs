using System.Threading.Tasks;
using HackneyRepairs.Actions;
using Moq;
using Xunit;
using System.Text;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using RepairsService;
using Newtonsoft.Json;
using System.Collections.Generic;
using HackneyRepairs.Logging;
using HackneyRepairs.DTOs;
using System;

namespace HackneyRepairs.Tests.Actions
{
    public class CacheActionsTest
    {
        //[Fact]
        //public async Task get_cached_item_with_null_key_gets_null_object()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "null";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    fakeCacheService.Setup(service => service.GetCacheItem<string>(key));
      
        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.GetCacheItem(key);
            
        //    Assert.Null(result);
        //}

        //[Fact]
        //public async Task get_cached_item_with_int_key_gets_int_object()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "int";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    fakeCacheService.Setup(service => service.GetCacheItem<string>(key));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.GetCacheItem(key);

        //    Assert.IsType<int>(result);
        //    Assert.Equal(1234, result);
        //}

        //[Fact]
        //public async Task get_cached_item_with_string_key_gets_string_object()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "string";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    fakeCacheService.Setup(service => service.GetCacheItem<string>(key));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.GetCacheItem(key);

        //    Assert.IsType<string>(result);
        //    Assert.Equal("This is the cached value for 54321", result);
        //}

        //[Fact]
        //public async Task put_cached_item_with_string_key_gets_success()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "success";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    var itemtobepushed = "bob";
        //    TimeSpan ttl = new TimeSpan(123456);
        //    fakeCacheService.Setup(service => service.PutCachedItem<string>(itemtobepushed, key, ttl));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.PutCachedItem(itemtobepushed, key, ttl);

        //    Assert.IsType<bool>(result);
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task put_cached_item_with_string_key_gets_failure()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "failure";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    var itemtobepushed = "bob";
        //    TimeSpan ttl = new TimeSpan(123456);
        //    fakeCacheService.Setup(service => service.PutCachedItem<string>(itemtobepushed, key, ttl));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.PutCachedItem(itemtobepushed, key, ttl);

        //    Assert.IsType<bool>(result);
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task delete_cached_item_with_string_key_gets_success()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "success";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    fakeCacheService.Setup(service => service.DeleteCacheItem(key));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.DeleteCacheItem(key);

        //    Assert.IsType<bool>(result);
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task delete_cached_item_with_string_key_gets_failure()
        //{
        //    var mockLogger = new Mock<ILoggerAdapter<CacheActions>>();
        //    var key = "failure";
        //    var fakeCacheService = new Mock<ICacheService>();
        //    fakeCacheService.Setup(service => service.DeleteCacheItem(key));

        //    var CacheActions = new CacheActions(fakeCacheService.Object, mockLogger.Object);
        //    var result = CacheActions.DeleteCacheItem(key);

        //    Assert.IsType<bool>(result);
        //    Assert.False(result);
        //}
    }
}
