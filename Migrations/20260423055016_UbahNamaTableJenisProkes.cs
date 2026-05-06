using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaTableJenisProkes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstJenisProdukObat",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.CreateTable(
                name: "MstJenisProkes",
                schema: "public",
                columns: table => new
                {
                    JenisProkesId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaJenisProkes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstJenisProkes", x => x.JenisProkesId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstJenisProkes",
                schema: "public");

            migrationBuilder.AddColumn<string>(
                name: "FotoName",
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

            migrationBuilder.CreateTable(
                name: "MstJenisProdukObat",
                schema: "public",
                columns: table => new
                {
                    JenisProdukObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    NamaJenisProdukObat = table.Column<string>(type: "text", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstJenisProdukObat", x => x.JenisProdukObatId);
                });
        }
    }
}
