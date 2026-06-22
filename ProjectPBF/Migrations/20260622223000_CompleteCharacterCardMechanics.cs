using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    public partial class CompleteCharacterCardMechanics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignSkillTemplateId",
                table: "CharacterSkills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cooldown",
                table: "CharacterSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Cost",
                table: "CharacterSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Effect",
                table: "CharacterSkills",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "CharacterSkills",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedHistoryPointCost",
                table: "CharacterSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampaignItemTemplateId",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Effect",
                table: "InventoryItems",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrantedByUserId",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "InventoryItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "InventoryItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "InventoryItems",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_CampaignSkillTemplateId",
                table: "CharacterSkills",
                column: "CampaignSkillTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CampaignItemTemplateId",
                table: "InventoryItems",
                column: "CampaignItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_GrantedByUserId",
                table: "InventoryItems",
                column: "GrantedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterSkills_CampaignSkillTemplates_CampaignSkillTemplateId",
                table: "CharacterSkills",
                column: "CampaignSkillTemplateId",
                principalTable: "CampaignSkillTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_AspNetUsers_GrantedByUserId",
                table: "InventoryItems",
                column: "GrantedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_CampaignItemTemplates_CampaignItemTemplateId",
                table: "InventoryItems",
                column: "CampaignItemTemplateId",
                principalTable: "CampaignItemTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterSkills_CampaignSkillTemplates_CampaignSkillTemplateId",
                table: "CharacterSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_AspNetUsers_GrantedByUserId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_CampaignItemTemplates_CampaignItemTemplateId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_CharacterSkills_CampaignSkillTemplateId",
                table: "CharacterSkills");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_CampaignItemTemplateId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_GrantedByUserId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "CampaignSkillTemplateId",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "Cooldown",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "Effect",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "RequestedHistoryPointCost",
                table: "CharacterSkills");

            migrationBuilder.DropColumn(
                name: "CampaignItemTemplateId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Effect",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "GrantedByUserId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "InventoryItems");
        }
    }
}
