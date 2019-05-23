using HackneyRepairs.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using RepairsService;
using System.Collections.Generic;
using HackneyRepairs.DTOs;
using HackneyRepairs.Formatters;
using System.Runtime.Serialization;

namespace HackneyRepairs.Actions
{
    public class RepairsActions
    {
        public IHackneyRepairsService _repairsService;
        public IHackneyRepairsServiceRequestBuilder _requestBuilder;
        public ILoggerAdapter<RepairsActions> _logger;
        public RepairsActions(IHackneyRepairsService repairsService, IHackneyRepairsServiceRequestBuilder requestBuilder, ILoggerAdapter<RepairsActions> logger)
        {
            _repairsService = repairsService;
            _requestBuilder = requestBuilder;
            _logger = logger;
        }

        public async Task<IEnumerable<RepairRequestBase>> GetRepairByPropertyReference(string propertyReference)
        {
            _logger.LogInformation($"Finding repair requests for Id: {propertyReference}");
            var repairRequests = await _repairsService.GetRepairByPropertyReference(propertyReference);
            if (repairRequests == null)
            {
                _logger.LogError($"Property not found for Id: {propertyReference}");
                throw new MissingPropertyException();
            }

            if (((List<RepairRequestBase>)repairRequests).Count == 0)
            {
                _logger.LogError($"Repairs not found for Id: {propertyReference}");
                return repairRequests;
            }

            _logger.LogInformation($"Repair request details returned for: {propertyReference}");
            return repairRequests;
        }

        public async Task<object> CreateRepair(RepairRequest request)
        {
            if (request.WorkOrders != null)
            {
                return await CreateRepairWithOrder(request);
            }
            else
            {
                return await CreateRepairWithoutOrder(request);
            }
        }

        public async Task<RepairRequest> GetRepair(string repairReference)
        {
            _logger.LogInformation($"Finding repair for Id : {repairReference}");
            var repairResponse = await _repairsService.GetRepairRequest(repairReference);
            if (!repairResponse.Any())
            {
                _logger.LogInformation($"Repair not found for Id: {repairReference}");
                throw new MissingRepairRequestException();
            }

            var repair = BuildRepair(repairResponse);
            return repair;
        }

        private async Task<object> CreateRepairWithOrder(RepairRequest request)
        {
            _logger.LogInformation($"Creating repair with order (prop ref: {request.PropertyReference})");
            string sessionToken = string.Empty;
            string uHUsername = string.Empty;
            if (!string.IsNullOrEmpty(request.LBHEmail))
            {
                uHUsername = _repairsService.GetUHUsername(request.LBHEmail);
                if (string.IsNullOrEmpty(uHUsername))
                {
                    throw new MissingUHUsernameException();
                }

                sessionToken = _repairsService.GenerateUHSession(uHUsername);
                if (string.IsNullOrEmpty(sessionToken))
                {
                    throw new MissingUHWebSessionTokenException();
                }
            }

            var repairRequest = string.IsNullOrEmpty(sessionToken) ? _requestBuilder.BuildNewRepairTasksRequest(request) : _requestBuilder.BuildNewRepairTasksRequestAsUser(request, sessionToken);

            var response = await _repairsService.CreateRepairWithOrderAsync(repairRequest);

            if (!response.Success)
            {
                throw new RepairsServiceException();
            }

            var workOrderList = response.WorksOrderList;
            if (workOrderList == null)
            {
                throw new MissingRepairRequestException();
            }

            var workOrderItem = workOrderList.FirstOrDefault();
            // update the request status to 000
            _repairsService.UpdateRequestStatus(workOrderItem.RepairRequestReference.Trim());

            var repairTasksResponse = await GetRepairTasksList(workOrderItem.RepairRequestReference);
            var tasksList = repairTasksResponse.TaskList;
            return new
            {
                repairRequestReference = workOrderItem.RepairRequestReference.Trim(),
                propertyReference = workOrderItem.PropertyReference.Trim(),
                problemDescription = request.ProblemDescription.Trim(),
                priority = request.Priority.Trim(),
                contact = new { name = request.Contact.Name, telephoneNumber = request.Contact.TelephoneNumber },
                workOrders = tasksList.Select(s => new
                {
                    workOrderReference = s.WorksOrderReference.Trim(),
                    sorCode = s.JobCode.Trim(),
                    supplierReference = s.SupplierReference.Trim()
                }).ToArray()
            };
        }

        private async Task<object> CreateRepairWithoutOrder(RepairRequest request)
        {
            _logger.LogInformation($"Creating repair with no work order");
            var repairRequest = _requestBuilder.BuildNewRepairRequest(request);

            var response = await _repairsService.CreateRepairAsync(repairRequest);

            if (!response.Success)
            {
                throw new RepairsServiceException();
            }

            var repairResponse = response.RepairRequest;
            if (repairResponse == null)
            {
                throw new MissingRepairRequestException();
            }

            // update the request status to 000
            _repairsService.UpdateRequestStatus(repairResponse.Reference.Trim());

            return new
            {
                repairRequestReference = repairResponse.Reference.Trim(),
                problemDescription = repairResponse.Problem.Trim(),
                priority = repairResponse.PriorityCode.Trim(),
                propertyReference = repairResponse.PropertyReference.Trim(),
                contact = new { name = repairResponse.Name, telephoneNumber = request.Contact.TelephoneNumber }
            };
        }

        private async Task<TaskListResponse> GetRepairTasksList(string requestReference)
        {
            _logger.LogInformation($"Getting repair task list ({requestReference})");
            var request = _requestBuilder.BuildRepairRequest(requestReference);
            var response = await _repairsService.GetRepairTasksAsync(request);
            if (!response.Success)
            {
                throw new RepairsServiceException();
            }

            return response;
        }

        private RepairRequest BuildRepair(IEnumerable<RepairWithWorkOrderDto> repositoryResult)
        {
            _logger.LogInformation($"Mapping results to the repair response object for Id: {repositoryResult.First().rq_ref}");
            var repair = new RepairRequest();

            var firstResult = repositoryResult.First();
            repair.RepairRequestReference = firstResult.rq_ref;
            repair.Priority = firstResult.rq_priority;
            repair.PropertyReference = firstResult.prop_ref;
            repair.ProblemDescription = firstResult.rq_problem;

            repair.Contact = new RepairRequestContact
            {
                Name = firstResult.rq_name,
                TelephoneNumber = firstResult.rq_phone
            };

            var workOrders = new List<WorkOrder>();
            foreach (var result in repositoryResult)
            {
                if (!string.IsNullOrWhiteSpace(result.wo_ref))
                {
                    var workOrder = new WorkOrder()
                    {
                        WorkOrderReference = result.wo_ref,
                        SupplierRef = result.sup_ref,
                        SorCode = result.job_code
                    };
                    GenericFormatter.TrimStringAttributes(workOrder);
                    workOrders.Add(workOrder);
                }
            }

            repair.WorkOrders = workOrders;
            GenericFormatter.TrimStringAttributes(repair);
            GenericFormatter.TrimStringAttributes(repair.Contact);

            return repair;
        }
    }
    
    public class MissingUHUsernameException : Exception
    {        
    }

    public class MissingUHWebSessionTokenException : Exception
    {
    }

	public class MissingRepairRequestException : Exception
	{
	}

	public class RepairsServiceException : Exception
	{
	}
}
