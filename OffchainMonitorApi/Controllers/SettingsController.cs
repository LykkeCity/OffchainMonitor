using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using SqlliteRepositories.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        [HttpGet("SetMultisig")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SetMultisig([FromQuery]string multisig)
        {
            Base58Data address = null;
            try
            {
                address = Base58Data.GetFromBase58Data(multisig);
            }
            catch (Exception exp)
            {
                throw new Exception("Some problem in parsing the provided address.", exp);
            }
            if (!(address is BitcoinAddress))
            {
                return BadRequest(string.Format("The passed {0} should be a Bitcoin address.",
                    multisig));
            }


            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecords = from record in context.Settings select record;
                if (settingRecords.Count() > 0)
                {
                    context.Settings.RemoveRange(settingRecords);
                }

                context.Settings.Add(new SettingsEntity { Multisig = multisig });

                await context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("GetMultisig")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMultisig()
        {
            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecord = (from record in context.Settings select record)
                    .FirstOrDefault();

                if (settingRecord == null)
                {
                    return BadRequest("Could not find a record for settings");
                }
                else
                {
                    return Ok(settingRecord.Multisig);
                }
            }
        }
    }
}
