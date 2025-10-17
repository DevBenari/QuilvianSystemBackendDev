using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hrdje : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_MstPengajuanCuti",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "Hrd_MstJenisLembur",
                schema: "public",
                columns: table => new
                {
                    JenisLemburId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaLembur = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MstJenisLembur", x => x.JenisLemburId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstJenisTiketing",
                schema: "public",
                columns: table => new
                {
                    JenisTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTicket = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Hrd_MstJenisTiketing", x => x.JenisTicketId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_PengajuanCuti",
                schema: "public",
                columns: table => new
                {
                    PengajuanCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_PengajuanCuti", x => x.PengajuanCutiId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_PengajuanLembur",
                schema: "public",
                columns: table => new
                {
                    PengajuanLemburId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisLemburId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglLembur = table.Column<DateTime>(type: "date", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    LamaLembur = table.Column<int>(type: "integer", nullable: false),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy2 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_PengajuanLembur", x => x.PengajuanLemburId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_PengajuanTiketing",
                schema: "public",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoAntrian = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JudulTicketing = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    Prioritas = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ruangan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TglDibutuhkan = table.Column<DateTime>(type: "date", nullable: true),
                    EstimasiBudget = table.Column<decimal>(type: "numeric", nullable: true),
                    Lampiran = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy2 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_PengajuanTiketing", x => x.TicketId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_MstJenisLembur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstJenisTiketing",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_PengajuanCuti",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_PengajuanLembur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_PengajuanTiketing",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "Hrd_MstPengajuanCuti",
                schema: "public",
                columns: table => new
                {
                    PengajuanCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlasanCuti = table.Column<string>(type: "text", nullable: false),
                    Approved2By = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CatatanApproved2By = table.Column<string>(type: "text", nullable: false),
                    CatatanApprovedBy = table.Column<string>(type: "text", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    JenisCutiId = table.Column<Guid>(type: "uuid", nullable: false),
                    JumlahCutiDiambil = table.Column<int>(type: "integer", nullable: false),
                    LampiranPendukung = table.Column<string>(type: "text", nullable: false),
                    MulaiCuti = table.Column<DateTime>(type: "date", nullable: false),
                    PICPengganti = table.Column<string>(type: "text", nullable: false),
                    SelesaiCuti = table.Column<DateTime>(type: "date", nullable: false),
                    SisaKuotaCuti = table.Column<int>(type: "integer", nullable: false),
                    TglPersetujuan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPersetujuan2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hrd_MstPengajuanCuti", x => x.PengajuanCutiId);
                });
        }
    }
}
