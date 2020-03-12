using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface ICacheRepository
    {
		Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference);
		Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference);
        Task<bool> SetAppointmentCache(DetailedAppointment appointment);
        Task<bool> DeleteAppointmentCache(string workOrderReference);
    }
}
