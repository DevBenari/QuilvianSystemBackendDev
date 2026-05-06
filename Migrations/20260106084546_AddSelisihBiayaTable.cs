using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddSelisihBiayaTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelisihBiayas",
                columns: table => new
                {
                    SelisihBiayaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPasien = table.Column<string>(type: "text", nullable: true),
                    AlamatPasien = table.Column<string>(type: "text", nullable: true),
                    NoRM = table.Column<string>(type: "text", nullable: true),
                    Kelas = table.Column<string>(type: "text", nullable: true),
                    NamaPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    AlamatPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    PekerjaanPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    NoPengenalPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    TipeTandaPengenal = table.Column<string>(type: "text", nullable: true),
                    NoHpPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    NoTelpKantorPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    HubunganPasien = table.Column<string>(type: "text", nullable: true),
                    TanggalTTD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: true),
                    PathTTDPetugas = table.Column<string>(type: "text", nullable: true),
                    PathTTDPenandaTangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SelisihBiayas", x => x.SelisihBiayaId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelisihBiayas");
        }
    }
}
