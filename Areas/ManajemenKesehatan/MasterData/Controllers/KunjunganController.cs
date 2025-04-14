using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
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
                        where a.IsDelete == false
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            a.PoliklinikId,
                            a.DokterId,
                            a.PasienId,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.Antrian,
                            a.JumlahKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
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
                    item.DokterId,
                    item.PasienId,
                    item.NoRekamMedis,
                    item.TipePasien,
                    item.TipePembayaran,
                    item.Antrian,
                    JumlahKunjungan = parsedKunjungan,
                    item.CreateDateTime,
                    item.CreateBy,
                    item.CreateByName
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
                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // Atur default jenis kunjungan
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) ||
                                 request.JenisKunjungan.Trim().Equals("string", StringComparison.OrdinalIgnoreCase)
                                 ? "Rawat Jalan"
                                 : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";

                // Ambil nama dokter
                var namaDokter = _applicationDbContext.Dokters
                    .Where(d => d.DokterId == request.DokterId)
                    .Select(d => d.NmDokter)
                    .FirstOrDefault() ?? "Dokter";

                string formatNama = "Dr.";
                if (!string.IsNullOrWhiteSpace(namaDokter))
                {
                    // Hapus awalan jika ada
                    string namaClean = namaDokter
                        .Replace("dr.", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("dr", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("dokter", "", StringComparison.OrdinalIgnoreCase)
                        .Trim();

                    var parts = namaClean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var namaDepan = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0].ToLower());
                        var inisialBelakang = parts[^1].Substring(0, 1).ToUpper();
                        formatNama = $"Dr. {namaDepan} {inisialBelakang}";
                    }
                    else
                    {
                        var namaSatu = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namaClean.ToLower());
                        formatNama = $"Dr. {namaSatu}";
                    }
                }

                // Ambil antrian terakhir berdasarkan DokterId & hari
                var today = DateTime.UtcNow.Date;
                var lastAntrian = _applicationDbContext.Kunjungans
                    .Where(k => k.DokterId == request.DokterId && k.CreateDateTime.Date == today)
                    .OrderByDescending(k => k.CreateDateTime)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastAntrian != null && !string.IsNullOrWhiteSpace(lastAntrian.Antrian))
                {
                    var parts = lastAntrian.Antrian.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int parsed))
                    {
                        nextNumber = parsed + 1;
                    }
                }

                string nomorAntrian = $"{formatNama} - {nextNumber:D3}";

                var kunjunganPasien = _applicationDbContext.Kunjungans
                    .FirstOrDefault(k => k.PasienId == request.PasienId);

                // --- JIKA KUNJUNGAN PERTAMA
                if (kunjunganPasien == null)
                {
                    List<KunjunganRiwayat> newRiwayat = new()
                {
                    new KunjunganRiwayat { Jenis = "IP", Jumlah = kodeJenis == "IP" ? 1 : 0 },
                    new KunjunganRiwayat { Jenis = "OP", Jumlah = kodeJenis == "OP" ? 1 : 0 }
                };

                    var kunjunganBaru = new Kunjungan
                    {
                        KunjunganID = Guid.NewGuid(),
                        PasienId = request.PasienId,
                        DokterId = request.DokterId,
                        PoliklinikId = request.PoliklinikId,
                        AsuransiId = request.AsuransiId,
                        JumlahKunjungan = JsonSerializer.Serialize(newRiwayat),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        NoRekamMedis = request.NoRekamMedis,
                        TipePasien = request.TipePasien,
                        TipePembayaran = request.TipePembayaran,
                        Antrian = nomorAntrian,
                        IsDelete = false
                    };

                    _applicationDbContext.Kunjungans.Add(kunjunganBaru);

                    await _applicationDbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Kunjungan berhasil ditambahkan",
                        data = new
                        {
                            request.PasienId,
                            request.DokterId,
                            NamaDokter = namaDokter,
                            JenisKunjungan = inputJenis,
                            Antrian = nomorAntrian,
                            JumlahKunjungan = newRiwayat
                        }
                    });
                }

                // --- JIKA KUNJUNGAN SUDAH ADA
                List<KunjunganRiwayat> jumlahKunjungan = new()
                {
                    new KunjunganRiwayat { Jenis = "IP", Jumlah = 0 },
                    new KunjunganRiwayat { Jenis = "OP", Jumlah = 0 }
                };

                if (!string.IsNullOrWhiteSpace(kunjunganPasien.JumlahKunjungan))
                {
                    try
                    {
                        jumlahKunjungan = JsonSerializer.Deserialize<List<KunjunganRiwayat>>(kunjunganPasien.JumlahKunjungan)
                                          ?? jumlahKunjungan;
                    }
                    catch
                    {
                        // fallback default list
                    }
                }

                foreach (var jenis in new[] { "IP", "OP" })
                {
                    var current = jumlahKunjungan.FirstOrDefault(k => k.Jenis == jenis);
                    if (current != null)
                    {
                        if (jenis == kodeJenis) current.Jumlah += 1;
                    }
                    else
                    {
                        jumlahKunjungan.Add(new KunjunganRiwayat
                        {
                            Jenis = jenis,
                            Jumlah = jenis == kodeJenis ? 1 : 0
                        });
                    }
                }

                kunjunganPasien.JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan);
                kunjunganPasien.Antrian = nomorAntrian;
                _applicationDbContext.Kunjungans.Update(kunjunganPasien);

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kunjungan berhasil ditambahkan",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        NamaDokter = namaDokter,
                        JenisKunjungan = inputJenis,
                        Antrian = nomorAntrian,
                        JumlahKunjungan = jumlahKunjungan
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
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                // Ambil user login
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Ambil data kunjungan berdasarkan id
                var kunjunganPasien = await _applicationDbContext.Kunjungans.FindAsync(id);
                if (kunjunganPasien == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan!" });
                }

                // Validasi TipePasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // Validasi JenisKunjungan
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) ||
                         request.JenisKunjungan.Trim().Equals("string", StringComparison.OrdinalIgnoreCase)
                         ? "Rawat Jalan"
                         : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";

                // Ambil nama dokter
                var namaDokter = _applicationDbContext.Dokters
                    .Where(d => d.DokterId == request.DokterId)
                    .Select(d => d.NmDokter)
                    .FirstOrDefault() ?? "Dokter";

                string formatNama = "Dr.";
                if (!string.IsNullOrWhiteSpace(namaDokter))
                {
                    string namaClean = namaDokter
                        .Replace("dr.", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("dr", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("dokter", "", StringComparison.OrdinalIgnoreCase)
                        .Trim();

                    var parts = namaClean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var namaDepan = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0].ToLower());
                        var inisialBelakang = parts[^1].Substring(0, 1).ToUpper();
                        formatNama = $"Dr. {namaDepan} {inisialBelakang}";
                    }
                    else
                    {
                        var namaSatu = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namaClean.ToLower());
                        formatNama = $"Dr. {namaSatu}";
                    }
                }

                // Ambil antrian terakhir berdasarkan DokterId & hari
                var today = DateTime.UtcNow.Date;
                var lastAntrian = _applicationDbContext.Kunjungans
                    .Where(k => k.DokterId == request.DokterId && k.CreateDateTime.Date == today)
                    .OrderByDescending(k => k.CreateDateTime)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastAntrian != null && !string.IsNullOrWhiteSpace(lastAntrian.Antrian))
                {
                    var parts = lastAntrian.Antrian.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int parsed))
                    {
                        nextNumber = parsed + 1;
                    }
                }

                string nomorAntrian = $"{formatNama} - {nextNumber:D3}";

                // Deserialize jumlah kunjungan
                List<KunjunganRiwayat> jumlahKunjungan = new()
                {
                    new KunjunganRiwayat { Jenis = "IP", Jumlah = 0 },
                    new KunjunganRiwayat { Jenis = "OP", Jumlah = 0 }
                };

                if (!string.IsNullOrWhiteSpace(kunjunganPasien.JumlahKunjungan))
                {
                    try
                    {
                        jumlahKunjungan = JsonSerializer.Deserialize<List<KunjunganRiwayat>>(kunjunganPasien.JumlahKunjungan)
                                          ?? jumlahKunjungan;
                    }
                    catch
                    {
                        // fallback default
                    }
                }

                // Update jumlah IP/OP
                foreach (var jenis in new[] { "IP", "OP" })
                {
                    var current = jumlahKunjungan.FirstOrDefault(k => k.Jenis == jenis);
                    if (current != null)
                    {
                        if (jenis == kodeJenis) current.Jumlah += 1;
                    }
                    else
                    {
                        jumlahKunjungan.Add(new KunjunganRiwayat
                        {
                            Jenis = jenis,
                            Jumlah = jenis == kodeJenis ? 1 : 0
                        });
                    }
                }

                // Simpan perubahan
                kunjunganPasien.DokterId = request.DokterId;
                kunjunganPasien.PoliklinikId = request.PoliklinikId;
                kunjunganPasien.AsuransiId = request.AsuransiId;
                kunjunganPasien.TipePasien = request.TipePasien;
                kunjunganPasien.TipePembayaran = request.TipePembayaran;
                kunjunganPasien.JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan);
                kunjunganPasien.Antrian = nomorAntrian;
                kunjunganPasien.UpdateDateTime = DateTimeOffset.UtcNow;
                kunjunganPasien.UpdateBy = UserActiveId;

                _applicationDbContext.Kunjungans.Update(kunjunganPasien);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kunjungan berhasil diperbarui",
                    data = new
                    {
                        NamaDokter = namaDokter,
                        JenisKunjungan = inputJenis,
                        Antrian = nomorAntrian,
                        JumlahKunjungan = jumlahKunjungan
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
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
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            a.PoliklinikId,
                            a.DokterId,
                            a.PasienId,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.Antrian,
                            a.JumlahKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.TipePasien.Contains(search) || u.NoRekamMedis.Contains(search));
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

            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderByDescending(u => u.TipePasien),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderBy(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderBy(u => u.TipePasien),
                    _ => query.OrderBy(u => u.CreateDateTime)
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
                    item.NoRekamMedis,
                    item.TipePasien,
                    item.TipePembayaran,
                    item.Antrian,
                    JumlahKunjungan = parsed,
                    item.CreateDateTime,
                    item.CreateBy,
                    item.CreateByName
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
