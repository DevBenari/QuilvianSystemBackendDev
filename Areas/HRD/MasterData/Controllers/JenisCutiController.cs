using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class JenisCutiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<JenisCutiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JenisCutiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<JenisCutiController> logger,
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
        public async Task<IActionResult> GetAllJenisCuti(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from a in _applicationDbContext.JenisCutis
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             JenisCutiId = a.JenisCutiId,
                             NamaCuti = a.NamaCuti,
                             KuotaTahunan = a.KuotaTahunan,
                             Keterangan = a.Keterangan
                         }).OrderByDescending(a => a.CreateDateTime);

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var listdata = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (!listdata.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

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
        public async Task<IActionResult> GetJenisCutiById(Guid id)
        {
            var listdata = await _applicationDbContext.JenisCutis.FindAsync(id);
            if (listdata == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data = listdata });
        }

        [HttpPost]
        public async Task<IActionResult> CreateJenisCuti([FromBody] JenisCuti vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;
            var dateNow = DateTime.UtcNow;

            // Cek duplikasi
            bool isDuplicate = _applicationDbContext.JenisCutis
                .Any(c => c.NamaCuti.ToLower() == vm.NamaCuti.ToLower() && c.IsDelete == false);

            if (isDuplicate)
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });

            var data = new JenisCuti
            {
                JenisCutiId = Guid.NewGuid(),
                CreateDateTime = dateNow,
                CreateBy = userActiveId,
                NamaCuti = vm.NamaCuti,
                KuotaTahunan = vm.KuotaTahunan,
                Keterangan = vm.Keterangan
            };

            _applicationDbContext.JenisCutis.Add(data);
            int result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });

            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJenisCuti(Guid id, [FromBody] JenisCuti vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!await _applicationDbContext.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;
            var data = await _applicationDbContext.JenisCutis.FindAsync(id);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            bool isDuplicate = await _applicationDbContext.JenisCutis
                .AnyAsync(c => c.NamaCuti.ToLower() == vm.NamaCuti.ToLower() && c.JenisCutiId != id);

            if (isDuplicate)
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });

            data.NamaCuti = vm.NamaCuti;
            data.KuotaTahunan = vm.KuotaTahunan;
            data.Keterangan = vm.Keterangan;
            data.UpdateBy = userActiveId;
            data.UpdateDateTime = DateTime.UtcNow;

            _applicationDbContext.JenisCutis.Update(data);
            int result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Update Data Berhasil || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJenisCuti(Guid id)
        {
            if (!await _applicationDbContext.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;
            var data = await _applicationDbContext.JenisCutis.FindAsync(id);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            data.DeleteBy = userActiveId;
            data.DeleteDateTime = DateTime.UtcNow;
            data.IsDelete = true;

            _applicationDbContext.JenisCutis.Update(data);
            int result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }
    }
}
