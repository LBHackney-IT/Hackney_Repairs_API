using HackneyRepairs.Interfaces;
using HackneyRepairs.Tests;
using HackneyRepairs.Actions;

namespace HackneyRepairs.Factories
{
    public class HackneyRepairsServiceFactory
    {
        public IHackneyRepairsService build(IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository, ILoggerAdapter<RepairsActions> logger)
        {
            if (TestStatus.IsRunningInTests == false)
            {
                return new Services.HackneyRepairsService(uhtRepository, uhwRepository, uHWWarehouseRepository, logger);
            }
            else
            {
                return new Services.FakeRepairsService();
            }
        }
    }
}
