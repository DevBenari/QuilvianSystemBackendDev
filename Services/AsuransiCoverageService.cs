//using Microsoft.EntityFrameworkCore;
//using QuilvianSystemBackendDev.Interfaces;
//using QuilvianSystemBackendDev.Repositories;

//namespace QuilvianSystemBackendDev.Services
//{
//    public class AsuransiCoverageService : IAsuransiCoverageService
//    {
//        private readonly ApplicationDbContext _db;

//        public AsuransiCoverageService(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        public async Task<bool?> GetIsCoveredAsync(Guid kunjunganId, string? jenisBilling, CancellationToken ct = default)
//        {
//            // 1. Ambil AsuransiId dari Kunjungan
//            var asuransiId = await _db.Kunjungans
//                .AsNoTracking()
//                .Where(x => x.KunjunganID == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
//                .Select(x => x.AsuransiId)
//                .FirstOrDefaultAsync(ct);

//            // Jika tidak ada asuransi → pasien mandiri
//            if (asuransiId == null || asuransiId == Guid.Empty)
//                return null;

//            // Normalisasi jenis billing
//            var jenis = NormalizeJenisBilling(jenisBilling);
//            if (jenis == null)
//                return false;

//            // 2. Cek coveran
//            var coverage = await _db.CoveranAsuransis
//                .AsNoTracking()
//                .Where(x => x.AsuransiId == asuransiId && (x.IsDelete == false || x.IsDelete == null))
//                .Select(x => new
//                {
//                    x.Lab,
//                    x.Tindakan,
//                    x.Kamar,
//                    x.Obat
//                })
//                .FirstOrDefaultAsync(ct);

//            if (coverage == null)
//                return false;

//            return jenis switch
//            {
//                "LAB" => coverage.Lab ?? false,
//                "TINDAKAN" => coverage.Tindakan ?? false,
//                "KAMAR" => coverage.Kamar ?? false,
//                "OBAT" => coverage.Obat ?? false,
//                _ => false
//            };
//        }

//        private static string? NormalizeJenisBilling(string? jenisBilling)
//        {
//            if (string.IsNullOrWhiteSpace(jenisBilling))
//                return null;

//            var val = jenisBilling.Trim().ToLower();

//            if (val.Contains("lab")) return "LAB";
//            if (val.Contains("tindakan")) return "TINDAKAN";
//            if (val.Contains("kamar")) return "KAMAR";
//            if (val.Contains("obat") || val.Contains("farmasi")) return "OBAT";

//            return null;
//        }
//    }
//}
