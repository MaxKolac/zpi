using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZPIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageVisibleDangerPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ImageVisibleDangerPercentage",
                table: "HostDevices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageVisibleDangerPercentage",
                table: "HostDevices");
        }
    }
}
