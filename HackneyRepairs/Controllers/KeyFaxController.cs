using System;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Builders;
using System.Threading.Tasks;
using HackneyRepairs.Factories;
using HackneyRepairs.Services;
using System.Xml;
using Newtonsoft.Json;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/keyfax")]
    public class KeyFaxController : Controller
    {
        private ILoggerAdapter<KeyFaxActions> _loggerAdapter;
        private readonly IExceptionLogger _exceptionLogger;
        private IHackneyKeyFaxService _keyfaxService;
        private IHackneyKeyFaxServiceRequestBuilder _requestBuilder;

        public KeyFaxController(ILoggerAdapter<KeyFaxActions> loggerAdapter, IExceptionLogger exceptionLogger)
        {
            var factory = new HackneyKeyFaxServiceFactory();
            _requestBuilder = new HackneyKeyFaxServiceRequestBuilder();
            _keyfaxService = factory.build(loggerAdapter);
            _loggerAdapter = loggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        [HttpGet("get_startup_url")]
        public async Task<JsonResult> GetKeyFaxStartUpURL()
        {
            try
            {
                KeyFaxActions actions = new KeyFaxActions(_keyfaxService, _requestBuilder, _loggerAdapter);
                
                //Keyfax return type is KeyFaxService.StartupResponse
                var result = await actions.GetStartUpURL();

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(result.ToString());
                //string json = JsonConvert.SerializeXmlNode(doc);
                //return new JsonResult(json)
                //{
                //    StatusCode = 200,
                //    ContentType = "application/json"
                //};
                return ResponseBuilder.Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        //public async Task<JsonResult> GetKeyFaxResults(string kfGUID)
        //{

        //    KeyFaxActions actions = new KeyFaxActions(_repairsService, _requestBuilder, _loggerAdapter);
        //    var result = await actions.GetStartUpURL(request);
        //    return ResponseBuilder.Ok(result);

        //}
    }
}