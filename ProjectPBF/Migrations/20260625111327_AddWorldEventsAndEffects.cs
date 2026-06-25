using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldEventsAndEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorldEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    InGameDate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    MechanicalImpactNote = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldEvents_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorldEvents_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldEventEffects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorldEventId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    ExperienceChange = table.Column<int>(type: "int", nullable: false),
                    HistoryPointsChange = table.Column<int>(type: "int", nullable: false),
                    StatisticPointsChange = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AppliedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldEventEffects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldEventEffects_AspNetUsers_AppliedByUserId",
                        column: x => x.AppliedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorldEventEffects_CharacterModels_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "CharacterModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorldEventEffects_WorldEvents_WorldEventId",
                        column: x => x.WorldEventId,
                        principalTable: "WorldEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorldEventEffects_AppliedByUserId",
                table: "WorldEventEffects",
                column: "AppliedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEventEffects_CharacterId",
                table: "WorldEventEffects",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEventEffects_WorldEventId",
                table: "WorldEventEffects",
                column: "WorldEventId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEvents_CampaignId",
                table: "WorldEvents",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEvents_CreatedAt",
                table: "WorldEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorldEvents_CreatedByUserId",
                table: "WorldEvents",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorldEventEffects");

            migrationBuilder.DropTable(
                name: "WorldEvents");
        }
    }
}
