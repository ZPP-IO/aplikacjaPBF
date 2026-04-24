using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddUserApprovalAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AccountStatus",
                table: "AspNetUsers",
                column: "AccountStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApprovedByUserId",
                table: "AspNetUsers",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApprovedByUserId",
                table: "AspNetUsers",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApprovedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AccountStatus",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApprovedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "AspNetUsers");
        }
    }
}
