using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class all : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NamaDepan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaBelakang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

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
                name: "MstDokter",
                schema: "dbo",
                columns: table => new
                {
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KdDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Str = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TglSip = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TglStr = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PanggilDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nik = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MstDokter", x => x.DokterId);
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
                    KodePasien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalDibuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DibuatOleh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JenisKelamin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendidikanTerakhir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatIdentitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatDomisili = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Negara = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provinsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kota = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kabupaten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KodePos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notelpon1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notelpon2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notelpon3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kewarganegaraan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusKewarganegaraan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatPerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoPerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GolonganDarah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alergi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiwayatPenyakit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiwayatOperasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiwayatPenyakitKeluarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    NamaKontakDarurat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hubkel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentitasDarurat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatDarurat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoDarurat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaOrtu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentitasOrtu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PekerjaanOrtu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HubkelAnak = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformasiSekolah = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MstDokter",
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
        }
    }
}
