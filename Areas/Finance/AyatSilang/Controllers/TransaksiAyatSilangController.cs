using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TransaksiAyatSilangController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TransaksiAyatSilangController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TransaksiAyatSilangController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TransaksiAyatSilangController> logger,
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
                        from trx in _applicationDbContext.TransaksiAyatSilangs.AsNoTracking()

                        join ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                            on trx.AyatSilangId equals ayat.AyatSilangId

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                            on trx.CreateBy equals u.UserActiveId

                        where trx.IsDelete == false

                        orderby trx.CreateDateTime descending

                        select new
                        {
                            trx.TransAyatSilangId,

                            trx.AyatSilangId,
                            ayat.NoAyatSilang,

                            trx.TglTransaksiMasuk,
                            trx.SaldoKredit,

                            trx.TglTransaksiKeluar,
                            trx.SaldoDebet,

                            trx.Keterangan,

                            trx.CreateDateTime,
                            trx.CreateBy,

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
                var data =
                    await _applicationDbContext.TransaksiAyatSilangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.TransAyatSilangId == id &&
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
        // CREATE TRANSAKSI MASUK
        // =========================================================

        [HttpPost("kredit")]
        public async Task<IActionResult> CreateKredit(
            [FromBody] TransaksiAyatSilang model)
        {
            try
            {
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

                var ayatSilang =
                    await _applicationDbContext.AyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.AyatSilangId == model.AyatSilangId &&
                        x.IsDelete == false);

                if (ayatSilang == null)
                {
                    return NotFound(new
                    {
                        message = "Ayat Silang tidak ditemukan."
                    });
                }

                var data = new TransaksiAyatSilang
                {
                    TransAyatSilangId = Guid.NewGuid(),

                    AyatSilangId = model.AyatSilangId,

                    TglTransaksiMasuk = model.TglTransaksiMasuk,

                    SaldoKredit = model.SaldoKredit,

                    TglTransaksiKeluar = DateTime.MinValue,

                    SaldoDebet = 0,

                    Keterangan = model.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,

                    IsDelete = false
                };

                ayatSilang.IsSudahTerpakai = false;

                _applicationDbContext.TransaksiAyatSilangs.Add(data);

                _applicationDbContext.AyatSilangs.Update(ayatSilang);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Transaksi kredit berhasil disimpan."
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
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // CREATE TRANSAKSI KELUAR
        // =========================================================

        [HttpPost("debet")]
        public async Task<IActionResult> CreateDebet(
            [FromBody] TransaksiAyatSilang model)
        {
            try
            {
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

                var ayatSilang =
                    await _applicationDbContext.AyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.AyatSilangId == model.AyatSilangId &&
                        x.IsDelete == false);

                if (ayatSilang == null)
                {
                    return NotFound(new
                    {
                        message = "Ayat Silang tidak ditemukan."
                    });
                }

                // =========================================================
                // HITUNG TOTAL SALDO
                // =========================================================

                decimal totalKredit =
                    await _applicationDbContext.TransaksiAyatSilangs
                    .Where(x =>
                        x.AyatSilangId == model.AyatSilangId &&
                        x.IsDelete == false)
                    .SumAsync(x => x.SaldoKredit);

                decimal totalDebet =
                    await _applicationDbContext.TransaksiAyatSilangs
                    .Where(x =>
                        x.AyatSilangId == model.AyatSilangId &&
                        x.IsDelete == false)
                    .SumAsync(x => x.SaldoDebet);

                decimal saldoTersedia =
                    totalKredit - totalDebet;

                if (model.SaldoDebet > saldoTersedia)
                {
                    return BadRequest(new
                    {
                        message = "Saldo tidak mencukupi."
                    });
                }

                var data = new TransaksiAyatSilang
                {
                    TransAyatSilangId = Guid.NewGuid(),

                    AyatSilangId = model.AyatSilangId,

                    TglTransaksiMasuk = DateTime.MinValue,

                    SaldoKredit = 0,

                    TglTransaksiKeluar = model.TglTransaksiKeluar,

                    SaldoDebet = model.SaldoDebet,

                    Keterangan = model.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,

                    IsDelete = false
                };

                _applicationDbContext.TransaksiAyatSilangs.Add(data);

                // =========================================================
                // UPDATE STATUS TERPAKAI
                // =========================================================

                decimal saldoAkhir =
                    saldoTersedia - model.SaldoDebet;

                ayatSilang.IsSudahTerpakai =
                    saldoAkhir <= 0;

                _applicationDbContext.AyatSilangs.Update(ayatSilang);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Transaksi debet berhasil disimpan.",
                        saldoAkhir
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
                    await _applicationDbContext.TransaksiAyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.TransAyatSilangId == id &&
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

                _applicationDbContext.TransaksiAyatSilangs.Update(data);

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

            Guid? ayatSilangId = null,

            string? noAyatSilang = null,
            string? search = null,

            DateTime? startDate = null,
            DateTime? endDate = null,

            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc"
        )
        {
            try
            {
                var query =
                    from trx in _applicationDbContext.TransaksiAyatSilangs.AsNoTracking()

                    join ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                        on trx.AyatSilangId equals ayat.AyatSilangId

                    where trx.IsDelete == false

                    select new
                    {
                        trx.TransAyatSilangId,

                        trx.AyatSilangId,

                        ayat.NoAyatSilang,

                        trx.TglTransaksiMasuk,
                        trx.SaldoKredit,

                        trx.TglTransaksiKeluar,
                        trx.SaldoDebet,

                        trx.Keterangan,

                        trx.CreateDateTime
                    };

                // FILTER

                if (ayatSilangId.HasValue)
                {
                    query = query.Where(x =>
                        x.AyatSilangId == ayatSilangId.Value);
                }

                if (!string.IsNullOrWhiteSpace(noAyatSilang))
                {
                    string keyword = $"%{noAyatSilang.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword));
                }

                // SEARCH

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword) ||
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
                    "saldokredit" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SaldoKredit)
                            : query.OrderBy(x => x.SaldoKredit),

                    "saldodebet" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SaldoDebet)
                            : query.OrderBy(x => x.SaldoDebet),

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