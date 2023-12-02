using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZPIServer.Migrations
{
    /// <inheritdoc />
    public partial class MovedFireStatusToHostDeviceFromSector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStatus",
                table: "Sectors");

            migrationBuilder.AddColumn<int>(
                name: "LastFireStatus",
                table: "HostDevices",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFireStatus",
                table: "HostDevices");

            migrationBuilder.AddColumn<int>(
                name: "LastStatus",
                table: "Sectors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
