using System.Threading.Tasks;
using System;
using Xunit;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using Newtonsoft.Json;
using HackneyRepairs.Models;
using System.Collections.Generic;

namespace HackneyRepairs.Tests
{
    public class PropertiesShould
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        public PropertiesShould()
        {
            Environment.SetEnvironmentVariable("UhtDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhwDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhWarehouseDb", "connectionString=Test");
            Environment.SetEnvironmentVariable("UhSorSupplierMapping", "08500820,H01|20040010,H01|20040020,H01|20040060,H01|20040310,H01|20060020,H01|20060030,H01|20110010,H01|48000000,H05|PRE00001,H02");
            _server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

        #region GET Properties by postcode tests
        [Fact]
        public async Task return_a_200_result_for_valid_requests()
        {
            var result = await _client.GetAsync("v1/properties?postcode=E8+1DT");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_400_result_for_no_parameters()
        {
            var result = await _client.GetAsync("v1/properties");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_400_result_for_empty_parameter_string()
        {
            var result = await _client.GetAsync("v1/properties?postcode=");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_400_result_for_invalid_postcode()
        {
            var result = await _client.GetAsync("v1/properties?postcode=NR");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_500_result_when_there_is_an_internal_server_error_for_properties_by_postcode()
        {
            var result = await _client.GetAsync("v1/properties?postcode=E8+2LN");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_requests()
        {
            var result = await _client.GetAsync("v1/properties?postcode=E8 1DT");
            string result_string = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":");
            json.Append("[");
            json.Append("{");
            json.Append("\"address\":\"Back Office, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"1/525252525\"");
            json.Append("},");
            json.Append("{");
            json.Append("\"address\":\"Meeting room, Maurice Bishop House, 17 Reading Lane\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"6/32453245\"");
            json.Append("}");
            json.Append("]");
            json.Append("}");
            Assert.Equal(json.ToString(), result_string);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_requests_with_lowercase_postcode()
        {
            var result = await _client.GetAsync("v1/properties?postcode=e8 1dt");
            string result_string = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":");
            json.Append("[");
            json.Append("{");
            json.Append("\"address\":\"Back Office, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"1/525252525\"");
            json.Append("},");
            json.Append("{");
            json.Append("\"address\":\"Meeting room, Maurice Bishop House, 17 Reading Lane\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"6/32453245\"");
            json.Append("}");
            json.Append("]");
            json.Append("}");
            Assert.Equal(json.ToString(), result_string);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_requests_with_lowercase_and_without_space_postcode()
        {
            var result = await _client.GetAsync("v1/properties?postcode=e81dt");
            string result_string = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":");
            json.Append("[");
            json.Append("{");
            json.Append("\"address\":\"Back Office, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"1/525252525\"");
            json.Append("},");
            json.Append("{");
            json.Append("\"address\":\"Meeting room, Maurice Bishop House, 17 Reading Lane\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"6/32453245\"");
            json.Append("}");
            json.Append("]");
            json.Append("}");
            Assert.Equal(json.ToString(), result_string);
        }

        [Fact]
        public async Task return_a_json_object_for_invalid_requests()
        {
            var result = await _client.GetAsync("v1/properties?postcode=E8");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("[");
            json.Append("{");
            json.Append("\"developerMessage\":\"Invalid parameter - postcode\",");
            json.Append("\"userMessage\":\"Please provide a valid post code\"");
            json.Append("}");
            json.Append("]");
            Assert.Equal(json.ToString(), resultString);
        }
        #endregion

        #region GET Property details by reference tests
        [Fact]
        public async Task return_a_200_result_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_500_result_when_there_is_an_internal_server_error()
        {
            var result = await _client.GetAsync("v1/properties/5252");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_a_404_result_when_there_is_no_property_found_for_the_reference()
        {
            var result = await _client.GetAsync("v1/properties/42525252512");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"address\":\"Back Office, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"52525252\",");
            json.Append("\"maintainable\":true");
            json.Append("}");
            Assert.Equal(json.ToString(), resultString);
        }

        [Fact]
        public async Task return_a_error_reponse_object_when_there_is_no_property_found_for_the_reference()
        {
            var result = await _client.GetAsync("v1/properties/42525252512");
            string resultString = await result.Content.ReadAsStringAsync();
            Assert.Contains("developerMessage", resultString);
        }
        #endregion

        #region GET Property block details by property reference tests
        [Fact]
        public async Task return_a_200_result_for_valid_block_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252/block");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_for_block_request_when_property_reference_is_not_found()
        {
            var result = await _client.GetAsync("v1/properties/5252525255/block");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_a_500_result_for_block_request_if_something_goes_wrong()
        {
            var result = await _client.GetAsync("v1/properties/5252/block");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_a_json_object_if_something_goes_wrong_with_a_property_block_request()
        {
            var result = await _client.GetAsync("v1/properties/5252/block");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("[");
            json.Append("{");
            json.Append("\"developerMessage\":\"API Internal Error\",");
            json.Append("\"userMessage\":\"API Internal Error\"");
            json.Append("}");
            json.Append("]");
            Assert.Equal(json.ToString(), resultString);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_block_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252/block");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"address\":\"Back Office Block, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"525252527\",");
            json.Append("\"maintainable\":true");
            json.Append("}");
            Assert.Equal(json.ToString(), resultString);
        }
        #endregion

		#region GET Property's block related work orders tests
        [Fact]
        public async Task return_a_200_result_for_valid_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/00074866/block/work_orders?trade=Plumbing");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_for_request_when_property_reference_is_not_found()
        {
            var result = await _client.GetAsync("v1/properties/99999999/block/work_orders?trade=Plumbing");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_a_403_result_for_request_when_trade_is_not_provided()
        {
            var result = await _client.GetAsync("v1/properties/00078556/block/work_orders?trade=Plumbing");
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task return_a_403_result_for_request_when_estate_reference_provided()
        {
            var result = await _client.GetAsync("v1/properties/00074866/block/work_orders");
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task return_a_500_result_for_request_if_something_goes_wrong()
        {
            var result = await _client.GetAsync("v1/properties/66666666/block/work_orders?trade=Plumbing");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_a_200_result_for_request_when_since_parameter_has_correct_format()
        {
            var result = await _client.GetAsync("v1/properties/00079999/block/work_orders?trade=Plumbing&since=01-01-2000");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task return_a_200_result_for_request_when_until_parameter_has_correct_format()
        {
            var result = await _client.GetAsync("v1/properties/00079999/block/work_orders?trade=Plumbing&until=01-01-2000");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task return_a_400_result_for_request_when_since_parameter_has_incorrect_format()
        {
            var result = await _client.GetAsync("v1/properties/66666666/block/work_orders?trade=Plumbing&since=01-011-2000");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_400_result_for_request_when_until_parameter_has_incorrect_format()
        {
            var result = await _client.GetAsync("v1/properties/66666666/block/work_orders?trade=Plumbing&until=011-01-2000");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task return_a_json_object_if_something_goes_wrong_with_a_property_work_orders_request()
        {
            var result = await _client.GetAsync("v1/properties/66666666/block/work_orders?trade=Plumbing");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("[");
            json.Append("{");
            json.Append("\"developerMessage\":\"API Internal Error\",");
            json.Append("\"userMessage\":\"API Internal Error\"");
            json.Append("}");
            json.Append("]");
            Assert.Equal(json.ToString(), resultString);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/00079999/block/work_orders?trade=Plumbing");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var workOrder = JsonConvert.DeserializeObject<List<UHWorkOrder>>(jsonResult);

            Assert.IsType<List<UHWorkOrder>>(workOrder);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        #endregion

        #region GET Property estate details by property reference tests
        [Fact]
        public async Task return_a_200_result_for_valid_estate_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252/estate");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_for_estate_request_when_property_reference_is_not_found()
        {
            var result = await _client.GetAsync("v1/properties/5252525255/estate");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_a_500_result_for_estate_request_if_something_goes_wrong()
        {
            var result = await _client.GetAsync("v1/properties/5252/estate");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task return_a_json_object_if_something_goes_wrong_with_a_property_estate_request()
        {
            var result = await _client.GetAsync("v1/properties/5252/estate");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("[");
            json.Append("{");
            json.Append("\"developerMessage\":\"API Internal Error\",");
            json.Append("\"userMessage\":\"API Internal Error\"");
            json.Append("}");
            json.Append("]");
            Assert.Equal(json.ToString(), resultString);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_estate_request_by_property_reference()
        {
            var result = await _client.GetAsync("v1/properties/52525252/estate");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"address\":\"Back Office Estate, Robert House, 6 - 15 Florfield Road\",");
            json.Append("\"postcode\":\"E8 1DT\",");
            json.Append("\"propertyReference\":\"525252527\",");
            json.Append("\"maintainable\":true");
            json.Append("}");
            Assert.Equal(json.ToString(), resultString);
        }
        #endregion
	}
}