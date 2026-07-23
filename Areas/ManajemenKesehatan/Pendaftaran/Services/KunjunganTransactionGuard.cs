using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services
{
    public sealed class KunjunganTransactionGuard
        : IKunjunganTransactionGuard
    {
        private readonly ApplicationDbContext _context;

        public KunjunganTransactionGuard(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task EnsureCanAddTransactionAsync(
            Guid kunjunganId,
            CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
            {
                throw new ArgumentException(
                    "KunjunganId wajib diisi.",
                    nameof(kunjunganId));
            }

            var currentTransaction =
                _context.Database.CurrentTransaction;

            if (currentTransaction == null)
            {
                throw new InvalidOperationException(
                    "Pengecekan status kunjungan harus dijalankan " +
                    "di dalam database transaction.");
            }

            var connection =
                _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using var command =
                connection.CreateCommand();

            command.Transaction =
                currentTransaction.GetDbTransaction();

            // Menggunakan verbatim string agar kompatibel dengan C# 10.
            command.CommandText = @"
                SELECT ""IsClosed""
                FROM public.""MstKunjungan""
                WHERE ""KunjunganID"" = @kunjunganId
                FOR UPDATE;
            ";

            var parameter =
                command.CreateParameter();

            parameter.ParameterName =
                "@kunjunganId";

            parameter.Value =
                kunjunganId;

            command.Parameters.Add(parameter);

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            if (result == null)
            {
                throw new KeyNotFoundException(
                    $"Kunjungan dengan ID {kunjunganId} tidak ditemukan.");
            }

            // Jika IsClosed null, dianggap belum ditutup.
            var isClosed =
                result != DBNull.Value &&
                Convert.ToBoolean(result);

            if (isClosed)
            {
                throw new InvalidOperationException(
                    "Billing kunjungan sudah ditutup. " +
                    "Resep, tindakan, pemeriksaan laboratorium, " +
                    "alat kesehatan, kamar, biaya administrasi, " +
                    "dan pelayanan lainnya tidak dapat ditambahkan.");
            }
        }
    }
}