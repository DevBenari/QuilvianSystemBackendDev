//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuilvianSystemBackendDev.Areas.Finance.AR.Models;
//using QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels;
//using QuilvianSystemBackendDev.Models;
//using QuilvianSystemBackendDev.Repositories;
//using Swashbuckle.AspNetCore.Annotations;

//namespace QuilvianSystemBackendDev.Areas.Finance.AR.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    [EnableCors("AllowSpecific")]
//    public class ARSettlementDetailController : ControllerBase
//    {
//        private readonly ApplicationDbContext _applicationDbContext;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        private readonly ILogger<ARSettlementDetailController> _logger;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public ARSettlementDetailController
//        (
//            ApplicationDbContext context,
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            ILogger<ARSettlementDetailController> logger,
//            IWebHostEnvironment webHostEnvironment
//        )
//        {
//            _applicationDbContext = context;
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _logger = logger;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        // =====================================================
//        // PAGED
//        // =====================================================

//        [HttpGet("paged")]
//        public async Task<IActionResult> PagedARSettlementDetail(
//            int page = 1,
//            int perPage = 10,
//            string? search = null,
//            string? orderBy = "TglTransaksi",
//            string? sortDirection = "desc",

//            [FromQuery, SwaggerSchema(Format = "date-time")]
//            DateTime? startDate = null,

//            [FromQuery, SwaggerSchema(Format = "date-time")]
//            DateTime? endDate = null
//        )
//        {
//            try
//            {
//                if (!await _applicationDbContext.Database.CanConnectAsync())
//                {
//                    return StatusCode(500, new
//                    {
//                        message = "Tidak dapat terhubung ke database."
//                    });
//                }

//                if (page < 1)
//                    page = 1;

//                if (perPage < 1)
//                    perPage = 10;

//                var query = _applicationDbContext.ARSettlementDetails
//                    .AsNoTracking()
//                    .AsQueryable();

//                // SEARCH
//                if (!string.IsNullOrWhiteSpace(search))
//                {
//                    search = $"%{search.Trim().ToLower()}%";

//                    query = query.Where(x =>
//                        EF.Functions.ILike(x.NoRegistrasi, search) ||
//                        EF.Functions.ILike(x.NoBill, search) ||
//                        EF.Functions.ILike(x.NoInvoice, search) ||
//                        EF.Functions.ILike(x.User, search) ||
//                        EF.Functions.ILike(x.TipeSettlement, search) ||
//                        EF.Functions.ILike(x.Keterangan ?? "", search)
//                    );
//                }

//                // FILTER DATE
//                if (startDate.HasValue && endDate.HasValue)
//                {
//                    DateTime startUtc =
//                        startDate.Value.Date.ToUniversalTime();

//                    DateTime endUtc =
//                        endDate.Value.Date
//                        .AddDays(1)
//                        .AddTicks(-1)
//                        .ToUniversalTime();

//                    query = query.Where(x =>
//                        x.TglTransaksi >= startUtc &&
//                        x.TglTransaksi <= endUtc);
//                }

//                // SORTING
//                var sortColumn =
//                    orderBy?.ToLower() ?? "tgltransaksi";

//                var isDescending =
//                    sortDirection?.ToLower() == "desc";

//                query = sortColumn switch
//                {
//                    "noregistrasi" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.NoRegistrasi)
//                            : query.OrderBy(x => x.NoRegistrasi),

//                    "nobill" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.NoBill)
//                            : query.OrderBy(x => x.NoBill),

//                    "noinvoice" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.NoInvoice)
//                            : query.OrderBy(x => x.NoInvoice),

//                    "jumlahuang" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.JumlahUang)
//                            : query.OrderBy(x => x.JumlahUang),

//                    "saldo" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.Saldo)
//                            : query.OrderBy(x => x.Saldo),

//                    "tgltransaksi" =>
//                        isDescending
//                            ? query.OrderByDescending(x => x.TglTransaksi)
//                            : query.OrderBy(x => x.TglTransaksi),

//                    _ =>
//                        query.OrderByDescending(x => x.TglTransaksi)
//                };

//                // PAGINATION
//                int totalRows = await query.CountAsync();

//                int totalPages =
//                    (int)Math.Ceiling(totalRows / (double)perPage);

//                var rows = await query
//                    .Skip((page - 1) * perPage)
//                    .Take(perPage)
//                    .ToListAsync();

//                return Ok(new
//                {
//                    status = "success",
//                    message = "Data berhasil diambil",

//                    data = new
//                    {
//                        Rows = rows,
//                        TotalRows = totalRows,
//                        CurrentPage = page,
//                        PerPage = perPage,
//                        TotalPages = totalPages
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, ex.Message);

//                return StatusCode(500, new
//                {
//                    message = ex.Message
//                });
//            }
//        }

//        // =====================================================
//        // GET BY ID
//        // =====================================================

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(Guid id)
//        {
//            try
//            {
//                var data = await _applicationDbContext.ARSettlementDetails
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(x =>
//                        x.DetailSettlementARId == id);

//                if (data == null)
//                {
//                    return NotFound(new
//                    {
//                        message = "Data tidak ditemukan."
//                    });
//                }

//                return Ok(new
//                {
//                    status = "success",
//                    data
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, ex.Message);

//                return StatusCode(500, new
//                {
//                    message = ex.Message
//                });
//            }
//        }

//        // =====================================================
//        // CREATE
//        // =====================================================

//        [HttpPost]
//        public async Task<IActionResult> Create(
//            [FromBody] ARSettlementDetailViewModel vm)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                bool settlementExists =
//                    await _applicationDbContext.ARSettlements
//                    .AnyAsync(x =>
//                        x.SettlementARId == vm.SettlementARId);

//                if (!settlementExists)
//                {
//                    return NotFound(new
//                    {
//                        message = "AR Settlement tidak ditemukan."
//                    });
//                }

//                var data = new ARSettlementDetail
//                {
//                    DetailSettlementARId = Guid.NewGuid(),

//                    SettlementARId = vm.SettlementARId,

//                    NoRegistrasi = vm.NoRegistrasi,
//                    NoBill = vm.NoBill,
//                    NoInvoice = vm.NoInvoice,

//                    TglTransaksi = vm.TglTransaksi,

//                    JumlahUang = vm.JumlahUang,
//                    Saldo = vm.Saldo,

//                    PembayaranKe = vm.PembayaranKe,

//                    IsCanceled = vm.IsCanceled,

//                    User = vm.User,

//                    TipeSettlement = vm.TipeSettlement,
//                    Keterangan = vm.Keterangan
//                };

//                _applicationDbContext.ARSettlementDetails.Add(data);

//                int result =
//                    await _applicationDbContext.SaveChangesAsync();

//                if (result > 0)
//                {
//                    return Created("", new
//                    {
//                        message = "Tambah data berhasil.",
//                        data
//                    });
//                }

//                return StatusCode(500, new
//                {
//                    message = "Gagal menyimpan data."
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, ex.Message);

//                return StatusCode(500, new
//                {
//                    message = ex.Message
//                });
//            }
//        }

//        // =====================================================
//        // UPDATE
//        // =====================================================

//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(
//            Guid id,
//            [FromBody] ARSettlementDetailViewModel vm)
//        {
//            try
//            {
//                var data =
//                    await _applicationDbContext.ARSettlementDetails
//                    .FirstOrDefaultAsync(x =>
//                        x.DetailSettlementARId == id);

//                if (data == null)
//                {
//                    return NotFound(new
//                    {
//                        message = "Data tidak ditemukan."
//                    });
//                }

//                data.SettlementARId = vm.SettlementARId;

//                data.NoRegistrasi = vm.NoRegistrasi;
//                data.NoBill = vm.NoBill;
//                data.NoInvoice = vm.NoInvoice;

//                data.TglTransaksi = vm.TglTransaksi;

//                data.JumlahUang = vm.JumlahUang;
//                data.Saldo = vm.Saldo;

//                data.PembayaranKe = vm.PembayaranKe;

//                data.IsCanceled = vm.IsCanceled;

//                data.User = vm.User;

//                data.TipeSettlement = vm.TipeSettlement;
//                data.Keterangan = vm.Keterangan;

//                _applicationDbContext.ARSettlementDetails.Update(data);

//                int result =
//                    await _applicationDbContext.SaveChangesAsync();

//                if (result > 0)
//                {
//                    return Ok(new
//                    {
//                        message = "Update data berhasil."
//                    });
//                }

//                return StatusCode(500, new
//                {
//                    message = "Gagal update data."
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, ex.Message);

//                return StatusCode(500, new
//                {
//                    message = ex.Message
//                });
//            }
//        }

//        // =====================================================
//        // DELETE
//        // =====================================================

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            try
//            {
//                var data =
//                    await _applicationDbContext.ARSettlementDetails
//                    .FirstOrDefaultAsync(x =>
//                        x.DetailSettlementARId == id);

//                if (data == null)
//                {
//                    return NotFound(new
//                    {
//                        message = "Data tidak ditemukan."
//                    });
//                }

//                _applicationDbContext.ARSettlementDetails.Remove(data);

//                int result =
//                    await _applicationDbContext.SaveChangesAsync();

//                if (result > 0)
//                {
//                    return Ok(new
//                    {
//                        message = "Delete berhasil."
//                    });
//                }

//                return StatusCode(500, new
//                {
//                    message = "Gagal delete data."
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, ex.Message);

//                return StatusCode(500, new
//                {
//                    message = ex.Message
//                });
//            }
//        }
//    }
//}