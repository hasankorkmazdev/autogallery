using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class LeadSellDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeadMedia_LeadId",
                schema: "Crm",
                table: "LeadMedia");

            migrationBuilder.AddColumn<int>(
                name: "SellDamage",
                schema: "Crm",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellPackage",
                schema: "Crm",
                table: "Leads",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Crm",
                table: "LeadMedia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeadMedia_LeadId_SortOrder",
                schema: "Crm",
                table: "LeadMedia",
                columns: new[] { "LeadId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeadMedia_LeadId_SortOrder",
                schema: "Crm",
                table: "LeadMedia");

            migrationBuilder.DropColumn(
                name: "SellDamage",
                schema: "Crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SellPackage",
                schema: "Crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Crm",
                table: "LeadMedia");

            migrationBuilder.CreateIndex(
                name: "IX_LeadMedia_LeadId",
                schema: "Crm",
                table: "LeadMedia",
                column: "LeadId");
        }
    }
}
