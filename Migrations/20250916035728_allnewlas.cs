using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class allnewlas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hrd_DetailKeahlian",
                schema: "public",
                columns: table => new
                {
                    DetailKeahlianId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    KeahlianId = table.Column<Guid>(type: "uuid", nullable: true),
                    LevelKeahlian = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Penilai = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_DetailKeahlian", x => x.DetailKeahlianId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_DokumenDetailKaryawan",
                schema: "public",
                columns: table => new
                {
                    DokDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPeserta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NoPeserta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TglUpload = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NamaDokumen = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StatusKepemilikan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_DokumenDetailKaryawan", x => x.DokDetailId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_HasilTest",
                schema: "public",
                columns: table => new
                {
                    HasilTestId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPeserta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NomorPeserta = table.Column<decimal>(type: "numeric", nullable: true),
                    TglTest = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HasilTestText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_HasilTest", x => x.HasilTestId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstKeahlian",
                schema: "public",
                columns: table => new
                {
                    KeahlianId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKeahlian = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MstKeahlian", x => x.KeahlianId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstSoal",
                schema: "public",
                columns: table => new
                {
                    SoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Soal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    KategoriTest = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MstSoal", x => x.SoalId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstTTD",
                schema: "public",
                columns: table => new
                {
                    TTDId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MstTTD", x => x.TTDId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_RiwayatPendidikan",
                schema: "public",
                columns: table => new
                {
                    PendidikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenjangPendidikan = table.Column<string>(type: "text", nullable: false),
                    NamaInstitusi = table.Column<string>(type: "text", nullable: false),
                    Jurusan = table.Column<string>(type: "text", nullable: false),
                    TahunMasuk = table.Column<int>(type: "integer", nullable: false),
                    TahunLulus = table.Column<int>(type: "integer", nullable: false),
                    NilaiIpk = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Hrd_RiwayatPendidikan", x => x.PendidikanId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_RiwayatSertifikat",
                schema: "public",
                columns: table => new
                {
                    SertifikasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaSertifikasi = table.Column<string>(type: "text", nullable: false),
                    NamaInstitusi = table.Column<string>(type: "text", nullable: false),
                    Penyelenggara = table.Column<string>(type: "text", nullable: false),
                    NoSertifikasi = table.Column<long>(type: "bigint", nullable: false),
                    TglTerbit = table.Column<DateTime>(type: "date", nullable: false),
                    TglKadaluarsa = table.Column<DateTime>(type: "date", nullable: false),
                    AsalPartisipasi = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_RiwayatSertifikat", x => x.SertifikasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_DetailKeahlian",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_DokumenDetailKaryawan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_HasilTest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstKeahlian",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstSoal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstTTD",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_RiwayatPendidikan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_RiwayatSertifikat",
                schema: "public");
        }
    }
}
