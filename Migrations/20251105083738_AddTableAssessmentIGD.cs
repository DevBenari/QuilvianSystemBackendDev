using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableAssessmentIGD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IGDAssessmentAwals",
                columns: table => new
                {
                    AssessmentAwalIGD = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsSpritualPenting = table.Column<bool>(type: "boolean", nullable: true),
                    IsMenngikutiKegiatanSpritual = table.Column<bool>(type: "boolean", nullable: true),
                    DataSubjektif = table.Column<string>(type: "text", nullable: true),
                    DataObjektif = table.Column<string>(type: "text", nullable: true),
                    KebutuhanTransportasi = table.Column<string>(type: "text", nullable: true),
                    StatusKehamilan = table.Column<string>(type: "text", nullable: true),
                    TTDPerawatId = table.Column<string>(type: "text", nullable: true),
                    TTDPath = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_IGDAssessmentAwals", x => x.AssessmentAwalIGD);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IGDAssessmentAwals");
        }
    }
}
