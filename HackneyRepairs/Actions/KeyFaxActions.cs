using HackneyRepairs.Interfaces;
using System.Threading.Tasks;

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

        //public async Task<object> GetStartUpURL(RepairRequest request)
        //{
        //    if (request.WorkOrders != null)
        //    {
        //        return await CreateRepairWithOrder(request);
        //    }
        //    else
        //    {
        //        return await CreateRepairWithoutOrder(request);
        //    }
        //}

        //public async Task<object> GetKeyFaxResult(string kfGUID)
        //{

        //}
    }
}
