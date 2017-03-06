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
    public class DataController : Controller
    {
        [HttpGet("AddCommitmentPunishmentPair")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCommitmentPunishmentPair([FromQuery]string commitment,
            [FromQuery]string punishment)
        {
            if (string.IsNullOrEmpty(commitment) || string.IsNullOrEmpty(punishment))
            {
                return BadRequest("Passed parameters should not be null or empty");
            }
            else
            {
                Transaction cTx = new Transaction(commitment);
                Transaction pTx = new Transaction(punishment);

                try
                {
                    using (OffchainMonitorContext context = new OffchainMonitorContext())
                    {
                        context.Commitments.Add(new CommitmentEntity
                        {
                            Commitment = commitment,
                            Punishment = punishment
                        });

                        for (int i = 0; i < cTx.Inputs.Count; i++)
                        {

                        }

                        await context.SaveChangesAsync();
                    }
                    return Ok();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }
    }
}
