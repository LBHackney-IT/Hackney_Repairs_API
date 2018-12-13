using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Logging
{
    public class LoggingAdapterTests
    {
        [Fact]
        public async Task Logs_information_When_Calling_PropertyActions_FindProperty()
        {
            var response = new PropertyInfoResponse()
            {
                PropertyList = new PropertySummary[2],
                Success = true
            };
            var property1 = new PropertySummary()
            {
                ShortAddress = "Front Office, Robert House, 6 - 15 Florfield Road",
                PostCodeValue = "E8 1DT",
                PropertyReference = "1/43453543"
            };
            var property2 = new PropertySummary()
            {
                ShortAddress = "Maurice Bishop House, 17 Reading Lane",
                PostCodeValue = "E8 1DT",
                PropertyReference = "2/32453245"
            };
            response.PropertyList[0] = property1;
            response.PropertyList[1] = property2;
            var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();

            var fakeService = new Mock<IHackneyPropertyService>();
            fakeService.Setup(service => service.GetPropertyListByPostCodeAsync(It.IsAny<ListByPostCodeRequest>()))
                .ReturnsAsync(response);

            var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
            fakeRequestBuilder.Setup(service => service.BuildListByPostCodeRequest("E8 1DT")).Returns(string.Empty);
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object,  mockLogger.Object);
            
            var result = await propertyActions.FindProperty("E8 1DT", null, null);

            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()));
        }
    }
}
