using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitor
{
    // With the help of https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    public class CommitmentContext : DbContext
    {
        public DbSet<Commitment> Commiments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./Commitment.db");
        }
    }

    public class Commitment
    {
        public int CommitmentId { get; set; }
        public int CommitmentTransaction { get; set; }
        public string PunishmentTransaction { get; set; }
    }
}
