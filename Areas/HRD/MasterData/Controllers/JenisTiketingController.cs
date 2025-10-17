using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JenisTiketingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JenisTiketingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/HRD/JenisTiketing?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetJenisTiketing(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from j in _context.JenisTiketings
                        join uc in _context.UserActives on j.CreateBy equals uc.UserActiveId into createdByJoin
                        from uc in createdByJoin.DefaultIfEmpty()
                        orderby j.CreateDateTime descending
                        select new
                        {
                            j.JenisTicketId,
                            j.DepartementId,
                            j.NamaTicket,
                            j.Keterangan,

                            j.CreateDateTime,
                            j.CreateBy,
                            CreateByName = uc != null ? uc.FullName : null,
                            j.UpdateDateTime,
                            j.UpdateBy,
                            j.DeleteDateTime,
                            j.DeleteBy,
                            j.IsDelete
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listData.Any())
                return NotFound(new { message = "Belum ada data || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listData,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        // ✅ GET: api/HRD/JenisTiketing/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JenisTiketing>> GetJenisTiketing(Guid id)
        {
            var jenis = await _context.JenisTiketings.FindAsync(id);

            if (jenis == null)
                return NotFound();

            return jenis;
        }

        // ✅ POST: api/HRD/JenisTiketing
        [HttpPost]
        public async Task<ActionResult<JenisTiketing>> PostJenisTiketing(JenisTiketing jenis)
        {
            jenis.JenisTicketId = Guid.NewGuid();
            jenis.CreateDateTime = DateTimeOffset.UtcNow;

            _context.JenisTiketings.Add(jenis);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJenisTiketing), new { id = jenis.JenisTicketId }, jenis);
        }

        // ✅ PUT: api/HRD/JenisTiketing/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJenisTiketing(Guid id, JenisTiketing jenis)
        {
            if (id != jenis.JenisTicketId)
                return BadRequest();

            jenis.UpdateDateTime = DateTimeOffset.UtcNow;
            _context.Entry(jenis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.JenisTiketings.Any(e => e.JenisTicketId == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // ✅ DELETE: api/HRD/JenisTiketing/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJenisTiketing(Guid id)
        {
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _context.JenisTiketings.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _context.JenisTiketings.Remove(data);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
