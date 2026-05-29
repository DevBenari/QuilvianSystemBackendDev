using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models;
using QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AyatSilangController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AyatSilangController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AyatSilangController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AyatSilangController> logger,
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
                var data =
                    await (
                        from ayat in _applicationDbContext.AyatSilangs.AsNoTracking()

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                            on ayat.CreateBy equals u.UserActiveId

                        // LEFT JOIN Asuransi
                        join a in _applicationDbContext.Asuransis.AsNoTracking()
                            on ayat.AsuransiId equals a.AsuransiId into asuransiJoin
                        from a in asuransiJoin.DefaultIfEmpty()

                            // LEFT JOIN MasterBanks
                        join b in _applicationDbContext.BankAccounts.AsNoTracking()
                            on ayat.BankId equals b.BankAccountId into bankJoin
                        from b in bankJoin.DefaultIfEmpty()

                        where ayat.IsDelete == false

                        orderby ayat.CreateDateTime descending

                        select new
                        {
                            ayat.AyatSilangId,
                            ayat.NoReferensi,
                            ayat.NoAyatSilang,

                            ayat.AsuransiId,
                            AsuransiName = a.NamaAsuransi,

                            ayat.BankId,
                            BankName = b.BankName,

                            ayat.TotalPembayaran,
                            ayat.TglPembayaran,

                            ayat.UserProcess,
                            ayat.IsSudahTerpakai,
                            ayat.Keterangan,

                            ayat.CreateDateTime,
                            ayat.CreateBy,

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
                    message = ex.Message
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
                var data = from ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                               // LEFT JOIN Asuransi
                           join a in _applicationDbContext.Asuransis.AsNoTracking()
                               on ayat.AsuransiId equals a.AsuransiId into asuransiJoin
                           from a in asuransiJoin.DefaultIfEmpty()

                               // LEFT JOIN MasterBanks
                           join b in _applicationDbContext.BankAccounts.AsNoTracking()
                               on ayat.BankId equals b.BankAccountId into bankJoin
                           from b in bankJoin.DefaultIfEmpty()

                           join u in _applicationDbContext.UserActives.AsNoTracking()
                               on ayat.CreateBy equals u.UserActiveId

                           where ayat.IsDelete == false && ayat.AyatSilangId == id

                           select new
                           {
                               ayat.AyatSilangId,

                               ayat.NoReferensi,
                               ayat.NoAyatSilang,

                               ayat.AsuransiId,
                               AsuransiName = a.NamaAsuransi,

                               ayat.BankId,
                               BankName = b.BankName,

                               ayat.TotalPembayaran,
                               ayat.TglPembayaran,

                               ayat.IsSudahTerpakai,

                               ayat.Keterangan,

                               ayat.CreateDateTime,

                               CreateByName = u.FullName
                           };

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
        // GENERATE NOMOR
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AyatSilangViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message = "User tidak terautentikasi."
                    });
                }

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

                // =========================================================
                // GENERATE NO AYAT SILANG
                // FORMAT :
                // AS-220526-0001
                // =========================================================

                DateTime now = DateTime.UtcNow;

                string dateFormat = now.ToString("ddMMyy");

                // cari data terakhir bulan & tahun yg sama
                var lastData =
                    await _applicationDbContext.AyatSilangs
                    .Where(x =>
                        x.CreateDateTime.Month == now.Month &&
                        x.CreateDateTime.Year == now.Year &&
                        x.IsDelete == false)
                    .OrderByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync();

                int runningNumber = 1;

                if (lastData != null &&
                    !string.IsNullOrWhiteSpace(lastData.NoAyatSilang))
                {
                    // ambil 4 digit terakhir
                    var splitData =
                        lastData.NoAyatSilang.Split('-');

                    if (splitData.Length >= 3)
                    {
                        int lastNumber =
                            int.Parse(splitData[2]);

                        runningNumber = lastNumber + 1;
                    }
                }

                string noAyatSilang =
                    $"AS-{dateFormat}-{runningNumber:0000}";

                // =========================================================
                // CREATE ENTITY
                // =========================================================

                var data = new AyatSilang
                {
                    AyatSilangId = Guid.NewGuid(),

                    NoReferensi = model.NoReferensi,

                    NoAyatSilang = noAyatSilang,

                    AsuransiId = model.AsuransiId,

                    BankId = model.BankId,

                    TotalPembayaran = model.TotalPembayaran,

                    TglPembayaran = model.TglPembayaran,

                    UserProcess = getUserActive.UserActiveId,

                    IsSudahTerpakai = false,

                    Keterangan = model.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,

                    IsDelete = false
                };

                _applicationDbContext.AyatSilangs.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data.AyatSilangId,
                        noAyatSilang
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] Models.AyatSilang model)
        {
            try
            {
                var data =
                    await _applicationDbContext.AyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.AyatSilangId == id &&
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

                data.NoReferensi = model.NoReferensi;

                data.AsuransiId = model.AsuransiId;
                data.BankId = model.BankId;

                data.TotalPembayaran = model.TotalPembayaran;
                data.TglPembayaran = model.TglPembayaran;

                data.IsSudahTerpakai = model.IsSudahTerpakai;

                data.Keterangan = model.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = getUserActive.UserActiveId;

                _applicationDbContext.AyatSilangs.Update(data);

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
                    await _applicationDbContext.AyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.AyatSilangId == id &&
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

                _applicationDbContext.AyatSilangs.Update(data);

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

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,

            string? noAyatSilang = null,
            string? noReferensi = null,
            string? asuransi = null,
            string? bank = null,
            string? search = null,

            DateTime? startDate = null,
            DateTime? endDate = null,

            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc"
        )
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query =
                    from ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                        // LEFT JOIN Asuransi
                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ayat.AsuransiId equals a.AsuransiId into asuransiJoin
                    from a in asuransiJoin.DefaultIfEmpty()

                        // LEFT JOIN MasterBanks
                    join b in _applicationDbContext.BankAccounts.AsNoTracking()
                        on ayat.BankId equals b.BankAccountId into bankJoin
                    from b in bankJoin.DefaultIfEmpty()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ayat.CreateBy equals u.UserActiveId

                    where ayat.IsDelete == false

                    select new
                    {
                        ayat.AyatSilangId,

                        ayat.NoReferensi,
                        ayat.NoAyatSilang,

                        ayat.AsuransiId,
                        AsuransiName = a.NamaAsuransi,

                        ayat.BankId,
                        BankName = b.BankName,

                        ayat.TotalPembayaran,
                        ayat.TglPembayaran,

                        ayat.IsSudahTerpakai,

                        ayat.Keterangan,

                        ayat.CreateDateTime,

                        CreateByName = u.FullName
                    };

                // FILTER

                if (!string.IsNullOrWhiteSpace(noAyatSilang))
                {
                    string keyword = $"%{noAyatSilang.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword));
                }

                if (!string.IsNullOrWhiteSpace(noReferensi))
                {
                    string keyword = $"%{noReferensi.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoReferensi, keyword));
                }

                if (!string.IsNullOrWhiteSpace(asuransi))
                {
                    string keyword = $"%{asuransi.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.AsuransiName, keyword));
                }

                if (!string.IsNullOrWhiteSpace(bank))
                {
                    string keyword = $"%{bank.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.BankName, keyword));
                }

                // SEARCH GLOBAL

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword) ||
                        EF.Functions.ILike(x.NoReferensi, keyword) ||
                        EF.Functions.ILike(x.AsuransiName, keyword) ||
                        EF.Functions.ILike(x.BankName, keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword));
                }

                // FILTER DATE

                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(x =>
                        x.CreateDateTime >= startDate.Value &&
                        x.CreateDateTime <= endDate.Value);
                }

                // SORTING

                bool isDescending =
                    sortDirection?.ToLower() == "desc";

                query = orderBy?.ToLower() switch
                {
                    "noayatsilang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoAyatSilang)
                            : query.OrderBy(x => x.NoAyatSilang),

                    "asuransi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.AsuransiName)
                            : query.OrderBy(x => x.AsuransiName),

                    "bank" =>
                        isDescending
                            ? query.OrderByDescending(x => x.BankName)
                            : query.OrderBy(x => x.BankName),

                    "totalpembayaran" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalPembayaran)
                            : query.OrderBy(x => x.TotalPembayaran),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime)
                };

                int totalRows = await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",

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
                    message = ex.Message
                });
            }
        }
    }
}