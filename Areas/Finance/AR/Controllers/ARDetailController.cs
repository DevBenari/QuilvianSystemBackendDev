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

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ARDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ARDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ARDetailController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ARDetailController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedARDetail(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglKunjungan",
            string? sortDirection = "desc",

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time")]
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
                    from d in _applicationDbContext.ARDetails
                        .AsNoTracking()

                    join h in _applicationDbContext.ARHeaders
                        .AsNoTracking()
                    on d.ARHeaderId equals h.ARHeaderId

                    where h.IsDelete == false

                    select new
                    {
                        d.ARDetailId,

                        d.AsuransiId,
                        d.ARHeaderId,

                        d.KunjunganId,
                        d.PasienId,

                        d.NoRM,
                        d.NamaPasien,

                        d.NoBilling,
                        d.NoRegistrasi,

                        d.TglKunjungan,
                        d.TglKeluar,

                        d.TotalPiutang,
                        d.DiskonTagihan,
                        d.SelisihTagihan,
                        d.TotalSetelahDiskon,

                        d.Keterangan,

                        h.NoInvoice
                    };

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoRM, search) ||
                        EF.Functions.ILike(x.NamaPasien, search) ||
                        EF.Functions.ILike(x.NoBilling, search) ||
                        EF.Functions.ILike(x.NoRegistrasi, search) ||
                        EF.Functions.ILike(x.NoInvoice, search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                // FILTER DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc =
                        startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc =
                        endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.TglKunjungan >= startUtc &&
                        x.TglKunjungan <= endUtc);
                }

                // SORTING
                var sortColumn =
                    orderBy?.ToLower() ?? "tglkunjungan";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "norm" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoRM)
                            : query.OrderBy(x => x.NoRM),

                    "namapasien" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaPasien)
                            : query.OrderBy(x => x.NamaPasien),

                    "nobilling" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoBilling)
                            : query.OrderBy(x => x.NoBilling),

                    "noregistrasi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoRegistrasi)
                            : query.OrderBy(x => x.NoRegistrasi),

                    "totalpiutang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalPiutang)
                            : query.OrderBy(x => x.TotalPiutang),

                    "tglkunjungan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglKunjungan)
                            : query.OrderBy(x => x.TglKunjungan),

                    _ =>
                        query.OrderByDescending(x => x.TglKunjungan)
                };

                // PAGINATION
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
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ARDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ARDetailId == id);

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

        // =====================================================
        // CREATE
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ARDetailViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool headerExists =
                    await _applicationDbContext.ARHeaders
                    .AnyAsync(x =>
                        x.ARHeaderId == vm.ARHeaderId &&
                        x.IsDelete == false);

                if (!headerExists)
                {
                    return NotFound(new
                    {
                        message = "AR Header tidak ditemukan."
                    });
                }

                var data = new ARDetail
                {
                    ARDetailId = Guid.NewGuid(),

                    AsuransiId = vm.AsuransiId,
                    ARHeaderId = vm.ARHeaderId,

                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,

                    NoRM = vm.NoRM,
                    NamaPasien = vm.NamaPasien,

                    NoBilling = vm.NoBilling,
                    NoRegistrasi = vm.NoRegistrasi,

                    TglKunjungan = vm.TglKunjungan,
                    TglKeluar = vm.TglKeluar,

                    TotalPiutang = vm.TotalPiutang,
                    DiskonTagihan = vm.DiskonTagihan,
                    SelisihTagihan = vm.SelisihTagihan,
                    TotalSetelahDiskon = vm.TotalSetelahDiskon,

                    Keterangan = vm.Keterangan
                };

                _applicationDbContext.ARDetails.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil."
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

        // =====================================================
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ARDetailViewModel vm)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARDetails
                    .FirstOrDefaultAsync(x =>
                        x.ARDetailId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                data.AsuransiId = vm.AsuransiId;
                data.ARHeaderId = vm.ARHeaderId;

                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;

                data.NoRM = vm.NoRM;
                data.NamaPasien = vm.NamaPasien;

                data.NoBilling = vm.NoBilling;
                data.NoRegistrasi = vm.NoRegistrasi;

                data.TglKunjungan = vm.TglKunjungan;
                data.TglKeluar = vm.TglKeluar;

                data.TotalPiutang = vm.TotalPiutang;
                data.DiskonTagihan = vm.DiskonTagihan;
                data.SelisihTagihan = vm.SelisihTagihan;
                data.TotalSetelahDiskon = vm.TotalSetelahDiskon;

                data.Keterangan = vm.Keterangan;

                _applicationDbContext.ARDetails.Update(data);

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

        // =====================================================
        // DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARDetails
                    .FirstOrDefaultAsync(x =>
                        x.ARDetailId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                _applicationDbContext.ARDetails.Remove(data);

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