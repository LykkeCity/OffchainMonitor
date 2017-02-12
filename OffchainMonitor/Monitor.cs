using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitor
{
    public class Monitor
    {
        public static void Start()
        {
            using (CommitmentContext context = new CommitmentContext())
            {
                var sampleCommitment = (from c in context.Commiments
                                        select c).FirstOrDefault();

                if(sampleCommitment != null)
                {

                }
                else
                {
                    throw new NotSupportedException
                        ("At least one commitment should exist for the client.");
                }
            }
        }
    }
}
