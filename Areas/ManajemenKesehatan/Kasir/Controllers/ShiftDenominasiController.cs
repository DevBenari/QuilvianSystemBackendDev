using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ShiftDenominasiController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShiftDenominasiController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _db.ShiftDenominasies
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new {
                    x.ShiftDenominasiId,
                    x.KodeShiftDenominasi,
                    x.LayananId,
                    x.KasirId,
                    x.TipePerhitungan,
                    x.DenominasiId,
                    x.LembarKoin,
                    x.TotalDenominasi,
                    x.Keterangan,
                    x.CreateDateTime,
                    x.CreateBy
                });

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            return Ok(new { message = "Berhasil", data = rows, pagination = new { CurrentPage = page, PerPage = perPage, TotalRows = totalRows, TotalPages = totalPages } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _db.ShiftDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });
            return Ok(new { message = "Ditemukan", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShiftDenominasiViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Data tidak valid" });
            var emailLogin = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(emailLogin))
            {
                return Unauthorized(new { message = "User tidak terautentikasi!" });
            }

            var getUserActive = await _db.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
            {
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });
            }

            var userActiveId = getUserActive.UserActiveId; // Ini sudah Guid

            var data = new ShiftDenominasi
            {
                ShiftDenominasiId = Guid.NewGuid(),
                KodeShiftDenominasi = vm.KodeShiftDenominasi,
                LayananId = vm.LayananId,
                KasirId = vm.KasirId,
                TipePerhitungan = vm.TipePerhitungan,
                DenominasiId = vm.DenominasiId,
                LembarKoin = vm.LembarKoin,
                TotalDenominasi = vm.TotalDenominasi,
                Keterangan = vm.Keterangan,
                CreateBy = userActiveId,
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _db.ShiftDenominasies.Add(data);
            await _db.SaveChangesAsync();
            return Created("", new { message = "Tambah Data Berhasil" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ShiftDenominasiViewModel vm)
        {
            var data = await _db.ShiftDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            var userActiveId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

            data.KodeShiftDenominasi = vm.KodeShiftDenominasi;
            data.LayananId = vm.LayananId;
            data.KasirId = vm.KasirId;
            data.TipePerhitungan = vm.TipePerhitungan;
            data.DenominasiId = vm.DenominasiId;
            data.LembarKoin = vm.LembarKoin;
            data.TotalDenominasi = vm.TotalDenominasi;
            data.Keterangan = vm.Keterangan;
            data.UpdateBy = userActiveId;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            _db.ShiftDenominasies.Update(data);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Update Data Berhasil" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _db.ShiftDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            var userActiveId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            data.IsDelete = true;
            data.DeleteBy = userActiveId;
            data.DeleteDateTime = DateTimeOffset.UtcNow;

            _db.ShiftDenominasies.Update(data);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus (soft delete)" });
        }
    }
}