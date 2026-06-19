using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionMemberStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dodaj kolumnę tylko jeśli nie istnieje (SQL Server)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionModels', 'DurationMinutes') IS NULL
BEGIN
    ALTER TABLE dbo.SessionModels ADD DurationMinutes int NOT NULL DEFAULT(0);
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionModels', 'IsPrivate') IS NULL
BEGIN
    ALTER TABLE dbo.SessionModels ADD IsPrivate bit NOT NULL DEFAULT(0);
END
");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "SessionMembers",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "NULL",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionMembers', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.SessionMembers ADD Status int NOT NULL DEFAULT(0);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Usuń kolumny tylko jeśli istnieją
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionModels', 'DurationMinutes') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SessionModels DROP COLUMN DurationMinutes;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionModels', 'IsPrivate') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SessionModels DROP COLUMN IsPrivate;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SessionMembers', 'Status') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SessionMembers DROP COLUMN Status;
END
");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "SessionMembers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "NULL");
        }
    }
}
