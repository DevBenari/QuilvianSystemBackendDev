using Hangfire;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Hangfire.Jobs
{
    public class BillingJob
    {
        private readonly ApplicationDbContext _db;

        public BillingJob(ApplicationDbContext db)
        {
            _db = db;
        }

        // JOB UNTUK UPDATE DPD DI BILLING
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)] // cegah double-run barengan
        public async Task DPDBillingRunAsync(CancellationToken ct)
        {
            // DPD = max(0, today_wib - (billing_date_wib + 90 days))
            // today_wib pakai (now() at time zone 'Asia/Jakarta')::date
            // billing_date_wib pakai ("BillingDate" at time zone 'Asia/Jakarta')::date

            var sql = @"
                UPDATE ""Billing""
                SET
                  ""TanggalJatuhTempo"" = COALESCE(
                      ""TanggalJatuhTempo"",
                      ((""BillingDate"" AT TIME ZONE 'Asia/Jakarta')::date + 90)::timestamp AT TIME ZONE 'Asia/Jakarta'
                  ),
                  ""DPD"" = GREATEST(
                      0,
                      (
                        ((now() AT TIME ZONE 'Asia/Jakarta')::date)
                        - COALESCE(
                            (""TanggalJatuhTempo"" AT TIME ZONE 'Asia/Jakarta')::date,
                            ((""BillingDate"" AT TIME ZONE 'Asia/Jakarta')::date + 90)
                          )
                      )::int
                  ),
                  ""UpdateDateTime"" = now()
                WHERE (""IsDelete"" IS NULL OR ""IsDelete"" = false)
                  AND ""BillingDate"" IS NOT NULL
                  -- optional: hanya yang belum lunas (sesuaikan aturanmu)
                  AND (""StatusBilling"" IS NULL OR ""StatusBilling"" = false);
                ";

            await _db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }
}
