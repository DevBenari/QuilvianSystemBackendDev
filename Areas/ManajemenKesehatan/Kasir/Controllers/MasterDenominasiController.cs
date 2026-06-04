using System.Security.Claims;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class MasterDenominasiController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MasterDenominasiController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _db.MasterDenominasies
                           .Where(x => x.IsDelete == false || x.IsDelete == null)
                           .OrderByDescending(x => x.CreateDateTime)
                           .Select(x => new {
                               x.DenominasiId,
                               x.KodeDenominasi,
                               x.MataUang,
                               x.NominalPecahan,
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
            var data = await _db.MasterDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });
            return Ok(new { message = "Ditemukan", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MasterDenominasiViewModel vm)
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

            bool isDuplicate = await _db.MasterDenominasies.AnyAsync(c => c.KodeDenominasi.ToLower() == vm.KodeDenominasi.ToLower() && c.IsDelete == false);
            if (isDuplicate) return Conflict(new { message = "Kode Denominasi sudah ada" });

            var data = new MasterDenominasi
            {
                DenominasiId = Guid.NewGuid(),
                KodeDenominasi = vm.KodeDenominasi,
                MataUang = vm.MataUang,
                NominalPecahan = vm.NominalPecahan,
                Keterangan = vm.Keterangan,
                CreateBy = userActiveId,
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _db.MasterDenominasies.Add(data);
            await _db.SaveChangesAsync();
            return Created("", new { message = "Tambah Data Berhasil" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterDenominasiViewModel vm)
        {
            var data = await _db.MasterDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            var emailLogin = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var getUserActive = await _db.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            bool isDuplicate = await _db.MasterDenominasies
                .AnyAsync(c => c.KodeDenominasi.ToLower() == vm.KodeDenominasi.ToLower()
                && c.DenominasiId != id && c.IsDelete == false);

            if (isDuplicate) return Conflict(new { message = "Kode Denominasi sudah ada" });

            data.KodeDenominasi = vm.KodeDenominasi;
            data.MataUang = vm.MataUang;
            data.NominalPecahan = vm.NominalPecahan;
            data.Keterangan = vm.Keterangan;
            data.UpdateBy = userActiveId;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            _db.MasterDenominasies.Update(data);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Update Data Berhasil" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _db.MasterDenominasies.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            var emailLogin = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var getUserActive = await _db.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            data.IsDelete = true;
            data.DeleteBy = userActiveId;
            data.DeleteDateTime = DateTimeOffset.UtcNow;

            _db.MasterDenominasies.Update(data);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete)" });
        }
    }
}