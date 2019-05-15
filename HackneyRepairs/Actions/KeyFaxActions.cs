using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HackneyRepairs.Actions
{
    public class KeyFaxActions
    {
        public IHackneyKeyFaxService _keyfaxService;
        public IHackneyKeyFaxServiceRequestBuilder _requestBuilder;
        public ILoggerAdapter<KeyFaxActions> _logger;

        public KeyFaxActions(IHackneyKeyFaxService keyfaxService, IHackneyKeyFaxServiceRequestBuilder requestBuilder, ILoggerAdapter<KeyFaxActions> logger)
        {
            _keyfaxService = keyfaxService;
            _requestBuilder = requestBuilder;
            _logger = logger;
        }

        public async Task<object> GetStartUpURLAsync(string returnURL)
        {
            _logger.LogInformation($"Getting KeyFax Start up URL");
            var startupXml = _requestBuilder.GetStartUpXML(returnURL);
            var response = await _keyfaxService.GetKeyFaxLaunchURLAsync(startupXml);
            return response;
        }

        public async Task<object> GetResultsAsync(string keyfaxGUID)
        {
            string _companyCode = Environment.GetEnvironmentVariable("KFCompany");
            _logger.LogInformation($"Getting KeyFax results for GUID: {keyfaxGUID}");
            var response = await _keyfaxService.GetKeyFaxResultsAsync(_companyCode, keyfaxGUID);

            //KeyFaxService.GetResultsResponse return type
            string resultXml = response.Body.GetResultsResult.ResultXml;
            //Check if error string is empty
            string errorText = response.Body.GetResultsResult.ErrorText;
            //Keyfax error
            if (!String.IsNullOrEmpty(errorText))
                return errorText;

            //Check Keyfax Data object is missing repair object element 
            //Keyfax response not serializable if element is missing?
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(resultXml);
            XmlNodeList repair;
            repair = doc.GetElementsByTagName("Repair");
            if (repair.Count == 0)
            {
                //Read value of advice node
                repair = doc.GetElementsByTagName("AdviceCodeDesc");
                string advice = repair[0].InnerText;
                return new HackneyKeyfaxDataResponse
                {
                     RepairCodeDesc = repair[0].InnerText
                };
            }

            KeyfaxData resultObject = this.DeserializeXML<KeyfaxData>(resultXml);
            return new HackneyKeyfaxDataResponse
            {
                FaultText = resultObject.Fault.FaultText,
                RepairCode = resultObject.Fault.Repair.RepairCode,
                RepairCodeDesc = resultObject.Fault.Repair.RepairCodeDesc,
                Priority = resultObject.Fault.Repair.Priority
            };
        }

        private T DeserializeXML<T>(string xmlContent)
        {
            T result;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
            return result = (T)serializer.Deserialize(memStream);
        }
    }
}
