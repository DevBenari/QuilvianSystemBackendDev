using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services
{
    public interface INoPhotoGeneratorService
    {
        Task<string> GenerateNoPhotoAsync(Guid? pemeriksaanLabId, CancellationToken cancellationToken = default);
        Task<string> GenerateNoOrderByLabIdAsync(Guid labId, CancellationToken cancellationToken = default);

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

        public async Task<string> GenerateNoPhotoAsync(Guid? pemeriksaanLabId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            var kategori = await (
                from p in _context.LabPemeriksaans.AsNoTracking()
                where p.PemeriksaanLabId == pemeriksaanLabId
                select new
                {
                    KodeKategori = p.KategoriPemeriksaan != null ? p.KategoriPemeriksaan.KodeKategori : null,
                    NamaKategori = p.KategoriPemeriksaan != null ? p.KategoriPemeriksaan.NamaKategori : null
                }
            ).FirstOrDefaultAsync(cancellationToken);

            if (kategori == null)
                throw new Exception("Kategori pemeriksaan tidak ditemukan dari PemeriksaanLabId tersebut.");

            var modality = GetModalityCode(kategori.KodeKategori, kategori.NamaKategori);

            var hospitalYear = ReferenceHospitalYear + (now.Year - ReferenceYear);
            if (hospitalYear < 0)
                throw new Exception("Perhitungan tahun berjalan rumah sakit tidak valid.");

            var monthCode = GetMonthCode(now.Month);

            var prefix = $"{modality}{hospitalYear:00}{monthCode}-";

            var lastNoPhoto = await _context.LabBookingDetails
                .AsNoTracking()
                .Where(x => x.NoPhoto != null && x.NoPhoto.StartsWith(prefix))
                .OrderByDescending(x => x.NoPhoto)
                .Select(x => x.NoPhoto)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNoPhoto))
            {
                var match = Regex.Match(lastNoPhoto, @"-(\d+)$");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:000}";
        }

        public async Task<string> GenerateNoOrderByLabIdAsync(Guid labId, CancellationToken cancellationToken = default)
        {
            var lastNoOrder = await _context.LabBookingDetails
                .AsNoTracking()
                .Where(x => x.LabId == labId &&
                            x.NoOrder != null &&
                            x.NoOrder != "")
                .OrderByDescending(x => x.NoOrder)
                .Select(x => x.NoOrder)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNoOrder) &&
                int.TryParse(lastNoOrder, out var parsed))
            {
                nextNumber = parsed + 1;
            }

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
    }
}
