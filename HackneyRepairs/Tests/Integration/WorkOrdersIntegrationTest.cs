using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using HackneyRepairs.Entities;
using System.Collections.Generic;
using System.Linq;
using HackneyRepairs.Models;
using System;

namespace HackneyRepairs.Tests.Integration
{
    public class WorkOrdersIntegrationTest
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public WorkOrdersIntegrationTest()
        {
            Environment.SetEnvironmentVariable("UhtDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhwDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhWarehouseDb", "connectionString=Test");
            _server = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

        #region GET Work order by work order reference tests
        [Fact]
        public async Task return_a_200_result_with_workOrder_json_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/work_orders/12345678");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<UHWorkOrder>(jsonResult);

            Assert.IsType<UHWorkOrder>(workOrder);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_200_result_with_workOrderWithMobileReports_json_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/work_orders/12345678?include=mobilereports");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<UHWorkOrderWithMobileReports>(jsonResult);

            Assert.IsType<UHWorkOrderWithMobileReports>(workOrder);
            Assert.True(workOrder.MobileReports.Any());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_400_result_for_an_invalid_parameter()
        {
            var badParameterValue = "badparam";
            var result = await _client.GetAsync($"v1/work_orders/12345678?include={badParameterValue}");

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_404_result_for_no_workorder_matching_reference()
        {
			var result = await _client.GetAsync("v1/work_orders/0");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        #endregion

        #region
        [Fact]
        public async Task when_retrieving_a_single_work_order_should_return_work_order_json()
        {
            var result = await _client.GetAsync("v1/work_orders/by_references?reference=123");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrders = JsonConvert.DeserializeObject<List<UHWorkOrder>>(jsonResult);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Single(workOrders);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task when_retrieving_a_single_work_order_with_mobile_reports_should_return_with_mobile_reports()
        {
            var result = await _client.GetAsync("v1/work_orders/by_references?reference=123&include=mobilereports");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrders = JsonConvert.DeserializeObject<List<UHWorkOrderWithMobileReports>>(jsonResult);

            Assert.Equal("Mobile report path", workOrders.FirstOrDefault().MobileReports.FirstOrDefault().ReportUri);
        }

        [Fact]
        public async Task when_retrieving_multiple_work_orders_should_return_work_order_json_for_each()
        {
            var result = await _client.GetAsync("v1/work_orders/by_references?reference=123&reference=456");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrders = JsonConvert.DeserializeObject<List<UHWorkOrder>>(jsonResult);

            Assert.Equal(2, workOrders.ToArray().Length);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task when_retrieving_a_missing_work_order_should_return_an_error()
        {
            var result = await _client.GetAsync("v1/work_orders/by_references?reference=MISSING");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task when_retrieving_multiple_work_orders_and_one_is_missing_should_return_an_error()
        {
            var result = await _client.GetAsync("v1/work_orders/by_references?reference=1&reference=MISSING");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
        #endregion

        #region GET All work orders by property reference tests
        [Fact]
        public async Task when_making_a_request_without_a_property_reference_returns_a_400()
        {
            var result = await _client.GetAsync("v1/work_orders");

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task when_making_a_valid_request_returns_a_200_result_with_a_list_of_work_orders()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678");
            var jsonresult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<List<UHWorkOrderBase>>(jsonresult).ToList();

            Assert.IsType<List<UHWorkOrderBase>>(workOrder);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task when_making_a_valid_query_with_multiple_property_references_returns_a_200_result_with_a_list_of_work_orders()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&propertyreference=56781234");
            var jsonresult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<List<UHWorkOrderBase>>(jsonresult).ToList();

            Assert.IsType<List<UHWorkOrderBase>>(workOrder);
        }

        [Fact]
        public async Task when_one_property_is_missing_still_returns_a_200_result_with_a_list_of_work_orders()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&propertyreference=0");
            var jsonresult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<List<UHWorkOrderBase>>(jsonresult).ToList();

            Assert.IsType<List<UHWorkOrderBase>>(workOrder);
        }

        [Fact]
        public async Task returns_an_empty_list_with_status_200_when_no_workorders_are_found()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=0");
            var jsonResult = await result.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("[]", jsonResult);
        }

		[Fact]
        public async Task returns_an_empty_list_with_status_200_when_no_properties_are_found()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345");
            var jsonResult = await result.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("[]", jsonResult);
        }

        [Fact]
        public async Task returns_a_200_response_when_valid_since_parameter_is_passed()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&since=01-01-2018");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task returns_a_200_response_when_valid_until_parameter_is_passed()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&until=01-01-2018");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task returns_a_400_response_when_invalid_since_parameter_is_passed()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&since=2018a-01-01");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task returns_a_400_response_when_invalid_until_parameter_is_passed()
        {
            var result = await _client.GetAsync("v1/work_orders?propertyreference=12345678&until=2018a-01-01");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }


        #endregion

        #region GET Work order notes test
        [Fact]
        public async Task return_a_200_result_with_notes_json_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/work_orders/12345678/notes");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var notes = JsonConvert.DeserializeObject<List<NotesEntity>>(jsonResult);

            Assert.IsType<List<NotesEntity>>(notes);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_an_empty_list_when_no_notes_found_for_workorder()
        {
            var result = await _client.GetAsync("v1/work_orders/9999999999/notes");
			var jsonResult = await result.Content.ReadAsStringAsync();
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("[]", jsonResult);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

		[Fact]
        public async Task return_404_when_workorder_not_found()
        {
            var result = await _client.GetAsync("v1/work_orders/00/notes");
            var jsonResult = await result.Content.ReadAsStringAsync();
			Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
        #endregion

        #region GET work order feed tests
        [Fact]
        public async Task return_a_200_result_with_list_workOrders_json_for_valid_request_in_feed()
        {
            var result = await _client.GetAsync("v1/work_orders/feed?startId=12345678");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrders = JsonConvert.DeserializeObject<List<UHWorkOrderFeed>>(jsonResult).ToList();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.IsType<List<UHWorkOrderFeed>>(workOrders);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_200_result_with_empty_array_when_no_results()
        {
            var result = await _client.GetAsync("v1/work_orders/feed?startId=99999999");
            var jsonResult = await result.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("[]", jsonResult);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_500_result_when_exception_is_thrown_in_service()
        {
            var result = await _client.GetAsync("v1/work_orders/feed?startID=11550853");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
        #endregion
    }
}
