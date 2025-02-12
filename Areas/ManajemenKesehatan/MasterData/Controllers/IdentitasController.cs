using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IdentitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        //get : api/identitas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Identitass.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        //get : api/identitas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Identitass.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        //post : api/identitas
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IdentitasViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Identitass
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(d => d.CreateDateTime)
                .FirstOrDefault();

            if (lastCode == null)
            { 
                model.KdIdentitas = "IDT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KdIdentitas.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KdIdentitas = "IDT" + setDateNow + "0001";
                }
                else
                {
                    model.KdIdentitas = "IDT" + setDateNow +
                        (Convert.ToInt32(lastCode.KdIdentitas.Substring(9)) + 1).ToString("D4");
                }
            }

            //validate model
            if (ModelState.IsValid)
            {
                var identitas = new Identitas
                {
                    IdentitasId = Guid.NewGuid(),
                    KdIdentitas = model.KdIdentitas,
                    JenisIdentitas = model.JenisIdentitas,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };

                var checkDuplicate = _context.Identitass.Where(c => c.KdIdentitas == model.KdIdentitas && c.JenisIdentitas == model.JenisIdentitas).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Identitass.Where(c => c.KdIdentitas == model.KdIdentitas && c.JenisIdentitas == model.JenisIdentitas).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Identitass.Add(identitas);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
                    }
                    else
                    {
                        return BadRequest(new { message = "Data tidak dapat di input !!! || 400 Bad Request" });
                    }
                }
                else
                {
                    return Conflict(new { message = "Terdapat duplikasi data !!! || 409 Conflict Data" });
                }
            }
            else
            {
                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
            }

        }

        //put : api/identitas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IdentitasViewModel model)
        {
            //cek apakah data ada di database
            var existingIdentitas = await _context.Identitass.FindAsync(id);
            if (existingIdentitas == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Identitass.Where
                (c => c.KdIdentitas == model.KdIdentitas && c.JenisIdentitas == model.JenisIdentitas
                && c.IdentitasId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            existingIdentitas.JenisIdentitas = model.JenisIdentitas;



            existingIdentitas.UpdateDateTime = DateTimeOffset.Now;
            existingIdentitas.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Identitass.Update(existingIdentitas);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
        }

        //delete : api/identitas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Identitass.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Identitass.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        //fungsi search
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.Identitass
                .Where(n => EF.Functions.Like(n.JenisIdentitas, $"%{keyword}%"))
                .ToListAsync();

            // Jika tidak ada data ditemukan
            if (!searchResults.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }

            // Mengembalikan hasil pencarian
            return Ok(new { message = "Data ditemukan.", data = searchResults });
        }
    }
}
