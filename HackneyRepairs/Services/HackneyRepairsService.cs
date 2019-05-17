using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.DTOs;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using RepairsService;

namespace HackneyRepairs.Services
{
	public class HackneyRepairsService : IHackneyRepairsService
	{
		private RepairServiceClient _client;
		private IUhtRepository _uhtRepository;
		private IUhwRepository _uhwRepository;
        private IUhWebRepository _uhWebRepository;
        private IUHWWarehouseRepository _uhWarehouseRepository;
		private ILoggerAdapter<RepairsActions> _logger;

        public HackneyRepairsService(IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository, IUhWebRepository uhwebRepository, ILoggerAdapter<RepairsActions> logger)
		{
			_client = new RepairServiceClient();
			_uhtRepository = uhtRepository;
			_uhwRepository = uhwRepository;
            _uhWebRepository = uhwebRepository;
            _uhWarehouseRepository = uHWWarehouseRepository;
			_logger = logger;
		}

        public async Task<IEnumerable<RepairRequestBase>> GetRepairByPropertyReference(string propertyReference)
        {
            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): Sent request to UhtRepository (Property reference: {propertyReference})");
            var result = (List<RepairRequestBase>)await _uhtRepository.GetRepairRequestsByPropertyReference(propertyReference);
            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): {result.Count} repair requests returned");

            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): Sent request to UhWarehouse (Property reference: {propertyReference})");
            var uhwarehouseResults = (List<RepairRequestBase>)await _uhWarehouseRepository.GetRepairRequestsByPropertyReference(propertyReference);
            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): {uhwarehouseResults.Count} repair requests returned");

            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): Merging list from repositories to a single list");
            result.InsertRange(0, uhwarehouseResults);

            if (result.Count == 0)
            {
                _logger.LogInformation($"HackneyWorkOrdersService/GetRepairByPropertyReference(): Repositories returned empty lists, checking if the property exists.");
                var property = await _uhtRepository.GetPropertyDetailsByReference(propertyReference);
                if (property == null)
                {
                    return null;
                }
            }

            _logger.LogInformation($"HackneyRepairsService/GetRepairByPropertyReference(): Total {result.Count} work orders returned for: {propertyReference})");
            return result;
        }

        public async Task<IEnumerable<RepairWithWorkOrderDto>> GetRepairRequest(string repairReference)
        {
            _logger.LogInformation($"HackneyRepairsService/GetRepairByReference(): Sent request to UhWarehouseRepository (Repair ref: {repairReference})");
            var warehouseResult = await _uhWarehouseRepository.GetRepairRequest(repairReference);
            _logger.LogInformation($"HackneyRepairsService/GetRepairByReference(): {warehouseResult.Count()} results returned from UhWarehouseRepository (Repair ref: {repairReference})");

            _logger.LogInformation($"HackneyRepairsService/GetRepairByReference(): Sent request to UhtRepository (Repair ref: {repairReference})");
            var liveResult = await _uhtRepository.GetRepairRequest(repairReference);
            _logger.LogInformation($"HackneyRepairsService/GetRepairByReference(): {liveResult.Count()} results returned from UhtRepository (Repair ref: {repairReference})");

            var combinedResults = warehouseResult.Concat(liveResult);
            _logger.LogInformation($"HackneyRepairsService/GetRepairByReference(): returning {combinedResults.Count()} results in total (Repair ref: {repairReference})");
            return combinedResults;
        }

		public Task<RepairCreateResponse> CreateRepairAsync(NewRepairRequest request)
		{
			_logger.LogInformation($"HackneyRepairsService/CreateRepairAsync(): Sent request to upstream RepairServiceClient (Request ref: {request.RepairRequest.RequestReference})");
			var response = _client.CreateRepairRequestAsync(request);
			_logger.LogInformation($"HackneyRepairsService/CreateRepairAsync(): Received response from upstream RepairServiceClient (Request ref: {request.RepairRequest.RequestReference})");
			return response;
		}

		public Task<WorksOrderListResponse> CreateRepairWithOrderAsync(NewRepairTasksRequest repairRequest)
		{
			_logger.LogInformation($"HackneyRepairsService/CreateRepairWithOrderAsync(): Sent request to upstream RepairServiceClient (Request ref: {repairRequest.RepairRequest.RequestReference})");
			var response = _client.CreateRepairWithOrderAsync(repairRequest);
			_logger.LogInformation($"HackneyRepairsService/CreateRepairWithOrderAsync(): Received response from upstream RepairServiceClient (Request ref: {repairRequest.RepairRequest.RequestReference})");
			return response;
		}

        public async Task<DrsOrder> GetWorkOrderDetails(string workOrderReference)
		{
    		_logger.LogInformation($"HackneyRepairsService/GetWorkOrderDetails(): Sent request to uhWarehouseRepository (Work order ref ref: {workOrderReference})");
    		var warehouseResponse = await _uhWarehouseRepository.GetWorkOrderDetails(workOrderReference);
    		if (warehouseResponse != null)
    		{
    			return warehouseResponse;
    		}
			
            _logger.LogInformation($"HackneyRepairsService/GetWorkOrderDetails(): No response from uhWarehouseRepository, sent request to UHT (Work order ref: {workOrderReference})");
            var uhResponse = await _uhtRepository.GetWorkOrderDetails(workOrderReference);
            return uhResponse;
		}

		public Task<bool> UpdateRequestStatus(string repairRequestReference)
		{
			_logger.LogInformation($"HackneyRepairsService/UpdateRequestStatus(): Sent request to upstream RepairServiceClient (Repair request equest ref: {repairRequestReference})");
			var response = _uhtRepository.UpdateRequestStatus(repairRequestReference);
			_logger.LogInformation($"HackneyRepairsService/UpdateRequestStatus(): Received response from upstream RepairServiceClient (Repair request equest ref: {repairRequestReference})");
			return response;
		}

		public Task<TaskListResponse> GetRepairTasksAsync(RepairRefRequest request)
		{
			_logger.LogInformation($"HackneyRepairsService/GetRepairTasksAsync(): Sent request to upstream RepairServiceClient (Request ref: {request.RequestReference})");
			var response = _client.GetRepairTasksByRequestAsync(request);
			_logger.LogInformation($"HackneyRepairsService/GetRepairTasksAsync(): Received response from upstream RepairServiceClient (Request ref: {request.RequestReference})");
			return response;
		}	

		public Task<int?> UpdateUHTVisitAndBlockTrigger(string workOrderReference, DateTime startDate, DateTime endDate, int orderId, int bookingId, string slotDetail)
		{
			_logger.LogInformation($"HackneyRepairsService/UpdateUHTVisitAndBlockTrigger(): Sent request to upstream UHTdb (Work order ref : {workOrderReference})");
			var response = _uhtRepository.UpdateVisitAndBlockTrigger(workOrderReference, startDate, endDate, orderId, bookingId, slotDetail);
			_logger.LogInformation($"HackneyRepairsService/UpdateUHTVisitAndBlockTrigger(): Received response from upstream  UHTdb (Work order ref: {workOrderReference})");
			return response;
		}

		public Task<WebResponse> IssueOrderAsync(WorksOrderRequest request)
		{
			_logger.LogInformation($"HackneyRepairsService/IssueOrderAsync(): Sent request to upstream RepairServiceClient (Order ref: {request.OrderReference})");
			var response = _client.IssueOrderAsync(request);
			_logger.LogInformation($"HackneyRepairsService/IssueOrderAsync(): Received response from upstream RepairServiceClient (Order ref: {request.OrderReference})");
			return response;
		}

		public Task AddOrderDocumentAsync(string documentType, string workOrderReference, int workOrderId, string processComment)
		{
			_logger.LogInformation($"HackneyRepairsService/AddOrderDocumentAsync(): Sent request to upstream  UHTdb (Work order ref: {workOrderReference})");
			var response = _uhwRepository.AddOrderDocumentAsync(documentType, workOrderReference, workOrderId, processComment);
			_logger.LogInformation($"HackneyRepairsService/AddOrderDocumentAsync(): Received response from upstream  UHTdb (Work order ref: {workOrderReference})");
			return response;
		}

        public string GenerateUHSession(string uHUsername)
        {
            _logger.LogInformation($"HackneyUHWebSessionService/GenerateUHSession(): Sent request to upstream UH Web warehouse (UHUsername: {uHUsername})");
            var response = _uhWebRepository.GenerateUHSession(uHUsername);
            _logger.LogInformation($"HackneyUHWebSessionService/GenerateUHSession(): Received response from upstream UH Web warehouse (UHUsername: {uHUsername})");
            return response;
        }
    }
}
