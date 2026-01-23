using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public class GenerateUrutanAngsuranService : IGenerateUrutanAngsuran
    {
        private readonly ApplicationDbContext _db;

        public GenerateUrutanAngsuranService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GenerateAsync(
            Guid kunjunganId,
            decimal? sisaPembayaranSetelahBayar,
            CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak boleh kosong.", nameof(kunjunganId));

            // lastAngsuran: max AngsuranKe > 0 untuk kunjungan tsb
            var lastAngsuran = await _db.MainKasirDetails
                .AsNoTracking()
                .Where(d => d.KunjunganId == kunjunganId)
                .Where(d => d.AngsuranKe.HasValue && d.AngsuranKe.Value > 0)
                .MaxAsync(d => (decimal?)d.AngsuranKe, cancellationToken);

            // Belum pernah angsuran sebelumnya:
            if (lastAngsuran is null || lastAngsuran.Value <= 0)
            {
                // Jika sekali bayar langsung lunas
                if (!sisaPembayaranSetelahBayar.HasValue || sisaPembayaranSetelahBayar.Value <= 0)
                    return 0;

                // Kalau masih ada sisa, berarti mulai angsuran
                return 1;
            }

            // Sudah pernah angsuran: selalu iterasi +1 (meskipun ini pembayaran terakhir dan sisa jadi 0)
            return (int)(lastAngsuran.Value + 1);
        }
    }
}
