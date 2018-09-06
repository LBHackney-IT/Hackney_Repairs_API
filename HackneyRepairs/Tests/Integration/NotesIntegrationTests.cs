﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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

        #region GetNoteFeed endpoint
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
    }
}