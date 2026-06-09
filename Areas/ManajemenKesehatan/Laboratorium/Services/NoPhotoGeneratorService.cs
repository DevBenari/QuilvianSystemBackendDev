using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services
{
    public interface INoPhotoGeneratorService
    {
        Task<string> GenerateNoOrderByLabIdAsync(
            Guid labId,
            CancellationToken cancellationToken = default);

        Task<string> EnsureNoOrderForBookingAsync(
            Guid bookingLabId,
            Guid labId,
            Guid? updateBy = null,
            CancellationToken cancellationToken = default);

        Task<int> GenerateNoPhotosByLabBookingIdAsync(
            Guid labBookingId,
            CancellationToken cancellationToken = default);
    }

    public class NoPhotoGeneratorService : INoPhotoGeneratorService
    {
        private readonly ApplicationDbContext _context;

        // Januari 2026 = umur RS 40 tahun.
        // Berarti tahun berdiri RS = 1986.
        // Nilai ini tidak perlu diganti setiap tahun.
        private const int HospitalEstablishedYear = 1986;

        // RS berulang tahun setiap bulan Januari.
        private const int HospitalAnniversaryMonth = 1;

        // Januari=A, Februari=B, ... Desember=L
        private static readonly Dictionary<int, string> MonthCodes = new()
        {
            { 1, "A" },
            { 2, "B" },
            { 3, "C" },
            { 4, "D" },
            { 5, "E" },
            { 6, "F" },
            { 7, "G" },
            { 8, "H" },
            { 9, "I" },
            { 10, "J" },
            { 11, "K" },
            { 12, "L" }
        };

        public NoPhotoGeneratorService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // NO ORDER
        // Format: 000001
        // Naik berdasarkan LabId / layanan penunjang.
        //
        // Contoh:
        // Radiologi   -> 000001, 000002, 000003
        // Laboratorium -> 000001, 000002, 000003
        // =========================================================
        public async Task<string> GenerateNoOrderByLabIdAsync(
            Guid labId,
            CancellationToken cancellationToken = default)
        {
            if (labId == Guid.Empty)
                throw new Exception("LabId tidak valid.");

            var existingNoOrders = await _context.LabBookings
                .AsNoTracking()
                .Where(b =>
                    b.NoOrder != null &&
                    b.NoOrder != "" &&
                    _context.LabBookingDetails.Any(d =>
                        d.BookingLabId.HasValue &&
                        d.BookingLabId.Value == b.BookingLabId &&
                        d.LabId.HasValue &&
                        d.LabId.Value == labId
                    )
                )
                .Select(b => b.NoOrder!)
                .Distinct()
                .ToListAsync(cancellationToken);

            var maxNumber = 0;

            foreach (var noOrder in existingNoOrders)
            {
                if (int.TryParse(noOrder, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            var nextNumber = maxNumber + 1;

            return nextNumber.ToString("D6"); // 000001
        }

        // =========================================================
        // ENSURE NO ORDER
        //
        // Dipanggil saat LabBookingDetail dibuat.
        //
        // Logic:
        // - Kalau LabBooking.NoOrder masih kosong, generate berdasarkan LabId.
        // - Kalau LabBooking.NoOrder sudah ada, pakai nomor lama.
        // - Jadi detail kedua, ketiga, dst dalam BookingLabId yang sama
        //   tetap memakai NoOrder yang sama.
        // =========================================================
        public async Task<string> EnsureNoOrderForBookingAsync(
            Guid bookingLabId,
            Guid labId,
            Guid? updateBy = null,
            CancellationToken cancellationToken = default)
        {
            if (bookingLabId == Guid.Empty)
                throw new Exception("BookingLabId tidak valid.");

            if (labId == Guid.Empty)
                throw new Exception("LabId tidak valid.");

            var booking = await _context.LabBookings
                .FirstOrDefaultAsync(x =>
                    x.BookingLabId == bookingLabId &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (booking == null)
                throw new Exception("Booking lab tidak ditemukan.");

            // Satu BookingLabId hanya boleh punya satu jenis LabId / layanan penunjang.
            var existingLabIds = await _context.LabBookingDetails
                .AsNoTracking()
                .Where(x =>
                    x.BookingLabId.HasValue &&
                    x.BookingLabId.Value == bookingLabId &&
                    x.LabId.HasValue &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => x.LabId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (existingLabIds.Any() && !existingLabIds.Contains(labId))
            {
                throw new Exception("Satu BookingLabId tidak boleh memiliki LabId / layanan penunjang berbeda.");
            }

            // Kalau sudah punya NoOrder, jangan generate ulang.
            if (!string.IsNullOrWhiteSpace(booking.NoOrder))
                return booking.NoOrder;

            var noOrder = await GenerateNoOrderByLabIdAsync(labId, cancellationToken);

            booking.NoOrder = noOrder;
            booking.UpdateDateTime = DateTime.UtcNow;

            if (updateBy.HasValue)
                booking.UpdateBy = updateBy.Value;

            return noOrder;
        }

        // =========================================================
        // NO PHOTO
        //
        // Format:
        // [KodeModality][UmurRS][KodeBulan]-[Urutan]
        //
        // Contoh:
        // R40A-001
        //
        // R  = Radiologi / huruf pertama kode kategori
        // 40 = umur RS
        // A  = Januari
        // 001 = urutan
        // =========================================================
        public async Task<int> GenerateNoPhotosByLabBookingIdAsync(
            Guid labBookingId,
            CancellationToken cancellationToken = default)
        {
            if (labBookingId == Guid.Empty)
                throw new Exception("BookingLabId tidak valid.");

            var now = DateTime.Now;

            var transaction = _context.Database.CurrentTransaction == null
                ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;

            try
            {
                var booking = await _context.LabBookings
                    .FirstOrDefaultAsync(x =>
                        x.BookingLabId == labBookingId &&
                        (x.IsDelete == false || x.IsDelete == null),
                        cancellationToken);

                if (booking == null)
                    throw new Exception("Booking lab tidak ditemukan.");

                if (booking.KonfirmatorId == null)
                    throw new Exception("No Photo hanya bisa digenerate jika booking lab sudah dikonfirmasi.");

                var details = await _context.LabBookingDetails
                    .Where(x =>
                        x.BookingLabId == labBookingId &&
                        (x.NoPhoto == null || x.NoPhoto == "") &&
                        (x.IsDelete == false || x.IsDelete == null))
                    .OrderBy(x => x.CreateDateTime)
                    .ToListAsync(cancellationToken);

                if (!details.Any())
                {
                    if (transaction != null)
                        await transaction.CommitAsync(cancellationToken);

                    return 0;
                }

                var pemeriksaanLabIds = details
                    .Where(x => x.PemeriksaanLabId.HasValue)
                    .Select(x => x.PemeriksaanLabId!.Value)
                    .Distinct()
                    .ToList();

                var kategoriByPemeriksaanLabId = await _context.LabPemeriksaans
                    .AsNoTracking()
                    .Where(x => pemeriksaanLabIds.Contains(x.PemeriksaanLabId))
                    .Select(x => new
                    {
                        x.PemeriksaanLabId,
                        KodeKategori = x.KategoriPemeriksaan != null
                            ? x.KategoriPemeriksaan.KodeKategori
                            : null,
                        NamaKategori = x.KategoriPemeriksaan != null
                            ? x.KategoriPemeriksaan.NamaKategori
                            : null
                    })
                    .ToDictionaryAsync(x => x.PemeriksaanLabId, cancellationToken);

                var hospitalYear = GetHospitalYear(now);
                var monthCode = GetMonthCode(now.Month);

                var lastNumberByPrefix = new Dictionary<string, int>();

                foreach (var detail in details)
                {
                    if (!detail.PemeriksaanLabId.HasValue)
                    {
                        throw new Exception(
                            $"PemeriksaanLabId kosong untuk DetailBookingLabId: {detail.DetailBookingLabId}");
                    }

                    if (!kategoriByPemeriksaanLabId.TryGetValue(detail.PemeriksaanLabId.Value, out var kategori))
                    {
                        throw new Exception(
                            $"Kategori pemeriksaan tidak ditemukan untuk PemeriksaanLabId: {detail.PemeriksaanLabId}");
                    }

                    var modality = GetModalityCode(
                        kategori.KodeKategori,
                        kategori.NamaKategori
                    );

                    var prefix = $"{modality}{hospitalYear:00}{monthCode}-";

                    if (!lastNumberByPrefix.ContainsKey(prefix))
                    {
                        lastNumberByPrefix[prefix] =
                            await GetLastNoPhotoNumberByPrefixAsync(prefix, cancellationToken);
                    }

                    var nextNumber = lastNumberByPrefix[prefix] + 1;

                    detail.NoPhoto = $"{prefix}{nextNumber:000}";

                    lastNumberByPrefix[prefix] = nextNumber;
                }

                await _context.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                    await transaction.CommitAsync(cancellationToken);

                return details.Count;
            }
            catch
            {
                if (transaction != null)
                    await transaction.RollbackAsync(cancellationToken);

                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }
        }

        // =========================================================
        // HELPER
        // =========================================================

        private static int GetHospitalYear(DateTime date)
        {
            var hospitalYear = date.Year - HospitalEstablishedYear;

            if (date.Month < HospitalAnniversaryMonth)
            {
                hospitalYear--;
            }

            if (hospitalYear < 0)
                throw new Exception("Perhitungan umur rumah sakit tidak valid.");

            return hospitalYear;
        }

        private static string GetModalityCode(string? kodeKategori, string? namaKategori)
        {
            var source = !string.IsNullOrWhiteSpace(kodeKategori)
                ? kodeKategori.Trim().ToUpper()
                : (namaKategori ?? string.Empty).Trim().ToUpper();

            var firstAlphabet = source.FirstOrDefault(char.IsLetter);

            return firstAlphabet == default ? "X" : firstAlphabet.ToString();
        }

        private static string GetMonthCode(int month)
        {
            if (!MonthCodes.TryGetValue(month, out var code))
                throw new ArgumentOutOfRangeException(nameof(month), "Month harus 1 sampai 12.");

            return code;
        }

        private async Task<int> GetLastNoPhotoNumberByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default)
        {
            var existingNoPhotos = await _context.LabBookingDetails
                .AsNoTracking()
                .Where(x =>
                    x.NoPhoto != null &&
                    x.NoPhoto.StartsWith(prefix))
                .Select(x => x.NoPhoto!)
                .ToListAsync(cancellationToken);

            var maxNumber = 0;

            foreach (var noPhoto in existingNoPhotos)
            {
                var match = Regex.Match(noPhoto, @"-(\d+)$");

                if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            return maxNumber;
        }
    }
}