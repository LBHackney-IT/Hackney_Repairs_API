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
            _startupXml.Append(@"<KeyfaxData test=""0""><Startup><Company>Hackney_41_OL2</Company><Mode>");
            _startupXml.Append(@"ROL</Mode><UserName>Ian</UserName><Password>Global</Password><Tenant>");
            _startupXml.Append(@"Mr A Test,10 Station Road, Poole, BH20 8UF</Tenant></Startup></KeyfaxData>");
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
