using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class updateAsuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstAsuransi",
                schema: "dbo",
                columns: table => new
                {
                    AsuransiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JenisAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KategoriAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TanggalMulaiKerjasama = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TanggalAkhirKerjasama = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RSRekanan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetodeKlaim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaktuKlaim = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BatasMaxKlaimPerTahun = table.Column<int>(type: "int", nullable: true),
                    BatasMaxKlaimPerKunjungan = table.Column<int>(type: "int", nullable: true),
                    DokumenKlaim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersentasiBiayaPertanggungan = table.Column<int>(type: "int", nullable: true),
                    ObatDitanggung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TambahanTanggungan = table.Column<int>(type: "int", nullable: true),
                    BiayaTidakDitanggung = table.Column<int>(type: "int", nullable: true),
                    MasaTunggu = table.Column<int>(type: "int", nullable: true),
                    MaxUsiaPasien = table.Column<int>(type: "int", nullable: true),
                    NoRekRumahSakit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaBank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaBankCabang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermOfPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatasWaktuPembayaran = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PenaltiTerlambatBayar = table.Column<int>(type: "int", nullable: true),
                    NamaPerusahaanAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlamatPusat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlamatCabang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoTelepon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailPusat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoHotlineDarurat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaPerwakilan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoTeleponPerwakilan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailPerwakilan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JabatanPerwakilan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstAsuransi", x => x.AsuransiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstAsuransi",
                schema: "dbo");
        }
    }
}
