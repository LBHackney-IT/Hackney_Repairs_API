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
            KeyfaxData resultObject = this.DeserializeXML<KeyfaxData>(resultXml);
            if (resultObject.Fault.Repair == null)
            {
                return new HackneyKeyfaxDataResponse
                {
                    RepairCodeDesc = resultObject.Fault.Advice != null ? resultObject.Fault.Advice.AdviceCodeDesc : "No SOR code returned from Keyfax"
                };
            }

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