using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pajak.Models;
using QuilvianSystemBackendDev.Areas.Finance.Pajak.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Finance.Pajak.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PajakController : ControllerBase
    {
        private const int MaxPerPage = 100;

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<PajakController> _logger;

        public PajakController(
            ApplicationDbContext applicationDbContext,
            ILogger<PajakController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? jenisPajak = null,
            bool? isActive = null,
            string? orderBy = "NamaPajak",
            string? sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(page, 1);
                perPage = Math.Clamp(perPage, 1, MaxPerPage);

                var query = _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePajak, keyword) ||
                        EF.Functions.ILike(x.NamaPajak, keyword) ||
                        EF.Functions.ILike(x.JenisPajak, keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? string.Empty, keyword));
                }

                if (!string.IsNullOrWhiteSpace(jenisPajak))
                {
                    var normalizedJenis = jenisPajak.Trim().ToUpperInvariant();
                    query = query.Where(x => x.JenisPajak.ToUpper() == normalizedJenis);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == isActive.Value);
                }

                var descending = string.Equals(
                    sortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

                query = (orderBy?.Trim().ToLowerInvariant()) switch
                {
                    "kodepajak" => descending
                        ? query.OrderByDescending(x => x.KodePajak)
                        : query.OrderBy(x => x.KodePajak),
                    "jenispajak" => descending
                        ? query.OrderByDescending(x => x.JenisPajak)
                        : query.OrderBy(x => x.JenisPajak),
                    "tarifpersen" => descending
                        ? query.OrderByDescending(x => x.TarifPersen)
                        : query.OrderBy(x => x.TarifPersen),
                    "createdat" => descending
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),
                    _ => descending
                        ? query.OrderByDescending(x => x.NamaPajak)
                        : query.OrderBy(x => x.NamaPajak)
                };

                var totalRows = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(x => new
                    {
                        x.PajakId,
                        x.KodePajak,
                        x.NamaPajak,
                        x.JenisPajak,
                        x.TarifPersen,
                        x.Keterangan,
                        x.IsActive,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.UpdatedAt,
                        x.UpdatedBy
                    })
                    .ToListAsync(cancellationToken);

                return Ok(new
                {
                    status = "success",
                    message = "Data pajak berhasil diambil.",
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
                _logger.LogError(ex, "Gagal mengambil daftar pajak.");
                return InternalServerError();
            }
        }

        [HttpGet("options")]
        public async Task<IActionResult> Options(
            string? jenisPajak = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .Where(x => x.IsActive);

                if (!string.IsNullOrWhiteSpace(jenisPajak))
                {
                    var normalizedJenis = jenisPajak.Trim().ToUpperInvariant();
                    query = query.Where(x => x.JenisPajak.ToUpper() == normalizedJenis);
                }

                var data = await query
                    .OrderBy(x => x.NamaPajak)
                    .Select(x => new
                    {
                        value = x.PajakId,
                        label = x.KodePajak + " - " + x.NamaPajak,
                        x.KodePajak,
                        x.NamaPajak,
                        x.JenisPajak,
                        x.TarifPersen
                    })
                    .ToListAsync(cancellationToken);

                return Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil opsi pajak.");
                return InternalServerError();
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .Where(x => x.PajakId == id)
                    .Select(x => new
                    {
                        x.PajakId,
                        x.KodePajak,
                        x.NamaPajak,
                        x.JenisPajak,
                        x.TarifPersen,
                        x.Keterangan,
                        x.IsActive,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.UpdatedAt,
                        x.UpdatedBy
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return data == null
                    ? NotFound(new { message = "Data pajak tidak ditemukan." })
                    : Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil pajak {PajakId}.", id);
                return InternalServerError();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] PajakViewModel vm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var kodePajak = vm.KodePajak.Trim().ToUpperInvariant();

                var exists = await _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.KodePajak.ToUpper() == kodePajak,
                        cancellationToken);

                if (exists)
                {
                    return Conflict(new
                    {
                        message = $"Kode pajak '{kodePajak}' sudah digunakan."
                    });
                }

                var data = new Models.Pajak
                {
                    PajakId = Guid.NewGuid(),
                    KodePajak = kodePajak,
                    NamaPajak = vm.NamaPajak.Trim(),
                    JenisPajak = vm.JenisPajak.Trim().ToUpperInvariant(),
                    TarifPersen = vm.TarifPersen,
                    Keterangan = NormalizeNullable(vm.Keterangan),
                    IsActive = vm.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _applicationDbContext.Pajaks.Add(data);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = data.PajakId },
                    new
                    {
                        message = "Tambah data pajak berhasil.",
                        data.PajakId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menambah pajak.");
                return InternalServerError();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] PajakViewModel vm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var data = await _applicationDbContext
                    .Pajaks
                    .FirstOrDefaultAsync(x => x.PajakId == id, cancellationToken);

                if (data == null)
                {
                    return NotFound(new { message = "Data pajak tidak ditemukan." });
                }

                var kodePajak = vm.KodePajak.Trim().ToUpperInvariant();

                var duplicate = await _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.PajakId != id && x.KodePajak.ToUpper() == kodePajak,
                        cancellationToken);

                if (duplicate)
                {
                    return Conflict(new
                    {
                        message = $"Kode pajak '{kodePajak}' sudah digunakan."
                    });
                }

                data.KodePajak = kodePajak;
                data.NamaPajak = vm.NamaPajak.Trim();
                data.JenisPajak = vm.JenisPajak.Trim().ToUpperInvariant();
                data.TarifPersen = vm.TarifPersen;
                data.Keterangan = NormalizeNullable(vm.Keterangan);
                data.IsActive = vm.IsActive;
                data.UpdatedAt = DateTime.UtcNow;

                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return Ok(new { message = "Update data pajak berhasil." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui pajak {PajakId}.", id);
                return InternalServerError();
            }
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] PajakStatusViewModel vm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _applicationDbContext
                    .Pajaks
                    .FirstOrDefaultAsync(x => x.PajakId == id, cancellationToken);

                if (data == null)
                {
                    return NotFound(new { message = "Data pajak tidak ditemukan." });
                }

                data.IsActive = vm.IsActive;
                data.UpdatedAt = DateTime.UtcNow;

                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    message = vm.IsActive
                        ? "Pajak berhasil diaktifkan."
                        : "Pajak berhasil dinonaktifkan."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui status pajak {PajakId}.", id);
                return InternalServerError();
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _applicationDbContext
                    .Pajaks
                    .FirstOrDefaultAsync(x => x.PajakId == id, cancellationToken);

                if (data == null)
                {
                    return NotFound(new { message = "Data pajak tidak ditemukan." });
                }

                var isUsed = await _applicationDbContext
                    .PajakRelasis
                    .AsNoTracking()
                    .AnyAsync(x => x.PajakId == id, cancellationToken);

                if (isUsed)
                {
                    return Conflict(new
                    {
                        message = "Pajak sudah memiliki relasi dan tidak dapat dihapus. Nonaktifkan pajak jika sudah tidak digunakan."
                    });
                }

                _applicationDbContext.Pajaks.Remove(data);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return Ok(new { message = "Delete data pajak berhasil." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus pajak {PajakId}.", id);
                return InternalServerError();
            }
        }

        private IActionResult InternalServerError()
        {
            return StatusCode(500, new
            {
                message = "Terjadi kesalahan pada server."
            });
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
