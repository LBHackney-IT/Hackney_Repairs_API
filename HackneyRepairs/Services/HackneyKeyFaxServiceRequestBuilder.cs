using System;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using KeyFaxService;
using System.Text;
using System.Collections.Specialized;

namespace HackneyRepairs.Services
{
    public class HackneyKeyFaxServiceRequestBuilder : IHackneyKeyFaxServiceRequestBuilder
    {
        private StringBuilder _startupXml;
        private NameValueCollection _configuration;

        public HackneyKeyFaxServiceRequestBuilder(NameValueCollection configuration)
        {
            _configuration = configuration;
        }

        public string GetStartUpXML(string returnURL)
        {
            _startupXml = new StringBuilder();
            _startupXml.Append(@"<KeyfaxData test=""0""><Startup>");
            _startupXml.Append(@"<OriginatingSystem>RepairsHub</OriginatingSystem>");
            _startupXml.AppendFormat(@"<Mode>{0}</Mode>", _configuration.Get("KFMode"));
            _startupXml.AppendFormat(@"<Company>{0}</Company>", _configuration.Get("KFCompany"));
            _startupXml.AppendFormat(@"<UserName>{0}</UserName>", _configuration.Get("KFUsername"));
            _startupXml.AppendFormat(@"<Password>{0}</Password>", _configuration.Get("KFPassword"));
            _startupXml.AppendFormat(@"<ReturnURL>{0}</ReturnURL>", returnURL);
            _startupXml.Append(@"</Startup></KeyfaxData>"); 
            return _startupXml.ToString();            
        }
    }
}
