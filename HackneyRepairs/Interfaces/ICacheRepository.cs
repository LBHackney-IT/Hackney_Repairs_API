using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheRepository
    {
		DetailedAppointment GetCachedLatestAppointmentByWorkOrderReference(string workOrderReference);
		IEnumerable<DetailedAppointment> GetCachedAppointmentsByWorkOrderReference(string workOrderReference);
        bool SetAppointmentCache(DetailedAppointment appointment);
        bool DeleteAppointmentCache(string workOrderReference);
    }
}
