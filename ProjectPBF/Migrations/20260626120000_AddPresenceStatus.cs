using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectPBF.Migrations
{
    /// <inheritdoc />
    public partial class AddPresenceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PresenceStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresenceStatus",
                table: "AspNetUsers");
        }
    }
}
