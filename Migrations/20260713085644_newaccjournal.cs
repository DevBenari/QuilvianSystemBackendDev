using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newaccjournal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fin_TempRecurringJournal",
                schema: "public",
                columns: table => new
                {
                    TempRJId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecurringJournalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecurringJournalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_TempRecurringJournal", x => x.TempRJId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_AccManualJurnal",
                schema: "public",
                columns: table => new
                {
                    AccManualJurnalId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeManualJurnal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TglDokumen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglManualJurnal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglPembatalan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipeDokumen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TempRJId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecurringJournalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecurringJournalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MataUangId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaMataUang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExchangeRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    RateToIdr = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    UnbalancedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_AccManualJurnal", x => x.AccManualJurnalId);
                    table.ForeignKey(
                        name: "FK_Fin_AccManualJurnal_Fin_TempRecurringJournal_TempRJId",
                        column: x => x.TempRJId,
                        principalSchema: "public",
                        principalTable: "Fin_TempRecurringJournal",
                        principalColumn: "TempRJId");
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetailTempRecurringJournal",
                schema: "public",
                columns: table => new
                {
                    DetailTempRJId = table.Column<Guid>(type: "uuid", nullable: false),
                    TempRJId = table.Column<Guid>(type: "uuid", nullable: false),
                    COAId = table.Column<Guid>(type: "uuid", nullable: false),
                    COACode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COAName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RoleSetupCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DebetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRegistrasi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailTempRecurringJournal", x => x.DetailTempRJId);
                    table.ForeignKey(
                        name: "FK_Fin_DetailTempRecurringJournal_Fin_TempRecurringJournal_Tem~",
                        column: x => x.TempRJId,
                        principalSchema: "public",
                        principalTable: "Fin_TempRecurringJournal",
                        principalColumn: "TempRJId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetAccManualJurnal",
                schema: "public",
                columns: table => new
                {
                    DetAccManualJurnalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccManualJurnalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailTempRJId = table.Column<Guid>(type: "uuid", nullable: true),
                    COAId = table.Column<Guid>(type: "uuid", nullable: false),
                    COACode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COAName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RoleSetupCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DebetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRegistrasi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetAccManualJurnal", x => x.DetAccManualJurnalId);
                    table.ForeignKey(
                        name: "FK_Fin_DetAccManualJurnal_Fin_AccManualJurnal_AccManualJurnalId",
                        column: x => x.AccManualJurnalId,
                        principalSchema: "public",
                        principalTable: "Fin_AccManualJurnal",
                        principalColumn: "AccManualJurnalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fin_DetAccManualJurnal_Fin_DetailTempRecurringJournal_Detai~",
                        column: x => x.DetailTempRJId,
                        principalSchema: "public",
                        principalTable: "Fin_DetailTempRecurringJournal",
                        principalColumn: "DetailTempRJId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fin_AccManualJurnal_TempRJId",
                schema: "public",
                table: "Fin_AccManualJurnal",
                column: "TempRJId");

            migrationBuilder.CreateIndex(
                name: "IX_Fin_DetAccManualJurnal_AccManualJurnalId",
                schema: "public",
                table: "Fin_DetAccManualJurnal",
                column: "AccManualJurnalId");

            migrationBuilder.CreateIndex(
                name: "IX_Fin_DetAccManualJurnal_DetailTempRJId",
                schema: "public",
                table: "Fin_DetAccManualJurnal",
                column: "DetailTempRJId");

            migrationBuilder.CreateIndex(
                name: "IX_Fin_DetailTempRecurringJournal_TempRJId",
                schema: "public",
                table: "Fin_DetailTempRecurringJournal",
                column: "TempRJId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_DetAccManualJurnal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_AccManualJurnal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_DetailTempRecurringJournal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_TempRecurringJournal",
                schema: "public");
        }
    }
}
