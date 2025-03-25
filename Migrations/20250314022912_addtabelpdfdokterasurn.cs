using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtabelpdfdokterasurn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "TanggalLahir",
                schema: "public",
                table: "PdfPasien",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "MstDokterAsuransi",
                schema: "public",
                columns: table => new
                {
                    DokterAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstDokterAsuransi", x => x.DokterAsuransiId);
                    table.ForeignKey(
                        name: "FK_MstDokterAsuransi_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "public",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                    table.ForeignKey(
                        name: "FK_MstDokterAsuransi_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstFasilitasPasien",
                schema: "public",
                columns: table => new
                {
                    FasilitasPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeFasilitas = table.Column<string>(type: "text", nullable: false),
                    NamaFasilitasPasien = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstFasilitasPasien", x => x.FasilitasPasienId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasienAmbulan",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienAmbulanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePdfPasienAmbulan = table.Column<string>(type: "text", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    AlamatPasien = table.Column<string>(type: "text", nullable: false),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: false),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    LayananAmbulan = table.Column<string>(type: "text", nullable: false),
                    DaerahTujuan = table.Column<string>(type: "text", nullable: false),
                    KelebihanJarak = table.Column<int>(type: "integer", nullable: false),
                    KelebihanWaktu = table.Column<int>(type: "integer", nullable: false),
                    JumlahParamedis = table.Column<int>(type: "integer", nullable: false),
                    IsAntarJemput = table.Column<bool>(type: "boolean", nullable: true),
                    Catatan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PdfPasienAmbulan", x => x.PendaftaranPasienAmbulanId);
                });

            migrationBuilder.CreateTable(
                name: "RgsFasilitasPasien",
                schema: "public",
                columns: table => new
                {
                    RegistFasilitasPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeRegistFasilitas = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    TTL = table.Column<DateOnly>(type: "date", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: false),
                    Alamat = table.Column<string>(type: "text", nullable: false),
                    NoTelepon = table.Column<string>(type: "text", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: false),
                    NamaFasilitasPasien = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_RgsFasilitasPasien", x => x.RegistFasilitasPasienId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_DokterId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "DokterId");
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
                name: "PdfPasienAmbulan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RgsFasilitasPasien",
                schema: "public");

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
        }
    }
}
