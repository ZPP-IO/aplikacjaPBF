using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitiLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    CharacterId = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivityLogs_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_CharacterModels_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "CharacterModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActionType",
                table: "ActivityLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CampaignId",
                table: "ActivityLogs",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CharacterId",
                table: "ActivityLogs",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserId",
                table: "ActivityLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");
        }
    }
}
