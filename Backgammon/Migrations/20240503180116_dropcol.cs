using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backgammon.Migrations
{
    /// <inheritdoc />
    public partial class dropcol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isSMSSend",
                table: "Pairs",
                newName: "IsSMSSend");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSMSSend",
                table: "Pairs",
                newName: "isSMSSend");
        }
    }
}
