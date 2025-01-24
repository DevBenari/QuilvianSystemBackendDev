using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianBackendDev.Models;
using QuilvianBackendDev.Repositories;
using System.Data;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class PegawaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PegawaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pegawai
        [HttpGet]
        public IActionResult GetAllPegawai()
        {
            var pegawaiList = _context.UserActives.ToList();
            if (!pegawaiList.Any())
            {
                return NotFound(new { message = "Data pegawai tidak ditemukan." });
            }

            return Ok(pegawaiList);
        }

        // GET: api/Pegawai/{id}
        [HttpGet("{id}")]
        public IActionResult GetPegawaiById(Guid id)
        {
            var pegawai = _context.UserActives.Find(id);
            if (pegawai == null)
            {
                return NotFound(new { message = "Pegawai tidak ditemukan." });
            }

            return Ok(pegawai);
        }

        // POST: api/Pegawai
        [HttpPost]
        public IActionResult AddPegawai([FromBody] Pegawai newPegawai)
        {
            if (newPegawai == null)
            {
                return BadRequest(new { message = "Data pegawai tidak valid." });
            }

            newPegawai.UserActiveId = Guid.NewGuid();
            newPegawai.IsActive = true;
            newPegawai.CreateDateTime = DateTimeOffset.Now;

            _context.UserActives.Add(newPegawai);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetPegawaiById), new { id = newPegawai.UserActiveId }, newPegawai);
        }

        // PUT: api/Pegawai/{id}
        [HttpPut("{id}")]
        public IActionResult UpdatePegawai(Guid id, [FromBody] Pegawai updatedPegawai)
        {
            var existingPegawai = _context.UserActives.Find(id);
            if (existingPegawai == null)
            {
                return NotFound(new { message = "Pegawai tidak ditemukan." });
            }

            existingPegawai.NamaLengkap = updatedPegawai.NamaLengkap;
            existingPegawai.NoIdentitas = updatedPegawai.NoIdentitas;
            existingPegawai.TempatLahir = updatedPegawai.TempatLahir;
            existingPegawai.TanggalLahir = updatedPegawai.TanggalLahir;
            existingPegawai.JenisKelamin = updatedPegawai.JenisKelamin;
            existingPegawai.AlamatDomisili = updatedPegawai.AlamatDomisili;
            existingPegawai.NomorHP = updatedPegawai.NomorHP;
            existingPegawai.Email = updatedPegawai.Email;
            existingPegawai.Pekerjaan = updatedPegawai.Pekerjaan;
            existingPegawai.NamaKantor = updatedPegawai.NamaKantor;
            existingPegawai.Departemen = updatedPegawai.Departemen;
            existingPegawai.NamaKeluarga = updatedPegawai.NamaKeluarga;
            existingPegawai.NomorTeleponKeluarga = updatedPegawai.NomorTeleponKeluarga;

            _context.SaveChanges();
            return NoContent();
        }

        // DELETE: api/Pegawai/{id}
        [HttpDelete("{id}")]
        public IActionResult DeletePegawai(Guid id)
        {
            var pegawai = _context.UserActives.Find(id);
            if (pegawai == null)
            {
                return NotFound(new { message = "Pegawai tidak ditemukan." });
            }

            _context.UserActives.Remove(pegawai);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
