using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PendidikanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PendidikanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pendidikan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Pendidikans.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Pendidikan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Pendidikans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Pendidikan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PendidikanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Pendidikans
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KodePendidikan)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KodePendidikan = "PDD" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KodePendidikan.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KodePendidikan = "PDD" + setDateNow + "0001";
                }
                else
                {
                    model.KodePendidikan = "PDD" + setDateNow +
                        (Convert.ToInt32(lastCode.KodePendidikan.Substring(9)) + 1).ToString("D4");
                }
            }

            //Validate ModelState
            if (ModelState.IsValid)
            {
                var pendidikan = new Pendidikan
                {
                    PendidikanId = Guid.NewGuid(),
                    KodePendidikan = model.KodePendidikan,
                    NamaPendidikan = model.NamaPendidikan,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };


                var checkDuplicate = _context.Pendidikans.Where(c => c.KodePendidikan == model.KodePendidikan && c.NamaPendidikan == model.NamaPendidikan).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Pendidikans.Where(c => c.KodePendidikan == model.KodePendidikan && c.NamaPendidikan == model.NamaPendidikan).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Pendidikans.Add(pendidikan);
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

        // PUT: api/Pendidikan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PendidikanViewModel model)
        {
            //cek apakah data ada di database
            var existingNegara = await _context.Pendidikans.FindAsync(id);
            if (existingNegara == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Pendidikans.Where
                (c => c.KodePendidikan == model.KodePendidikan && c.NamaPendidikan == model.NamaPendidikan
                && c.PendidikanId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingNegara.NamaPendidikan = model.NamaPendidikan;

            // kode negara tidak diupdate hanya diganti nama pendidikan saja
            //existingNegara.KodePendidikan = model.KodePendidikan;

            existingNegara.UpdateDateTime = DateTimeOffset.Now;
            existingNegara.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Pendidikans.Update(existingNegara);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
        }

        // DELETE: api/Pendidikan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Pendidikans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Pendidikans.Remove(record);
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
            var searchResults = await _context.Pendidikans
                .Where(n => EF.Functions.Like(n.NamaPendidikan, $"%{keyword}%"))
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
