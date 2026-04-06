using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Services
{
    using Microsoft.EntityFrameworkCore;

    public class AsuransiCoverageService : IAsuransiCoverageService
    {
        private readonly ApplicationDbContext _db;

        public AsuransiCoverageService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool?> GetIsCoveredAsync(Guid kunjunganId, string? jenisBilling, Guid? itemId = null, CancellationToken ct = default)
        {
            var asuransiId = await _db.Kunjungans
                .AsNoTracking()
                .Where(x => x.KunjunganID == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                .Select(x => x.AsuransiId)
                .FirstOrDefaultAsync(ct);

            // pasien mandiri
            if (asuransiId == null || asuransiId == Guid.Empty)
                return null;

            if (string.IsNullOrWhiteSpace(jenisBilling))
                return false;

            if (itemId == null || itemId == Guid.Empty)
                return false;

            switch (jenisBilling.Trim())
            {
                case "Pemeriksaan Lab":
                    return await _db.PemeriksaanAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.PemeriksaanLabId == itemId &&
                            (x.IsDelete == false),
                            ct);

                case "Obat":
                    return await _db.CoveranObatAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.ObatId == itemId &&
                            (x.IsDelete == false),
                            ct);

                case "Tindakan":
                    return await _db.TindakanAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.TindakanId == itemId &&
                            (x.IsDelete == false),
                            ct);

                case "Kamar":
                    return await _db.KamarAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.KamarId == itemId &&
                            (x.IsDelete == false),
                            ct);

                default:
                    return false;
            }
        }
    }
}
