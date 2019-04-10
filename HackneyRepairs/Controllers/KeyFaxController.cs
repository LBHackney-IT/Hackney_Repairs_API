using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Builders;
using System.Threading.Tasks;
using HackneyRepairs.Factories;

namespace HackneyRepairs.Controllers
{
    public class KeyFaxController : Controller
    {
        private ILoggerAdapter<KeyFaxActions> _loggerAdapter;
        private readonly IExceptionLogger _exceptionLogger;
        private IHackneyKeyFaxService _keyfaxService;

        public KeyFaxController(ILoggerAdapter<KeyFaxActions> loggerAdapter, IExceptionLogger exceptionLogger)
        {
            var factory = new HackneyKeyFaxServiceFactory();
            _keyfaxService = factory.build(loggerAdapter);
            _loggerAdapter = loggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        //public async Task<JsonResult> GetKeyFaxStartUpURL()
        //{
        //    KeyFaxActions actions = new KeyFaxActions(_repairsService, _requestBuilder, _loggerAdapter);
        //    var result = await actions.GetStartUpURL(request);
        //    return ResponseBuilder.Ok(result);
        //}

        //public async Task<JsonResult> GetKeyFaxResults(string kfGUID)
        //{

        //    KeyFaxActions actions = new KeyFaxActions(_repairsService, _requestBuilder, _loggerAdapter);
        //    var result = await actions.GetStartUpURL(request);
        //    return ResponseBuilder.Ok(result);

        //}
    }
}