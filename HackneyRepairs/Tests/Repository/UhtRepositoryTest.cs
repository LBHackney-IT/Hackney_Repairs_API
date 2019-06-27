using System;
using System.Linq;
using HackneyRepairs.DbContext;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace HackneyRepairs.Tests.Repository
{
    [Collection("Universal Housing")]
    public class UhtRepositoryTest
    {
        private UniversalHousingSimulator<UhtDbContext> _simulator;
        private ILoggerAdapter<UhtRepository> _logger;

        public UhtRepositoryTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

            _logger = new Mock<ILoggerAdapter<UhtRepository>>().Object;
            _simulator = new UniversalHousingSimulator<UhtDbContext>();

            _simulator.Reset();
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrderByPropertyReference_should_return_no_work_orders_when_none_exist()
        {
            var repo = new UhtRepository((UhtDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrderByPropertyReference("123");

            Assert.Empty(workOrders);
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrderByPropertyReference_should_return_a_work_order_when_one_exists_for_the_property()
        {
            _simulator.InsertTrade(trade: "DR", trade_desc: "Door Repair");
            _simulator.InsertTask(rq_ref: "00000021", trade: "DR", task_no: 1, job_code: "JOB4321");
            _simulator.InsertRequest(rq_ref: "00000021", rq_problem: "can't get in");
            _simulator.InsertWorkOrder(
                wo_ref: "00000009",
                prop_ref: "00070000",
                rq_ref: "00000021",
                est_cost: 19.99,
                act_cost: 29.99,
                created: "2019-05-20 12:00:00",
                completed: "2020-01-01 13:30:00",
                u_servitor_ref: "SR1",
                u_dlo_status: "XYZ",
                wo_status: "999",
                date_due: "2020-01-02 17:30:00");

            var repo = new UhtRepository((UhtDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrderByPropertyReference("00070000");

            Assert.Single<UHWorkOrder>(workOrders);

            var retrievedWorkOrder = workOrders.First();
            var expectedWorkOrder = new UHWorkOrder
            {
                WorkOrderReference = "00000009",
                RepairRequestReference = "00000021",
                ProblemDescription = "can't get in",
                WorkOrderStatus = "999",
                DLOStatus = "XYZ",
                ServitorReference = "SR1",
                PropertyReference = "00070000",
                SORCode = "JOB4321",
                Trade = "Door Repair",
                EstimatedCost = 19.99F,
                ActualCost = 29.99F,
                Created = new DateTime(2019, 5, 20, 12, 0, 0),
                AuthDate = new DateTime(1900, 1, 1),
                CompletedOn = new DateTime(2020, 1, 1, 13, 30, 0),
                DateDue = new DateTime(2020, 1, 2, 17, 30, 0)
            };

            Assert.Equal(
                JsonConvert.SerializeObject(expectedWorkOrder),
                JsonConvert.SerializeObject(retrievedWorkOrder));
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrdersByPropertyReferences_should_return_all_work_orders_for_all_given_properties()
        {
            _simulator.InsertTrade(trade: "AS");
            _simulator.InsertTask(rq_ref: "00000012", trade: "AS", task_no: 1);
            _simulator.InsertRequest(rq_ref: "00000012");

            _simulator.InsertWorkOrder(wo_ref: "00000001", prop_ref: "00070001", rq_ref: "00000012", wo_status: "300", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000002", prop_ref: "00070001", rq_ref: "00000012", wo_status: "001", created: PostCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000003", prop_ref: "00070001", rq_ref: "00000012", wo_status: "002", created: PostCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000004", prop_ref: "00080001", rq_ref: "00000012", wo_status: "500", created: PostCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000005", prop_ref: "00080001", rq_ref: "00000012", wo_status: "700", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000006", prop_ref: "00080001", rq_ref: "00000012", wo_status: "900", created: PreCutOff());

            var repo = new UhtRepository((UhtDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrdersByPropertyReferences(new[] { "00070001", "00080001" }, new DateTime(2000, 1, 1), new DateTime(2079, 1, 1));

            var retrievedWorkOrderRefs = workOrders.Select(wo => wo.WorkOrderReference);
            var expectedWorkOrderRefs = new[] { "00000002", "00000003", "00000004" };

            Assert.Equal(expectedWorkOrderRefs, retrievedWorkOrderRefs.ToArray());
        }

        private string PreCutOff()
        {
          DateTime dtCutoff = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 0, 0);
          dtCutoff = dtCutoff.AddDays(-1);
          return dtCutoff.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string PostCutOff()
        {
          DateTime dtCutoff = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
          return dtCutoff.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
