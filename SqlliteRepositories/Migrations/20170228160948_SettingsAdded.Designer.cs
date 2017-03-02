using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SqlliteRepositories.Model;

namespace SqlliteRepositories.Migrations
{
    [DbContext(typeof(OffchainMonitorContext))]
    [Migration("20170228160948_SettingsAdded")]
    partial class SettingsAdded
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("SqlliteRepositories.Model.CommitmentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Commitment");

                    b.Property<string>("Punishment");

                    b.HasKey("Id");

                    b.ToTable("Commitments");
                });

            modelBuilder.Entity("SqlliteRepositories.Model.LogEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Component");

                    b.Property<string>("Context");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Level");

                    b.Property<string>("Msg");

                    b.Property<string>("Process");

                    b.Property<string>("Stack");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("SqlliteRepositories.Model.SettingsEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Multisig");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });
        }
    }
}
