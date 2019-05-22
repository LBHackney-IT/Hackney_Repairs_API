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

            //Keyfax error
            if (!string.IsNullOrEmpty(response.Body.GetResultsResult.ErrorText))
                return response.Body.GetResultsResult.ErrorText;

            //Check Keyfax Data object is missing repair object element 
            //Keyfax response not can not deserialize if element is missing?
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(resultXml);
            XmlNodeList keyfaxDataNode;
            keyfaxDataNode = doc.GetElementsByTagName("Repair");
            //if (keyfaxDataNode.Count == 0)
            //{
            //    //Read value of advice node
            //    keyfaxDataNode = doc.GetElementsByTagName("AdviceCodeDesc");
            //    string advice = keyfaxDataNode[0].InnerText;
            //    return new HackneyKeyfaxDataResponse
            //    {
            //        RepairCodeDesc = keyfaxDataNode[0].InnerText
            //    };
            //}
            HackneyKeyfaxDataResponse resultObject = new HackneyKeyfaxDataResponse();

            //Read each`value required for response from XML
            //keyfaxDataNode = doc.GetElementsByTagName("FaultText");
            //resultObject.FaultText = keyfaxDataNode.Count > 0 ? keyfaxDataNode[0].InnerText : null;
            //keyfaxDataNode = doc.GetElementsByTagName("RepairCode");
            //resultObject.RepairCode = keyfaxDataNode.Count > 0 ? keyfaxDataNode[0].InnerText : null;
            //keyfaxDataNode = doc.GetElementsByTagName("RepairCodeDesc");
            //resultObject.RepairCodeDesc = keyfaxDataNode.Count > 0 ? keyfaxDataNode[0].InnerText : null;
            //keyfaxDataNode = doc.GetElementsByTagName("Priority");
            //resultObject.Priority = keyfaxDataNode.Count > 0 ? keyfaxDataNode[0].InnerText : null;
            KeyfaxData resultObject2 = this.DeserializeXML<KeyfaxData>(resultXml);
            return new HackneyKeyfaxDataResponse
            {
                FaultText = resultObject2.Fault.FaultText,
                RepairCode = resultObject2.Fault.Repair.RepairCode,
                RepairCodeDesc = resultObject2.Fault.Repair.RepairCodeDesc,
                Priority = resultObject2.Fault.Repair.Priority
            };

            //return resultObject2;
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