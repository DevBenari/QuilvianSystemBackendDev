using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class IGDObservasiDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<IGDObservasiDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IGDObservasiDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDObservasiDetailController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = (from a in _applicationDbContext.IGDObservasiDetails
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId

                            // join ke obat
                            join o in _applicationDbContext.Obats
                            on a.ObatId equals o.ObatId into oGroup
                            from o in oGroup.DefaultIfEmpty()

                            where (a.IsDelete == false || a.IsDelete == null)
                            && a.ObservasiDetailIgdId == id
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.ObservasiDetailIgdId,
                                a.ObservasiIgdId,
                                a.TglObservasi,
                                a.ObatId,
                                NamaObat = o.ObatName ?? null,
                                DosisObat = o.TakaranDosis ?? null,
                                a.GambaranEKG,
                                a.DCShock,
                                a.TekananDarahDiastolic,
                                a.TekananDarahSystolic,
                                a.RR,
                                a.Suhu,
                                a.SPO2,
                                a.Urine,
                                a.Pendarahan,
                                a.Muntah,
                                a.Keterangan,
                            });
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IGDObservasiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                //// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                //                    && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new IGDObservasiDetail
                {
                    ObservasiDetailIgdId = Guid.NewGuid(),
                    ObservasiIgdId = vm.ObservasiIgdId,
                    TglObservasi = vm.TglObservasi,
                    ObatId = vm.ObatId,
                    GambaranEKG = vm.GambaranEKG,
                    DCShock = vm.DCShock,
                    TekananDarahDiastolic = vm.TekananDarahDiastolic,
                    TekananDarahSystolic = vm.TekananDarahSystolic,
                    RR = vm.RR,
                    Suhu = vm.Suhu,
                    SPO2 = vm.SPO2,
                    Urine = vm.Urine,
                    Pendarahan = vm.Pendarahan,
                    Muntah = vm.Muntah,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.IGDObservasiDetails.Add(data);
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

        [HttpPut("detail/{id}")]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] IGDObservasiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var userActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (userActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                // Cari data existing
                var data = await _applicationDbContext.IGDObservasiDetails
                    .FirstOrDefaultAsync(o => o.ObservasiDetailIgdId == id && (o.IsDelete == false || o.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Observasi Detail dengan ID {id} tidak ditemukan." });

                // Update field
                data.TglObservasi = vm.TglObservasi;
                data.ObatId = vm.ObatId;
                data.GambaranEKG = vm.GambaranEKG;
                data.DCShock = vm.DCShock;
                data.TekananDarahDiastolic = vm.TekananDarahDiastolic;
                data.TekananDarahSystolic = vm.TekananDarahSystolic;
                data.RR = vm.RR;
                data.Suhu = vm.Suhu;
                data.SPO2 = vm.SPO2;
                data.Urine = vm.Urine;
                data.Pendarahan = vm.Pendarahan;
                data.Muntah = vm.Muntah;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActive.UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDObservasiDetails.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Update Observasi Detail IGD berhasil || 200 OK" });

                return StatusCode(500, new { message = "Gagal memperbarui data di database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                // **Cari Data**
                var data = await _applicationDbContext.IGDObservasiDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.IGDObservasiDetails.Update(data);
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
    }
}
