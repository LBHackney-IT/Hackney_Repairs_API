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
    public class UHWWarehouseRepositoryTest
    {
        private UniversalHousingSimulator<UHWWarehouseDbContext> _simulator;
        private ILoggerAdapter<UHWWarehouseRepository> _logger;

        public UHWWarehouseRepositoryTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

            _logger = new Mock<ILoggerAdapter<UHWWarehouseRepository>>().Object;
            _simulator = new UniversalHousingSimulator<UHWWarehouseDbContext>();

            _simulator.Reset();
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrderByPropertyReference_should_return_no_work_orders_when_none_exist()
        {
            var repo = new UHWWarehouseRepository((UHWWarehouseDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrderByPropertyReference("123");

            Assert.Empty(workOrders);
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrderByPropertyReference_should_return_a_work_order_when_one_exists_for_the_property()
        {
            _simulator.InsertTrade(trade: "AS", trade_desc: "Asbestos");
            _simulator.InsertTask(rq_ref: "00000012", trade: "AS", task_no: 1, job_code: "JOB1234");
            _simulator.InsertRequest(rq_ref: "00000012", rq_problem: "christmas problems");
            _simulator.InsertWorkOrder(
                wo_ref: "00000002",
                prop_ref: "00076258",
                rq_ref: "00000012",
                est_cost: 19.99,
                act_cost: 29.99,
                created: "2000-12-25 12:00:00",
                completed: "2001-01-01 13:30:00",
                u_servitor_ref: "servitor-ref",
                u_dlo_status: "ABC",
                wo_status: "300",
                date_due: "2001-01-02 17:30:00");

            var repo = new UHWWarehouseRepository((UHWWarehouseDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrderByPropertyReference("00076258");

            Assert.Single<UHWorkOrder>(workOrders);

            var retrievedWorkOrder = workOrders.First();
            var expectedWorkOrder = new UHWorkOrder
            {
                WorkOrderReference = "00000002",
                RepairRequestReference = "00000012",
                ProblemDescription = "christmas problems",
                WorkOrderStatus = "300",
                DLOStatus = "ABC",
                ServitorReference = "servitor-ref",
                PropertyReference = "00076258",
                SORCode = "JOB1234",
                Trade = "Asbestos",
                EstimatedCost = 19.99F,
                ActualCost = 29.99F,
                Created = new DateTime(2000, 12, 25, 12, 0, 0),
                AuthDate = new DateTime(),
                CompletedOn = new DateTime(2001, 1, 1, 13, 30, 0),
                DateDue = new DateTime(2001, 1, 2, 17, 30, 0)
            };

            Assert.Equal(
                JsonConvert.SerializeObject(expectedWorkOrder),
                JsonConvert.SerializeObject(retrievedWorkOrder));
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrderByPropertyReference_should_return_all_valid_work_orders_that_exist_for_the_property()
        {
            _simulator.InsertTrade(trade: "AS");
            _simulator.InsertTask(rq_ref: "00000002", trade: "AS", task_no: 1);
            _simulator.InsertRequest(rq_ref: "00000002");
            _simulator.InsertWorkOrder(wo_ref: "00000002", prop_ref: "00076258", rq_ref: "00000002", wo_status: "300", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000003", prop_ref: "00076258", rq_ref: "00000002", wo_status: "300", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000004", prop_ref: "00076258", rq_ref: "00000002", wo_status: "300", created: PreCutOff());

            var repo = new UHWWarehouseRepository((UHWWarehouseDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrderByPropertyReference("00076258");

            var retrievedWorkOrderRefs = workOrders.Select(wo => wo.WorkOrderReference);
            var expectedWorkOrderRefs = new[] { "00000002", "00000003", "00000004" };

            Assert.Equal(expectedWorkOrderRefs, retrievedWorkOrderRefs.ToArray());
        }

        [Fact.WhenUniversalHousingIsRunning]
        public async void GetWorkOrdersByPropertyReferences_should_return_all_work_orders_for_all_given_properties_from_the_last_day()
        {
            _simulator.InsertTrade(trade: "AS");
            _simulator.InsertTask(rq_ref: "00000012", trade: "AS", task_no: 1);
            _simulator.InsertRequest(rq_ref: "00000012");

            _simulator.InsertWorkOrder(wo_ref: "00000001", prop_ref: "00070001", rq_ref: "00000012", wo_status: "300", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000002", prop_ref: "00070001", rq_ref: "00000012", wo_status: "001", created: PostCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000003", prop_ref: "00070001", rq_ref: "00000012", wo_status: "002", created: PostCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000004", prop_ref: "00080001", rq_ref: "00000012", wo_status: "500", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000005", prop_ref: "00080001", rq_ref: "00000012", wo_status: "700", created: PreCutOff());
            _simulator.InsertWorkOrder(wo_ref: "00000006", prop_ref: "00080001", rq_ref: "00000012", wo_status: "900", created: PreCutOff());

            var repo = new UHWWarehouseRepository((UHWWarehouseDbContext)_simulator.context, _logger);
            var workOrders = await repo.GetWorkOrdersByPropertyReferences(new[] { "00070001", "00080001" }, new DateTime(2000, 1, 1), new DateTime(2079, 1, 1));

            var retrievedWorkOrderRefs = workOrders.Select(wo => wo.WorkOrderReference);
            var expectedWorkOrderRefs = new[] { "00000001", "00000004", "00000005", "00000006" };

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
