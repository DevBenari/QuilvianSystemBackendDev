using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hrd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hrd_MstJenisCuti",
                schema: "public",
                columns: table => new
                {
                    JenisCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaCuti = table.Column<string>(type: "text", nullable: true),
                    KuotaTahunan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MstJenisCuti", x => x.JenisCutiId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstPengajuanCuti",
                schema: "public",
                columns: table => new
                {
                    PengajuanCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    MulaiCuti = table.Column<DateTime>(type: "date", nullable: false),
                    SelesaiCuti = table.Column<DateTime>(type: "date", nullable: false),
                    JumlahCutiDiambil = table.Column<int>(type: "integer", nullable: false),
                    SisaKuotaCuti = table.Column<int>(type: "integer", nullable: false),
                    AlasanCuti = table.Column<string>(type: "text", nullable: false),
                    PICPengganti = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPersetujuan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CatatanApprovedBy = table.Column<string>(type: "text", nullable: false),
                    Approved2By = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPersetujuan2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CatatanApproved2By = table.Column<string>(type: "text", nullable: false),
                    LampiranPendukung = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Hrd_MstPengajuanCuti", x => x.PengajuanCutiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_MstJenisCuti",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstPengajuanCuti",
                schema: "public");
        }
    }
}
