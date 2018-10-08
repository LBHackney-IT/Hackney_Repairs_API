using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            var workOrders = await service.GetWorkOrdersByPropertyReferences(propertyRefs, null, null);

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

            var workOrders = await service.GetWorkOrdersByPropertyReferences(new string[] { "00000020", "00000021" }, null, null);

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

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrdersForPropertyRef(string propertyRef, UHWorkOrder[] workOrders)
            {
                _uhtRepositoryMock.Setup(repo => repo.GetWorkOrderByPropertyReference(propertyRef)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUhtWorkOrdersForPropertyRefs(string[] propertyRefs, UHWorkOrder[] workOrders)
            {
                _uhtRepositoryMock.Setup(repo => repo.GetWorkOrdersByPropertyReferences(propertyRefs, DateTime.Now, DateTime.Now)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehouseWorkOrdersForPropertyRef(string propertyRef, UHWorkOrder[] workOrders)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetWorkOrderByPropertyReference(propertyRef)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehouseWorkOrdersForPropertyRefs(string[] propertyRefs, UHWorkOrder[] workOrders)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetWorkOrdersByPropertyReferences(propertyRefs, DateTime.Now, DateTime.Now)).Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
                return this;
            }

            public HackneyWorkOrdersServiceTestBuilder WithUHWarehousePropertyDetails(string propertyRef, PropertyDetails property)
            {
                _uhWarehouseRepositoryMock.Setup(repo => repo.GetPropertyDetailsByReference(propertyRef)).Returns(Task.FromResult<PropertyDetails>(property));
                return this;
            }

            public HackneyWorkOrdersService Service => new HackneyWorkOrdersService(_uhtRepositoryMock.Object, null, _uhWarehouseRepositoryMock.Object, _loggerMock.Object);
        }
    }
}
