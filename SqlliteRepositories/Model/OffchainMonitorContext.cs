using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlliteRepositories.Model
{
    // With help of https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    public class OffchainMonitorContext : DbContext
    {
        public DbSet<LogEntity> Log { get; set; }
        public DbSet<CommitmentEntity> Commitments { get; set; }
        public DbSet<SettingsEntity> Settings { get; set; }
        public DbSet<MultisigOutputEntity> MultisigOutputs { get; set; }
        public DbSet<CommitmentMultisigOutput> CommitmentMultisigOutput { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./OffchainMonitor.db");
        }


        // ToDo: For multisig output, There is no need for Id primary key, transaction Id and 
        // output number could serve for that, [Key] attribute could be added but the
        // "dotnet ef migrations add" complains about using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Code First to ignore PluralizingTableName convention 
            // If you keep this convention then the generated tables will have pluralized names. 
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MultisigOutputEntity>()
                .HasKey(mo => new { mo.TransactionId, mo.OutputNumber });
            modelBuilder.Entity<CommitmentMultisigOutput>()
                .HasKey(co => new { co.CommitmentId, co.MultisigOutputTxId, co.Outputumber });

            modelBuilder.Entity<CommitmentMultisigOutput>()
                .HasOne(co => co.Output)
                .WithMany(o => o.CommitmentOutputs)
                .HasForeignKey(co => new { co.MultisigOutputTxId, co.Outputumber });

            modelBuilder.Entity<CommitmentMultisigOutput>()
                .HasOne(co => co.Commitment)
                .WithMany(o => o.CommitmentOutputs)
                .HasForeignKey(co => co.CommitmentId);
        }
    }

    public class CommitmentMultisigOutput
    {
        public int CommitmentId { get; set; }
        public CommitmentEntity Commitment { get; set; }
        public string MultisigOutputTxId { get; set; }
        public int Outputumber { get; set; }
        public MultisigOutputEntity Output { get; set; }
    }

    public class LogEntity
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Level { get; set; }
        public string Component { get; set; }
        public string Process { get; set; }
        public string Context { get; set; }
        public string Type { get; set; }
        public string Stack { get; set; }
        public string Msg { get; set; }
    }

    public class CommitmentEntity
    {
        public int Id
        {
            get;
            set;
        }
        public string Commitment
        {
            get;
            set;
        }

        public string Punishment
        {
            get;
            set;
        }
        public List<CommitmentMultisigOutput> CommitmentOutputs
        {
            get;
            set;
        }
    }

    public class MultisigOutputEntity
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

        public List<CommitmentMultisigOutput> CommitmentOutputs
        {
            get;
            set;
        }
    }

    public class SettingsEntity
    {
        [Key]
        public string Key
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
    }
}
