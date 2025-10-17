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
    public class MasterKeahlianController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MasterKeahlianController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                from m in _context.Set<MasterKeahlian>()
                orderby m.CreateDateTime descending
                select new
                {
                    m.KeahlianId,
                    m.NamaKeahlian,
                    m.IsActive,
                    m.Keterangan,
                    m.CreateDateTime, m.CreateBy, m.UpdateDateTime, m.UpdateBy, m.DeleteDateTime, m.DeleteBy, m.IsDelete
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
        public async Task<ActionResult<MasterKeahlian>> GetById(Guid id)
        {
            var data = await _context.Set<MasterKeahlian>().FindAsync(id);
            if (data == null) return NotFound();
            return data;
        }

        [HttpPost]
        public async Task<ActionResult<MasterKeahlian>> Create([FromBody] MasterKeahlianVM vm)
        {
            var entity = new MasterKeahlian
            {
                KeahlianId = Guid.NewGuid(),
                NamaKeahlian = vm.NamaKeahlian,
                IsActive = vm.IsActive ?? true,
                Keterangan = vm.Keterangan,
                CreateDateTime = DateTimeOffset.UtcNow
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.KeahlianId }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterKeahlianVM vm)
        {
            var entity = await _context.Set<MasterKeahlian>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.NamaKeahlian = vm.NamaKeahlian;
            entity.IsActive = vm.IsActive ?? entity.IsActive;
            entity.Keterangan = vm.Keterangan;
            entity.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _context.Set<MasterKeahlian>().FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan." });

            _context.Remove(data);
            var result = await _context.SaveChangesAsync();
            if (result > 0) return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
