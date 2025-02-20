using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tablePersalinans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstPersalinan",
                schema: "dbo",
                columns: table => new
                {
                    PersalinanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodePersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalPersalinan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipePersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TindakanPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTindakanPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KomplikasiPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKamar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoKamar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KategoriKamar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CatatanPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DokterPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BidanPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnastesiPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObservasiPersalinan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaBayi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisKelaminBayi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TTLBayi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BeratBayi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PanjangBayi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaAyah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaIbu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusBayi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPersalinan", x => x.PersalinanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPersalinan",
                schema: "dbo");
        }
    }
}
