using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class pairsIsSMSFieldAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isSMSSend",
                table: "Pairs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isSMSSend",
                table: "Pairs");
        }
    }
}
