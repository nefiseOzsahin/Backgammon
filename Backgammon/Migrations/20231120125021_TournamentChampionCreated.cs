using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class TournamentChampionCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChampion",
                table: "TournamentUsers");

            migrationBuilder.CreateTable(
                name: "TournamentChampions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    ChampionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentChampions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentChampions_AspNetUsers_ChampionId",
                        column: x => x.ChampionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentChampions_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentChampions_ChampionId",
                table: "TournamentChampions",
                column: "ChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentChampions_TournamentId",
                table: "TournamentChampions",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentChampions");

            migrationBuilder.AddColumn<bool>(
                name: "IsChampion",
                table: "TournamentUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
