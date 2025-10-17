using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addSDKIperawattable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerawatObejctive",
                schema: "public",
                columns: table => new
                {
                    ObjNurseId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaObjective = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PerawatObejctive", x => x.ObjNurseId);
                });

            migrationBuilder.CreateTable(
                name: "PerawatSubjective",
                schema: "public",
                columns: table => new
                {
                    SubNurseId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaSubjective = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PerawatSubjective", x => x.SubNurseId);
                });

            migrationBuilder.CreateTable(
                name: "SDKIDiagnosa",
                schema: "public",
                columns: table => new
                {
                    SDKIDiagnosaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SDKIKodeDiagnosa = table.Column<string>(type: "text", nullable: true),
                    NamaDiagnosa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKIDiagnosa", x => x.SDKIDiagnosaId);
                });

            migrationBuilder.CreateTable(
                name: "SDKIEdukasi",
                schema: "public",
                columns: table => new
                {
                    SDKIEdukasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    SDKIEtiologiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaEdukasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKIEdukasi", x => x.SDKIEdukasiId);
                });

            migrationBuilder.CreateTable(
                name: "SDKIEtiologi",
                schema: "public",
                columns: table => new
                {
                    SDKIEtiologiId = table.Column<Guid>(type: "uuid", nullable: false),
                    SDKIDiagnosa = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaEtiologi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKIEtiologi", x => x.SDKIEtiologiId);
                });

            migrationBuilder.CreateTable(
                name: "SDKIKolaborasi",
                schema: "public",
                columns: table => new
                {
                    SDKIKolaborasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    SDKIEtiologiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKolaborasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKIKolaborasi", x => x.SDKIKolaborasiId);
                });

            migrationBuilder.CreateTable(
                name: "SDKITeraupetik",
                schema: "public",
                columns: table => new
                {
                    SDKITeraupetikId = table.Column<Guid>(type: "uuid", nullable: false),
                    SDKIEtiologiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaTeraupetik = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKITeraupetik", x => x.SDKITeraupetikId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerawatObejctive",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PerawatSubjective",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDKIDiagnosa",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDKIEdukasi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDKIEtiologi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDKIKolaborasi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDKITeraupetik",
                schema: "public");
        }
    }
}
