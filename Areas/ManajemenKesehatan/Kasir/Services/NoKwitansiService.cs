using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Repositories;

public class NoKwitansiService : INoKwitansiService
{
    private readonly ApplicationDbContext _db;

    public NoKwitansiService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateNoKwitansiAsync(DateTimeOffset tglPembayaran, CancellationToken ct = default)
    {
        // yyyyMMdd sesuai permintaan
        var datePart = tglPembayaran.ToString("yyyyMMdd");
        var prefix = "KWS";

        // lock per tanggal, supaya urutan aman saat concurrent
        var lockKey = StableHash32($"kwitansi:{datePart}");

        // Pastikan connection open (tidak bikin trx baru)
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // IMPORTANT: ini lock "xact" -> butuh transaction aktif.
        // Karena controller sudah BeginTransactionAsync, lock ini ikut trx controller.
        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0});",
            new object[] { lockKey },
            ct);

        // ambil NoKwitansi terakhir pada tanggal tsb
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
            var seqStr = last.Substring(prefix.Length, 4);
            if (int.TryParse(seqStr, out var seq))
                nextSeq = seq + 1;
        }

        return $"{prefix}{nextSeq:0000}{datePart}";
    }

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
