using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addpdfpasien2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstDokter_DokterId",
                table: "DokterPolis");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis");


            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalLahir",
                schema: "public",
                table: "PdfPasien",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamTutup",
                schema: "public",
                table: "MstSubPoli",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBuka",
                schema: "public",
                table: "MstSubPoli",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalPersalinan",
                schema: "public",
                table: "MstPersalinan",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TTLBayi",
                schema: "public",
                table: "MstPersalinan",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalOperasi",
                schema: "public",
                table: "MstOperasi",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TglBerlaku",
                schema: "public",
                table: "MstCoveranAsuransi",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TglBerakhir",
                schema: "public",
                table: "MstCoveranAsuransi",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
                       
            migrationBuilder.CreateTable(
                name: "PdfPasienMCU",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienMCUId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePdfPasienMCU = table.Column<string>(type: "text", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    TanggalPendaftaran = table.Column<DateOnly>(type: "date", nullable: true),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    AlamatPasien = table.Column<string>(type: "text", nullable: false),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: false),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: false),
                    KabupatenKota = table.Column<string>(type: "text", nullable: false),
                    Kecamatan = table.Column<string>(type: "text", nullable: false),
                    TipePasien = table.Column<string>(type: "text", nullable: false),
                    Asuransi = table.Column<string>(type: "text", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: false),
                    KodeMember = table.Column<string>(type: "text", nullable: false),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: false),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: false),
                    TipeRujukan = table.Column<string>(type: "text", nullable: false),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasienMCU", x => x.PendaftaranPasienMCUId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasienRadiologi",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienRadiologiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePdfPasienRadiologi = table.Column<string>(type: "text", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    TanggalPendaftaran = table.Column<DateOnly>(type: "date", nullable: true),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    AlamatPasien = table.Column<string>(type: "text", nullable: false),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: false),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: false),
                    KabupatenKota = table.Column<string>(type: "text", nullable: false),
                    Kecamatan = table.Column<string>(type: "text", nullable: false),
                    TipePasien = table.Column<string>(type: "text", nullable: false),
                    Asuransi = table.Column<string>(type: "text", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: false),
                    KodeMember = table.Column<string>(type: "text", nullable: false),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: false),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: false),
                    TipeRujukan = table.Column<string>(type: "text", nullable: false),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasienRadiologi", x => x.PendaftaranPasienRadiologiId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasienRehabMedik",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienRehabMedikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePdfPasienRehabMedik = table.Column<string>(type: "text", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    TanggalPendaftaran = table.Column<DateOnly>(type: "date", nullable: true),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    AlamatPasien = table.Column<string>(type: "text", nullable: false),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: false),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: false),
                    KabupatenKota = table.Column<string>(type: "text", nullable: false),
                    Kecamatan = table.Column<string>(type: "text", nullable: false),
                    TipePasien = table.Column<string>(type: "text", nullable: false),
                    Asuransi = table.Column<string>(type: "text", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: false),
                    KodeMember = table.Column<string>(type: "text", nullable: false),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: false),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: false),
                    TipeRujukan = table.Column<string>(type: "text", nullable: false),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasienRehabMedik", x => x.PendaftaranPasienRehabMedikId);
                });          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDokterAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstFasilitasPasien",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKodePos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienAmbulan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienMCU",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienRadiologi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienRehabMedik",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienUGD",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PendaftaranPasienOptiks");

            migrationBuilder.DropTable(
                name: "RgsFasilitasPasien",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Sukus");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalLahir",
                schema: "public",
                table: "PdfPasien",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamTutup",
                schema: "public",
                table: "MstSubPoli",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamBuka",
                schema: "public",
                table: "MstSubPoli",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalPersalinan",
                schema: "public",
                table: "MstPersalinan",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TTLBayi",
                schema: "public",
                table: "MstPersalinan",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalOperasi",
                schema: "public",
                table: "MstOperasi",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaDokter",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TglStr",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TglSip",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglBerlaku",
                schema: "public",
                table: "MstCoveranAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglBerakhir",
                schema: "public",
                table: "MstCoveranAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis",
                column: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstDokter_DokterId",
                table: "DokterPolis",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
