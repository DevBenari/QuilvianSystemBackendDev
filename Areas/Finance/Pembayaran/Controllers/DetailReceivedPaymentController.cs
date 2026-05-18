using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DetailReceivedPaymentController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DetailReceivedPaymentController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DetailReceivedPaymentController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailReceivedPaymentController> logger,
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
        public async Task<IActionResult> GetAllDetailReceivedPayment(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from drp in _applicationDbContext.DetailReceivedPayments
                         join u in _applicationDbContext.UserActives
                         on drp.CreateBy equals u.UserActiveId
                         where drp.IsDelete == false
                         select new
                         {
                             drp.DetailReceivedPaymentId,
                             drp.ReceivedPaymentId,
                             drp.AsuransiId,
                             drp.NoInvoice,
                             drp.TotalInvoice,
                             drp.DueDate,
                             drp.IsCanceled,
                             drp.COADiskonId,
                             drp.NamaCOADiskon,
                             drp.PersenCOADiskon,
                             drp.COATambahanId,
                             drp.PPH23Per,
                             drp.PPH23Nom,
                             drp.NamaCoaTambahan,
                             drp.NominalTambahan,
                             drp.Keterangan,
                             drp.CreateDateTime,
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
        public async Task<IActionResult> GetDetailReceivedPaymentById(Guid id)
        {
            var data = await _applicationDbContext.DetailReceivedPayments
                .FirstOrDefaultAsync(x =>
                    x.DetailReceivedPaymentId == id &&
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

        [HttpPost]
        public async Task<IActionResult> CreateDetailReceivedPayment([FromBody] DetailReceivedPayment model)
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

                model.DetailReceivedPaymentId = Guid.NewGuid();
                model.CreateBy = getUserActive.UserActiveId;
                model.CreateDateTime = DateTime.UtcNow;
                model.IsDelete = false;

                _applicationDbContext.DetailReceivedPayments.Add(model);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil disimpan."
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
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDetailReceivedPayment(Guid id, [FromBody] DetailReceivedPayment model)
        {
            try
            {
                var data = await _applicationDbContext.DetailReceivedPayments
                    .FirstOrDefaultAsync(x =>
                        x.DetailReceivedPaymentId == id &&
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

                data.ReceivedPaymentId = model.ReceivedPaymentId;
                data.AsuransiId = model.AsuransiId;
                data.NoInvoice = model.NoInvoice;
                data.TotalInvoice = model.TotalInvoice;
                data.DueDate = model.DueDate;
                data.IsCanceled = model.IsCanceled;
                data.COADiskonId = model.COADiskonId;
                data.NamaCOADiskon = model.NamaCOADiskon;
                data.PersenCOADiskon = model.PersenCOADiskon;
                data.COATambahanId = model.COATambahanId;
                data.NamaCoaTambahan = model.NamaCoaTambahan;
                data.NominalTambahan = model.NominalTambahan;
                data.PPH23Per = model.PPH23Per;
                data.PPH23Nom = model.PPH23Nom;
                data.Keterangan = model.Keterangan;

                data.UpdateBy = getUserActive.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.DetailReceivedPayments.Update(data);

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
        public async Task<IActionResult> DeleteDetailReceivedPayment(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.DetailReceivedPayments
                    .FirstOrDefaultAsync(x =>
                        x.DetailReceivedPaymentId == id &&
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

                _applicationDbContext.DetailReceivedPayments.Update(data);

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
        public async Task<IActionResult> PagedDetailReceivedPayment(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? endDate = null)
        {
            try
            {
                var query = from drp in _applicationDbContext.DetailReceivedPayments
                            join u in _applicationDbContext.UserActives
                            on drp.CreateBy equals u.UserActiveId
                            where drp.IsDelete == false
                            select new
                            {
                                drp.DetailReceivedPaymentId,
                                drp.ReceivedPaymentId,
                                drp.AsuransiId,
                                drp.NoInvoice,
                                drp.TotalInvoice,
                                drp.DueDate,
                                drp.IsCanceled,
                                drp.COADiskonId,
                                drp.NamaCOADiskon,
                                drp.PersenCOADiskon,
                                drp.COATambahanId,
                                drp.NamaCoaTambahan,
                                drp.NominalTambahan,
                                drp.PPH23Nom,
                                drp.PPH23Per,
                                drp.Keterangan,
                                drp.CreateDateTime,
                                CreateByName = u.FullName
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoInvoice, search) ||
                        EF.Functions.ILike(x.NamaCOADiskon, search) ||
                        EF.Functions.ILike(x.NamaCoaTambahan, search) ||
                        EF.Functions.ILike(x.Keterangan, search));
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    var endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "noinvoice" => isDescending
                        ? query.OrderByDescending(x => x.NoInvoice)
                        : query.OrderBy(x => x.NoInvoice),

                    "totalinvoice" => isDescending
                        ? query.OrderByDescending(x => x.TotalInvoice)
                        : query.OrderBy(x => x.TotalInvoice),

                    "duedate" => isDescending
                        ? query.OrderByDescending(x => x.DueDate)
                        : query.OrderBy(x => x.DueDate),

                    _ => isDescending
                        ? query.OrderByDescending(x => x.CreateDateTime)
                        : query.OrderBy(x => x.CreateDateTime)
                };

                int totalRows = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

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
