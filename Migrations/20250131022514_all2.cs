using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class all2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstAgama",
                schema: "dbo",
                columns: table => new
                {
                    AgamaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgamaKode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisAgama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstAgama", x => x.AgamaId);
                });

            migrationBuilder.CreateTable(
                name: "MstGolonganDarah",
                schema: "dbo",
                columns: table => new
                {
                    GolonganDarahId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeGolonganDarah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaGolonganDarah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstGolonganDarah", x => x.GolonganDarahId);
                });

            migrationBuilder.CreateTable(
                name: "MstPekerjaan",
                schema: "dbo",
                columns: table => new
                {
                    PekerjaanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodePekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPekerjaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPekerjaan", x => x.PekerjaanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPendidikan",
                schema: "dbo",
                columns: table => new
                {
                    PendidikanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodePendidikan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPendidikan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPendidikan", x => x.PendidikanId);
                });

            migrationBuilder.CreateTable(
                name: "MstTitle",
                schema: "dbo",
                columns: table => new
                {
                    TitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstTitle", x => x.TitleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstAgama",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstGolonganDarah",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstPekerjaan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstPendidikan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstTitle",
                schema: "dbo");
        }
    }
}
