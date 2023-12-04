using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZPIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationDescription",
                table: "HostDevices",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationDescription",
                table: "HostDevices");
        }
    }
}
