using System;
using System.Collections.Generic;
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
    public class NotesIntegrationTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public NotesIntegrationTests()
        {
            Environment.SetEnvironmentVariable("UhWarehouseDb", "database=Test");
            Environment.SetEnvironmentVariable("UhwDb", "database=Test");
            Environment.SetEnvironmentVariable("UhtDb", "database=Test");
            _server = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            _client = _server.CreateClient();
        }

        #region GET notes feed
        [Fact]
        public async Task return_a_200_result_with_Note_list_json_for_valid_request()
        {
            var result = await _client.GetAsync("v1/notes/feed?startId=12345678&noteTarget=uhorder");
            var jsonResult = await result.Content.ReadAsStringAsync();
            var notes = JsonConvert.DeserializeObject<List<Note>>(jsonResult);

            Assert.IsType<List<Note>>(notes);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_200_result_with_empty_json_list_when_no_notes_are_found()
        {
            var result = await _client.GetAsync("v1/notes/feed?startId=99999999&noteTarget=uhorder");
            var jsonResult = await result.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(jsonResult, "[]");
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task return_a_500_when_internal_server_error()
        {
            var result = await _client.GetAsync("v1/notes/feed?startId=11550853&noteTarget=uhorder");
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
        #endregion

        #region POST add notes
        [Fact]
        public async Task return_a_204_result_when_request_valid_and_workorder_exists()
        {
            StringBuilder postBody = new StringBuilder();
            postBody.Append("{");
            postBody.Append("\"objectReference\":\"1234567\", ");
            postBody.Append("\"userId\":\"randomUser\", ");
            postBody.Append("\"text\":\"random text\", ");
            postBody.Append("\"objectKey\":\"uhorder\", ");
            postBody.Append("\"lbhEmail\":\"bob@hackney.gov.uk\" ");
            postBody.Append("}");

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/notes", new StringContent(postBody.ToString(), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task return_a_404_result_if_objectReference_does_not_exist()
        {
            StringBuilder postBody = new StringBuilder();
            postBody.Append("{");
            postBody.Append("\"objectReference\":\"0\", ");
            postBody.Append("\"userId\":\"randomUser\", ");
            postBody.Append("\"text\":\"random text\", ");
            postBody.Append("\"objectKey\":\"uhorder\", ");
            postBody.Append("\"lbhEmail\":\"bob@hackney.gov.uk\" ");
            postBody.Append("}");

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/notes", new StringContent(postBody.ToString(), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task return_a_400_result_if_bad_formed_request()
        {
            var requestBody = JsonConvert.SerializeObject(new
            {
                invalidAttribute1 = "yes",
                invalidAttribute2 = "no"
            });

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync("v1/notes", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}