using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Tests;
using HackneyRepairs.Interfaces;
using System.Collections.Specialized;

namespace HackneyRepairs.Factories
{
    public class HackneyAppointmentServiceFactory
    {
		public IHackneyAppointmentsService build(ILoggerAdapter<AppointmentActions> logger, IUhtRepository uhtRepository, IDRSRepository dRSRepository, ICacheRepository cacheRepository, NameValueCollection configuration)
        {
            if (TestStatus.IsRunningInTests == false)
            {
				return new Services.HackneyAppointmentsService(logger, uhtRepository, dRSRepository, cacheRepository, configuration);
            }
            else
            {
                return new Services.FakeAppointmentService();
            }
        }
    }
}
