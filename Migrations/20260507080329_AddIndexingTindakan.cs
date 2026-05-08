using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddIndexingTindakan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Index untuk mempercepat pencarian tindakan berdasarkan:
             * - KodeTindakan
             * - UnitAsal
             * - IsDelete
             *
             * Dipakai untuk query seperti:
             * KodeTindakan = "IGD-ASSESSMENTMEDIS"
             * UnitAsal = "IGD"
             */
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_MstTindakan_KodeTindakan_UnitAsal_IsDelete""
                ON public.""MstTindakan"" 
                (""KodeTindakan"", ""UnitAsal"", ""IsDelete"");
            ");

            /*
             * Index untuk mempercepat join:
             * MstTarifKelas.TindakanId = MstTindakan.TindakanId
             *
             * Sekaligus mempercepat filter IsDelete dan sorting CreateDateTime DESC.
             */
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_MstTarifKelas_TindakanId_IsDelete_CreateDateTime""
                ON public.""MstTarifKelas"" 
                (""TindakanId"", ""IsDelete"", ""CreateDateTime"" DESC);
            ");

            /*
             * Optional tapi sangat disarankan:
             * Index ini membantu pengecekan double billing tindakan.
             *
             * Dipakai saat service cek:
             * KunjunganId
             * JenisBilling = Tindakan
             * BillingKode = 002
             * ItemId = TindakanId
             * IsDelete = false/null
             */
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Billing_KunjunganId_JenisBilling_BillingKode_ItemId_IsDelete""
                ON public.""Billing""
                (""KunjunganId"", ""JenisBilling"", ""BillingKode"", ""ItemId"", ""IsDelete"");
            ");

            /*
             * Optional:
             * Index ini membantu pengecekan 1 pasien hanya boleh punya 1 biaya admin per hari.
             * Query biaya admin melakukan join Billing -> Kunjungan,
             * lalu filter BillingDate, JenisBilling, BillingKode, IsDelete.
             */
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Billing_KunjunganId_JenisBilling_BillingKode_BillingDate_IsDelete""
                ON public.""Billing""
                (""KunjunganId"", ""JenisBilling"", ""BillingKode"", ""BillingDate"", ""IsDelete"");
            ");

            /*
             * Optional:
             * Index untuk mempercepat join billing ke kunjungan dan filter pasien.
             */
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_MstKunjungan_PasienId_IsDelete""
                ON public.""MstKunjungan""
                (""PasienId"", ""IsDelete"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_MstTindakan_KodeTindakan_UnitAsal_IsDelete"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_MstTarifKelas_TindakanId_IsDelete_CreateDateTime"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_Billing_KunjunganId_JenisBilling_BillingKode_ItemId_IsDelete"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_Billing_KunjunganId_JenisBilling_BillingKode_BillingDate_IsDelete"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_MstKunjungan_PasienId_IsDelete"";
            ");
        }
    }
}
