using System.Threading.Tasks;
using HackneyRepairs.Actions;
using Moq;
using Xunit;
using System.Text;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using RepairsService;
using Newtonsoft.Json;
using System.Collections.Generic;
using HackneyRepairs.Logging;
using HackneyRepairs.DTOs;
using System;

namespace HackneyRepairs.Tests.Actions
{
    public class RepairsActionsTest
    {
        [Fact]
        public async Task create_repair_returns_a_created_repair_response_object()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "123456",
                Contact = new RepairRequestContact { Name = "Test", TelephoneNumber = "0123456789" }
            };

            var repairRequest = new NewRepairRequest
            {
                RepairRequest = new RepairRequestInfo
                {
                    PropertyRef = request.PropertyReference,
                    Priority = request.Priority,
                    Problem = request.ProblemDescription
                }
            };
            var fakeRepairService = new Mock<IHackneyRepairsService>();
            var response = new RepairCreateResponse
            {
                Success = true,
                RepairRequest = new RepairRequestDto
                {
                    Reference = "123",
                    Problem = "tap leaking",
                    PropertyReference = "123456",
                    PriorityCode = "N",
                    LocationCode = "1",
                    Name = "Test"
                }
            };
            fakeRepairService.Setup(service => service.CreateRepairAsync(repairRequest))
                .ReturnsAsync(response);
            fakeRepairService.Setup(service => service.UpdateRequestStatus("123")).ReturnsAsync(true);
            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            fakeRequestBuilder.Setup(builder => builder.BuildNewRepairRequest(request)).Returns(repairRequest);
      
            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);
            var result = await repairsActions.CreateRepair(request);
            var response1 = new 
            {
                repairRequestReference = "123",
                problemDescription = "tap leaking",
                priority = "N",
                propertyReference = "123456",
                contact = new { name = "Test", telephoneNumber = "0123456789" }  
            };
            Assert.Equal(response1, result);
        }

        [Fact]
        public async Task create_repair_raises_an_exception_if_the_service_responds_with_an_error()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "123456890"
            };
            var repairRequest = new NewRepairRequest();
            var response = new RepairCreateResponse { Success = false, RepairRequest = null };
            var fakeService = new Mock<IHackneyRepairsService>();
            fakeService.Setup(service => service.CreateRepairAsync(repairRequest))
                .ReturnsAsync(response);
            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            fakeRequestBuilder.Setup(builder => builder.BuildNewRepairRequest(request)).Returns(repairRequest);

            var repairActions = new RepairsActions(fakeService.Object, fakeRequestBuilder.Object, mockLogger.Object);
            await Assert.ThrowsAsync<RepairsServiceException>(async () => await repairActions.CreateRepair(request));
        }

        [Fact]
        public async Task create_repair_raises_an_exception_if_the_repair_request_is_missing()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest();
            var repairRequest = new NewRepairRequest();
            var response = new RepairCreateResponse { Success = true, RepairRequest = null };
            var fakeService = new Mock<IHackneyRepairsService>();
            fakeService.Setup(service => service.CreateRepairAsync(It.IsAny<NewRepairRequest>()))
                .ReturnsAsync(response);
            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            fakeRequestBuilder.Setup(builder => builder.BuildNewRepairRequest(request)).Returns(repairRequest);
            var repairsActions = new RepairsActions(fakeService.Object, fakeRequestBuilder.Object, mockLogger.Object);
            await Assert.ThrowsAsync<MissingRepairRequestException>(async () => await repairsActions.CreateRepair(request));
        }

        [Fact]
        public void creating_a_repair_request_object_is_successful_if_the_request_priority_code_is_valid()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest()
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "123456890"
            };
            Assert.Equal("N", request.Priority);
        }

        [Fact]
        public async Task create_repair_with_work_order_including_username_returns_a_created_repair_response_object_with_sessionToken()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "00000320",
                UHUsername = "bob",
                Contact = new RepairRequestContact
                {
                    Name = "Test",
                    TelephoneNumber = "0123456789"
                },
                WorkOrders = new List<WorkOrder>
                {
                    new WorkOrder
                    {
                        SorCode = "20090190"
                    }
                }
            };

            var repairRequest = new NewRepairTasksRequest
            {
                RepairRequest = new RepairRequestInfo
                {
                    PropertyRef = request.PropertyReference,
                    Priority = request.Priority,
                    Problem = request.ProblemDescription
                },
                TaskList = new List<RepairTaskInfo>
                {
                    new RepairTaskInfo
                    {
                        JobCode = "12345678",
                        PropertyReference = "00000320",
                    }
                }.ToArray(),
                SessionToken = "ba1b3444-491b-43ff-9af9-cdf64623f624",
                CompanyCode = "001"
            };
            var fakeRepairService = new Mock<IHackneyRepairsService>();
            var response = new WorksOrderListResponse
            {
                Success = true,
                WorksOrderList = new List<WorksOrderDto>
                {
                    new WorksOrderDto
                    {
                        RepairRequestReference = "123456",
                        OrderReference = "987654",
                        PropertyReference = "00000320",
                        SupplierReference = "00000127"
                    }
                }.ToArray()
            };
            var tasksListResponse = new TaskListResponse
            {
                Success = true,
                TaskList = new List<RepairTaskDto>
                {
                    new RepairTaskDto
                    {
                        WorksOrderReference = "987654",
                        SupplierReference = "00000127",
                        JobCode = "12345678"
                    }
                }.ToArray()
            };
            fakeRepairService.Setup(service => service.CreateRepairWithOrderAsync(repairRequest))
                .ReturnsAsync(response);
            fakeRepairService.Setup(service => service.UpdateRequestStatus("123456")).ReturnsAsync(true);

            fakeRepairService.Setup(service => service.GenerateUHSession("bob")).Returns("ba1b3444-491b-43ff-9af9-cdf64623f624");

            fakeRepairService.Setup(service => service.GetRepairTasksAsync(It.IsAny<RepairRefRequest>()))
                .ReturnsAsync(tasksListResponse);

            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            
            fakeRequestBuilder.Setup(builder => builder.BuildNewRepairTasksRequestAsUser(request, "ba1b3444-491b-43ff-9af9-cdf64623f624")).Returns(repairRequest);
            fakeRequestBuilder.Setup(builder => builder.BuildRepairRequest("123456")).Returns(new RepairRefRequest());

            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);
            var result = await repairsActions.CreateRepair(request);
            var workOrders = new object[1];
            workOrders[0] = new { workOrderReference = "987654", sorCode = "12345678", supplierReference = "00000127" };
            var response1 = new
            {
                repairRequestReference = "123456",
                propertyReference = "00000320",
                problemDescription = "tap leaking",
                priority = "N",
                contact = new { name = "Test", telephoneNumber = "0123456789" },
                workOrders = workOrders
            };

            Assert.Equal(JsonConvert.SerializeObject(response1), JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task create_repair_with_work_order_returns_a_created_repair_response_object_with_orders_included()
        {
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();
            var request = new RepairRequest
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "00000320",
                Contact = new RepairRequestContact
                {
                    Name = "Test",
                    TelephoneNumber = "0123456789"
                },
                WorkOrders = new List<WorkOrder>
                {
                    new WorkOrder
                    {
                        SorCode = "20090190"
                    }
                }
            };

            var repairRequest = new NewRepairTasksRequest
            {
                RepairRequest = new RepairRequestInfo
                {
                    PropertyRef = request.PropertyReference,
                    Priority = request.Priority,
                    Problem = request.ProblemDescription
                },
                TaskList = new List<RepairTaskInfo>
                {
                    new RepairTaskInfo
                    {
                        JobCode = "12345678",
                        PropertyReference = "00000320",
                    }
                }.ToArray()
            };
            var fakeRepairService = new Mock<IHackneyRepairsService>();
            var response = new WorksOrderListResponse
            {
                Success = true,
                WorksOrderList = new List<WorksOrderDto>
                {
                    new WorksOrderDto
                    {
                        RepairRequestReference = "123456",
                        OrderReference = "987654",
                        PropertyReference = "00000320",
                        SupplierReference = "00000127"
                    }
                }.ToArray()
            };
            var tasksListResponse = new TaskListResponse
            {
                Success = true,
                TaskList = new List<RepairTaskDto>
                {
                    new RepairTaskDto
                    {
                        WorksOrderReference = "987654",
                        SupplierReference = "00000127",
                        JobCode = "12345678"
                    }
                }.ToArray()
            };
            fakeRepairService.Setup(service => service.CreateRepairWithOrderAsync(repairRequest))
                .ReturnsAsync(response);
            fakeRepairService.Setup(service => service.UpdateRequestStatus("123456")).ReturnsAsync(true);

            fakeRepairService.Setup(service => service.GetRepairTasksAsync(It.IsAny<RepairRefRequest>()))
                .ReturnsAsync(tasksListResponse);

            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            fakeRequestBuilder.Setup(builder => builder.BuildNewRepairTasksRequest(request)).Returns(repairRequest);
            fakeRequestBuilder.Setup(builder => builder.BuildRepairRequest("123456")).Returns(new RepairRefRequest());
                
            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);
            var result = await repairsActions.CreateRepair(request);
            var workOrders = new object[1];
            workOrders[0] = new { workOrderReference = "987654", sorCode = "12345678", supplierReference = "00000127" };
            var response1 = new
            {
                repairRequestReference = "123456",
                propertyReference = "00000320",
                problemDescription = "tap leaking",
                priority = "N",
                contact = new { name ="Test", telephoneNumber= "0123456789" },
                workOrders = workOrders
            };

            Assert.Equal(JsonConvert.SerializeObject(response1), JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task get_repair_request_by_property_reference_returns_a_list_of_repairs_for_a_valid_request()
        {
            IEnumerable<RepairRequestBase> expected = new List<RepairRequestBase>()
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

            var fakeRepairService = new Mock<IHackneyRepairsService>();
            fakeRepairService.Setup(service => service.GetRepairByPropertyReference("12345678"))
                .ReturnsAsync(expected);
            
            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();

            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);

            var response = repairsActions.GetRepairByPropertyReference("12345678");
            Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(response.Result));
        }

        [Fact]
        public async Task get_repair_request_by_property_reference_throws_an_exception_when_property_not_found()
        {
            var fakeRepairService = new Mock<IHackneyRepairsService>();
            fakeRepairService.Setup(service => service.GetRepairByPropertyReference(It.IsAny<string>()))
			     .ReturnsAsync((List<RepairRequestBase>)null);

            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();

            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);

            await Assert.ThrowsAsync<HackneyRepairs.Actions.MissingPropertyException>(async () => await repairsActions.GetRepairByPropertyReference("12345678"));
        }

        [Fact]
        public async Task get_repair_request_by_repair_reference_throws_an_exception_when_property_not_found()
        {
            var fakeRepairService = new Mock<IHackneyRepairsService>();
            fakeRepairService.Setup(service => service.GetRepairRequest(It.IsAny<string>()))
                             .Returns(Task.Run(() => (IEnumerable<RepairWithWorkOrderDto>)new List<RepairWithWorkOrderDto>()));

            var fakeRequestBuilder = new Mock<IHackneyRepairsServiceRequestBuilder>();
            var mockLogger = new Mock<ILoggerAdapter<RepairsActions>>();

            var repairsActions = new RepairsActions(fakeRepairService.Object, fakeRequestBuilder.Object, mockLogger.Object);

            await Assert.ThrowsAsync<MissingRepairRequestException>(async () => await repairsActions.GetRepair("12345678"));
        }
    }
}
