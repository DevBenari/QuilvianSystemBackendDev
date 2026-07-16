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
    public class PajakRelasiController : ControllerBase
    {
        private const int MaxPerPage = 100;

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<PajakRelasiController> _logger;

        public PajakRelasiController(
            ApplicationDbContext applicationDbContext,
            ILogger<PajakRelasiController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? jenisRelasi = null,
            Guid? relasiId = null,
            Guid? pajakId = null,
            bool? isActive = null,
            string? orderBy = "TanggalMulai",
            string? sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(page, 1);
                perPage = Math.Clamp(perPage, 1, MaxPerPage);

                // JOIN hanya dilakukan lewat nilai PajakId, tanpa foreign key constraint.
                var query =
                    from relasi in _applicationDbContext.PajakRelasis.AsNoTracking()
                    join pajak in _applicationDbContext.Pajaks.AsNoTracking()
                        on relasi.PajakId equals pajak.PajakId
                    select new
                    {
                        relasi,
                        pajak
                    };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";
                    query = query.Where(x =>
                        EF.Functions.ILike(x.pajak.KodePajak, keyword) ||
                        EF.Functions.ILike(x.pajak.NamaPajak, keyword) ||
                        EF.Functions.ILike(x.relasi.JenisRelasi, keyword) ||
                        EF.Functions.ILike(x.relasi.JenisTransaksi ?? string.Empty, keyword) ||
                        EF.Functions.ILike(x.relasi.Keterangan ?? string.Empty, keyword));
                }

                if (!string.IsNullOrWhiteSpace(jenisRelasi))
                {
                    var normalizedJenis = jenisRelasi.Trim().ToUpperInvariant();
                    query = query.Where(x => x.relasi.JenisRelasi.ToUpper() == normalizedJenis);
                }

                if (relasiId.HasValue)
                {
                    query = query.Where(x => x.relasi.RelasiId == relasiId.Value);
                }

                if (pajakId.HasValue)
                {
                    query = query.Where(x => x.relasi.PajakId == pajakId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(x => x.relasi.IsActive == isActive.Value);
                }

                var descending = string.Equals(
                    sortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

                query = (orderBy?.Trim().ToLowerInvariant()) switch
                {
                    "kodepajak" => descending
                        ? query.OrderByDescending(x => x.pajak.KodePajak)
                        : query.OrderBy(x => x.pajak.KodePajak),
                    "jenispajak" => descending
                        ? query.OrderByDescending(x => x.pajak.JenisPajak)
                        : query.OrderBy(x => x.pajak.JenisPajak),
                    "jenisrelasi" => descending
                        ? query.OrderByDescending(x => x.relasi.JenisRelasi)
                        : query.OrderBy(x => x.relasi.JenisRelasi),
                    "tanggalberakhir" => descending
                        ? query.OrderByDescending(x => x.relasi.TanggalBerakhir)
                        : query.OrderBy(x => x.relasi.TanggalBerakhir),
                    _ => descending
                        ? query.OrderByDescending(x => x.relasi.TanggalMulai)
                        : query.OrderBy(x => x.relasi.TanggalMulai)
                };

                var totalRows = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(x => new
                    {
                        x.relasi.PajakRelasiId,
                        x.relasi.PajakId,
                        x.pajak.KodePajak,
                        x.pajak.NamaPajak,
                        x.pajak.JenisPajak,
                        x.pajak.TarifPersen,
                        x.relasi.JenisRelasi,
                        x.relasi.RelasiId,
                        x.relasi.JenisTransaksi,
                        x.relasi.TanggalMulai,
                        x.relasi.TanggalBerakhir,
                        x.relasi.IsActive,
                        x.relasi.Keterangan,
                        x.relasi.CreatedAt,
                        x.relasi.CreatedBy,
                        x.relasi.UpdatedAt,
                        x.relasi.UpdatedBy
                    })
                    .ToListAsync(cancellationToken);

                return Ok(new
                {
                    status = "success",
                    message = "Data relasi pajak berhasil diambil.",
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
                _logger.LogError(ex, "Gagal mengambil relasi pajak.");
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
                var data = await (
                    from relasi in _applicationDbContext.PajakRelasis.AsNoTracking()
                    join pajak in _applicationDbContext.Pajaks.AsNoTracking()
                        on relasi.PajakId equals pajak.PajakId
                    where relasi.PajakRelasiId == id
                    select new
                    {
                        relasi.PajakRelasiId,
                        relasi.PajakId,
                        pajak.KodePajak,
                        pajak.NamaPajak,
                        pajak.JenisPajak,
                        pajak.TarifPersen,
                        relasi.JenisRelasi,
                        relasi.RelasiId,
                        relasi.JenisTransaksi,
                        relasi.TanggalMulai,
                        relasi.TanggalBerakhir,
                        relasi.IsActive,
                        relasi.Keterangan,
                        relasi.CreatedAt,
                        relasi.CreatedBy,
                        relasi.UpdatedAt,
                        relasi.UpdatedBy
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return data == null
                    ? NotFound(new { message = "Data relasi pajak tidak ditemukan." })
                    : Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil relasi pajak {PajakRelasiId}.", id);
                return InternalServerError();
            }
        }

        [HttpGet("by-reference/{jenisRelasi}/{relasiId:guid}")]
        public async Task<IActionResult> GetByReference(
            string jenisRelasi,
            Guid relasiId,
            DateOnly? tanggalTransaksi = null,
            string? jenisTransaksi = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var normalizedJenisRelasi = jenisRelasi.Trim().ToUpperInvariant();
                var effectiveDate = tanggalTransaksi ?? DateOnly.FromDateTime(DateTime.UtcNow);

                var query =
                    from relasi in _applicationDbContext.PajakRelasis.AsNoTracking()
                    join pajak in _applicationDbContext.Pajaks.AsNoTracking()
                        on relasi.PajakId equals pajak.PajakId
                    where relasi.JenisRelasi == normalizedJenisRelasi
                          && relasi.RelasiId == relasiId
                          && relasi.IsActive
                          && pajak.IsActive
                          && relasi.TanggalMulai <= effectiveDate
                          && (!relasi.TanggalBerakhir.HasValue || relasi.TanggalBerakhir >= effectiveDate)
                    select new
                    {
                        relasi,
                        pajak
                    };

                if (!string.IsNullOrWhiteSpace(jenisTransaksi))
                {
                    var normalizedJenisTransaksi = jenisTransaksi.Trim().ToUpperInvariant();
                    query = query.Where(x =>
                        x.relasi.JenisTransaksi == null ||
                        x.relasi.JenisTransaksi.ToUpper() == normalizedJenisTransaksi);
                }

                var data = await query
                    .OrderBy(x => x.pajak.KodePajak)
                    .Select(x => new
                    {
                        x.relasi.PajakRelasiId,
                        x.relasi.PajakId,
                        x.pajak.KodePajak,
                        x.pajak.NamaPajak,
                        x.pajak.JenisPajak,
                        x.pajak.TarifPersen,
                        x.relasi.JenisRelasi,
                        x.relasi.RelasiId,
                        x.relasi.JenisTransaksi,
                        x.relasi.TanggalMulai,
                        x.relasi.TanggalBerakhir
                    })
                    .ToListAsync(cancellationToken);

                return Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Gagal mengambil pajak untuk {JenisRelasi} {RelasiId}.",
                    jenisRelasi,
                    relasiId);
                return InternalServerError();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] PajakRelasiViewModel vm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var pajakExists = await _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .AnyAsync(x => x.PajakId == vm.PajakId, cancellationToken);

                if (!pajakExists)
                {
                    return BadRequest(new
                    {
                        message = "PajakId tidak ditemukan pada master pajak."
                    });
                }

                var jenisRelasi = vm.JenisRelasi.Trim().ToUpperInvariant();
                var jenisTransaksi = NormalizeUpperNullable(vm.JenisTransaksi);

                var duplicate = await _applicationDbContext
                    .PajakRelasis
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.PajakId == vm.PajakId &&
                        x.JenisRelasi == jenisRelasi &&
                        x.RelasiId == vm.RelasiId &&
                        x.JenisTransaksi == jenisTransaksi &&
                        x.TanggalMulai == vm.TanggalMulai,
                        cancellationToken);

                if (duplicate)
                {
                    return Conflict(new
                    {
                        message = "Relasi pajak dengan periode awal yang sama sudah tersedia."
                    });
                }

                var data = new PajakRelasi
                {
                    PajakRelasiId = Guid.NewGuid(),
                    PajakId = vm.PajakId,
                    JenisRelasi = jenisRelasi,
                    RelasiId = vm.RelasiId,
                    JenisTransaksi = jenisTransaksi,
                    TanggalMulai = vm.TanggalMulai,
                    TanggalBerakhir = vm.TanggalBerakhir,
                    IsActive = vm.IsActive,
                    Keterangan = NormalizeNullable(vm.Keterangan),
                    CreatedAt = DateTime.UtcNow
                };

                _applicationDbContext.PajakRelasis.Add(data);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = data.PajakRelasiId },
                    new
                    {
                        message = "Tambah relasi pajak berhasil.",
                        data.PajakRelasiId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menambah relasi pajak.");
                return InternalServerError();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] PajakRelasiViewModel vm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var data = await _applicationDbContext
                    .PajakRelasis
                    .FirstOrDefaultAsync(
                        x => x.PajakRelasiId == id,
                        cancellationToken);

                if (data == null)
                {
                    return NotFound(new { message = "Data relasi pajak tidak ditemukan." });
                }

                var pajakExists = await _applicationDbContext
                    .Pajaks
                    .AsNoTracking()
                    .AnyAsync(x => x.PajakId == vm.PajakId, cancellationToken);

                if (!pajakExists)
                {
                    return BadRequest(new
                    {
                        message = "PajakId tidak ditemukan pada master pajak."
                    });
                }

                var jenisRelasi = vm.JenisRelasi.Trim().ToUpperInvariant();
                var jenisTransaksi = NormalizeUpperNullable(vm.JenisTransaksi);

                var duplicate = await _applicationDbContext
                    .PajakRelasis
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.PajakRelasiId != id &&
                        x.PajakId == vm.PajakId &&
                        x.JenisRelasi == jenisRelasi &&
                        x.RelasiId == vm.RelasiId &&
                        x.JenisTransaksi == jenisTransaksi &&
                        x.TanggalMulai == vm.TanggalMulai,
                        cancellationToken);

                if (duplicate)
                {
                    return Conflict(new
                    {
                        message = "Relasi pajak dengan periode awal yang sama sudah tersedia."
                    });
                }

                data.PajakId = vm.PajakId;
                data.JenisRelasi = jenisRelasi;
                data.RelasiId = vm.RelasiId;
                data.JenisTransaksi = jenisTransaksi;
                data.TanggalMulai = vm.TanggalMulai;
                data.TanggalBerakhir = vm.TanggalBerakhir;
                data.IsActive = vm.IsActive;
                data.Keterangan = NormalizeNullable(vm.Keterangan);
                data.UpdatedAt = DateTime.UtcNow;

                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return Ok(new { message = "Update relasi pajak berhasil." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui relasi pajak {PajakRelasiId}.", id);
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
                    .PajakRelasis
                    .FirstOrDefaultAsync(
                        x => x.PajakRelasiId == id,
                        cancellationToken);

                if (data == null)
                {
                    return NotFound(new { message = "Data relasi pajak tidak ditemukan." });
                }

                _applicationDbContext.PajakRelasis.Remove(data);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                return Ok(new { message = "Delete relasi pajak berhasil." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus relasi pajak {PajakRelasiId}.", id);
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

        private static string? NormalizeUpperNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }
    }
}
