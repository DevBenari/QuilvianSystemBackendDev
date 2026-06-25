using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AP.Models;
using QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels;
using QuilvianSystemBackendDev.Migrations;
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
    public class PembayaranAPController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PembayaranAPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PembayaranAPController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PembayaranAPController> logger,
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

        private async Task<string> GenerateKodePembayaranAP()
        {
            var year = DateTime.UtcNow.Year % 100;
            var prefix = $"APPAY-{year:00}-";

            var lastKode = await _applicationDbContext.PembayaranAPs
                .AsNoTracking()
                .Where(x =>
                    x.KodePembayaranAP != null &&
                    x.KodePembayaranAP.StartsWith(prefix))
                .OrderByDescending(x => x.KodePembayaranAP)
                .Select(x => x.KodePembayaranAP)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastKode))
            {
                var numberText = lastKode.Substring(prefix.Length);

                if (int.TryParse(numberText, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:000000}";
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedPembayaranAP(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? supplierId = null,
            Guid? bankId = null,
            string? tipePembayaran = null,
            string? statusPembayaran = null,
            string? kodePembayaranAP = null,
            string? noReferensi = null,

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
                    from ap in _applicationDbContext.PembayaranAPs.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ap.CreateBy equals u.UserActiveId into userJoin

                    from u in userJoin.DefaultIfEmpty()

                    where ap.IsDelete == false

                    select new
                    {
                        ap.PembayaranAPId,
                        ap.KodePembayaranAP,
                        ap.NoReferensi,
                        ap.TotalTagihan,
                        ap.SupplierId,

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == ap.SupplierId)
                                .Select(s => s.SupplierName)
                                .FirstOrDefault(),
                        NoRekekening =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == ap.SupplierId)
                                .Select(s => s.NoRekening)
                                .FirstOrDefault(),

                        NoTukarFaktur =
                            (
                                from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()
                                join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                                    on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId
                                join dtf in _applicationDbContext.DetailTukarFakturs.AsNoTracking()
                                    on pi.POId equals dtf.POId
                                join tf in _applicationDbContext.TukarFakturs.AsNoTracking()
                                    on dtf.TukarFakturId equals tf.TukarFakturId
                                where det.PembayaranAPId == ap.PembayaranAPId &&
                                      det.IsDelete == false &&
                                      pi.IsDelete == false &&
                                      dtf.IsDelete == false &&
                                      tf.IsDelete == false
                                select tf.NoTukarFaktur
                            ).FirstOrDefault(),

                        ap.TglPembayaranAP,
                        ap.BankId,

                        BankPBF =
                            (
                                from s in _applicationDbContext.Suppliers
                                join m in _applicationDbContext.MasterBanks
                                    on s.BankId equals m.BankId
                                select new
                                {
                                    m.BankName
                                }
                            ).FirstOrDefault(),

                        NamaBank =
                            _applicationDbContext.MasterBanks
                                .Where(b => b.BankId == ap.BankId)
                                .Select(b => b.BankName)
                                .FirstOrDefault(),

                        TglJatuhTempo =
                            (
                                from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()
                                join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                                    on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId
                                where det.PembayaranAPId == ap.PembayaranAPId &&
                                      det.IsDelete == false &&
                                      pi.IsDelete == false
                                select pi.TglJatuhTempo
                            ).FirstOrDefault(),

                        ap.TipePembayaran,
                        ap.StatusPembayaran,
                        ap.Potongan,
                        ap.Keterangan,
                        ap.CreateDateTime,

                        CreateByName = u != null ? u.FullName : null,

                        JumlahDetail =
                            _applicationDbContext.DetailPembayaranAPs
                                .Count(d =>
                                    d.PembayaranAPId == ap.PembayaranAPId &&
                                    d.IsDelete == false),

                        TotalPembayaranTagihan =
                            _applicationDbContext.DetailPembayaranAPs
                                .Where(d =>
                                    d.PembayaranAPId == ap.PembayaranAPId &&
                                    d.IsDelete == false)
                                .Sum(d => (decimal?)d.PembayaranTagihan) ?? 0
                    };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePembayaranAP ?? "", pattern) ||
                        EF.Functions.ILike(x.NoReferensi ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaSupplier ?? "", pattern) ||
                        EF.Functions.ILike(x.NoTukarFaktur ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaBank ?? "", pattern) ||
                        EF.Functions.ILike(x.TipePembayaran ?? "", pattern) ||
                        EF.Functions.ILike(x.StatusPembayaran ?? "", pattern) ||
                        EF.Functions.ILike(x.Keterangan ?? "", pattern)
                    );
                }

                if (!string.IsNullOrWhiteSpace(kodePembayaranAP))
                {
                    var pattern = $"%{kodePembayaranAP.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePembayaranAP ?? "", pattern));
                }

                if (!string.IsNullOrWhiteSpace(noReferensi))
                {
                    var pattern = $"%{noReferensi.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoReferensi ?? "", pattern));
                }

                if (supplierId.HasValue)
                {
                    query = query.Where(x => x.SupplierId == supplierId.Value);
                }

                if (bankId.HasValue)
                {
                    query = query.Where(x => x.BankId == bankId.Value);
                }

                if (!string.IsNullOrWhiteSpace(tipePembayaran))
                {
                    query = query.Where(x =>
                        x.TipePembayaran != null &&
                        x.TipePembayaran.ToLower() == tipePembayaran.ToLower());
                }

                if (!string.IsNullOrWhiteSpace(statusPembayaran))
                {
                    query = query.Where(x =>
                        x.StatusPembayaran != null &&
                        x.StatusPembayaran.ToLower() == statusPembayaran.ToLower());
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.TglPembayaranAP >= startUtc &&
                        x.TglPembayaranAP <= endUtc);
                }

                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodepembayaranap" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePembayaranAP)
                            : query.OrderBy(x => x.KodePembayaranAP),

                    "noreferensi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoReferensi)
                            : query.OrderBy(x => x.NoReferensi),

                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "namabank" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaBank)
                            : query.OrderBy(x => x.NamaBank),

                    "tglpembayaranap" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglPembayaranAP)
                            : query.OrderBy(x => x.TglPembayaranAP),

                    "tgljatuhtempo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglJatuhTempo)
                            : query.OrderBy(x => x.TglJatuhTempo),

                    "totaltagihan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalTagihan)
                            : query.OrderBy(x => x.TotalTagihan),

                    "statuspembayaran" =>
                        isDescending
                            ? query.OrderByDescending(x => x.StatusPembayaran)
                            : query.OrderBy(x => x.StatusPembayaran),

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
                var header = await _applicationDbContext.PembayaranAPs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.PembayaranAPId == id &&
                        x.IsDelete == false);

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var data = new PembayaranAPViewModel
                {
                    PembayaranAPId = header.PembayaranAPId,
                    KodePembayaranAP = header.KodePembayaranAP,
                    NoReferensi = header.NoReferensi,
                    TotalTagihan = header.TotalTagihan,
                    SupplierId = header.SupplierId,

                    NamaSupplier = await _applicationDbContext.Suppliers
                        .Where(s => s.SupplierId == header.SupplierId)
                        .Select(s => s.SupplierName)
                        .FirstOrDefaultAsync(),

                    NoRekening = await _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == header.SupplierId)
                                .Select(s => s.NoRekening)
                                .FirstOrDefaultAsync(),

                    TglPembayaranAP = header.TglPembayaranAP,
                    BankId = header.BankId,

                    NamaBank = await _applicationDbContext.MasterBanks
                        .Where(b => b.BankId == header.BankId)
                        .Select(b => b.BankName)
                        .FirstOrDefaultAsync(),

                    TipePembayaran = header.TipePembayaran,
                    StatusPembayaran = header.StatusPembayaran,
                    Potongan = header.Potongan,
                    Keterangan = header.Keterangan
                };

                data.Details = await
                    (
                        from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()
                        join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                            on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId into piJoin
                        from pi in piJoin.DefaultIfEmpty()

                        where det.PembayaranAPId == id &&
                              det.IsDelete == false

                        select new DetailPembayaranAPViewModel
                        {
                            DetailPembayaranAPId = det.DetailPembayaranAPId,
                            PembayaranAPId = det.PembayaranAPId,
                            PurchasingInvoiceId = det.PurchasingInvoiceId,

                            KodePurchasingInvoice =
                                pi == null ? null :
                                _applicationDbContext.DetailTukarFakturs
                                    .Where(dtf =>
                                        dtf.POId == pi.POId &&
                                        dtf.IsDelete == false)
                                    .Select(dtf => dtf.KodePurchasingInvoice)
                                    .FirstOrDefault(),

                            TglPembuatanInvoice = pi == null ? null : pi.TglPembuatanInvoice,
                            NoInvoice = pi == null ? null : pi.NoInvoice,

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

                            TotalTagihan = pi == null ? null : pi.POAmount,
                            SisaTagihan = det.SisaTagihan,
                            PembayaranTagihan = det.PembayaranTagihan,
                            Keterangan = det.Keterangan
                        }
                    ).ToListAsync();

                data.NoTukarFaktur = data.Details
                    .Select(x => x.NoTukarFaktur)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                data.TglJatuhTempo = await
                    (
                        from det in _applicationDbContext.DetailPembayaranAPs.AsNoTracking()
                        join pi in _applicationDbContext.PurchasingInvoices.AsNoTracking()
                            on det.PurchasingInvoiceId equals pi.PurchasingInvoiceId
                        where det.PembayaranAPId == id &&
                              det.IsDelete == false &&
                              pi.IsDelete == false
                        select pi.TglJatuhTempo
                    ).FirstOrDefaultAsync();

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
        public async Task<IActionResult> Create([FromBody] PembayaranAPViewModel vm)
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

                var pembayaranAPId = Guid.NewGuid();

                var data = new PembayaranAP
                {
                    PembayaranAPId = pembayaranAPId,
                    KodePembayaranAP = await GenerateKodePembayaranAP(),
                    NoReferensi = vm.NoReferensi,
                    TotalTagihan = vm.TotalTagihan,
                    SupplierId = vm.SupplierId,
                    TglPembayaranAP = vm.TglPembayaranAP,
                    BankId = vm.BankId,
                    TipePembayaran = vm.TipePembayaran,
                    StatusPembayaran = vm.StatusPembayaran,
                    Potongan = vm.Potongan,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.PembayaranAPs.Add(data);

                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var item in vm.Details)
                    {
                        var detail = new DetailPembayaranAP
                        {
                            DetailPembayaranAPId = Guid.NewGuid(),
                            PembayaranAPId = pembayaranAPId,
                            PurchasingInvoiceId = item.PurchasingInvoiceId,
                            SisaTagihan = item.SisaTagihan,
                            PembayaranTagihan = item.PembayaranTagihan,
                            Keterangan = item.Keterangan,

                            CreateDateTime = DateTime.UtcNow,
                            CreateBy = userActiveId.Value,
                            IsDelete = false
                        };

                        _applicationDbContext.DetailPembayaranAPs.Add(detail);
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.PembayaranAPId,
                            data.KodePembayaranAP
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
            [FromBody] PembayaranAPViewModel vm)
        {
            try
            {
                var data = await _applicationDbContext.PembayaranAPs
                    .FirstOrDefaultAsync(x =>
                        x.PembayaranAPId == id &&
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

                data.NoReferensi = vm.NoReferensi;
                data.TotalTagihan = vm.TotalTagihan;
                data.SupplierId = vm.SupplierId;
                data.TglPembayaranAP = vm.TglPembayaranAP;
                data.BankId = vm.BankId;
                data.TipePembayaran = vm.TipePembayaran;
                data.StatusPembayaran = vm.StatusPembayaran;
                data.Potongan = vm.Potongan;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.PembayaranAPs.Update(data);

                var oldDetails = await _applicationDbContext.DetailPembayaranAPs
                    .Where(x =>
                        x.PembayaranAPId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var oldDetail in oldDetails)
                {
                    oldDetail.IsDelete = true;
                    oldDetail.DeleteDateTime = DateTime.UtcNow;
                    oldDetail.DeleteBy = userActiveId.Value;
                }

                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var item in vm.Details)
                    {
                        var detail = new DetailPembayaranAP
                        {
                            DetailPembayaranAPId = Guid.NewGuid(),
                            PembayaranAPId = id,
                            PurchasingInvoiceId = item.PurchasingInvoiceId,
                            SisaTagihan = item.SisaTagihan,
                            PembayaranTagihan = item.PembayaranTagihan,
                            Keterangan = item.Keterangan,

                            CreateDateTime = DateTime.UtcNow,
                            CreateBy = userActiveId.Value,
                            IsDelete = false
                        };

                        _applicationDbContext.DetailPembayaranAPs.Add(detail);
                    }
                }

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
                var data = await _applicationDbContext.PembayaranAPs
                    .FirstOrDefaultAsync(x =>
                        x.PembayaranAPId == id &&
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

                _applicationDbContext.PembayaranAPs.Update(data);

                var details = await _applicationDbContext.DetailPembayaranAPs
                    .Where(x =>
                        x.PembayaranAPId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDelete = true;
                    detail.DeleteDateTime = DateTime.UtcNow;
                    detail.DeleteBy = userActiveId.Value;
                }

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