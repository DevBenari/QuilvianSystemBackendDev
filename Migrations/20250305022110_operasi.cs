using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class operasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    TanggalOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    KeluhanOperasi = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstOperasi", x => x.OperasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstOperasi",
                schema: "public");
        }
    }
}
