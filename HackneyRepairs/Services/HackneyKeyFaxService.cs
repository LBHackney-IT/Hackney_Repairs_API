using HackneyRepairs.Interfaces;
using HackneyRepairs.Actions;

namespace HackneyRepairs.Services
{
    public class HackneyKeyFaxService : IHackneyKeyFaxService
    {
        //private RepairServiceClient _client;
        private ILoggerAdapter<KeyFaxActions> _logger;

        public HackneyKeyFaxService(ILoggerAdapter<KeyFaxActions> logger)
        {
            //_client = new RepairServiceClient();
            _logger = logger;
        }
    }
}
