using System.Dynamic;
using System.Text.Json;
﻿using Microsoft.AspNetCore.Authorization;
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
    public class WilayahController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WilayahController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GeoData/Provinsi
        [HttpGet("Provinsi")]
        public async Task<IActionResult> GetAllProvinsi()
        {
            var records = await _context.Provinsis
                .Select(p => new
                {
                    ProvinsiId = p.ProvinsiId,
                    NamaProvinsi = p.ProvinsiName
                })
                .ToListAsync();

            return records.Any()
                ? Ok(new { message = "Data ditemukan.", data = records })
                : NotFound(new { message = "Tidak ada data ditemukan." });
        }

        // GET: api/GeoData/Kabupaten
        [HttpGet("Kabupaten")]
        public async Task<IActionResult> GetAllKabupaten()
        {

            var records = await _context.KabupatenKotas
                .Select(k => new
                {
                    KabupatenKotaId = k.KabupatenKotaId,
                    KabupatenKotaName = k.KabupatenKotaName,
                    KabupatenKotaCode = k.KabupatenKotaCode
                })
                .ToListAsync();
            return records.Any()
           ? Ok(new { message = "Data ditemukan.", data = records })
           : NotFound(new { message = "Tidak ada data ditemukan." });
        }


        // GET: api/GeoData/Kecamatan
        [HttpGet("Kecamatan")]
        public async Task<IActionResult> GetAllKecamatan()
        {
            //var records = await _context.Kecamatans.Include(k => k.Kabupaten).ToListAsync();
            //return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
            var records = await _context.Kecamatans
                .Select(k => new
                {
                    KecamatanId = k.KecamatanId,
                    KecamatanCode = k.KecamatanCode,
                    KecamatanName = k.KecamatanName,
                })
                .ToListAsync();

            return records.Any()
            ? Ok(new { message = "Data ditemukan.", data = records })
            : NotFound(new { message = "Tidak ada data ditemukan." });


        }

        // GET: api/GeoData/Kelurahan
        [HttpGet("Kelurahan")]
        public async Task<IActionResult> GetAllKelurahan()
        {
            //var records = await _context.Kelurahans.Include(k => k.Kecamatan).ToListAsync();
            //return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
            var records = await _context.Kelurahans
                .Select(k => new
                {
                    KelurahanId = k.KelurahanId,
                    KelurahanCode = k.KelurahanCode,
                    KelurahanName = k.KelurahanName,
                })
                .ToListAsync();
            return records.Any()
            ? Ok(new { message = "Data ditemukan.", data = records })
            : NotFound(new { message = "Tidak ada data ditemukan." });

        }

        // GET: api/GeoData/{model}/{id}
        [HttpGet("{model}/{id}")]
        public async Task<IActionResult> GetById(string model, Guid id)
        {
            if (model == "Provinsi")
            {
                var record = await _context.Provinsis.FindAsync(id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Provinsi dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kabupaten")
            {
                var record = await _context.KabupatenKotas.Include(k => k.Provinsi).FirstOrDefaultAsync(k => k.KabupatenKotaId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kabupaten dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kecamatan")
            {
                var record = await _context.Kecamatans.Include(k => k.Kabupatenkota).FirstOrDefaultAsync(k => k.KecamatanId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kecamatan dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kelurahan")
            {
                var record = await _context.Kelurahans.Include(k => k.Kecamatan).FirstOrDefaultAsync(k => k.KelurahanId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kelurahan dengan ID {id} tidak ditemukan." });
            }

            return BadRequest(new { message = "Model tidak valid." });
        }

        // POST: api/GeoData/Provinsi
        [HttpPost("Provinsi")]
        public async Task<IActionResult> CreateProvinsi([FromBody] ProvinsiViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Provinsis
                .OrderByDescending(k => k.ProvinsiCode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.ProvinsiCode = "PRV" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.ProvinsiCode.Substring(3, 6);
                if (lastCodeTrim != setDateNow)
                {
                    model.ProvinsiCode = "PRV" + setDateNow + "0001";
                }
                else
                {
                    model.ProvinsiCode = "PRV" + setDateNow + (Convert.ToInt32(lastCode.ProvinsiCode.Substring(9)) + 1).ToString("D4");
                }
            }

            if (ModelState.IsValid)
            {
                var prov = new Provinsi
                {
                  ProvinsiId = Guid.NewGuid(),
                  ProvinsiCode = model.ProvinsiCode,
                  ProvinsiName = model.ProvinsiName,
                  NegaraId = model.NegaraId

                };

                var checkDuplicate = _context.Provinsis.Where(c => c.ProvinsiCode == model.ProvinsiCode && c.ProvinsiName == model.ProvinsiName).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Provinsis.Where(c => c.ProvinsiCode == model.ProvinsiCode && c.ProvinsiName == model.ProvinsiName).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Provinsis.Add(prov);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetById), new { model = "Provinsi", id = prov.ProvinsiId }, model);
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
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
            }
            //if (model == null) return BadRequest(new { message = "Data tidak valid." });

            //model.ProvinsiId = Guid.NewGuid();
            //_context.Provinsis.Add(model);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetById), new { model = "Provinsi", id = model.ProvinsiId }, model);
        }

        //POST: api/GeoData/Kabupaten
       [HttpPost("Kabupaten")]
        public async Task<IActionResult> CreateKabupaten([FromBody] KabupatenViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.KabupatenKotas
                .OrderByDescending(k => k.KabupatenKotaCode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KabupatenKotaCode = "KBT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KabupatenKotaCode.Substring(3, 6);
                if (lastCodeTrim != setDateNow)
                {
                    model.KabupatenKotaCode = "KBT" + setDateNow + "0001";
                }
                else
                {
                    model.KabupatenKotaCode = "KBT" + setDateNow + (Convert.ToInt32(lastCode.KabupatenKotaCode.Substring(9)) + 1).ToString("D4");
                }
            }

            if (ModelState.IsValid)
            {
                var kab = new KabupatenKota
                {
                    KabupatenKotaId = Guid.NewGuid(),
                    KabupatenKotaCode = model.KabupatenKotaCode,
                    KabupatenKotaName = model.KabupatenKotaName,
                    ProvinsiId = model.ProvinsiId
                };

                var checkDuplicate = _context.KabupatenKotas.Where(c => c.KabupatenKotaCode == model.KabupatenKotaCode && c.KabupatenKotaName == model.KabupatenKotaName).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.KabupatenKotas.Where(c => c.KabupatenKotaCode == model.KabupatenKotaCode && c.KabupatenKotaName == model.KabupatenKotaName).FirstOrDefault();
                    if (result == null)
                    {
                        _context.KabupatenKotas.Add(kab);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetById), new { model = "Kabupaten", id = kab.KabupatenKotaId }, model);
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
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
            }
            //if (model == null) return BadRequest(new { message = "Data tidak valid." });

            //model.KabupatenId = Guid.NewGuid();
            //_context.Kabupatens.Add(model);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetById), new { model = "Kabupaten", id = model.KabupatenId }, model);
        }

        // POST: api/GeoData/Kecamatan
        [HttpPost("Kecamatan")]
        public async Task<IActionResult> CreateKecamatan([FromBody] KecamatanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Kecamatans
                .OrderByDescending(k => k.KecamatanCode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KecamatanCode = "CMT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KecamatanCode.Substring(3, 6);
                if (lastCodeTrim != setDateNow)
                {
                    model.KecamatanCode = "CMT" + setDateNow + "0001";
                }
                else
                {
                    model.KecamatanCode = "CMT" + setDateNow + (Convert.ToInt32(lastCode.KecamatanCode.Substring(9)) + 1).ToString("D4");
                }
            }

            if (ModelState.IsValid)
            {
                var kec = new Kecamatan
                {
                    KecamatanId = Guid.NewGuid(),
                    KecamatanCode = model.KecamatanCode,
                    KecamatanName = model.KecamatanName,
                    KabupatenKotaId = model.KabupatenId,
                };

                var checkDuplicate = _context.Kecamatans.Where(c => c.KecamatanCode == model.KecamatanCode && c.KecamatanName == model.KecamatanName).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Kecamatans.Where(c => c.KecamatanCode == model.KecamatanCode && c.KecamatanName == model.KecamatanName).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Kecamatans.Add(kec);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAllKecamatan), new { message = "Tambah Data Berhasil || 201 Created" }, model);
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
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
            }
            //if (model == null) return BadRequest(new { message = "Data tidak valid." });

            //model.KecamatanId = Guid.NewGuid();
            //_context.Kecamatans.Add(model);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetById), new { model = "Kecamatan", id = model.KecamatanId }, model);
        }

        // POST: api/GeoData/Kelurahan
        [HttpPost("Kelurahan")]
        public async Task<IActionResult> CreateKelurahan([FromBody] KelurahanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Kelurahans
                .OrderByDescending(k => k.KelurahanCode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KelurahanCode = "KLR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KelurahanCode.Substring(3, 6);
                if (lastCodeTrim != setDateNow)
                {
                    model.KelurahanCode = "KLR" + setDateNow + "0001";
                }
                else
                {
                    model.KelurahanCode = "KLR" + setDateNow + (Convert.ToInt32(lastCode.KelurahanCode.Substring(9)) + 1).ToString("D4");
                }
            }

            if (ModelState.IsValid)
            {
                var kel = new Kelurahan
                {
                    KelurahanId = Guid.NewGuid(),
                    KelurahanCode = model.KelurahanCode,
                    KelurahanName = model.KelurahanName,
                    KecamatanId = model.KecamatanId
                };

                var checkDuplicate = _context.Kelurahans.Where(c => c.KelurahanCode == model.KelurahanCode && c.KelurahanName == model.KelurahanName).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Kelurahans.Where(c => c.KelurahanCode == model.KelurahanCode && c.KelurahanName == model.KelurahanName).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Kelurahans.Add(kel);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAllKelurahan), new { message = "Tambah Data Berhasil || 201 Created" }, model);
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
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
            }
            //if (model == null) return BadRequest(new { message = "Data tidak valid." });

            //model.KelurahanId = Guid.NewGuid();
            //_context.Kelurahans.Add(model);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetById), new { model = "Kelurahan", id = model.KelurahanId }, model);
        }

        // PUT: api/GeoData/{model}/{id}
        [HttpPut("{model}/{id}")]
        public async Task<IActionResult> Update(string model, Guid id, [FromBody] JsonElement requestBody)
        {
            try
            {
                dynamic existingRecord = null;
                // Tentukan model yang akan diupdate
                switch (model.ToLower())
                {
                    case "provinsi":
                        existingRecord = await _context.Provinsis.FindAsync(id);
                        break;
                    case "kabupaten":
                        existingRecord = await _context.KabupatenKotas.FindAsync(id);
                        break;
                    case "kecamatan":
                        existingRecord = await _context.Kecamatans.FindAsync(id);
                        break;
                    case "kelurahan":
                        existingRecord = await _context.Kelurahans.FindAsync(id);
                        break;
                    default:
                        return BadRequest(new { message = "Model tidak valid. Gunakan salah satu dari: provinsi, kabupaten, kecamatan, kelurahan." });
                }

                if (existingRecord == null)
                {
                    return NotFound(new { message = $"{model} dengan ID {id} tidak ditemukan." });
                }

                // Ambil nama dari request body secara case-insensitive
                if (TryGetPropertyIgnoreCase(requestBody, "name", out string newName))
                {
                    // Perbarui nama pada record
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        if (model.ToLower() == "provinsi") existingRecord.ProvinsiName = newName;
                        if (model.ToLower() == "kabupaten") existingRecord.NamaKabupaten = newName;
                        if (model.ToLower() == "kecamatan") existingRecord.NamaKecamatan = newName;
                        if (model.ToLower() == "kelurahan") existingRecord.NamaKelurahan = newName;

                        _context.Update(existingRecord);
                        await _context.SaveChangesAsync();
                        return Ok(new { message = "Nama berhasil diperbarui.", data = existingRecord });
                    }
                    else
                    {
                        return BadRequest(new { message = "Nama tidak boleh kosong." });
                    }
                }

                return BadRequest(new { message = "Request body harus memiliki properti 'name'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
            }
        }

        private bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out string value)
        {
            value = null;

            // Cari properti secara case-insensitive
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        value = property.Value.GetString();
                        return true;
                    }
                }
            }

            return false;
        }


        // DELETE: api/GeoData/{model}/{id}
        [HttpDelete("{model}/{id}")]
        public async Task<IActionResult> Delete(string model, Guid id)
        {
            try
                {
                    dynamic record = null;

                    // Tentukan model yang akan dihapus
                    switch (model.ToLower())
                    {
                        case "provinsi":
                            record = await _context.Provinsis.FindAsync(id);
                            break;
                        case "kabupaten":
                            record = await _context.KabupatenKotas.FindAsync(id);
                            break;
                        case "kecamatan":
                            record = await _context.Kecamatans.FindAsync(id);
                            break;
                        case "kelurahan":
                            record = await _context.Kelurahans.FindAsync(id);
                            break;
                        default:
                            return BadRequest(new { message = "Model tidak valid. Gunakan salah satu dari: provinsi, kabupaten, kecamatan, kelurahan." });
                    }

                    if (record == null)
                    {
                        return NotFound(new { message = $"{model} dengan ID {id} tidak ditemukan." });
                    }

                    // Hapus record dari database
                    _context.Remove(record);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Data berhasil dihapus." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
                }
                //if (model == "Provinsi")
                //{
                //    var record = await _context.Provinsis.FindAsync(id);
                //    if (record == null) return NotFound(new { message = $"Provinsi dengan ID {id} tidak ditemukan." });

                //    _context.Provinsis.Remove(record);
                //    await _context.SaveChangesAsync();
                //    return Ok(new { message = "Data berhasil dihapus." });
                //}

                //// Similar logic for Kabupaten, Kecamatan, and Kelurahan...

                //return BadRequest(new { message = "Model tidak valid." });
        }
    }
}
