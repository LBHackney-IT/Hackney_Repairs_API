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
            List<DetailedAppointment> detailedAppointments = new List<DetailedAppointment>();
            string cacheKey = "appointments:workorder:";
            try
            {
                _logger.LogInformation($"Getting current appointments details from cache for {workOrderReference}");
                var cache = CacheManager.Cache;
                var appointments = cache.StringGet(cacheKey + workOrderReference);
                if (appointments.IsNull)
                {
                    _logger.LogInformation($"Cache miss for {workOrderReference}");
                    return detailedAppointments;
                }
                else
                {
                    _logger.LogInformation($"Cache hit for {workOrderReference}");
                    return JsonConvert.DeserializeObject<List<DetailedAppointment>>(appointments);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new CacheRepositoryException(ex.Message);
            }
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

        public void SetAppointmentCache(DetailedAppointment appointment)
        {
            var workOrderReference = appointment.Id.ToString();            
            try
            {
                var jAppointment = JsonConvert.SerializeObject(appointment);
                string cacheKey = string.Format("appointment:workorder:{0}", workOrderReference);
                var cache = CacheManager.Cache;
                _logger.LogInformation($"Setting current appointment details from cache for {workOrderReference}");
                cache.StringSet(cacheKey, jAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new CacheRepositoryException(ex.Message);
            }
        }

        public void SetAppointmentsCache(List<DetailedAppointment> appointments)
        {
            var workOrderReference = appointments[0].Id.ToString();
            try
            {
                var jAppointment = JsonConvert.SerializeObject(appointments);
                string cacheKey = string.Format("appointments:workorder:{0}", workOrderReference);
                var cache = CacheManager.Cache;
                _logger.LogInformation($"Setting current appointments details from cache for {workOrderReference}");
                cache.StringSet(cacheKey, jAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new CacheRepositoryException(ex.Message);
            }
        }
    }

    public class CacheRepositoryException : Exception
    {
        public CacheRepositoryException() { }
        public CacheRepositoryException(string message) : base(message)
        { }
    }
}
