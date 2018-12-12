using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using HackneyRepairs.Models;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace HackneyRepairs.Tests.Integration
{
    public class RepairsShould
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RepairsShould()
        {
            Environment.SetEnvironmentVariable("UhtDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhwDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhWarehouseDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhSorSupplierMapping", "08500820,H01|20040010,H01|20040020,H01|20040060,H01|20040310,H01|20060020,H01|20060030,H01|20110010,H01|48000000,H05|PRE00001,H02");
            _server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

		#region GET Repairs by repair reference tests
        [Fact]
        public async Task return_a_200_result_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/repairs/123456");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_for_no_request_matching_reference()
        {
            var result = await _client.GetAsync("v1/repairs/123456899");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_a_500_result_when_there_is_an_internal_server_error()
        {
            var result = await _client.GetAsync("v1/repairs/ABCXYZ");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
        #endregion

        #region POST Create Repair request tests
        [Fact]
        public async Task return_200_response_for_successful_request()
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"priority\":\"N\", ");
            json.Append("\"propertyReference\":\"00000320\", ");
            json.Append("\"problemDescription\":\"tap leaking\", ");
            json.Append("\"contact\": { ");
            json.Append("\"name\": \"Al Smith\", ");
            json.Append("\"telephoneNumber\": \"07876543210\" ");
            json.Append("}");
            json.Append("}");

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/repairs", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_400_result_for_no_request_parameters_in_the_body()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/repairs", new StringContent("", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_500_result_for_invalid_requests()
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"priority\":\"N\",");
            json.Append("\"propertyReference\":\"01234568\",");
            json.Append("\"problemDescription\":\"tap leaking\",");
            json.Append("\"contact\": { ");
            json.Append("\"name\": \"Al Smith\", ");
            json.Append("\"telephoneNumber\": \"07876543210\" ");
            json.Append("}");
            json.Append("}");

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await _client.PostAsync("v1/repairs", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_json_object_for_successful_creation_request_without_orders()
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");

            json.Append("\"problemDescription\":\"tap leaking\",");
            json.Append("\"priority\":\"N\",");
            json.Append("\"propertyReference\":\"00000320\", ");
            json.Append("\"contact\": { ");
            json.Append("\"name\": \"Al Smith\", ");
            json.Append("\"telephoneNumber\": \"07876543210\" ");
            json.Append("}");
            json.Append("}");

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/repairs", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();

            StringBuilder responsejson = new StringBuilder();
            responsejson.Append("{");
            responsejson.Append("\"repairRequestReference\":\"123\",");
            responsejson.Append("\"problemDescription\":\"tap leaking\",");
            responsejson.Append("\"priority\":\"N\",");
            responsejson.Append("\"propertyReference\":\"00000320\",");
            responsejson.Append("\"contact\":");
            responsejson.Append("{");
            responsejson.Append("\"name\":\"Al Smith\",");
            responsejson.Append("\"telephoneNumber\":\"07876543210\"");
            responsejson.Append("}");
            responsejson.Append("}");

            Assert.Equal(responsejson.ToString(), responseString);
        }

        [Fact]
        public async Task return_json_object_for_successful_creation_request_with_orders()
        {
            var request = new RepairRequest
            {
                ProblemDescription = "tap leaking",
                Priority = "N",
                PropertyReference = "00000320",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "07876543210",
                    EmailAddress = "al.smith@hotmail.com",
                    CallbackTime = "8am - 12pm"
                },
                WorkOrders = new List<WorkOrder>
                {
                    new WorkOrder
                    {
                        SorCode = "20040020"
                    }
                }
            };

            StringBuilder responsejson = new StringBuilder();
            responsejson.Append("{");
            responsejson.Append("\"repairRequestReference\":\"123456\",");
            responsejson.Append("\"propertyReference\":\"00000320\",");
            responsejson.Append("\"problemDescription\":\"tap leaking\",");
            responsejson.Append("\"priority\":\"N\",");
            responsejson.Append("\"contact\":");
            responsejson.Append("{");
            responsejson.Append("\"name\":\"Al Smith\",");
            responsejson.Append("\"telephoneNumber\":\"07876543210\"");
            responsejson.Append("},");
            responsejson.Append("\"workOrders\":[");
            responsejson.Append("{");
            responsejson.Append("\"workOrderReference\":\"987654\",");
            responsejson.Append("\"sorCode\":\"20090190\",");
            responsejson.Append("\"supplierReference\":\"000000127\"");
            responsejson.Append("}]");
            responsejson.Append("}");

            var requestJson = JsonConvert.SerializeObject(request);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/repairs", new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(responsejson.ToString(), responseString);
        }

        [Fact]
        public async Task return_a_json_object_for_invalid_repair_request()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/repairs", new StringContent("", Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("[");
            json.Append("{");
            json.Append("\"developerMessage\":\"Please provide  a valid repair request\",");
            json.Append("\"userMessage\":\"Please provide  a valid repair request\"");
            json.Append("}");
            json.Append("]");
            Assert.Equal(json.ToString(), responseString);
        }
        #endregion

		#region GET all repairs for a property tests
        [Fact]
        public async Task return_a_200_result_for_valid_request_by_property_reference()
        {
			var result = await _client.GetAsync("v1/repairs/?propertyReference=12345678");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_when_property_not_found()
        {
			var result = await _client.GetAsync("v1/repairs/?propertyReference=0");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

		[Fact]
        public async Task return_an_empty_list_when_no_repairs_found()
        {
			var result = await _client.GetAsync("v1/repairs/?propertyReference=999999999");
            var jsonResult = await result.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("[]", jsonResult);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }
        #endregion
    }
}
