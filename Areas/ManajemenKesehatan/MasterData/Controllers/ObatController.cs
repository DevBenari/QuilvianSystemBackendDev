using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObatController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ObatController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatController> logger,
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
        public async Task<IActionResult> GetAllObat(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.Obats
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            ObatId = a.ObatId,
                            ObatCode = a.ObatCode,
                            ObatName = a.ObatName,
                            IsActive = a.IsActive,
                            Note = a.Note,
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
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
        public async Task<IActionResult> GetObatById(Guid id)
        {
            var obat = await _applicationDbContext.Obats.FindAsync(id);
            if (obat == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = obat
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateObat([FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                var lastCode = _applicationDbContext.Obats
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.CreateDateTime)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"OBT{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.ObatCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"OBT{setDateNow}0001";
                    }
                    else
                    {
                        var lastNumber = int.Parse(lastCode.ObatCode.Substring(9));
                        kode = $"OBT{setDateNow}{(lastNumber + 1).ToString("D4")}";
                    }
                }

                bool isDuplicate = _applicationDbContext.Obats
                    .Any(c => c.ObatCode == kode && c.ObatName.ToLower() == vm.ObatName.ToLower());

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                var data = new Obat
                {
                    ObatId = Guid.NewGuid(),
                    CreateDateTime = dateNow.Date,
                    CreateBy = userActiveId,
                    ObatCode = kode,
                    ObatName = vm.ObatName,
                    HargaJual = vm.HargaJual,
                    Stock = vm.Stock,
                    IsActive = vm.IsActive,
                    Note = vm.Note
                };

                _applicationDbContext.Obats.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObat(Guid id, [FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.Obats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                bool isDuplicate = await _applicationDbContext.Obats
                    .AnyAsync(c => c.ObatName.ToLower() == vm.ObatName.ToLower() && c.ObatId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                data.ObatName = vm.ObatName;
                data.HargaJual = vm.HargaJual;
                data.Stock = vm.Stock;
                data.IsActive = vm.IsActive;
                data.Note = vm.Note;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.Obats.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObat(Guid id)
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.Obats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Obats.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedObat(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var query = from a in _applicationDbContext.Obats
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                ObatId = a.ObatId,
                                ObatCode = a.ObatCode,
                                ObatName = a.ObatName,
                                IsActive = a.IsActive,
                                Note = a.Note,
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%";
                    query = query.Where(u =>
                        EF.Functions.ILike(u.ObatName, search) ||
                        EF.Functions.ILike(u.ObatCode, search)
                    );
                }

                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "createdatetime" => isDescending ? query.OrderByDescending(u => u.CreateDateTime) : query.OrderBy(u => u.CreateDateTime),
                    "obatcode" => isDescending ? query.OrderByDescending(u => u.ObatCode) : query.OrderBy(u => u.ObatCode),
                    "obatname" => isDescending ? query.OrderByDescending(u => u.ObatName) : query.OrderBy(u => u.ObatName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

                int totalRows = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

                if (rows.Count == 0 && page > totalPages)
                {
                    return NotFound(new { message = "Page not found." });
                }

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
