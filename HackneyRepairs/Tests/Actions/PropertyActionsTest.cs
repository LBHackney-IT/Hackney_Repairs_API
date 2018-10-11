using System.Threading.Tasks;
using HackneyRepairs.Actions;
using Moq;
using Xunit;
using System.Text;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using Newtonsoft.Json;
using HackneyRepairs.Models;
using HackneyRepairs.Services;
using System.Collections.Generic;
using System;

namespace HackneyRepairs.Tests.Actions
{
	public class PropertyActionsTest
	{
		[Fact]
		public async Task find_properties_returns_a_list_of_properties()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
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
			var PropertyList = new PropertySummary[2];
			PropertyList[0] = property1;
			PropertyList[1] = property2;
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyListByPostCode("E8 1DT"))
				.ReturnsAsync(PropertyList);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			fakeRequestBuilder.Setup(service => service.BuildListByPostCodeRequest("E8 1DT"))
				.Returns("E8 1DT");
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			var results = await propertyActions.FindProperty("E8 1DT");
			var outputproperty1 = new
			{
				address = "Front Office, Robert House, 6 - 15 Florfield Road",
				postcode = "E8 1DT",
				propertyReference = "1/43453543"
			};
			var outputproperty2 = new
			{
				address = "Maurice Bishop House, 17 Reading Lane",
				postcode = "E8 1DT",
				propertyReference = "2/32453245"
			};
			var properties = new object[2];
			properties[0] = outputproperty1;
			properties[1] = outputproperty2;
			var json = new { results = properties };
			Assert.Equal(JsonConvert.SerializeObject(json), JsonConvert.SerializeObject(results));
		}

		[Fact]
		public async Task find_properties_returns_an_empty_response_when_no_matches()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var PropertyList = new PropertySummary[0];
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyListByPostCode(It.IsAny<string>()))
				.ReturnsAsync(PropertyList);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			fakeRequestBuilder.Setup(service => service.BuildListByPostCodeRequest("E8 2LN")).Returns(string.Empty);
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			var results = await propertyActions.FindProperty("E8 2LN");
			var properties = new object[0];
			var json = new { results = properties };
			Assert.Equal(JsonConvert.SerializeObject(json), JsonConvert.SerializeObject(results));
		}

		[Fact]
		public async Task find_properties_raises_an_exception_if_the_property_list_is_missing()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var fakeService = new Mock<IHackneyPropertyService>();
			PropertySummary[] response = null;
			fakeService.Setup(service => service.GetPropertyListByPostCode(It.IsAny<string>()))
				.ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			await Assert.ThrowsAsync<PropertyServiceException>(async () => await propertyActions.FindProperty("E8 2LN"));
		}

		[Fact]
		public async Task get_property_details_by_reference_returns_a_property_object_for_a_valid_request()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var response = new PropertyDetails()
			{
				ShortAddress = "Front Office, Robert House, 6 - 15 Florfield Road",
				PostCodeValue = "E8 1DT",
				PropertyReference = "43453543",
				Maintainable = false
			};
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyByRefAsync("43453543")).ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			var results = await propertyActions.FindPropertyDetailsByRef("43453543");
			var property = new
			{
				address = "Front Office, Robert House, 6 - 15 Florfield Road",
				postcode = "E8 1DT",
				propertyReference = "43453543",
				maintainable = false
			};
			Assert.Equal(property, results);
		}

		[Fact]
		public async Task get_property_details_by_reference_raises_an_exception_if_the_property_is_missing()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var response = (PropertyDetails)null;
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyByRefAsync(It.IsAny<string>()))
				.ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			await Assert.ThrowsAsync<MissingPropertyException>(async () => await propertyActions.FindPropertyDetailsByRef("52525252534"));
		}

		[Fact]
		public async Task get_property_details_by_reference_raises_an_exception_if_the_service_responds_with_an_error()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyByRefAsync(It.IsAny<string>()))
					   .ThrowsAsync(new System.Exception());
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			await Assert.ThrowsAsync<PropertyServiceException>(async () => await propertyActions.FindPropertyDetailsByRef("525252525"));
		}

		[Fact]
		public async Task get_property_block_details_by_reference_returns_a_property_object_for_a_valid_request()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var response = new PropertyDetails()
			{
				ShortAddress = "Front Office Block, Robert House, 6 - 15 Florfield Road",
				PostCodeValue = "E8 1DT",
				PropertyReference = "43453543",
				Maintainable = true
			};
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyBlockByRef("43453543"))
				.ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			var results = await propertyActions.FindPropertyBlockDetailsByRef("43453543");
			var property = new
			{
				address = "Front Office Block, Robert House, 6 - 15 Florfield Road",
				postcode = "E8 1DT",
				propertyReference = "43453543",
				maintainable = true
			};
			Assert.Equal(property, results);
		}

		[Fact]
		public async Task get_property_block_details_by_reference_raises_an_exception_if_the_service_responds_with_an_error()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var request = new ByPropertyRefRequest();
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyBlockByRef("525252525"))
					   .ThrowsAsync(new System.Exception());
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			await Assert.ThrowsAsync<PropertyServiceException>(async () => await propertyActions.FindPropertyBlockDetailsByRef("525252525"));
		}

		[Fact]
		public async Task get_property_estate_details_by_reference_returns_a_property_object_for_a_valid_request()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var response = new PropertyDetails()
			{
				ShortAddress = "Front Office Estate, Robert House, 6 - 15 Florfield Road",
				PostCodeValue = "E8 1DT",
				PropertyReference = "43453543",
				Maintainable = true
			};
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyEstateByRef("43453543"))
				.ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			var results = await propertyActions.FindPropertyEstateDetailsByRef("43453543");
			var property = new
			{
				address = "Front Office Estate, Robert House, 6 - 15 Florfield Road",
				postcode = "E8 1DT",
				propertyReference = "43453543",
				maintainable = true
			};
			Assert.Equal(property, results);
		}

		[Fact]
		public async Task get_property_estate_details_by_reference_returns_an_empty_property_details_object_if_the_property_is_missing()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var response = new PropertyDetails
			{
				ShortAddress = "An Address Estate",
				PostCodeValue = "A Postcode",
				PropertyReference = "52525252",
				Maintainable = true
			};
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyEstateByRef("52525252534"))
				.ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var results = new
			{
				address = "An Address Estate",
				postcode = "A Postcode",
				propertyReference = "52525252",
				maintainable = true
			};
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			Assert.Equal(results, await propertyActions.FindPropertyEstateDetailsByRef("52525252534"));
		}

		[Fact]
		public async Task get_property_estate_details_by_reference_raises_an_exception_if_the_service_responds_with_an_error()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var request = new ByPropertyRefRequest();
			var response = (PropertyDetails)null;
			var fakeService = new Mock<IHackneyPropertyService>();
			fakeService.Setup(service => service.GetPropertyEstateByRef("525252525")).ReturnsAsync(response);
			var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
			var workOrdersService = new Mock<IHackneyWorkOrdersService>();
			PropertyActions propertyActions = new PropertyActions(fakeService.Object, fakeRequestBuilder.Object, workOrdersService.Object, mockLogger.Object);
			await Assert.ThrowsAsync<MissingPropertyException>(async () => await propertyActions.FindPropertyEstateDetailsByRef("525252525"));
		}

		#region GetWorkOrdersForBlock

        [Fact]
        public async Task get_workorders_for_block_should_return_list_of_workorders_when_block_or_lower_property_passed()
		{
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
			var mockPropertyService = new Mock<IHackneyPropertyService>();
			var mockWorkordersService = new Mock<IHackneyWorkOrdersService>();
			PropertyLevelModel dwelling = new PropertyLevelModel()
            {
				MajorReference = "00074866",
                PropertyReference = "00079999",
                Address = "St Thomass Square 1 Pitcarin House",
                LevelCode = "7"

            };
			PropertyLevelModel block = new PropertyLevelModel()
			{
				MajorReference = "00078556",
				PropertyReference = "00074866",
				Address = "St Thomass Square 1-93 Pitcarin House",
				LevelCode = "3"

			};
			PropertyLevelModel estate = new PropertyLevelModel()
			{
				MajorReference = "00087086",
				PropertyReference = "00078556",
				Address = "Frampton Park Road Frampton Park Estate",
				LevelCode = "2"
			};
			PropertyLevelModel hackney = new PropertyLevelModel()
			{
				MajorReference = "",
				PropertyReference = "00087086",
				Address = "Wilton Way Hackney Homes Limited",
				LevelCode = "0"
			};

			mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00079999"))
			                   .Returns(Task.FromResult<PropertyLevelModel>(block));
			mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00074866"))
			                   .Returns(Task.FromResult<PropertyLevelModel>(estate));
			mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00078556"))
                               .Returns(Task.FromResult<PropertyLevelModel>(hackney));
			mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00087086"))
                   .Returns(Task.FromResult<PropertyLevelModel>(null));

			IEnumerable<UHWorkOrder> workOrders = new List<UHWorkOrder>
			{
				new UHWorkOrder()
                {
					PropertyReference = "00079999"
                },
				new UHWorkOrder()
				{
					PropertyReference = "00074866"
				},
				new UHWorkOrder()
				{
					PropertyReference = "00078556"
				},
				new UHWorkOrder()
				{
					PropertyReference = "00087086"
				}
			};

            mockWorkordersService.Setup(service => service.GetWorkOrderByBlockReference("00074866", "Plumbing", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
			                     .Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(workOrders));
			var mockRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();

			PropertyActions propertyActions = new PropertyActions(mockPropertyService.Object, mockRequestBuilder.Object, mockWorkordersService.Object, mockLogger.Object);

            DateTime date = DateTime.Now;
            var response = await propertyActions.GetWorkOrdersForBlock("00079999", "Plumbing", date, date);

			Assert.True(response is IEnumerable<UHWorkOrder>);
			Assert.IsType(new List<UHWorkOrder>().GetType(), response);
		}

		[Fact]
		public async Task get_workorders_for_block_should_return_empty_list_of_if_no_workorders_found()
        {
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
            var mockPropertyService = new Mock<IHackneyPropertyService>();
            var mockWorkordersService = new Mock<IHackneyWorkOrdersService>();
            PropertyLevelModel dwelling = new PropertyLevelModel()
            {
                MajorReference = "00074866",
                PropertyReference = "00079999",
                Address = "St Thomass Square 1 Pitcarin House",
                LevelCode = "7"

            };
            PropertyLevelModel block = new PropertyLevelModel()
            {
                MajorReference = "00078556",
                PropertyReference = "00074866",
                Address = "St Thomass Square 1-93 Pitcarin House",
                LevelCode = "3"

            };
            PropertyLevelModel estate = new PropertyLevelModel()
            {
                MajorReference = "00087086",
                PropertyReference = "00078556",
                Address = "Frampton Park Road Frampton Park Estate",
                LevelCode = "2"
            };
            PropertyLevelModel hackney = new PropertyLevelModel()
            {
                MajorReference = "",
                PropertyReference = "00087086",
                Address = "Wilton Way Hackney Homes Limited",
                LevelCode = "0"
            };
   
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00079999"))
                               .Returns(Task.FromResult<PropertyLevelModel>(block));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00074866"))
                               .Returns(Task.FromResult<PropertyLevelModel>(estate));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00078556"))
                               .Returns(Task.FromResult<PropertyLevelModel>(hackney));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00087086"))
                   .Returns(Task.FromResult<PropertyLevelModel>(null));

            mockWorkordersService.Setup(service => service.GetWorkOrderByBlockReference("00074866", "Plumbing", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
			                     .Returns(Task.FromResult<IEnumerable<UHWorkOrder>>(new List<UHWorkOrder>()));
            var mockRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();

            PropertyActions propertyActions = new PropertyActions(mockPropertyService.Object, mockRequestBuilder.Object, mockWorkordersService.Object, mockLogger.Object);

            DateTime date = DateTime.Now;
            var response = await propertyActions.GetWorkOrdersForBlock("00079999", "Plumbing", date, date);

            Assert.True(response is IEnumerable<UHWorkOrder>);
            Assert.IsType(new List<UHWorkOrder>().GetType(), response);
        }

		[Fact]
        public async Task get_workorders_for_block_should_return_missing_property_exception_if_property_not_found()
        {
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
            var mockPropertyService = new Mock<IHackneyPropertyService>();
            var mockWorkordersService = new Mock<IHackneyWorkOrdersService>();
            
            
			mockPropertyService.Setup(service => service.GetPropertyLevelInfo(It.IsAny<string>()))
                               .Returns(Task.FromResult<PropertyLevelModel>(null));
   
            var mockRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();

            PropertyActions propertyActions = new PropertyActions(mockPropertyService.Object, mockRequestBuilder.Object, mockWorkordersService.Object, mockLogger.Object);
            DateTime date = DateTime.Now;

			await Assert.ThrowsAsync<MissingPropertyException>(async () => await propertyActions.GetWorkOrdersForBlock("00079999", "Plumbing", date, date));
        }

		[Fact]
        public async Task get_workorders_for_block_should_return_invalid_parameter_exception_if_property_is_higher_than_block()
        {
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
            var mockPropertyService = new Mock<IHackneyPropertyService>();
            var mockWorkordersService = new Mock<IHackneyWorkOrdersService>();
            PropertyLevelModel dwelling = new PropertyLevelModel()
            {
                MajorReference = "00074866",
                PropertyReference = "00079999",
                Address = "St Thomass Square 1 Pitcarin House",
                LevelCode = "7"

            };
            PropertyLevelModel block = new PropertyLevelModel()
            {
                MajorReference = "00078556",
                PropertyReference = "00074866",
                Address = "St Thomass Square 1-93 Pitcarin House",
                LevelCode = "3"

            };
            PropertyLevelModel estate = new PropertyLevelModel()
            {
                MajorReference = "00087086",
                PropertyReference = "00078556",
                Address = "Frampton Park Road Frampton Park Estate",
                LevelCode = "2"
            };
            PropertyLevelModel hackney = new PropertyLevelModel()
            {
                MajorReference = "",
                PropertyReference = "00087086",
                Address = "Wilton Way Hackney Homes Limited",
                LevelCode = "0"
            };

            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00079999"))
                               .Returns(Task.FromResult<PropertyLevelModel>(block));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00074866"))
                               .Returns(Task.FromResult<PropertyLevelModel>(estate));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00078556"))
                               .Returns(Task.FromResult<PropertyLevelModel>(hackney));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00087086"))
                   .Returns(Task.FromResult<PropertyLevelModel>(null));
   
            var mockRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();

            PropertyActions propertyActions = new PropertyActions(mockPropertyService.Object, mockRequestBuilder.Object, mockWorkordersService.Object, mockLogger.Object);
            DateTime date = DateTime.Now;

			await Assert.ThrowsAsync<InvalidParameterException>(async () => await propertyActions.GetWorkOrdersForBlock("00078556", "Plumbing", date, date));
        }

		[Fact]
		public async Task get_workorders_for_block_should_return_invalid_parameter_exception_if_trade_not_provided()
        {
			var mockLogger = new Mock<ILoggerAdapter<PropertyActions>>();
            var mockPropertyService = new Mock<IHackneyPropertyService>();
            var mockWorkordersService = new Mock<IHackneyWorkOrdersService>();
            PropertyLevelModel dwelling = new PropertyLevelModel()
            {
                MajorReference = "00074866",
                PropertyReference = "00079999",
                Address = "St Thomass Square 1 Pitcarin House",
                LevelCode = "7"

            };
            PropertyLevelModel block = new PropertyLevelModel()
            {
                MajorReference = "00078556",
                PropertyReference = "00074866",
                Address = "St Thomass Square 1-93 Pitcarin House",
                LevelCode = "3"

            };
            PropertyLevelModel estate = new PropertyLevelModel()
            {
                MajorReference = "00087086",
                PropertyReference = "00078556",
                Address = "Frampton Park Road Frampton Park Estate",
                LevelCode = "2"
            };
            PropertyLevelModel hackney = new PropertyLevelModel()
            {
                MajorReference = "",
                PropertyReference = "00087086",
                Address = "Wilton Way Hackney Homes Limited",
                LevelCode = "0"
            };

            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00079999"))
                               .Returns(Task.FromResult<PropertyLevelModel>(block));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00074866"))
                               .Returns(Task.FromResult<PropertyLevelModel>(estate));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00078556"))
                               .Returns(Task.FromResult<PropertyLevelModel>(hackney));
            mockPropertyService.Setup(service => service.GetPropertyLevelInfo("00087086"))
                   .Returns(Task.FromResult<PropertyLevelModel>(null));

            var mockRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();

            PropertyActions propertyActions = new PropertyActions(mockPropertyService.Object, mockRequestBuilder.Object, mockWorkordersService.Object, mockLogger.Object);
            DateTime date = DateTime.Now;

			await Assert.ThrowsAsync<InvalidParameterException>(async () => await propertyActions.GetWorkOrdersForBlock("00079999", "", date, date));
        }
  
        #endregion
	}
    
}
