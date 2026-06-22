using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AP.Models;
using QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class DetailPembayaranAPController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DetailPembayaranAPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DetailPembayaranAPController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailPembayaranAPController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<Guid?> GetUserActiveId()
        {
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailLogin))
                return null;

            var userActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            return userActive?.UserActiveId;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedDetailPembayaranAP(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? pembayaranAPId = null,
            Guid? purchasingInvoiceId = null,
            string? kodePurchasingInvoice = null,
            string? noInvoice = null,
            string? noTukarFaktur = null,

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

                if (perPage > 200)
                    perPage = 200;

                var query =
                    from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()

                    join ap in _applicationDbContext.PembayaranAPs.AsNoTracking()
                        on det.PembayaranAPId equals ap.PembayaranAPId into apJoin
                    from ap in apJoin.DefaultIfEmpty()

                    join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                        on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId into piJoin
                    from pi in piJoin.DefaultIfEmpty()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on det.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()

                    where det.IsDelete == false

                    select new
                    {
                        det.DetailPembayaranAPId,
                        det.PembayaranAPId,
                        det.PurchasingInvoiceId,

                        KodePembayaranAP = ap != null ? ap.KodePembayaranAP : null,

                        KodePurchasingInvoice =
                            pi == null ? null :
                            _applicationDbContext.DetailTukarFakturs
                                .Where(dtf =>
                                    dtf.POId == pi.POId &&
                                    dtf.IsDelete == false)
                                .Select(dtf => dtf.KodePurchasingInvoice)
                                .FirstOrDefault(),

                        TglPembuatanInvoice =
                            pi == null ? null : pi.TglPembuatanInvoice,

                        NoInvoice =
                            pi == null ? null : pi.NoInvoice,

                        NoTukarFaktur =
                            pi == null ? null :
                            (
                                from dtf in _applicationDbContext.DetailTukarFakturs.AsNoTracking()
                                join tf in _applicationDbContext.TukarFakturs.AsNoTracking()
                                    on dtf.TukarFakturId equals tf.TukarFakturId
                                where dtf.POId == pi.POId &&
                                      dtf.IsDelete == false &&
                                      tf.IsDelete == false
                                select tf.NoTukarFaktur
                            ).FirstOrDefault(),

                        TotalTagihan =
                            pi == null ? null : pi.POAmount,

                        det.SisaTagihan,
                        det.PembayaranTagihan,
                        det.Keterangan,
                        det.CreateDateTime,

                        CreateByName = u != null ? u.FullName : null
                    };

                // =====================================================
                // SEARCH GLOBAL
                // =====================================================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePembayaranAP ?? "", pattern) ||
                        EF.Functions.ILike(x.KodePurchasingInvoice ?? "", pattern) ||
                        EF.Functions.ILike(x.NoInvoice ?? "", pattern) ||
                        EF.Functions.ILike(x.NoTukarFaktur ?? "", pattern) ||
                        EF.Functions.ILike(x.Keterangan ?? "", pattern)
                    );
                }

                // =====================================================
                // FILTER
                // =====================================================
                if (pembayaranAPId.HasValue)
                {
                    query = query.Where(x =>
                        x.PembayaranAPId == pembayaranAPId.Value);
                }

                if (purchasingInvoiceId.HasValue)
                {
                    query = query.Where(x =>
                        x.PurchasingInvoiceId == purchasingInvoiceId.Value);
                }

                if (!string.IsNullOrWhiteSpace(kodePurchasingInvoice))
                {
                    var pattern = $"%{kodePurchasingInvoice.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePurchasingInvoice ?? "", pattern));
                }

                if (!string.IsNullOrWhiteSpace(noInvoice))
                {
                    var pattern = $"%{noInvoice.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoInvoice ?? "", pattern));
                }

                if (!string.IsNullOrWhiteSpace(noTukarFaktur))
                {
                    var pattern = $"%{noTukarFaktur.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoTukarFaktur ?? "", pattern));
                }

                // FILTER DATE BERDASARKAN TGL PEMBUATAN INVOICE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.TglPembuatanInvoice >= startUtc &&
                        x.TglPembuatanInvoice <= endUtc);
                }

                // =====================================================
                // SORTING
                // =====================================================
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodepembayaranap" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePembayaranAP)
                            : query.OrderBy(x => x.KodePembayaranAP),

                    "kodepurchasinginvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePurchasingInvoice)
                            : query.OrderBy(x => x.KodePurchasingInvoice),

                    "tglpembuataninvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglPembuatanInvoice)
                            : query.OrderBy(x => x.TglPembuatanInvoice),

                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "notukarfaktur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoTukarFaktur)
                            : query.OrderBy(x => x.NoTukarFaktur),

                    "totaltagihan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalTagihan)
                            : query.OrderBy(x => x.TotalTagihan),

                    "sisatagihan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SisaTagihan)
                            : query.OrderBy(x => x.SisaTagihan),

                    "pembayarantagihan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.PembayaranTagihan)
                            : query.OrderBy(x => x.PembayaranTagihan),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderByDescending(x => x.CreateDateTime)
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await
                    (
                        from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()

                        join ap in _applicationDbContext.PembayaranAPs.AsNoTracking()
                            on det.PembayaranAPId equals ap.PembayaranAPId into apJoin
                        from ap in apJoin.DefaultIfEmpty()

                        join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                            on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId into piJoin
                        from pi in piJoin.DefaultIfEmpty()

                        where det.DetailPembayaranAPId == id &&
                              det.IsDelete == false

                        select new
                        {
                            det.DetailPembayaranAPId,
                            det.PembayaranAPId,
                            det.PurchasingInvoiceId,

                            KodePembayaranAP = ap != null ? ap.KodePembayaranAP : null,

                            KodePurchasingInvoice =
                                pi == null ? null :
                                _applicationDbContext.DetailTukarFakturs
                                    .Where(dtf =>
                                        dtf.POId == pi.POId &&
                                        dtf.IsDelete == false)
                                    .Select(dtf => dtf.KodePurchasingInvoice)
                                    .FirstOrDefault(),

                            TglPembuatanInvoice =
                                pi == null ? null : pi.TglPembuatanInvoice,

                            NoInvoice =
                                pi == null ? null : pi.NoInvoice,

                            NoTukarFaktur =
                                pi == null ? null :
                                (
                                    from dtf in _applicationDbContext.DetailTukarFakturs.AsNoTracking()
                                    join tf in _applicationDbContext.TukarFakturs.AsNoTracking()
                                        on dtf.TukarFakturId equals tf.TukarFakturId
                                    where dtf.POId == pi.POId &&
                                          dtf.IsDelete == false &&
                                          tf.IsDelete == false
                                    select tf.NoTukarFaktur
                                ).FirstOrDefault(),

                            TotalTagihan =
                                pi == null ? null : pi.POAmount,

                            det.SisaTagihan,
                            det.PembayaranTagihan,
                            det.Keterangan,
                            det.CreateDateTime
                        }
                    ).FirstOrDefaultAsync();

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
        // GET BY PEMBAYARAN AP ID
        // =====================================================

        [HttpGet("by-pembayaran-ap/{pembayaranAPId:guid}")]
        public async Task<IActionResult> GetByPembayaranAPId(Guid pembayaranAPId)
        {
            try
            {
                var data = await
                    (
                        from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()

                        join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                            on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId into piJoin
                        from pi in piJoin.DefaultIfEmpty()

                        where det.PembayaranAPId == pembayaranAPId &&
                              det.IsDelete == false

                        select new
                        {
                            det.DetailPembayaranAPId,
                            det.PembayaranAPId,
                            det.PurchasingInvoiceId,

                            KodePurchasingInvoice =
                                pi == null ? null :
                                _applicationDbContext.DetailTukarFakturs
                                    .Where(dtf =>
                                        dtf.POId == pi.POId &&
                                        dtf.IsDelete == false)
                                    .Select(dtf => dtf.KodePurchasingInvoice)
                                    .FirstOrDefault(),

                            TglPembuatanInvoice =
                                pi == null ? null : pi.TglPembuatanInvoice,

                            NoInvoice =
                                pi == null ? null : pi.NoInvoice,

                            NoTukarFaktur =
                                pi == null ? null :
                                (
                                    from dtf in _applicationDbContext.DetailTukarFakturs.AsNoTracking()
                                    join tf in _applicationDbContext.TukarFakturs.AsNoTracking()
                                        on dtf.TukarFakturId equals tf.TukarFakturId
                                    where dtf.POId == pi.POId &&
                                          dtf.IsDelete == false &&
                                          tf.IsDelete == false
                                    select tf.NoTukarFaktur
                                ).FirstOrDefault(),

                            TotalTagihan =
                                pi == null ? null : pi.POAmount,

                            det.SisaTagihan,
                            det.PembayaranTagihan,
                            det.Keterangan,
                            det.CreateDateTime
                        }
                    )
                    .OrderByDescending(x => x.CreateDateTime)
                    .ToListAsync();

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

        // =====================================================
        // CREATE
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetailPembayaranAPViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userActiveId = await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                if (vm.PembayaranAPId == null)
                {
                    return BadRequest(new
                    {
                        message = "PembayaranAPId wajib diisi."
                    });
                }

                var pembayaranAPExists = await _applicationDbContext.PembayaranAPs
                    .AnyAsync(x =>
                        x.PembayaranAPId == vm.PembayaranAPId &&
                        x.IsDelete == false);

                if (!pembayaranAPExists)
                {
                    return NotFound(new
                    {
                        message = "Data Pembayaran AP tidak ditemukan."
                    });
                }

                var data = new DetailPembayaranAP
                {
                    DetailPembayaranAPId = Guid.NewGuid(),
                    PembayaranAPId = vm.PembayaranAPId,
                    PurchasingInvoiceId = vm.PurchasingInvoiceId,
                    SisaTagihan = vm.SisaTagihan,
                    PembayaranTagihan = vm.PembayaranTagihan,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.DetailPembayaranAPs.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.DetailPembayaranAPId
                        }
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] DetailPembayaranAPViewModel vm)
        {
            try
            {
                var data = await _applicationDbContext.DetailPembayaranAPs
                    .FirstOrDefaultAsync(x =>
                        x.DetailPembayaranAPId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId = await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.PembayaranAPId = vm.PembayaranAPId;
                data.PurchasingInvoiceId = vm.PurchasingInvoiceId;
                data.SisaTagihan = vm.SisaTagihan;
                data.PembayaranTagihan = vm.PembayaranTagihan;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.DetailPembayaranAPs.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

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

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.DetailPembayaranAPs
                    .FirstOrDefaultAsync(x =>
                        x.DetailPembayaranAPId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId = await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.IsDelete = true;
                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = userActiveId.Value;

                _applicationDbContext.DetailPembayaranAPs.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

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