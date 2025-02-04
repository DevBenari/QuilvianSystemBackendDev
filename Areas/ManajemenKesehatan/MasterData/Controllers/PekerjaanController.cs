using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PekerjaanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PekerjaanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pekerjaan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Pekerjaans.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Pekerjaan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Pekerjaans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Pekerjaan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Pekerjaan model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            model.PekerjaanId = Guid.NewGuid();
            _context.Pekerjaans.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.PekerjaanId }, model);
        }

        // PUT: api/Pekerjaan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Pekerjaan model)
        {
            if (model == null || id != model.PekerjaanId)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            var existingRecord = await _context.Pekerjaans.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }
            // Update properties
            foreach (var prop in model.GetType().GetProperties())
            {
                var value = prop.GetValue(model);
                if (value != null)
                {
                    prop.SetValue(existingRecord, value);
                }
            }

            _context.Pekerjaans.Update(existingRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui." });
        }

        // DELETE: api/Pekerjaan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Pekerjaans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Pekerjaans.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }
    }
}
