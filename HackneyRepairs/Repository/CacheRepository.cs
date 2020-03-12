using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Infrastructure;

namespace HackneyRepairs.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private CacheManager _cacheManager;
        private ILoggerAdapter<CacheRepository> _logger;

        public CacheRepository(CacheManager cacheManager, ILoggerAdapter<CacheRepository> logger)
        {
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public Task<bool> DeleteAppointmentCache(string workOrderReference)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
        {
            throw new NotImplementedException();
        }

        public Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetAppointmentCache(DetailedAppointment appointment)
        {
            throw new NotImplementedException();
        }
    }
}
