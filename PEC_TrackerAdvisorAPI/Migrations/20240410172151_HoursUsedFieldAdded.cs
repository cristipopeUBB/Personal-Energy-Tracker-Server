using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEC_TrackerAdvisorAPI.Migrations
{
    /// <inheritdoc />
    public partial class HoursUsedFieldAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HoursUsed",
                table: "devices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoursUsed",
                table: "devices");
        }
    }
}
