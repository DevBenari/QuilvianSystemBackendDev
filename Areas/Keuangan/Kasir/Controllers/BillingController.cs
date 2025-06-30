using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<BillingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BillingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BillingController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetBillingByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId)
        {
            var kunjungan = await _applicationDbContext.Billings.Where(b => b.KunjunganId == kunjunganId && !b.IsDelete).ToListAsync();
            if (kunjungan == null)
                return NotFound(new { message = "Data kunjungan tidak ditemukan!" });

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = kunjungan
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilling(Guid id, [FromBody] BillingViewModel vm )
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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

                // cari data
                var billing = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b => b.BillingId == id );

                if (billing == null)
                    return NotFound(new { message = "Data billing tidak ditemukan." });

                var kodePrefix = billing.BillingKode?.Substring(0, 2).ToUpperInvariant();

                decimal harga = 0;

                switch (kodePrefix)
                {
                    case "OB":
                        var obat = await _applicationDbContext.Obats
                            .FirstOrDefaultAsync(o => o.ObatId == billing.ItemId && !o.IsDelete);
                        if (obat == null)
                            return NotFound(new { message = "Data obat tidak ditemukan." });

                        harga = obat.HargaJual;
                        break;

                    case "TD":
                        // Ambil Tindakan
                        var tindakan = await _applicationDbContext.Tindakans
                            .FirstOrDefaultAsync(t => t.TindakanId == billing.ItemId && !t.IsDelete);
                        if (tindakan == null)
                            return NotFound(new { message = "Data tindakan tidak ditemukan." });

                        // Ambil kunjungan
                        var kunjungan = await _applicationDbContext.Kunjungans
                            .FirstOrDefaultAsync(k => k.KunjunganID == billing.KunjunganId);
                        if (kunjungan == null)
                            return NotFound(new { message = "Data kunjungan tidak ditemukan." });

                        // Ambil kelas berdasarkan jenis kunjungan
                        var kelas = await _applicationDbContext.Kelass
                            .FirstOrDefaultAsync(k => k.KodeKelas == kunjungan.JenisKunjungan);
                        if (kelas == null)
                            return NotFound(new { message = "Kelas untuk jenis kunjungan ini tidak ditemukan." });

                        // Ambil tarif kelas untuk tindakan dan kelas
                        var tarifKelas = await _applicationDbContext.TarifKelass
                            .FirstOrDefaultAsync(t => t.TindakanId == tindakan.TindakanId && t.KelasId == kelas.KelasId);
                        if (tarifKelas == null)
                            return NotFound(new { message = "Tarif untuk tindakan dan kelas ini tidak ditemukan." });

                        harga = tarifKelas.TarifTotal ?? 0;
                        break;

                    default:
                        return BadRequest(new { message = "BillingKode tidak dikenali (harus OB atau TD)." });
                }

                // Update billing
                billing.HargaItem = harga;
                billing.SubTotalItem = harga * (vm.QtyItem ?? 1); // default 1 jika null
                billing.UpdateDateTime = DateTimeOffset.UtcNow;
                billing.UpdateBy = userActiveId;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Billing berhasil diperbarui." });
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

    }
}
