using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TindakanAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TindakanAsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TindakanAsuransiController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanAsuransiController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        // Tambah getsdfd
        [HttpGet]
        public async Task<IActionResult> GetAlL(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.TindakanAsuransis
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.TindakanAsuransiId,
                            a.AsuransiId,
                            a.TindakanId,
                            // ============================
                            // MARKUP
                            // ============================
                            a.MarkupDokter,
                            a.MarkupRs,
                            a.MarkupJp,
                            a.MarkupBahp,
                            a.MarkupLainnya,
                            a.MarkupTotal,
                            a.IsMarkupBerlaku,
                            a.MarkupDari,
                            a.MarkupSampai,

                            // ============================
                            // DISKON
                            // ============================
                            a.DiskonDokter,
                            a.DiskonRs,
                            a.DiskonJp,
                            a.DiskonBahp,
                            a.DiskonTotal,
                            a.IsDiskonBerlaku,
                            a.DiskonDari,
                            a.DiskonSampai
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
        // POST: api/tindakanasuransi
        [HttpPost]
        public async Task<IActionResult> CreateTindakanAsuransi([FromBody] TindakanAsuransiViewModel vm)
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
                bool isDuplicate = _applicationDbContext.TindakanAsuransis
                    .Any(c => c.TindakanId == vm.TindakanId && c.AsuransiId == vm.AsuransiId && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new TindakanAsuransi
                {
                    TindakanAsuransiId = Guid.NewGuid(),
                    TindakanId = vm.TindakanId,
                    AsuransiId = vm.AsuransiId,
                    // ============================
                    // MARKUP
                    // ============================
                    MarkupDokter = vm.MarkupDokter,
                    MarkupRs = vm.MarkupRs,
                    MarkupJp = vm.MarkupJp,
                    MarkupBahp = vm.MarkupBahp,
                    MarkupLainnya = vm.MarkupLainnya,
                    MarkupTotal = vm.MarkupTotal,

                    IsMarkupBerlaku = vm.IsMarkupBerlaku,
                    MarkupDari = vm.MarkupDari,
                    MarkupSampai = vm.MarkupSampai,

                    // ============================
                    // DISKON
                    // ============================
                    DiskonDokter = vm.DiskonDokter,
                    DiskonRs = vm.DiskonRs,
                    DiskonJp = vm.DiskonJp,
                    DiskonBahp = vm.DiskonBahp,
                    DiskonTotal = vm.DiskonTotal,

                    IsDiskonBerlaku = vm.IsDiskonBerlaku,
                    DiskonDari = vm.DiskonDari,
                    DiskonSampai = vm.DiskonSampai,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId
                };

                // **Simpan ke Database**
                _applicationDbContext.TindakanAsuransis.Add(data);
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

        // GET: api/tindakanasuransi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTindakanAsuransiById(Guid id)
        {
            var data = await _applicationDbContext.TindakanAsuransis
                .Where(t => t.TindakanId == id && !t.IsDelete)
                .ToListAsync();  // Mengambil semua data yang sesuai dalam bentuk list

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new { message = "Data ditemukan || 200 OK", data });
        }

        // DELETE: api/tindakanasuransi/{id}
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
                var data = await _applicationDbContext.TindakanAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TindakanAsuransis.Update(data);
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

        // PUT: api/TindakanAsuransi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTindakanAsuransi(Guid id, [FromBody] TindakanAsuransiViewModel vm)
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
                var data = await _applicationDbContext.TindakanAsuransis
                    .FirstOrDefaultAsync(x => x.TindakanAsuransiId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = $"Data relasi dengan ID {id} tidak ditemukan || 404 Not Found" });
                }

                // ======================================================
                // 2) Cek duplikasi (kecuali data yang sedang diupdate)
                // ======================================================
                bool isDuplicate = await _applicationDbContext.TindakanAsuransis
                    .AnyAsync(c =>
                        c.TindakanAsuransiId != id &&
                        c.TindakanId == vm.TindakanId &&
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
                data.TindakanId = vm.TindakanId;
                data.AsuransiId = vm.AsuransiId;
                // ============================
                // MARKUP
                // ============================
                data.MarkupDokter = vm.MarkupDokter;
                data.MarkupRs = vm.MarkupRs;
                data.MarkupJp = vm.MarkupJp;
                data.MarkupBahp = vm.MarkupBahp;
                data.MarkupLainnya = vm.MarkupLainnya;
                data.MarkupTotal = vm.MarkupTotal;

                data.IsMarkupBerlaku = vm.IsMarkupBerlaku ;
                data.MarkupDari = vm.MarkupDari;
                data.MarkupSampai = vm.MarkupSampai;

                // ============================
                // DISKON
                // ============================
                data.DiskonDokter = vm.DiskonDokter;
                data.DiskonRs = vm.DiskonRs;
                data.DiskonJp = vm.DiskonJp;
                data.DiskonBahp = vm.DiskonBahp;
                data.DiskonTotal = vm.DiskonTotal;

                data.IsDiskonBerlaku = vm.IsDiskonBerlaku;
                data.DiskonDari = vm.DiskonDari;
                data.DiskonSampai = vm.DiskonSampai;


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
