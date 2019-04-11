using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Builders;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Controllers
{
    public class CautionaryContactController : Controller
    {
        [HttpGet]
        public async Task<JsonResult> GetCautionaryContactByFirstLineOfAddress()
        {
            CautionaryContactActions actions = new CautionaryContactActions();
            var result = await actions.GetCautionaryContactByFirstLineOfAddress();
            return ResponseBuilder.Ok(result);
        }
    }
}