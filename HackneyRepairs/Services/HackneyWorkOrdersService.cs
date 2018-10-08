using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;

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

        public async Task<IEnumerable<string>> GetMobileReports(string servitorReference)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetMobileReports(): Sent request to MobileReportsRepository (Servitor reference: {servitorReference})");
            return MobileReportsRepository.GetReports(servitorReference);
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Sent request to _UhtRepository to get data from live for {propertyReference}");
            var liveData = await _uhtRepository.GetWorkOrderByPropertyReference(propertyReference);
            var result = liveData.ToList();
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): {result.Count} work orders returned for {propertyReference}");


            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Sent request to _UHWarehouseRepository to get data from warehouse for {propertyReference}");
            var warehouseData = await _uhWarehouseRepository.GetWorkOrderByPropertyReference(propertyReference);
            var lWarehouseData = warehouseData.ToList();
            result.InsertRange(0, lWarehouseData);
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): {lWarehouseData.Count} work orders returned for {propertyReference}");

            if (result.Count() == 0)
            {
                _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Repositories returned empty lists, checking if the property exists.");
                var property = await _uhWarehouseRepository.GetPropertyDetailsByReference(propertyReference);
                if (property == null)
                {
                    return null;
                }
            }

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByPropertyReference(): Total {result.Count} work orders returned for {propertyReference}");
            return result;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime? since, DateTime? until)
        {
            if (since == null)
            {
                since = DateTime.Now.AddYears(-2);
            }
            if (until == null)
            {
                until = DateTime.Now;
            }

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrdersByPropertyReferences(): Sent request to _UhtRepository to get data from live for {propertyReferences}");
            var liveData = await _uhtRepository.GetWorkOrdersByPropertyReferences(propertyReferences, since.Value, until.Value);
            var result = liveData.ToList();
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrdersByPropertyReferences(): {result.Count} work orders returned for {propertyReferences}");

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrdersByPropertyReferences(): Sent request to _UHWarehouseRepository to get data from warehouse for {propertyReferences}");
            var warehouseData = await _uhWarehouseRepository.GetWorkOrdersByPropertyReferences(propertyReferences, since.Value, until.Value);
            var lWarehouseData = warehouseData.ToList();

            result.InsertRange(0, lWarehouseData);
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrdersByPropertyReferences(): {lWarehouseData.Count} work orders returned for {propertyReferences}");

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrdersByPropertyReferences(): Total {result.Count} work orders returned for {propertyReferences}");
            return result;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string blockReference, string trade)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): Sent request to _UhtRepository to get data from live for block reference: {blockReference}");
            var liveData = await _uhtRepository.GetWorkOrderByBlockReference(blockReference, trade);
            var result = (List<UHWorkOrder>)liveData;
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): {result.Count} work orders returned for block reference: {blockReference}");

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): Sent request to _UHWarehouseRepository to get data from warehouse for block reference: {blockReference}");
            var warehouseData = await _uhWarehouseRepository.GetWorkOrderByBlockReference(blockReference, trade);
            var lWarehouseData = (List<UHWorkOrder>)warehouseData;
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): {lWarehouseData.Count} work orders returned for block reference: {blockReference}");
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): Merging list from repositories to a single list");
            result.InsertRange(0, lWarehouseData);

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByBlockReferences(): Total {result.Count} work orders returned for block reference: {blockReference}");
            return result;
        }

        public async Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Sent request to UhtRepository to get data from live (WorkOrder reference: {workOrderReference})");
            var result = (List<Note>)await _uhwRepository.GetNotesByWorkOrderReference(workOrderReference);
            _logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): {result.Count} notes returned for: {workOrderReference})");
            return result;
            // Portion of code commented temporarily until UHWarehouse's note table gets and index
            // Repository method's query has been edited for removing the cutoff date condition
            //_logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Sent request to UHWarehouseRepository to get data from warehouse (Workorder referece: {workOrderReference})");
            //var warehouseData = (List<Note>)await _uhWarehouseRepository.GetNotesByWorkOrderReference(workOrderReference);
            //_logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): {warehouseData.Count} notes returned for: {workOrderReference})");
            //_logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Merging list from repositories to a single list");
            //result.InsertRange(0, warehouseData);

            //_logger.LogInformation($"HackneyWorkOrdersService/GetNotesByWorkOrderReference(): Total {result.Count} notes returned for: {workOrderReference})");
            //return result;
        }

		public async Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int batchSize)
        {
            int feedMaxBatchSize = Int32.Parse(Environment.GetEnvironmentVariable("NoteFeedMaxBatchSize"));
            if (batchSize > feedMaxBatchSize || batchSize < 1)
            {
                batchSize = feedMaxBatchSize;
            }
			var result = new List<Note>();
			
			_logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying Uh Warehouse for: {startId}");
			result = (List<Note>) await _uhWarehouseRepository.GetNoteFeed(startId, noteTarget, batchSize);
			_logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): {result.Count()} notes received for: {startId}");
			if (result.Count() == batchSize)
			{
				_logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Returning UH warehouse only results to for: {startId}");
				return result;
			}
			
			_logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Querying UH and expecting up to {batchSize - result.Count() } results for: {startId}");
			var uhwResult = await _uhwRepository.GetNoteFeed(startId, noteTarget, batchSize - result.Count());
            _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): {uhwResult.Count()} results received for: {startId}");
            _logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Joining warehouse and uh results into a single list for: {startId}");
			result.InsertRange(result.Count(), uhwResult);

			_logger.LogInformation($"HackneyWorkOrdersService/GetNoteFeed(): Joint list contains {result.Count()} notes for: {startId}");
			return result;
        }


        public async Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int batchSize)
        {
            int feedMaxBatchSize = Int32.Parse(Environment.GetEnvironmentVariable("NoteFeedMaxBatchSize"));
            if (batchSize > feedMaxBatchSize || batchSize < 1)
            {
                batchSize = feedMaxBatchSize;
            }

			List<UHWorkOrderFeed> result = new List<UHWorkOrderFeed>();
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): Querying Uh Warehouse for {startId} and for up to {batchSize} workOrders");
			result = (List<UHWorkOrderFeed>) await _uhWarehouseRepository.GetWorkOrderFeed(startId, batchSize);
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): {result.Count()} workOrders received for: {startId}");

			if (result.Count() == batchSize)
			{
				_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): Returning UH Warehouse only results for: {startId}");
				return result;
			}

            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): Querying UHT repository for up to {batchSize} workOrders for: {startId}");
			var uhtResult = await _uhtRepository.GetWorkOrderFeed(startId, batchSize - result.Count());
            _logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): Joining warehouse and UHT repository results into a single list for: {startId}");
			result.InsertRange(result.Count(), uhtResult);
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderFeed(): Joint list contains {result.Count()} work orders for: {startId}");
			return result;
        }
    }
}
