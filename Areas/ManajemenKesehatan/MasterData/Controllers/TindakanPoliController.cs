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
    public class TindakanPoliController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TindakanPoliController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TindakanPoliController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanPoliController> logger,
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
        public async Task<IActionResult> GetAlL(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.TindakanPolis
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.TindakanPoliId,
                            a.PoliklinikId,
                            a.TindakanId
                        };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
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
        // POST: api/tindakanpoli
        [HttpPost]
        public async Task<IActionResult> CreateTindakanPoli([FromBody] TindakanPoliViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
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

                // **Cek Duplikasi**
                bool isDuplicate = _applicationDbContext.TindakanPolis
                    .Any(c => c.TindakanId == vm.TindakanId && c.PoliklinikId == vm.PoliId && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new TindakanPoli
                {
                    TindakanPoliId = Guid.NewGuid(),
                    TindakanId = vm.TindakanId,
                    PoliklinikId = vm.PoliId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId
                };

                // **Simpan ke Database**
                _applicationDbContext.TindakanPolis.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Relasi Berhasil || 201 Created" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // GET: api/tindakanpoli/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTindakanPoliById(Guid id)
        {
            // Mengambil semua relasi Tindakan yang sesuai dengan TindakanId
            var data = await _applicationDbContext.TindakanPolis
                .Where(t => t.TindakanId == id && !t.IsDelete)
                .ToListAsync();  // Mengambil semua data yang sesuai dalam bentuk list

            if (data == null || !data.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new { message = "Data ditemukan || 200 OK", data });
        }


        // DELETE: api/tindakanpoli/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTindakanPoli(Guid id)
        {
            try
            {
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

                // **Cari Data Relasi**
                var data = await _applicationDbContext.TindakanPolis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TindakanPolis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
