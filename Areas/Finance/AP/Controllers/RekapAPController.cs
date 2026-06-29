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
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class RekapAPController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RekapAPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RekapAPController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RekapAPController> logger,
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

        // =====================================================
        // PAGED REKAP AP
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedRekapAP(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "NamaSupplier",
            string? sortDirection = "asc",
            Guid? supplierId = null,

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

                var hasDateFilter =
                    startDate.HasValue &&
                    endDate.HasValue;

                DateTime startUtc = DateTime.MinValue;
                DateTime endUtc = DateTime.MaxValue;

                if (hasDateFilter)
                {
                    startUtc =
                        startDate!.Value.Date.ToUniversalTime();

                    endUtc =
                        endDate!.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();
                }

                var query =
                    _applicationDbContext.Suppliers
                    .AsNoTracking()
                    .Where(s => s.IsDelete == false)
                    .Select(s => new
                    {
                        SupplierId = s.SupplierId,
                        NamaSupplier = s.SupplierName,

                        RekapAPId =
                            _applicationDbContext.RekapAPs
                            .Where(r =>
                                r.SupplierId == s.SupplierId &&
                                r.IsDelete == false)
                            .Select(r => (Guid?)r.RekapAPId)
                            .FirstOrDefault(),

                        RekapVariasiHarga =
                            _applicationDbContext.RekapAPs
                            .Where(r =>
                                r.SupplierId == s.SupplierId &&
                                r.IsDelete == false)
                            .Select(r => r.RekapVariasiHarga)
                            .FirstOrDefault() ?? 0,

                        RekapOther =
                            _applicationDbContext.RekapAPs
                            .Where(r =>
                                r.SupplierId == s.SupplierId &&
                                r.IsDelete == false)
                            .Select(r => r.RekapOther)
                            .FirstOrDefault() ?? 0,

                        Keterangan =
                            _applicationDbContext.RekapAPs
                            .Where(r =>
                                r.SupplierId == s.SupplierId &&
                                r.IsDelete == false)
                            .Select(r => r.Keterangan)
                            .FirstOrDefault(),

                        // =================================================
                        // RekapPenerimaan
                        // Kalkulasi HargaTotalPO dari ReceiveOrder
                        // =================================================
                        RekapPenerimaan =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro =>
                                ro.SupplierId == s.SupplierId &&
                                ro.IsDelete == false &&
                                (
                                    !hasDateFilter ||
                                    (
                                        ro.CreateDateTime >= startUtc &&
                                        ro.CreateDateTime <= endUtc
                                    )
                                )
                            )
                            .Sum(ro => (decimal?)ro.HargaTotalPO) ?? 0,

                        // =================================================
                        // RekapPPN
                        // Kalkulasi NominalPPN dari ReceiveOrder
                        // =================================================
                        RekapPPN =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro =>
                                ro.SupplierId == s.SupplierId &&
                                ro.IsDelete == false &&
                                (
                                    !hasDateFilter ||
                                    (
                                        ro.CreateDateTime >= startUtc &&
                                        ro.CreateDateTime <= endUtc
                                    )
                                )
                            )
                            .Sum(ro => (decimal?)ro.NominalPPN) ?? 0,

                        // =================================================
                        // RekapDiskon
                        // Kalkulasi TotalDiskon dari ReceiveOrder
                        // =================================================
                        RekapDiskon =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro =>
                                ro.SupplierId == s.SupplierId &&
                                ro.IsDelete == false &&
                                (
                                    !hasDateFilter ||
                                    (
                                        ro.CreateDateTime >= startUtc &&
                                        ro.CreateDateTime <= endUtc
                                    )
                                )
                            )
                            .Sum(ro => (decimal?)ro.TotalDiskon) ?? 0,

                        // =================================================
                        // RekapDiakui
                        // Kalkulasi detail tukar faktur berdasarkan SupplierId
                        // Jika di project Anda TukarFaktur punya field
                        // TotalInvoiceDetail langsung, bagian ini bisa diganti.
                        // =================================================
                        RekapDiakui =
                            _applicationDbContext.DetailTukarFakturs
                            .Where(d =>
                                d.SupplierId == s.SupplierId &&
                                d.IsDelete == false &&
                                _applicationDbContext.TukarFakturs.Any(tf =>
                                    tf.TukarFakturId == d.TukarFakturId &&
                                    tf.IsDelete == false &&
                                    (
                                        !hasDateFilter ||
                                        (
                                            tf.TglRegistrasi >= startUtc &&
                                            tf.TglRegistrasi <= endUtc
                                        )
                                    )
                                )
                            )
                            .Sum(d => (decimal?)d.NilaiPurchasingInvoice) ?? 0,

                        // =================================================
                        // RekapRetur
                        // Kalkulasi dari ItemRetur berdasarkan HeaderRetur.SupplierId
                        // Di model ItemRetur sebelumnya field totalnya SubtotalHarga.
                        // Kalau di project Anda namanya HargaTotal, ganti SubtotalHarga.
                        // =================================================
                        RekapRetur =
                            (
                                from item in _applicationDbContext.ItemReturs
                                join header in _applicationDbContext.HeaderReturs
                                    on item.HeaderReturId equals header.HeaderReturId
                                where
                                    item.IsDelete == false &&
                                    header.IsDelete == false &&
                                    header.SupplierId == s.SupplierId &&
                                    (
                                        !hasDateFilter ||
                                        (
                                            item.TglRetur >= startUtc &&
                                            item.TglRetur <= endUtc
                                        )
                                    )
                                select (decimal?)item.SubtotalHarga
                            )
                            .Sum() ?? 0,

                        // =================================================
                        // RekapDibayar
                        // Dari PembayaranAP berdasarkan SupplierId
                        // Saya pakai TotalTagihan karena model Anda punya field itu.
                        // Jika ada TotalPembayaranTagihan, ganti TotalTagihan.
                        // =================================================
                        RekapDibayar =
                            _applicationDbContext.PembayaranAPs
                            .Where(p =>
                                p.SupplierId == s.SupplierId &&
                                p.IsDelete == false &&
                                (
                                    !hasDateFilter ||
                                    (
                                        p.TglPembayaranAP >= startUtc &&
                                        p.TglPembayaranAP <= endUtc
                                    )
                                )
                            )
                            .Sum(p => (decimal?)p.TotalTagihan) ?? 0,

                        // =================================================
                        // SisaTagihan
                        // Dari DetailPembayaranAP berdasarkan SupplierId parent PembayaranAP
                        // =================================================
                        SisaTagihan =
                            (
                                from detail in _applicationDbContext.DetailPembayaranAPs
                                join pembayaran in _applicationDbContext.PembayaranAPs
                                    on detail.PembayaranAPId equals pembayaran.PembayaranAPId
                                where
                                    detail.IsDelete == false &&
                                    pembayaran.IsDelete == false &&
                                    pembayaran.SupplierId == s.SupplierId &&
                                    (
                                        !hasDateFilter ||
                                        (
                                            pembayaran.TglPembayaranAP >= startUtc &&
                                            pembayaran.TglPembayaranAP <= endUtc
                                        )
                                    )
                                select (decimal?)detail.SisaTagihan
                            )
                            .Sum() ?? 0
                    })
                    .Select(x => new
                    {
                        x.RekapAPId,
                        x.SupplierId,
                        x.NamaSupplier,

                        x.RekapPenerimaan,
                        x.RekapDiakui,
                        x.RekapPPN,
                        x.RekapVariasiHarga,
                        x.RekapDiskon,
                        x.RekapRetur,
                        x.RekapOther,

                        TotalRekap =
                            (x.RekapDiakui + x.RekapPPN) -
                            (x.RekapDiskon + x.RekapRetur),

                        x.RekapDibayar,
                        x.SisaTagihan,
                        x.Keterangan
                    });

                // =========================
                // Search
                // =========================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NamaSupplier ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword));
                }

                // =========================
                // Filter Supplier
                // =========================
                if (supplierId.HasValue)
                {
                    query = query.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                // =========================
                // Sorting
                // =========================
                var sortColumn =
                    orderBy?.ToLower() ?? "namasupplier";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "rekappenerimaan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapPenerimaan)
                            : query.OrderBy(x => x.RekapPenerimaan),

                    "rekapdiakui" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapDiakui)
                            : query.OrderBy(x => x.RekapDiakui),

                    "rekappn" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapPPN)
                            : query.OrderBy(x => x.RekapPPN),

                    "rekapvariasiharga" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapVariasiHarga)
                            : query.OrderBy(x => x.RekapVariasiHarga),

                    "rekapdiskon" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapDiskon)
                            : query.OrderBy(x => x.RekapDiskon),

                    "rekapretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapRetur)
                            : query.OrderBy(x => x.RekapRetur),

                    "rekapother" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapOther)
                            : query.OrderBy(x => x.RekapOther),

                    "totalrekap" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalRekap)
                            : query.OrderBy(x => x.TotalRekap),

                    "rekapdibayar" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RekapDibayar)
                            : query.OrderBy(x => x.RekapDibayar),

                    "sisatagihan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SisaTagihan)
                            : query.OrderBy(x => x.SisaTagihan),

                    _ =>
                        query.OrderBy(x => x.NamaSupplier)
                };

                // =========================
                // Pagination
                // =========================
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
        // GET BY SUPPLIER ID
        // =====================================================

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplierId(
            Guid supplierId,

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? endDate = null
        )
        {
            try
            {
                var result =
                    await PagedRekapAP(
                        page: 1,
                        perPage: 1,
                        search: null,
                        orderBy: "NamaSupplier",
                        sortDirection: "asc",
                        supplierId: supplierId,
                        startDate: startDate,
                        endDate: endDate
                    );

                return result;
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
        // CREATE MANUAL REKAP AP
        // Untuk menyimpan RekapVariasiHarga, RekapOther, Keterangan
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] RekapAPViewModel vm)
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

                var duplicate =
                    await _applicationDbContext.RekapAPs
                    .AnyAsync(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false);

                if (duplicate)
                {
                    return BadRequest(new
                    {
                        message = "Rekap AP untuk supplier ini sudah ada. Silakan gunakan update."
                    });
                }

                var data = new RekapAP
                {
                    RekapAPId = Guid.NewGuid(),
                    SupplierId = vm.SupplierId,
                    RekapVariasiHarga = vm.RekapVariasiHarga ?? 0,
                    RekapOther = vm.RekapOther ?? 0,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.RekapAPs.Add(data);

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
                            data.RekapAPId,
                            data.SupplierId
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
        // UPDATE MANUAL REKAP AP
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] RekapAPViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.RekapAPs
                    .FirstOrDefaultAsync(x =>
                        x.RekapAPId == id &&
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

                var duplicate =
                    await _applicationDbContext.RekapAPs
                    .AnyAsync(x =>
                        x.RekapAPId != id &&
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false);

                if (duplicate)
                {
                    return BadRequest(new
                    {
                        message = "Rekap AP untuk supplier ini sudah digunakan di data lain."
                    });
                }

                data.SupplierId = vm.SupplierId;
                data.RekapVariasiHarga = vm.RekapVariasiHarga ?? 0;
                data.RekapOther = vm.RekapOther ?? 0;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.RekapAPs.Update(data);

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
        // DELETE MANUAL REKAP AP
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.RekapAPs
                    .FirstOrDefaultAsync(x =>
                        x.RekapAPId == id &&
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

                _applicationDbContext.RekapAPs.Update(data);

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