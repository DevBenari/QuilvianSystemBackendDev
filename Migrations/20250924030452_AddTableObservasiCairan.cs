using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableObservasiCairan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObservasiCairans",
                columns: table => new
                {
                    ObservasiCairanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserActivePerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglObservasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CairanMasuk = table.Column<string>(type: "text", nullable: true),
                    CairanKeluar = table.Column<string>(type: "text", nullable: true),
                    CairanSisa = table.Column<decimal>(type: "numeric", nullable: true),
                    JumlahUrin = table.Column<decimal>(type: "numeric", nullable: true),
                    TTDId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPath = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ObservasiCairans", x => x.ObservasiCairanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObservasiCairans");
        }
    }
}
