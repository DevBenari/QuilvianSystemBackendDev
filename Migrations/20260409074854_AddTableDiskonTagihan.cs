using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableDiskonTagihan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KategoriTindakan",
                schema: "public",
                table: "MstTarifKelas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoName",
                schema: "public",
                table: "Hrd_Karyawan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "Hrd_Karyawan",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiskonTagihans",
                columns: table => new
                {
                    DiskonTagihanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaDiskon = table.Column<string>(type: "text", nullable: true),
                    ValueDiskon = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_DiskonTagihans", x => x.DiskonTagihanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskonTagihans");

            migrationBuilder.DropColumn(
                name: "KategoriTindakan",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "Hrd_Karyawan");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "Hrd_Karyawan");
        }
    }
}
