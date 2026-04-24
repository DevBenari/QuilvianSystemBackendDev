using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class OCw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObservasiCairan",
                schema: "public",
                columns: table => new
                {
                    ObservasiCairanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglObservasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CairanMasuk = table.Column<string>(type: "text", nullable: false),
                    CairanKeluar = table.Column<string>(type: "text", nullable: false),
                    CairanSisa = table.Column<decimal>(type: "numeric", nullable: false),
                    JumlahUrin = table.Column<decimal>(type: "numeric", nullable: false),
                    TTDId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathTtd = table.Column<string>(type: "text", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_ObservasiCairan", x => x.ObservasiCairanId);
                });

            migrationBuilder.CreateTable(
                name: "ObservasiCairanWsd",
                schema: "public",
                columns: table => new
                {
                    ObservasiCairanWSDId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglAwalObservasiWSD = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglAkhirObservasiWSD = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CairanSisaWSDSebelumnya = table.Column<decimal>(type: "numeric", nullable: false),
                    CairanWSDBertambah = table.Column<decimal>(type: "numeric", nullable: false),
                    CairanSisaWSDTabung = table.Column<decimal>(type: "numeric", nullable: false),
                    TtdId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathTtd = table.Column<string>(type: "text", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_ObservasiCairanWsd", x => x.ObservasiCairanWSDId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObservasiCairan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ObservasiCairanWsd",
                schema: "public");
        }
    }
}
