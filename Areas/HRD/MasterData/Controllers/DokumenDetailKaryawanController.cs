using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DokumenDetailKaryawanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DokumenDetailKaryawanController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                from d in _context.Set<DokumenDetailKaryawan>()
                orderby d.CreateDateTime descending
                select new
                {
                    d.DokDetailId,
                    d.UserActiveId,
                    d.NamaPeserta,
                    d.NoPeserta,
                    d.TglUpload,
                    d.NamaDokumen,
                    d.FilePath,
                    d.StatusKepemilikan,
                    d.CreateDateTime, d.CreateBy,
                    d.UpdateDateTime, d.UpdateBy,
                    d.DeleteDateTime, d.DeleteBy,
                    d.IsDelete
                };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
            if (!listData.Any()) return NotFound(new { message = "Belum ada data || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listData,
                pagination = new { CurrentPage = page, PerPage = perPage, TotalRows = totalRows, TotalPages = totalPages }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DokumenDetailKaryawan>> GetById(Guid id)
        {
            var data = await _context.Set<DokumenDetailKaryawan>().FindAsync(id);
            if (data == null) return NotFound();
            return data;
        }

        [HttpPost]
        public async Task<ActionResult<DokumenDetailKaryawan>> Create([FromBody] DokumenDetailKaryawanVM vm)
        {
            var entity = new DokumenDetailKaryawan
            {
                DokDetailId = Guid.NewGuid(),
                UserActiveId = vm.UserActiveId,
                NamaPeserta = vm.NamaPeserta,
                NoPeserta = vm.NoPeserta,
                TglUpload = vm.TglUpload ?? DateTimeOffset.UtcNow,
                NamaDokumen = vm.NamaDokumen,
                FilePath = vm.FilePath,
                StatusKepemilikan = vm.StatusKepemilikan,
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.DokDetailId }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DokumenDetailKaryawanVM vm)
        {
            var entity = await _context.Set<DokumenDetailKaryawan>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.UserActiveId = vm.UserActiveId;
            entity.NamaPeserta = vm.NamaPeserta;
            entity.NoPeserta = vm.NoPeserta;
            entity.TglUpload = vm.TglUpload ?? entity.TglUpload;
            entity.NamaDokumen = vm.NamaDokumen;
            entity.FilePath = vm.FilePath;
            entity.StatusKepemilikan = vm.StatusKepemilikan;
            entity.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _context.Set<DokumenDetailKaryawan>().FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan." });

            _context.Remove(data);
            var result = await _context.SaveChangesAsync();
            if (result > 0) return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
