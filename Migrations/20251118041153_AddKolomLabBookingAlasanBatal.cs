using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomLabBookingAlasanBatal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlasanPembatalan",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPembayaran",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TTDPathPembatalan",
                table: "LabBookings",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlasanPembatalan",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "StatusPembayaran",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "TTDPathPembatalan",
                table: "LabBookings");
        }
    }
}
