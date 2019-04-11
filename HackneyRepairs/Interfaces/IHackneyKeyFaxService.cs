using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyKeyFaxService
    {
        Task<StartupResponse> GetKeyFaxLaunchURL(string startupRequestXML);
        Task<GetResultsResponse> GetKeyFaxResults(string companyCode, string keyfaxGUID);
    }
}
