using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class MataUangController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<MataUangController> _logger;

        public MataUangController
        (
            ApplicationDbContext context,
            ILogger<MataUangController> logger
        )
        {
            _applicationDbContext = context;
            _logger = logger;
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

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedMataUang(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "KodeMataUang",
            string? sortDirection = "asc",
            bool? isBaseCurrency = null
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
                    _applicationDbContext.MataUangs
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodeMataUang ?? "", search) ||
                        EF.Functions.ILike(x.NamaMataUang ?? "", search) ||
                        EF.Functions.ILike(x.SimbolMataUang ?? "", search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                if (isBaseCurrency.HasValue)
                {
                    query = query.Where(x =>
                        x.IsBaseCurrency == isBaseCurrency.Value);
                }

                var sortColumn =
                    orderBy?.ToLower() ?? "kodematauang";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodematauang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeMataUang)
                            : query.OrderBy(x => x.KodeMataUang),

                    "namamatauang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaMataUang)
                            : query.OrderBy(x => x.NamaMataUang),

                    "simbolmatauang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SimbolMataUang)
                            : query.OrderBy(x => x.SimbolMataUang),

                    "isbasecurrency" =>
                        isDescending
                            ? query.OrderByDescending(x => x.IsBaseCurrency)
                            : query.OrderBy(x => x.IsBaseCurrency),

                    _ =>
                        query.OrderBy(x => x.KodeMataUang)
                };

                int totalRows =
                    await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                var rows =
                    await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(x => new
                    {
                        x.MataUangId,
                        x.KodeMataUang,
                        x.NamaMataUang,
                        x.SimbolMataUang,
                        x.IsBaseCurrency,
                        x.Keterangan
                    })
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
                var data =
                    await _applicationDbContext.MataUangs
                    .AsNoTracking()
                    .Where(x =>
                        x.MataUangId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.MataUangId,
                        x.KodeMataUang,
                        x.NamaMataUang,
                        x.SimbolMataUang,
                        x.IsBaseCurrency,
                        x.Keterangan
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
        // CREATE
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] MataUangRequest vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

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

                var kode =
                    vm.KodeMataUang.Trim().ToUpper();

                var exists =
                    await _applicationDbContext.MataUangs
                    .AnyAsync(x =>
                        x.IsDelete == false &&
                        x.KodeMataUang.ToLower() == kode.ToLower());

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Kode mata uang sudah digunakan."
                    });
                }

                if (vm.IsBaseCurrency)
                {
                    var oldBaseCurrencies =
                        await _applicationDbContext.MataUangs
                        .Where(x =>
                            x.IsDelete == false &&
                            x.IsBaseCurrency == true)
                        .ToListAsync();

                    foreach (var item in oldBaseCurrencies)
                    {
                        item.IsBaseCurrency = false;
                        item.UpdateDateTime = DateTime.UtcNow;
                        item.UpdateBy = userActiveId.Value;
                    }
                }

                var data = new MataUang
                {
                    MataUangId = Guid.NewGuid(),
                    KodeMataUang = kode,
                    NamaMataUang = vm.NamaMataUang,
                    SimbolMataUang = vm.SimbolMataUang,
                    IsBaseCurrency = vm.IsBaseCurrency,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.MataUangs.Add(data);

                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Created("", new
                {
                    message = "Tambah data berhasil.",
                    data = new
                    {
                        data.MataUangId,
                        data.KodeMataUang,
                        data.NamaMataUang,
                        data.SimbolMataUang,
                        data.IsBaseCurrency,
                        data.Keterangan
                    }
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
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] MataUangRequest vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.MataUangs
                    .FirstOrDefaultAsync(x =>
                        x.MataUangId == id &&
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

                var kode =
                    vm.KodeMataUang.Trim().ToUpper();

                var exists =
                    await _applicationDbContext.MataUangs
                    .AnyAsync(x =>
                        x.IsDelete == false &&
                        x.MataUangId != id &&
                        x.KodeMataUang.ToLower() == kode.ToLower());

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Kode mata uang sudah digunakan."
                    });
                }

                if (vm.IsBaseCurrency)
                {
                    var oldBaseCurrencies =
                        await _applicationDbContext.MataUangs
                        .Where(x =>
                            x.IsDelete == false &&
                            x.MataUangId != id &&
                            x.IsBaseCurrency == true)
                        .ToListAsync();

                    foreach (var item in oldBaseCurrencies)
                    {
                        item.IsBaseCurrency = false;
                        item.UpdateDateTime = DateTime.UtcNow;
                        item.UpdateBy = userActiveId.Value;
                    }
                }

                data.KodeMataUang = kode;
                data.NamaMataUang = vm.NamaMataUang;
                data.SimbolMataUang = vm.SimbolMataUang;
                data.IsBaseCurrency = vm.IsBaseCurrency;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.MataUangs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update data berhasil."
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
        // DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.MataUangs
                    .FirstOrDefaultAsync(x =>
                        x.MataUangId == id &&
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

                var usedInExchangeRate =
                    await _applicationDbContext.ExchangeRates
                    .AnyAsync(x =>
                        x.MataUangId == id &&
                        x.IsDelete == false);

                if (usedInExchangeRate)
                {
                    return BadRequest(new
                    {
                        message = "Mata uang tidak dapat dihapus karena sudah digunakan pada exchange rate."
                    });
                }

                data.IsDelete = true;
                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = userActiveId.Value;

                _applicationDbContext.MataUangs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Delete berhasil."
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

    public class MataUangRequest
    {
        public string KodeMataUang { get; set; } = string.Empty;
        public string NamaMataUang { get; set; } = string.Empty;
        public string? SimbolMataUang { get; set; }
        public bool IsBaseCurrency { get; set; }
        public string? Keterangan { get; set; }
    }
}
