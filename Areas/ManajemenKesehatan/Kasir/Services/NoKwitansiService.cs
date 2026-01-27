using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        const string prefix = "KWS";

        // penting: “tanggal” untuk sequence harian
        // pakai offset yang dikirim user (biasanya +07)
        var dateOnly = tglPembayaran.Date; // DateTime
        var datePart = tglPembayaran.ToString("yyyyMMdd");

        // Pastikan koneksi open
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // Pakai transaksi yang sedang berjalan (kalau controller sudah BeginTransactionAsync)
        var currentTx = _db.Database.CurrentTransaction?.GetDbTransaction();

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = currentTx;

        cmd.CommandText = @"
    INSERT INTO ""KwitansiSequences"" (""KwitansiDate"", ""LastSeq"", ""UpdatedAt"")
    VALUES (@p_date, 1, @p_now)
    ON CONFLICT (""KwitansiDate"")
    DO UPDATE SET
        ""LastSeq"" = ""KwitansiSequences"".""LastSeq"" + 1,
        ""UpdatedAt"" = EXCLUDED.""UpdatedAt""
    RETURNING ""LastSeq"";
    ";

        var pDate = cmd.CreateParameter();
        pDate.ParameterName = "@p_date";
        pDate.Value = dateOnly;         // akan masuk ke kolom type "date"
        cmd.Parameters.Add(pDate);

        var pNow = cmd.CreateParameter();
        pNow.ParameterName = "@p_now";
        pNow.Value = DateTimeOffset.UtcNow;
        cmd.Parameters.Add(pNow);

        var result = await cmd.ExecuteScalarAsync(ct);
        var nextSeq = Convert.ToInt32(result);

        return $"{prefix}{nextSeq:0000}{datePart}";
    }
}
