using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Services;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Services
{
    public class HackneyWorkOrdersServiceTest
    {
        public HackneyWorkOrdersServiceTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");
        }

        [Theory]
        [InlineData("300")]
        [InlineData("500")]
        [InlineData("700")]
        [InlineData("900")]
        public async void GetWorkOrder_retrieves_terminal_work_orders_from_uhw(string workOrderStatus)
        {
            var expectedWarehouseWorkOrder = new UHWorkOrder { WorkOrderStatus = workOrderStatus };

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhwWorkOrderForWorkOrderRef("00000001", expectedWarehouseWorkOrder)
                .Service;

            var workOrder = await service.GetWorkOrder("00000001");

            Assert.Same(expectedWarehouseWorkOrder, workOrder);
        }

        [Theory]
        [InlineData("01")]
        [InlineData("1000")]
        [InlineData("OTHER")]
        public async void GetWorkOrder_retrieves_still_active_work_orders_from_uht(string workOrderStatus)
        {
            var warehouseWorkOrder = new UHWorkOrder { WorkOrderStatus = workOrderStatus };
            var expectedUhtWorkOrder = new UHWorkOrder { WorkOrderStatus = workOrderStatus };

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhwWorkOrderForWorkOrderRef("00000001", warehouseWorkOrder)
                .WithUhtWorkOrderForWorkOrderRef("00000001", expectedUhtWorkOrder)
                .Service;

            var workOrder = await service.GetWorkOrder("00000001");

            Assert.NotSame(warehouseWorkOrder, workOrder);
            Assert.Same(expectedUhtWorkOrder, workOrder);
        }

        [Theory]
        [InlineData("300", "ACTIVE")]
        [InlineData("500", "OTHER")]
        [InlineData("700", "ACTIVE")]
        [InlineData("900", "OTHER")]
        public async void GetWorkOrders_only_retrieves_terminal_work_orders_from_uhw(string terminalStatus, string activeStatus)
        {
            var activeWarehouse123 = new UHWorkOrder { WorkOrderReference = "123", WorkOrderStatus = activeStatus };
            var activeUht123 = new UHWorkOrder { WorkOrderReference = "123", WorkOrderStatus = activeStatus };
            var terminalWarehouse456 = new UHWorkOrder { WorkOrderReference = "456", WorkOrderStatus = terminalStatus };

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhwWorkOrders(new UHWorkOrder[] { activeWarehouse123, terminalWarehouse456 })
                .WithUhtWorkOrders(new UHWorkOrder[] { activeUht123 })
                .Service;

            var workOrders = await service.GetWorkOrders(new string[] { "123", "456" });

            Assert.Equal(2, workOrders.ToArray().Length);
            Assert.Contains(activeUht123, workOrders);
            Assert.Contains(terminalWarehouse456, workOrders);
        }

        [Fact]
        public async void GetWorkOrderByPropertyReference_retrieves_recent_work_orders_for_the_given_property_reference_from_both_uht_and_uhw()
        {
            var uhtWorkOrders = new[] { new UHWorkOrder(), new UHWorkOrder() };
            var uhwWorkOrders = new[] { new UHWorkOrder() };

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhtWorkOrdersForPropertyRef("00000018", uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRef("00000018", uhwWorkOrders)
                .Service;

            var workOrders = await service.GetWorkOrderByPropertyReference("00000018");

            Assert.Contains(uhtWorkOrders[0], workOrders);
            Assert.Contains(uhtWorkOrders[1], workOrders);
            Assert.Contains(uhwWorkOrders[0], workOrders);
        }

        [Fact]
        public async void GetWorkOrderByPropertyReference_returns_null_if_no_work_orders_are_found_and_the_property_does_not_exist()
        {
            var uhtWorkOrders = new UHWorkOrder[0];
            var uhwWorkOrders = new UHWorkOrder[0];

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhtWorkOrdersForPropertyRef("00000019", uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRef("00000019", uhwWorkOrders)
                .WithUHWarehousePropertyDetails("00000019", null)
                .Service;

            var workOrders = await service.GetWorkOrderByPropertyReference("00000019");

            Assert.Null(workOrders);
        }

        [Fact]
        public async void GetWorkOrderByPropertyReference_returns_an_empty_list_if_no_work_orders_are_found_but_the_property_does_exist()
        {
            var uhtWorkOrders = new UHWorkOrder[0];
            var uhwWorkOrders = new UHWorkOrder[0];
            var property = new PropertyDetails();

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhtWorkOrdersForPropertyRef("00000020", uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRef("00000020", uhwWorkOrders)
                .WithUHWarehousePropertyDetails("00000020", property)
                .Service;

            var workOrders = await service.GetWorkOrderByPropertyReference("00000020");

            Assert.Empty(workOrders);
        }

        [Fact]
        public async void GetWorkOrdersByPropertyReferences_retrieves_recent_work_orders_for_the_given_property_references_from_both_uht_and_uhw()
        {
            var propertyRefs = new string[] { "00000018", "00000019" };
            var uhtWorkOrders = new[] { new UHWorkOrder(), new UHWorkOrder() };
            var uhwWorkOrders = new[] { new UHWorkOrder() };

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhtWorkOrdersForPropertyRefs(propertyRefs, uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRefs(propertyRefs, uhwWorkOrders)
                .Service;
            var date = DateTime.Now;
            var workOrders = await service.GetWorkOrdersByPropertyReferences(propertyRefs, date, date);

            Assert.Contains(uhtWorkOrders[0], workOrders);
            Assert.Contains(uhtWorkOrders[1], workOrders);
            Assert.Contains(uhwWorkOrders[0], workOrders);
        }

        [Fact]
        public async void GetWorkOrdersByPropertyReferences_returns_an_empty_list_if_no_work_orders_are_found_regardless_of_whether_the_property_exists()
        {
            var uhtWorkOrders = new UHWorkOrder[0];
            var uhwWorkOrders = new UHWorkOrder[0];
            var property = new PropertyDetails();

            var service = new HackneyWorkOrdersServiceTestBuilder()
                .WithUhtWorkOrdersForPropertyRef("00000020", uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRef("00000020", uhwWorkOrders)
                .WithUhtWorkOrdersForPropertyRef("00000021", uhtWorkOrders)
                .WithUHWarehouseWorkOrdersForPropertyRef("00000021", uhwWorkOrders)
                .WithUHWarehousePropertyDetails("00000020", property)
                .Service;

            DateTime date = DateTime.Now;
            var workOrders = await service.GetWorkOrdersByPropertyReferences(new string[] { "00000020", "00000021" }, date, date);

            Assert.Empty(workOrders);
        }

        private class HackneyWorkOrdersServiceTestBuilder
        {
            private Mock<IUhtRepository> _uhtRepositoryMock;
            private Mock<IUHWWarehouseRepository> _uhWarehouseRepositoryMock;
            private Mock<ILoggerAdapter<WorkOrdersActions>> _loggerMock;

            public HackneyWorkOrdersServiceTestBuilder()
            {
                _uhtRepositoryMock = new Mock<IUhtRepository>();
                _uhWarehouseRepositoryMock = new Mock<IUHWWarehouseRepository>();
                _loggerMock = new Mock<ILoggerAdapter<WorkOrdersActions>>();
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhwWorkOrderForWorkOrderRef(string workOrderRef, UHWorkOrder workOrder)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetWorkOrderByWorkOrderReference(workOrderRef)).Returns(Task.FromResult<UHWorkOrder>(workOrder));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrderForWorkOrderRef(string workOrderRef, UHWorkOrder workOrder)
            {
                _uhtRepositoryMock.Setup(repo => repo.GetWorkOrder(workOrderRef)).Returns(Task.FromResult<UHWorkOrder>(workOrder));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrdersForPropertyRef(string propertyRef, UHWorkOrder[] workOrders)
            {
                _uhtRepositoryMock.Setup(repo => repo.GetWorkOrderByPropertyReference(propertyRef)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrdersForPropertyRefs(string[] propertyRefs, UHWorkOrder[] workOrders)
            {
                _uhtRepositoryMock.Setup(repo => repo.GetWorkOrdersByPropertyReferences(propertyRefs, It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehouseWorkOrdersForPropertyRef(string propertyRef, UHWorkOrder[] workOrders)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetWorkOrderByPropertyReference(propertyRef)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehouseWorkOrdersForPropertyRefs(string[] propertyRefs, UHWorkOrder[] workOrders)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetWorkOrdersByPropertyReferences(propertyRefs, It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehousePropertyDetails(string propertyRef, PropertyDetails property)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetPropertyDetailsByReference(propertyRef)).Returns(Task.FromResult<PropertyDetails>(property));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhwWorkOrders(UHWorkOrder[] workOrders)
            {
                _uhWarehouseRepositoryMock
                    .Setup(repo => repo.GetWorkOrdersByWorkOrderReferences(It.IsAny<string[]>()))
                    .Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrders(UHWorkOrder[] workOrders)
            {
                _uhtRepositoryMock
                    .Setup(repo => repo.GetWorkOrders(It.IsAny<string[]>()))
                    .Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));

                return this;
            }

            public HackneyWorkOrdersService Service => new HackneyWorkOrdersService(_uhtRepositoryMock.Object, null, _uhWarehouseRepositoryMock.Object, _loggerMock.Object);
        }
    }
}
