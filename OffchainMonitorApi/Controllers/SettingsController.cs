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
        [HttpGet("SetSettingsValue")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SetSettingsValue([FromQuery]string key, [FromQuery]string value)
        {
            if(string.IsNullOrEmpty(key))
            {
                return BadRequest("Key should not be null or empty.");
            }

            if(string.IsNullOrEmpty(value))
            {
                return BadRequest("Value should not be null or empty.");
            }

            if (key.ToLower() == "multisig")
            {
                Base58Data address = null;
                try
                {
                    address = Base58Data.GetFromBase58Data(value);
                }
                catch (Exception exp)
                {
                    throw new Exception("Some problem in parsing the provided address.", exp);
                }
                if (!(address is BitcoinAddress))
                {
                    return BadRequest(string.Format("The passed {0} should be a Bitcoin address.",
                        value));
                }
            }

            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecords = from record in context.Settings
                                     where record.Key == key.ToLower()
                                     select record;
                if (settingRecords.Count() > 0)
                {
                    context.Settings.RemoveRange(settingRecords);
                }

                context.Settings.Add(new SettingsEntity { Key = key, Value = value });

                await context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("GetSettingsValue")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSettingsValue([FromQuery]string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Key should not be null or empty.");
            }

            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecord = (from record in context.Settings where record.Key == key select record)
                    .FirstOrDefault();

                if (settingRecord == null)
                {
                    return BadRequest("Could not find a record for settings");
                }
                else
                {
                    return Ok(settingRecord.Value);
                }
            }
        }
    }
}
