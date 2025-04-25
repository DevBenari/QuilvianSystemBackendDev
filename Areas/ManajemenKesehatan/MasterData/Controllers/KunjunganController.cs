using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KunjunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<KunjunganController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKunjungan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join p in _applicationDbContext.Polikliniks
                        on a.PoliklinikId equals p.PoliklinikId
                        join ps in _applicationDbContext.PendaftaranPasienBarus
                        on a.PasienId equals ps.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters
                        on a.DokterId equals d.DokterId
                        where a.IsDelete == false
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            a.PoliklinikId,
                            p.NamaPoliklinik,

                            a.DokterId,
                            a.PasienId,
                            ps.NamaLengkap,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.JumlahKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            a.IsFinished,
                            a.IsScreening,
                            a.IsPresent,
                            a.Antrian,
                            d.NmDokter,
                            gambardokter = !string.IsNullOrEmpty(d.FotoName)
                            ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                            : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                            CreateByName = u.FullName
                        };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rawData = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            // Deserialize JumlahKunjungan per item
            var listdata = rawData.Select(item =>
            {
                List<KunjunganRiwayat> parsedKunjungan = new();
                if (!string.IsNullOrWhiteSpace(item.JumlahKunjungan))
                {
                    try
                    {
                        parsedKunjungan = JsonSerializer.Deserialize<List<KunjunganRiwayat>>(item.JumlahKunjungan)
                                          ?? new List<KunjunganRiwayat>();
                    }
                    catch
                    {
                        parsedKunjungan = new(); // fallback jika JSON invalid
                    }
                }

                return new
                {
                    item.KunjunganID,
                    item.AsuransiId,
                    item.PoliklinikId,
                    item.NamaPoliklinik,
                    item.DokterId,
                    item.PasienId,
                    item.NamaLengkap,
                    item.IsScreening,
                    item.IsPresent,

                    item.NoRekamMedis,
                    item.TipePasien,
                    item.TipePembayaran,
                    JumlahKunjungan = parsedKunjungan,
                    item.CreateDateTime,
                    item.CreateBy,
                    item.CreateByName,

                    item.NmDokter,
                    item.gambardokter,

                    item.Antrian,
                    item.IsFinished,
                };
            }).ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKunjunganPasienById(Guid id)
        {
            var listdata = _applicationDbContext.Kunjungans.Find(id);
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateKunjunganPasien([FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive?.UserActiveId ?? Guid.Empty;

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) || request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";


                // Ambil kode antrean dari tabel Poliklinik
                var kodePoli = _applicationDbContext.Polikliniks
                    .Where(p => p.PoliklinikId == request.PoliklinikId)
                    .Select(p => p.KodeAntreanPoli)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(kodePoli))
                    return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                // Hitung nomor antrian hari ini berdasarkan Poliklinik
                var today = DateTime.UtcNow.Date;

                var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                    .Count(k => k.PoliklinikId == request.PoliklinikId
                                && k.CreateDateTime.Date == today
                                && !k.IsDelete);

                int nomorAntrian = jumlahAntrianHariIni + 1;
                string nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}"; // Contoh: BU001

                // Hitung jumlah kunjungan IP dan OP dari dokter hari ini
                var allKunjunganPasien = _applicationDbContext.Kunjungans
                    .Where(k => k.PoliklinikId == request.PoliklinikId && k.CreateDateTime.Date == today && k.IsDelete == false)
                    .ToList();

                List<KunjunganRiwayat> jumlahKunjungan = new()
                {
                    new KunjunganRiwayat
                    {
                        Jenis = "IP",
                        Jumlah = allKunjunganPasien
                            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                            .Where(k => k.Jenis == "IP")
                            .Sum(k => k.Jumlah)
                    },
                    new KunjunganRiwayat
                    {
                        Jenis = "OP",
                        Jumlah = allKunjunganPasien
                            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                            .Where(k => k.Jenis == "OP")
                            .Sum(k => k.Jumlah)
                    }
                };

                // Tambahkan ke jenis kunjungan saat ini
                var currentJenis = jumlahKunjungan.FirstOrDefault(k => k.Jenis == kodeJenis);
                if (currentJenis != null)
                {
                    currentJenis.Jumlah += 1;
                }
                else
                {
                    jumlahKunjungan.Add(new KunjunganRiwayat { Jenis = kodeJenis, Jumlah = 1 });
                }

                var newKunjungan = new Kunjungan
                {
                    KunjunganID = Guid.NewGuid(),
                    PasienId = request.PasienId,
                    DokterId = request.DokterId,
                    PoliklinikId = request.PoliklinikId,
                    AsuransiId = request.AsuransiId,
                    JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan),
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = UserActiveId,
                    NoRekamMedis = request.NoRekamMedis,
                    TipePasien = request.TipePasien,
                    TipePembayaran = request.TipePembayaran,
                    IsFinished = false,
                    IsDelete = false,
                    IsScreening = false,
                    IsPresent = true,
                    Antrian = nomorAntrianFormatted   // Format akhir: BU001
                };

                _applicationDbContext.Kunjungans.Add(newKunjungan);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kunjungan baru berhasil ditambahkan.",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        JenisKunjungan = inputJenis,
                        JumlahKunjungan = jumlahKunjungan,
                        NomorAntrian = nomorAntrianFormatted
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKunjunganPasien(Guid id, [FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
                return BadRequest(new { message = "Data tidak boleh kosong!" });

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                var userActiveId = getUserActive?.UserActiveId ?? Guid.Empty;

                var existingKunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
                if (existingKunjungan == null)
                    return NotFound(new { message = "Data kunjungan tidak ditemukan!" });

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });

                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) || request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";

                // Ambil kode antrean dari tabel Poliklinik
                var kodePoli = _applicationDbContext.Polikliniks
                    .Where(p => p.PoliklinikId == request.PoliklinikId)
                    .Select(p => p.KodeAntreanPoli)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(kodePoli))
                    return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                // Hitung nomor antrian hari ini berdasarkan Poliklinik
                var today = DateTime.UtcNow.Date;

                var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                    .Count(k => k.PoliklinikId == request.PoliklinikId
                                && k.CreateDateTime.Date == today
                                && !k.IsDelete);

                int nomorAntrian = jumlahAntrianHariIni + 1;
                string nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}";

                // Hitung jumlah kunjungan berdasarkan poliklinik hari ini
                var allKunjunganPasien = _applicationDbContext.Kunjungans
                    .Where(k => k.PoliklinikId == request.PoliklinikId && k.CreateDateTime.Date == today && !k.IsDelete)
                    .ToList();

                List<KunjunganRiwayat> jumlahKunjungan = new()
                {
                    new KunjunganRiwayat
                    {
                        Jenis = "IP",
                        Jumlah = allKunjunganPasien
                            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                            .Where(k => k.Jenis == "IP")
                            .Sum(k => k.Jumlah)
                    },
                    new KunjunganRiwayat
                    {
                        Jenis = "OP",
                        Jumlah = allKunjunganPasien
                            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                            .Where(k => k.Jenis == "OP")
                            .Sum(k => k.Jumlah)
                    }
                };

                var currentJenis = jumlahKunjungan.FirstOrDefault(k => k.Jenis == kodeJenis);
                if (currentJenis != null)
                    currentJenis.Jumlah += 1;
                else
                    jumlahKunjungan.Add(new KunjunganRiwayat { Jenis = kodeJenis, Jumlah = 1 });

                // Update semua field
                existingKunjungan.PasienId = request.PasienId;
                existingKunjungan.DokterId = request.DokterId;
                existingKunjungan.PoliklinikId = request.PoliklinikId;
                existingKunjungan.AsuransiId = request.AsuransiId;
                existingKunjungan.NoRekamMedis = request.NoRekamMedis;
                existingKunjungan.TipePasien = request.TipePasien;
                existingKunjungan.TipePembayaran = request.TipePembayaran;
               
                existingKunjungan.JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan);
                existingKunjungan.Antrian = nomorAntrianFormatted;
                existingKunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
                existingKunjungan.UpdateBy = userActiveId;

                //existingKunjungan.IsFinished = request.IsFinished;
                //existingKunjungan.IsScreening = request.IsScreening;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Data kunjungan berhasil diperbarui.",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        request.PoliklinikId,
                        JenisKunjungan = inputJenis,
                        JumlahKunjungan = jumlahKunjungan,
                        NomorAntrian = nomorAntrianFormatted
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}/is-finished")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] UpdateIsFinishedViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsFinished = request.IsFinished;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/is-screening")]
        public async Task<IActionResult> UpdateIsScreening(Guid id, [FromBody] UpdateIsScreeningViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsScreening = request.IsScreening;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpPut("{id}/is-present")]
        public async Task<IActionResult> UpdateIsPresent(Guid id, [FromBody] UpdateIsPresentViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsPresent = request.IsPresent;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Dokter**
                var data = _applicationDbContext.Kunjungans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Kunjungans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedKunjungan(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null)
        {
            var query = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join p in _applicationDbContext.Polikliniks
                        on a.PoliklinikId equals p.PoliklinikId
                        join ps in _applicationDbContext.PendaftaranPasienBarus
                        on a.PasienId equals ps.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters
                        on a.DokterId equals d.DokterId
                        where a.IsDelete == false
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            a.PoliklinikId,
                            p.NamaPoliklinik,
                            a.DokterId,
                            a.PasienId,
                            ps.NamaLengkap,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.JumlahKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            a.IsFinished,
                            a.IsScreening,
                            a.IsPresent,
                            a.Antrian,
                            d.NmDokter,
                            gambardokter = !string.IsNullOrEmpty(d.FotoName)
                            ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                            : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                            CreateByName = u.FullName
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLengkap, search) ||
                    EF.Functions.ILike(u.NmDokter, search) ||
                    EF.Functions.ILike(u.NoRekamMedis, search) ||
                    EF.Functions.ILike(u.NamaPoliklinik, search) ||
                    EF.Functions.ILike(u.Antrian, search)
                );
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek)));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.AddMonths(-1).Month &&
                            u.CreateDateTime.Year == today.AddMonths(-1).Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            query = sortDirection?.ToLower() == "asc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderBy(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderBy(u => u.TipePasien),
                    "Nama Dokter" => query.OrderBy(u => u.NmDokter),
                    "Nama Poliklinik" => query.OrderBy(u => u.NamaPoliklinik),
                    _ => query.OrderBy(u => u.CreateDateTime)

                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderByDescending(u => u.TipePasien),
                    "Nama Dokter" => query.OrderByDescending(u => u.NmDokter),
                    "Nama Poliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rawData = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            // Deserialize JumlahKunjungan per baris
            var result = rawData.Select(item =>
            {
                List<KunjunganRiwayat> parsed = new();
                if (!string.IsNullOrWhiteSpace(item.JumlahKunjungan))
                {
                    try
                    {
                        parsed = JsonSerializer.Deserialize<List<KunjunganRiwayat>>(item.JumlahKunjungan) ?? new();
                    }
                    catch
                    {
                        parsed = new();
                    }
                }

                return new
                {
                    item.KunjunganID,
                    item.AsuransiId,
                    item.PoliklinikId,
                    item.DokterId,
                    item.PasienId,
                    item.NamaLengkap,
                    item.NoRekamMedis,
                    item.TipePasien,
                    item.TipePembayaran,
                    JumlahKunjungan = parsed,
                    item.CreateDateTime,
                    item.CreateBy,
                    item.CreateByName,
                    item.NmDokter,
                    item.NamaPoliklinik,
                    item.Antrian,
                    item.IsScreening,
                    item.IsFinished,
                    item.IsPresent,
                    item.gambardokter,
                };
            }).ToList();

            if (!result.Any() && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = result,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
