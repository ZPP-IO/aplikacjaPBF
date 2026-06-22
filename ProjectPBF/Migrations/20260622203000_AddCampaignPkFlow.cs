using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    public partial class AddCampaignPkFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatisticPointsPerLevel",
                table: "CampaignModels",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "AvailableStatisticPoints",
                table: "CharacterModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatisticPointsChange",
                table: "CharacterDevelopmentLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatisticPointsPerLevel",
                table: "CampaignModels");

            migrationBuilder.DropColumn(
                name: "AvailableStatisticPoints",
                table: "CharacterModels");

            migrationBuilder.DropColumn(
                name: "StatisticPointsChange",
                table: "CharacterDevelopmentLogs");
        }
    }
}
