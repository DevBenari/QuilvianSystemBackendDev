using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class jnsuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adm_JenisPembayaran",
                schema: "public",
                columns: table => new
                {
                    JenisPembayaranId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPembayaran = table.Column<string>(type: "text", nullable: false),
                    NominalDefault = table.Column<int>(type: "integer", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    bln = table.Column<string>(type: "text", nullable: true),
                    TanggalMasuk = table.Column<DateTime>(type: "date", nullable: false),
                    TanggalKeluar = table.Column<DateTime>(type: "date", nullable: false),
                    Set = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adm_JenisPembayaran", x => x.JenisPembayaranId);
                });

            migrationBuilder.CreateTable(
                name: "Adm_JenisUser",
                schema: "public",
                columns: table => new
                {
                    JenisUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaJenisUser = table.Column<string>(type: "text", nullable: false),
                    Kode = table.Column<string>(type: "text", nullable: true),
                    Nomor = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adm_JenisUser", x => x.JenisUserId);
                });

            migrationBuilder.CreateTable(
                name: "Adm_Pembayaran",
                schema: "public",
                columns: table => new
                {
                    PembayaranId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaJenisUser = table.Column<string>(type: "text", nullable: false),
                    JenisPembayaranId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPembayaran = table.Column<string>(type: "text", nullable: false),
                    Nominal = table.Column<int>(type: "integer", nullable: false),
                    TanggalPembayaran = table.Column<DateTime>(type: "date", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adm_Pembayaran", x => x.PembayaranId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adm_JenisPembayaran",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Adm_JenisUser",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Adm_Pembayaran",
                schema: "public");
        }
    }
}
