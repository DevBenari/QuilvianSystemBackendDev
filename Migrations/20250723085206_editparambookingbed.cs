using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editparambookingbed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RanapId",
                schema: "public",
                table: "BookingBedRanap",
                newName: "KunjunganId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KunjunganId",
                schema: "public",
                table: "BookingBedRanap",
                newName: "RanapId");
        }
    }
}
