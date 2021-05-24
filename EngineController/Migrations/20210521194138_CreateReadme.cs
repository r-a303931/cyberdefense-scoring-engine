using Microsoft.EntityFrameworkCore.Migrations;

namespace EngineController.Migrations
{
    public partial class CreateReadme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionPenalties",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PenaltyName = table.Column<string>(type: "TEXT", nullable: true),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    PenaltyScript = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionPenalties", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionTasks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskName = table.Column<string>(type: "TEXT", nullable: true),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidationScript = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionTasks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Readme",
                columns: table => new
                {
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AppliedCompetitionPenalties",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetitionPenaltyID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppliedCompetitionPenalties", x => new { x.CompetitionPenaltyID, x.TeamID });
                    table.ForeignKey(
                        name: "FK_AppliedCompetitionPenalties_CompetitionPenalties_CompetitionPenaltyID",
                        column: x => x.CompetitionPenaltyID,
                        principalTable: "CompetitionPenalties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppliedCompetitionPenalties_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppliedCompetitionPenalties_Teams_TeamID1",
                        column: x => x.TeamID1,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompletedCompetitionTasks",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetitionTaskID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedCompetitionTasks", x => new { x.CompetitionTaskID, x.TeamID });
                    table.ForeignKey(
                        name: "FK_CompletedCompetitionTasks_CompetitionTasks_CompetitionTaskID",
                        column: x => x.CompetitionTaskID,
                        principalTable: "CompetitionTasks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedCompetitionTasks_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedCompetitionTasks_Teams_TeamID1",
                        column: x => x.TeamID1,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppliedCompetitionPenalties_TeamID",
                table: "AppliedCompetitionPenalties",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedCompetitionPenalties_TeamID1",
                table: "AppliedCompetitionPenalties",
                column: "TeamID1");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedCompetitionTasks_TeamID",
                table: "CompletedCompetitionTasks",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedCompetitionTasks_TeamID1",
                table: "CompletedCompetitionTasks",
                column: "TeamID1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppliedCompetitionPenalties");

            migrationBuilder.DropTable(
                name: "CompletedCompetitionTasks");

            migrationBuilder.DropTable(
                name: "Readme");

            migrationBuilder.DropTable(
                name: "CompetitionPenalties");

            migrationBuilder.DropTable(
                name: "CompetitionTasks");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
