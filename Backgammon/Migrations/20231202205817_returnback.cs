using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class returnback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourTournamentUser");

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "TournamentUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentUsers_TournamentId",
                table: "TournamentUsers",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentUsers_Tournaments_TournamentId",
                table: "TournamentUsers",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentUsers_Tournaments_TournamentId",
                table: "TournamentUsers");

            migrationBuilder.DropIndex(
                name: "IX_TournamentUsers_TournamentId",
                table: "TournamentUsers");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "TournamentUsers");

            migrationBuilder.CreateTable(
                name: "TourTournamentUser",
                columns: table => new
                {
                    TournamentUsersId = table.Column<int>(type: "int", nullable: false),
                    ToursId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourTournamentUser", x => new { x.TournamentUsersId, x.ToursId });
                    table.ForeignKey(
                        name: "FK_TourTournamentUser_TournamentUsers_TournamentUsersId",
                        column: x => x.TournamentUsersId,
                        principalTable: "TournamentUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourTournamentUser_Tours_ToursId",
                        column: x => x.ToursId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourTournamentUser_ToursId",
                table: "TourTournamentUser",
                column: "ToursId");
        }
    }
}
