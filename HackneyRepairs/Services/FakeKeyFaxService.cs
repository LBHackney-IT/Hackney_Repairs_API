using HackneyRepairs.Interfaces;
using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Services
{
    public class FakeKeyFaxService : IHackneyKeyFaxService
    {
        public async Task<StartupResponse> GetKeyFaxLaunchURL(string startupRequestXML)
        {
            var responsebody = new StartupResponseBody
            {
                StartupResult = 
                {
                    LaunchUrl = @"http://lbhkfxappt01/InterView/OL2/Main.aspx?co=Hackney_41_OL2&amp;guid=876ff947-d132-4646-9257-bbffe3eeb4e6",
                    Guid = "876ff947-d132-4646-9257-bbffe3eeb4e6"
                }
            };

            var response = new StartupResponse
            {
                Body = responsebody
            };
            //return Task.Run(() => response);
            return response;
        }
    }
}
