using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class edittableuseractive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pegawai",
                schema: "public");

            migrationBuilder.AddColumn<Guid>(
                name: "AgamaId",
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
                name: "NoSTR",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorTelepon",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinsiId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPegawai",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgamaId",
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
                name: "NoSTR",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NomorTelepon",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "ProvinsiId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "StatusPegawai",
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

            migrationBuilder.CreateTable(
                name: "Pegawai",
                schema: "public",
                columns: table => new
                {
                    PegawaiId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgamaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsAktif = table.Column<bool>(type: "boolean", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsPerawat = table.Column<bool>(type: "boolean", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    JenisPegawai = table.Column<string>(type: "text", nullable: true),
                    KabupatenKotaId = table.Column<Guid>(type: "uuid", nullable: true),
                    KecamatanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "text", nullable: true),
                    NamaLengkap = table.Column<string>(type: "text", nullable: true),
                    NoIdentitas = table.Column<string>(type: "text", nullable: true),
                    NoSTR = table.Column<string>(type: "text", nullable: true),
                    NomorTelepon = table.Column<string>(type: "text", nullable: true),
                    PendidikanId = table.Column<Guid>(type: "uuid", nullable: true),
                    PinPegawai = table.Column<string>(type: "text", nullable: true),
                    PosisiId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusPegawai = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TempatLahir = table.Column<string>(type: "text", nullable: true),
                    TglAkhirKontrak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAwalKontrak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pegawai", x => x.PegawaiId);
                });
        }
    }
}
