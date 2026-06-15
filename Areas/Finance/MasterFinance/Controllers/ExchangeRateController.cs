using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController
        (
            ApplicationDbContext context,
            ILogger<ExchangeRateController> logger
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
        public async Task<IActionResult> PagedExchangeRate(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "RateDate",
            string? sortDirection = "desc",
            Guid? mataUangId = null,

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

                var query =
                    from e in _applicationDbContext.ExchangeRates.AsNoTracking()
                    join m in _applicationDbContext.MataUangs.AsNoTracking()
                        on e.MataUangId equals m.MataUangId
                    where e.IsDelete == false &&
                          m.IsDelete == false
                    select new
                    {
                        e.ExchangeRateId,
                        e.MataUangId,
                        m.KodeMataUang,
                        m.NamaMataUang,
                        m.SimbolMataUang,
                        e.RateToIDR,
                        e.RateDate,
                        e.Keterangan
                    };

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

                if (mataUangId.HasValue)
                {
                    query = query.Where(x =>
                        x.MataUangId == mataUangId.Value);
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

                    query = query.Where(x =>
                        x.RateDate >= startUtc &&
                        x.RateDate <= endUtc);
                }

                var sortColumn =
                    orderBy?.ToLower() ?? "ratedate";

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

                    "ratetoidr" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RateToIDR)
                            : query.OrderBy(x => x.RateToIDR),

                    "ratedate" =>
                        isDescending
                            ? query.OrderByDescending(x => x.RateDate)
                            : query.OrderBy(x => x.RateDate),

                    _ =>
                        query.OrderByDescending(x => x.RateDate)
                };

                int totalRows =
                    await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

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
        // GET BY ID
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await
                    (
                        from e in _applicationDbContext.ExchangeRates.AsNoTracking()
                        join m in _applicationDbContext.MataUangs.AsNoTracking()
                            on e.MataUangId equals m.MataUangId
                        where e.ExchangeRateId == id &&
                              e.IsDelete == false &&
                              m.IsDelete == false
                        select new
                        {
                            e.ExchangeRateId,
                            e.MataUangId,
                            m.KodeMataUang,
                            m.NamaMataUang,
                            m.SimbolMataUang,
                            e.RateToIDR,
                            e.RateDate,
                            e.Keterangan
                        }
                    )
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
            [FromBody] ExchangeRateRequest vm)
        {
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

                var mataUang =
                    await _applicationDbContext.MataUangs
                    .AsNoTracking()
                    .Where(x =>
                        x.MataUangId == vm.MataUangId &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.MataUangId,
                        x.KodeMataUang,
                        x.NamaMataUang,
                        x.SimbolMataUang
                    })
                    .FirstOrDefaultAsync();

                if (mataUang == null)
                {
                    return BadRequest(new
                    {
                        message = "Mata uang tidak ditemukan."
                    });
                }

                if (vm.RateToIDR <= 0)
                {
                    return BadRequest(new
                    {
                        message = "RateToIDR harus lebih dari 0."
                    });
                }

                var data = new ExchangeRate
                {
                    ExchangeRateId = Guid.NewGuid(),
                    MataUangId = vm.MataUangId,
                    RateToIDR = vm.RateToIDR,
                    RateDate = vm.RateDate,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.ExchangeRates.Add(data);

                await _applicationDbContext.SaveChangesAsync();

                return Created("", new
                {
                    message = "Tambah data berhasil.",
                    data = new
                    {
                        data.ExchangeRateId,
                        data.MataUangId,
                        mataUang.KodeMataUang,
                        mataUang.NamaMataUang,
                        mataUang.SimbolMataUang,
                        data.RateToIDR,
                        data.RateDate,
                        data.Keterangan
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
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ExchangeRateRequest vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.ExchangeRates
                    .FirstOrDefaultAsync(x =>
                        x.ExchangeRateId == id &&
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

                var mataUangExists =
                    await _applicationDbContext.MataUangs
                    .AnyAsync(x =>
                        x.MataUangId == vm.MataUangId &&
                        x.IsDelete == false);

                if (!mataUangExists)
                {
                    return BadRequest(new
                    {
                        message = "Mata uang tidak ditemukan."
                    });
                }

                if (vm.RateToIDR <= 0)
                {
                    return BadRequest(new
                    {
                        message = "RateToIDR harus lebih dari 0."
                    });
                }

                data.MataUangId = vm.MataUangId;
                data.RateToIDR = vm.RateToIDR;
                data.RateDate = vm.RateDate;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.ExchangeRates.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Update data berhasil."
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ExchangeRates
                    .FirstOrDefaultAsync(x =>
                        x.ExchangeRateId == id &&
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

                _applicationDbContext.ExchangeRates.Update(data);

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

    public class ExchangeRateRequest
    {
        public Guid MataUangId { get; set; }

        public decimal RateToIDR { get; set; }

        public DateTime RateDate { get; set; }

        public string? Keterangan { get; set; }
    }
}
