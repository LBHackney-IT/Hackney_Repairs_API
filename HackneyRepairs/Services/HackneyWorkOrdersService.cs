﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
    public class HackneyWorkOrdersService : IHackneyWorkOrdersService
    {
        private IUhtRepository _uhtRepository;
		private IUhwRepository _uhwRepository;
		private IUHWWarehouseRepository _uhWarehouseRepository;
        private ILoggerAdapter<WorkOrdersActions> _logger;

		public HackneyWorkOrdersService(IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uhWarehouseRepository, ILoggerAdapter<WorkOrdersActions> logger)
        {
            _uhtRepository = uhtRepository;
			_uhwRepository = uhwRepository;
			_uhWarehouseRepository = uhWarehouseRepository;
            _logger = logger;
        }

		public async Task<UHWorkOrder> GetWorkOrder(string workOrderReference)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrder(): Sent request to UhWarehouseRepository (WorkOrder reference: {workOrderReference})");
            var warehouseData = await _uhWarehouseRepository.GetWorkOrderByWorkOrderReference(workOrderReference);
            if (warehouseData != null)
            {
                return warehouseData;
            }

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrder(): No workOrders found in the warehouse. Request sent to UhtRepository (WorkOrder referenc)");
            var uhtData = await _uhtRepository.GetWorkOrder(workOrderReference);
            return uhtData;
        }
        
		public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReferences(IEnumerable<string> propertyReferences)
        {
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Sent request to _UhtRepository to get data from live for {propertyReferences.Count().ToString()} property Ids)");
            var liveData = await _uhtRepository.GetWorkOrderByPropertyReferences(propertyReferences);
			var lLiveData = (List<UHWorkOrder>)liveData;
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): {lLiveData.Count} work orders returned {propertyReferences.Count().ToString()} property Ids)");

			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Sent request to _UHWarehouseRepository to get data from warehouse {propertyReferences.Count().ToString()} property Ids)");
            var warehouseData =  await _uhWarehouseRepository.GetWorkOrderByPropertyReferences(propertyReferences);
			var lWarehouseData = (List<UHWorkOrder>)warehouseData;
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): {lWarehouseData.Count} work orders returned {propertyReferences.Count().ToString()} property Ids)");

			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Merging list from repositories to a single list");
			List<UHWorkOrder> result = lLiveData;
			result.InsertRange(0,lWarehouseData);
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Total {result.Count} work orders returned {propertyReferences.Count().ToString()} property Ids)");
			return result;
        }

        public async Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Sent request to UhtRepository to get data from live (WorkOrder reference: {workOrderReference})");
            var liveData = (List<Note>)await _uhwRepository.GetNotesByWorkOrderReference(workOrderReference);
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): {liveData.Count} notes returned for: {workOrderReference})");

            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Sent request to UHWarehouseRepository to get data from warehouse (Workorder referece: {workOrderReference})");
            var warehouseData = (List<Note>)await _uhWarehouseRepository.GetNotesByWorkOrderReference(workOrderReference);
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): {warehouseData.Count} notes returned for: {workOrderReference})");
                        
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Merging list from repositories to a single list");
            var result = liveData;
            result.InsertRange(0, warehouseData);
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Total {result.Count} notes returned for: {workOrderReference})");
            return result;
        }

        public async Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int size)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying Uh Warehouse for checking if {noteTarget} exists");
            if (!(await EnsureNoteTargetExistsInDB(noteTarget)))
            {
                _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): noteTarget {noteTarget} not found");
                return new List<Note>
                {
                    new Note()
                };
            }

            var feedMaxBatchSize = Int32.Parse(Environment.GetEnvironmentVariable("NoteFeedMaxBatchSize"));
            if (size > feedMaxBatchSize || size < 1)
            {
                size = feedMaxBatchSize;
            }

            _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying Uh Warehouse for: {startId}");
            var warehouseResult = await _uhWarehouseRepository.GetNoteFeed(startId, noteTarget, size);
            var warehouseResultCount = warehouseResult.Count();
            _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): {warehouseResultCount} results returned for: {startId}");

            if (warehouseResultCount == size)
            {
                _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Returning UH warehouse only results to actions for: {startId}");
                return warehouseResult;
            }

            IEnumerable<Note> uhResult;
            if (warehouseResultCount == 0)
            {
                _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying UH and expecting up to 50 results for: {startId}");
                uhResult = await _uhwRepository.GetNoteFeed(startId, noteTarget, size, null);
                _logger.LogInformation($"HackneyWorkOrdersService/GetRecentNotes(): {uhResult.Count()} results returned for: {startId}");
                return uhResult;
            }
            else
            {
                var remainingCount = size - warehouseResultCount;
                _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying UH and expecting up to {remainingCount} results for: {startId}");
                uhResult = await _uhwRepository.GetNoteFeed(startId, noteTarget, size, remainingCount);
                _logger.LogInformation($"HackneyWorkOrdersService/GetRecentNotes(): {uhResult.Count()} results returned for: {startId}");

                if (uhResult.Any())
                {
                    _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Joining warehouse and uh results into a single list for: {startId}");
                    List<Note> jointResult = (List<Note>)uhResult;
                    jointResult.InsertRange(0, warehouseResult);
                    _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Joint list contains {jointResult.Count()} notes for: {startId}");
                    return jointResult;
                }
                return warehouseResult;
            }
        }

        private async Task<bool> EnsureNoteTargetExistsInDB(string noteTarget)
        {
            var result = await _uhWarehouseRepository.GetDistinctNoteKeyObjects();
            if (result.Contains(noteTarget))
            {
                return true;
            }
            return false;
        }
    }
}
