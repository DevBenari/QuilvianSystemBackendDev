using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableChecklist2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    UrutanChecklistItem = table.Column<decimal>(type: "numeric", nullable: true),
                    KodeChecklistItem = table.Column<string>(type: "text", nullable: true),
                    NamaChecklistItem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ChecklistItems", x => x.ChecklistItemId);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistResponses",
                columns: table => new
                {
                    ChecklistResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PraOperasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleAnswers = table.Column<bool>(type: "boolean", nullable: true),
                    ChecklistAnswers = table.Column<bool>(type: "boolean", nullable: true),
                    AnswersId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ChecklistResponses", x => x.ChecklistResponseId);
                });

            migrationBuilder.CreateTable(
                name: "PraOperasis",
                columns: table => new
                {
                    PraOperasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    PainAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    VitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusMental = table.Column<string>(type: "text", nullable: true),
                    PengobatanSaatIni = table.Column<string>(type: "text", nullable: true),
                    AlatBantu = table.Column<string>(type: "text", nullable: true),
                    JenisOperasi = table.Column<string>(type: "text", nullable: true),
                    WaktuOperasi = table.Column<string>(type: "text", nullable: true),
                    TempatOperasi = table.Column<string>(type: "text", nullable: true),
                    HasilLab = table.Column<string>(type: "text", nullable: true),
                    IsBatukFluDemam = table.Column<bool>(type: "boolean", nullable: false),
                    IsHaid = table.Column<bool>(type: "boolean", nullable: false),
                    ProsedurOperasi = table.Column<string>(type: "text", nullable: true),
                    TanggalOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerawatBedahId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatRuanganId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TTDPerawatRuanganId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPerawatBedahId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDDokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPerawatPrimerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPerawatRuanganPath = table.Column<string>(type: "text", nullable: true),
                    TTDPerawatBedahPath = table.Column<string>(type: "text", nullable: true),
                    TTDDokterPath = table.Column<string>(type: "text", nullable: true),
                    TTDPerawatPrimerPath = table.Column<string>(type: "text", nullable: true),
                    TTDKeluarga = table.Column<string>(type: "text", nullable: true),
                    TglCatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPernyataanPasien = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPernyataanDokter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PraOperasis", x => x.PraOperasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "ChecklistResponses");

            migrationBuilder.DropTable(
                name: "PraOperasis");
        }
    }
}
