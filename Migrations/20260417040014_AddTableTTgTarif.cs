using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableTTgTarif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiskonDokters",
                columns: table => new
                {
                    DiskonApprovedId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Approved1Id = table.Column<Guid>(type: "uuid", nullable: true),
                    IsApproved1 = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedDate1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DiskonDokters", x => x.DiskonApprovedId);
                });

            migrationBuilder.CreateTable(
                name: "TarifAlkess",
                columns: table => new
                {
                    TarifAlkesId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlkesId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifAlkess", x => x.TarifAlkesId);
                });

            migrationBuilder.CreateTable(
                name: "TarifHemodialisas",
                columns: table => new
                {
                    TarifHemodialisaId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifHemodialisas", x => x.TarifHemodialisaId);
                });

            migrationBuilder.CreateTable(
                name: "TarifKamars",
                columns: table => new
                {
                    TarifKamarId = table.Column<Guid>(type: "uuid", nullable: false),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifKamars", x => x.TarifKamarId);
                });

            migrationBuilder.CreateTable(
                name: "TarifMicrobiologis",
                columns: table => new
                {
                    TarifMicroId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifMicrobiologis", x => x.TarifMicroId);
                });

            migrationBuilder.CreateTable(
                name: "TarifOperasis",
                columns: table => new
                {
                    TarifOperasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifOperasis", x => x.TarifOperasiId);
                });

            migrationBuilder.CreateTable(
                name: "TarifPaketLayanans",
                columns: table => new
                {
                    TarifPaketLayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaketLayananId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifPaketLayanans", x => x.TarifPaketLayananId);
                });

            migrationBuilder.CreateTable(
                name: "TarifPatalogiAnatomis",
                columns: table => new
                {
                    TarifPatmiId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifPatalogiAnatomis", x => x.TarifPatmiId);
                });

            migrationBuilder.CreateTable(
                name: "TarifPatologiKliniks",
                columns: table => new
                {
                    TarifPatnikId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifPatologiKliniks", x => x.TarifPatnikId);
                });

            migrationBuilder.CreateTable(
                name: "TarifRehabMediks",
                columns: table => new
                {
                    TarifRehabId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifRehabMediks", x => x.TarifRehabId);
                });

            migrationBuilder.CreateTable(
                name: "TarifVisits",
                columns: table => new
                {
                    TarifVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifVisits", x => x.TarifVisitId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskonDokters");

            migrationBuilder.DropTable(
                name: "TarifAlkess");

            migrationBuilder.DropTable(
                name: "TarifHemodialisas");

            migrationBuilder.DropTable(
                name: "TarifKamars");

            migrationBuilder.DropTable(
                name: "TarifMicrobiologis");

            migrationBuilder.DropTable(
                name: "TarifOperasis");

            migrationBuilder.DropTable(
                name: "TarifPaketLayanans");

            migrationBuilder.DropTable(
                name: "TarifPatalogiAnatomis");

            migrationBuilder.DropTable(
                name: "TarifPatologiKliniks");

            migrationBuilder.DropTable(
                name: "TarifRehabMediks");

            migrationBuilder.DropTable(
                name: "TarifVisits");
        }
    }
}
