using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Services;
using RepairsService;
using Xunit;

namespace HackneyRepairs.Tests.Services
{
    public class HackneyRepairsServiceRequestBuilderTests
    {
        private NameValueCollection configuration = new NameValueCollection
            {
                { "UHUsername", "uhuser" },
                { "UHPassword", "uhpassword" },
                { "UHSourceSystem", "sourcesystem" },
                { "UhSorSupplierMapping", "08500820,H01|20040010,H01|20040020,H01|20040060,H01|20040310,H01|20060020,H01|20060030,H01|20110010,H01|48000000,H05|PRE00001,H02" }
            };

        [Fact]
        public void return_a_built_request_object()
        {
            var builder = new HackneyRepairsServiceRequestBuilder(new NameValueCollection());
            var request = builder.BuildNewRepairRequest(new RepairRequest { Contact = new RepairRequestContact() });
            Assert.IsType<NewRepairRequest>(request);
        }

        [Fact]
        public void build_new_repair_request_builds_a_valid_request()
        {
            var builder = new HackneyRepairsServiceRequestBuilder(configuration);
            var request = builder.BuildNewRepairRequest(new RepairRequest
            {
                Priority = "N",
                PropertyReference = "123456",
                ProblemDescription = "tap leaking",
                Contact = new RepairRequestContact
                {
                    Name = "Test",
                    TelephoneNumber = "0123456789"
                }
            });
            Assert.Equal("N", request.RepairRequest.Priority);
            Assert.Equal("123456", request.RepairRequest.PropertyRef);
            Assert.Equal("tap leaking", request.RepairRequest.Problem);
            Assert.Equal("uhuser", request.DirectUser.UserName);
            Assert.Equal("uhpassword", request.DirectUser.UserPassword);
            Assert.Equal("sourcesystem", request.SourceSystem);
            Assert.Equal("Test", request.RepairRequest.Name);
            Assert.Equal("0123456789", request.RepairRequest.Phone);
        }

        [Fact]
        public async Task should_return_the_correct_contractor_reference_for_a_given_SOR_code()
        {
            var builder = new HackneyRepairsServiceRequestBuilder(configuration);
            var request = builder.BuildNewRepairTasksRequest(new RepairRequest
            {
                Priority = "N",
                PropertyReference = "123456",
                ProblemDescription = "tap leaking",
                Contact = new RepairRequestContact
                {
                    Name = "Test",
                    TelephoneNumber = "0123456789"
                },
                WorkOrders = new List<WorkOrder>
                {
                    new WorkOrder
                    {
                        SorCode = "20110010",
                        EstimatedUnits = "1"
                    }
                }
            });
            Assert.Equal("H01", request.TaskList[0].SupplierReference);
        }

        [Fact]
        public async Task should_raise_an_SOR_exception_for_an_invalid_SOR_code()
        {
            var builder = new HackneyRepairsServiceRequestBuilder(configuration);
            Assert.Throws<InvalidSORCodeException>(() =>
                            builder.BuildNewRepairTasksRequest(new RepairRequest
                            {
                                Priority = "N",
                                PropertyReference = "123456",
                                ProblemDescription = "tap leaking",
                                Contact = new RepairRequestContact
                                {
                                    Name = "Test",
                                    TelephoneNumber = "0123456789"
                                },
                                WorkOrders = new List<WorkOrder>
                                {
                                     new WorkOrder
                                     {
                                         SorCode = "20110020"
                                     }
                                }
                            }));
        }

        public async Task should_raise_an_SOR_exception_for_an_empty_SOR_code()
        {
            var builder = new HackneyRepairsServiceRequestBuilder(configuration);
            Assert.Throws<InvalidSORCodeException>(() =>
                            builder.BuildNewRepairTasksRequest(new RepairRequest
                            {
                                Priority = "N",
                                PropertyReference = "123456",
                                ProblemDescription = "tap leaking",
                                Contact = new RepairRequestContact
                                {
                                    Name = "Test",
                                    TelephoneNumber = "0123456789"
                                },
                                WorkOrders = new List<WorkOrder>
                                {
                                     new WorkOrder
                                     {
                                         SorCode = ""
                                     }
                                }
                            }));
        }
    }
}
