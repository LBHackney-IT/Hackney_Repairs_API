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
        //Convert to string.Format(@"""{0}"" = {1}", "yes", true);
        //To allow use of parameters
        private StringBuilder _startupXml;
        private NameValueCollection _configuration;

        public HackneyKeyFaxServiceRequestBuilder(NameValueCollection configuration)
        {
            _configuration = configuration;
        }

        public string GetStartUpXML()
        {
            _startupXml = new StringBuilder();
            _startupXml.Append(string.Format(@"<KeyfaxData test=""0""><Startup>"));
            _startupXml.Append(string.Format(@"<Mode>{0}</Mode>", _configuration.Get("KFMode")));
            _startupXml.Append(string.Format(@"<Company>{0}</Company>", _configuration.Get("KFCompany")));
            _startupXml.Append(string.Format(@"<UserName>{0}</UserName>", _configuration.Get("KFUsername")));
            _startupXml.Append(string.Format(@"<Password>{0}</Password>", _configuration.Get("KFPassword")));
            _startupXml.Append(string.Format(@"<ReturnURL>{0}</ReturnURL>", _configuration.Get("KFReturnUrl")));
            _startupXml.Append(string.Format(@"</Startup></KeyfaxData>")); 
            return _startupXml.ToString();            
        }
    }
}
