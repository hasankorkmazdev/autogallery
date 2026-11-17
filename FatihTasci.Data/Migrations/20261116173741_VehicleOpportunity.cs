using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class VehicleOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpportunity",
                schema: "Inventory",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpportunity",
                schema: "Inventory",
                table: "Vehicles");
        }
    }
}
