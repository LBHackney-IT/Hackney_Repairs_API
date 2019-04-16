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
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":[]\"");

            StringBuilder expectedString = new StringBuilder();
            json.Append("{");
            json.Append("\"results\":[]\"");

            Assert.Equal(json.ToString(), expectedString.ToString());
        }
        #endregion
    }
}
