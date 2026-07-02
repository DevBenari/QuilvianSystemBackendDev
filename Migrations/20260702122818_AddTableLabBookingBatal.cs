using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableLabBookingBatal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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



            migrationBuilder.AddColumn<string>(
                name: "StatusKonfirmasi",
                schema: "public",
                table: "LabBooking",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabBookingBatals",
                columns: table => new
                {
                    BatalBookingLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailLabBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisPembatalan = table.Column<string>(type: "text", nullable: true),
                    TglPembatalan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabBookingBatals", x => x.BatalBookingLabId);
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "LabBookingBatals");

            migrationBuilder.DropColumn(
                name: "IsCito",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "StatusKonfirmasi",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.AddColumn<bool>(
                name: "IsCito",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true);

        }
    }
}
