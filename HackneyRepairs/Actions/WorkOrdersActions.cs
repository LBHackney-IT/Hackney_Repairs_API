using System;
using System.Threading.Tasks;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Repository;

namespace HackneyRepairs.Actions
{
    public class WorkOrdersActions
    {
	IHackneyWorkOrdersService _workOrdersService;
	private readonly ILoggerAdapter<WorkOrdersActions> _logger;

	public WorkOrdersActions(IHackneyWorkOrdersService workOrdersService, ILoggerAdapter<WorkOrdersActions> logger)
        {
	    _workOrdersService = workOrdersService;
	    _logger = logger;
        }

        public async Task<WorkOrderEntity> GetWorkOrderByReference(string workOrderReference)
        {
	    _logger.LogInformation($"Finding work order details for reference: {workOrderReference}");
	    var result = await _workOrdersService.GetWorkOrderByReference(workOrderReference);            
	    if (result == null)
	    {
	        _logger.LogError($"Work order not found for reference: {workOrderReference}");
	        throw new MissingWorkOrderException();
	    }
	    _logger.LogInformation($"Work order details returned for: {workOrderReference}");
	    return result;
        }
    }
    public class MissingWorkOrderException : System.Exception { }
}
