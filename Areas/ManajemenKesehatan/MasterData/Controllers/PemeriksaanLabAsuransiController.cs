using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PemeriksaanLabAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PemeriksaanLabAsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PemeriksaanLabAsuransiController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PemeriksaanLabAsuransiController> logger,
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

            var query = from a in _applicationDbContext.PemeriksaanLabAsuransis
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.PemeriksaanLabAsuransiId,
                            a.AsuransiId,
                            a.PemeriksaanLabId,
                            a.Diskon
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

        [HttpPost]
        public async Task<IActionResult> CreateTindakanAsuransi([FromBody] PemeriksaanLabAsuransiVM vm)
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
                bool isDuplicate = _applicationDbContext.PemeriksaanLabAsuransis
                    .Any(c => c.PemeriksaanLabId == vm.PemeriksaanLabId && c.AsuransiId == vm.AsuransiId && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new PemeriksaanLabAsuransi
                {
                    PemeriksaanLabAsuransiId = Guid.NewGuid(),
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    AsuransiId = vm.AsuransiId,
                    Diskon = vm.Diskon,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId
                };

                // **Simpan ke Database**
                _applicationDbContext.PemeriksaanLabAsuransis.Add(data);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPemeriksaanLabAsuransiById(Guid id)
        {
            var data = await _applicationDbContext.PemeriksaanLabAsuransis
                .Where(t => t.PemeriksaanLabId == id && !t.IsDelete)
                .ToListAsync();  // Mengambil semua data yang sesuai dalam bentuk list

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new { message = "Data ditemukan || 200 OK", data });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTindakanAsuransi(Guid id)
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
                var data = await _applicationDbContext.PemeriksaanLabAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PemeriksaanLabAsuransis.Update(data);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePemeriksaanLabAsuransi(Guid id, [FromBody] PemeriksaanLabAsuransiVM vm)
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

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // ======================================================
                // 1) Ambil data lama yang mau diupdate
                // ======================================================
                var data = await _applicationDbContext.PemeriksaanLabAsuransis
                    .FirstOrDefaultAsync(x => x.PemeriksaanLabAsuransiId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = $"Data relasi dengan ID {id} tidak ditemukan || 404 Not Found" });
                }

                // ======================================================
                // 2) Cek duplikasi (kecuali data yang sedang diupdate)
                // ======================================================
                bool isDuplicate = await _applicationDbContext.PemeriksaanLabAsuransis
                    .AnyAsync(c =>
                        c.PemeriksaanLabAsuransiId != id &&
                        c.PemeriksaanLabId == vm.PemeriksaanLabId &&
                        c.AsuransiId == vm.AsuransiId &&
                        c.IsDelete == false
                    );

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // ======================================================
                // 3) Update data
                // ======================================================
                data.PemeriksaanLabId = vm.PemeriksaanLabId;
                data.AsuransiId = vm.AsuransiId;
                data.Diskon = vm.Diskon;

                // audit update (jika ada fieldnya)
                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId;

                // ======================================================
                // 4) Save
                // ======================================================
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Relasi Berhasil || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diupdate." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


    }
}
