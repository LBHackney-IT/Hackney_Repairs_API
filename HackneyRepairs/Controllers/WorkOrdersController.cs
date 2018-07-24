using System;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Entities;
using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Controllers
{
	[Produces("application/json")]
    [Route("v1/workorders")]
	public class WorkOrdersController : Controller
    {
		private IHackneyWorkOrdersService _workOrdersService;
		private ILoggerAdapter<WorkOrdersActions> _loggerAdapter;

		public WorkOrdersController(ILoggerAdapter<WorkOrdersActions> loggerAdapter, IUhtRepository uhtRepository)
        {
			_loggerAdapter = loggerAdapter;
			var factory = new HackneyWorkOrdersServiceFactory();
			_workOrdersService = factory.build(uhtRepository, _loggerAdapter);
        }
        
		[HttpGet("{workOrderReference}")]
		public async Task<JsonResult> Get(string workOrderReference)
		{
			var workOrdersActions = new WorkOrdersActions(_workOrdersService, _loggerAdapter);
			WorkOrderEntity result = new WorkOrderEntity();
			try
			{
				result = await workOrdersActions.GetWorkOrderByReference(workOrderReference);
			}
			catch (MissingWorkOrderException ex)
			{
				var error = new ApiErrorMessage
				{
					developerMessage = ex.Message,
					userMessage = @"Cannot find repair."
				};
				return Json(error);
			}
			catch (UhtRepositoryException ex)
			{
				var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message,
                    userMessage = @"We had issues with connecting to the data source."
                };
                return Json(error);
			}
			var json = Json(result);
            json.StatusCode = 200;
            return json;
		}
    }
}
