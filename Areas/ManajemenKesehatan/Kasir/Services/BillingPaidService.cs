using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public interface IBillingService
    {
        Task<int> MarkBillingAsPaidAsync(Guid kunjunganId);
    }

    public class BillingPaidService : IBillingService
    {
        private readonly ApplicationDbContext _db;
        public BillingPaidService(ApplicationDbContext db) => _db = db;

        public Task<int> MarkBillingAsPaidAsync(Guid kunjunganId)
        {
            return _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Billings""
            SET ""StatusBilling"" = TRUE
            WHERE ""KunjunganId"" = {kunjunganId}
        ");
        }
    }
}
