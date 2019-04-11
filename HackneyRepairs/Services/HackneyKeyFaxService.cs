using HackneyRepairs.Interfaces;
using HackneyRepairs.Actions;
using KeyFaxService;
using System.Threading.Tasks;

namespace HackneyRepairs.Services
{
    public class HackneyKeyFaxService : IHackneyKeyFaxService
    {
        private KeyfaxWSSoapClient _client;
        private ILoggerAdapter<KeyFaxActions> _logger;

        public HackneyKeyFaxService(ILoggerAdapter<KeyFaxActions> logger)
        {
            //_client = new RepairServiceClient();
            _logger = logger;
        }

        public async Task<StartupResponse> GetKeyFaxLaunchURL(string startupRequestXML)
        {
            //Create KeyFax soap client object
            _client = new KeyfaxWSSoapClient(KeyfaxWSSoapClient.EndpointConfiguration.KeyfaxWSSoap);
            _logger.LogInformation($"HackneyKeyFaxService/GetKeyFaxLaunchURL(): Sent request to upstream KeyFaxServiceClient (Request ref: testing)");
            var response = await _client.StartupAsync(startupRequestXML);
            _logger.LogInformation($"HackneyKeyFaxService/GetKeyFaxLaunchURL(): Received response from upstream KeyFaxServiceClient (Request ref: testing)");
            return response;
        }
    }
}
