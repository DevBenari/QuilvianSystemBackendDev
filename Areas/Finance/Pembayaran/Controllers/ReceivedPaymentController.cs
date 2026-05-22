using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ReceivedPaymentController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ReceivedPaymentController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReceivedPaymentController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ReceivedPaymentController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReceivedPayment(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from rp in _applicationDbContext.ReceivedPayments
                         join u in _applicationDbContext.UserActives
                         on rp.CreateBy equals u.UserActiveId
                         where rp.IsDelete == false
                         select new
                         {
                             rp.ReceivedPaymentId,
                             rp.BankId,
                             rp.NoInvoice,
                             rp.TotalReceived,
                             rp.TglPembayaran,
                             rp.SisaPembayaran,
                             rp.TotalTagihanPasien,
                             rp.PembayaranKe,
                             rp.IsCanceled,
                             rp.Keterangan,
                             rp.CreateDateTime,
                             CreateByName = u.FullName
                         }).OrderByDescending(x => x.CreateDateTime);

            int totalRows = query.Count();
            int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listData.Any())
            {
                return NotFound(new
                {
                    message = "Belum ada data atau halaman tidak ditemukan."
                });
            }

            return Ok(new
            {
                message = "Berhasil tampilkan data",
                data = listData,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceivedPaymentById(Guid id)
        {
            var data = await _applicationDbContext.ReceivedPayments
                .FirstOrDefaultAsync(x =>
                    x.ReceivedPaymentId == id &&
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
                message = "Data ditemukan",
                data = data
            });
        }


        [HttpPut("cancelReceivedPaymentId/{id}")]
        public async Task<IActionResult> CancelARDetail(
        Guid id,
        [FromBody] CancelViewModel vm)
        {
            try
            {
                var data = await _applicationDbContext.ReceivedPayments
                    .FirstOrDefaultAsync(x =>
                        x.ReceivedPaymentId == id &&
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

                // UPDATE ISCANCELED
                data.IsCanceled = vm.IsCanceled;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = getUserActive.UserActiveId;

                _applicationDbContext.ReceivedPayments.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Status cancel berhasil diupdate."
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

        [HttpPost]
        public async Task<IActionResult> CreateReceivedPayment([FromBody] ReceivedPayment model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Data tidak valid."
                    });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message = "User tidak terautentikasi."
                    });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                model.ReceivedPaymentId = Guid.NewGuid();
                model.CreateBy = getUserActive.UserActiveId;
                model.CreateDateTime = DateTime.UtcNow;
                model.IsDelete = false;

                _applicationDbContext.ReceivedPayments.Add(model);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil disimpan.",
                        data = model
                    });
                }

                return StatusCode(500, new
                {
                    message = "Data gagal disimpan."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceivedPayment(Guid id, [FromBody] ReceivedPayment model)
        {
            try
            {
                var data = await _applicationDbContext.ReceivedPayments
                    .FirstOrDefaultAsync(x =>
                        x.ReceivedPaymentId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                data.BankId = model.BankId;
                data.NoInvoice = model.NoInvoice;

                data.TotalReceived = model.TotalReceived;
                data.TglPembayaran = model.TglPembayaran;
                data.SisaPembayaran = model.SisaPembayaran;
                data.TotalTagihanPasien = model.TotalTagihanPasien;
                data.PembayaranKe = model.PembayaranKe;
                data.IsCanceled = model.IsCanceled;
                data.Keterangan = model.Keterangan;

                data.UpdateBy = getUserActive.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.ReceivedPayments.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil diupdate."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Data gagal diupdate."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceivedPayment(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ReceivedPayments
                    .FirstOrDefaultAsync(x =>
                        x.ReceivedPaymentId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                data.IsDelete = true;
                data.DeleteBy = getUserActive.UserActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                _applicationDbContext.ReceivedPayments.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil dihapus."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Data gagal dihapus."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedReceivedPayment(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            string? isCanceled = null,

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? endDate = null
        )
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                // DEFAULT TANGGAL HARI INI
                startDate ??= DateTime.UtcNow.Date;
                endDate ??= DateTime.UtcNow.Date;

                var query =
                    from rp in _applicationDbContext.ReceivedPayments

                    join u in _applicationDbContext.UserActives
                        on rp.CreateBy equals u.UserActiveId

                    where rp.IsDelete == false

                    select new
                    {
                        rp.ReceivedPaymentId,
                        rp.BankId,
                        rp.NoInvoice,

                        rp.TotalReceived,
                        rp.TglPembayaran,
                        rp.SisaPembayaran,
                        rp.TotalTagihanPasien,
                        rp.PembayaranKe,

                        rp.IsCanceled,
                        rp.Keterangan,

                        rp.CreateDateTime,

                        CreateByName = u.FullName
                    };

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.Keterangan ?? "", keyword)
                    );
                }

                // FILTER ISCANCELED
                if (!string.IsNullOrWhiteSpace(isCanceled))
                {
                    bool parsedIsCanceled =
                        bool.Parse(isCanceled);

                    query = query.Where(x =>
                        x.IsCanceled == parsedIsCanceled);
                }

                // FILTER DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc =
                        startDate.Value.Date.ToUniversalTime();

                    var endUtc =
                        endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                // SORTING
                var sortColumn =
                    orderBy?.ToLower() ?? "createdatetime";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "tglpembayaran" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglPembayaran)
                            : query.OrderBy(x => x.TglPembayaran),

                    "totalreceived" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalReceived)
                            : query.OrderBy(x => x.TotalReceived),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime)
                };

                // PAGINATION
                int totalRows =
                    await query.CountAsync();

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
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }

}
