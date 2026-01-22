using System.Data;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public class NoKwitansiService : INoKwitansiService
    {
        private readonly ApplicationDbContext _db;

        public NoKwitansiService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateNoKwitansiAsync(DateTimeOffset tglPembayaran, CancellationToken ct = default)
        {
            // format tanggal: HariBulanTahun dari tanggal pembayaran yang diinput
            var datePart = tglPembayaran.ToString("yyyyMMdd");
            var prefix = "KWS";

            // kunci transaksi per tanggal agar tidak double saat concurrent request
            // pakai hash yang stabil (int32) untuk pg_advisory_xact_lock
            var lockKey = StableHash32($"kwitansi:{datePart}");

            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            // advisory lock (berlaku selama transaksi)
            await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0});", new object[] { lockKey }, ct);

            // ambil nomor terakhir untuk tanggal tsb (urut desc)
            // pola: KWS + 4 digit + yyyyMMdd
            var last = await _db.MainKasirs.AsNoTracking()
                .Where(x => x.NoKwitansi != null
                            && x.NoKwitansi.StartsWith(prefix)
                            && x.NoKwitansi.EndsWith(datePart))
                .OrderByDescending(x => x.NoKwitansi)
                .Select(x => x.NoKwitansi)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;

            if (!string.IsNullOrWhiteSpace(last) && last.Length >= (prefix.Length + 4 + datePart.Length))
            {
                // substring 4 digit setelah "KWS"
                var seqStr = last.Substring(prefix.Length, 4);
                if (int.TryParse(seqStr, out var seq))
                    nextSeq = seq + 1;
            }

            var noKwitansi = $"{prefix}{nextSeq:0000}{datePart}";

            await tx.CommitAsync(ct);

            return noKwitansi;
        }

        // hash stabil ke int32 (tidak pakai string.GetHashCode karena bisa berubah per process)
        private static int StableHash32(string s)
        {
            unchecked
            {
                int hash = 23;
                foreach (var c in s)
                    hash = (hash * 31) + c;
                return hash;
            }
        }
    }
}
