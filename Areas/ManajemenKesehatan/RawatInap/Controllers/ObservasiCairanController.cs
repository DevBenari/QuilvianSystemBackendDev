using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Observasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObservasiCairanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ObservasiCairanController> _logger;

        public ObservasiCairanController(ApplicationDbContext context, ILogger<ObservasiCairanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from o in _context.ObservasiCairans
                        join u in _context.UserActives on o.CreateBy equals u.UserActiveId
                        where o.IsDelete == false
                        orderby o.TglObservasi descending
                        select new
                        {
                            o.ObservasiCairanId,
                            o.KunjunganId,
                            o.PasienId,
                            o.TglObservasi,
                            o.CairanMasuk,
                            o.CairanKeluar,
                            o.CairanSisa,
                            o.JumlahUrin,
                            o.TTDId,
                            o.PathTtd,
                            o.Keterangan,
                            CreateByName = u.FullName
                        };

            int totalRows = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var data = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = data,
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ObservasiCairans.FindAsync(id);

            if (data == null || data.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObservasiCairanViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            // Ambil email user dari JWT
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.UserActives.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            // Mapping ViewModel ke Entity
            var model = new ObservasiCairan
            {
                ObservasiCairanId = Guid.NewGuid(),
                KunjunganId = vm.KunjunganId,
                PasienId = vm.PasienId,
                UserActiveId = user.UserActiveId, // Override dari JWT, bukan dari input
                TglObservasi = vm.TglObservasi,
                CairanMasuk = vm.CairanMasuk,
                CairanKeluar = vm.CairanKeluar,
                CairanSisa = vm.CairanSisa,
                JumlahUrin = vm.JumlahUrin,
                TTDId = vm.TTDId,
                PathTtd = vm.PathTtd,
                Keterangan = vm.Keterangan,

                CreateDateTime = DateTime.UtcNow,
                CreateBy = user.UserActiveId
            };

            _context.ObservasiCairans.Add(model);
            await _context.SaveChangesAsync();

            return Created("", new { message = "Data berhasil dibuat || 201 Created" });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ObservasiCairan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var data = await _context.ObservasiCairans.FindAsync(id);
            if (data == null || data.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.UserActives.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            // Update field yang dapat diubah
            data.TglObservasi = model.TglObservasi;
            data.CairanMasuk = model.CairanMasuk;
            data.CairanKeluar = model.CairanKeluar;
            data.CairanSisa = model.CairanSisa;
            data.JumlahUrin = model.JumlahUrin;
            data.Keterangan = model.Keterangan;
            data.PathTtd = model.PathTtd;
            data.TTDId = model.TTDId;

            data.UpdateDateTime = DateTime.UtcNow;
            data.UpdateBy = user.UserActiveId;

            _context.ObservasiCairans.Update(data);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui || 200 OK" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var data = await _context.ObservasiCairans.FindAsync(id);
            if (data == null || data.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.UserActives.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            data.IsDelete = true;
            data.DeleteBy = user.UserActiveId;
            data.DeleteDateTime = DateTime.UtcNow;

            _context.ObservasiCairans.Update(data);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete)" });
        }
    }
}
