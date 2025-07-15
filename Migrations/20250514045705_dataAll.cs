using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class dataAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

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
                name: "CoveranTindakanAsuransi",
                schema: "public",
                columns: table => new
                {
                    CoveranTindakanAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokterAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRsAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJpAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahpAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLainAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotalAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    KSOAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_CoveranTindakanAsuransi", x => x.CoveranTindakanAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "DokterPolis",
                columns: table => new
                {
                    DokterPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "MstAgama",
                schema: "public",
                columns: table => new
                {
                    AgamaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeAgama = table.Column<string>(type: "text", nullable: true),
                    NamaAgama = table.Column<string>(type: "text", nullable: true),
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
                    TanggalRegist = table.Column<string>(type: "text", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    JenisAsuransi = table.Column<string>(type: "text", nullable: true),
                    KategoriAsuransi = table.Column<string>(type: "text", nullable: true),
                    StatusAsuransi = table.Column<string>(type: "text", nullable: true),
                    TanggalMulaiKerjasama = table.Column<string>(type: "text", nullable: true),
                    TanggalAkhirKerjasama = table.Column<string>(type: "text", nullable: true),
                    MetodeKlaim = table.Column<string>(type: "text", nullable: true),
                    BatasMaxKlaimPerTahun = table.Column<int>(type: "integer", nullable: true),
                    BatasMaxKlaimPerKunjungan = table.Column<int>(type: "integer", nullable: true),
                    PersentasiBiayaPertanggungan = table.Column<decimal>(type: "numeric", nullable: true),
                    TambahanTanggungan = table.Column<int>(type: "integer", nullable: true),
                    NoRekRumahSakit = table.Column<string>(type: "text", nullable: true),
                    NamaBank = table.Column<string>(type: "text", nullable: true),
                    TermOfPayment = table.Column<string>(type: "text", nullable: true),
                    NamaPerusahaanAsuransi = table.Column<string>(type: "text", nullable: true),
                    NoTelepon = table.Column<string>(type: "text", nullable: true),
                    EmailPusat = table.Column<string>(type: "text", nullable: true),
                    IsPKS = table.Column<bool>(type: "boolean", nullable: true),
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
                name: "MstAsuransiPasien",
                schema: "public",
                columns: table => new
                {
                    AsuransiPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<string>(type: "text", nullable: true),
                    NoPolis = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<string>(type: "text", nullable: true),
                    Umur = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstAsuransiPasien", x => x.AsuransiPasienId);
                });

            migrationBuilder.CreateTable(
                name: "MstBentukObat",
                schema: "public",
                columns: table => new
                {
                    BentukObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBentukObat = table.Column<string>(type: "text", nullable: false),
                    NamaBentukObat = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstBentukObat", x => x.BentukObatId);
                });

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
                    TglBerlaku = table.Column<string>(type: "text", nullable: true),
                    TglBerakhir = table.Column<string>(type: "text", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "MstCoveranObatAsuransi",
                schema: "public",
                columns: table => new
                {
                    CoveranObatAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    KategoriObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKategoriObat = table.Column<string>(type: "text", nullable: true),
                    HargaRetail = table.Column<decimal>(type: "numeric", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    PersentaseDiskon = table.Column<int>(type: "integer", nullable: true),
                    TarifObatAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    Kelas = table.Column<string>(type: "text", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstCoveranObatAsuransi", x => x.CoveranObatAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstCurrentMedication",
                schema: "public",
                columns: table => new
                {
                    CurrentMedicationID = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: true),
                    NamaObat = table.Column<string>(type: "text", nullable: true),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    Frekuensi = table.Column<string>(type: "text", nullable: true),
                    LamaKonsumsi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstCurrentMedication", x => x.CurrentMedicationID);
                });

            migrationBuilder.CreateTable(
                name: "MstDepartement",
                schema: "public",
                columns: table => new
                {
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeDepartement = table.Column<string>(type: "text", nullable: false),
                    NamaDepartement = table.Column<string>(type: "text", nullable: true),
                    KepalaDepartement = table.Column<string>(type: "text", nullable: true),
                    Lokasi = table.Column<string>(type: "text", nullable: true),
                    Telepon = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    JamBuka = table.Column<string>(type: "text", nullable: true),
                    JamTutup = table.Column<string>(type: "text", nullable: true),
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
                name: "MstDetailICD",
                schema: "public",
                columns: table => new
                {
                    DetailICDId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoapId = table.Column<Guid>(type: "uuid", nullable: true),
                    ICDId = table.Column<Guid>(type: "uuid", nullable: true),
                    isUtama = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstDetailICD", x => x.DetailICDId);
                });

            migrationBuilder.CreateTable(
                name: "MstDetailResep",
                schema: "public",
                columns: table => new
                {
                    DetailResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDetailResep", x => x.DetailResepId);
                });

            migrationBuilder.CreateTable(
                name: "MstDiscount",
                schema: "public",
                columns: table => new
                {
                    DiscountId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeDiscount = table.Column<string>(type: "text", nullable: false),
                    DiscountValue = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDiscount", x => x.DiscountId);
                });

            migrationBuilder.CreateTable(
                name: "MstDokter",
                schema: "public",
                columns: table => new
                {
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    KdDokter = table.Column<string>(type: "text", nullable: false),
                    NmDokter = table.Column<string>(type: "text", nullable: false),
                    Sip = table.Column<string>(type: "text", nullable: true),
                    Str = table.Column<string>(type: "text", nullable: true),
                    Spesialis = table.Column<string>(type: "text", nullable: true),
                    TglSip = table.Column<string>(type: "text", nullable: true),
                    TglStr = table.Column<string>(type: "text", nullable: true),
                    Nik = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Nohp = table.Column<string>(type: "text", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    IsAsuransi = table.Column<bool>(type: "boolean", nullable: true),
                    FotoName = table.Column<string>(type: "text", nullable: true),
                    FotoPath = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
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
                name: "MstDokterAsuransi",
                schema: "public",
                columns: table => new
                {
                    DokterAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "MstICD-10",
                schema: "public",
                columns: table => new
                {
                    ICDId = table.Column<Guid>(type: "uuid", nullable: false),
                    ICDCode = table.Column<string>(type: "text", nullable: true),
                    ICDName = table.Column<string>(type: "text", nullable: true),
                    DTDCode = table.Column<string>(type: "text", nullable: true),
                    NamaDiagnosa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstICD-10", x => x.ICDId);
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
                name: "MstKandungan",
                schema: "public",
                columns: table => new
                {
                    KandunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKandungan = table.Column<string>(type: "text", nullable: false),
                    NamaKandungan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstKandungan", x => x.KandunganId);
                });

            migrationBuilder.CreateTable(
                name: "MstKategoriObat",
                schema: "public",
                columns: table => new
                {
                    KategoriObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKategoriObat = table.Column<string>(type: "text", nullable: false),
                    CategoryExtGroupCode = table.Column<string>(type: "text", nullable: false),
                    NamaKategoriObat = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstKategoriObat", x => x.KategoriObatId);
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
                name: "MstKelas",
                schema: "public",
                columns: table => new
                {
                    KelasId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKelas = table.Column<string>(type: "text", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    DeskripsiKelas = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstKelas", x => x.KelasId);
                });

            migrationBuilder.CreateTable(
                name: "MstKunjungan",
                schema: "public",
                columns: table => new
                {
                    KunjunganID = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    TipePasien = table.Column<string>(type: "text", nullable: true),
                    TipePembayaran = table.Column<string>(type: "text", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: true),
                    JenisKunjungan = table.Column<string>(type: "text", nullable: false),
                    Antrian = table.Column<string>(type: "text", nullable: true),
                    IsScreening = table.Column<bool>(type: "boolean", nullable: true),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstKunjungan", x => x.KunjunganID);
                });

            migrationBuilder.CreateTable(
                name: "MstMeasurement",
                schema: "public",
                columns: table => new
                {
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeMeasurement = table.Column<string>(type: "text", nullable: false),
                    NamaMeasurement = table.Column<string>(type: "text", nullable: false),
                    MeasurementExtCode = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstMeasurement", x => x.MeasurementId);
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
                name: "MstObat",
                schema: "public",
                columns: table => new
                {
                    ObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatCode = table.Column<string>(type: "text", nullable: true),
                    ObatName = table.Column<string>(type: "text", nullable: false),
                    JumlahSatuan = table.Column<string>(type: "text", nullable: true),
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    BentukObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    HargaJual = table.Column<decimal>(type: "numeric", nullable: false),
                    HargaAwal = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    Minimal = table.Column<int>(type: "integer", nullable: true),
                    Maximal = table.Column<int>(type: "integer", nullable: true),
                    Farmakologi = table.Column<string>(type: "text", nullable: true),
                    Peringatan = table.Column<string>(type: "text", nullable: true),
                    Indikasi = table.Column<string>(type: "text", nullable: true),
                    Kontraindikasi = table.Column<string>(type: "text", nullable: true),
                    CaraKerja = table.Column<string>(type: "text", nullable: true),
                    InteraksiObat = table.Column<string>(type: "text", nullable: true),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstObat", x => x.ObatId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatAsuransi",
                schema: "public",
                columns: table => new
                {
                    ObatAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstObatAsuransi", x => x.ObatAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatKandungan",
                schema: "public",
                columns: table => new
                {
                    ObatKandunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KandunganId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstObatKandungan", x => x.ObatKandunganId);
                });

            migrationBuilder.CreateTable(
                name: "MstOperasi",
                schema: "public",
                columns: table => new
                {
                    OperasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeOperasi = table.Column<string>(type: "text", nullable: false),
                    JenisOperasi = table.Column<string>(type: "text", nullable: false),
                    TipeOperasi = table.Column<string>(type: "text", nullable: false),
                    NamaTindakanOperasi = table.Column<string>(type: "text", nullable: false),
                    TanggalOperasi = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusOperasi = table.Column<string>(type: "text", nullable: false),
                    LamaOperasi = table.Column<int>(type: "integer", nullable: false),
                    RuanganOperasi = table.Column<string>(type: "text", nullable: false),
                    LokasiRuanganOperasi = table.Column<string>(type: "text", nullable: false),
                    TipeCCVC = table.Column<bool>(type: "boolean", nullable: false),
                    CatatanMedis = table.Column<string>(type: "text", nullable: true),
                    NamaDokterOperator = table.Column<string>(type: "text", nullable: false),
                    NamaDokterAnastesi = table.Column<string>(type: "text", nullable: false),
                    DokterTambahan1 = table.Column<string>(type: "text", nullable: true),
                    DokterTambahan2 = table.Column<string>(type: "text", nullable: true),
                    DokterTambahan3 = table.Column<string>(type: "text", nullable: true),
                    DokterTambahan4 = table.Column<string>(type: "text", nullable: true),
                    DokterTambahan5 = table.Column<string>(type: "text", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    KeluhanOperasi = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstOperasi", x => x.OperasiId);
                });

            migrationBuilder.CreateTable(
                name: "MstPainAssessment",
                schema: "public",
                columns: table => new
                {
                    PainAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    KeluhanUtama = table.Column<string>(type: "text", nullable: true),
                    IsPain = table.Column<bool>(type: "boolean", nullable: true),
                    Pemicu = table.Column<string>(type: "text", nullable: true),
                    Kualitas = table.Column<string>(type: "text", nullable: true),
                    Lokasi = table.Column<string>(type: "text", nullable: true),
                    SkalaPainId = table.Column<Guid>(type: "uuid", nullable: true),
                    Frekuensi = table.Column<string>(type: "text", nullable: true),
                    PainManagement = table.Column<string>(type: "text", nullable: true),
                    IsInheritedDisease = table.Column<bool>(type: "boolean", nullable: true),
                    InheritedDisease = table.Column<string>(type: "text", nullable: true),
                    IsAlergic = table.Column<bool>(type: "boolean", nullable: true),
                    Alergic = table.Column<string>(type: "text", nullable: true),
                    NafsuMakan = table.Column<string>(type: "text", nullable: true),
                    IsMual = table.Column<bool>(type: "boolean", nullable: true),
                    IsMuntah = table.Column<bool>(type: "boolean", nullable: true),
                    IsFallRisk = table.Column<bool>(type: "boolean", nullable: true),
                    FallRisk = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstPainAssessment", x => x.PainAssessmentId);
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
                    TanggalPersalinan = table.Column<DateOnly>(type: "date", nullable: true),
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
                    TTLBayi = table.Column<DateOnly>(type: "date", nullable: true),
                    BeratBayi = table.Column<string>(type: "text", nullable: false),
                    PanjangBayi = table.Column<string>(type: "text", nullable: false),
                    NamaAyah = table.Column<string>(type: "text", nullable: false),
                    NamaIbu = table.Column<string>(type: "text", nullable: false),
                    StatusBayi = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstPersalinan", x => x.PersalinanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPoliklinik",
                schema: "public",
                columns: table => new
                {
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePoliklinik = table.Column<string>(type: "text", nullable: false),
                    KodeAntreanPoli = table.Column<string>(type: "text", nullable: false),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: false),
                    KepalaPoliklinik = table.Column<string>(type: "text", nullable: false),
                    Ruang = table.Column<string>(type: "text", nullable: false),
                    Telepon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    HariOperasional = table.Column<string>(type: "text", nullable: false),
                    JamBuka = table.Column<TimeSpan>(type: "interval", nullable: true),
                    JamTutup = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LayananPoliklinik = table.Column<string>(type: "text", nullable: true),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    JumlahMaxPasien = table.Column<int>(type: "integer", nullable: false),
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
                name: "MstResep",
                schema: "public",
                columns: table => new
                {
                    ResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstResep", x => x.ResepId);
                });

            migrationBuilder.CreateTable(
                name: "MstResepTemplate",
                schema: "public",
                columns: table => new
                {
                    ResepTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeResepTemplate = table.Column<string>(type: "text", nullable: true),
                    Judul = table.Column<string>(type: "text", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstResepTemplate", x => x.ResepTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "MstSatuan",
                schema: "public",
                columns: table => new
                {
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeSatuan = table.Column<string>(type: "text", nullable: false),
                    NamaSatuan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstSatuan", x => x.SatuanId);
                });

            migrationBuilder.CreateTable(
                name: "MstSkalaPain",
                schema: "public",
                columns: table => new
                {
                    SkalaPainId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeSkalaPain = table.Column<string>(type: "text", nullable: false),
                    ScoreSkalaPain = table.Column<string>(type: "text", nullable: true),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    KategoriSkala = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSkalaPain", x => x.SkalaPainId);
                });

            migrationBuilder.CreateTable(
                name: "MstSOAP",
                schema: "public",
                columns: table => new
                {
                    SOAPID = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subjective = table.Column<string>(type: "text", nullable: true),
                    Objective = table.Column<string>(type: "text", nullable: true),
                    Assessment = table.Column<string>(type: "text", nullable: true),
                    Planning = table.Column<string>(type: "text", nullable: true),
                    Profesi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSOAP", x => x.SOAPID);
                });

            migrationBuilder.CreateTable(
                name: "MstSupplier",
                schema: "public",
                columns: table => new
                {
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierCode = table.Column<string>(type: "text", nullable: false),
                    SupplierName = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    TermOfPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TermOfPaymentName = table.Column<string>(type: "text", nullable: true),
                    Ppn = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    Telepon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsPKS = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstSupplier", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "MstTarifKelas",
                schema: "public",
                columns: table => new
                {
                    TarifKelasId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstTarifKelas", x => x.TarifKelasId);
                });

            migrationBuilder.CreateTable(
                name: "MstTermOfPayment",
                schema: "public",
                columns: table => new
                {
                    TermOfPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermOfPaymentCode = table.Column<string>(type: "text", nullable: false),
                    TermOfPaymentName = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstTermOfPayment", x => x.TermOfPaymentId);
                });

            migrationBuilder.CreateTable(
                name: "MstTindakan",
                schema: "public",
                columns: table => new
                {
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeTindakan = table.Column<string>(type: "text", nullable: false),
                    NamaTindakan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstTindakan", x => x.TindakanId);
                });

            migrationBuilder.CreateTable(
                name: "MstTindakanAsuransi",
                schema: "public",
                columns: table => new
                {
                    TindakanAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstTindakanAsuransi", x => x.TindakanAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstTindakanPoli",
                schema: "public",
                columns: table => new
                {
                    TindakanPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstTindakanPoli", x => x.TindakanPoliId);
                });

            migrationBuilder.CreateTable(
                name: "MstTipeUser",
                schema: "public",
                columns: table => new
                {
                    TipeUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeTipeUser = table.Column<string>(type: "text", nullable: false),
                    NamaTipeUser = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstTipeUser", x => x.TipeUserId);
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
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Handphone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FotoName = table.Column<string>(type: "text", nullable: true),
                    FotoPath = table.Column<string>(type: "text", nullable: true),
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
                name: "MstVitalSign",
                schema: "public",
                columns: table => new
                {
                    VitalSignId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    Suhu = table.Column<decimal>(type: "numeric", nullable: true),
                    HR = table.Column<int>(type: "integer", nullable: true),
                    RR = table.Column<int>(type: "integer", nullable: true),
                    TekananDarahSystolic = table.Column<int>(type: "integer", nullable: true),
                    TekananDarahDiastolic = table.Column<int>(type: "integer", nullable: true),
                    SaturasiOksigen = table.Column<decimal>(type: "numeric", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    BMI = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstVitalSign", x => x.VitalSignId);
                });

            migrationBuilder.CreateTable(
                name: "MstWarehouseLocation",
                schema: "public",
                columns: table => new
                {
                    WarehouseLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseLocationCode = table.Column<string>(type: "text", nullable: false),
                    WarehouseLocationName = table.Column<string>(type: "text", nullable: false),
                    WarehouseManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseManagerName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstWarehouseLocation", x => x.WarehouseLocationId);
                });

            migrationBuilder.CreateTable(
                name: "PdfPasien",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    NamaLengkap = table.Column<string>(type: "text", nullable: false),
                    NoIdentitas = table.Column<string>(type: "text", nullable: false),
                    TempatLahir = table.Column<string>(type: "text", nullable: false),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
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
                name: "PdfPasienAmbulan",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienAmbulanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePdfPasienAmbulan = table.Column<string>(type: "text", nullable: false),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    AlamatPasien = table.Column<string>(type: "text", nullable: true),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    LayananAmbulan = table.Column<string>(type: "text", nullable: true),
                    DaerahTujuan = table.Column<string>(type: "text", nullable: true),
                    KelebihanJarak = table.Column<int>(type: "integer", nullable: true),
                    KelebihanWaktu = table.Column<int>(type: "integer", nullable: true),
                    JumlahParamedis = table.Column<int>(type: "integer", nullable: true),
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
                name: "PdfPasienBaru",
                schema: "public",
                columns: table => new
                {
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePasien = table.Column<string>(type: "text", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: true),
                    TipePasien = table.Column<string>(type: "text", nullable: true),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaLengkap = table.Column<string>(type: "text", nullable: false),
                    IdentitasId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoIdentitas = table.Column<string>(type: "text", nullable: false),
                    TempatLahir = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    StatusPerkawinan = table.Column<string>(type: "text", nullable: true),
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
                    NoPasien = table.Column<string>(type: "text", nullable: true),
                    NoWali2 = table.Column<string>(type: "text", nullable: true),
                    NoWali3 = table.Column<string>(type: "text", nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "text", nullable: true),
                    Suku = table.Column<string>(type: "text", nullable: true),
                    StatusKewarganegaraan = table.Column<string>(type: "text", nullable: true),
                    PekerjaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPerusahaan = table.Column<string>(type: "text", nullable: true),
                    AlamatPerusahaan = table.Column<string>(type: "text", nullable: true),
                    NoTeleponPerusahaan = table.Column<string>(type: "text", nullable: true),
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
                    FotoName = table.Column<string>(type: "text", nullable: true),
                    FotoPath = table.Column<string>(type: "text", nullable: true),
                    QrCode = table.Column<string>(type: "text", nullable: true),
                    QrCodeImage = table.Column<byte[]>(type: "bytea", nullable: true),
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
                    NamaPasien = table.Column<string>(type: "text", nullable: true),
                    AlamatPasien = table.Column<string>(type: "text", nullable: true),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: true),
                    KabupatenKota = table.Column<string>(type: "text", nullable: true),
                    Kecamatan = table.Column<string>(type: "text", nullable: true),
                    TipePasien = table.Column<string>(type: "text", nullable: true),
                    Asuransi = table.Column<string>(type: "text", nullable: true),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: true),
                    KodeMember = table.Column<string>(type: "text", nullable: true),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: true),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: true),
                    TipeRujukan = table.Column<string>(type: "text", nullable: true),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: true),
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
                    NoRekamMedis = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateOnly>(type: "date", nullable: true),
                    TanggalPendaftaran = table.Column<DateOnly>(type: "date", nullable: true),
                    NamaPasien = table.Column<string>(type: "text", nullable: true),
                    AlamatPasien = table.Column<string>(type: "text", nullable: true),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: true),
                    KabupatenKota = table.Column<string>(type: "text", nullable: true),
                    Kecamatan = table.Column<string>(type: "text", nullable: true),
                    TipePasien = table.Column<string>(type: "text", nullable: true),
                    Asuransi = table.Column<string>(type: "text", nullable: true),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: true),
                    KodeMember = table.Column<string>(type: "text", nullable: true),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: true),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: true),
                    TipeRujukan = table.Column<string>(type: "text", nullable: true),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: true),
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
                    NamaPasien = table.Column<string>(type: "text", nullable: true),
                    AlamatPasien = table.Column<string>(type: "text", nullable: true),
                    NoTelpPasien = table.Column<string>(type: "text", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Provinsi = table.Column<string>(type: "text", nullable: true),
                    KabupatenKota = table.Column<string>(type: "text", nullable: true),
                    Kecamatan = table.Column<string>(type: "text", nullable: true),
                    TipePasien = table.Column<string>(type: "text", nullable: true),
                    Asuransi = table.Column<string>(type: "text", nullable: true),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: true),
                    KodeMember = table.Column<string>(type: "text", nullable: true),
                    TipePemeriksaan = table.Column<string>(type: "text", nullable: true),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: true),
                    TipeRujukan = table.Column<string>(type: "text", nullable: true),
                    JenisKonsul = table.Column<string>(type: "text", nullable: true),
                    NamaRSRujukan = table.Column<string>(type: "text", nullable: true),
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
                    Umur = table.Column<int>(type: "integer", nullable: true),
                    NoTelp = table.Column<string>(type: "text", nullable: true),
                    NamaDokterUGD = table.Column<string>(type: "text", nullable: true),
                    Diagnosa = table.Column<string>(type: "text", nullable: true),
                    Tindakan = table.Column<string>(type: "text", nullable: true),
                    BiayaAdmin = table.Column<decimal>(type: "numeric", nullable: true),
                    Kelas = table.Column<string>(type: "text", nullable: true),
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
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    NoTelp = table.Column<string>(type: "text", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    DokterOptik = table.Column<string>(type: "text", nullable: true),
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
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    NoTelepon = table.Column<string>(type: "text", nullable: true),
                    DokterPemeriksa = table.Column<string>(type: "text", nullable: true),
                    NamaFasilitasPasien = table.Column<string>(type: "text", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "TindakanKunjungans",
                columns: table => new
                {
                    TindakanKunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: true),
                    Disposition = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TindakanKunjungans", x => x.TindakanKunjunganId);
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
                    JamBuka = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    JamTutup = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    LayananSubPoli = table.Column<string>(type: "text", nullable: true),
                    JumlahMaxPasien = table.Column<int>(type: "integer", nullable: false),
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
                name: "MstDokterSubPoli",
                schema: "public",
                columns: table => new
                {
                    DokterSubPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaDokter = table.Column<string>(type: "text", nullable: false),
                    KodeDokterSubPoli = table.Column<string>(type: "text", nullable: true),
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSubPoli = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDokterSubPoli", x => x.DokterSubPoliId);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "public",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstSubPoli_SubPoliId",
                        column: x => x.SubPoliId,
                        principalSchema: "public",
                        principalTable: "MstSubPoli",
                        principalColumn: "SubPoliId");
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
                name: "MstJadwalPraktek",
                schema: "public",
                columns: table => new
                {
                    JadwalPraktekId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeJadwalPraktek = table.Column<string>(type: "text", nullable: false),
                    WaktuPraktek = table.Column<string>(type: "text", nullable: false),
                    HariPraktek = table.Column<string>(type: "text", nullable: false),
                    JamMulai = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    JamBerakhir = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DokterSubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                        column: x => x.DokterSubPoliId,
                        principalSchema: "public",
                        principalTable: "MstDokterSubPoli",
                        principalColumn: "DokterSubPoliId");
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

            migrationBuilder.CreateTable(
                name: "MstKodePos",
                schema: "public",
                columns: table => new
                {
                    KodePosId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniqueKodePos = table.Column<string>(type: "text", nullable: false),
                    NamaKodePos = table.Column<string>(type: "text", nullable: false),
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstKodePos", x => x.KodePosId);
                    table.ForeignKey(
                        name: "FK_MstKodePos_MstKelurahan_KelurahanId",
                        column: x => x.KelurahanId,
                        principalSchema: "public",
                        principalTable: "MstKelurahan",
                        principalColumn: "KelurahanId");
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
                name: "IX_MstDokterSubPoli_AsuransiId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_DokterId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_SubPoliId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "SubPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId");

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
                name: "IX_MstKodePos_KelurahanId",
                schema: "public",
                table: "MstKodePos",
                column: "KelurahanId");

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

            migrationBuilder.CreateIndex(
                name: "IX_MstSubPoli_PoliId",
                schema: "public",
                table: "MstSubPoli",
                column: "PoliId");
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
                name: "CoveranTindakanAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAgama",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAsuransiPasien",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBentukObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCoveranAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCoveranObatAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCurrentMedication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDetailICD",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDetailResep",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDiscount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDokterAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstFasilitasPasien",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstGolonganDarah",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstICD-10",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstIdentitas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJabatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJadwalPraktek",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKandungan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKategoriObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKeanggotaan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKelas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKodePos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKunjungan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMeasurement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatKandungan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstOperasi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPainAssessment",
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
                name: "MstPosition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstResep",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstResepTemplate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSatuan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSkalaPain",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSOAP",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSupplier",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTarifKelas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTermOfPayment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTindakan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTindakanAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTindakanPoli",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTipeUser",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTitle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstUserActive",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstVitalSign",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstWarehouseLocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasien",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienAmbulan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PdfPasienBaru",
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

            migrationBuilder.DropTable(
                name: "TindakanKunjungans");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DokterPolis");

            migrationBuilder.DropTable(
                name: "MstDokterSubPoli",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKelurahan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKategoriPeralatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDepartement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDokter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSubPoli",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKecamatan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPoliklinik",
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
