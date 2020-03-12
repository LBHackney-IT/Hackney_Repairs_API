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
    }
}
