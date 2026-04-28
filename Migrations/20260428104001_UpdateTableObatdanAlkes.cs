using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UpdateTableObatdanAlkes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SatuanId",
                schema: "public",
                table: "MstKonversiSatuan",
                newName: "SatuanKecilId");

            migrationBuilder.RenameColumn(
                name: "ObatId",
                schema: "public",
                table: "MstKonversiSatuan",
                newName: "SatuanBesarId");

            migrationBuilder.AddColumn<Guid>(
                name: "ObatAlkesId",
                schema: "public",
                table: "MstKonversiSatuan",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstGroupObatAlkes",
                schema: "public",
                columns: table => new
                {
                    GroupObatAlkesId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaGroupObatAlkes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstGroupObatAlkes", x => x.GroupObatAlkesId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatAlkes",
                schema: "public",
                columns: table => new
                {
                    ObatAlkesId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeObatAlkes = table.Column<string>(type: "text", nullable: true),
                    GroupObatAlkesId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaObatAlkes = table.Column<string>(type: "text", nullable: false),
                    KategoriTerapeutikId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubKategoriTerapeutikId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    HighAlert = table.Column<bool>(type: "boolean", nullable: true),
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dosis = table.Column<decimal>(type: "numeric", nullable: true),
                    Etiket = table.Column<string>(type: "text", nullable: true),
                    KodeKFAId = table.Column<Guid>(type: "uuid", nullable: true),
                    BZA = table.Column<string>(type: "text", nullable: true),
                    POV = table.Column<string>(type: "text", nullable: true),
                    POAK = table.Column<string>(type: "text", nullable: true),
                    ObatRuteId = table.Column<Guid>(type: "uuid", nullable: true),
                    KekuatanSediaan = table.Column<decimal>(type: "numeric", nullable: true),
                    VolumeSediaan = table.Column<decimal>(type: "numeric", nullable: true),
                    BentukSediaan = table.Column<decimal>(type: "numeric", nullable: true),
                    KomoditasId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockMinimal = table.Column<decimal>(type: "numeric", nullable: true),
                    StockMaximal = table.Column<decimal>(type: "numeric", nullable: true),
                    BentukObatAlkesId = table.Column<Guid>(type: "uuid", nullable: true),
                    GolonganObatAlkesId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstObatAlkes", x => x.ObatAlkesId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstGroupObatAlkes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatAlkes",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "ObatAlkesId",
                schema: "public",
                table: "MstKonversiSatuan");

            migrationBuilder.RenameColumn(
                name: "SatuanKecilId",
                schema: "public",
                table: "MstKonversiSatuan",
                newName: "SatuanId");

            migrationBuilder.RenameColumn(
                name: "SatuanBesarId",
                schema: "public",
                table: "MstKonversiSatuan",
                newName: "ObatId");
        }
    }
}
