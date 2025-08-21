using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [Route("api/HRD/[controller]")]
    [ApiController]
    public class JenisLemburController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JenisLemburController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/HRD/JenisLembur?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetJenisLembur(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from j in _context.JenisLemburs
                        join uc in _context.UserActives on j.CreateBy equals uc.UserActiveId into createdByJoin
                        from uc in createdByJoin.DefaultIfEmpty()
                        //where j.IsDelete == false
                        orderby j.CreateDateTime descending
                        select new
                        {
                            j.JenisLemburId,
                            j.NamaLembur,
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

        // ✅ GET: api/HRD/JenisLembur/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JenisLembur>> GetJenisLembur(Guid id)
        {
            var jenis = await _context.JenisLemburs.FindAsync(id);

            if (jenis == null)
                return NotFound();

            return jenis;
        }

        // ✅ POST: api/HRD/JenisLembur
        [HttpPost]
        public async Task<ActionResult<JenisLembur>> PostJenisLembur(JenisLembur jenis)
        {
            jenis.JenisLemburId = Guid.NewGuid();
            jenis.CreateDateTime = DateTimeOffset.UtcNow;

            _context.JenisLemburs.Add(jenis);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJenisLembur), new { id = jenis.JenisLemburId }, jenis);
        }

        // ✅ PUT: api/HRD/JenisLembur/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJenisLembur(Guid id, JenisLembur jenis)
        {
            if (id != jenis.JenisLemburId)
                return BadRequest();

            jenis.UpdateDateTime = DateTimeOffset.UtcNow;
            _context.Entry(jenis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.JenisLemburs.Any(e => e.JenisLemburId == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJenisLembur(Guid id)
        {
            // ✅ cek koneksi DB
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            // ✅ cari data berdasarkan id
            var data = await _context.JenisLemburs.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            // ✅ lakukan hard delete
            _context.JenisLemburs.Remove(data);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
