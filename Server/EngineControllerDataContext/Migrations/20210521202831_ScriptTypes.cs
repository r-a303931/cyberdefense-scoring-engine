using Microsoft.EntityFrameworkCore.Migrations;

namespace EngineController.Migrations
{
    public partial class ScriptTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Readme",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "ScriptType",
                table: "CompetitionTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SystemIdentifier",
                table: "CompetitionTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScriptType",
                table: "CompetitionPenalties",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SystemIdentifier",
                table: "CompetitionPenalties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Readme",
                table: "Readme",
                column: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Readme",
                table: "Readme");

            migrationBuilder.DropColumn(
                name: "ScriptType",
                table: "CompetitionTasks");

            migrationBuilder.DropColumn(
                name: "SystemIdentifier",
                table: "CompetitionTasks");

            migrationBuilder.DropColumn(
                name: "ScriptType",
                table: "CompetitionPenalties");

            migrationBuilder.DropColumn(
                name: "SystemIdentifier",
                table: "CompetitionPenalties");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Readme",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
