using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using HackneyRepairs.Entities;

namespace HackneyRepairs.Tests.Integration
{
    public class WorkOrdersIntegrationTest
    {
		private readonly TestServer _server;
        private readonly HttpClient _client;

        public WorkOrdersIntegrationTest()
        {
			_server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

		#region Get WorkOrders Tests
        [Fact]
        public async Task return_a_200_result_with_workOrder_json_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/workorders/12345678");
			var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<WorkOrderEntity>(jsonResult);

			Assert.IsType<WorkOrderEntity>(workOrder);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

		[Fact]
        public async Task return_a_404_result_for_no_workorder_matching_reference()
        {
			var result = await _client.GetAsync("v1/workorders/9999999999");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
        #endregion      
    }
}
