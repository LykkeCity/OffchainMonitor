using System;
using Microsoft.EntityFrameworkCore;

namespace SqlliteRepositories.Model
{
    // With help of https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    public class OffchainMonitorContext : DbContext
    {
        public DbSet<LogEntity> Log { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./OffchainMonitor.db");
        }
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
}
