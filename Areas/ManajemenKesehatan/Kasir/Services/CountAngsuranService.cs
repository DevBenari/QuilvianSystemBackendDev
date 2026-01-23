using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public class CountAngsuranService : ICountAngsuran
    {
        private readonly ApplicationDbContext _db;

        public CountAngsuranService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> CountAsync(Guid kunjunganId, CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak boleh kosong.", nameof(kunjunganId));

            // Total angsuran = jumlah transaksi cicilan yang sudah tersimpan
            // AngsuranKe > 0 berarti cicilan (bukan lunas / bukan 0)
            var total = await _db.MainKasirDetails
                .AsNoTracking()
                .Where(d => d.KunjunganId == kunjunganId)
                .Where(d => d.AngsuranKe.HasValue && d.AngsuranKe.Value > 0)
                .CountAsync(cancellationToken);

            return total;
        }
    }
}
