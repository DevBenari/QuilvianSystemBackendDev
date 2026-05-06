using System.Security.Claims;
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
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TarifKelasController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TarifKelasController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TarifKelasController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TarifKelasController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet]
        public async Task<IActionResult> GetAlLTarifTindakan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.TarifKelass
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            a.TindakanId,
                            a.KelasId,
                            a.TarifDokter,
                            a.TarifRs,
                            a.TarifJp,
                            a.TarifBahp,
                            a.TarifLain,
                            a.TarifTotal,
                            a.KSO,
                            //a.PemeriksaanLabId,
                            //a.PeralatanId,
                            //a.DokterId,
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
            var listdata = _applicationDbContext.TarifKelass
                .Where(t => t.TindakanId == id && !t.IsDelete)
                .ToListAsync();
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
        public async Task<IActionResult> CreateTarifTindakan([FromBody] TarifKelasViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
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

                // **Ambil Tanggal Sekarang**
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd"); // Format: YYMMDD

                bool isDuplicate = _applicationDbContext.TarifKelass
                    .Any(t => t.TindakanId == vm.TindakanId &&
                              t.KelasId == vm.KelasId
                              && t.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Tarif dengan kombinasi Poliklinik, Tindakan, dan Kelas ini sudah ada!" });
                }

                // **Hitung Tarif Total**
                decimal total = (vm.TarifDokter ?? 0) +
                       (vm.TarifRs ?? 0) +
                       (vm.TarifJp ?? 0) +
                       (vm.TarifBahp ?? 0) +
                       (vm.TarifLain ?? 0);

                // **Buat Data Baru**
                var data = new TarifKelas
                {
                    TarifKelasId = Guid.NewGuid(),
                    TindakanId = vm.TindakanId,
                    KelasId = vm.KelasId,
                    TarifDokter = vm.TarifDokter,
                    TarifRs = vm.TarifRs,
                    TarifJp = vm.TarifJp,
                    TarifBahp = vm.TarifBahp,
                    TarifLain = vm.TarifLain,
                    TarifTotal = total,
                    KodeLayanan = vm.KodeLayanan,
                    KategoriTindakan = vm.KategoriTindakan,
                    KSO = vm.KSO,
                    //PemeriksaanLabId = vm.PemeriksaanLabId,
                    //PeralatanId = vm.PeralatanId,
                    //DokterId = vm.DokterId,

                    // **User Activity**
                    CreateBy = userActiveId,
                    CreateDateTime = dateNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.TarifKelass.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
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
        public async Task<IActionResult> UpdateTarifTindakan(Guid id, [FromBody] TarifKelasViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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
                var data = await _applicationDbContext.TarifKelass.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.TindakanId = vm.TindakanId;
                data.KelasId = vm.KelasId;
                //data.PemeriksaanLabId = vm.PemeriksaanLabId;
                //data.PeralatanId = vm.PeralatanId;
                data.KodeLayanan = vm.KodeLayanan;
                data.KategoriTindakan = vm.KategoriTindakan;
                data.TarifDokter = vm.TarifDokter;
                data.TarifRs = vm.TarifRs;
                data.TarifJp = vm.TarifJp;
                data.TarifBahp = vm.TarifBahp;
                data.TarifLain = vm.TarifLain;
                data.TarifTotal = (vm.TarifDokter ?? 0) +
                    (vm.TarifRs ?? 0) +
                    (vm.TarifJp ?? 0) +
                    (vm.TarifBahp ?? 0) +
                    (vm.TarifLain ?? 0);
                data.KSO = vm.KSO;
                //data.DokterId = vm.DokterId;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                bool isDuplicate = _applicationDbContext.TarifKelass
                .Any(t => t.TindakanId == vm.TindakanId &&
                          t.KelasId == vm.KelasId
                          && t.IsDelete == false && t.TarifKelasId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Tarif dengan kombinasi Poliklinik, Tindakan, dan Kelas ini sudah ada!" });
                }

                _applicationDbContext.TarifKelass.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
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
                var data = await _applicationDbContext.TarifKelass.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TarifKelass.Update(data);
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
        public async Task<IActionResult> PagedTarifTindakan(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,

            // ✅ filter spesifik
            string? namaTindakan = null,
            string? namaKelas = null,
            string? namaDokter = null,
            string? kategori = null,

            CancellationToken cancellationToken = default
        )
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100; // biar gak membebani server

            // Base query (projection ringan + AsNoTracking)
            var q =
                from a in _applicationDbContext.TarifKelass.AsNoTracking()
                join t in _applicationDbContext.Tindakans.AsNoTracking()
                    on a.TindakanId equals t.TindakanId
                join kls in _applicationDbContext.Kelass.AsNoTracking()
                    on a.KelasId equals kls.KelasId

                // Dokter bisa nullable => LEFT JOIN
                //join d in _applicationDbContext.Dokters.AsNoTracking()
                //    on a.DokterId equals d.DokterId into dokterGroup
                //from d in dokterGroup.DefaultIfEmpty()

                    // CreateBy bisa nullable => LEFT JOIN
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()

                where a.IsDelete == false

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.TarifKelasId,
                    a.TindakanId,
                    a.KelasId,
                    //a.DokterId,

                    NamaTindakan = t.NamaTindakan,
                    NamaKelas = kls.NamaKelas,
                    a.KategoriTindakan,
                    a.KodeLayanan,
                    //NamaDokter = d != null ? d.NmDokter : null,

                    a.TarifDokter,
                    a.TarifRs,
                    a.TarifJp,
                    a.TarifBahp,
                    a.TarifLain,
                    a.TarifTotal,
                    a.KSO,
                };

            // =========================
            // FILTER: search global (nama tindakan/kelas/dokter)
            // (1 huruf: prefix search biar lebih ringan daripada %...%)
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                var pattern = (search.Length <= 1)
                    ? $"{search}%"
                    : $"%{search}%";

                q = q.Where(x =>
                    (x.NamaTindakan != null && EF.Functions.ILike(x.NamaTindakan, pattern)) ||
                    (x.NamaKelas != null && EF.Functions.ILike(x.NamaKelas, pattern)) 
                );
            }

            // =========================
            // FILTER: spesifik per kolom
            // =========================
            if (!string.IsNullOrWhiteSpace(namaTindakan))
            {
                var p = $"%{namaTindakan.Trim().ToLower()}%";
                q = q.Where(x => x.NamaTindakan != null && EF.Functions.ILike(x.NamaTindakan, p));
            }

            if (!string.IsNullOrWhiteSpace(namaKelas))
            {
                var p = $"%{namaKelas.Trim().ToLower()}%";
                q = q.Where(x => x.NamaKelas != null && EF.Functions.ILike(x.NamaKelas, p));
            }

            if (!string.IsNullOrWhiteSpace(kategori))
            {
                var p = $"%{kategori.Trim().ToLower()}%";
                q = q.Where(x => x.KategoriTindakan != null && EF.Functions.ILike(x.KategoriTindakan, p));
            }

            // =========================
            // FILTER: tanggal range (pakai boundary, jangan .Date)
            // =========================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);

                var startUtc = new DateTimeOffset(start);
                var endUtcExclusive = new DateTimeOffset(endExclusive);

                q = q.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtcExclusive);
            }

            // =========================
            // FILTER: periode (pakai boundary juga)
            // =========================
            if (periode.HasValue)
            {
                var todayUtc = DateTime.UtcNow.Date;
                DateTimeOffset start, endExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = new DateTimeOffset(todayUtc, TimeSpan.Zero);
                        endExclusive = new DateTimeOffset(todayUtc.AddDays(1), TimeSpan.Zero);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        var diff = (int)todayUtc.DayOfWeek; // Sunday=0
                        var weekStart = todayUtc.AddDays(-diff);
                        start = new DateTimeOffset(weekStart, TimeSpan.Zero);
                        endExclusive = new DateTimeOffset(todayUtc.AddDays(1), TimeSpan.Zero);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var diff2 = (int)todayUtc.DayOfWeek;
                        var thisWeekStart = todayUtc.AddDays(-diff2);
                        var lastWeekStart = thisWeekStart.AddDays(-7);
                        start = new DateTimeOffset(lastWeekStart, TimeSpan.Zero);
                        endExclusive = new DateTimeOffset(thisWeekStart, TimeSpan.Zero);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        var monthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        start = new DateTimeOffset(monthStart);
                        endExclusive = new DateTimeOffset(monthStart.AddMonths(1));
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        start = new DateTimeOffset(lastMonthStart);
                        endExclusive = new DateTimeOffset(thisMonthStart);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        var yearStart = new DateTime(todayUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        start = new DateTimeOffset(yearStart);
                        endExclusive = new DateTimeOffset(yearStart.AddYears(1));
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart2 = new DateTime(todayUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastYearStart = thisYearStart2.AddYears(-1);
                        start = new DateTimeOffset(lastYearStart);
                        endExclusive = new DateTimeOffset(thisYearStart2);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        start = new DateTimeOffset(todayUtc.AddMonths(-3), TimeSpan.Zero);
                        endExclusive = new DateTimeOffset(todayUtc.AddDays(1), TimeSpan.Zero);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = new DateTimeOffset(todayUtc.AddMonths(-6), TimeSpan.Zero);
                        endExclusive = new DateTimeOffset(todayUtc.AddDays(1), TimeSpan.Zero);
                        q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;
                }
            }

            // =========================
            // SORTING aman
            // =========================
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (orderBy ?? "CreateDateTime") switch
            {
                "CreateByName" => desc ? q.OrderByDescending(x => x.CreateByName) : q.OrderBy(x => x.CreateByName),
                "NamaTindakan" => desc ? q.OrderByDescending(x => x.NamaTindakan) : q.OrderBy(x => x.NamaTindakan),
                "NamaKelas" => desc ? q.OrderByDescending(x => x.NamaKelas) : q.OrderBy(x => x.NamaKelas),
                //"NamaDokter" => desc ? q.OrderByDescending(x => x.NamaDokter) : q.OrderBy(x => x.NamaDokter),
                _ => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // COUNT + PAGE (async)
            // =========================
            var totalRows = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = Array.Empty<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            var rows = await q
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(cancellationToken);

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
