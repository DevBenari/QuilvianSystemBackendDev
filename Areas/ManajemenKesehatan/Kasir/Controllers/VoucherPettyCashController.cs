using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class VoucherPettyCashController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VoucherPettyCashController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherPettyCashViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            // Ambil user active ID dari JWT
            var emailLogin = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var getUserActive = await _db.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            try
            {
                // Buat object baru
                var data = new VoucherPettyCash
                {
                    VoucherPettyCashId = Guid.NewGuid(),
                    KodeVoucherPC = vm.KodeVoucherPC,
                    LayananId = vm.LayananId,
                    KasirId = vm.KasirId,
                    ShiftSesi = vm.ShiftSesi,
                    NamaPenerima = vm.NamaPenerima,
                    TanggalPengajuan = vm.TanggalPengajuan,
                    KategoriVoucher = vm.KategoriVoucher,
                    NominalVoucher = vm.NominalVoucher,
                    BuktiNota = vm.BuktiNota,
                    StatusVoucher = vm.StatusVoucher,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _db.VoucherPettyCashes.Add(data);
                int result = await _db.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Voucher Petty Cash Berhasil || 201 Created" });
                else
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listdata = await _db.VoucherPettyCashes
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .ToListAsync();
            return Ok(new { message = "Data berhasil diambil", data = listdata });
        }
    }
}