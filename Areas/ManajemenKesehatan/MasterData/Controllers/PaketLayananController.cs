using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PaketLayananController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PaketLayananController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public PaketLayananController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PaketLayananController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var header = await (
                    from a in _applicationDbContext.PaketLayanans.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.PaketLayananId == id && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.PaketLayananId,
                        a.KodePaketLayanan,
                        a.NamaPaketLayanan,
                        a.IsMCU,
                        a.TglPembuatan,
                        a.Keterangan,

                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    }
                ).FirstOrDefaultAsync();

                if (header == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var details = await (
                    from d in _applicationDbContext.PaketLayananDetails.AsNoTracking()
                    where d.DetailPaketId == id && (d.IsDelete == false || d.IsDelete == null)
                    select new
                    {
                        d.DetailPaketLayananId,
                        d.DetailPaketId,
                        d.LayananId,
                        d.TglPembuatan,
                        d.Keterangan,

                        d.CreateBy,
                        d.CreateDateTime
                        // kalau ada:
                        // d.UpdateBy,
                        // d.UpdateDateTime
                    }
                ).ToListAsync();

                return Ok(new
                {
                    message = "Ditemukan || 200 OK",
                    data = new
                    {
                        header,
                        details
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaketLayananVM req)
        {
            try
            {
                if (req == null)
                    return BadRequest(new { message = "Request tidak valid." });

                if (req.Details == null || !req.Details.Any())
                    return BadRequest(new { message = "Details wajib diisi minimal 1." });

                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                var paketId = Guid.NewGuid();
                var now = DateTime.Now;

                var headerEntity = new PaketLayanan
                {
                    PaketLayananId = paketId,
                    KodePaketLayanan = req.KodePaketLayanan,
                    NamaPaketLayanan = req.NamaPaketLayanan,
                    TglPembuatan = req.TglPembuatan ?? now,
                    IsMCU = req.IsMCU,
                    Keterangan = req.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = now,
                    IsDelete = false
                };

                var detailEntities = req.Details.Select(d => new PaketLayananDetail
                {
                    DetailPaketLayananId = Guid.NewGuid(),

                    // relasi ke header TANPA nambah kolom baru:
                    DetailPaketId = paketId,

                    LayananId = d.LayananId,
                    TglPembuatan = now,
                    Keterangan = d.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = now,
                    IsDelete = false
                }).ToList();

                // Insert sekaligus (1 kali SaveChanges)
                _applicationDbContext.PaketLayanans.Add(headerEntity);
                _applicationDbContext.PaketLayananDetails.AddRange(detailEntities);

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil disimpan || 200 OK",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] PaketLayananVM req)
        {
            try
            {
                if (req == null)
                    return BadRequest(new { message = "Request tidak valid." });

                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

                // === HEADER ===
                var headerEntity = await _applicationDbContext.PaketLayanans
                    .FirstOrDefaultAsync(x => x.PaketLayananId == id && (x.IsDelete == false || x.IsDelete == null));

                if (headerEntity == null)
                    return NotFound(new { message = "Data tidak ditemukan." });


                headerEntity.NamaPaketLayanan = req.NamaPaketLayanan;
                headerEntity.TglPembuatan = req.TglPembuatan ?? headerEntity.TglPembuatan;
                headerEntity.IsMCU = req.IsMCU;
                headerEntity.Keterangan = req.Keterangan;


                headerEntity.UpdateBy = userActiveId;
                headerEntity.UpdateDateTime = DateTimeOffset.UtcNow;

                // === DETAILS (UPSERT + SOFT DELETE) ===
                var existingDetails = await _applicationDbContext.PaketLayananDetails
                    .Where(d => d.DetailPaketId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                var incoming = req.Details ?? new List<PaketLayananDetailVM>();

                // Update / Insert detail yang dikirim
                foreach (var d in incoming)
                {
                    PaketLayananDetail? existing = null;

                    if (d.DetailPaketId.HasValue)
                    {
                        existing = existingDetails.FirstOrDefault(x => x.DetailPaketId == d.DetailPaketId);
                    }

                    if (existing != null)
                    {
                        // UPDATE existing detail
                        existing.LayananId = d.LayananId;
                        existing.Keterangan = d.Keterangan;
                        existing.UpdateBy = userActiveId;
                        existing.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        // INSERT detail baru
                        var newDetail = new PaketLayananDetail
                        {
                            DetailPaketLayananId = Guid.NewGuid(),
                            DetailPaketId = id,              // relasi ke header tanpa tambah kolom baru
                            LayananId = d.LayananId,
                            TglPembuatan = DateTime.UtcNow,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            IsDelete = false
                        };

                        _applicationDbContext.PaketLayananDetails.Add(newDetail);
                    }
                }

                // Soft delete detail yang lama tapi tidak dikirim lagi
                // (kalau kamu TIDAK mau auto-soft-delete, kamu bisa hapus blok ini)
                var incomingKeys = new HashSet<Guid>(
                    incoming.Where(x => x.DetailPaketId.HasValue).Select(x => x.DetailPaketId!.Value)
                );

                foreach (var ex in existingDetails)
                {
                    // kalau tidak ada di request => tandai delete
                    if (!incomingKeys.Contains((Guid)ex.DetailPaketId))
                    {
                        ex.IsDelete = true;
                        ex.UpdateBy = userActiveId;
                        ex.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                // Response seperti gaya kamu: header + details
                var header = new
                {
                    headerEntity.PaketLayananId,
                    headerEntity.KodePaketLayanan,
                    headerEntity.NamaPaketLayanan,
                    headerEntity.TglPembuatan,
                    headerEntity.Keterangan,
                    headerEntity.CreateBy,
                    headerEntity.CreateDateTime,
                    headerEntity.UpdateBy,
                    headerEntity.UpdateDateTime
                };

                var details = await _applicationDbContext.PaketLayananDetails.AsNoTracking()
                    .Where(d => d.DetailPaketId == id && (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DetailPaketLayananId,
                        d.DetailPaketId,
                        d.LayananId,
                        d.TglPembuatan,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Berhasil diupdate || 200 OK",
                    data = new { header, details }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

                // === HEADER ===
                var header = await _applicationDbContext.PaketLayanans
                    .FirstOrDefaultAsync(x =>
                        x.PaketLayananId == id && (x.IsDelete == false || x.IsDelete == null));

                if (header == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // === DETAILS ===
                var details = await _applicationDbContext.PaketLayananDetails
                    .Where(d => d.DetailPaketId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                // Soft delete header
                header.IsDelete = true;
                header.UpdateBy = userActiveId;
                header.UpdateDateTime = DateTimeOffset.UtcNow;

                // Soft delete details
                foreach (var d in details)
                {
                    d.IsDelete = true;
                    d.UpdateBy = userActiveId;
                    d.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Berhasil dihapus (soft delete) || 200 OK",
                    data = new
                    {
                        PaketLayananId = id,
                        DeletedDetailsCount = details.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =========================
                // 1) BASE QUERY (HEADERS)
                // =========================
                var query =
                    from a in _applicationDbContext.PaketLayanans.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.UpdateBy,
                        a.UpdateDateTime,

                        a.PaketLayananId,
                        a.KodePaketLayanan,
                        a.NamaPaketLayanan,
                        a.IsMCU,
                        a.TglPembuatan,
                        a.Keterangan
                    };

                // =========================
                // 2) SEARCH
                // =========================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim()}%";
                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePaketLayanan!, search) ||
                        EF.Functions.ILike(x.NamaPaketLayanan!, search) ||
                        EF.Functions.ILike(x.CreateByName!, search) ||
                        EF.Functions.ILike(x.Keterangan!, search));
                }

                // =========================
                // 3) DATE RANGE FILTER (pakai range, lebih index-friendly)
                // =========================
                // Asumsi CreateDateTime disimpan UTC (kalau bukan, hapus ToUniversalTime)
                if (startDate.HasValue || endDate.HasValue)
                {
                    var startUtc = startDate?.Date.ToUniversalTime();
                    var endUtcExclusive = endDate?.Date.AddDays(1).ToUniversalTime(); // exclusive

                    if (startUtc.HasValue)
                        query = query.Where(x => x.CreateDateTime >= startUtc.Value);

                    if (endUtcExclusive.HasValue)
                        query = query.Where(x => x.CreateDateTime < endUtcExclusive.Value);
                }

                // =========================
                // 4) PERIODE FILTER (hindari .Date/.Month/.Year pada kolom)
                // =========================
                if (periode.HasValue)
                {
                    var today = DateTime.UtcNow.Date;

                    DateTime? pStart = null;
                    DateTime? pEndExclusive = null;

                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            pStart = today;
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.ThisWeek:
                            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                            pStart = startOfWeek;
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.LastWeek:
                            var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                            pStart = startThisWeek.AddDays(-7);
                            pEndExclusive = startThisWeek;
                            break;

                        case PeriodeFilter.ThisMonth:
                            var startMonth = new DateTime(today.Year, today.Month, 1);
                            pStart = startMonth;
                            pEndExclusive = startMonth.AddMonths(1);
                            break;

                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            var startLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                            pStart = startLastMonth;
                            pEndExclusive = startLastMonth.AddMonths(1);
                            break;

                        case PeriodeFilter.ThisYear:
                            var startYear = new DateTime(today.Year, 1, 1);
                            pStart = startYear;
                            pEndExclusive = startYear.AddYears(1);
                            break;

                        case PeriodeFilter.LastYear:
                            var startLastYear = new DateTime(today.Year - 1, 1, 1);
                            pStart = startLastYear;
                            pEndExclusive = startLastYear.AddYears(1);
                            break;

                        case PeriodeFilter.Last3Months:
                            pStart = today.AddMonths(-3);
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Last6Months:
                            pStart = today.AddMonths(-6);
                            pEndExclusive = today.AddDays(1);
                            break;
                    }

                    if (pStart.HasValue)
                        query = query.Where(x => x.CreateDateTime >= pStart.Value);

                    if (pEndExclusive.HasValue)
                        query = query.Where(x => x.CreateDateTime < pEndExclusive.Value);
                }

                // =========================
                // 5) ORDERING
                // =========================
                var asc = sortDirection?.ToLower() == "asc";

                query = (orderBy, asc) switch
                {
                    ("CreateDateTime", true) => query.OrderBy(x => x.CreateDateTime),
                    ("CreateDateTime", false) => query.OrderByDescending(x => x.CreateDateTime),

                    ("CreateByName", true) => query.OrderBy(x => x.CreateByName),
                    ("CreateByName", false) => query.OrderByDescending(x => x.CreateByName),

                    ("KodePaketLayanan", true) => query.OrderBy(x => x.KodePaketLayanan),
                    ("KodePaketLayanan", false) => query.OrderByDescending(x => x.KodePaketLayanan),

                    ("NamaPaketLayanan", true) => query.OrderBy(x => x.NamaPaketLayanan),
                    ("NamaPaketLayanan", false) => query.OrderByDescending(x => x.NamaPaketLayanan),

                    ("TglPembuatan", true) => query.OrderBy(x => x.TglPembuatan),
                    ("TglPembuatan", false) => query.OrderByDescending(x => x.TglPembuatan),

                    _ when asc => query.OrderBy(x => x.CreateDateTime),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                };

                // =========================
                // 6) PAGINATION + COUNT
                // =========================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var headers = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (headers.Count == 0 && page > totalPages && totalRows > 0)
                    return NotFound(new { message = "Page not found." });

                if (headers.Count == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "Data retrieved successfully",
                        data = new
                        {
                            Rows = new List<object>(),
                            TotalRows = totalRows,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = totalPages
                        }
                    });
                }

                // =========================
                // 7) DETAILS: 1 query saja (tetap bukan N+1)
                // =========================
                var paketIds = headers.Select(x => x.PaketLayananId).ToList();

                var details = await _applicationDbContext.PaketLayananDetails
                    .AsNoTracking()
                    .Where(d =>
                        d.DetailPaketId.HasValue &&
                        paketIds.Contains(d.DetailPaketId.Value) &&
                        (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DetailPaketLayananId,
                        d.DetailPaketId,
                        d.LayananId,
                        d.TglPembuatan,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    })
                    .OrderByDescending(d => d.CreateDateTime)
                    .ToListAsync();

                // =========================
                // 8) MAPPING O(H + D)
                // =========================
                var detailLookup = details
                    .GroupBy(d => d.DetailPaketId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (object)x).ToList()   // <-- paksa jadi List<object>
                    );

                var rows = headers.Select(h => new
                {
                    h.CreateDateTime,
                    h.CreateBy,
                    h.CreateByName,
                    h.UpdateBy,
                    h.UpdateDateTime,

                    h.PaketLayananId,
                    h.KodePaketLayanan,
                    h.NamaPaketLayanan,
                    h.TglPembuatan,
                    h.Keterangan,

                    Details = detailLookup.TryGetValue(h.PaketLayananId, out var list)
                        ? list
                        : new List<object>()
                }).ToList();

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = rows,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

    }
}
