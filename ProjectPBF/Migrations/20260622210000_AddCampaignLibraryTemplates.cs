using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    public partial class AddCampaignLibraryTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignClassId",
                table: "CharacterModels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MainStatistic = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    StartingHealth = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    StartingResource = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    StartingGold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignClasses_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Effect = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Price = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignItemTemplates_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignSkillTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Cost = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Cooldown = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Effect = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSkillTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignSkillTemplates_CampaignModels_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "CampaignModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModels_CampaignClassId",
                table: "CharacterModels",
                column: "CampaignClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignClasses_CampaignId",
                table: "CampaignClasses",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignClasses_CampaignId_Name",
                table: "CampaignClasses",
                columns: new[] { "CampaignId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignItemTemplates_CampaignId",
                table: "CampaignItemTemplates",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignItemTemplates_CampaignId_Name",
                table: "CampaignItemTemplates",
                columns: new[] { "CampaignId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSkillTemplates_CampaignId",
                table: "CampaignSkillTemplates",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSkillTemplates_CampaignId_Name",
                table: "CampaignSkillTemplates",
                columns: new[] { "CampaignId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterModels_CampaignClasses_CampaignClassId",
                table: "CharacterModels",
                column: "CampaignClassId",
                principalTable: "CampaignClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterModels_CampaignClasses_CampaignClassId",
                table: "CharacterModels");

            migrationBuilder.DropTable(name: "CampaignItemTemplates");
            migrationBuilder.DropTable(name: "CampaignSkillTemplates");
            migrationBuilder.DropTable(name: "CampaignClasses");

            migrationBuilder.DropIndex(
                name: "IX_CharacterModels_CampaignClassId",
                table: "CharacterModels");

            migrationBuilder.DropColumn(
                name: "CampaignClassId",
                table: "CharacterModels");
        }
    }
}
