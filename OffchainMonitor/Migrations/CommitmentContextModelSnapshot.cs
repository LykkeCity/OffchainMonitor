using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using OffchainMonitor;

namespace OffchainMonitor.Migrations
{
    [DbContext(typeof(CommitmentContext))]
    partial class CommitmentContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("OffchainMonitor.CommitmentModel+Commitment", b =>
                {
                    b.Property<int>("CommitmentId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CommitmentTransaction");

                    b.Property<string>("PunishmentTransaction");

                    b.HasKey("CommitmentId");

                    b.ToTable("Commiments");
                });
        }
    }
}
