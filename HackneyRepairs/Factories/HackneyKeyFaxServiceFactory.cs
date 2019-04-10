using System;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Services;
using HackneyRepairs.Tests;

namespace HackneyRepairs.Factories
{
    public class HackneyKeyFaxServiceFactory
    {
        public IHackneyKeyFaxService build(ILoggerAdapter<KeyFaxActions> logger)
        {
            if (TestStatus.IsRunningInTests == false)
            {
                return new HackneyKeyFaxService(logger);
            }
            else
            {
                return new FakeKeyFaxService();
            }
        }
    }
}
