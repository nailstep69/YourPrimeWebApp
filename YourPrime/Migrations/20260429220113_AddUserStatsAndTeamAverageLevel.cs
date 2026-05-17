using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourPrime.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatsAndTeamAverageLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Assists",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Goals",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AverageLevel",
                table: "Teams",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assists",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Goals",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AverageLevel",
                table: "Teams");
        }
    }
}
