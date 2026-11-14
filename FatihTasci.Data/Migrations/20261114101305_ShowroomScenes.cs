using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShowroomScenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShowroomScenes",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastDeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    OriginalPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MediumPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LargePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomScenes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomScenes_SortOrder",
                schema: "Common",
                table: "ShowroomScenes",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowroomScenes",
                schema: "Common");
        }
    }
}
