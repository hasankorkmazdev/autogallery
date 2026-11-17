using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "Crm",
                table: "Deliveries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "Crm",
                table: "Deliveries");
        }
    }
}
