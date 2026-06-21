using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadTagsAndArchiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "ForumThreads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedByUserId",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ForumThreads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Tags",
                table: "ForumThreads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tags",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreads_IsArchived",
                table: "ForumThreads",
                column: "IsArchived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ForumThreads_IsArchived",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ForumPosts");
        }
    }
}
