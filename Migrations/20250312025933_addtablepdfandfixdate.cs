using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablepdfandfixdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalLahir",
                schema: "public",
                table: "PdfPasienBaru",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamTutup",
                schema: "public",
                table: "MstPoliklinik",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBuka",
                schema: "public",
                table: "MstPoliklinik",
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
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TglStr",
                schema: "public",
                table: "MstDokter",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TglSip",
                schema: "public",
                table: "MstDokter",
                type: "date",
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

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalMulaiKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalAkhirKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Createdate",
                schema: "public",
                table: "MstAsuransi",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PdfPasienUGD",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienUGDId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePasienUGD = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    TTL = table.Column<DateOnly>(type: "date", nullable: true),
                    Umur = table.Column<int>(type: "integer", nullable: false),
                    NoTelp = table.Column<string>(type: "text", nullable: false),
                    NamaDokterUGD = table.Column<string>(type: "text", nullable: false),
                    Diagnosa = table.Column<string>(type: "text", nullable: false),
                    Tindakan = table.Column<string>(type: "text", nullable: false),
                    BiayaAdmin = table.Column<decimal>(type: "numeric", nullable: false),
                    Kelas = table.Column<string>(type: "text", nullable: false),
                    AsuransiId = table.Column<string>(type: "text", nullable: true),
                    NoPolis = table.Column<string>(type: "text", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    Afliasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PdfPasienUGD", x => x.PendaftaranPasienUGDId);
                });

            migrationBuilder.CreateTable(
                name: "PendaftaranPasienOptiks",
                columns: table => new
                {
                    PendaftaranPasienOptikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePasienOptik = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    TTL = table.Column<DateOnly>(type: "date", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    NoTelp = table.Column<string>(type: "text", nullable: false),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    DokterOptik = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PendaftaranPasienOptiks", x => x.PendaftaranPasienOptikId);
                });

            migrationBuilder.CreateTable(
                name: "Sukus",
                columns: table => new
                {
                    SukuId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeSuku = table.Column<string>(type: "text", nullable: false),
                    NamaSuku = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Sukus", x => x.SukuId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PdfPasienUGD",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PendaftaranPasienOptiks");

            migrationBuilder.DropTable(
                name: "Sukus");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TanggalLahir",
                schema: "public",
                table: "PdfPasienBaru",
                type: "timestamp with time zone",
                nullable: true,
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
                name: "JamTutup",
                schema: "public",
                table: "MstPoliklinik",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamBuka",
                schema: "public",
                table: "MstPoliklinik",
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
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglStr",
                schema: "public",
                table: "MstDokter",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglSip",
                schema: "public",
                table: "MstDokter",
                type: "timestamp with time zone",
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

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TanggalMulaiKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TanggalAkhirKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Createdate",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
