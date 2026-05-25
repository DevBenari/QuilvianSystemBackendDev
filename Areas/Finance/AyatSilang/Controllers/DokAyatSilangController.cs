using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokAyatSilangController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DokAyatSilangController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DokAyatSilangController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DokAyatSilangController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data =
                    await (
                        from dok in _applicationDbContext.DokAyatSilangs.AsNoTracking()

                        join ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                            on dok.AyatSilangId equals ayat.AyatSilangId

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                            on dok.CreateBy equals u.UserActiveId

                        where dok.IsDelete == false

                        orderby dok.CreateDateTime descending

                        select new
                        {
                            dok.DokAyatSilangId,

                            dok.AyatSilangId,
                            ayat.NoAyatSilang,

                            dok.NamaDokumen,

                            dok.TglPenyimpanan,

                            dok.Keterangan,

                            dok.CreateDateTime,
                            dok.CreateBy,

                            CreateByName = u.FullName
                        }
                    ).ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",
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

        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.DokAyatSilangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.DokAyatSilangId == id &&
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

        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [RequestSizeLimit(50_000_000)] // 50 MB
        public async Task<IActionResult> Create(
            [FromForm] DokAyatSilang model)
        {
            try
            {
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

                // =========================================================
                // VALIDASI FILE
                // =========================================================

                if (model.FileAyatSilang == null ||
                    model.FileAyatSilang.Length == 0)
                {
                    return BadRequest(new
                    {
                        message = "File wajib diupload."
                    });
                }

                // =========================================================
                // CEK AYAT SILANG
                // =========================================================

                var cekAyatSilang =
                    await _applicationDbContext.AyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.AyatSilangId == model.AyatSilangId &&
                        x.IsDelete == false);

                if (cekAyatSilang == null)
                {
                    return NotFound(new
                    {
                        message = "Data Ayat Silang tidak ditemukan."
                    });
                }

                // =========================================================
                // PATH FOLDER
                // =========================================================

                string folderPath =
                    Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Upload",
                        "DokAyatSilang"
                    );

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // =========================================================
                // GENERATE FILE NAME
                // =========================================================

                string extension =
                    Path.GetExtension(model.FileAyatSilang.FileName);

                string fileName =
                    $"{Guid.NewGuid()}{extension}";

                string fullPath =
                    Path.Combine(folderPath, fileName);

                // =========================================================
                // SAVE FILE
                // =========================================================

                using (var stream =
                    new FileStream(fullPath, FileMode.Create))
                {
                    await model.FileAyatSilang.CopyToAsync(stream);
                }

                // =========================================================
                // SAVE DATABASE
                // =========================================================

                var data = new DokAyatSilang
                {
                    DokAyatSilangId = Guid.NewGuid(),

                    AyatSilangId = model.AyatSilangId,

                    NamaDokumen = fileName,

                    TglPenyimpanan = DateTime.UtcNow,

                    Keterangan = model.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,

                    IsDelete = false
                };

                _applicationDbContext.DokAyatSilangs.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Upload dokumen berhasil.",
                        fileName
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
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =========================================================
        // DOWNLOAD FILE
        // =========================================================

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.DokAyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.DokAyatSilangId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Dokumen tidak ditemukan."
                    });
                }

                string folderPath =
                    Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Upload",
                        "DokAyatSilang"
                    );

                string fullPath =
                    Path.Combine(folderPath, data.NamaDokumen);

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new
                    {
                        message = "File fisik tidak ditemukan."
                    });
                }

                byte[] fileBytes =
                    await System.IO.File.ReadAllBytesAsync(fullPath);

                string contentType =
                    "application/octet-stream";

                return File(
                    fileBytes,
                    contentType,
                    data.NamaDokumen
                );
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

        // =========================================================
        // DELETE
        // =========================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.DokAyatSilangs
                    .FirstOrDefaultAsync(x =>
                        x.DokAyatSilangId == id &&
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

                // =========================================================
                // DELETE FILE FISIK
                // =========================================================

                string folderPath =
                    Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Upload",
                        "DokAyatSilang"
                    );

                string fullPath =
                    Path.Combine(folderPath, data.NamaDokumen);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // =========================================================
                // SOFT DELETE
                // =========================================================

                data.IsDelete = true;

                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = getUserActive.UserActiveId;

                _applicationDbContext.DokAyatSilangs.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

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
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,

            string? noAyatSilang = null,
            string? namaDokumen = null,
            string? search = null,

            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc"
        )
        {
            try
            {
                var query =
                    from dok in _applicationDbContext.DokAyatSilangs.AsNoTracking()

                    join ayat in _applicationDbContext.AyatSilangs.AsNoTracking()
                        on dok.AyatSilangId equals ayat.AyatSilangId

                    where dok.IsDelete == false

                    select new
                    {
                        dok.DokAyatSilangId,

                        dok.AyatSilangId,
                        ayat.NoAyatSilang,

                        dok.NamaDokumen,

                        dok.TglPenyimpanan,

                        dok.Keterangan,

                        dok.CreateDateTime
                    };

                if (!string.IsNullOrWhiteSpace(noAyatSilang))
                {
                    string keyword = $"%{noAyatSilang.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword));
                }

                if (!string.IsNullOrWhiteSpace(namaDokumen))
                {
                    string keyword = $"%{namaDokumen.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NamaDokumen, keyword));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoAyatSilang, keyword) ||
                        EF.Functions.ILike(x.NamaDokumen, keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword));
                }

                bool isDescending =
                    sortDirection?.ToLower() == "desc";

                query = orderBy?.ToLower() switch
                {
                    "namaDokumen" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaDokumen)
                            : query.OrderBy(x => x.NamaDokumen),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime)
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
    }
}