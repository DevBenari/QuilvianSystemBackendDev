using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
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
        public async Task<IActionResult> Create([FromBody] PekerjaanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Pekerjaans
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KodePekerjaan)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KodePekerjaan = "PKJ" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KodePekerjaan.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KodePekerjaan = "PKJ" + setDateNow + "0001";
                }
                else
                {
                    model.KodePekerjaan = "PKJ" + setDateNow +
                        (Convert.ToInt32(lastCode.KodePekerjaan.Substring(9)) + 1).ToString("D4");
                }
            }

            //Validate ModelState
            if (ModelState.IsValid)
            {
                var pekerjaan = new Pekerjaan
                {
                    PekerjaanId = Guid.NewGuid(),
                    KodePekerjaan = model.KodePekerjaan,
                    NamaPekerjaan = model.NamaPekerjaan,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };


                var checkDuplicate = _context.Pekerjaans.Where(c => c.KodePekerjaan == model.KodePekerjaan && c.NamaPekerjaan == model.NamaPekerjaan).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Pekerjaans.Where(c => c.KodePekerjaan == model.KodePekerjaan && c.NamaPekerjaan == model.NamaPekerjaan).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Pekerjaans.Add(pekerjaan);
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

        // PUT: api/Pekerjaan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PekerjaanViewModel model)
        {
            //cek apakah data ada di database
            var existingPekerjaan = await _context.Pekerjaans.FindAsync(id);
            if (existingPekerjaan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Pekerjaans.Where
                (c => c.KodePekerjaan == model.KodePekerjaan && c.NamaPekerjaan == model.NamaPekerjaan
                && c.PekerjaanId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingPekerjaan.NamaPekerjaan = model.NamaPekerjaan;

            // kode negara tidak diupdate hanya diganti nama pekerjaannya saja
            //existingNegara.KodeNegara = model.KodeNegara;

            existingPekerjaan.UpdateDateTime = DateTimeOffset.Now;
            existingPekerjaan.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Pekerjaans.Update(existingPekerjaan);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
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

        //search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.Pekerjaans
                .Where(n => EF.Functions.Like(n.NamaPekerjaan, $"%{keyword}%"))
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
