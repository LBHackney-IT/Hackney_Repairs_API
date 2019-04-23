using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyKeyFaxService
    {
        Task<StartupResponse> GetKeyFaxLaunchURLAsync(string startupRequestXML);
        Task<GetResultsResponse> GetKeyFaxResultsAsync(string companyCode, string keyfaxGUID);
    }
}
