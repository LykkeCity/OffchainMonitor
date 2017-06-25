using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.DB
{
    public class BlockchainStateManagerContext : DbContext
    {
        public DbSet<Fee> Fees { get; set; }
        public DbSet<ChannelCoin> ChannelCoins { get; set; }
        public DbSet<OffchainChannel> OffchainChannels { get; set; }

        public BlockchainStateManagerContext() : base("BlockchainStateManager")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChannelCoin>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<OffchainChannel>()
                .HasKey(o => new { o.ChannelId });

            modelBuilder.Entity<Fee>()
                .HasKey(f => new { f.TransactionId, f.OutputNumber });
        }
    }

    public partial class ChannelCoin
    {
        public long Id
        { get; set; }
        public string TransactionId { get; set; }
        public int OutputNumber { get; set; }
        public long ReservedForChannel { get; set; }
        public Nullable<bool> ReservationFinalized { get; set; }
        public Nullable<bool> ReservationTimedout { get; set; }
        public System.DateTime ReservationCreationDate { get; set; }
        public string ReservedForMultisig { get; set; }
        public byte[] Version { get; set; }
        public System.DateTime ReservationEndDate { get; set; }

        public OffchainChannel OffchainChannel { get; set; }
    }

    public partial class OffchainChannel
    {
        public long ChannelId { get; set; }
        public Nullable<long> ReplacedBy { get; set; }
        public string unsignedTransactionHash { get; set; }
        public byte[] Version { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChannelCoin> ChannelCoins { get; set; }
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

        public ulong Satoshi
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

        public string Script
        {
            get;
            set;
        }
    }
}
