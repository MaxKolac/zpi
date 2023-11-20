using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZPIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageAndSeparateGeoLocationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExactLocation",
                table: "HostDevices");

            migrationBuilder.DropColumn(
                name: "LastTemperature",
                table: "HostDevices");

            migrationBuilder.AddColumn<byte[]>(
                name: "LastImage",
                table: "HostDevices",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastKnownTemperature",
                table: "HostDevices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocationAltitude",
                table: "HostDevices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocationLatitude",
                table: "HostDevices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastImage",
                table: "HostDevices");

            migrationBuilder.DropColumn(
                name: "LastKnownTemperature",
                table: "HostDevices");

            migrationBuilder.DropColumn(
                name: "LocationAltitude",
                table: "HostDevices");

            migrationBuilder.DropColumn(
                name: "LocationLatitude",
                table: "HostDevices");

            migrationBuilder.AddColumn<string>(
                name: "ExactLocation",
                table: "HostDevices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastTemperature",
                table: "HostDevices",
                type: "TEXT",
                nullable: true);
        }
    }
}
