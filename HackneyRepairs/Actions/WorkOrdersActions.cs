using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Newtonsoft.Json;
using HackneyRepairs.Formatters;

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

        public async Task<dynamic> GetWorkOrder(string workOrderReference, bool mobileReports = false)
		{
			_logger.LogInformation($"Finding work order details for reference: {workOrderReference}");
            var result = await _workOrdersService.GetWorkOrder(workOrderReference);
			if (result == null)
			{
				_logger.LogError($"Work order not found for reference: {workOrderReference}");
				throw new MissingWorkOrderException();
			}
            if (mobileReports)
            {
                var resultWithMobileReports = MapToWorkOrderWithMobileReports(result);
                if (string.IsNullOrWhiteSpace(result.ServitorReference))
                {
                    _logger.LogError($"Work order {workOrderReference} does not have a servitor reference, mobile reports cannot be found");
                    resultWithMobileReports.MobileReports = new List<string>();
                    return resultWithMobileReports;
                }
                _logger.LogError($"Getting mobile reports matching servitor reference {result.ServitorReference} for work order {workOrderReference}");
                resultWithMobileReports.MobileReports = await _workOrdersService.GetMobileReports(result.ServitorReference);
                return resultWithMobileReports;
            }

			_logger.LogInformation($"Work order details returned for: {workOrderReference}");
            return result;
		}

        public async Task<IEnumerable<UHWorkOrderWithMobileReports>> GetWorkOrders(string[] workOrderReferences, bool withMobileReports = false)
        {
            _logger.LogInformation($"Finding work order details for references: {GenericFormatter.CommaSeparate(workOrderReferences)}");
            var workOrders = await _workOrdersService.GetWorkOrders(workOrderReferences);

            if (workOrderReferences.Length > workOrders.ToArray().Length)
            {
                _logger.LogError($"Work order not found for one of: {GenericFormatter.CommaSeparate(workOrderReferences)}");
                throw new MissingWorkOrderException();
            }

            IEnumerable<UHWorkOrderWithMobileReports> results = await Task.WhenAll(
                workOrders.Select(async workOrder =>
                {
                    var resultWithMobileReports = MapToWorkOrderWithMobileReports(workOrder);

                    if (withMobileReports)
                    {
                        if (string.IsNullOrWhiteSpace(workOrder.ServitorReference))
                        {
                            _logger.LogError($"Work order {workOrder.WorkOrderReference} does not have a servitor reference, mobile reports cannot be found");
                            resultWithMobileReports.MobileReports = new List<string>();
                        }
                        else
                        {
                            _logger.LogError($"Getting mobile reports matching servitor reference {workOrder.ServitorReference} for work order {workOrder.WorkOrderReference}");
                            resultWithMobileReports.MobileReports = await _workOrdersService.GetMobileReports(workOrder.ServitorReference);
                        }
                    }

                    return resultWithMobileReports;
                })
            );

            _logger.LogInformation($"Work order details returned for: {workOrderReferences}");

            return results;
        }

		public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference)
		{
			_logger.LogInformation($"Finding work order details for property reference: {propertyReference}");
			var result = await _workOrdersService.GetWorkOrderByPropertyReference(propertyReference);
            if (result == null)
			{
				_logger.LogError($"Property not found for property reference: {propertyReference}");
				throw new MissingPropertyException();
			}
			if ((result.ToList()).Count == 0)
			{
				_logger.LogError($"Work order not found for property reference: {propertyReference}");
				return result;
			}
			_logger.LogInformation($"Work order details returned for property reference: {propertyReference}");
			return result;
		}

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until)
        {
            _logger.LogInformation($"Finding work order details for property references: {GenericFormatter.CommaSeparate(propertyReferences)}");
            var result = await _workOrdersService.GetWorkOrdersByPropertyReferences(propertyReferences, since, until);

            if ((result.ToList()).Count == 0)
            {
                _logger.LogError($"Work orders not found for property references: {GenericFormatter.CommaSeparate(propertyReferences)}");
            }

            return result;
        }

		public async Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
		{
			_logger.LogInformation($"Finding notes by work order: {workOrderReference}");
			var result = await _workOrdersService.GetNotesByWorkOrderReference(workOrderReference);
            if (result == null)
			{
				_logger.LogError($"Work order reference not found Ref: {workOrderReference}");
				throw new MissingWorkOrderException();
			}
			if ((result.ToList()).Count == 0)
            {
				_logger.LogError($"Notes not found for: {workOrderReference}");
				return result;
            }
			_logger.LogInformation($"Notes returned for: {workOrderReference}");
            return result;
		}

        public async Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrdersFeed(string startID, int resultSize)
        {
            _logger.LogInformation($"Getting work order feed for {startID}");
            return await _workOrdersService.GetWorkOrderFeed(startID, resultSize);
        }

        private UHWorkOrderWithMobileReports MapToWorkOrderWithMobileReports(UHWorkOrder workOrder)
        {
            if (workOrder == null)
            {
                return default(UHWorkOrderWithMobileReports);
            }
            var jsonObject = JsonConvert.SerializeObject(workOrder);
            return JsonConvert.DeserializeObject<UHWorkOrderWithMobileReports>(jsonObject);
        }
    }

    public class MissingWorkOrderException : Exception { }
	public class MissingNotesException : Exception { }
	public class IncludeChildrenNotSupportedException : Exception { }
	public class TradeNotSpecifiedException : Exception { }
}
