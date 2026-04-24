using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    GameMasterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignModels_AspNetUsers_GameMasterId",
                        column: x => x.GameMasterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignCharacterModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCharacterModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignCharacterModels_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCharacterModels_CharacterModels_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "CharacterModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacterModels_CampaignId",
                table: "CampaignCharacterModels",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacterModels_CampaignId_CharacterId",
                table: "CampaignCharacterModels",
                columns: new[] { "CampaignId", "CharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacterModels_CharacterId",
                table: "CampaignCharacterModels",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignModels_GameMasterId",
                table: "CampaignModels",
                column: "GameMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignModels_Status",
                table: "CampaignModels",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignCharacterModels");

            migrationBuilder.DropTable(
                name: "CampaignModels");
        }
    }
}
