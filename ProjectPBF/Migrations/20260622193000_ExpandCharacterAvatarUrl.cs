using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCharacterAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CharacterModels', 'AvatarUrl') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CharacterModels] ALTER COLUMN [AvatarUrl] nvarchar(max) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.CharacterModels', 'AvatarUrl') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CharacterModels] ALTER COLUMN [AvatarUrl] nvarchar(2048) NULL;
END
");
        }
    }
}
