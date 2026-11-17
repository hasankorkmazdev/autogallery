using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FatihTasci.Data.Migrations
{
    /// <inheritdoc />
    public partial class SocialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "Common",
                table: "SiteSettings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            // Eski Instagram / Facebook / Youtube anahtarları tek SocialLinks (JSON) anahtarına taşınır.
            migrationBuilder.Sql(@"
DECLARE @json nvarchar(4000) = (
    SELECT '[' + STRING_AGG(CONCAT('{""Platform"":', CASE [Key] WHEN 'Instagram' THEN 1 WHEN 'Facebook' THEN 2 ELSE 4 END,
        ',""Url"":""', STRING_ESCAPE(LTRIM(RTRIM([Value])), 'json'), '""}'), ',')
        WITHIN GROUP (ORDER BY CASE [Key] WHEN 'Instagram' THEN 1 WHEN 'Facebook' THEN 2 ELSE 3 END) + ']'
    FROM [Common].[SiteSettings]
    WHERE [Key] IN ('Instagram', 'Facebook', 'Youtube') AND NULLIF(LTRIM(RTRIM([Value])), '') IS NOT NULL);

IF NOT EXISTS (SELECT 1 FROM [Common].[SiteSettings] WHERE [Key] = 'SocialLinks')
    UPDATE [Common].[SiteSettings] SET [Key] = 'SocialLinks', [Value] = @json
    WHERE [Id] = (SELECT MIN([Id]) FROM [Common].[SiteSettings] WHERE [Key] IN ('Instagram', 'Facebook', 'Youtube'));

DELETE FROM [Common].[SiteSettings] WHERE [Key] IN ('Instagram', 'Facebook', 'Youtube');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // SocialLinks anahtarı geri bölünmez; sütun eski boyuna döner.
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "Common",
                table: "SiteSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
