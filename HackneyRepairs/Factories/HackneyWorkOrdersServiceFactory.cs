using HackneyRepairs.Interfaces;
using HackneyRepairs.Tests;
using HackneyRepairs.Actions;

namespace HackneyRepairs.Factories
{
    public class HackneyWorkOrdersServiceFactory
    {
	public IHackneyWorkOrdersService build(IUhtRepository uhtRepository, ILoggerAdapter<WorkOrdersActions> logger)
        {
            if (TestStatus.IsRunningInTests == false)
            {
                return new Services.HackneyWorkOrdersService(uhtRepository, logger);
            }
            else
            {
		return new Services.FakeWorkOrdersService();
            }
        }
    }
}
