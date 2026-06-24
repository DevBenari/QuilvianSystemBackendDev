using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Faktur.Models;
using QuilvianSystemBackendDev.Areas.Finance.Faktur.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class TukarFakturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TukarFakturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TukarFakturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TukarFakturController> logger,
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

            var getUserActive =
                await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x =>
                    x.Email == emailLogin);

            if (getUserActive == null)
                return null;

            return getUserActive.UserActiveId;
        }

        private async Task<string> GenerateNoTukarFakturAsync()
        {
            var prefix = $"IV{DateTime.Now:yy}"; // Contoh: IV26

            var lastNo =
                await _applicationDbContext.TukarFakturs
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.NoTukarFaktur != null &&
                    x.NoTukarFaktur.StartsWith(prefix))
                .OrderByDescending(x => x.NoTukarFaktur)
                .Select(x => x.NoTukarFaktur)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNo) && lastNo.Length > prefix.Length)
            {
                var numberPart = lastNo.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        // =====================================================
        // PAGED HEADER
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedTukarFaktur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglRegistrasi",
            decimal? ppn = null,
            string? sortDirection = "desc",
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

                var baseQuery =
                    _applicationDbContext.TukarFakturs
                        .AsNoTracking()
                        .Where(x => x.IsDelete == false);

                // =========================
                // Search
                // =========================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.NoTukarFaktur ?? "", search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search) ||

                        _applicationDbContext.Suppliers.Any(s =>
                            s.SupplierId == x.SupplierId &&
                            EF.Functions.ILike(s.SupplierName ?? "", search)
                        ) ||

                        _applicationDbContext.DetailTukarFakturs.Any(d =>
                            d.TukarFakturId == x.TukarFakturId &&
                            d.IsDelete == false &&
                            (
                                EF.Functions.ILike(d.KodePurchasingInvoice ?? "", search) ||
                                EF.Functions.ILike(d.NomorPO ?? "", search) ||
                                EF.Functions.ILike(d.NoInvoice ?? "", search) ||
                                EF.Functions.ILike(d.StatusInvoice ?? "", search) ||
                                EF.Functions.ILike(d.Keterangan ?? "", search)
                            )
                        )
                    );
                }

                // =========================
                // Filter Supplier
                // =========================
                if (supplierId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                // =========================
                // Filter PPN
                // Berdasarkan Supplier.PPN
                // Contoh: ppn=11, ppn=0, ppn=10
                // =========================
                if (ppn.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        _applicationDbContext.Suppliers.Any(s =>
                            s.SupplierId == x.SupplierId &&
                            s.PPN == ppn.Value
                        )
                    );
                }

                // =========================
                // Filter Tanggal
                // =========================
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
                        x.TglRegistrasi >= startUtc &&
                        x.TglRegistrasi <= endUtc);
                }

                // =========================
                // Select Data
                // =========================
                var query =
                    baseQuery.Select(x => new
                    {
                        x.TukarFakturId,
                        x.SupplierId,

                        Supplier =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == x.SupplierId)
                                .Select(s => new
                                {
                                    s.SupplierId,
                                    s.SupplierCode,
                                    s.SupplierName,
                                    s.ContactPerson,
                                    s.TermOfPayment,
                                    s.LeadTime,
                                    s.Address,
                                    s.City,
                                    s.PhoneNumber,
                                    s.Email,
                                    s.IsPKS,
                                    s.IsActive,
                                    s.BankId,
                                    s.NoRekening,
                                    s.AccountHolderName,
                                    s.IsFullPaid,
                                    s.IsBloodBankSupplier,
                                    s.PaymentMethod,
                                    s.PPN,
                                    s.Note
                                })
                                .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == x.SupplierId)
                                .Select(s => s.SupplierName)
                                .FirstOrDefault(),

                        PPN =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == x.SupplierId)
                                .Select(s => s.PPN)
                                .FirstOrDefault(),


                        MataUang =
                            (
                                from s in _applicationDbContext.Suppliers
                                join m in _applicationDbContext.MataUangs
                                    on s.MataUangId equals m.MataUangId
                                where s.SupplierId == x.SupplierId
                                select new
                                {
                                    m.MataUangId,
                                    m.SimbolMataUang,
                                    m.NamaMataUang
                                }
                            ).FirstOrDefault(),

                        x.NoTukarFaktur,
                        x.TglRegistrasi,
                        x.TglTerimaFaktur,
                        x.TglJatuhTempo,
                        x.TotalInvoiceGRN,
                        x.TotalInvoiceAP,
                        x.StatusTagihan,
                        x.Keterangan,

                        JumlahDetail =
                            _applicationDbContext.DetailTukarFakturs
                                .Count(d =>
                                    d.TukarFakturId == x.TukarFakturId &&
                                    d.IsDelete == false),

                        TotalInvoiceDetail =
                            _applicationDbContext.DetailTukarFakturs
                                .Where(d =>
                                    d.TukarFakturId == x.TukarFakturId &&
                                    d.IsDelete == false)
                                .Sum(d => (decimal?)d.NilaiPurchasingInvoice) ?? 0
                    });

                // =========================
                // Sorting
                // =========================
                var sortColumn =
                    orderBy?.ToLower() ?? "tglregistrasi";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "notukarfaktur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoTukarFaktur)
                            : query.OrderBy(x => x.NoTukarFaktur),

                    "supplierid" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SupplierId)
                            : query.OrderBy(x => x.SupplierId),

                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "ppn" =>
                        isDescending
                            ? query.OrderByDescending(x => x.PPN)
                            : query.OrderBy(x => x.PPN),

                    "tglregistrasi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglRegistrasi)
                            : query.OrderBy(x => x.TglRegistrasi),

                    "tglterimafaktur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglTerimaFaktur)
                            : query.OrderBy(x => x.TglTerimaFaktur),

                    "tgljatuhtempo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglJatuhTempo)
                            : query.OrderBy(x => x.TglJatuhTempo),

                    "totalinvoicegrn" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoiceGRN)
                            : query.OrderBy(x => x.TotalInvoiceGRN),

                    "totalinvoiceap" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoiceAP)
                            : query.OrderBy(x => x.TotalInvoiceAP),

                    "totalinvoicedetail" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoiceDetail)
                            : query.OrderBy(x => x.TotalInvoiceDetail),

                    _ =>
                        query.OrderByDescending(x => x.TglRegistrasi)
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
        // GET BY ID HEADER + DETAIL
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var header =
                    await _applicationDbContext.TukarFakturs
                    .AsNoTracking()
                    .Where(x =>
                        x.TukarFakturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.TukarFakturId,
                        x.SupplierId,

                        Supplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => new
                            {
                                s.SupplierId,
                                s.SupplierCode,
                                s.SupplierName,
                                s.ContactPerson,
                                s.TermOfPayment,
                                s.LeadTime,
                                s.Address,
                                s.City,
                                s.PhoneNumber,
                                s.Email,
                                s.IsPKS,
                                s.IsActive,
                                s.BankId,
                                s.NoRekening,
                                s.AccountHolderName,
                                s.IsFullPaid,
                                s.IsBloodBankSupplier,
                                s.PaymentMethod,
                                s.PPN,
                                s.Note
                            })
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        x.NoTukarFaktur,
                        x.TglRegistrasi,
                        x.TglTerimaFaktur,
                        x.TglJatuhTempo,
                        x.TotalInvoiceGRN,
                        x.TotalInvoiceAP,
                        x.Keterangan,

                        JumlahDetail =
                            _applicationDbContext.DetailTukarFakturs
                            .Count(d =>
                                d.TukarFakturId == x.TukarFakturId &&
                                d.IsDelete == false),

                        TotalInvoiceDetail =
                            _applicationDbContext.DetailTukarFakturs
                            .Where(d =>
                                d.TukarFakturId == x.TukarFakturId &&
                                d.IsDelete == false)
                            .Sum(d => (decimal?)d.NilaiPurchasingInvoice) ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var details =
                    await _applicationDbContext.DetailTukarFakturs
                    .AsNoTracking()
                    .Where(d =>
                        d.TukarFakturId == id &&
                        d.IsDelete == false)
                    .Select(d => new
                    {
                        d.DetailTukarFakturId,
                        d.TukarFakturId,

                        header.NoTukarFaktur,

                        d.TglPembuatanInvoice,
                        d.KodePurchasingInvoice,

                        d.POId,
                        d.SupplierId,

                        Supplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == d.SupplierId)
                            .Select(s => new
                            {
                                s.SupplierId,
                                s.SupplierCode,
                                s.SupplierName,
                                s.ContactPerson,
                                s.TermOfPayment,
                                s.LeadTime,
                                s.Address,
                                s.City,
                                s.PhoneNumber,
                                s.Email,
                                s.IsPKS,
                                s.IsActive,
                                s.BankId,
                                s.NoRekening,
                                s.AccountHolderName,
                                s.IsFullPaid,
                                s.IsBloodBankSupplier,
                                s.PaymentMethod,
                                s.PPN,
                                s.Note
                            })
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == d.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        d.NomorPO,
                        d.NoInvoice,
                        d.NilaiPurchasingInvoice,
                        header.TglJatuhTempo,
                        d.StatusInvoice,
                        d.Keterangan
                    })
                    .OrderByDescending(d => d.TglPembuatanInvoice)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        header.TukarFakturId,
                        header.SupplierId,
                        header.Supplier,
                        header.NamaSupplier,
                        header.NoTukarFaktur,
                        header.TglRegistrasi,
                        header.TglTerimaFaktur,
                        header.TglJatuhTempo,
                        header.TotalInvoiceGRN,
                        header.TotalInvoiceAP,
                        header.Keterangan,
                        header.JumlahDetail,
                        header.TotalInvoiceDetail,
                        Details = details
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
        // CREATE HEADER
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] TukarFakturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
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

                var supplier =
                    await _applicationDbContext.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.SupplierId,
                        x.SupplierCode,
                        x.SupplierName,
                        x.ContactPerson,
                        x.TermOfPayment,
                        x.LeadTime,
                        x.Address,
                        x.City,
                        x.PhoneNumber,
                        x.Email,
                        x.IsPKS,
                        x.IsActive,
                        x.BankId,
                        x.NoRekening,
                        x.AccountHolderName,
                        x.IsFullPaid,
                        x.IsBloodBankSupplier,
                        x.PaymentMethod,
                        x.PPN,
                        x.Note
                    })
                    .FirstOrDefaultAsync();

                if (supplier == null)
                {
                    return BadRequest(new
                    {
                        message = "Supplier tidak ditemukan."
                    });
                }

                var headerId =
                    Guid.NewGuid();

                var noTukarFaktur =
                    await GenerateNoTukarFakturAsync();

                var header = new TukarFaktur
                {
                    TukarFakturId = headerId,
                    SupplierId = vm.SupplierId,
                    NoTukarFaktur = noTukarFaktur,

                    TglRegistrasi = vm.TglRegistrasi,
                    TglTerimaFaktur = vm.TglTerimaFaktur,
                    TglJatuhTempo = vm.TglJatuhTempo,

                    TotalInvoiceGRN = vm.TotalInvoiceGRN,
                    TotalInvoiceAP = vm.TotalInvoiceAP,

                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.TukarFakturs.Add(header);

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
                            TukarFakturId = headerId,
                            NoTukarFaktur = noTukarFaktur,
                            Supplier = supplier
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
        // UPDATE HEADER
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] TukarFakturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data =
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == id &&
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

                data.SupplierId = vm.SupplierId;
                data.TglRegistrasi = vm.TglRegistrasi;
                data.TglTerimaFaktur = vm.TglTerimaFaktur;
                data.TglJatuhTempo = vm.TglJatuhTempo;

                data.TotalInvoiceGRN = vm.TotalInvoiceGRN;
                data.TotalInvoiceAP = vm.TotalInvoiceAP;

                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.TukarFakturs.Update(data);

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
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == id &&
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

                _applicationDbContext.TukarFakturs.Update(data);

                var details =
                    await _applicationDbContext.DetailTukarFakturs
                    .Where(x =>
                        x.TukarFakturId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDelete = true;
                    detail.DeleteDateTime = DateTime.UtcNow;
                    detail.DeleteBy = userActiveId.Value;

                    _applicationDbContext.DetailTukarFakturs.Update(detail);
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