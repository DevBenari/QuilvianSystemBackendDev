using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Migrations;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AsuransiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsuransiController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/Asuransi
        [HttpGet]
        public async Task<IActionResult> GetAllAsuransi()
        {
            var records = await _context.Asuransis.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }
      

        // GET: api/Asuransi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsuransiById(Guid id)
        {
            var records = await _context.Asuransis.ToListAsync();
            if (records == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // POST: api/Asuransi
        [HttpPost]
        public async Task<IActionResult>AddAsuransi([FromBody] AsuransiViewModel newAsuransi)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Asuransis
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KodeAsuransi)
                .FirstOrDefault();

            if (lastCode == null)
            {
                newAsuransi.KodeAsuransi = "ASR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KodeAsuransi.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    newAsuransi.KodeAsuransi = "ASR" + setDateNow + "0001";
                }
                else
                {
                    newAsuransi.KodeAsuransi = "ASR" + setDateNow +
                        (Convert.ToInt32(lastCode.KodeAsuransi.Substring(9)) + 1).ToString("D4");
                }
            }

            //VALIDATE MODELSTATE
            if (ModelState.IsValid)
            {
                var asuransi = new Asuransi
                {
                    AsuransiId = Guid.NewGuid(),
                    KodeAsuransi = newAsuransi.KodeAsuransi,
                    NamaAsuransi = newAsuransi.NamaAsuransi,
                    TipePerusahaan = newAsuransi.TipePerusahaan,
                    Status = newAsuransi.Status,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };

                //check duplikasi
                var checkDuplicate = _context.Asuransis.Where(c => c.KodeAsuransi == newAsuransi.KodeAsuransi && c.NamaAsuransi == newAsuransi.NamaAsuransi).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Asuransis.Where(c => c.KodeAsuransi == newAsuransi.KodeAsuransi && c.NamaAsuransi == newAsuransi.NamaAsuransi).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Asuransis.Add(asuransi);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAllAsuransi), new { message = "Tambah Data Berhasil || 201 Created" }, newAsuransi);
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
        

        // PUT: api/Asuransi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsuransi(Guid id, [FromBody] AsuransiViewModel updatedAsuransi)
        {
            //cek apakah data ada di database
            var existingNegara = await _context.Asuransis.FindAsync(id);
            if (existingNegara == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Asuransis.Where
                (c => c.KodeAsuransi == updatedAsuransi.KodeAsuransi && c.NamaAsuransi == updatedAsuransi.NamaAsuransi
                && c.AsuransiId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            existingNegara.NamaAsuransi = updatedAsuransi.NamaAsuransi;
            existingNegara.TipePerusahaan = updatedAsuransi.TipePerusahaan;
            existingNegara.Status = updatedAsuransi.Status;
            

           

            existingNegara.UpdateDateTime = DateTimeOffset.Now;
            existingNegara.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Asuransis.Update(existingNegara);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAllAsuransi), new { message = "Tambah Data Berhasil || 201 Created" }, updatedAsuransi);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
        }

        // DELETE: api/Asuransi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsuransi(Guid id)
        {
            var record = await _context.Asuransis.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Asuransis.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.Asuransis
                .Where(n => EF.Functions.Like(n.NamaAsuransi, $"%{keyword}%") || EF.Functions.Like(n.KodeAsuransi, $"%{keyword}%"))
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
