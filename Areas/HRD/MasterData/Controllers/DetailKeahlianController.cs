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
    public class DetailKeahlianController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DetailKeahlianController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                from d in _context.Set<DetailKeahlian>()
                orderby d.CreateDateTime descending
                select new
                {
                    d.DetailKeahlianId,
                    d.UserActiveId,
                    d.KeahlianId,
                    d.LevelKeahlian,
                    d.Penilai,
                    d.CreateDateTime, d.CreateBy, d.UpdateDateTime, d.UpdateBy, d.DeleteDateTime, d.DeleteBy, d.IsDelete
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
        public async Task<ActionResult<DetailKeahlian>> GetById(Guid id)
        {
            var data = await _context.Set<DetailKeahlian>().FindAsync(id);
            if (data == null) return NotFound();
            return data;
        }

        [HttpPost]
        public async Task<ActionResult<DetailKeahlian>> Create([FromBody] DetailKeahlianVM vm)
        {
            var entity = new DetailKeahlian
            {
                DetailKeahlianId = Guid.NewGuid(),
                UserActiveId = vm.UserActiveId,
                KeahlianId = vm.KeahlianId,
                LevelKeahlian = vm.LevelKeahlian,
                Penilai = vm.Penilai,
                CreateDateTime = DateTimeOffset.UtcNow
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.DetailKeahlianId }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DetailKeahlianVM vm)
        {
            var entity = await _context.Set<DetailKeahlian>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.UserActiveId = vm.UserActiveId;
            entity.KeahlianId = vm.KeahlianId;
            entity.LevelKeahlian = vm.LevelKeahlian;
            entity.Penilai = vm.Penilai;
            entity.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _context.Set<DetailKeahlian>().FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan." });

            _context.Remove(data);
            var result = await _context.SaveChangesAsync();
            if (result > 0) return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
