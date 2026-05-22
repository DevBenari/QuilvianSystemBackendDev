using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class BankAccountController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<BankAccountController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BankAccountController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BankAccountController> logger,
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
        public async Task<IActionResult> GetAllBankAccount(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from ba in _applicationDbContext.BankAccounts
                         join u in _applicationDbContext.UserActives
                         on ba.CreateBy equals u.UserActiveId
                         where ba.IsDelete == false
                         select new
                         {
                             ba.BankAccountId,
                             ba.BankId,
                             ba.BankName,
                             ba.BankShortName,
                             ba.NoAccount,
                             ba.AccountName,
                             ba.CurrencyCode,
                             ba.Keterangan,
                             ba.CreateDateTime,
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
        public async Task<IActionResult> GetBankAccountById(Guid id)
        {
            var data = await _applicationDbContext.BankAccounts
                .FirstOrDefaultAsync(x =>
                    x.BankAccountId == id &&
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
        public async Task<IActionResult> CreateBankAccount([FromBody] BankAccount model)
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

                model.BankAccountId = Guid.NewGuid();
                model.CreateBy = getUserActive.UserActiveId;
                model.CreateDateTime = DateTime.UtcNow;
                model.IsDelete = false;

                _applicationDbContext.BankAccounts.Add(model);

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
        public async Task<IActionResult> UpdateBankAccount(Guid id, [FromBody] BankAccount model)
        {
            try
            {
                var data = await _applicationDbContext.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.BankAccountId == id &&
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
                data.BankName = model.BankName;
                data.BankShortName = model.BankShortName;
                data.NoAccount = model.NoAccount;
                data.AccountName = model.AccountName;
                data.CurrencyCode = model.CurrencyCode;
                data.Keterangan = model.Keterangan;

                data.UpdateBy = getUserActive.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.BankAccounts.Update(data);

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
        public async Task<IActionResult> DeleteBankAccount(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.BankAccountId == id &&
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

                _applicationDbContext.BankAccounts.Update(data);

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
        public async Task<IActionResult> PagedBankAccount(
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
                var query = from ba in _applicationDbContext.BankAccounts
                            join u in _applicationDbContext.UserActives
                            on ba.CreateBy equals u.UserActiveId
                            where ba.IsDelete == false
                            select new
                            {
                                ba.BankAccountId,
                                ba.BankId,
                                ba.BankName,
                                ba.BankShortName,
                                ba.NoAccount,
                                ba.AccountName,
                                ba.CurrencyCode,
                                ba.Keterangan,
                                ba.CreateDateTime,
                                CreateByName = u.FullName
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.BankName, search) ||
                        EF.Functions.ILike(x.BankShortName, search) ||
                        EF.Functions.ILike(x.AccountName, search) ||
                        EF.Functions.ILike(x.CurrencyCode, search) ||
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
                    "bankname" => isDescending
                        ? query.OrderByDescending(x => x.BankName)
                        : query.OrderBy(x => x.BankName),

                    "accountname" => isDescending
                        ? query.OrderByDescending(x => x.AccountName)
                        : query.OrderBy(x => x.AccountName),

                    "currencycode" => isDescending
                        ? query.OrderByDescending(x => x.CurrencyCode)
                        : query.OrderBy(x => x.CurrencyCode),

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
