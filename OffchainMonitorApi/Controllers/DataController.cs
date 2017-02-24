using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        [HttpGet("AddCommitmentPunishmentPair")]
        public IActionResult AddCommitmentPunishmentPair([FromQuery]string commitment,
            [FromQuery]string punishment)
        {
            return Ok();
        }
    }
}
