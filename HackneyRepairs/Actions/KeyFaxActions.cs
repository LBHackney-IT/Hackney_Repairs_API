using HackneyRepairs.Interfaces;
using System.Threading.Tasks;
using System;

namespace HackneyRepairs.Actions
{
    public class KeyFaxActions
    {
        public IHackneyKeyFaxService _keyfaxService;
        public IHackneyKeyFaxServiceRequestBuilder _requestBuilder;
        public ILoggerAdapter<KeyFaxActions> _logger;

        public KeyFaxActions(IHackneyKeyFaxService keyfaxService, IHackneyKeyFaxServiceRequestBuilder requestBuilder, ILoggerAdapter<KeyFaxActions> logger)
        {
            _keyfaxService = keyfaxService;
            _requestBuilder = requestBuilder;
            _logger = logger;
        }

        public async Task<object> GetStartUpURL()
        {
            _logger.LogInformation($"Getting KeyFax Start up URL");
            var response = await _keyfaxService.GetKeyFaxLaunchURL(_requestBuilder.StartUpXML);
            return response;
        }

        public async Task<object> GetResults(string keyfaxGUID)
        {
            string _companyCode = Environment.GetEnvironmentVariable("KFCompanyCode");
            _logger.LogInformation($"Getting KeyFax results for GUID: {keyfaxGUID}");
            var response = await _keyfaxService.GetKeyFaxResults(_companyCode, keyfaxGUID);
            return response;
        }
    }
}
