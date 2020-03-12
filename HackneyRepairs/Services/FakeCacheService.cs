using HackneyRepairs.Interfaces;
using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Services
{
    public class FakeCacheService : ICacheService
    {
        public async Task<bool> DeleteCacheItem(string key)
        {
            var response = true;
            return response;
        }

        public Task<T> GetCacheItem<T>(string key) where T : class
        {
            throw new System.NotImplementedException();
        }

        //public Task<T> GetCacheItem<T>(string key) where T : class
        //{            
        //    throw new Exce
        //    //switch (key)
        //    //{
        //    //    case "1234":
        //    //        return null;
        //    //    case "54321":
        //    //        return Task.Run(() => "This is the cached value");
        //    //    default:
        //    //        return Task.Run(() => "This is the cached value");
        //    //}
        //}
    }
}
