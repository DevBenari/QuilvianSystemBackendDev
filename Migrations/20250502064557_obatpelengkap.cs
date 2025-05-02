using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class obatpelengkap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstKandungan",
                schema: "public",
                columns: table => new
                {
                    KandunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKandungan = table.Column<string>(type: "text", nullable: false),
                    NamaKandungan = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKandungan", x => x.KandunganId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatAsuransi",
                schema: "public",
                columns: table => new
                {
                    ObatAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    KandunganId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstObatKandungan", x => x.ObatKandunganId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstKandungan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatKandungan",
                schema: "public");
        }
    }
}
