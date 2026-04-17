using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableMasterBarang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstBarang",
                schema: "public",
                columns: table => new
                {
                    BarangId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBarang = table.Column<string>(type: "text", nullable: true),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaBarang = table.Column<string>(type: "text", nullable: true),
                    KategoriBarangId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasResikoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Spesifikasi = table.Column<string>(type: "text", nullable: true),
                    IsPerluResep = table.Column<bool>(type: "boolean", nullable: true),
                    StokMinimum = table.Column<decimal>(type: "numeric", nullable: true),
                    StokMaximum = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBarang", x => x.BarangId);
                });

            migrationBuilder.CreateTable(
                name: "MstBarangHarga",
                schema: "public",
                columns: table => new
                {
                    HargaBarangId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarangId = table.Column<Guid>(type: "uuid", nullable: true),
                    HteHargaBarang = table.Column<decimal>(type: "numeric", nullable: true),
                    HneHargaBarang = table.Column<decimal>(type: "numeric", nullable: true),
                    TglBerlaku = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglBerakhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBarangHarga", x => x.HargaBarangId);
                });

            migrationBuilder.CreateTable(
                name: "MstBarangKategori",
                schema: "public",
                columns: table => new
                {
                    KategoriBarangId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKategoriBarang = table.Column<string>(type: "text", nullable: true),
                    NamaKategoriBarang = table.Column<string>(type: "text", nullable: true),
                    GrupKategoriBarang = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBarangKategori", x => x.KategoriBarangId);
                });

            migrationBuilder.CreateTable(
                name: "MstBarangStok",
                schema: "public",
                columns: table => new
                {
                    StokBarangId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarangId = table.Column<Guid>(type: "uuid", nullable: true),
                    LokasiPenyimpananId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyStokBarang = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBarangStok", x => x.StokBarangId);
                });

            migrationBuilder.CreateTable(
                name: "MstBrand",
                schema: "public",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBrand = table.Column<string>(type: "text", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaBrand = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBrand", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "MstKelasResiko",
                schema: "public",
                columns: table => new
                {
                    KelasResikoId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKelasResiko = table.Column<string>(type: "text", nullable: true),
                    NamaKelasResiko = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstKelasResiko", x => x.KelasResikoId);
                });

            migrationBuilder.CreateTable(
                name: "MstLantai",
                schema: "public",
                columns: table => new
                {
                    LantaiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBarang = table.Column<string>(type: "text", nullable: true),
                    NamaLantai = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstLantai", x => x.LantaiId);
                });

            migrationBuilder.CreateTable(
                name: "MstLokasiPenyimpanan",
                schema: "public",
                columns: table => new
                {
                    LokasiPenyimpananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeLokasiPenyimpanan = table.Column<string>(type: "text", nullable: true),
                    NamaLokasi = table.Column<string>(type: "text", nullable: true),
                    LantaiId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstLokasiPenyimpanan", x => x.LokasiPenyimpananId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstBarang",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBarangHarga",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBarangKategori",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBarangStok",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBrand",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKelasResiko",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstLantai",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstLokasiPenyimpanan",
                schema: "public");
        }
    }
}
