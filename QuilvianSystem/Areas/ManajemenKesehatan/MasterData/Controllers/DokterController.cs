using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    public class DokterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DokterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Dokters.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Dokter
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Dokter model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            model.DokterId = Guid.NewGuid();
            _context.Dokters.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.DokterId }, model);
        }

        // PUT: api/Dokter/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Dokter model)
        {
            if (model == null || id != model.DokterId)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            var existingRecord = await _context.Dokters.FindAsync(id);
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

            _context.Dokters.Update(existingRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui." });
        }

        // DELETE: api/Dokter/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Dokters.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }
    }
}
