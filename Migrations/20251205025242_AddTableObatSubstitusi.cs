using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableObatSubstitusi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObatSubstitusis",
                columns: table => new
                {
                    SubstitusiObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    PengambilObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    PengemasObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaktuAccDokter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DokterAccId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ObatSubstitusis", x => x.SubstitusiObatId);
                });

            migrationBuilder.CreateTable(
                name: "ObatTelaahs",
                columns: table => new
                {
                    TelaahObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsTepatIdentitas = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatObat = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatDosis = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatRute = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatWaktu = table.Column<bool>(type: "boolean", nullable: false),
                    PetugasCekFinalId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPetugasCekFinal = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ObatTelaahs", x => x.TelaahObatId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObatSubstitusis");

            migrationBuilder.DropTable(
                name: "ObatTelaahs");
        }
    }
}
