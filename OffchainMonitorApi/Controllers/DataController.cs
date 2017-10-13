using Core.Repositories.Settings;
using LkeServices.Transactions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using OffchainMonitorApi.Models;
using SqlliteRepositories.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        IBitcoinTransactionService bitcoinTransactionService;
        ISettingsRepository settingsRepository;

        public DataController(ISettingsRepository _settingsRepository,
            IBitcoinTransactionService _bitcoinTransactionService) : base()
        {
            bitcoinTransactionService = _bitcoinTransactionService;
            settingsRepository = _settingsRepository;
        }

        [HttpGet("GetVersion")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            return Ok(version);
        }

        /// <summary>
        /// Adds monitoring of blockchain for the passed commitment, if detected on blockchain, it would be punished by the passed punishment.
        /// </summary>
        /// <param name="commitment">The commitment to monitor on blockchain, it is probably an unsigned segwit transaction which is signed only by the party requesting monitor. Actually what is monitored on blockchain is the TxId of this segwit transaction which is independent of signatures.</param>
        /// <param name="punishment">The punishment to broadcast, when the commitment was detected on blockchain.</param>
        /// <param name="overwrite">If the TxId of the passed commitment is already present in DB, this flag specifies whether to overwrite it or not.</param>
        /// <returns></returns>
        [HttpGet("AddCommitmentPunishmentPair")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCommitmentPunishmentPair([FromQuery]string commitment,
            [FromQuery]string punishment, [FromQuery]bool overwrite = false)
        {
            if (string.IsNullOrEmpty(commitment) || string.IsNullOrEmpty(punishment))
            {
                return BadRequest("Passed parameters should not be null or empty");
            }
            else
            {
                try
                {
                    Transaction cTx = new Transaction(commitment);
                    Transaction pTx = new Transaction(punishment);

                    using (OffchainMonitorContext context = new OffchainMonitorContext())
                    {
                        var matchedCommitment = (from c in context.Commitments
                                                 where c.CommitmentTxId == cTx.GetHash().ToString()
                                                 select c).FirstOrDefault();

                        if (matchedCommitment != null)
                        {
                            if (overwrite == false)
                            {
                                return BadRequest("To overwrite an existing punishment for existing commitment id, the overwrite flag should bet set to true");
                            }
                            else
                            {
                                matchedCommitment.Commitment = commitment;
                                matchedCommitment.Punishment = punishment;
                            }
                        }
                        else
                        {
                            var newlyAddedCommitment = new CommitmentEntity
                            {
                                Commitment = commitment,
                                CommitmentTxId = cTx.GetHash().ToString(),
                                Punishment = punishment
                            };

                            await context.Commitments.AddAsync(newlyAddedCommitment);
                        }

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }

                return Ok();
            }
        }

        /// <summary>
        /// Returns the commitments which are being monitored.
        /// </summary>
        /// <param name="from">The start number of records to return.</param>
        /// <param name="to">The end number of records to return (non-iclusive). -1 means to return all records to the end.</param>
        /// <returns>A json array from monitored commitments, including the punishments and whether the commitment has been punished or not.</returns>
        [HttpGet("ListMonitoredCommitments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListMonitoredCommitments([FromQuery]int from, [FromQuery]int to)
        {
            if (from < 0)
            {
                return BadRequest("From should not be negative.");
            }

            if (to != -1 && to <= from)
            {
                return BadRequest("To should be bigger than from");
            }
            try
            {
                using (OffchainMonitorContext context = new OffchainMonitorContext())
                {
                    var records = (from record in context.Commitments
                                  select new { CommtmentTxId = record.CommitmentTxId, Commitment = record.Commitment, Punishment = record.Punishment, Punished = record.Punished }).Skip(from);

                    if(to != -1)
                    {
                        records = records.Take(to - from);
                    }

                    if (records.Count() > 0)
                    {
                        return Json(records.ToList());
                    }
                    else
                    {
                        return BadRequest("No records to return.");
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        // Until SegWit acivation since punishment requires commitment id, and it is not available until hub broadcasts it, this is not usable
        // After segwit activation commitment id will be available independent of its signing
        [HttpGet("AddCommitmentPunishmentPairOldDesign")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Obsolete]
        public async Task<IActionResult> AddCommitmentPunishmentPairOldDesign([FromQuery]string commitment,
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
                        var newlyAddedCommitment = new CommitmentEntity
                        {
                            Commitment = commitment,
                            Punishment = punishment
                        };

                        bool found = false;
                        var multisig = await settingsRepository.Get<string>("multisig");
                        var netwok = await Helper.GetNetwork(settingsRepository);
                        for (int i = 0; i < cTx.Inputs.Count; i++)
                        {
                            var prevHash = cTx.Inputs[i].PrevOut.Hash.ToString();
                            var prevTx = await bitcoinTransactionService.GetTransaction(prevHash);
                            var prevOut = cTx.Inputs[i].PrevOut;
                            if (prevTx.Outputs[prevOut.N].ScriptPubKey.GetDestinationAddress(netwok).ToString()
                                == multisig)
                            {
                                found = true;

                                var existingMultisigOutput = (from o in context.MultisigOutputs
                                                              where o.TransactionId == prevOut.Hash.ToString() && o.OutputNumber == prevOut.N
                                                              select o).FirstOrDefault();
                                if (existingMultisigOutput == null)
                                {
                                    var newMultisigOutput = new MultisigOutputEntity();
                                    newMultisigOutput.TransactionId = prevOut.Hash.ToString();
                                    newMultisigOutput.OutputNumber = (int)prevOut.N;
                                    newMultisigOutput.LastSeen = DateTime.UtcNow;
                                    await context.MultisigOutputs.AddAsync(newMultisigOutput);

                                    existingMultisigOutput = newMultisigOutput;
                                }

                                existingMultisigOutput.LastSeen = DateTime.UtcNow;
                                existingMultisigOutput.Commitments.Add(newlyAddedCommitment);

                                newlyAddedCommitment.CommitmentOutput = existingMultisigOutput;
                                await context.Commitments.AddAsync(newlyAddedCommitment);
                            }
                        }

                        if (!found)
                        {
                            return BadRequest(string.Format("The provide transaction does not pay to multisig:{0}", multisig));
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

        [HttpPost("AddFee")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [Obsolete]
        public async Task<IActionResult> AddFee([FromBody]AddFeeRequest request)
        {
            var checkResult = request.CheckModel();
            if (checkResult != null)
            {
                return StatusCode(500, checkResult);
            }
            return Ok();
        }
    }
}
