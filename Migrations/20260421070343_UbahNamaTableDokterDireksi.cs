using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaTableDokterDireksi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskonApproveds");

            migrationBuilder.CreateTable(
                name: "DiskonDireksis",
                columns: table => new
                {
                    DiskonAprrovedId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Approved1Id = table.Column<Guid>(type: "uuid", nullable: true),
                    IsApproved1 = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedDate1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Approved2Id = table.Column<Guid>(type: "uuid", nullable: true),
                    IsApproved2 = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedDate2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Approved3Id = table.Column<Guid>(type: "uuid", nullable: true),
                    IsApproved3 = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedDate3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DiskonDireksis", x => x.DiskonAprrovedId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskonDireksis");

            migrationBuilder.CreateTable(
                name: "DiskonApproveds",
                columns: table => new
                {
                    DiskonAprrovedId = table.Column<Guid>(type: "uuid", nullable: false),
                    Approved1Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Approved2Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Approved3Id = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedDate1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedDate2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedDate3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsApproved1 = table.Column<bool>(type: "boolean", nullable: true),
                    IsApproved2 = table.Column<bool>(type: "boolean", nullable: true),
                    IsApproved3 = table.Column<bool>(type: "boolean", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskonApproveds", x => x.DiskonAprrovedId);
                });
        }
    }
}
