using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddLaporanAnestesi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaporanAnestesiDetails",
                columns: table => new
                {
                    DetailLaporanAnestesiId = table.Column<Guid>(type: "uuid", nullable: false),
                    LaporanAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    VMSevoflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalSevoflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    VMIsoflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalIsoflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    VMEnflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalEnflurane = table.Column<decimal>(type: "numeric", nullable: true),
                    FlowO2 = table.Column<string>(type: "text", nullable: true),
                    FlowN2O = table.Column<string>(type: "text", nullable: true),
                    GolonganDarah = table.Column<string>(type: "text", nullable: true),
                    TransfusiSebelumnya = table.Column<string>(type: "text", nullable: true),
                    Cairan = table.Column<decimal>(type: "numeric", nullable: true),
                    Kristaloid = table.Column<decimal>(type: "numeric", nullable: true),
                    Koloid = table.Column<decimal>(type: "numeric", nullable: true),
                    KeadaanPernapasan = table.Column<string>(type: "text", nullable: true),
                    StatusGizi = table.Column<string>(type: "text", nullable: true),
                    ASA = table.Column<string>(type: "text", nullable: true),
                    Pendarahan = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_LaporanAnestesiDetails", x => x.DetailLaporanAnestesiId);
                });

            migrationBuilder.CreateTable(
                name: "LaporanAnestesis",
                columns: table => new
                {
                    LaporanAnestesiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailTindakan = table.Column<string>(type: "text", nullable: true),
                    DokterOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterAsistenId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsistenAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Premidikasi = table.Column<List<string>>(type: "text[]", nullable: true),
                    TanggalOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuSelesaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuMulaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurasiOperasi = table.Column<TimeSpan>(type: "interval", nullable: true),
                    WaktuMulaiAnestesi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuSelesaiAnestesi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurasiAnestesi = table.Column<TimeSpan>(type: "interval", nullable: true),
                    PosisiOperasi = table.Column<string>(type: "text", nullable: true),
                    Oksigenasi = table.Column<string>(type: "text", nullable: true),
                    Induksi = table.Column<List<string>>(type: "text[]", nullable: true),
                    Intubasi = table.Column<string>(type: "text", nullable: true),
                    NoIntubasi = table.Column<decimal>(type: "numeric", nullable: false),
                    ProsesIntubasi = table.Column<string>(type: "text", nullable: true),
                    AlasanProsesIntubasi = table.Column<string>(type: "text", nullable: true),
                    GenderBayiLahir = table.Column<string>(type: "text", nullable: true),
                    WaktuCesar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APGARScore = table.Column<decimal>(type: "numeric", nullable: false),
                    PathTTDDokterAnestesi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LaporanAnestesis", x => x.LaporanAnestesiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaporanAnestesiDetails");

            migrationBuilder.DropTable(
                name: "LaporanAnestesis");
        }
    }
}
