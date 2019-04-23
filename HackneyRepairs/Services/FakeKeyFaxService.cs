using HackneyRepairs.Interfaces;
using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Services
{
    public class FakeKeyFaxService : IHackneyKeyFaxService
    {
        public async Task<StartupResponse> GetKeyFaxLaunchURLAsync(string startupRequestXML)
        {
            var responsebody = new StartupResponseBody
            {
                StartupResult = 
                {
                    LaunchUrl = @"http://lbhkfxappt01/InterView/OL2/Main.aspx?co=blah",
                    Guid = "876ff947-d132-4646-9257-bbffe3eeb4e6"
                }
            };

            var response = new StartupResponse
            {
                Body = responsebody
            };
            //return Task.Run(() => response);
            return response;

            //"startupResult":
            //"errorText": "Startup failed: Company / configuration 'Hackney_41_OL': Unable to load D:\\KeyfaxData\\KFLauncher\\Hackney_41_OL_config.xml: " +
            //    "Could not find file 'D:\\KeyfaxData\\KFLauncher\\Hackney_41_OL_config.xml'."
        }

        public async Task<GetResultsResponse> GetKeyFaxResultsAsync(string companyCode, string keyfaxGUID)
        {
            var responsebody = new GetResultsResponseBody
            {
                GetResultsResult =
                {
                    ResultXml = "<sorcode>2000520</sorcode>"
                }
            };

            var response = new GetResultsResponse
            {
                Body = responsebody
            };
            return response;
        }
    }
}
