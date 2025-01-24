using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianBackendDev.Migrations
{
    public partial class semua : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "dbo",
                table: "MstDokter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "dbo",
                table: "MstDokter",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "dbo",
                table: "MstDokter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "MstDokter",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "dbo",
                table: "MstDokter",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "dbo",
                table: "MstDokter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "MstDokter",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "NamaBelakang",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaDepan",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MasterPegawai",
                schema: "dbo",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserActiveCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoPenjamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NIK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JenisKelamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kewarganegaraan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendidikanTerakhir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatDomisili = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformasiAlamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorHP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Departemen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKeluargaTerdekat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HubunganKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KelurahanKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKtpKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaAyah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaIbu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaSutri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataKaryawanInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MasterPegawai", x => x.UserActiveId);
                });

            migrationBuilder.CreateTable(
                name: "MstAsuransi",
                schema: "dbo",
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
                    table.PrimaryKey("PK_MstAsuransi", x => x.AsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstDokterPraktek",
                schema: "dbo",
                columns: table => new
                {
                    DokterPraktekId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamPraktek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hari = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamMasuk = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamKeluar = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_MstDokterPraktek", x => x.DokterPraktekId);
                    table.ForeignKey(
                        name: "FK_MstDokterPraktek_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "dbo",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKeangotaan",
                schema: "dbo",
                columns: table => new
                {
                    KeangotaanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeangotaanKode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisKeangotaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisPromo = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MstKeangotaan", x => x.KeangotaanId);
                });

            migrationBuilder.CreateTable(
                name: "MstProvinsi",
                schema: "dbo",
                columns: table => new
                {
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinsiCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstProvinsi", x => x.ProvinsiId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasien",
                schema: "dbo",
                columns: table => new
                {
                    PendaftaranPasienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Penjamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasien", x => x.PendaftaranPasienId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasienBaru",
                schema: "dbo",
                columns: table => new
                {
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoPenjamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JenisKelamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kewarganegaraan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendidikanTerakhir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatDomisili = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformasiAlamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorHP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKantor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GolonganDarah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alergi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKeluargaTerdekat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HubunganKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KelurahanKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorTeleponKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaAyah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaIbu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaSutri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorKtpSutri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataKaryawanInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasienBaru", x => x.PendaftaranPasienBaruId);
                });

            migrationBuilder.CreateTable(
                name: "MstKabupaten",
                schema: "dbo",
                columns: table => new
                {
                    KabupatenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KabupatenCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKabupaten", x => x.KabupatenId);
                    table.ForeignKey(
                        name: "FK_MstKabupaten_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "dbo",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKecamatan",
                schema: "dbo",
                columns: table => new
                {
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KecamatanCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KecamatanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKecamatan", x => x.KecamatanId);
                    table.ForeignKey(
                        name: "FK_MstKecamatan_MstKabupaten_KabupatenId",
                        column: x => x.KabupatenId,
                        principalSchema: "dbo",
                        principalTable: "MstKabupaten",
                        principalColumn: "KabupatenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKelurahan",
                schema: "dbo",
                columns: table => new
                {
                    KelurahanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KelurahanCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KelurahanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKelurahan", x => x.KelurahanId);
                    table.ForeignKey(
                        name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                        column: x => x.KecamatanId,
                        principalSchema: "dbo",
                        principalTable: "MstKecamatan",
                        principalColumn: "KecamatanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterPraktek_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupaten_ProvinsiId",
                schema: "dbo",
                table: "MstKabupaten",
                column: "ProvinsiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKecamatan_KabupatenId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKelurahan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan",
                column: "KecamatanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterPegawai",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstAsuransi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstDokterPraktek",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKeangotaan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKelurahan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PdfPasien",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PdfPasienBaru",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKecamatan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKabupaten",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstProvinsi",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "NamaBelakang",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NamaDepan",
                table: "AspNetUsers");
        }
    }
}
