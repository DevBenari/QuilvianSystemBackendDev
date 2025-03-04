using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class allPostgre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AspNetGroupRole",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    PositionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetGroupRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRole",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    PositionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    KodeUser = table.Column<string>(type: "text", nullable: false),
                    NamaUser = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstAgama",
                schema: "public",
                columns: table => new
                {
                    AgamaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeAgama = table.Column<string>(type: "text", nullable: false),
                    NamaAgama = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstAgama", x => x.AgamaId);
                });

            migrationBuilder.CreateTable(
                name: "MstAsuransi",
                schema: "public",
                columns: table => new
                {
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeAsuransi = table.Column<string>(type: "text", nullable: true),
                    Createdate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    JenisAsuransi = table.Column<string>(type: "text", nullable: true),
                    KategoriAsuransi = table.Column<string>(type: "text", nullable: true),
                    StatusAsuransi = table.Column<string>(type: "text", nullable: true),
                    TanggalMulaiKerjasama = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TanggalAkhirKerjasama = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RSRekanan = table.Column<string>(type: "text", nullable: true),
                    MetodeKlaim = table.Column<string>(type: "text", nullable: true),
                    WaktuKlaim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BatasMaxKlaimPerTahun = table.Column<int>(type: "integer", nullable: true),
                    BatasMaxKlaimPerKunjungan = table.Column<int>(type: "integer", nullable: true),
                    DokumenKlaim = table.Column<string>(type: "text", nullable: true),
                    Layanan = table.Column<string>(type: "text", nullable: true),
                    PersentasiBiayaPertanggungan = table.Column<int>(type: "integer", nullable: true),
                    ObatDitanggung = table.Column<string>(type: "text", nullable: true),
                    TambahanTanggungan = table.Column<int>(type: "integer", nullable: true),
                    BiayaTidakDitanggung = table.Column<int>(type: "integer", nullable: true),
                    MasaTunggu = table.Column<int>(type: "integer", nullable: true),
                    MaxUsiaPasien = table.Column<int>(type: "integer", nullable: true),
                    NoRekRumahSakit = table.Column<string>(type: "text", nullable: true),
                    NamaBank = table.Column<string>(type: "text", nullable: true),
                    NamaBankCabang = table.Column<string>(type: "text", nullable: true),
                    TermOfPayment = table.Column<string>(type: "text", nullable: true),
                    BatasWaktuPembayaran = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PenaltiTerlambatBayar = table.Column<int>(type: "integer", nullable: true),
                    NamaPerusahaanAsuransi = table.Column<string>(type: "text", nullable: true),
                    AlamatPusat = table.Column<string>(type: "text", nullable: true),
                    AlamatCabang = table.Column<string>(type: "text", nullable: true),
                    NoTelepon = table.Column<string>(type: "text", nullable: true),
                    EmailPusat = table.Column<string>(type: "text", nullable: true),
                    NoHotlineDarurat = table.Column<string>(type: "text", nullable: true),
                    NamaPerwakilan = table.Column<string>(type: "text", nullable: true),
                    NoTeleponPerwakilan = table.Column<string>(type: "text", nullable: true),
                    EmailPerwakilan = table.Column<string>(type: "text", nullable: true),
                    JabatanPerwakilan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstAsuransi", x => x.AsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstDepartement",
                schema: "public",
                columns: table => new
                {
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeDepartement = table.Column<string>(type: "text", nullable: false),
                    NamaDepartement = table.Column<string>(type: "text", nullable: false),
                    KepalaDepartement = table.Column<string>(type: "text", nullable: false),
                    Lokasi = table.Column<string>(type: "text", nullable: false),
                    Telepon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    JamBuka = table.Column<string>(type: "text", nullable: false),
                    JamTutup = table.Column<string>(type: "text", nullable: false),
                    Layanan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDepartement", x => x.DepartementId);
                });

            migrationBuilder.CreateTable(
                name: "MstDokter",
                schema: "public",
                columns: table => new
                {
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    KdDokter = table.Column<string>(type: "text", nullable: false),
                    NmDokter = table.Column<string>(type: "text", nullable: false),
                    Sip = table.Column<string>(type: "text", nullable: false),
                    Str = table.Column<string>(type: "text", nullable: false),
                    TglSip = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglStr = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PanggilDokter = table.Column<string>(type: "text", nullable: false),
                    Nik = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstDokter", x => x.DokterId);
                });

            migrationBuilder.CreateTable(
                name: "MstGolonganDarah",
                schema: "public",
                columns: table => new
                {
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeGolonganDarah = table.Column<string>(type: "text", nullable: false),
                    NamaGolonganDarah = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstGolonganDarah", x => x.GolonganDarahId);
                });

            migrationBuilder.CreateTable(
                name: "MstIdentitas",
                schema: "public",
                columns: table => new
                {
                    IdentitasId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeIdentitas = table.Column<string>(type: "text", nullable: false),
                    JenisIdentitas = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstIdentitas", x => x.IdentitasId);
                });

            migrationBuilder.CreateTable(
                name: "MstJabatan",
                schema: "public",
                columns: table => new
                {
                    JabatanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeJabatan = table.Column<string>(type: "text", nullable: false),
                    NamaJabatan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstJabatan", x => x.JabatanId);
                });

            migrationBuilder.CreateTable(
                name: "MstKategoriPeralatan",
                schema: "public",
                columns: table => new
                {
                    KategoriPeralatanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKategoriPeralatan = table.Column<string>(type: "text", nullable: false),
                    NamaKategoriPeralatan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstKategoriPeralatan", x => x.KategoriPeralatanId);
                });

            migrationBuilder.CreateTable(
                name: "MstKeanggotaan",
                schema: "public",
                columns: table => new
                {
                    KeanggotaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKeanggotaan = table.Column<string>(type: "text", nullable: false),
                    JenisKeanggotaan = table.Column<string>(type: "text", nullable: false),
                    JenisPromo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstKeanggotaan", x => x.KeanggotaanId);
                });

            migrationBuilder.CreateTable(
                name: "MstNegara",
                schema: "public",
                columns: table => new
                {
                    NegaraId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeNegara = table.Column<string>(type: "text", nullable: false),
                    NamaNegara = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstNegara", x => x.NegaraId);
                });

            migrationBuilder.CreateTable(
                name: "MstPekerjaan",
                schema: "public",
                columns: table => new
                {
                    PekerjaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePekerjaan = table.Column<string>(type: "text", nullable: false),
                    NamaPekerjaan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstPekerjaan", x => x.PekerjaanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPendidikan",
                schema: "public",
                columns: table => new
                {
                    PendidikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePendidikan = table.Column<string>(type: "text", nullable: false),
                    NamaPendidikan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstPendidikan", x => x.PendidikanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPersalinan",
                schema: "public",
                columns: table => new
                {
                    PersalinanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePersalinan = table.Column<string>(type: "text", nullable: false),
                    NamaPersalinan = table.Column<string>(type: "text", nullable: false),
                    TanggalPersalinan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipePersalinan = table.Column<string>(type: "text", nullable: false),
                    TindakanPersalinan = table.Column<string>(type: "text", nullable: false),
                    SubTindakanPersalinan = table.Column<string>(type: "text", nullable: false),
                    KomplikasiPersalinan = table.Column<string>(type: "text", nullable: false),
                    NamaKamar = table.Column<string>(type: "text", nullable: false),
                    NoKamar = table.Column<string>(type: "text", nullable: false),
                    KategoriKamar = table.Column<string>(type: "text", nullable: false),
                    CatatanPersalinan = table.Column<string>(type: "text", nullable: false),
                    DokterPersalinan = table.Column<string>(type: "text", nullable: false),
                    BidanPersalinan = table.Column<string>(type: "text", nullable: false),
                    AnastesiPersalinan = table.Column<string>(type: "text", nullable: false),
                    ObservasiPersalinan = table.Column<string>(type: "text", nullable: false),
                    NamaBayi = table.Column<string>(type: "text", nullable: false),
                    JenisKelaminBayi = table.Column<string>(type: "text", nullable: false),
                    TTLBayi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BeratBayi = table.Column<string>(type: "text", nullable: false),
                    PanjangBayi = table.Column<string>(type: "text", nullable: false),
                    NamaAyah = table.Column<string>(type: "text", nullable: false),
                    NamaIbu = table.Column<string>(type: "text", nullable: false),
                    StatusBayi = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPersalinan", x => x.PersalinanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPoliklinik",
                schema: "public",
                columns: table => new
                {
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePoliklinik = table.Column<string>(type: "text", nullable: false),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: false),
                    KepalaPoliklinik = table.Column<string>(type: "text", nullable: false),
                    Lokasi = table.Column<string>(type: "text", nullable: false),
                    Telepon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    HariOperasional = table.Column<string>(type: "text", nullable: false),
                    JamBuka = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamTutup = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LayananPoliklinik = table.Column<string>(type: "text", nullable: false),
                    JumlahMaxPasien = table.Column<int>(type: "integer", nullable: false),
                    Deskripsi = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstPoliklinik", x => x.PoliklinikId);
                });

            migrationBuilder.CreateTable(
                name: "MstTitle",
                schema: "public",
                columns: table => new
                {
                    TitleId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeTitle = table.Column<string>(type: "text", nullable: false),
                    NamaTitle = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstTitle", x => x.TitleId);
                });

            migrationBuilder.CreateTable(
                name: "MstUserActive",
                schema: "public",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveCode = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    IdentityNumber = table.Column<string>(type: "text", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Handphone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstUserActive", x => x.UserActiveId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasien",
                schema: "dbo",
                columns: table => new
                {
                    PendaftaranPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    NamaLengkap = table.Column<string>(type: "text", nullable: false),
                    NoIdentitas = table.Column<string>(type: "text", nullable: false),
                    TempatLahir = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Penjamin = table.Column<string>(type: "text", nullable: false),
                    Layanan = table.Column<string>(type: "text", nullable: false),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PdfPasien", x => x.PendaftaranPasienId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasienBaru",
                schema: "dbo",
                columns: table => new
                {
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePasien = table.Column<string>(type: "text", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: true),
                    TipePasien = table.Column<string>(type: "text", nullable: false),
                    NoRekamMedisLama = table.Column<string>(type: "text", nullable: true),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaLengkap = table.Column<string>(type: "text", nullable: false),
                    IdentitasId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoIdentitas = table.Column<string>(type: "text", nullable: false),
                    TempatLahir = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    AgamaId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendidikanTerakhirId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlamatIdentitas = table.Column<string>(type: "text", nullable: true),
                    AlamatDomisili = table.Column<string>(type: "text", nullable: true),
                    NegaraId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: true),
                    KotaId = table.Column<Guid>(type: "uuid", nullable: true),
                    KecKabId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodePos = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NoTelepon1 = table.Column<int>(type: "integer", nullable: true),
                    NoTelepon2 = table.Column<int>(type: "integer", nullable: true),
                    NoTelepon3 = table.Column<int>(type: "integer", nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "text", nullable: false),
                    Suku = table.Column<string>(type: "text", nullable: true),
                    StatusKewarganegaraan = table.Column<string>(type: "text", nullable: true),
                    PekerjaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPerusahaan = table.Column<string>(type: "text", nullable: true),
                    AlamatPerusahaan = table.Column<string>(type: "text", nullable: true),
                    NoTeleponPerusahaan = table.Column<int>(type: "integer", nullable: true),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: true),
                    Alergi = table.Column<string>(type: "text", nullable: true),
                    RiwayatPenyakit = table.Column<string>(type: "text", nullable: true),
                    RiwayatOperasi = table.Column<string>(type: "text", nullable: true),
                    RiwayatPenyakitKeluarga = table.Column<string>(type: "text", nullable: true),
                    NamaKontakDarurat = table.Column<string>(type: "text", nullable: true),
                    HubunganPasien = table.Column<string>(type: "text", nullable: true),
                    NoIdentitasDarurat = table.Column<string>(type: "text", nullable: true),
                    AlamatDarurat = table.Column<string>(type: "text", nullable: true),
                    NoTeleponDarurat = table.Column<string>(type: "text", nullable: true),
                    NamaOrangTua = table.Column<string>(type: "text", nullable: true),
                    IdentitasOrangTua = table.Column<string>(type: "text", nullable: true),
                    PekerjaanOrangTua = table.Column<string>(type: "text", nullable: true),
                    HubunganAnak = table.Column<string>(type: "text", nullable: true),
                    InformasiSekolah = table.Column<string>(type: "text", nullable: true),
                    Foto = table.Column<string>(type: "text", nullable: true),
                    QrCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PdfPasienBaru", x => x.PendaftaranPasienBaruId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                name: "MstPosition",
                schema: "public",
                columns: table => new
                {
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionCode = table.Column<string>(type: "text", nullable: false),
                    PositionName = table.Column<string>(type: "text", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstPosition", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_MstPosition_MstDepartement_DepartementId",
                        column: x => x.DepartementId,
                        principalSchema: "public",
                        principalTable: "MstDepartement",
                        principalColumn: "DepartementId");
                });

            migrationBuilder.CreateTable(
                name: "MstDokterPraktek",
                schema: "public",
                columns: table => new
                {
                    DokterPraktekId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dokter = table.Column<string>(type: "text", nullable: false),
                    Layanan = table.Column<string>(type: "text", nullable: false),
                    JamPraktek = table.Column<string>(type: "text", nullable: false),
                    Hari = table.Column<string>(type: "text", nullable: false),
                    JamMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JamKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstDokterPraktek", x => x.DokterPraktekId);
                    table.ForeignKey(
                        name: "FK_MstDokterPraktek_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstPeralatan",
                schema: "public",
                columns: table => new
                {
                    PeralatanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePeralatan = table.Column<string>(type: "text", nullable: false),
                    NamaPeralatan = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    Purchase_date = table.Column<string>(type: "text", nullable: false),
                    Maintenance_status = table.Column<string>(type: "text", nullable: false),
                    Operational_status = table.Column<string>(type: "text", nullable: false),
                    Department_name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    KategoriPeralatanId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstPeralatan", x => x.PeralatanId);
                    table.ForeignKey(
                        name: "FK_MstPeralatan_MstKategoriPeralatan_KategoriPeralatanId",
                        column: x => x.KategoriPeralatanId,
                        principalSchema: "public",
                        principalTable: "MstKategoriPeralatan",
                        principalColumn: "KategoriPeralatanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstProvinsi",
                schema: "public",
                columns: table => new
                {
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeProvinsi = table.Column<string>(type: "text", nullable: false),
                    NamaProvinsi = table.Column<string>(type: "text", nullable: false),
                    NegaraId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstProvinsi", x => x.ProvinsiId);
                    table.ForeignKey(
                        name: "FK_MstProvinsi_MstNegara_NegaraId",
                        column: x => x.NegaraId,
                        principalSchema: "public",
                        principalTable: "MstNegara",
                        principalColumn: "NegaraId");
                });

            migrationBuilder.CreateTable(
                name: "MstKabupatenKota",
                schema: "public",
                columns: table => new
                {
                    KabupatenKotaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKabupatenKota = table.Column<string>(type: "text", nullable: false),
                    NamaKabupatenKota = table.Column<string>(type: "text", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstKabupatenKota", x => x.KabupatenKotaId);
                    table.ForeignKey(
                        name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "public",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId");
                });

            migrationBuilder.CreateTable(
                name: "MstKecamatan",
                schema: "public",
                columns: table => new
                {
                    KecamatanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKecamatan = table.Column<string>(type: "text", nullable: false),
                    NamaKecamatan = table.Column<string>(type: "text", nullable: false),
                    KabupatenKotaId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstKecamatan", x => x.KecamatanId);
                    table.ForeignKey(
                        name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                        column: x => x.KabupatenKotaId,
                        principalSchema: "public",
                        principalTable: "MstKabupatenKota",
                        principalColumn: "KabupatenKotaId");
                });

            migrationBuilder.CreateTable(
                name: "MstKelurahan",
                schema: "public",
                columns: table => new
                {
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKelurahan = table.Column<string>(type: "text", nullable: false),
                    NamaKelurahan = table.Column<string>(type: "text", nullable: false),
                    KecamatanId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstKelurahan", x => x.KelurahanId);
                    table.ForeignKey(
                        name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                        column: x => x.KecamatanId,
                        principalSchema: "public",
                        principalTable: "MstKecamatan",
                        principalColumn: "KecamatanId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterPraktek_DokterId",
                schema: "public",
                table: "MstDokterPraktek",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupatenKota_ProvinsiId",
                schema: "public",
                table: "MstKabupatenKota",
                column: "ProvinsiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKecamatan_KabupatenKotaId",
                schema: "public",
                table: "MstKecamatan",
                column: "KabupatenKotaId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKelurahan_KecamatanId",
                schema: "public",
                table: "MstKelurahan",
                column: "KecamatanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPeralatan_KategoriPeralatanId",
                schema: "public",
                table: "MstPeralatan",
                column: "KategoriPeralatanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPosition_DepartementId",
                schema: "public",
                table: "MstPosition",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProvinsi_NegaraId",
                schema: "public",
                table: "MstProvinsi",
                column: "NegaraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetGroupRole",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRole",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "MstAgama",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDokterPraktek",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstGolonganDarah",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstIdentitas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJabatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKeanggotaan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKelurahan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPekerjaan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPendidikan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPeralatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPersalinan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPoliklinik",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPosition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTitle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstUserActive",
                schema: "public");

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
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKecamatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKategoriPeralatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDepartement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKabupatenKota",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProvinsi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNegara",
                schema: "public");
        }
    }
}
