using HackneyRepairs.Interfaces;
using RepairsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using HackneyRepairs.Entities;
using HackneyRepairs.Actions;
using HackneyRepairs.DTOs;

namespace HackneyRepairs.Services
{
    public class FakeRepairsService : IHackneyRepairsService
    {
        public Task<RepairCreateResponse> CreateRepairAsync(NewRepairRequest request)
        {
            var response = new RepairCreateResponse
            {
                Success = true,
                RepairRequest = new RepairRequestDto
                {
                    Reference = "123  ",
                    Problem = "tap leaking  ",
                    PriorityCode = "N",
                    PropertyReference = "00000320",
                    LocationCode = "1  ",
                    Name = "Al Smith"
                }
            };
            switch (request.RepairRequest.PropertyRef)
            {
                case "01234568":
                    return Task.Run(() => new RepairCreateResponse
                    {
                        Success = false,
                        RepairRequest = new RepairRequestDto()
                    });
                case "00000320":
                    return Task.Run(() => response);
                default:
                    return Task.Run(() => new RepairCreateResponse
                    {
                        Success = true,
                        RepairRequest = new RepairRequestDto()
                    });
            }
        }

        public Task<WorksOrderListResponse> CreateRepairWithOrderAsync(NewRepairTasksRequest repairRequest)
        {
            var response = new WorksOrderListResponse
            { 
                Success = true,
                WorksOrderList = new List<WorksOrderDto>
                {
                    new WorksOrderDto
                    {
                        RepairRequestReference = "123456",
                        OrderReference = "987654  ",
                        PropertyReference = "00000320   ",
                        SupplierReference = "000000127 "
                    }
                }.ToArray()
            };
            switch (repairRequest.RepairRequest.PropertyRef)
            {
                case "123456890":
                    return Task.Run(() => new WorksOrderListResponse
                    {
                        Success = false,
                        WorksOrderList = new List<WorksOrderDto>().ToArray()
                    });
                case "00000320":
                    return Task.Run(() => response);
                default:
                    return Task.Run(() => new WorksOrderListResponse
                    {
                        Success = true,
                        WorksOrderList = new List<WorksOrderDto>().ToArray()
                    });
            }
        }

        public Task<DrsOrder> GetWorkOrderDetails(string workOrderReference)
        {
            if (workOrderReference == "01550853")
            {
                throw new Exception();
            }

            var drsOrder = new DrsOrder
            {
                contract = "H01",
                prop_ref = "12345",
                propname = "Address name",
                priority = "N",
                address1 = "Address name",
                postcode = "addresspostcode",
                createdDate = DateTime.Today,
                dueDate = DateTime.Today.AddDays(30),
                Tasks = new List<DrsTask> 
                {
                    new DrsTask
                    {
                        job_code = "00210356",
                        comments = "Some comments",
                        itemValue = Decimal.MinValue,
                        itemqty = Decimal.MinValue,
                        trade = "GL",
                        smv = 1
                    }
                },
                wo_ref = workOrderReference
            };
            return Task.Run(() => drsOrder);
        }

        public Task<bool> UpdateRequestStatus(string repairRequestReference)
        {
            return Task.Run(() => true);
        }

        public Task<TaskListResponse> GetRepairTasksAsync(RepairRefRequest request)
        {
            var tasksListResponse = new TaskListResponse
            {
                Success = true,
                TaskList = new List<RepairTaskDto>
                {
                    new RepairTaskDto
                    {
                        WorksOrderReference = "987654",
                        RepairRequestReference = "123456",
                        SupplierReference = "000000127",
                        JobCode = "20090190"
                    }
                }.ToArray()
            };
            return Task.Run(() => tasksListResponse);
        }

        public Task<int?> UpdateUHTVisitAndBlockTrigger(string workOrderReference, DateTime startDate, DateTime endDate, int orderId, int bookingId, string slotDetail)
        {
            int? returnValue = 0;
            return Task.Run(() => returnValue);
        }

        public Task<WebResponse> IssueOrderAsync(WorksOrderRequest request)
        {
            var response = new WebResponse
            {
                Success = true
            };
            return Task.Run(() => response);
        }

        public Task AddOrderDocumentAsync(string documentType, string workOrderReference, int workOrderId, string processComment)
        {
            return Task.FromResult(false);
        }

		public Task<IEnumerable<RepairRequestBase>> GetRepairByPropertyReference(string propertyReference)
        {
			IEnumerable<RepairRequestBase> requests = new List<RepairRequestBase>()
			{
				new RepairRequestBase
				{
					RepairRequestReference = "43453543  ",
					ProblemDescription = "tap leaking ",
					Priority = "N",
					PropertyReference = "123456890",
				},
				new RepairRequestBase
                {
                    RepairRequestReference = "43453542  ",
                    ProblemDescription = "tap still leaking ",
                    Priority = "N",
                    PropertyReference = "123456890",
                }
			};

			switch (propertyReference)
            {
                case "999999999":
					return Task.Run(() => new List<RepairRequestBase>().AsEnumerable());
				case "0":
					return Task.Run(() => (IEnumerable<RepairRequestBase>)null);
                default:
					return Task.Run(() => requests);
            }
        }

        public Task<IEnumerable<RepairWithWorkOrderDto>> GetRepairRequest(string repairReference)
        {
            if (string.Equals(repairReference, "ABCXYZ"))
            {
                throw new RepairsServiceException();
            }

            var fakeResponse = new List<RepairWithWorkOrderDto>();
            if (string.Equals(repairReference, "123456899"))
            {
                return Task.Run(() => (IEnumerable<RepairWithWorkOrderDto>)fakeResponse);
            }

            var fakeElement = new RepairWithWorkOrderDto();
            return Task.Run(() => fakeResponse.Append(fakeElement));
        }

        public string GenerateUHSession(string uHUsername)
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUHUsername(string lBHEmail)
        {
            return "HackneyAPI";
        }

        public Task<WebResponse> CancelOrderAsync(WorksOrderRequest worksOrderRequest)
        {
            var response = new WebResponse
            {
                Success = true
            };
            return Task.Run(() => response);
        }
    }
}
