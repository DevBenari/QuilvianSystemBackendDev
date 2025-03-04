using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class postgre2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDokterPraktek",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Kewarganegaraan",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstPoliklinik");

            migrationBuilder.RenameColumn(
                name: "Foto",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "JudulFileFoto");

            migrationBuilder.RenameColumn(
                name: "PanggilDokter",
                schema: "public",
                table: "MstDokter",
                newName: "Nohp");

            migrationBuilder.AddColumn<string>(
                name: "FotoName",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBytes",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KewarganegaraanId",
                schema: "public",
                table: "PdfPasienBaru",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LayananPoliklinik",
                schema: "public",
                table: "MstPoliklinik",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Deskripsi",
                schema: "public",
                table: "MstPoliklinik",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstPersalinan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstPersalinan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstPersalinan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstPersalinan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstPersalinan",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstPersalinan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstPersalinan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Alamat",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FotoDokter",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBytes",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAsuransi",
                schema: "public",
                table: "MstDokter",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JudulFileFoto",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telepon",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NamaDepartement",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Lokasi",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KepalaDepartement",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "JamBuka",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsPKS",
                schema: "public",
                table: "MstAsuransi",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaAgama",
                schema: "public",
                table: "MstAgama",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KodeAgama",
                schema: "public",
                table: "MstAgama",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "MstCoveranAsuransi",
                schema: "public",
                columns: table => new
                {
                    CoveranAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeCoveranAsuransi = table.Column<string>(type: "text", nullable: false),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    ServiceCode = table.Column<string>(type: "text", nullable: true),
                    ServiceDesc = table.Column<string>(type: "text", nullable: true),
                    ServiceCodeClass = table.Column<string>(type: "text", nullable: true),
                    Class = table.Column<string>(type: "text", nullable: true),
                    IsSurgery = table.Column<bool>(type: "boolean", nullable: true),
                    Tarif = table.Column<decimal>(type: "numeric", nullable: true),
                    TglBerlaku = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglBerakhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPKS = table.Column<bool>(type: "boolean", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstCoveranAsuransi", x => x.CoveranAsuransiId);
                    table.ForeignKey(
                        name: "FK_MstCoveranAsuransi_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "public",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                });

            migrationBuilder.CreateTable(
                name: "MstSubPoli",
                schema: "public",
                columns: table => new
                {
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaSubPoli = table.Column<string>(type: "text", nullable: false),
                    KodeSubPoli = table.Column<string>(type: "text", nullable: false),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    KepalaSubPoli = table.Column<string>(type: "text", nullable: false),
                    Lokasi = table.Column<string>(type: "text", nullable: false),
                    Telepon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    HariOperasional = table.Column<string>(type: "text", nullable: false),
                    JamBuka = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamTutup = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LayananSubPoli = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSubPoli", x => x.SubPoliId);
                    table.ForeignKey(
                        name: "FK_MstSubPoli_MstPoliklinik_PoliId",
                        column: x => x.PoliId,
                        principalSchema: "public",
                        principalTable: "MstPoliklinik",
                        principalColumn: "PoliklinikId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DokterPolis",
                columns: table => new
                {
                    DokterPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaDokter = table.Column<string>(type: "text", nullable: false),
                    KodeDokterPoli = table.Column<string>(type: "text", nullable: true),
                    KodeDokterSubPoli = table.Column<string>(type: "text", nullable: true),
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPoliKlinik = table.Column<string>(type: "text", nullable: true),
                    NamaSubPoli = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DokterPolis", x => x.DokterPoliId);
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstPoliklinik_PoliId",
                        column: x => x.PoliId,
                        principalSchema: "public",
                        principalTable: "MstPoliklinik",
                        principalColumn: "PoliklinikId");
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstSubPoli_SubPoliId",
                        column: x => x.SubPoliId,
                        principalSchema: "public",
                        principalTable: "MstSubPoli",
                        principalColumn: "SubPoliId");
                });

            migrationBuilder.CreateTable(
                name: "MstJadwalPraktek",
                schema: "public",
                columns: table => new
                {
                    JadwalPraktekId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaDokter = table.Column<string>(type: "text", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeJadwalPraktek = table.Column<string>(type: "text", nullable: false),
                    WaktuPraktek = table.Column<string>(type: "text", nullable: false),
                    HariPraktek = table.Column<string>(type: "text", nullable: false),
                    JamMulai = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamBerakhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxPasien = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MstJadwalPraktek", x => x.JadwalPraktekId);
                    table.ForeignKey(
                        name: "FK_MstJadwalPraktek_DokterPolis_DokterPoliId",
                        column: x => x.DokterPoliId,
                        principalTable: "DokterPolis",
                        principalColumn: "DokterPoliId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_PoliId",
                table: "DokterPolis",
                column: "PoliId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_SubPoliId",
                table: "DokterPolis",
                column: "SubPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCoveranAsuransi_AsuransiId",
                schema: "public",
                table: "MstCoveranAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstSubPoli_PoliId",
                schema: "public",
                table: "MstSubPoli",
                column: "PoliId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstCoveranAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJadwalPraktek",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DokterPolis");

            migrationBuilder.DropTable(
                name: "MstSubPoli",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KewarganegaraanId",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "Alamat",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "FotoDokter",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "IsAsuransi",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "JudulFileFoto",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "IsPKS",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.RenameColumn(
                name: "JudulFileFoto",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "Foto");

            migrationBuilder.RenameColumn(
                name: "Nohp",
                schema: "public",
                table: "MstDokter",
                newName: "PanggilDokter");

            migrationBuilder.AddColumn<string>(
                name: "Kewarganegaraan",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LayananPoliklinik",
                schema: "public",
                table: "MstPoliklinik",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Deskripsi",
                schema: "public",
                table: "MstPoliklinik",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstPoliklinik",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Telepon",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaDepartement",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lokasi",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KepalaDepartement",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JamBuka",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NamaAgama",
                schema: "public",
                table: "MstAgama",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KodeAgama",
                schema: "public",
                table: "MstAgama",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MstDokterPraktek",
                schema: "public",
                columns: table => new
                {
                    DokterPraktekId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Dokter = table.Column<string>(type: "text", nullable: false),
                    Hari = table.Column<string>(type: "text", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    JamKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamPraktek = table.Column<string>(type: "text", nullable: false),
                    Layanan = table.Column<string>(type: "text", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDokterPraktek", x => x.DokterPraktekId);
                    table.ForeignKey(
                        name: "FK_MstDokterPraktek_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterPraktek_DokterId",
                schema: "public",
                table: "MstDokterPraktek",
                column: "DokterId");
        }
    }
}
