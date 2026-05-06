using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AlatPemakaianDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AlatPemakaianDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlatPemakaianDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AlatPemakaianDetailController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] AlatPemakaianDetailViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        //// **Cek Duplikasi**
        //        //bool isDuplicate = await _applicationDbContext.Diskons
        //        //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
        //        //                    && c.IsDelete == false);

        //        //if (isDuplicate)
        //        //{
        //        //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
        //        //}

        //        // Ambil nama & harga alat (sesuaikan tabel/kolomnya)
        //        var alatDb = await _applicationDbContext.TarifKelass
        //            .Where(x => x.PeralatanId == vm.PeralatanId && x.KelasId == vm.KelasId)
        //            .FirstOrDefaultAsync();

        //        var namaAlat = await _applicationDbContext.Peralatans
        //            .Where(x => x.PeralatanId == vm.PeralatanId)
        //            .Select(x => x.NamaPeralatan)
        //            .FirstOrDefaultAsync();

        //        if (alatDb == null && namaAlat == null)
        //            return BadRequest(new { message = $"Peralatan tidak ditemukan: {vm.PeralatanId}" });

        //        // **Buat Data Baru**
        //        var data = new AlatPemakaianDetail
        //        {
        //            DetailPemakaianAlatId = Guid.NewGuid(),
        //            PemakaianAlatId = vm.PemakaianAlatId,
        //            PeralatanId = vm.PeralatanId,
        //            KelasId = vm.KelasId,
        //            QtyPemakaian = vm.QtyPemakaian,
        //            HargaPeralatan = vm.HargaPeralatan,
        //            TotalPemakaianAlat = vm.TotalPemakaianAlat,
        //            Keterangan = vm.Keterangan,
        //            CreateBy = userActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //        };

        //        // **Simpan ke Database**
        //        _applicationDbContext.AlatPemakaianDetails.Add(data);

        //        int billingIndex = await _applicationDbContext.Billings
        //            .CountAsync(b =>
        //                b.KunjunganId == vm.KunjunganId.Value &&
        //                b.JenisBilling.ToLower() == "alkes");
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

    }
}
