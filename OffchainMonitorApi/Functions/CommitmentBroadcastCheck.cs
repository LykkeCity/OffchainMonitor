using LkeServices.Triggers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Functions
{
    public class CommitmentBroadcastCheck
    {
        [TimerTrigger("00:00:10")]
        public async Task CheckCommitmentBroadcast()
        {
        }
    }
}
