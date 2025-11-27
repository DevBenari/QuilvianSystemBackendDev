using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableRecallDetailRecallKonsulGizi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiziKonsultasis",
                columns: table => new
                {
                    KonsultasiGiziId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglKonsultasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Diagnosa = table.Column<string>(type: "text", nullable: true),
                    DokterPerujukId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterKonsulenId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosaHasil = table.Column<string>(type: "text", nullable: true),
                    TindakanId = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
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
                    table.PrimaryKey("PK_GiziKonsultasis", x => x.KonsultasiGiziId);
                });

            migrationBuilder.CreateTable(
                name: "RecallDetails",
                columns: table => new
                {
                    DetailRecallId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallId = table.Column<Guid>(type: "uuid", nullable: false),
                    MakananSelingan = table.Column<string>(type: "text", nullable: true),
                    WaktuMakanan = table.Column<string>(type: "text", nullable: true),
                    BanyakGR = table.Column<decimal>(type: "numeric", nullable: true),
                    BanyakUTR = table.Column<decimal>(type: "numeric", nullable: true),
                    IsSelingan = table.Column<bool>(type: "boolean", nullable: true),
                    KAL = table.Column<decimal>(type: "numeric", nullable: true),
                    Protein = table.Column<decimal>(type: "numeric", nullable: true),
                    Lemak = table.Column<decimal>(type: "numeric", nullable: true),
                    CHO = table.Column<decimal>(type: "numeric", nullable: true),
                    CA = table.Column<decimal>(type: "numeric", nullable: true),
                    FE = table.Column<decimal>(type: "numeric", nullable: true),
                    VitA = table.Column<decimal>(type: "numeric", nullable: true),
                    VitB1 = table.Column<decimal>(type: "numeric", nullable: true),
                    VitC = table.Column<decimal>(type: "numeric", nullable: true),
                    IsRataRataHarian = table.Column<bool>(type: "boolean", nullable: true),
                    IsRDA = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_RecallDetails", x => x.DetailRecallId);
                });

            migrationBuilder.CreateTable(
                name: "Recalls",
                columns: table => new
                {
                    RecallId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    SikapPasienDiet = table.Column<string>(type: "text", nullable: true),
                    AnjuranDiet = table.Column<string>(type: "text", nullable: true),
                    TglRecall = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DietesienId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Recalls", x => x.RecallId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiziKonsultasis");

            migrationBuilder.DropTable(
                name: "RecallDetails");

            migrationBuilder.DropTable(
                name: "Recalls");
        }
    }
}
