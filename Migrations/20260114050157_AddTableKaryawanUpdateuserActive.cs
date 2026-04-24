using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableKaryawanUpdateuserActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgamaId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "GolonganDarahId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "IsPerawat",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "JenisPegawai",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KabupatenKotaId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KecamatanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KelurahanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "Kewarganegaraan",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NamaBank",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoPolisAsuransi",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NomorRekening",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TglAkhirKontrak",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TglAwalKontrak",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TglKeluar",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TglMasuk",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.RenameColumn(
                name: "ProvinsiId",
                schema: "public",
                table: "MstUserActive",
                newName: "InstalasiUnitId");

            migrationBuilder.AddColumn<string>(
                name: "EdukasiDetail",
                table: "AssesmentEdukasiDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Hrd_InstalasiUnit",
                schema: "public",
                columns: table => new
                {
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeInstalasiUnit = table.Column<string>(type: "text", nullable: true),
                    NamaInstalasiUnit = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_InstalasiUnit", x => x.InstalasiUnitId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_Karyawan",
                schema: "public",
                columns: table => new
                {
                    KaryawanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    JabatanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoIdentitas = table.Column<string>(type: "text", nullable: true),
                    KodeKaryawan = table.Column<string>(type: "text", nullable: true),
                    NoRekening = table.Column<string>(type: "text", nullable: true),
                    BankId = table.Column<string>(type: "text", nullable: true),
                    TanggalKontrak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TanggalAwalKerja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TanggalAkhirKerja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoHandphone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_Karyawan", x => x.KaryawanId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MappingPosisi",
                schema: "public",
                columns: table => new
                {
                    MappingPosisiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MappingPosisi", x => x.MappingPosisiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_InstalasiUnit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_Karyawan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MappingPosisi",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "EdukasiDetail",
                table: "AssesmentEdukasiDetails");

            migrationBuilder.RenameColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstUserActive",
                newName: "ProvinsiId");

            migrationBuilder.AddColumn<Guid>(
                name: "AgamaId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GolonganDarahId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPerawat",
                schema: "public",
                table: "MstUserActive",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisPegawai",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KabupatenKotaId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KecamatanId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KelurahanId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kewarganegaraan",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaBank",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPolisAsuransi",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorRekening",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglAkhirKontrak",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglAwalKontrak",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglKeluar",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglMasuk",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
