﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DrsAppointmentsService;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Utils;

namespace HackneyRepairs.Services
{
    public class HackneyAppointmentsService : IHackneyAppointmentsService
    {
        private const string CacheKeyAppointments_s_jobs = "appointments:s_job:workorder:";
        private const string CacheKeyAppointments_p_jobs = "appointments:p_job:workorder:"; 
        private readonly SOAPClient _client;
        private IUhtRepository _uhtRepository;
        private ILoggerAdapter<AppointmentActions> _logger;
        private IDRSRepository _drsRepository;
        private ICacheRepository _cacheRepository;
        private NameValueCollection _configuration;
        private DRSCacheHelper _cacheHelper;

        public HackneyAppointmentsService(ILoggerAdapter<AppointmentActions> logger, IUhtRepository uhtRepository, IDRSRepository dRSRepository, ICacheRepository cacheRepository, NameValueCollection configuration)
        {
            _client = new SOAPClient();
            _uhtRepository = uhtRepository;
            _drsRepository = dRSRepository;
            _cacheRepository = cacheRepository;
            _logger = logger;
            _configuration = configuration;
            _cacheHelper = new DRSCacheHelper(_configuration);
        }

        public Task<checkAvailabilityResponse> GetAppointmentsForWorkOrderReference(xmbCheckAvailability checkAvailabilityRequest)
        {
            _logger.LogInformation($"HackneyAppointmentsService/GetAppointmentsForWorkOrderReference(): Sent request to upstream AppointmentServiceClient (Order Id: {checkAvailabilityRequest.theOrder.orderId})");
            var response = _client.checkAvailabilityAsync(checkAvailabilityRequest);
            _logger.LogInformation($"HackneyAppointmentsService/GetAppointmentsForWorkOrderReference(): Received response from upstream PropertyServiceClient (Order Id: {checkAvailabilityRequest.theOrder.orderId})");
            return response;
        }

        public Task<openSessionResponse> OpenSessionAsync(xmbOpenSession openSession)
        {
            _logger.LogInformation($"HackneyAppointmentsService/OpenSessionAsync(): Sent request to upstream AppointmentServiceClient (Id: {openSession.id})");
            var response = _client.openSessionAsync(openSession);
            _logger.LogInformation($"HackneyAppointmentsService/OpenSessionAsync(): Received response from upstream PropertyServiceClient (Id: {openSession.id})");
            return response;
        }

        public Task<closeSessionResponse> CloseSessionAsync(xmbCloseSession closeSession)
        {
            _logger.LogInformation($"HackneyAppointmentsService/CloseSessionAsync(): Sent request to upstream AppointmentServiceClient (Id: {closeSession.id})");
            var response = _client.closeSessionAsync(closeSession);
            _logger.LogInformation($"HackneyAppointmentsService/CloseSessionAsync(): Received response from upstream PropertyServiceClient (Id: {closeSession.id})");
            return response;
        }

        public Task<createOrderResponse> CreateWorkOrderAsync(xmbCreateOrder createOrder)
        {
            _logger.LogInformation($"HackneyAppointmentsService/CreateWorkOrderAsync(): Sent request to upstream AppointmentServiceClient (Order Id: {createOrder.theOrder.orderId})");
            var response = _client.createOrderAsync(createOrder);
            _logger.LogInformation($"HackneyAppointmentsService/CreateWorkOrderAsync(): Received response from upstream PropertyServiceClient (Order Id: {createOrder.theOrder.orderId})");
            return response;
        }

        public Task<scheduleBookingResponse> ScheduleBookingAsync(xmbScheduleBooking scheduleBooking)
        {
            _logger.LogInformation($"HackneyAppointmentsService/ScheduleBookingAsync(): Sent request to upstream AppointmentServiceClient (Order Id: {scheduleBooking.theBooking.orderId})");
            var response = _client.scheduleBookingAsync(scheduleBooking);
            _logger.LogInformation($"HackneyAppointmentsService/ScheduleBookingAsync(): Received response from upstream PropertyServiceClient (Order Id: {scheduleBooking.theBooking.orderId})");
            return response;
        }

        public Task<selectOrderResponse> SelectOrderAsync(xmbSelectOrder selectOrder)
        {
            _logger.LogInformation($"HackneyAppointmentsService/SelectOrderAsync(): Sent request to upstream AppointmentServiceClient (Order Id: {selectOrder.primaryOrderNumber})");
            var response = _client.selectOrderAsync(selectOrder);
            _logger.LogInformation($"HackneyAppointmentsService/SelectOrderAsync(): Received response from upstream PropertyServiceClient (Order Id: {selectOrder.primaryOrderNumber})");
            return response;
        }

        public Task<selectBookingResponse> SelectBookingAsync(xmbSelectBooking selectBooking)
        {
            _logger.LogInformation($"HackneyAppointmentsService/SelectBookingAsync(): Sent request to upstream AppointmentServiceClient (Id: {selectBooking.id})");
            var response = _client.selectBookingAsync(selectBooking);
            _logger.LogInformation($"HackneyAppointmentsService/SelectBookingAsync(): Received response from upstream PropertyServiceClient (Id: {selectBooking.id})");
            return response;
        }

		public async Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
        {            
            bool cacheNewRecord = false;
            _logger.LogInformation($@"HackneyAppointmentsService/GetCurrentAppointmentByWorkOrderReference(): 
                                    Check if there are appointments in the cache for Work Order ref: {workOrderReference}");
            //var cachedAppointments = _cacheRepository.GetCachedItemByKey<List<DetailedAppointment>>(cachekey);
            var cached_s_job_Appointments = _cacheRepository.GetCachedItemByKey<List<DetailedAppointment>>(string.Format(CacheKeyAppointments_s_jobs + workOrderReference));
            var cached_p_job_Appointments = _cacheRepository.GetCachedItemByKey<List<DetailedAppointment>>(string.Format(CacheKeyAppointments_p_jobs + workOrderReference));
            var cachedAppointments = (cached_p_job_Appointments ?? new List<DetailedAppointment>()).Union(cached_s_job_Appointments ?? new List<DetailedAppointment>());

           //run each time
            if (cachedAppointments.Count() != 0)
            {
                cachedAppointments = cachedAppointments.Select(x =>
                {
                    x.SourceSystem = "CACHE";
                    return x;
                }).ToList();
                return cachedAppointments;
            }
            else
            {
                cacheNewRecord = true;
            }

            _logger.LogInformation($@"HackneyAppointmentsService/GetAppointmentsByWorkOrderReference(): 
                    Sent request to get appointments for workOrderReference from DRS: {workOrderReference})");
            var drsResponse = await _drsRepository.GetAppointmentsByWorkOrderReference(workOrderReference);

			if (drsResponse.Any())
			{
                if (cacheNewRecord)
                {
                    var status = drsResponse.ToList()[0].Status;

                    //Set source to CACHE once
                    _cacheRepository.PutCachedItem(drsResponse.ToList(), string.Format(CacheKeyAppointments_s_jobs + workOrderReference), _cacheHelper.getTTLForStatus(status));
                }

                return drsResponse;
			}

			_logger.LogInformation($@"HackneyAppointmentsService/GetAppointmentsByWorkOrderReference(): 
                Sent request to get appointments for workOrderReference from UHT: {workOrderReference})");
            //TODO: Check item is CACHED, CACHE ITEM?
            var uhtResponse = await _uhtRepository.GetAppointmentsByWorkOrderReference(workOrderReference);
			return uhtResponse;
        }

		public async Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($@"HackneyAppointmentsService/GetCurrentAppointmentByWorkOrderReference(): 
                                    Check if there is an appointment in the cache for Work Order ref: {workOrderReference}");

            var cached_s_job_Appointments = _cacheRepository.GetCachedItemByKey<List<DetailedAppointment>>(string.Format(CacheKeyAppointments_s_jobs + workOrderReference));
            var cached_p_job_Appointments = _cacheRepository.GetCachedItemByKey<List<DetailedAppointment>>(string.Format(CacheKeyAppointments_p_jobs + workOrderReference));
            var cachedAppointments = (cached_p_job_Appointments ?? new List<DetailedAppointment>()).Union(cached_s_job_Appointments ?? new List<DetailedAppointment>());
            DetailedAppointment app = cachedAppointments.OrderByDescending(a => a.CreationDate).FirstOrDefault();
            bool cacheNewRecord;
            if (app != null)
            {
                app.SourceSystem = "CACHE";                   
                return app;
            }
            else
            {
                cacheNewRecord = true;
            }

            _logger.LogInformation($@"HackneyAppointmentsService/GetCurrentAppointmentByWorkOrderReference(): 
                                    Check if there is an appointment in DRS for Work Order ref: {workOrderReference}");
			var drsAppointment = await _drsRepository.GetLatestAppointmentByWorkOrderReference(workOrderReference);
			if (drsAppointment != null)
			{
                List<DetailedAppointment> da = new List<DetailedAppointment>
                {
                    drsAppointment
                };
                if (cacheNewRecord)
                {
                    var status = da[0].Status;
                    _cacheRepository.PutCachedItem(da, CacheKeyAppointments_s_jobs + workOrderReference, _cacheHelper.getTTLForStatus(status));
                }

				return drsAppointment;
			}

            _logger.LogInformation($@"HackneyAppointmentsService/GetCurrentAppointmentByWorkOrderReference(): 
                                    Check if there is an appointment in UHT for Work Order ref: {workOrderReference}");

            //TODO: Check if item is CACHED? 
			var uhAppointment = await _uhtRepository.GetLatestAppointmentByWorkOrderReference(workOrderReference);
            if (cacheNewRecord && uhAppointment != null)
            {
                List<DetailedAppointment> da = new List<DetailedAppointment>
                {
                    drsAppointment
                };
                var status = da[0].Status;
                _cacheRepository.PutCachedItem(da, CacheKeyAppointments_s_jobs + workOrderReference, _cacheHelper.getTTLForStatus(status));
            }

            return uhAppointment;
        }
    }
}
