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

        public async Task<object> GetStartUpURLAsync()
        {
            _logger.LogInformation($"Getting KeyFax Start up URL");
            var response = await _keyfaxService.GetKeyFaxLaunchURLAsync(_requestBuilder.StartUpXML);
            return response;
        }

        public async Task<object> GetResultsAsync(string keyfaxGUID)
        {
            string _companyCode = Environment.GetEnvironmentVariable("KFCompanyCode");
            _logger.LogInformation($"Getting KeyFax results for GUID: {keyfaxGUID}");
            var response = await _keyfaxService.GetKeyFaxResultsAsync(_companyCode, keyfaxGUID);
            return response;
        }
    }
}
