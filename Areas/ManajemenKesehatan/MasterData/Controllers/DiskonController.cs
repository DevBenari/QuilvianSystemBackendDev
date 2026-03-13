using System.Linq;
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
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DiskonController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DiskonController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DiskonController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DiskonController> logger,
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
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                var baseQuery =
                    from a in _applicationDbContext.Diskons.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.IsDelete == false || a.IsDelete == null
                    select new
                    {
                        a.DiskonId,
                        a.NamaDiskon,
                        a.TglBerlaku,
                        a.TglBerakhir,
                        a.IsAsuransi,
                        a.AsuransiId,
                        a.PersenDiskon,
                        a.NominalDiskon,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    };

                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var headers = await baseQuery
                    .OrderByDescending(a => a.CreateDateTime)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (!headers.Any())
                {
                    return NotFound(new
                    {
                        message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found"
                    });
                }

                var diskonIds = headers.Select(x => x.DiskonId).ToList();

                var details = await _applicationDbContext.DiskonDetails
                    .AsNoTracking()
                    .Where(d => diskonIds.Contains((Guid)d.DiskonId) && (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    })
                    .OrderByDescending(d => d.CreateDateTime)
                    .ToListAsync();

                var result = headers.Select(h => new
                {
                    h.DiskonId,
                    h.NamaDiskon,
                    h.TglBerlaku,
                    h.TglBerakhir,
                    h.IsAsuransi,
                    h.AsuransiId,
                    h.PersenDiskon,
                    h.NominalDiskon,
                    h.Keterangan,
                    h.CreateBy,
                    h.CreateByName,
                    h.CreateDateTime,
                    h.UpdateBy,
                    h.UpdateDateTime,
                    Details = details
                        .Where(d => d.DiskonId == h.DiskonId)
                        .ToList()
                }).ToList();

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var header = await (
                    from a in _applicationDbContext.Diskons.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.DiskonId == id && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.DiskonId,
                        a.NamaDiskon,
                        a.TglBerlaku,
                        a.TglBerakhir,
                        a.IsAsuransi,
                        a.AsuransiId,
                        a.PersenDiskon,
                        a.NominalDiskon,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    }
                ).FirstOrDefaultAsync();

                if (header == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                var details = await (
                    from d in _applicationDbContext.DiskonDetails.AsNoTracking()
                    where d.DiskonId == id && (d.IsDelete == false || d.IsDelete == null)
                    select new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime
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
        public async Task<IActionResult> Create([FromBody] DiskonViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                if (string.IsNullOrWhiteSpace(vm.NamaDiskon))
                {
                    return BadRequest(new { message = "Nama diskon wajib diisi." });
                }

                if (vm.TglBerlaku.HasValue && vm.TglBerakhir.HasValue && vm.TglBerakhir < vm.TglBerlaku)
                {
                    return BadRequest(new { message = "Tanggal berakhir tidak boleh lebih kecil dari tanggal berlaku." });
                }

                bool isDuplicate = await _applicationDbContext.Diskons
                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                                && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama diskon ini telah tersedia." });
                }

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                var diskonId = Guid.NewGuid();

                var data = new Diskon
                {
                    DiskonId = diskonId,
                    NamaDiskon = vm.NamaDiskon,
                    TglBerlaku = vm.TglBerlaku,
                    TglBerakhir = vm.TglBerakhir,
                    IsAsuransi = vm.IsAsuransi,
                    AsuransiId = vm.AsuransiId,
                    PersenDiskon = vm.PersenDiskon,
                    NominalDiskon = vm.NominalDiskon,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.Diskons.Add(data);

                if (vm.Details != null && vm.Details.Any())
                {
                    var detailEntities = vm.Details.Select(d => new DiskonDetail
                    {
                        DetailDiskonId = Guid.NewGuid(),
                        DiskonId = diskonId,
                        LayananId = d.LayananId,
                        KodeLayanan = d.KodeLayanan,
                        KategoriLayanan = d.KategoriLayanan,
                        MaxQty = d.MaxQty,
                        MaxHarga = d.MaxHarga,
                        Keterangan = d.Keterangan,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    }).ToList();

                    _applicationDbContext.DiskonDetails.AddRange(detailEntities);
                }

                var result = await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        diskonId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DiskonViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ambil email user login dari claim
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // ambil user active
                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // validasi nama diskon
                if (string.IsNullOrWhiteSpace(vm.NamaDiskon))
                {
                    return BadRequest(new { message = "Nama diskon wajib diisi." });
                }

                // validasi tanggal
                if (vm.TglBerlaku.HasValue && vm.TglBerakhir.HasValue && vm.TglBerakhir < vm.TglBerlaku)
                {
                    return BadRequest(new { message = "Tanggal berakhir tidak boleh lebih kecil dari tanggal berlaku." });
                }

                // cek data diskon
                var existingDiskon = await _applicationDbContext.Diskons
                    .FirstOrDefaultAsync(x => x.DiskonId == id && x.IsDelete == false);

                if (existingDiskon == null)
                {
                    return NotFound(new { message = "Data diskon tidak ditemukan." });
                }

                // cek duplikasi nama diskon selain id yang sedang diedit
                bool isDuplicate = await _applicationDbContext.Diskons
                    .AnyAsync(c =>
                        c.DiskonId != id &&
                        c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim() &&
                        c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama diskon ini telah tersedia." });
                }

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                // update header diskon
                existingDiskon.NamaDiskon = vm.NamaDiskon;
                existingDiskon.TglBerlaku = vm.TglBerlaku;
                existingDiskon.TglBerakhir = vm.TglBerakhir;
                existingDiskon.IsAsuransi = vm.IsAsuransi;
                existingDiskon.AsuransiId = vm.AsuransiId;
                existingDiskon.PersenDiskon = vm.PersenDiskon;
                existingDiskon.NominalDiskon = vm.NominalDiskon;
                existingDiskon.Keterangan = vm.Keterangan;
                existingDiskon.UpdateBy = userActiveId;
                existingDiskon.UpdateDateTime = DateTimeOffset.UtcNow;

                // tambah detail baru tanpa menghapus detail lama
                if (vm.Details != null && vm.Details.Any())
                {
                    var existingDetails = await _applicationDbContext.DiskonDetails
                        .Where(x => x.DiskonId == id && x.IsDelete == false)
                        .ToListAsync();

                    var newDetails = new List<DiskonDetail>();

                    foreach (var d in vm.Details)
                    {
                        bool detailSudahAda = existingDetails.Any(x =>
                            x.LayananId == d.LayananId &&
                            x.KodeLayanan == d.KodeLayanan &&
                            x.KategoriLayanan == d.KategoriLayanan &&
                            x.MaxQty == d.MaxQty &&
                            x.MaxHarga == d.MaxHarga &&
                            x.IsDelete == false);

                        if (!detailSudahAda)
                        {
                            newDetails.Add(new DiskonDetail
                            {
                                DetailDiskonId = Guid.NewGuid(),
                                DiskonId = id,
                                LayananId = d.LayananId,
                                KodeLayanan = d.KodeLayanan,
                                KategoriLayanan = d.KategoriLayanan,
                                MaxQty = d.MaxQty,
                                MaxHarga = d.MaxHarga,
                                Keterangan = d.Keterangan,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                                IsDelete = false
                            });
                        }
                    }

                    if (newDetails.Any())
                    {
                        await _applicationDbContext.DiskonDetails.AddRangeAsync(newDetails);
                    }
                }

                var result = await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Ubah Data Berhasil || 200 OK",
                        diskonId = id
                    });
                }

                return Ok(new
                {
                    message = "Tidak ada perubahan data.",
                    diskonId = id
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                var data = await _applicationDbContext.Diskons
                    .FirstOrDefaultAsync(x => x.DiskonId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = "Data diskon tidak ditemukan." });
                }

                var details = await _applicationDbContext.DiskonDetails
                    .Where(x => x.DiskonId == id && x.IsDelete == false)
                    .ToListAsync();

                data.IsDelete = true;
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                if (details.Any())
                {
                    foreach (var item in details)
                    {
                        item.IsDelete = true;
                        item.DeleteBy = userActiveId;
                        item.DeleteDateTime = DateTimeOffset.UtcNow;
                    }

                    _applicationDbContext.DiskonDetails.UpdateRange(details);
                }

                _applicationDbContext.Diskons.Update(data);

                var result = await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data header dan detail berhasil dihapus (soft delete) || 200 OK",
                        diskonId = id
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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

                var query = from a in _applicationDbContext.Diskons.AsNoTracking()
                            join u in _applicationDbContext.UserActives.AsNoTracking()
                                on a.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            where a.IsDelete == false || a.IsDelete == null
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u != null ? u.FullName : null,
                                a.DiskonId,
                                a.NamaDiskon,
                                a.TglBerlaku,
                                a.TglBerakhir,
                                a.IsAsuransi,
                                a.AsuransiId,
                                a.PersenDiskon,
                                a.NominalDiskon,
                                a.Keterangan,
                                a.UpdateBy,
                                a.UpdateDateTime
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim()}%";
                    query = query.Where(u =>
                        EF.Functions.ILike(u.NamaDiskon!, search) ||
                        EF.Functions.ILike(u.CreateByName!, search) ||
                        EF.Functions.ILike(u.Keterangan!, search));
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    query = query.Where(u =>
                        u.CreateDateTime >= startUtc &&
                        u.CreateDateTime <= endUtc);
                }
                else if (startDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    query = query.Where(u => u.CreateDateTime >= startUtc);
                }
                else if (endDate.HasValue)
                {
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(u => u.CreateDateTime <= endUtc);
                }

                if (periode.HasValue)
                {
                    var today = DateTime.UtcNow.Date;

                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            query = query.Where(u => u.CreateDateTime.Date == today);
                            break;

                        case PeriodeFilter.ThisWeek:
                            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                            query = query.Where(u =>
                                u.CreateDateTime.Date >= startOfWeek &&
                                u.CreateDateTime.Date <= today);
                            break;

                        case PeriodeFilter.LastWeek:
                            var startLastWeek = today.AddDays(-7 - (int)today.DayOfWeek);
                            var endLastWeek = today.AddDays(-(int)today.DayOfWeek);
                            query = query.Where(u =>
                                u.CreateDateTime.Date >= startLastWeek &&
                                u.CreateDateTime.Date < endLastWeek);
                            break;

                        case PeriodeFilter.ThisMonth:
                            query = query.Where(u =>
                                u.CreateDateTime.Month == today.Month &&
                                u.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastMonth:
                            var lastMonthDate = today.AddMonths(-1);
                            query = query.Where(u =>
                                u.CreateDateTime.Month == lastMonthDate.Month &&
                                u.CreateDateTime.Year == lastMonthDate.Year);
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
                        "NamaDiskon" => query.OrderBy(u => u.NamaDiskon),
                        "TglBerlaku" => query.OrderBy(u => u.TglBerlaku),
                        "TglBerakhir" => query.OrderBy(u => u.TglBerakhir),
                        _ => query.OrderBy(u => u.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                        "NamaDiskon" => query.OrderByDescending(u => u.NamaDiskon),
                        "TglBerlaku" => query.OrderByDescending(u => u.TglBerlaku),
                        "TglBerakhir" => query.OrderByDescending(u => u.TglBerakhir),
                        _ => query.OrderByDescending(u => u.CreateDateTime)
                    };

                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var headers = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (headers.Count == 0 && page > totalPages && totalRows > 0)
                {
                    return NotFound(new { message = "Page not found." });
                }

                var diskonIds = headers.Select(x => x.DiskonId).ToList();

                var details = await _applicationDbContext.DiskonDetails
                    .AsNoTracking()
                    .Where(d => diskonIds.Contains((Guid)d.DiskonId) && (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    })
                    .OrderByDescending(d => d.CreateDateTime)
                    .ToListAsync();

                var rows = headers.Select(h => new
                {
                    h.CreateDateTime,
                    h.CreateBy,
                    h.CreateByName,
                    h.DiskonId,
                    h.NamaDiskon,
                    h.TglBerlaku,
                    h.TglBerakhir,
                    h.IsAsuransi,
                    h.AsuransiId,
                    h.PersenDiskon,
                    h.NominalDiskon,
                    h.Keterangan,
                    h.UpdateBy,
                    h.UpdateDateTime,
                    Details = details
                        .Where(d => d.DiskonId == h.DiskonId)
                        .ToList()
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