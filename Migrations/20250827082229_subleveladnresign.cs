using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class subleveladnresign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PengajuanResigns",
                columns: table => new
                {
                    ResignId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglEfektifResign = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoticePeriod = table.Column<float>(type: "real", nullable: false),
                    AlasanUtama = table.Column<string>(type: "text", nullable: true),
                    AlasanTambahan = table.Column<string>(type: "text", nullable: true),
                    Approved1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Approved2 = table.Column<Guid>(type: "uuid", nullable: false),
                    isTerimaPenawaran = table.Column<bool>(type: "boolean", nullable: false),
                    StatusResign = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PengajuanResigns", x => x.ResignId);
                });

            migrationBuilder.CreateTable(
                name: "SubLevels",
                columns: table => new
                {
                    SubLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubLevelNum = table.Column<float>(type: "real", nullable: false),
                    PayGrade = table.Column<Guid>(type: "uuid", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AdditionalSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Subsidy = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Compensation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reimbursement = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DailyTransport = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MealAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MealOutsideOffice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiligentFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    isOvertime = table.Column<bool>(type: "boolean", nullable: false),
                    isAbsent = table.Column<bool>(type: "boolean", nullable: false),
                    isInsentif = table.Column<bool>(type: "boolean", nullable: false),
                    isBonus = table.Column<bool>(type: "boolean", nullable: false),
                    isLeaveCompansation = table.Column<bool>(type: "boolean", nullable: false),
                    isPositionAllowance = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SubLevels", x => x.SubLevelId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PengajuanResigns");

            migrationBuilder.DropTable(
                name: "SubLevels");
        }
    }
}
