using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class VehicleCampaignPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CampaignPrice",
                schema: "Inventory",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignPrice",
                schema: "Inventory",
                table: "Vehicles");
        }
    }
}
