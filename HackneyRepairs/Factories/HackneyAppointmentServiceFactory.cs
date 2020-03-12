using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Tests;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Factories
{
    public class HackneyAppointmentServiceFactory
    {
		public IHackneyAppointmentsService build(ILoggerAdapter<AppointmentActions> logger, IUhtRepository uhtRepository, IDRSRepository dRSRepository, ICacheRepository cacheRepository)
        {
            if (TestStatus.IsRunningInTests == false)
            {
				return new Services.HackneyAppointmentsService(logger, uhtRepository, dRSRepository, cacheRepository);
            }
            else
            {
                return new Services.FakeAppointmentService();
            }
        }
    }
}
