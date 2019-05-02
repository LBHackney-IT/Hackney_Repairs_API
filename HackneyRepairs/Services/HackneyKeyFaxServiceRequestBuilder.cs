using System;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using KeyFaxService;
using System.Text;

namespace HackneyRepairs.Services
{
    public class HackneyKeyFaxServiceRequestBuilder : IHackneyKeyFaxServiceRequestBuilder
    {
        //Convert to string.Format(@"""{0}"" = {1}", "yes", true);
        //To allow use of parameters
        private readonly StringBuilder _startupXml;
  
        public HackneyKeyFaxServiceRequestBuilder()
        {
            _startupXml = new StringBuilder();
            _startupXml.Append(@"<KeyfaxData test=""0""><Startup><Company>Hackney_Test</Company>");
            _startupXml.Append(@"<Mode>RD</Mode><UserName>AGILBERTSON</UserName><Password>Global</Password>");
            //_startupXml.Append(@"<Tenant>Mr A Test, 10 Station Road, Poole, BH20 8UF</Tenant>");
            _startupXml.Append(@"</Startup></KeyfaxData>");
        }

        public string StartUpXML
        {
            get { return _startupXml.ToString(); }
        }

        //public StartupRequest BuildNewStartupRequest()
        //{
        //    KeyFaxService.StartupRequest inValue = new KeyFaxService.StartupRequest();
        //    inValue.Body = new KeyFaxService.StartupRequestBody();
        //    inValue.Body.startupXml = _startupXml.ToString();
        //    return inValue;
        //}   
    }
}
