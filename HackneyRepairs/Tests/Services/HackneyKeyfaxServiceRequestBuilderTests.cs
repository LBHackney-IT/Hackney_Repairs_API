using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Services;
using RepairsService;
using Xunit;

namespace HackneyRepairs.Tests.Services
{
    public class HackneyKeyFaxServiceRequestBuilderTests
    {
        private NameValueCollection configuration = new NameValueCollection
            {
                { "KFMode", "RD" },
                { "KFCompany", "Hackney_Test" },
                { "KFUsername", "Ian" },
                { "KFPassword", "Global" }
            };

        [Fact]
        public void return_a_built_request_object()
        {
            var builder = new HackneyKeyFaxServiceRequestBuilder(new NameValueCollection());
            var request = builder.GetStartUpXML();
            Assert.IsType<string>(request);
        }

        [Fact]
        public void build_new_keyfax_request_builds_a_valid_request()
        {
            var builder = new HackneyKeyFaxServiceRequestBuilder(configuration);
            var request = builder.GetStartUpXML();
            Assert.Contains("Mode", request);
            Assert.Contains("Company", request);
            Assert.Contains("Username", request);
        }        
    }
}
