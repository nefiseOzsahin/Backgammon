using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class isSmsSendFieldAddedTournamentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSMSSend",
                table: "Pairs");

            migrationBuilder.AddColumn<bool>(
                name: "IsSMSSend",
                table: "TournamentUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSMSSend",
                table: "TournamentUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsSMSSend",
                table: "Pairs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
