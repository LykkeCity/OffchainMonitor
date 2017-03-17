using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlliteRepositories.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commitments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Commitment = table.Column<string>(nullable: true),
                    Punishment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Component = table.Column<string>(nullable: true),
                    Context = table.Column<string>(nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Msg = table.Column<string>(nullable: true),
                    Process = table.Column<string>(nullable: true),
                    Stack = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultisigOutputs",
                columns: table => new
                {
                    TransactionId = table.Column<string>(nullable: false),
                    OutputNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultisigOutputs", x => new { x.TransactionId, x.OutputNumber });
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentMultisigOutput",
                columns: table => new
                {
                    CommitmentId = table.Column<int>(nullable: false),
                    MultisigOutputTxId = table.Column<string>(nullable: false),
                    Outputumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentMultisigOutput", x => new { x.CommitmentId, x.MultisigOutputTxId, x.Outputumber });
                    table.ForeignKey(
                        name: "FK_CommitmentMultisigOutput_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitmentMultisigOutput_MultisigOutputs_MultisigOutputTxId_Outputumber",
                        columns: x => new { x.MultisigOutputTxId, x.Outputumber },
                        principalTable: "MultisigOutputs",
                        principalColumns: new[] { "TransactionId", "OutputNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentMultisigOutput_MultisigOutputTxId_Outputumber",
                table: "CommitmentMultisigOutput",
                columns: new[] { "MultisigOutputTxId", "Outputumber" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitmentMultisigOutput");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Commitments");

            migrationBuilder.DropTable(
                name: "MultisigOutputs");
        }
    }
}
