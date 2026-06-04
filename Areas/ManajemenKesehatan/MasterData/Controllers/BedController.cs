using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
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
    [EnableCors("FrontendCorsPolicy")]
    public class BedController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<BedController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BedController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BedController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.Beds
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BedId,
                             a.KamarId,
                             a.NomorBed,
                             a.PosisiBed,
                             a.Status,
                             a.Deskripsi,
                         }).OrderByDescending(a => a.CreateDateTime);

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.Beds.Find(id);
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
        public async Task<IActionResult> Create([FromBody] BedViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // Ambil user dari token
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

                // 🔄 Inline logic dari GenerateNomorBed (tanpa method terpisah)
                var kodeKamar = await _applicationDbContext.Kamars
                    .Where(k => k.KamarId == vm.KamarId)
                    .Select(k => k.KodeKamar)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(kodeKamar))
                {
                    return BadRequest(new { message = "Kode kamar tidak ditemukan!" });
                }

                int existingCount = await _applicationDbContext.Beds
                    .Where(b => b.NomorBed.StartsWith(kodeKamar))
                    .CountAsync();

                int nextNumber = existingCount + 1;
                string nomorUrut = nextNumber.ToString("D3");
                string nomorBed = $"{kodeKamar}{nomorUrut}";

                // Buat entitas Bed
                var data = new Bed
                {
                    BedId = Guid.NewGuid(),
                    KamarId = vm.KamarId,
                    NomorBed = nomorBed,
                    PosisiBed = vm.PosisiBed,
                    Status = false,
                    Deskripsi = vm.Deskripsi,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.Beds.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created", data.NomorBed });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BedViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                // Ambil data Bed dari DB
                var existingBed = await _applicationDbContext.Beds.FirstOrDefaultAsync(b => b.BedId == id);
                if (existingBed == null)
                {
                    return NotFound(new { message = "Data bed tidak ditemukan!" });
                }

                bool kamarBerubah = existingBed.KamarId != vm.KamarId;

                if (kamarBerubah)
                {
                    // Jika kamar pindah, regenerasi nomor bed berdasarkan KodeKamar baru
                    var kodeKamar = await _applicationDbContext.Kamars
                        .Where(k => k.KamarId == vm.KamarId)
                        .Select(k => k.KodeKamar)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrWhiteSpace(kodeKamar))
                    {
                        return BadRequest(new { message = "Kode kamar baru tidak ditemukan!" });
                    }

                    int existingCount = await _applicationDbContext.Beds
                        .Where(b => b.NomorBed.StartsWith(kodeKamar))
                        .CountAsync();

                    int nextNumber = existingCount + 1;
                    string nomorUrut = nextNumber.ToString("D3");
                    existingBed.NomorBed = $"{kodeKamar}{nomorUrut}";
                    existingBed.KamarId = vm.KamarId;
                }

                // Update field lainnya
                existingBed.PosisiBed = vm.PosisiBed;
                existingBed.Deskripsi = vm.Deskripsi;

                existingBed.UpdateBy = userActiveId;
                existingBed.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Beds.Update(existingBed);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil", existingBed.NomorBed });
                }
                else
                {
                    return StatusCode(500, new { message = "Update gagal disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan perubahan: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("UpdateStatusBed/{id}")]
        public async Task<IActionResult> UpdateStatusBed(Guid id, StatusBedViewModel vm)
        {
            var data = await _applicationDbContext.Beds.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Bed tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.Status = vm.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status Bed berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.Beds.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Beds.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult Paged(
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

            // Query data
            var query = (from a in _applicationDbContext.Beds
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BedId,
                             a.KamarId,
                             a.NomorBed,
                             a.PosisiBed,
                             a.Status,
                             a.Deskripsi,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KamarId.ToString(), search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
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
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
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

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

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


        [HttpGet("pagedBedInformation")]
        public async Task<IActionResult> PagedGetAllBed(
            int page = 1,
            int perPage = 10,
            Guid? bedId = null,
            Guid? kamarId = null,
            Guid? kelasId = null,
            string? search = null,
            string? namaKamar = null,
            string? orderBy = "TarifHarian",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY + LEFT JOIN yang benar
            // =========================
            var query =
                from a in _applicationDbContext.Beds.AsNoTracking()
                where a.IsDelete != true

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uJoin
                from u in uJoin.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kamars.AsNoTracking()
                    on a.KamarId equals k0.KamarId into kJoin
                from k in kJoin.DefaultIfEmpty()

                join kl0 in _applicationDbContext.Kelass.AsNoTracking()
                    on k.KelasId equals kl0.KelasId into klJoin
                from kl in klJoin.DefaultIfEmpty()

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.BedId,
                    a.NomorBed,
                    a.PosisiBed,
                    a.Status,
                    a.Deskripsi,

                    a.KamarId,
                    NamaKamar = k != null ? k.NamaKamar : null,
                    Lantai = k != null ? k.Lantai : null,
                    PosisiKamar = k != null ? k.PosisiRuangan : null,
                    KodeKamar = k != null ? k.KodeKamar : null,

                    // penting: bikin nullable supaya aman kalau k null
                    TarifHarian = k != null ? (decimal?)k.TarifHarian : null,
                    DeskripsiKamar = k != null ? k.Deskripsi : null,

                    // penting: bikin nullable supaya aman kalau kl null
                    KelasId = kl != null ? (Guid?)kl.KelasId : null,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    KodeKelas = kl != null ? kl.KodeKelas : null,
                    DeskripsiKelas = kl != null ? kl.DeskripsiKelas : null,
                };

            // =========================
            // 2) FILTERS
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var like = $"%{search.Trim()}%";
                query = query.Where(x =>
                    (x.NamaKelas != null && EF.Functions.ILike(x.NamaKelas, like)) ||
                    (x.NamaKamar != null && EF.Functions.ILike(x.NamaKamar, like))
                );
            }

            if (!string.IsNullOrWhiteSpace(namaKamar))
            {
                var like = $"%{namaKamar.Trim()}%";
                query = query.Where(x => x.NamaKamar != null && EF.Functions.ILike(x.NamaKamar, like));
            }

            if (bedId.HasValue && bedId.Value != Guid.Empty)
                query = query.Where(x => x.BedId == bedId.Value);

            if (kamarId.HasValue && kamarId.Value != Guid.Empty)
                query = query.Where(x => x.KamarId == kamarId.Value);

            if (kelasId.HasValue && kelasId.Value != Guid.Empty)
                query = query.Where(x => x.KelasId.HasValue && x.KelasId.Value == kelasId.Value);

            // Date range (sargable)
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date.ToUniversalTime();
                var endExclusive = endDate.Value.Date.AddDays(1).ToUniversalTime();

                DateTimeOffset startOff = new DateTimeOffset(start, TimeSpan.Zero);
                DateTimeOffset endOff = new DateTimeOffset(endExclusive, TimeSpan.Zero);

                query = query.Where(x => x.CreateDateTime >= startOff && x.CreateDateTime < endOff);
            }

            // Periode (pakai range, bukan .Date ==)
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                DateTime start;
                DateTime endExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Yesterday:
                        start = today.AddDays(-1);
                        endExclusive = today;
                        break;

                    case PeriodeFilter.ThisWeek:
                        start = today.AddDays(-(int)today.DayOfWeek);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        endExclusive = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        start = thisMonthStart.AddMonths(-1);
                        endExclusive = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        start = new DateTime(today.Year - 1, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    default:
                        start = DateTime.MinValue;
                        endExclusive = DateTime.MaxValue;
                        break;
                }

                var startOff = new DateTimeOffset(start, TimeSpan.Zero);
                var endOff = new DateTimeOffset(endExclusive, TimeSpan.Zero);

                query = query.Where(x => x.CreateDateTime >= startOff && x.CreateDateTime < endOff);
            }

            // =========================
            // 3) SORT
            // =========================
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = desc
                ? (orderBy == "TarifHarian"
                    ? query.OrderByDescending(x => x.TarifHarian)
                    : query.OrderByDescending(x => x.CreateDateTime))
                : (orderBy == "TarifHarian"
                    ? query.OrderBy(x => x.TarifHarian)
                    : query.OrderBy(x => x.CreateDateTime));

            // =========================
            // 4) PAGING (async)
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
                return NotFound(new { message = "Data tidak ditemukan." });

            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

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

    }
}
