using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class NegaraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NegaraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get : api/Negara
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Negaras.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // Get : api/Negara/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Negaras.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // Post : api/Negara
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Negara model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            model.NegaraId = Guid.NewGuid();
            _context.Negaras.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.NegaraId }, model);
        }

        // Put : api/Negara/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Negara model)
        {
            if (model == null || model.NegaraId != id)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            var existingRecord = await _context.Negaras.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound(new { message = "Data tidak ditemukan" });
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
            _context.Negaras.Update(existingRecord);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil diupdate." });
        }

        // Delete : api/Negara/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Negaras.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Negaras.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }


    }
}
