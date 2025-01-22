using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystem.Migrations
{
    public partial class datanew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.RenameColumn(
                name: "PlaceOfBirth",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "IdentityNumber",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "TempatLahir");

            migrationBuilder.RenameColumn(
                name: "Handphone",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Suku");

            migrationBuilder.RenameColumn(
                name: "Gender",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "PendidikanTerakhir");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Pekerjaan");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "NomorTeleponKeluarga");

            migrationBuilder.AlterColumn<string>(
                name: "Foto",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Agama",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlamatDomisili",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlamatKantor",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlamatKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataKaryawanInput",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departemen",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HubunganKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InformasiAlamat",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JenisIdentitas",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JenisKelamin",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KabupatenKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kecamatan",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kelurahan",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KelurahanKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kewarganegaraan",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NIK",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaAyah",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaIbu",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaKantor",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaLengkap",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaSutri",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoIdentitas",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoPenjamin",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoRekamMedis",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorHP",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorKeluargaTerdekat",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorKtpKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorTeleponKantor",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalLahir",
                schema: "dbo",
                table: "MasterPegawai",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Asuransis",
                columns: table => new
                {
                    AsuransiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KodeAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipePerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Asuransis", x => x.AsuransiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asuransis");

            migrationBuilder.DropColumn(
                name: "Agama",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "AlamatDomisili",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "AlamatKantor",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "AlamatKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "DataKaryawanInput",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "Departemen",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "HubunganKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "InformasiAlamat",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "JenisIdentitas",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "JenisKelamin",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "KabupatenKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "Kecamatan",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "Kelurahan",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "KelurahanKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "Kewarganegaraan",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NIK",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaAyah",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaIbu",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaKantor",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaLengkap",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NamaSutri",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NoIdentitas",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NoPenjamin",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NoRekamMedis",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NomorHP",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NomorKeluargaTerdekat",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NomorKtpKeluarga",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "NomorTeleponKantor",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.DropColumn(
                name: "TanggalLahir",
                schema: "dbo",
                table: "MasterPegawai");

            migrationBuilder.RenameColumn(
                name: "Title",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "PlaceOfBirth");

            migrationBuilder.RenameColumn(
                name: "TempatLahir",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "IdentityNumber");

            migrationBuilder.RenameColumn(
                name: "Suku",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Handphone");

            migrationBuilder.RenameColumn(
                name: "PendidikanTerakhir",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "Pekerjaan",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "NomorTeleponKeluarga",
                schema: "dbo",
                table: "MasterPegawai",
                newName: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "Foto",
                schema: "dbo",
                table: "MasterPegawai",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfBirth",
                schema: "dbo",
                table: "MasterPegawai",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
