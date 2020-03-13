using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheService
    {
        bool PutCachedItem<T>(T objectToBeCached, string key);
        T GetCacheItem<T>(string key)
            where T : class;
        bool DeleteCacheItem(string key);
    }
}
