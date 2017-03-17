using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SqlliteRepositories.Model;

namespace SqlliteRepositories.Migrations
{
    [DbContext(typeof(OffchainMonitorContext))]
    [Migration("20170307132901_InitialMigration")]
    partial class InitialMigration
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

            modelBuilder.Entity("SqlliteRepositories.Model.CommitmentMultisigOutput", b =>
                {
                    b.Property<int>("CommitmentId");

                    b.Property<string>("MultisigOutputTxId");

                    b.Property<int>("Outputumber");

                    b.HasKey("CommitmentId", "MultisigOutputTxId", "Outputumber");

                    b.HasIndex("MultisigOutputTxId", "Outputumber");

                    b.ToTable("CommitmentMultisigOutput");
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

            modelBuilder.Entity("SqlliteRepositories.Model.MultisigOutputEntity", b =>
                {
                    b.Property<string>("TransactionId");

                    b.Property<int>("OutputNumber");

                    b.HasKey("TransactionId", "OutputNumber");

                    b.ToTable("MultisigOutputs");
                });

            modelBuilder.Entity("SqlliteRepositories.Model.SettingsEntity", b =>
                {
                    b.Property<string>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("SqlliteRepositories.Model.CommitmentMultisigOutput", b =>
                {
                    b.HasOne("SqlliteRepositories.Model.CommitmentEntity", "Commitment")
                        .WithMany("CommitmentOutputs")
                        .HasForeignKey("CommitmentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SqlliteRepositories.Model.MultisigOutputEntity", "Output")
                        .WithMany("CommitmentOutputs")
                        .HasForeignKey("MultisigOutputTxId", "Outputumber")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
