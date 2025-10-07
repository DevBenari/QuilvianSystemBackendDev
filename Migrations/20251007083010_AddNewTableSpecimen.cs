using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewTableSpecimen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstSpecimen",
                schema: "public",
                columns: table => new
                {
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaSpecimen = table.Column<string>(type: "text", nullable: true),
                    KodeSpecimen = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSpecimen", x => x.SpecimenId);
                });

            migrationBuilder.CreateTable(
                name: "SpecimenMethods",
                columns: table => new
                {
                    SpecimenMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaraPengambilanSpecimen = table.Column<string>(type: "text", nullable: true),
                    KodeSpecimenMethod = table.Column<string>(type: "text", nullable: true),
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_SpecimenMethods", x => x.SpecimenMethodId);
                });

            migrationBuilder.CreateTable(
                name: "SpecimenPemeriksaans",
                columns: table => new
                {
                    SpecimenPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PemeriksaanSpecimen = table.Column<string>(type: "text", nullable: true),
                    KodeSpecimenTest = table.Column<string>(type: "text", nullable: true),
                    JenisSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_SpecimenPemeriksaans", x => x.SpecimenPemeriksaanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstSpecimen",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SpecimenMethods");

            migrationBuilder.DropTable(
                name: "SpecimenPemeriksaans");
        }
    }
}
