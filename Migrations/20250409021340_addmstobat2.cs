using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addmstobat2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstCoveranObatAsuransi",
                schema: "public",
                columns: table => new
                {
                    CoveranObatAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeCoveranObat = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstCoveranObatAsuransi", x => x.CoveranObatAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstObat",
                schema: "public",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "text", nullable: false),
                    ProductExtCode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Barcode = table.Column<string>(type: "text", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    KategoryObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKategoriObat = table.Column<string>(type: "text", nullable: true),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    MeasurementName = table.Column<string>(type: "text", nullable: true),
                    WarehouseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseLocationName = table.Column<string>(type: "text", nullable: true),
                    DiscountId = table.Column<string>(type: "text", nullable: true),
                    DiscountValue = table.Column<string>(type: "text", nullable: true),
                    ExpiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DosageStrength = table.Column<string>(type: "text", nullable: true),
                    DosageVolume = table.Column<string>(type: "text", nullable: true),
                    DosageForm = table.Column<string>(type: "text", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    Cogs = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    RetailPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    StorageLocation = table.Column<string>(type: "text", nullable: false),
                    RackNumber = table.Column<string>(type: "text", nullable: false),
                    IsSupplierUtama = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstObat", x => x.ProductId);
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstCoveranObatAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSupplier",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTermOfPayment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstWarehouseLocation",
                schema: "public");
        }
    }
}
