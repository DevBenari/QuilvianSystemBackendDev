using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newsql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hrd_CounterOffer",
                schema: "public",
                columns: table => new
                {
                    CounterOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerusahaanRekruter = table.Column<string>(type: "text", nullable: true),
                    IndustriRekruter = table.Column<string>(type: "text", nullable: true),
                    TawaranJabatan = table.Column<string>(type: "text", nullable: true),
                    TglOffer = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TawaranGaji = table.Column<decimal>(type: "numeric", nullable: true),
                    InsentifPercentase = table.Column<decimal>(type: "numeric", nullable: true),
                    TawaranKompensasi = table.Column<decimal>(type: "numeric", nullable: true),
                    DeadlineRespont = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TawaranBenefitFasilitas = table.Column<string>(type: "text", nullable: true),
                    UsulGaji = table.Column<decimal>(type: "numeric", nullable: true),
                    PercentaseKenaikan = table.Column<decimal>(type: "numeric", nullable: true),
                    PercentaseBonus = table.Column<decimal>(type: "numeric", nullable: true),
                    EquityPenyesuaian = table.Column<string>(type: "text", nullable: true),
                    TglEfektif = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PermintaanPromosi = table.Column<string>(type: "text", nullable: true),
                    PermintaanLainnya = table.Column<string>(type: "text", nullable: true),
                    PencapaianUtama = table.Column<string>(type: "text", nullable: true),
                    RisetPasar = table.Column<string>(type: "text", nullable: true),
                    KomitmenMasaDepan = table.Column<string>(type: "text", nullable: true),
                    LevelRisk = table.Column<string>(type: "text", nullable: true),
                    KnowladgeTransferRisk = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_CounterOffer", x => x.CounterOfferId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstGradeLevelJob",
                schema: "public",
                columns: table => new
                {
                    GradeLevelJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Hrd_MstGradeLevelJob", x => x.GradeLevelJobId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_CounterOffer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstGradeLevelJob",
                schema: "public");
        }
    }
}
