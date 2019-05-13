using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Xml;

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
            KeyfaxData resultObject = DeserializeXML<KeyfaxData>(resultXml);

            return new HackneyKeyfaxDataResponse
            {
                FaultText = resultObject.Fault.FaultText,
                RepairCode = resultObject.Fault.Repair.RepairCode,
                RepairCodeDesc = resultObject.Fault.Repair.RepairCodeDesc,
                Priority = resultObject.Fault.Repair.Priority
            };
            //return resultObject;
        }

        private T DeserializeXML<T>(string xmlContent)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T result;
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            //using (TextReader reader = new StringReader(withOutEncoding))
            //{
            //    //XmlWriterSettings xWriterSettings = new XmlWriterSettings();
            //    //xWriterSettings.OmitXmlDeclaration = true;
            //    //XmlWriter xmlWriter = XmlWriter.Create();
            //    result = (T)serializer.Deserialize(reader);
            //}
            return result = (T)serializer.Deserialize(memStream);
        }
    }
}
