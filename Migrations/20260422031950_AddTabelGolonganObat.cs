using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTabelGolonganObat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstGolonganObat",
                schema: "public",
                columns: table => new
                {
                    GolonganObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaGolonganObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstGolonganObat", x => x.GolonganObatId);
                });

            migrationBuilder.CreateTable(
                name: "MstJenisProdukObat",
                schema: "public",
                columns: table => new
                {
                    JenisProdukObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaJenisProdukObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstJenisProdukObat", x => x.JenisProdukObatId);
                });

            migrationBuilder.CreateTable(
                name: "MstKategoriTerapeutik",
                schema: "public",
                columns: table => new
                {
                    KategoriTerapeutikId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKategoriTerapeutik = table.Column<string>(type: "text", nullable: true),
                    FungsiObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstKategoriTerapeutik", x => x.KategoriTerapeutikId);
                });

            migrationBuilder.CreateTable(
                name: "MstKodeKFA",
                schema: "public",
                columns: table => new
                {
                    KFAId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKode = table.Column<string>(type: "text", nullable: true),
                    NamaKFA = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstKodeKFA", x => x.KFAId);
                });

            migrationBuilder.CreateTable(
                name: "MstKomoditas",
                schema: "public",
                columns: table => new
                {
                    KomoditasId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKomoditas = table.Column<string>(type: "text", nullable: true),
                    IsMaterialGrup = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstKomoditas", x => x.KomoditasId);
                });

            migrationBuilder.CreateTable(
                name: "MstPrincipal",
                schema: "public",
                columns: table => new
                {
                    PrincipalId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPrincipal = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstPrincipal", x => x.PrincipalId);
                });

            migrationBuilder.CreateTable(
                name: "MstSubKategoriTerapeutik",
                schema: "public",
                columns: table => new
                {
                    SubKategoriTerapeutikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KategoriTerapeutikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSubKategoriTerapeutik = table.Column<string>(type: "text", nullable: true),
                    FungsiObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSubKategoriTerapeutik", x => x.SubKategoriTerapeutikId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstGolonganObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJenisProdukObat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKategoriTerapeutik",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKodeKFA",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKomoditas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPrincipal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSubKategoriTerapeutik",
                schema: "public");
        }
    }
}
