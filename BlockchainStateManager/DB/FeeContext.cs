using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.DB
{
    public class FeeContext : DbContext
    {
        public DbSet<Fee> Fees { get; set; }

        public FeeContext(): base ("BlockchainStateManager")
        { 
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Fee>()
                .HasKey(f => new { f.TransactionId, f.OutputNumber });
        }
    }

    public class Fee
    {
        public string TransactionId
        {
            get;
            set;
        }

        public int OutputNumber
        {
            get;
            set;
        }

        public double Satoshi
        {
            get;
            set;
        }

        public bool Consumed
        {
            get;
            set;
        }

        public string PrivateKey
        {
            get;
            set;
        }

    }
}
