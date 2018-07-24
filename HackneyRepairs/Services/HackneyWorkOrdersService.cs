using System;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Services
{
	public class HackneyWorkOrdersService : IHackneyWorkOrdersService
	{
        private IUhtRepository _uhtRepository;
		private ILoggerAdapter<WorkOrdersActions> _logger;

		public HackneyWorkOrdersService(IUhtRepository uhtRepository, ILoggerAdapter<WorkOrdersActions> logger)
        {
			_uhtRepository = uhtRepository;
			_logger = logger;
        }
      
		public Task<WorkOrderEntity> GetWorkOrderByReference(string workOrderReference)
		{
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByReference(): Sent request to UhtRepository (WorkOrder reference: {workOrderReference})");
			var response = _uhtRepository.GetWorkOrder(workOrderReference);
			_logger.LogInformation($"HackneyWorkOrdersService/GetWorkOrderByReference(): Work order details returned for: {workOrderReference})");
            return response;
		}
	}
}
