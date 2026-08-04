using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services
{
    /// <summary>
    /// Satu service untuk:
    /// 1. Menentukan status coverage setiap LabBookingDetail.
    /// 2. Menyimpan IsTercover pada LabBookingDetail.
    /// 3. Menghitung nilai tercover dan tidak tercover.
    /// 4. Menyimpan rekap kalkulasi pada header LabBooking.
    /// </summary>
    public sealed class LabBookingCoverageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LabBookingCoverageService> _logger;

        public LabBookingCoverageService(
            ApplicationDbContext context,
            ILogger<LabBookingCoverageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LabBookingCoverageResult> ApplyCoverageAndRecalculateAsync(
            Guid bookingLabId,
            CancellationToken cancellationToken = default)
        {
            if (bookingLabId == Guid.Empty)
            {
                throw new ArgumentException(
                    "BookingLabId tidak valid.",
                    nameof(bookingLabId));
            }

            IDbContextTransaction? ownedTransaction = null;

            try
            {
                // Jika controller sudah membuka transaksi, service memakai transaksi tersebut.
                // Jika belum, service membuat transaksi sendiri.
                if (_context.Database.CurrentTransaction == null)
                {
                    ownedTransaction = await _context.Database.BeginTransactionAsync(
                        IsolationLevel.ReadCommitted,
                        cancellationToken);
                }

                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                var header = await GetAndLockHeaderAsync(
                    connection,
                    bookingLabId,
                    cancellationToken);

                if (header == null)
                {
                    throw new KeyNotFoundException(
                        $"Booking laboratorium dengan ID {bookingLabId} tidak ditemukan.");
                }

                // Langkah 1:
                // Tandai IsTercover pada seluruh detail aktif berdasarkan pasangan:
                // AsuransiId + PemeriksaanLabId pada MstPemeriksaanAsuransi.
                await UpdateDetailCoverageStatusAsync(
                    connection,
                    bookingLabId,
                    header.AsuransiId,
                    cancellationToken);

                // Langkah 2:
                // Ambil detail yang statusnya sudah diperbarui dan hitung subtotalnya.
                var details = await GetCalculatedDetailsAsync(
                    connection,
                    bookingLabId,
                    cancellationToken);

                var nilaiTercover = details
                    .Where(x => x.IsTercover)
                    .Sum(x => x.Subtotal);

                var nilaiTidakTercover = details
                    .Where(x => !x.IsTercover)
                    .Sum(x => x.Subtotal);

                // Langkah 3:
                // Simpan hasil kalkulasi pada header LabBooking.
                await UpdateBookingHeaderAsync(
                    connection,
                    bookingLabId,
                    nilaiTercover,
                    nilaiTidakTercover,
                    cancellationToken);

                if (ownedTransaction != null)
                {
                    await ownedTransaction.CommitAsync(cancellationToken);
                }

                return new LabBookingCoverageResult
                {
                    BookingLabId = bookingLabId,
                    AsuransiId = header.AsuransiId,
                    JumlahPemeriksaan = details.Count,
                    JumlahTercover = details.Count(x => x.IsTercover),
                    JumlahTidakTercover = details.Count(x => !x.IsTercover),
                    NilaiTercover = nilaiTercover,
                    NilaiTidakTercover = nilaiTidakTercover,
                    TotalPemeriksaan = nilaiTercover + nilaiTidakTercover,
                    Details = details
                };
            }
            catch (Exception ex)
            {
                if (ownedTransaction != null)
                {
                    await ownedTransaction.RollbackAsync(CancellationToken.None);
                }

                _logger.LogError(
                    ex,
                    "Gagal menerapkan coverage dan menghitung ulang LabBooking {BookingLabId}.",
                    bookingLabId);

                throw;
            }
            finally
            {
                if (ownedTransaction != null)
                {
                    await ownedTransaction.DisposeAsync();
                }
            }
        }

        private async Task<BookingHeader?> GetAndLockHeaderAsync(
            DbConnection connection,
            Guid bookingLabId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT
    lb.""BookingLabId"",
    lb.""AsuransiId""
FROM public.""LabBooking"" lb
WHERE lb.""BookingLabId"" = @bookingLabId
  AND COALESCE(lb.""IsDelete"", FALSE) = FALSE
FOR UPDATE;";

            await using var command = CreateCommand(connection, sql);
            AddGuidParameter(command, "@bookingLabId", bookingLabId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new BookingHeader
            {
                BookingLabId = reader.GetGuid(0),
                AsuransiId = reader.IsDBNull(1)
                    ? null
                    : reader.GetGuid(1)
            };
        }

        private async Task UpdateDetailCoverageStatusAsync(
            DbConnection connection,
            Guid bookingLabId,
            Guid? asuransiId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
UPDATE public.""LabBookingDetail"" AS d
SET
    ""IsTercover"" = CASE
        WHEN @asuransiId IS NULL THEN FALSE
        ELSE EXISTS
        (
            SELECT 1
            FROM public.""MstPemeriksaanAsuransi"" pa
            WHERE pa.""PemeriksaanLabId"" = d.""PemeriksaanLabId""
              AND pa.""AsuransiId"" = @asuransiId
              AND COALESCE(pa.""IsDelete"", FALSE) = FALSE
        )
    END,
    ""UpdateDateTime"" = CURRENT_TIMESTAMP
WHERE d.""BookingLabId"" = @bookingLabId
  AND COALESCE(d.""IsDelete"", FALSE) = FALSE;";

            await using var command = CreateCommand(connection, sql);
            AddGuidParameter(command, "@bookingLabId", bookingLabId);
            AddNullableGuidParameter(command, "@asuransiId", asuransiId);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task<List<LabBookingCoverageDetailResult>> GetCalculatedDetailsAsync(
            DbConnection connection,
            Guid bookingLabId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT
    d.""DetailBookingLabId"",
    d.""PemeriksaanLabId"",
    p.""NamaPemeriksaan"",
    COALESCE(p.""HargaPemeriksaan"", 0) AS ""HargaPemeriksaan"",
    COALESCE(d.""QtyOrder"", 1) AS ""QtyOrder"",
    COALESCE(d.""IsTercover"", FALSE) AS ""IsTercover""
FROM public.""LabBookingDetail"" d
INNER JOIN public.""LabPemeriksaans"" p
    ON p.""PemeriksaanLabId"" = d.""PemeriksaanLabId""
WHERE d.""BookingLabId"" = @bookingLabId
  AND COALESCE(d.""IsDelete"", FALSE) = FALSE
  AND COALESCE(p.""IsDelete"", FALSE) = FALSE
ORDER BY d.""CreateDateTime"" ASC;";

            await using var command = CreateCommand(connection, sql);
            AddGuidParameter(command, "@bookingLabId", bookingLabId);

            var result = new List<LabBookingCoverageDetailResult>();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var hargaPemeriksaan = reader.GetDecimal(3);
                var qtyOrder = reader.GetDecimal(4);
                var isTercover = reader.GetBoolean(5);

                var subtotal = Math.Round(
                    hargaPemeriksaan * qtyOrder,
                    2,
                    MidpointRounding.AwayFromZero);

                result.Add(new LabBookingCoverageDetailResult
                {
                    DetailBookingLabId = reader.GetGuid(0),
                    PemeriksaanLabId = reader.IsDBNull(1)
                        ? null
                        : reader.GetGuid(1),
                    NamaPemeriksaan = reader.IsDBNull(2)
                        ? string.Empty
                        : reader.GetString(2),
                    HargaPemeriksaan = hargaPemeriksaan,
                    QtyOrder = qtyOrder,
                    Subtotal = subtotal,
                    IsTercover = isTercover,
                    StatusCoverage = isTercover
                        ? "Tercover"
                        : "Tidak Tercover"
                });
            }

            return result;
        }

        private async Task UpdateBookingHeaderAsync(
            DbConnection connection,
            Guid bookingLabId,
            decimal nilaiTercover,
            decimal nilaiTidakTercover,
            CancellationToken cancellationToken)
        {
            const string sql = @"
UPDATE public.""LabBooking""
SET
    ""NilaiTercover"" = @nilaiTercover,
    ""NilaiTidakTercover"" = @nilaiTidakTercover,
    ""UpdateDateTime"" = CURRENT_TIMESTAMP
WHERE ""BookingLabId"" = @bookingLabId
  AND COALESCE(""IsDelete"", FALSE) = FALSE;";

            await using var command = CreateCommand(connection, sql);
            AddGuidParameter(command, "@bookingLabId", bookingLabId);
            AddDecimalParameter(command, "@nilaiTercover", nilaiTercover);
            AddDecimalParameter(command, "@nilaiTidakTercover", nilaiTidakTercover);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private DbCommand CreateCommand(
            DbConnection connection,
            string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction != null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            return command;
        }

        private static void AddGuidParameter(
            DbCommand command,
            string parameterName,
            Guid value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = DbType.Guid;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static void AddNullableGuidParameter(
            DbCommand command,
            string parameterName,
            Guid? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = DbType.Guid;
            parameter.Value = value.HasValue
                ? value.Value
                : DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static void AddDecimalParameter(
            DbCommand command,
            string parameterName,
            decimal value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = DbType.Decimal;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private sealed class BookingHeader
        {
            public Guid BookingLabId { get; set; }
            public Guid? AsuransiId { get; set; }
        }
    }

    public sealed class LabBookingCoverageResult
    {
        public Guid BookingLabId { get; set; }
        public Guid? AsuransiId { get; set; }

        public int JumlahPemeriksaan { get; set; }
        public int JumlahTercover { get; set; }
        public int JumlahTidakTercover { get; set; }

        public decimal NilaiTercover { get; set; }
        public decimal NilaiTidakTercover { get; set; }
        public decimal TotalPemeriksaan { get; set; }

        public List<LabBookingCoverageDetailResult> Details { get; set; } = new();
    }

    public sealed class LabBookingCoverageDetailResult
    {
        public Guid DetailBookingLabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public string NamaPemeriksaan { get; set; } = string.Empty;

        public decimal HargaPemeriksaan { get; set; }
        public decimal QtyOrder { get; set; }
        public decimal Subtotal { get; set; }

        public bool IsTercover { get; set; }
        public string StatusCoverage { get; set; } = string.Empty;
    }
}