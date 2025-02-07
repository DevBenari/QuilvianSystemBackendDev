using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class changeTypeDataPdfPasienBaru : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterPegawai",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "Agama",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "DibuatOleh",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "GolonganDarah",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Hubkel",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "HubkelAnak",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Identitas",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IdentitasDarurat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IdentitasOrtu",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Kabupaten",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Kecamatan",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Kelurahan",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Kota",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NamaOrtu",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Negara",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoDarurat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Notelpon1",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Notelpon2",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Notelpon3",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "PekerjaanOrtu",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "PendidikanTerakhir",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Provinsi",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "TanggalDibuat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "Kewarganegaraan",
                schema: "dbo",
                table: "PdfPasienBaru",
                newName: "PekerjaanOrangTua");

            migrationBuilder.AlterColumn<string>(
                name: "TempatLahir",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatPenyakitKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatPenyakit",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatOperasi",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "QrCode",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Pekerjaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NoRekamMedis",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NoIdentitas",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NamaPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NamaLengkap",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NamaKontakDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KodePos",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KodePasien",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JenisKelamin",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InformasiSekolah",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Foto",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Alergi",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AlamatPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AlamatIdentitas",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AlamatDomisili",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AlamatDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AgamaId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GolonganDarahId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HubunganAnak",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HubunganPasien",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdentitasId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentitasOrangTua",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KecKabId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KelurahanId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KewarganegaraanId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KotaId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaOrangTua",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NegaraId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoIdentitasDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoTelepon1",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoTelepon2",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoTelepon3",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoTeleponDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoTeleponPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendidikanTerakhirId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinsiId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TitleId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstUserActive",
                schema: "dbo",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserActiveCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handphone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_MstUserActive", x => x.UserActiveId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstUserActive",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "AgamaId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "GolonganDarahId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "HubunganAnak",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "HubunganPasien",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IdentitasId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IdentitasOrangTua",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KecKabId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KelurahanId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KewarganegaraanId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KotaId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NamaOrangTua",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NegaraId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoIdentitasDarurat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoTelepon1",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoTelepon2",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoTelepon3",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoTeleponDarurat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NoTeleponPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "PendidikanTerakhirId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "ProvinsiId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "TitleId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "PekerjaanOrangTua",
                schema: "dbo",
                table: "PdfPasienBaru",
                newName: "Kewarganegaraan");

            migrationBuilder.AlterColumn<string>(
                name: "TempatLahir",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatPenyakitKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatPenyakit",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RiwayatOperasi",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QrCode",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pekerjaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoRekamMedis",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoIdentitas",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaLengkap",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaKontakDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KodePos",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KodePasien",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JenisKelamin",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InformasiSekolah",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Foto",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alergi",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlamatPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlamatIdentitas",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlamatDomisili",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlamatDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Agama",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DibuatOleh",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GolonganDarah",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Hubkel",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HubkelAnak",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identitas",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentitasDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentitasOrtu",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kabupaten",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kecamatan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kelurahan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kota",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaOrtu",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Negara",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoDarurat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoPerusahaan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notelpon1",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notelpon2",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notelpon3",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PekerjaanOrtu",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PendidikanTerakhir",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Provinsi",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalDibuat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MasterPegawai",
                schema: "dbo",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Agama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatDomisili = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataKaryawanInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Departemen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HubunganKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformasiAlamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    JenisIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisKelamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KelurahanKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kewarganegaraan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NIK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaAyah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaIbu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaSutri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoPenjamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorHP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKeluargaTerdekat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKtpKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendidikanTerakhir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserActiveCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPegawai", x => x.UserActiveId);
                });
        }
    }
}
