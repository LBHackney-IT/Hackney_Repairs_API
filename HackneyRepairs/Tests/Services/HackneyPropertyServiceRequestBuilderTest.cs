using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using Moq;
using Xunit;
using System.Text;
using HackneyRepairs.Formatters;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Services;

namespace HackneyRepairs.Tests.Services
{
    public class HackneyPropertyServiceRequestBuilderTest
    {
        [Fact]
        public void return_a_built_request_object()
        {
            IHackneyPropertyServiceRequestBuilder builder =
                new HackneyPropertyServiceRequestBuilder(new NameValueCollection(), new PostcodeFormatter());
            var request = builder.BuildListByPostCodeRequest("anypostcode");
            Assert.IsType<string>(request);
        }

        [Fact]
        public void build_list_by_postcode_request_builds_a_valid_request()
        {
            var configuration = new NameValueCollection();
            IHackneyPropertyServiceRequestBuilder builder = new HackneyPropertyServiceRequestBuilder(configuration, new PostcodeFormatter());
            var request = builder.BuildListByPostCodeRequest("N16 8RE");
            Assert.Equal("N16 8RE", request);
        }

        [Fact]
        public void build_by_property_ref_request_builds_a_valid_request()
        {
            var configuration = new NameValueCollection
            {
                { "UHUsername", "uhuser" },
                { "UHPassword", "uhpassword" },
                { "UHSourceSystem", "sourcesystem" }
            };
            IHackneyPropertyServiceRequestBuilder builder = new HackneyPropertyServiceRequestBuilder(configuration, new PostcodeFormatter());
            var request = builder.BuildByPropertyRefRequest("43453543");
            Assert.Equal("43453543", request.PropertyReference);
            Assert.Equal("uhuser", request.DirectUser.UserName);
            Assert.Equal("uhpassword", request.DirectUser.UserPassword);
            Assert.Equal("sourcesystem", request.SourceSystem);
        }
    }
}
