using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Infrastructure;
using Newtonsoft.Json;

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

        public bool DeleteAppointmentCache(string workOrderReference)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DetailedAppointment> GetCachedAppointmentsByWorkOrderReference(string workOrderReference)
        {
            throw new NotImplementedException();
        }

        public DetailedAppointment GetCachedLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            DetailedAppointment detailedAppointment = new DetailedAppointment();
            string cacheKey = "appointment:workorder:";
            try
            {
                _logger.LogInformation($"Getting current appointment details from cache for {workOrderReference}");
                var cache = CacheManager.Cache;
                var appointment = cache.StringGet(cacheKey + workOrderReference);
                if (appointment.IsNull)
                {
                    _logger.LogInformation($"Cache miss for {workOrderReference}");
                    return detailedAppointment;
                }
                else
                {
                    _logger.LogInformation($"Cache hit for {workOrderReference}");
                    return JsonConvert.DeserializeObject<DetailedAppointment>(appointment);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new CacheRepositoryException(ex.Message);
            }
            
        }

        public bool SetAppointmentCache(DetailedAppointment appointment)
        {
            throw new NotImplementedException();
        }
    }

    public class CacheRepositoryException : Exception
    {
        public CacheRepositoryException() { }
        public CacheRepositoryException(string message) : base(message)
        { }
    }
}
