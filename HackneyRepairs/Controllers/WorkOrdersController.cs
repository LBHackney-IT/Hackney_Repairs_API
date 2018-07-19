using System;
using System.Threading.Tasks;
using HackneyRepairs.Entities;
using HackneyRepairs.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Controllers
{
	[Produces("application/json")]
    [Route("v1/workorders")]
	public class WorkOrdersController : Controller
    {
        public WorkOrdersController()
        {
        }

		[HttpGet("{workOrderReference}")]
		public async Task<JsonResult> Get(string workOrderReference)
		{
			var json = Json(new WorkOrderEntity());
			if (workOrderReference == "99999999")
			{
				
				json.StatusCode = 404;
			}
			else
			{
				
                json.StatusCode = 200;
			}
            return json;
		}
    }
}
