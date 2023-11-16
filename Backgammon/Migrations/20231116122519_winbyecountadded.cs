using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class winbyecountadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ByeCount",
                table: "TournamentUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinCount",
                table: "TournamentUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ByeCount",
                table: "TournamentUsers");

            migrationBuilder.DropColumn(
                name: "WinCount",
                table: "TournamentUsers");
        }
    }
}
