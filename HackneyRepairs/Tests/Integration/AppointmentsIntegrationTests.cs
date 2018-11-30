using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace HackneyRepairs.Tests.Integration
{
	public class AppointmentsIntegrationTests
	{
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public AppointmentsIntegrationTests()
		{
            Environment.SetEnvironmentVariable("UhtDb", "database=Test");
            Environment.SetEnvironmentVariable("UhwDb", "database=Test");
            Environment.SetEnvironmentVariable("DRSDb", "database=Test");
            _server = new TestServer(new WebHostBuilder()
				.UseStartup<TestStartup>());
			_client = _server.CreateClient();
		}

		#region GET available appointments for work order
		[Fact]
		public async Task return_a_200_result_for_valid_requests()
		{
			var result = await _client.GetAsync("v1/work_orders/01550854/available_appointments");
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
		}

		[Fact]
		public async Task return_a_400_result_for_empty_parameter_string()
		{
			var result = await _client.GetAsync("v1/work_orders/ /available_appointments");
			Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
		}

		[Fact]
		public async Task return_a_500_result_when_there_is_an_internal_server_error()
		{
			var result = await _client.GetAsync("v1/work_orders/01550853/available_appointments");
			Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
		}

		[Fact]
		public async Task return_a_200_result_when_there_are_no_available_appointments()
		{
			var result = await _client.GetAsync("v1/work_orders/01550751/available_appointments");
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
		}

		[Fact]
		public async Task return_an_empty_results_array_when_there_are_no_available_appointments()
		{
			var result = await _client.GetAsync("v1/work_orders/01550751/available_appointments");
			string resultString = await result.Content.ReadAsStringAsync();
			StringBuilder json = new StringBuilder();
			json.Append("{\"results\":[");
			json.Append("]}");
			Assert.Equal(json.ToString(), resultString);
		}



		[Fact]
		public async Task return_a_json_object_for_valid_requests()
		{
			var result = await _client.GetAsync("v1/work_orders/01550854/available_appointments");
			string result_string = await result.Content.ReadAsStringAsync();
			StringBuilder json = new StringBuilder();
			json.Append("{\"results\":[{");
			json.Append("\"beginDate\":\"2017-10-18T10:00:00Z\",");
			json.Append("\"endDate\":\"2017-10-18T12:00:00Z\",");
			json.Append("\"bestSlot\":true");
			json.Append("},");
			json.Append("{");
			json.Append("\"beginDate\":\"2017-10-18T12:00:00Z\",");
			json.Append("\"endDate\":\"2017-10-18T14:00:00Z\",");
			json.Append("\"bestSlot\":false");
			json.Append("},");
			json.Append("{");
			json.Append("\"beginDate\":\"2017-10-18T14:00:00Z\",");
			json.Append("\"endDate\":\"2017-10-18T16:00:00Z\",");
			json.Append("\"bestSlot\":false");
			json.Append("}]}");
			Assert.Equal(json.ToString(), result_string);
		}
		#endregion

		#region Get appointments by by work order
        [Fact]
        public async Task return_list_of_DetailedAppointment_json_object_with_200()
        {
            var result = await _client.GetAsync("v1/work_orders/01550854/appointments");
            var jsonresult = await result.Content.ReadAsStringAsync();
            var appointments = JsonConvert.DeserializeObject<List<DetailedAppointment>>(jsonresult).ToList();

            Assert.IsType<List<DetailedAppointment>>(appointments);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_404_result_for_no_Work_order_found()
        {
            var result = await _client.GetAsync("v1/work_orders/888888888/appointments");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_an_empty_list_if_no_appointments_found()
        {
            var expected = new List<DetailedAppointment>();
            var result = await _client.GetAsync("v1/work_orders/99999999/appointments");
            Assert.IsType<List<DetailedAppointment>>(expected);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }
        #endregion

		#region POST Create an book / appointment
		[Fact]
		public async Task return_a_200_result_for_valid_Post_requests()
		{
			StringBuilder json = new StringBuilder();
			json.Append("{");
			json.Append("\"beginDate\":\"2017-11-10T10:00:00Z\",");
			json.Append("\"endDate\":\"2017-11-10T12:00:00Z\" ");
			json.Append("}");

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await _client.PostAsync("v1/work_orders/01550854/appointments", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
		}

		[Fact]
		public async Task return_a_json_object_for_valid_valid_Post_requests()
		{
			StringBuilder json = new StringBuilder();
			json.Append("{");
			json.Append("\"beginDate\":\"2017-11-10T10:00:00Z\",");
			json.Append("\"endDate\":\"2017-11-10T12:00:00Z\"");
			json.Append("}");

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await _client.PostAsync("v1/work_orders/01550854/appointments", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
			string result_string = await response.Content.ReadAsStringAsync();
			Assert.Equal(json.ToString(), result_string);
		}

		[Fact]
		public async Task return_a_400_result_for_empty_request_when_booking_appointment()
		{
			StringBuilder json = new StringBuilder();
			json.Append("{}");

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await _client.PostAsync("v1/work_orders/01550854/appointments", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task return_a_500_result_when_there_is_an_internal_server_error_when_booking_appointment()
		{
			StringBuilder json = new StringBuilder();
			json.Append("{");
			json.Append("\"beginDate\":\"2017-11-10T10:00:00Z\", ");
			json.Append("\"endDate\":\"2017-11-10T12:00:00Z\", ");
			json.Append("}");

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await _client.PostAsync("v1/work_orders/01550853/appointments", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
			Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
		}
        #endregion
        
		#region Get latest appointment by work order
        [Fact]
        public async Task return_a_DetailedAppointment_json_object_with_200()
        {
            var result = await _client.GetAsync("v1/work_orders/01550854/appointments/latest");
            var jsonresult = await result.Content.ReadAsStringAsync();
            var appointments = JsonConvert.DeserializeObject<DetailedAppointment>(jsonresult);

            Assert.IsType<DetailedAppointment>(appointments);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }
        
        [Fact]
        public async Task return_a_404_result_when_no_Work_order_found()
        {
            var result = await _client.GetAsync("v1/work_orders/888888888/appointments/latest");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task return_an_empty_list_if_no_appointment_found()
        {
            var expected = new DetailedAppointment();
            var result = await _client.GetAsync("v1/work_orders/99999999/appointments/latest");
            Assert.IsType<DetailedAppointment>(expected);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }
        #endregion
	}
}
