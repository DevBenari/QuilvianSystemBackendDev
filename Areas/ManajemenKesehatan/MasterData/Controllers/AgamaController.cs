using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Migrations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgamaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgamaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Agama
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Agamas.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Agama/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Agamas.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Agama
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AgamaViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Agamas
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.AgamaKode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.AgamaKode = "AGM" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.AgamaKode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.AgamaKode = "AGM" + setDateNow + "0001";
                }
                else
                {
                    model.AgamaKode = "AGM" + setDateNow +
                        (Convert.ToInt32(lastCode.AgamaKode.Substring(9)) + 1).ToString("D4");
                }
            }

            //validate modelstate
            if (ModelState.IsValid)
            {
                var agama = new Agama()
                {
                    AgamaId = Guid.NewGuid(),
                    AgamaKode = model.AgamaKode,
                    JenisAgama = model.JenisAgama,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };
                var checkDuplicate = _context.Agamas.Where(c => c.AgamaKode == model.AgamaKode && c.JenisAgama == model.JenisAgama).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Agamas.Where(c => c.AgamaKode == model.AgamaKode && c.JenisAgama == model.JenisAgama).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Agamas.Add(agama);
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
                return BadRequest(new { message = "Data tidak valid." });
            }
        }

            // PUT: api/Agama/{id}
            [HttpPut("{id}")]
            public async Task<IActionResult> Update(Guid id, [FromBody] AgamaViewModel model)
            {
                //cek apakah data ada di database
                var existingAgama = await _context.Agamas.FindAsync(id);
                if (existingAgama == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
                }

                //cek duplikat data
                var checkDuplicate = _context.Agamas.Where
                    (c => c.AgamaKode == model.AgamaKode && c.JenisAgama == model.JenisAgama
                    && c.AgamaId != id).FirstOrDefault();
                if (checkDuplicate != null)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Update properti dari data yang ada dengan nilai dari model
                existingAgama.JenisAgama = model.JenisAgama;
                //tidak update kode agama
                //existingAgama.AgamaKode = model.AgamaKode;
                existingAgama.UpdateDateTime = DateTimeOffset.Now;
                existingAgama.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

                // Simpan perubahan ke database
                try
                {
                    _context.Agamas.Update(existingAgama);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
                }
                catch (Exception ex)
                {
                    // Tangani kesalahan jika ada
                    return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
                }
            }


            // DELETE: api/Agama/{id}
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(Guid id)
            {
                var record = await _context.Agamas.FindAsync(id);
                if (record == null)
                {
                    return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
                }
                _context.Agamas.Remove(record);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data berhasil dihapus." });
             }

            //fungsi search
            // GET: api/Agama/Search?keyword={keyword}
            [HttpGet("Search")]
            public async Task<IActionResult> Search([FromQuery] string keyword)
            {
                // Validasi input keyword
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
                }

                // Lakukan pencarian di database (case-insensitive)
                var searchResults = await _context.Agamas
                    .Where(n => EF.Functions.Like(n.JenisAgama, $"%{keyword}%")).ToListAsync();

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

