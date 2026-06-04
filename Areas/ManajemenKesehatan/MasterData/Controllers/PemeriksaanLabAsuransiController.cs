using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PemeriksaanLabAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PemeriksaanLabAsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PemeriksaanLabAsuransiController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PemeriksaanLabAsuransiController> logger,
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
        public async Task<IActionResult> GetAlL(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.PemeriksaanLabAsuransis
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.PemeriksaanLabAsuransiId,
                            a.AsuransiId,
                            a.PemeriksaanLabId,

                            // ============================
                            // MARKUP
                            // ============================
                            a.MarkupDokter,
                            a.MarkupRs,
                            a.MarkupJp,
                            a.MarkupBahp,
                            a.MarkupLainnya,
                            a.MarkupTotal,
                            a.IsMarkupBerlaku,
                            a.MarkupDari,
                            a.MarkupSampai,

                            // ============================
                            // DISKON
                            // ============================
                            a.DiskonDokter,
                            a.DiskonRs,
                            a.DiskonJp,
                            a.DiskonBahp,
                            a.DiskonTotal,
                            a.IsDiskonBerlaku,
                            a.DiskonDari,
                            a.DiskonSampai
                        };

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

        [HttpPost]
        public async Task<IActionResult> CreateTindakanAsuransi([FromBody] PemeriksaanLabAsuransiVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
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

                // **Cek Duplikasi**
                bool isDuplicate = _applicationDbContext.PemeriksaanLabAsuransis
                    .Any(c => c.PemeriksaanLabId == vm.PemeriksaanLabId && c.AsuransiId == vm.AsuransiId && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new PemeriksaanLabAsuransi
                {
                    PemeriksaanLabAsuransiId = Guid.NewGuid(),
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    AsuransiId = vm.AsuransiId,

                    // ============================
                    // MARKUP
                    // ============================
                    MarkupDokter = vm.MarkupDokter,
                    MarkupRs = vm.MarkupRs,
                    MarkupJp = vm.MarkupJp,
                    MarkupBahp = vm.MarkupBahp,
                    MarkupLainnya = vm.MarkupLainnya,
                    MarkupTotal = vm.MarkupTotal,

                    IsMarkupBerlaku = vm.IsMarkupBerlaku ,
                    MarkupDari = vm.MarkupDari,
                    MarkupSampai = vm.MarkupSampai,

                    // ============================
                    // DISKON
                    // ============================
                    DiskonDokter = vm.DiskonDokter,
                    DiskonRs = vm.DiskonRs,
                    DiskonJp = vm.DiskonJp,
                    DiskonBahp = vm.DiskonBahp,
                    DiskonTotal = vm.DiskonTotal,

                    IsDiskonBerlaku = vm.IsDiskonBerlaku,
                    DiskonDari = vm.DiskonDari,
                    DiskonSampai = vm.DiskonSampai,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId
                };

                // **Simpan ke Database**
                _applicationDbContext.PemeriksaanLabAsuransis.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Relasi Berhasil || 201 Created" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPemeriksaanLabAsuransiById(Guid id)
        {
            var data = await _applicationDbContext.PemeriksaanLabAsuransis
                .Where(t => t.PemeriksaanLabId == id && !t.IsDelete)
                .ToListAsync();  // Mengambil semua data yang sesuai dalam bentuk list

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new { message = "Data ditemukan || 200 OK", data });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTindakanAsuransi(Guid id)
        {
            try
            {
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

                // **Cari Data Relasi**
                var data = await _applicationDbContext.PemeriksaanLabAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PemeriksaanLabAsuransis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePemeriksaanLabAsuransi(Guid id, [FromBody] PemeriksaanLabAsuransiVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // ======================================================
                // 1) Ambil data lama yang mau diupdate
                // ======================================================
                var data = await _applicationDbContext.PemeriksaanLabAsuransis
                    .FirstOrDefaultAsync(x => x.PemeriksaanLabAsuransiId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = $"Data relasi dengan ID {id} tidak ditemukan || 404 Not Found" });
                }

                // ======================================================
                // 2) Cek duplikasi (kecuali data yang sedang diupdate)
                // ======================================================
                bool isDuplicate = await _applicationDbContext.PemeriksaanLabAsuransis
                    .AnyAsync(c =>
                        c.PemeriksaanLabAsuransiId != id &&
                        c.PemeriksaanLabId == vm.PemeriksaanLabId &&
                        c.AsuransiId == vm.AsuransiId &&
                        c.IsDelete == false
                    );

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // ======================================================
                // 3) Update data
                // ======================================================
                data.PemeriksaanLabId = vm.PemeriksaanLabId;
                data.AsuransiId = vm.AsuransiId;
                // ============================
                // MARKUP
                // ============================
                data.MarkupDokter = vm.MarkupDokter;
                data.MarkupRs = vm.MarkupRs;
                data.MarkupJp = vm.MarkupJp;
                data.MarkupBahp = vm.MarkupBahp;
                data.MarkupLainnya = vm.MarkupLainnya;
                data.MarkupTotal = vm.MarkupTotal;

                data.IsMarkupBerlaku = vm.IsMarkupBerlaku;
                data.MarkupDari = vm.MarkupDari;
                data.MarkupSampai = vm.MarkupSampai;

                // ============================
                // DISKON
                // ============================
                data.DiskonDokter = vm.DiskonDokter;
                data.DiskonRs = vm.DiskonRs;
                data.DiskonJp = vm.DiskonJp;
                data.DiskonBahp = vm.DiskonBahp;
                data.DiskonTotal = vm.DiskonTotal;

                data.IsDiskonBerlaku = vm.IsDiskonBerlaku ;
                data.DiskonDari = vm.DiskonDari;
                data.DiskonSampai = vm.DiskonSampai;


                // audit update (jika ada fieldnya)
                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId;

                // ======================================================
                // 4) Save
                // ======================================================
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Relasi Berhasil || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diupdate." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedPemeriksaanLabAsuransi(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] Guid? asuransiId = null,
            [FromQuery] Guid? pemeriksaanLabId = null,
            [FromQuery] Guid? labId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =====================================================
            // 1) BASE ROWS (dipakai untuk filter/search, lalu digroup jadi parent)
            //    Source utama tetap PemeriksaanLabAsuransis (mapping)
            // =====================================================
            var rowsQ =
                from a in _applicationDbContext.PemeriksaanLabAsuransis.AsNoTracking()
                where (a.IsDelete == false || a.IsDelete == null)

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into ug
                from u in ug.DefaultIfEmpty()

                    // Asuransi (LEFT JOIN) - untuk search/filter & nanti child query
                join as0 in _applicationDbContext.Asuransis.AsNoTracking()
                    on a.AsuransiId equals as0.AsuransiId into ag
                from asu in ag.DefaultIfEmpty()

                    // Pemeriksaan (LEFT JOIN)
                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on a.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpg
                from lp in lpg.DefaultIfEmpty()

                    // Kategori (LEFT JOIN)
                join k0 in _applicationDbContext.LabKategoriPemeriksaans.AsNoTracking()
                    on lp.KategoriPemeriksaanId equals k0.KategoriPemeriksaanId into kg
                from k in kg.DefaultIfEmpty()

                    // Lab (LEFT JOIN)
                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on k.LabId equals l0.LabId into lg
                from l in lg.DefaultIfEmpty()

                select new
                {
                    a.CreateDateTime,
                    a.AsuransiId,
                    NamaAsuransi = asu != null ? asu.NamaAsuransi : null,

                    a.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    KodePemeriksaan = lp != null ? lp.KodePemeriksaan : null,
                    HargaPemeriksaan = lp.HargaPemeriksaan,

                    KategoriId = k.KategoriPemeriksaanId,
                    NamaKategori = k.NamaKategori,

                    LabId = (Guid?)l.LabId,
                    NamaLab = l != null ? l.NamaLab : null,

                    CreateByName = u != null ? u.FullName : null,
                };

            // =====================================================
            // FILTERS (server-side)
            // =====================================================
            if (asuransiId.HasValue && asuransiId.Value != Guid.Empty)
                rowsQ = rowsQ.Where(x => x.AsuransiId == asuransiId.Value);

            if (pemeriksaanLabId.HasValue && pemeriksaanLabId.Value != Guid.Empty)
                rowsQ = rowsQ.Where(x => x.PemeriksaanLabId == pemeriksaanLabId.Value);

            if (labId.HasValue && labId.Value != Guid.Empty)
                rowsQ = rowsQ.Where(x => x.LabId == labId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                rowsQ = rowsQ.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;
                    case PeriodeFilter.Yesterday:
                        start = todayStart.AddDays(-1); end = todayStart; break;
                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;
                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;
                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;
                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;
                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;
                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;
                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;
                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;
                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                rowsQ = rowsQ.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                rowsQ = rowsQ.Where(x =>
                    EF.Functions.ILike(x.NamaAsuransi ?? "", pattern) ||
                    EF.Functions.ILike(x.NamaLab ?? "", pattern) ||
                    EF.Functions.ILike(x.NamaPemeriksaan ?? "", pattern) ||
                    EF.Functions.ILike(x.CreateByName ?? "", pattern)
                );
            }

            // =====================================================
            // 2) PARENT QUERY: group per PemeriksaanLabId (1 pemeriksaan tampil 1x)
            //    Sort default pakai latest CreateDateTime mapping (paling masuk akal)
            // =====================================================
            var parentQ =
                from r in rowsQ
                group r by new { r.PemeriksaanLabId, r.NamaPemeriksaan, r.LabId, r.NamaLab } into g
                select new
                {
                    g.Key.PemeriksaanLabId,
                    g.Key.NamaPemeriksaan,
                    g.Key.LabId,
                    g.Key.NamaLab,
                    LastCreateDateTime = g.Max(x => x.CreateDateTime)
                };

            bool desc = (sortDirection ?? "desc").ToLower() == "desc";
            parentQ = (orderBy ?? "CreateDateTime") switch
            {
                "NamaLab" => desc ? parentQ.OrderByDescending(x => x.NamaLab) : parentQ.OrderBy(x => x.NamaLab),
                "NamaPemeriksaan" => desc ? parentQ.OrderByDescending(x => x.NamaPemeriksaan) : parentQ.OrderBy(x => x.NamaPemeriksaan),
                "CreateDateTime" or _ => desc ? parentQ.OrderByDescending(x => x.LastCreateDateTime) : parentQ.OrderBy(x => x.LastCreateDateTime),
            };

            var totalRows = await parentQ.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parents = await parentQ
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parents.Count == 0)
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            var pemeriksaanIds = parents.Select(x => x.PemeriksaanLabId).Distinct().ToList();

            // =====================================================
            // 3) CHILD QUERY: ambil semua Asuransi untuk pemeriksaan yang tampil (bulk)
            //    (ini yang kamu maksud “lookup AsuransiId”)
            // =====================================================
            var childRows = await (
                from a in _applicationDbContext.PemeriksaanLabAsuransis.AsNoTracking()
                where (a.IsDelete == false || a.IsDelete == null)
                      && a.PemeriksaanLabId != null
                      && pemeriksaanIds.Contains(a.PemeriksaanLabId.Value)

                join as0 in _applicationDbContext.Asuransis.AsNoTracking()
                    on a.AsuransiId equals as0.AsuransiId into ag
                from asu in ag.DefaultIfEmpty()

                select new
                {
                    PemeriksaanLabId = a.PemeriksaanLabId!.Value,
                    a.PemeriksaanLabAsuransiId,
                    a.AsuransiId,
                    NamaAsuransi = asu != null ? asu.NamaAsuransi : null,

                    // MARKUP
                    a.MarkupDokter,
                    a.MarkupRs,
                    a.MarkupJp,
                    a.MarkupBahp,
                    a.MarkupLainnya,
                    a.MarkupTotal,
                    a.IsMarkupBerlaku,
                    a.MarkupDari,
                    a.MarkupSampai,

                    // DISKON
                    a.DiskonDokter,
                    a.DiskonRs,
                    a.DiskonJp,
                    a.DiskonBahp,
                    a.DiskonTotal,
                    a.IsDiskonBerlaku,
                    a.DiskonDari,
                    a.DiskonSampai,

                    a.CreateDateTime
                }
            ).ToListAsync(ct);

            var childLookup = childRows
                .GroupBy(x => x.PemeriksaanLabId)
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(x => x.CreateDateTime) // opsional
                    .Select(x => (object)x)
                    .ToList());

            // =====================================================
            // 4) Build output: 1 parent + list asuransi cover
            // =====================================================
            var result = parents.Select(p => new
            {
                p.PemeriksaanLabId,
                p.NamaPemeriksaan,
                p.LabId,
                p.NamaLab,
                p.LastCreateDateTime,

                AsuransiCover = childLookup.TryGetValue((Guid)p.PemeriksaanLabId, out var list)
                    ? list
                    : new List<object>()
            }).ToList();

            return Ok(new
            {
                status = "success",
                message = "Data berhasil diambil.",
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
