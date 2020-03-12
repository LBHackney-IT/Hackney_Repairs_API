using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetCacheItem<T>(string key)
            where T : class;
        Task<bool> DeleteCacheItem(string key);
    }
}
