using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableAssessmentGizi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                table: "TindakanHarians",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiziAssessments",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Anthropometri = table.Column<string>(type: "text", nullable: true),
                    Biokimia = table.Column<string>(type: "text", nullable: true),
                    Klinis = table.Column<string>(type: "text", nullable: true),
                    RiwayatGizi = table.Column<string>(type: "text", nullable: true),
                    RiwayatPersonal = table.Column<string>(type: "text", nullable: true),
                    DiagnosisGizi = table.Column<string>(type: "text", nullable: true),
                    IntervensiGizi = table.Column<string>(type: "text", nullable: true),
                    JenisDiet = table.Column<string>(type: "text", nullable: true),
                    BentukMakanan = table.Column<string>(type: "text", nullable: true),
                    Frekuensi = table.Column<string>(type: "text", nullable: true),
                    RutePemberian = table.Column<string>(type: "text", nullable: true),
                    Energi = table.Column<decimal>(type: "numeric", nullable: true),
                    Protein = table.Column<decimal>(type: "numeric", nullable: true),
                    Karbohidrat = table.Column<decimal>(type: "numeric", nullable: true),
                    Lemak = table.Column<decimal>(type: "numeric", nullable: true),
                    EdukasiAwal = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_GiziAssessments", x => x.AssessmentId);
                });

            migrationBuilder.CreateTable(
                name: "GiziEvaluasis",
                columns: table => new
                {
                    EvaluasiGiziId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentGiziId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglEvaluasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MakananPokok = table.Column<decimal>(type: "numeric", nullable: true),
                    LHTinggiLemak = table.Column<decimal>(type: "numeric", nullable: true),
                    LHRendahLemak = table.Column<decimal>(type: "numeric", nullable: true),
                    LaukNabati = table.Column<decimal>(type: "numeric", nullable: true),
                    Sayur = table.Column<decimal>(type: "numeric", nullable: true),
                    Buah = table.Column<decimal>(type: "numeric", nullable: true),
                    SusuDiabetes = table.Column<decimal>(type: "numeric", nullable: true),
                    SusuBiasa = table.Column<decimal>(type: "numeric", nullable: true),
                    JumlahKalori = table.Column<decimal>(type: "numeric", nullable: true),
                    IdentifikasiMasalah = table.Column<string>(type: "text", nullable: true),
                    TindakLanjut = table.Column<string>(type: "text", nullable: true),
                    CatatanPerawat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_GiziEvaluasis", x => x.EvaluasiGiziId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiziAssessments");

            migrationBuilder.DropTable(
                name: "GiziEvaluasis");

            migrationBuilder.DropColumn(
                name: "Diagnosa",
                table: "TindakanHarians");
        }
    }
}
