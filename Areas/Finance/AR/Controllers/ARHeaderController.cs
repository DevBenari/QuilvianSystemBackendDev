using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AR.Models;
using QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ARHeaderController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ARHeaderController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ARHeaderController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ARHeaderController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        // =========================================================
        // GET ALL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                var data =
                    await (
                        from ar in _applicationDbContext.ARHeaders.AsNoTracking()

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ar.CreateBy equals u.UserActiveId

                        join a in _applicationDbContext.Asuransis.AsNoTracking()
                            on ar.AsuransiId equals a.AsuransiId

                        where ar.IsDelete == false

                        orderby ar.CreateDateTime descending

                        select new
                        {
                            ar.ARHeaderId,
                            ar.AsuransiId,

                            AsuransiName = a.NamaAsuransi, // ✅ tambahan

                            ar.Tipe_Kunjungan,              // ✅ sudah ada di ARHeader
                            ar.JenisAR,

                            ar.NoInvoice,

                            ar.TglPembuatanInvoice,
                            ar.TglKirim,
                            ar.TglTerima,
                            ar.TglTagihan,
                            ar.TglJatuhTempo,

                            ar.DueDate,
                            ar.TotalInvoice,

                            ar.IsDocumentComplited,

                            ar.Keterangan,

                            ar.CreateDateTime,
                            ar.CreateBy,

                            CreateByName = u.FullName
                        }
                    ).ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal : {ex.Message}"
                });
            }
        }

        // =========================================================
        // GET ALL PAGED
        // =========================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedARHeader(
            int page = 1,
            int perPage = 10,

            string? asuransi = null,
            string? noInvoice = null,
            string? tipeKunjungan = null,
            string? isCanceled = null,

            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null
        )
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query =
                    from ar in _applicationDbContext.ARHeaders.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ar.CreateBy equals u.UserActiveId

                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ar.AsuransiId equals a.AsuransiId

                    where ar.IsDelete == false

                    select new
                    {
                        ar.ARHeaderId,
                        ar.AsuransiId,
                        AsuransiName = a.NamaAsuransi,

                        ar.Tipe_Kunjungan,
                        ar.JenisAR,

                        ar.NoInvoice,

                        ar.TglPembuatanInvoice,
                        ar.TglKirim,
                        ar.TglTerima,
                        ar.TglTagihan,
                        ar.TglJatuhTempo,

                        ar.DueDate,
                        ar.TotalInvoice,

                        ar.IsDocumentComplited,
                        ar.IsCanceled,
                        ar.Keterangan,

                        ar.CreateDateTime,
                        ar.CreateBy,

                        CreateByName = u.FullName
                    };

                // FILTER ASURANSI SENDIRI
                if (!string.IsNullOrWhiteSpace(asuransi))
                {
                    var keywordAsuransi = $"%{asuransi.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.AsuransiName, keywordAsuransi)
                    );
                }

                // FILTER NO INVOICE SENDIRI
                if (!string.IsNullOrWhiteSpace(noInvoice))
                {
                    var keywordInvoice = $"%{noInvoice.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoInvoice, keywordInvoice)
                    );
                }

                // FILTER TIPE KUNJUNGAN SENDIRI
                if (!string.IsNullOrWhiteSpace(tipeKunjungan))
                {
                    var keywordTipeKunjungan = $"%{tipeKunjungan.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.Tipe_Kunjungan, keywordTipeKunjungan)
                    );
                }

                // FILTER TIPE KUNJUNGAN SENDIRI
                if (!string.IsNullOrWhiteSpace(isCanceled))
                {
                    bool parsedIsCanceled = bool.Parse(isCanceled);

                    query = query.Where(x => x.IsCanceled == parsedIsCanceled);
                }

                // SEARCH GLOBAL
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoInvoice, keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||
                        EF.Functions.ILike(x.Tipe_Kunjungan, keyword) ||
                        EF.Functions.ILike(x.AsuransiName, keyword) ||
                        x.IsCanceled.ToString().ToLower().Contains(search.Trim().ToLower())
                    );
                }

                // FILTER DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                // SORTING
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "asuransi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.AsuransiName)
                            : query.OrderBy(x => x.AsuransiName),

                    "tipekunjungan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.Tipe_Kunjungan)
                            : query.OrderBy(x => x.Tipe_Kunjungan),

                    "totalinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoice)
                            : query.OrderBy(x => x.TotalInvoice),

                    "createbyname" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateByName)
                            : query.OrderBy(x => x.CreateByName),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderByDescending(x => x.CreateDateTime)
                };

                // PAGINATION
                int totalRows = await query.CountAsync();

                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",

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
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal : {ex.Message}"
                });
            }
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ARHeaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    status = "success",
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // CREATE
        // =========================================================
        private string GetRomanMonth(int month)
        {
            return month switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                11 => "XI",
                12 => "XII",
                _ => ""
            };
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ARHeaderViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi." });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });
                }

                // =========================================================
                // 🔥 GENERATE NO INVOICE (AMAN & STABLE)
                // =========================================================

                var year = DateTime.UtcNow.Year;
                var month = DateTime.UtcNow.Month;

                string romanMonth = GetRomanMonth(month);

                // 🔥 mapping tipe kunjungan
                string tipeKunjungan = vm.Tipe_Kunjungan switch
                {
                    "Rawat Jalan" => "OP",
                    "Rawat Inap" => "IP",
                    "IGD" => "IGD",
                    _ => vm.Tipe_Kunjungan
                };

                string prefix = "RSMMC";

                var lastInvoice = await _applicationDbContext.ARHeaders
                    .Where(x =>
                        x.CreateDateTime.Year == year &&
                        x.IsDelete == false)
                    .OrderByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastInvoice != null &&
                    !string.IsNullOrWhiteSpace(lastInvoice.NoInvoice))
                {
                    // ambil angka di depan invoice (0001)
                    var match = Regex.Match(lastInvoice.NoInvoice, @"^\d+");

                    if (match.Success)
                    {
                        nextNumber = int.Parse(match.Value) + 1;
                    }
                }

                string noInvoice =
                    $"{nextNumber:0000}/{tipeKunjungan}/{prefix}/{romanMonth}/{year}";

                // =========================================================
                // CREATE ENTITY
                // =========================================================

                var data = new ARHeader
                {
                    ARHeaderId = Guid.NewGuid(),

                    AsuransiId = vm.AsuransiId,
                    JenisAR = vm.JenisAR,
                    Tipe_Kunjungan = vm.Tipe_Kunjungan,

                    NoInvoice = noInvoice,

                    TglPembuatanInvoice = vm.TglPembuatanInvoice,
                    DueDate = vm.DueDate,
                    TotalInvoice = vm.TotalInvoice,

                    TglKirim = vm.TglKirim,
                    TglTerima = vm.TglTerima,
                    TglTagihan = vm.TglTagihan,
                    TglJatuhTempo = vm.TglJatuhTempo,

                    IsDocumentComplited = vm.IsDocumentComplited,

                    IsCanceled = vm.IsCanceled,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,
                    IsDelete = false
                };

                _applicationDbContext.ARHeaders.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        noInvoice = noInvoice
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal menyimpan data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =========================================================
        // UPDATE
        // =========================================================
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelARHeader(Guid id)
        {
            try
            {
                var header = await _applicationDbContext.ARHeaders
                    .FirstOrDefaultAsync(x => x.ARHeaderId == id);

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "AR Header tidak ditemukan."
                    });
                }

                // optional safety: kalau sudah canceled
                if (header.IsCanceled)
                {
                    return BadRequest(new
                    {
                        message = "AR Header sudah dibatalkan."
                    });
                }

                header.IsCanceled = true;

                _applicationDbContext.ARHeaders.Update(header);

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "AR Header berhasil dibatalkan."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
            public async Task<IActionResult> Update(
                Guid id,
                [FromBody] ARHeaderViewModel vm)
            {
            try
            {
                var data =
                    await _applicationDbContext.ARHeaders
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive =
                    await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                bool isDuplicate =
                    await _applicationDbContext.ARHeaders
                    .AnyAsync(x =>
                        x.NoInvoice.ToLower() ==
                        vm.NoInvoice.ToLower()
                        &&
                        x.ARHeaderId != id
                        &&
                        x.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new
                    {
                        message = "No Invoice sudah digunakan."
                    });
                }

                data.AsuransiId = vm.AsuransiId;

                data.NoInvoice = vm.NoInvoice.Trim();

                data.TglPembuatanInvoice =
                    vm.TglPembuatanInvoice;

                data.DueDate = vm.DueDate;

                data.TotalInvoice = vm.TotalInvoice;

                data.TglKirim = vm.TglKirim;
                data.TglTerima = vm.TglTerima;
                data.TglTagihan = vm.TglTagihan;
                data.TglJatuhTempo = vm.TglJatuhTempo;

                data.IsDocumentComplited =
                    vm.IsDocumentComplited;

                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = getUserActive.UserActiveId;

                _applicationDbContext.ARHeaders.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update data berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal update data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARHeaders
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive =
                    await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.IsDelete = true;

                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = getUserActive.UserActiveId;

                _applicationDbContext.ARHeaders.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Delete berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal delete data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}