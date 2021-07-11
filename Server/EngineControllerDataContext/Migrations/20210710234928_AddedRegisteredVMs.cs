using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EngineController.Migrations
{
    public partial class AddedRegisteredVMs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Readme");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedCompetitionTasks",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppliedCompetitionPenalties",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.AddColumn<Guid>(
                name: "VmId",
                table: "CompletedCompetitionTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SystemIdentifier",
                table: "CompetitionTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SystemIdentifier",
                table: "CompetitionPenalties",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VmId",
                table: "AppliedCompetitionPenalties",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedCompetitionTasks",
                table: "CompletedCompetitionTasks",
                columns: new[] { "CompetitionTaskID", "TeamID", "VmId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppliedCompetitionPenalties",
                table: "AppliedCompetitionPenalties",
                columns: new[] { "CompetitionPenaltyID", "TeamID", "VmId" });

            migrationBuilder.CreateTable(
                name: "CompetitionSystem",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReadmeText = table.Column<string>(type: "TEXT", nullable: true),
                    SystemIdentifier = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionSystem", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredVirtualMachines",
                columns: table => new
                {
                    VmId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemIdentifier = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCheckIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsConnectedNow = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeamID1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredVirtualMachines", x => x.VmId);
                    table.ForeignKey(
                        name: "FK_RegisteredVirtualMachines_CompetitionSystem_SystemIdentifier",
                        column: x => x.SystemIdentifier,
                        principalTable: "CompetitionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisteredVirtualMachines_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisteredVirtualMachines_Teams_TeamID1",
                        column: x => x.TeamID1,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedCompetitionTasks_AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks",
                column: "AppliedVirtualMachineVmId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedCompetitionTasks_VmId",
                table: "CompletedCompetitionTasks",
                column: "VmId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionTasks_SystemIdentifier",
                table: "CompetitionTasks",
                column: "SystemIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionPenalties_SystemIdentifier",
                table: "CompetitionPenalties",
                column: "SystemIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedCompetitionPenalties_AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties",
                column: "AppliedVirtualMachineVmId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedCompetitionPenalties_VmId",
                table: "AppliedCompetitionPenalties",
                column: "VmId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVirtualMachines_SystemIdentifier",
                table: "RegisteredVirtualMachines",
                column: "SystemIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVirtualMachines_TeamID",
                table: "RegisteredVirtualMachines",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVirtualMachines_TeamID1",
                table: "RegisteredVirtualMachines",
                column: "TeamID1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedCompetitionPenalties_RegisteredVirtualMachines_AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties",
                column: "AppliedVirtualMachineVmId",
                principalTable: "RegisteredVirtualMachines",
                principalColumn: "VmId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedCompetitionPenalties_RegisteredVirtualMachines_VmId",
                table: "AppliedCompetitionPenalties",
                column: "VmId",
                principalTable: "RegisteredVirtualMachines",
                principalColumn: "VmId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPenalties_CompetitionSystem_SystemIdentifier",
                table: "CompetitionPenalties",
                column: "SystemIdentifier",
                principalTable: "CompetitionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionTasks_CompetitionSystem_SystemIdentifier",
                table: "CompetitionTasks",
                column: "SystemIdentifier",
                principalTable: "CompetitionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedCompetitionTasks_RegisteredVirtualMachines_AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks",
                column: "AppliedVirtualMachineVmId",
                principalTable: "RegisteredVirtualMachines",
                principalColumn: "VmId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedCompetitionTasks_RegisteredVirtualMachines_VmId",
                table: "CompletedCompetitionTasks",
                column: "VmId",
                principalTable: "RegisteredVirtualMachines",
                principalColumn: "VmId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppliedCompetitionPenalties_RegisteredVirtualMachines_AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedCompetitionPenalties_RegisteredVirtualMachines_VmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPenalties_CompetitionSystem_SystemIdentifier",
                table: "CompetitionPenalties");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionTasks_CompetitionSystem_SystemIdentifier",
                table: "CompetitionTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_CompletedCompetitionTasks_RegisteredVirtualMachines_AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_CompletedCompetitionTasks_RegisteredVirtualMachines_VmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropTable(
                name: "RegisteredVirtualMachines");

            migrationBuilder.DropTable(
                name: "CompetitionSystem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedCompetitionTasks",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropIndex(
                name: "IX_CompletedCompetitionTasks_AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropIndex(
                name: "IX_CompletedCompetitionTasks_VmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionTasks_SystemIdentifier",
                table: "CompetitionTasks");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionPenalties_SystemIdentifier",
                table: "CompetitionPenalties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppliedCompetitionPenalties",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropIndex(
                name: "IX_AppliedCompetitionPenalties_AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropIndex(
                name: "IX_AppliedCompetitionPenalties_VmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropColumn(
                name: "VmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropColumn(
                name: "AppliedVirtualMachineVmId",
                table: "CompletedCompetitionTasks");

            migrationBuilder.DropColumn(
                name: "VmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.DropColumn(
                name: "AppliedVirtualMachineVmId",
                table: "AppliedCompetitionPenalties");

            migrationBuilder.AlterColumn<string>(
                name: "SystemIdentifier",
                table: "CompetitionTasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "SystemIdentifier",
                table: "CompetitionPenalties",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedCompetitionTasks",
                table: "CompletedCompetitionTasks",
                columns: new[] { "CompetitionTaskID", "TeamID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppliedCompetitionPenalties",
                table: "AppliedCompetitionPenalties",
                columns: new[] { "CompetitionPenaltyID", "TeamID" });

            migrationBuilder.CreateTable(
                name: "Readme",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readme", x => x.ID);
                });
        }
    }
}
