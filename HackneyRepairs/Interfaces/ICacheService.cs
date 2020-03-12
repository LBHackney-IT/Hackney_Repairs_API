using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheService
    {
        Task<bool> DeleteCacheItem(string key);
    }
}
