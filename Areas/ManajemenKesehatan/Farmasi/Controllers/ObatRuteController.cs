using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ObatRuteController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ObatRuteController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatRuteController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatRuteController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10, CancellationToken ct = default)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================================================
            // QUERY HEADER OBAT RUTE
            // =========================================================
            var query =
                from a in _applicationDbContext.ObatRutes.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into uG
                from u in uG.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.RuteObatId,
                    a.RuteObat,
                    a.Keterangan
                };

            query = query.OrderByDescending(a => a.CreateDateTime);

            // =========================================================
            // HITUNG TOTAL DATA
            // =========================================================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return NotFound(new
                {
                    message = "Belum ada data. || 404 Not Found"
                });
            }

            if (page > totalPages)
            {
                return NotFound(new
                {
                    message = "Halaman tidak ditemukan. || 404 Not Found"
                });
            }

            // =========================================================
            // AMBIL HEADER SESUAI PAGING
            // =========================================================
            var headers = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            var ruteObatIds = headers
                .Select(x => x.RuteObatId)
                .ToList();

            // =========================================================
            // AMBIL DETAIL BERDASARKAN HEADER YANG TAMPIL
            // Ini bukan N+1, karena detail diambil sekali saja.
            // =========================================================
            var details = await _applicationDbContext.ObatRuteDetails
                .AsNoTracking()
                .Where(d =>
                    d.RuteObatId != null &&
                    ruteObatIds.Contains(d.RuteObatId.Value) &&
                    (d.IsDelete == false || d.IsDelete == null)
                )
                .Select(d => new
                {
                    RuteObatId = d.RuteObatId!.Value,
                    d.DetailRuteObatId,
                    d.NamaSingkat,
                    d.Kepanjangan,
                    d.Keterangan,
                    d.CreateDateTime,
                    d.CreateBy
                })
                .ToListAsync(ct);

            // =========================================================
            // GABUNGKAN HEADER + DETAIL
            // =========================================================
            var listdata = headers
                .GroupJoin(
                    details,
                    header => header.RuteObatId,
                    detail => detail.RuteObatId,
                    (header, detailGroup) => new
                    {
                        header.CreateDateTime,
                        header.CreateBy,
                        header.CreateByName,

                        header.RuteObatId,
                        header.RuteObat,
                        header.Keterangan,

                        ObatRuteDetails = detailGroup
                            .Select(d => new
                            {
                                d.DetailRuteObatId,
                                d.RuteObatId,
                                d.NamaSingkat,
                                d.Kepanjangan,
                                d.Keterangan,
                                d.CreateDateTime,
                                d.CreateBy
                            })
                            .ToList()
                    }
                )
                .ToList();

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
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var data = await (
                from a in _applicationDbContext.ObatRutes.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into uG
                from u in uG.DefaultIfEmpty()

                where a.RuteObatId == id
                      && (a.IsDelete == false || a.IsDelete == null)

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.RuteObatId,
                    a.RuteObat,
                    a.Keterangan,

                    ObatRuteDetails = a.ObatRuteDetails
                        .Where(d => d.IsDelete == false || d.IsDelete == null)
                        .Select(d => new
                        {
                            d.DetailRuteObatId,
                            d.RuteObatId,
                            d.NamaSingkat,
                            d.Kepanjangan,
                            d.Keterangan,
                            d.CreateDateTime,
                            d.CreateBy
                        })
                        .ToList()
                }
            ).FirstOrDefaultAsync(ct);

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Data tidak ditemukan."
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Ditemukan || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObatRuteViewModel vm)
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

                //// **Cek Duplikasi**
                bool isDuplicate = await _applicationDbContext.ObatRutes
                                    .AnyAsync(c => c.RuteObat.ToLower().Trim() == vm.RuteObat.ToLower().Trim() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama rute obat ini telah tersedia" });
                }

                // **Buat Data Baru**
                var data = new ObatRute
                {
                    RuteObatId = Guid.NewGuid(),
                    RuteObat = vm.RuteObat,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.ObatRutes.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] ObatRuteViewModel vm)
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
                var data = await _applicationDbContext.ObatRutes.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //// **Cek Duplikasi**
                bool isDuplicate = await _applicationDbContext.ObatRutes
                                    .AnyAsync(c => c.RuteObat.ToLower().Trim() == vm.RuteObat.ToLower().Trim() 
                                    && c.IsDelete == false && c.RuteObatId!= id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama rute obat ini telah tersedia" });
                }

                // **Update Data**
                data.RuteObat = vm.RuteObat;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ObatRutes.Update(data);
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
            var data = await _applicationDbContext.ObatRutes
                .Include(x => x.ObatRuteDetails)
                .FirstOrDefaultAsync(x => x.RuteObatId == id);

            if (data == null || data.IsDelete == true)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Data obat rute tidak ditemukan"
                });
            }

            data.IsDelete = true;
            data.DeleteDateTime = DateTime.UtcNow;

            foreach (var detail in data.ObatRuteDetails)
            {
                detail.IsDelete = true;
                detail.DeleteDateTime = DateTime.UtcNow;
            }

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "Data obat rute berhasil dihapus"
            });
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
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================================================
            // QUERY HEADER OBAT RUTE
            // =========================================================
            var query =
                from a in _applicationDbContext.ObatRutes.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into uG
                from u in uG.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.RuteObatId,
                    a.RuteObat,
                    a.Keterangan
                };

            // =========================================================
            // FILTER SEARCH
            // =========================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.RuteObat ?? "", keyword) ||
                    EF.Functions.ILike(x.Keterangan ?? "", keyword)
                );
            }

            // =========================================================
            // FILTER TANGGAL
            // =========================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc
                );
            }

            // =========================================================
            // FILTER PERIODE
            // =========================================================
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today
                        );
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year
                        );
                        break;

                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime.Month == lastMonth.Month &&
                            x.CreateDateTime.Year == lastMonth.Year
                        );
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // =========================================================
            // SORTING
            // =========================================================
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "RuteObat" => query.OrderByDescending(x => x.RuteObat),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "RuteObat" => query.OrderBy(x => x.RuteObat),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // =========================================================
            // COUNT
            // =========================================================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
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

            if (page > totalPages)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Page not found."
                });
            }

            // =========================================================
            // AMBIL HEADER SESUAI PAGE
            // =========================================================
            var headers = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            var ruteObatIds = headers
                .Select(x => x.RuteObatId)
                .ToList();

            // =========================================================
            // AMBIL DETAIL BERDASARKAN HEADER YANG MUNCUL
            // Ini bukan N+1, karena detail diambil sekali saja.
            // =========================================================
            var details = await _applicationDbContext.ObatRuteDetails
                .AsNoTracking()
                .Where(d =>
                    d.RuteObatId != null &&
                    ruteObatIds.Contains(d.RuteObatId.Value) &&
                    (d.IsDelete == false || d.IsDelete == null)
                )
                .Select(d => new
                {
                    d.DetailRuteObatId,
                    d.RuteObatId,
                    d.NamaSingkat,
                    d.Kepanjangan,
                    d.Keterangan,
                    d.CreateDateTime,
                    d.CreateBy
                })
                .ToListAsync(ct);

            var detailLookup = details
                .GroupBy(d => d.RuteObatId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =========================================================
            // GABUNGKAN HEADER + DETAIL
            // =========================================================
            var rows = headers.Select(h => new
            {
                h.CreateDateTime,
                h.CreateBy,
                h.CreateByName,

                h.RuteObatId,
                h.RuteObat,
                h.Keterangan,

                ObatRuteDetails = detailLookup.TryGetValue(h.RuteObatId, out var itemDetails)
                    ? itemDetails
                    : details.Where(x => false).ToList()
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

    }
}
