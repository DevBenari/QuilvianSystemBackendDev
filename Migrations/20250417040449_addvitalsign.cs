using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addvitalsign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstVitalSign",
                schema: "public",
                columns: table => new
                {
                    VitalSignId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    Suhu = table.Column<decimal>(type: "numeric", nullable: true),
                    HR = table.Column<int>(type: "integer", nullable: true),
                    RR = table.Column<int>(type: "integer", nullable: true),
                    TekananDarahSystolic = table.Column<int>(type: "integer", nullable: true),
                    TekananDarahDiastolic = table.Column<int>(type: "integer", nullable: true),
                    SaturasiOksigen = table.Column<decimal>(type: "numeric", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    BMI = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstVitalSign", x => x.VitalSignId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstVitalSign",
                schema: "public");
        }
    }
}
