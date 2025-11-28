using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableIGDObservasidanDetailnya : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IGDObservasiDetails",
                columns: table => new
                {
                    ObservasiDetailIgdId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservasiIgdId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglObservasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    GambaranEKG = table.Column<string>(type: "text", nullable: true),
                    DCShock = table.Column<string>(type: "text", nullable: true),
                    TD = table.Column<decimal>(type: "numeric", nullable: true),
                    RR = table.Column<decimal>(type: "numeric", nullable: true),
                    Suhu = table.Column<decimal>(type: "numeric", nullable: true),
                    SPO2 = table.Column<decimal>(type: "numeric", nullable: true),
                    Urine = table.Column<decimal>(type: "numeric", nullable: true),
                    Pendarahan = table.Column<decimal>(type: "numeric", nullable: true),
                    Muntah = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_IGDObservasiDetails", x => x.ObservasiDetailIgdId);
                });

            migrationBuilder.CreateTable(
                name: "IGDObservasis",
                columns: table => new
                {
                    ObservasiIgdId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Airway = table.Column<string>(type: "text", nullable: true),
                    Breathing = table.Column<string>(type: "text", nullable: true),
                    Circulation = table.Column<string>(type: "text", nullable: true),
                    Disability = table.Column<string>(type: "text", nullable: true),
                    Eye = table.Column<string>(type: "text", nullable: true),
                    Motor = table.Column<string>(type: "text", nullable: true),
                    Verbal = table.Column<string>(type: "text", nullable: true),
                    AlatBantuNapas = table.Column<string>(type: "text", nullable: true),
                    AlatBantuOksigenasi = table.Column<string>(type: "text", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglObservasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_IGDObservasis", x => x.ObservasiIgdId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IGDObservasiDetails");

            migrationBuilder.DropTable(
                name: "IGDObservasis");
        }
    }
}
