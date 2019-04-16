using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HackneyRepairs.Tests.Integration
{
    public class CautionaryContactShould
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        public CautionaryContactShould()
        {
            Environment.SetEnvironmentVariable("UhwDb", "connectionString=Test");
            _server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

        #region GET Cautionary Contact By Property Reference
        [Fact]
        public async Task return_a_200_result_for_valid_request_by_reference()
        {
            var result = await _client.GetAsync("v1/cautionary_contact/?reference=52525252");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_empty_object_result_when_there_is_no_cautionary_contact_found_for_the_reference()
        {
            var result = await _client.GetAsync("v1/cautionary_contact/?reference=52525252");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":[]\"");
            Assert.Equal(json.ToString(), resultString);
        }

        [Fact]
        public async Task return_a_json_object_for_valid_reference()
        {
            var result = await _client.GetAsync("v1/cautionary_contact/?reference=52525252");
            string resultString = await result.Content.ReadAsStringAsync();
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"propertyReference\":\"52525252\",");
            json.Append("\"contactNo\":\"111111\",");
            json.Append("\"title\":\"MRS\",");
            json.Append("\"forenames\":\"BLIN\",");
            json.Append("\"surename\":\"null\",");
            json.Append("\"callerNotes\":\"Don't come here, weapons found in the property\",");
            json.Append("\"alertCode\":CX");
            json.Append("}");
            Assert.Equal(json.ToString(), resultString);
        }
        #endregion
    }
}
