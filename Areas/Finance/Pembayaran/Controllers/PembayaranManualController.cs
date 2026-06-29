using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PembayaranManualController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PembayaranManualController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PembayaranManualController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PembayaranManualController> logger,
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
            var emailLogin =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailLogin))
                return null;

            var userActive =
                await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            return userActive?.UserActiveId;
        }

        private async Task<string> GenerateKodePembayaranManualAsync()
        {
            var prefix = $"APPAY-Manual-{DateTime.Now:yy}-";

            var lastCode =
                await _applicationDbContext.PembayaranManuals
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.KodePembayaranManual != null &&
                    x.KodePembayaranManual.StartsWith(prefix))
                .OrderByDescending(x => x.KodePembayaranManual)
                .Select(x => x.KodePembayaranManual)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.Length > prefix.Length)
            {
                var numberPart =
                    lastCode.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        // =====================================================
        // PAGED HEADER PEMBAYARAN MANUAL
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedPembayaranManual(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglPembayaranManual",
            string? sortDirection = "desc",
            Guid? supplierId = null,
            Guid? mataUangId = null,
            Guid? poId = null,
            string? statusPembayaranManual = null,

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

                var baseQuery =
                    _applicationDbContext.PembayaranManuals
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.KodePembayaranManual ?? "", keyword) ||
                        EF.Functions.ILike(x.NoReferensiManual ?? "", keyword) ||
                        EF.Functions.ILike(x.NomorFakturPajak ?? "", keyword) ||
                        EF.Functions.ILike(x.StatusPembayaranManual ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||

                        _applicationDbContext.Suppliers.Any(s =>
                            s.SupplierId == x.SupplierId &&
                            EF.Functions.ILike(s.SupplierName ?? "", keyword)
                        )
                    );
                }

                if (supplierId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                if (mataUangId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.MataUangId == mataUangId.Value);
                }

                if (poId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.PoId == poId.Value);
                }

                if (!string.IsNullOrWhiteSpace(statusPembayaranManual))
                {
                    var status = statusPembayaranManual.Trim();

                    baseQuery = baseQuery.Where(x =>
                        x.StatusPembayaranManual != null &&
                        x.StatusPembayaranManual.ToLower() == status.ToLower());
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc =
                        startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc =
                        endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    baseQuery = baseQuery.Where(x =>
                        x.TglPembayaranManual >= startUtc &&
                        x.TglPembayaranManual <= endUtc);
                }

                var query =
                    baseQuery.Select(x => new
                    {
                        x.PembayaranManualId,
                        x.KodePembayaranManual,
                        x.TglDokumen,
                        x.TglPembayaranManual,
                        x.MataUangId,
                        x.ExchangeRateId,
                        x.TglJatuhTempo,
                        x.SupplierId,

                        SupplierNama =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        PPN =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.PPN)
                            .FirstOrDefault(),

                        x.PajakId,
                        x.PersenanPajak,
                        x.NominalPajak,
                        x.NomorFakturPajak,
                        x.TglFakturPajak,
                        x.PoId,
                        x.NoReferensiManual,
                        x.StatusPembayaranManual,
                        x.Keterangan,

                        TotalDetail =
                            _applicationDbContext.DetailPembayaranManuals
                            .Where(d =>
                                d.PembayaranManualId == x.PembayaranManualId &&
                                d.IsDelete == false)
                            .Sum(d => (decimal?)d.NominalPembayaran) ?? 0,

                        JumlahDetail =
                            _applicationDbContext.DetailPembayaranManuals
                            .Count(d =>
                                d.PembayaranManualId == x.PembayaranManualId &&
                                d.IsDelete == false),

                        x.CreateDateTime,
                        x.UpdateDateTime
                    });

                var sortColumn =
                    orderBy?.ToLower() ?? "tglpembayaranmanual";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodepembayaranmanual" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePembayaranManual)
                            : query.OrderBy(x => x.KodePembayaranManual),

                    "tglpembayaranmanual" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglPembayaranManual)
                            : query.OrderBy(x => x.TglPembayaranManual),

                    "tgldokumen" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglDokumen)
                            : query.OrderBy(x => x.TglDokumen),

                    "tgljatuhtempo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglJatuhTempo)
                            : query.OrderBy(x => x.TglJatuhTempo),

                    "suppliernama" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SupplierNama)
                            : query.OrderBy(x => x.SupplierNama),

                    "statuspembayaranmanual" =>
                        isDescending
                            ? query.OrderByDescending(x => x.StatusPembayaranManual)
                            : query.OrderBy(x => x.StatusPembayaranManual),

                    "totaldetail" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalDetail)
                            : query.OrderBy(x => x.TotalDetail),

                    _ =>
                        query.OrderByDescending(x => x.TglPembayaranManual)
                };

                int totalRows =
                    await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                if (totalRows == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "No data found",
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
                {
                    return NotFound(new
                    {
                        message = "Page not found."
                    });
                }

                var rows =
                    await query
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
        // GET BY ID HEADER + SUMMARY DETAIL
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.PembayaranManuals
                    .AsNoTracking()
                    .Where(x =>
                        x.PembayaranManualId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.PembayaranManualId,
                        x.KodePembayaranManual,
                        x.TglDokumen,
                        x.TglPembayaranManual,
                        x.MataUangId,
                        x.ExchangeRateId,
                        x.TglJatuhTempo,
                        x.SupplierId,

                        SupplierNama =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        PPN =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.PPN)
                            .FirstOrDefault(),

                        x.PajakId,
                        x.PersenanPajak,
                        x.NominalPajak,
                        x.NomorFakturPajak,
                        x.TglFakturPajak,
                        x.PoId,
                        x.NoReferensiManual,
                        x.StatusPembayaranManual,
                        x.Keterangan,

                        TotalDetail =
                            _applicationDbContext.DetailPembayaranManuals
                            .Where(d =>
                                d.PembayaranManualId == x.PembayaranManualId &&
                                d.IsDelete == false)
                            .Sum(d => (decimal?)d.NominalPembayaran) ?? 0,

                        JumlahDetail =
                            _applicationDbContext.DetailPembayaranManuals
                            .Count(d =>
                                d.PembayaranManualId == x.PembayaranManualId &&
                                d.IsDelete == false)
                    })
                    .FirstOrDefaultAsync();

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
        // CREATE HEADER ONLY
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PembayaranManualViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                var supplierExists =
                    await _applicationDbContext.Suppliers
                    .AnyAsync(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false);

                if (!supplierExists)
                {
                    return BadRequest(new
                    {
                        message = "Supplier tidak ditemukan."
                    });
                }

                var pembayaranManualId =
                    Guid.NewGuid();

                var kodePembayaranManual =
                    await GenerateKodePembayaranManualAsync();

                var data = new PembayaranManual
                {
                    PembayaranManualId = pembayaranManualId,
                    KodePembayaranManual = kodePembayaranManual,

                    TglDokumen = vm.TglDokumen,
                    TglPembayaranManual = vm.TglPembayaranManual,
                    MataUangId = vm.MataUangId,
                    ExchangeRateId = vm.ExchangeRateId,
                    TglJatuhTempo = vm.TglJatuhTempo,
                    SupplierId = vm.SupplierId,

                    PajakId = vm.PajakId,
                    PersenanPajak = vm.PersenanPajak,
                    NominalPajak = vm.NominalPajak,
                    NomorFakturPajak = vm.NomorFakturPajak,
                    TglFakturPajak = vm.TglFakturPajak,
                    PoId = vm.PoId,
                    NoReferensiManual = vm.NoReferensiManual,

                    StatusPembayaranManual =
                        string.IsNullOrWhiteSpace(vm.StatusPembayaranManual)
                            ? "Tanpa Referensi"
                            : vm.StatusPembayaranManual,

                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.PembayaranManuals.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            PembayaranManualId = pembayaranManualId,
                            KodePembayaranManual = kodePembayaranManual
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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // UPDATE HEADER ONLY
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PembayaranManualViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.PembayaranManuals
                    .FirstOrDefaultAsync(x =>
                        x.PembayaranManualId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                var supplierExists =
                    await _applicationDbContext.Suppliers
                    .AnyAsync(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false);

                if (!supplierExists)
                {
                    return BadRequest(new
                    {
                        message = "Supplier tidak ditemukan."
                    });
                }

                data.TglDokumen = vm.TglDokumen;
                data.TglPembayaranManual = vm.TglPembayaranManual;
                data.MataUangId = vm.MataUangId;
                data.ExchangeRateId = vm.ExchangeRateId;
                data.TglJatuhTempo = vm.TglJatuhTempo;
                data.SupplierId = vm.SupplierId;
                data.PajakId = vm.PajakId;
                data.PersenanPajak = vm.PersenanPajak;
                data.NominalPajak = vm.NominalPajak;
                data.NomorFakturPajak = vm.NomorFakturPajak;
                data.TglFakturPajak = vm.TglFakturPajak;
                data.PoId = vm.PoId;
                data.NoReferensiManual = vm.NoReferensiManual;
                data.StatusPembayaranManual = vm.StatusPembayaranManual;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.PembayaranManuals.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE HEADER + DETAIL
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.PembayaranManuals
                    .FirstOrDefaultAsync(x =>
                        x.PembayaranManualId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId =
                    await GetUserActiveId();

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

                _applicationDbContext.PembayaranManuals.Update(data);

                var details =
                    await _applicationDbContext.DetailPembayaranManuals
                    .Where(x =>
                        x.PembayaranManualId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDelete = true;
                    detail.DeleteDateTime = DateTime.UtcNow;
                    detail.DeleteBy = userActiveId.Value;

                    _applicationDbContext.DetailPembayaranManuals.Update(detail);
                }

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}