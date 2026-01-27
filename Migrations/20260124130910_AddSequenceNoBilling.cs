using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddSequenceNoBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<long>(
                name: "InvoiceBillingSeq",
                schema: "public",
                startValue: 1L,
                incrementBy: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "InvoiceBillingSeq",
                schema: "public");
        }
    }
}
