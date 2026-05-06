using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableHasilLab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuratJaminanId",
                table: "LabBookings");

            migrationBuilder.RenameColumn(
                name: "SuratJaminanPath",
                table: "LabBookings",
                newName: "NomorSuratJaminan");

            migrationBuilder.AddColumn<string>(
                name: "NoOrder",
                table: "RuangBedahBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "PetugasId",
                table: "RuangBedahBookings",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatatanJaminan",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoOrder",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlasanPembatalan",
                table: "LabBookingDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TTDPembatalanPath",
                table: "LabBookingDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabHasilDetails",
                columns: table => new
                {
                    DetailHasilLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TanggalSelesai = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoPhotoLab = table.Column<string>(type: "text", nullable: true),
                    PhotoLabPath = table.Column<string>(type: "text", nullable: true),
                    HasilLabManual = table.Column<string>(type: "text", nullable: true),
                    HasilLabAI = table.Column<string>(type: "text", nullable: true),
                    JumlahFilm = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabHasilDetails", x => x.DetailHasilLabId);
                });

            migrationBuilder.CreateTable(
                name: "LabHasils",
                columns: table => new
                {
                    HasilLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserActiveId = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
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
                    table.PrimaryKey("PK_LabHasils", x => x.HasilLabId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabHasilDetails");

            migrationBuilder.DropTable(
                name: "LabHasils");

            migrationBuilder.DropColumn(
                name: "NoOrder",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "PetugasId",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "CatatanJaminan",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "NoOrder",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "AlasanPembatalan",
                table: "LabBookingDetails");

            migrationBuilder.DropColumn(
                name: "TTDPembatalanPath",
                table: "LabBookingDetails");

            migrationBuilder.RenameColumn(
                name: "NomorSuratJaminan",
                table: "LabBookings",
                newName: "SuratJaminanPath");

            migrationBuilder.AddColumn<Guid>(
                name: "SuratJaminanId",
                table: "LabBookings",
                type: "uuid",
                nullable: true);
        }
    }
}
