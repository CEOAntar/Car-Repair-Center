using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRepairCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "RepairOrders",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "RepairOrders");
        }
    }
}
