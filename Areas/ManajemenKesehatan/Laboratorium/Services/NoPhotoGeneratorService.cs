using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services
{
    public interface INoPhotoGeneratorService
    {
        Task<string> GetLastNoOrderNumberByKunjunganIdAsync(Guid kunjunganId,CancellationToken cancellationToken = default); 
        Task<int> GenerateNoPhotosByLabBookingIdAsync(Guid labBookingId, CancellationToken cancellationToken = default);

    }

    public class NoPhotoGeneratorService : INoPhotoGeneratorService
    {
        private readonly ApplicationDbContext _context;

        // Januari 2026 = umur RS 40 tahun
        private const int ReferenceYear = 2026;
        private const int ReferenceHospitalYear = 40;

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

        public async Task<int> GenerateNoPhotosByLabBookingIdAsync(
            Guid labBookingId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            var transaction = _context.Database.CurrentTransaction == null
                ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;

            try
            {
                var booking = await _context.LabBookings
                    .FirstOrDefaultAsync(x => x.BookingLabId == labBookingId, cancellationToken);

                if (booking == null)
                    throw new Exception("Booking lab tidak ditemukan.");

                if (booking.KonfirmatorId == null)
                    throw new Exception("No Photo hanya bisa digenerate jika booking lab sudah dikonfirmasi.");

                var details = await _context.LabBookingDetails
                    .Where(x =>
                        x.BookingLabId == labBookingId &&
                        (x.NoPhoto == null || x.NoPhoto == ""))
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
                        PemeriksaanLabId = x.PemeriksaanLabId,
                        KodeKategori = x.KategoriPemeriksaan != null
                            ? x.KategoriPemeriksaan.KodeKategori
                            : null,
                        NamaKategori = x.KategoriPemeriksaan != null
                            ? x.KategoriPemeriksaan.NamaKategori
                            : null
                    })
                    .ToDictionaryAsync(x => x.PemeriksaanLabId, cancellationToken);

                var hospitalYear = ReferenceHospitalYear + (now.Year - ReferenceYear);
                if (hospitalYear < 0)
                    throw new Exception("Perhitungan tahun berjalan rumah sakit tidak valid.");

                var monthCode = GetMonthCode(now.Month);

                var lastNumberByPrefix = new Dictionary<string, int>();

                foreach (var detail in details)
                {
                    if (!detail.PemeriksaanLabId.HasValue)
                        throw new Exception($"PemeriksaanLabId kosong untuk DetailBookingLabId: {detail.DetailBookingLabId}");

                    if (!kategoriByPemeriksaanLabId.TryGetValue(detail.PemeriksaanLabId.Value, out var kategori))
                        throw new Exception($"Kategori pemeriksaan tidak ditemukan untuk PemeriksaanLabId: {detail.PemeriksaanLabId}");

                    var modality = GetModalityCode(kategori.KodeKategori, kategori.NamaKategori);

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

        public async Task<string> GetLastNoOrderNumberByKunjunganIdAsync(
            Guid kunjunganId,
            CancellationToken cancellationToken = default)
        {
            var existingNoOrders = await _context.LabBookings
                .AsNoTracking()
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.NoOrder != null &&
                    x.NoOrder != "")
                .Select(x => x.NoOrder!)
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
                .Where(x => x.NoPhoto != null && x.NoPhoto.StartsWith(prefix))
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
