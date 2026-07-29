using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditKolomIsCitoLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCito",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.AddColumn<bool>(
                name: "IsCito",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCito",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.AddColumn<bool>(
                name: "IsCito",
                schema: "public",
                table: "LabBookingDetail",
                type: "boolean",
                nullable: true);
        }
    }
}
