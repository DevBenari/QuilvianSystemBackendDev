using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public interface IBillingService
    {
        Task<int> MarkBillingKunjunganAsPaidAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct);

        Task<int> MarkBillingResepTebusAsPaidAsync(
            Guid resepTebusId,
            Guid userActiveId,
            CancellationToken ct);
    }

    public class BillingPaidService : IBillingService
    {
        private readonly ApplicationDbContext _db;

        public BillingPaidService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<int> MarkBillingKunjunganAsPaidAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct)
        {
            return _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""Billing""
                SET 
                    ""StatusBilling"" = TRUE,
                    ""UpdateBy"" = {userActiveId},
                    ""UpdateDateTime"" = {DateTimeOffset.UtcNow}
                WHERE ""KunjunganId"" = {kunjunganId}
                  AND COALESCE(""IsDelete"", FALSE) = FALSE
                  AND COALESCE(""StatusBilling"", FALSE) = FALSE
            ", ct);
        }

        public Task<int> MarkBillingResepTebusAsPaidAsync(
            Guid resepTebusId,
            Guid userActiveId,
            CancellationToken ct)
        {
            return _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""Billing""
                SET 
                    ""StatusBilling"" = TRUE,
                    ""UpdateBy"" = {userActiveId},
                    ""UpdateDateTime"" = {DateTimeOffset.UtcNow}
                WHERE ""ResepTebusId"" = {resepTebusId}
                  AND COALESCE(""IsDelete"", FALSE) = FALSE
                  AND COALESCE(""StatusBilling"", FALSE) = FALSE
            ", ct);
        }
    }
}