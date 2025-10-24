using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddkolomdetailLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOrder",
                table: "LabBookings");

            migrationBuilder.AddColumn<string>(
                name: "NoOrder",
                table: "LabBookingDetails",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOrder",
                table: "LabBookingDetails");

            migrationBuilder.AddColumn<string>(
                name: "NoOrder",
                table: "LabBookings",
                type: "text",
                nullable: true);
        }
    }
}
