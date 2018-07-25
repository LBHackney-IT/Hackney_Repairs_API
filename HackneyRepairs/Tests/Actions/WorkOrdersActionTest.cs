using System;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Actions
{
	public class WorkOrdersActionTest
	{
		Mock<ILoggerAdapter<WorkOrdersActions>> _mockLogger;
		public WorkOrdersActionTest()
		{
			_mockLogger = new Mock<ILoggerAdapter<WorkOrdersActions>>();
		}

		[Fact]
		public async Task get_work_orders_returns_a_work_order()
		{
			Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
			_workOrderService.Setup(service => service.GetWorkOrderByReference(It.IsAny<string>()))
							 .ReturnsAsync(new WorkOrderEntity());
			WorkOrdersActions workOrdersActions = new WorkOrdersActions(_workOrderService.Object, _mockLogger.Object);

			var response = await workOrdersActions.GetWorkOrderByReference("12345678");

			Assert.True(response is WorkOrderEntity);
		}

		[Fact]
		public async Task get_work_orders_returns_a_work_order_with_the_same_ref_as_paramater()
		{
			Random rnd = new Random();
			string randomReference = rnd.Next(10000000, 99999999).ToString();
			Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
			_workOrderService.Setup(service => service.GetWorkOrderByReference(randomReference))
							 .Returns(Task.FromResult(new WorkOrderEntity { wo_ref = randomReference }));
			WorkOrdersActions workOrdersActions = new WorkOrdersActions(_workOrderService.Object, _mockLogger.Object);

			var response = await workOrdersActions.GetWorkOrderByReference(randomReference);
			var expected = new WorkOrderEntity
			{
				wo_ref = randomReference
			};
            
			Assert.Equal(response.wo_ref, expected.wo_ref);
		}

		[Fact]
		public async Task get_work_orders_throws_not_found_excpetion_when_no_work_order_found()
		{
			Random rnd = new Random();
            string randomReference = rnd.Next(100000000, 999999990).ToString();
			Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
			_workOrderService.Setup(service => service.GetWorkOrderByReference(randomReference))
			                 .Returns(Task.FromResult( (WorkOrderEntity) null));
			WorkOrdersActions workOrdersActions = new WorkOrdersActions(_workOrderService.Object, _mockLogger.Object);         
			await Assert.ThrowsAsync<MissingWorkOrderException>(async () => await workOrdersActions.GetWorkOrderByReference(randomReference));
		}
	}
}
