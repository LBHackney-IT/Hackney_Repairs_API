using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Actions
{
    public class KeyFaxActions
    {
        public IHackneyRepairsService _repairsService;
        public IHackneyRepairsServiceRequestBuilder _requestBuilder;
        public ILoggerAdapter<KeyFaxActions> _logger;

        public KeyFaxActions(IHackneyRepairsService repairsService, IHackneyRepairsServiceRequestBuilder requestBuilder, ILoggerAdapter<KeyFaxActions> logger)
        {
            _repairsService = repairsService;
            _requestBuilder = requestBuilder;
            _logger = logger;
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
