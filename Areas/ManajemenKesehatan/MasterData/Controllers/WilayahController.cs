using System.Dynamic;
using System.Security.Claims;
using System.Text.Json;
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class WilayahController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WilayahController
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PendaftaranPasienBaruController> logger,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/GeoData/Provinsi
        [HttpGet("Provinsi")]
        public async Task<IActionResult> GetAllProvinsi(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from p in _context.Provinsis
                          select new
                          {
                            ProvinsiId = p.ProvinsiId,
                            NamaProvinsi = p.ProvinsiName,
                            NegaraId = p.NegaraId
                          };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        // GET: api/GeoData/Kabupaten
        [HttpGet("Kabupaten")]
        public async Task<IActionResult> GetAllKabupaten(int page = 1, int perPage = 10)
        {

            var query = from k in _context.KabupatenKotas
                        select new
                        {
                            KabupatenKotaId = k.KabupatenKotaId,
                            KabupatenKotaName = k.KabupatenKotaName,
                            KabupatenKotaCode = k.KabupatenKotaCode,
                            ProvinsiId = k.ProvinsiId
                        };
            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });

        }


        // GET: api/GeoData/Kecamatan
        [HttpGet("Kecamatan")]
        public async Task<IActionResult> GetAllKecamatan(int page = 1, int perPage = 10)
        {
            var query = from k in _context.Kecamatans
                select new
                {
                    KecamatanId = k.KecamatanId,
                    KecamatanCode = k.KecamatanCode,
                    KecamatanName = k.KecamatanName,
                    KabupatenKotaId = k.KabupatenKotaId
                };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });

        }

        // GET: api/GeoData/Kelurahan
        [HttpGet("Kelurahan")]
        public async Task<IActionResult> GetAllKelurahan(int page = 1, int perPage = 10)
        {
            //var records = await _context.Kelurahans.Include(k => k.Kecamatan).ToListAsync();
            //return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
            var query = from k in _context.Kelurahans
                        select new
                        {
                            KelurahanId = k.KelurahanId,
                            KelurahanCode = k.KelurahanCode,
                            KelurahanName = k.KelurahanName,
                            KecamatanId = k.KecamatanId
                        };
            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });

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
                var record = await _context.KabupatenKotas.Include(k => k.ProvinsiId).FirstOrDefaultAsync(k => k.KabupatenKotaId == id);

                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kabupaten dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kecamatan")
            {
                var record = await _context.Kecamatans.Include(k => k.Kabupatenkota).FirstOrDefaultAsync(k => k.KecamatanId == id);

                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kecamatan dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kelurahan")
            {
                var record = await _context.Kelurahans.FirstOrDefaultAsync(k => k.KelurahanId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kelurahan dengan ID {id} tidak ditemukan." });
            }

            return BadRequest(new { message = "Model tidak valid." });
        }

        // POST: api/GeoData/Provinsi
        [HttpPost("Provinsi")]
        public async Task<IActionResult> CreateProvinsi([FromBody] ProvinsiViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                var dateNow = DateTimeOffset.Now;


                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.Provinsis
                    .OrderByDescending(k => k.ProvinsiCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PRV{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.ProvinsiCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PRV{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PRV{setDateNow}" + (Convert.ToInt32(lastCode.ProvinsiCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                var isDuplicate = _context.Provinsis
                    .Any(c => c.ProvinsiCode == kode && c.ProvinsiName == model.ProvinsiName);
                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                if (ModelState.IsValid)
                {
                    var prov = new Provinsi
                    {
                        ProvinsiId = Guid.NewGuid(),
                        ProvinsiCode = kode,
                        ProvinsiName = model.ProvinsiName,
                        NegaraId = model.NegaraId

                    };
                    _context.Provinsis.Add(prov);
                    _context.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
            }
        }

        //POST: api/GeoData/Kabupaten
       [HttpPost("Kabupaten")]
        public async Task<IActionResult> CreateKabupaten([FromBody] KabupatenViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                var dateNow = DateTimeOffset.Now;
                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.KabupatenKotas
                    .OrderByDescending(k => k.KabupatenKotaCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KTA{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KabupatenKotaCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KTA{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KTA{setDateNow}" + (Convert.ToInt32(lastCode.KabupatenKotaCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                var isDuplicate = _context.KabupatenKotas
                    .Any(c => c.KabupatenKotaCode == kode && c.KabupatenKotaName == model.KabupatenKotaName);
                
                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                if (ModelState.IsValid)
                {
                    var kab = new KabupatenKota
                    {
                        KabupatenKotaId = Guid.NewGuid(),
                        KabupatenKotaCode = kode,
                        KabupatenKotaName = model.KabupatenKotaName,
                        ProvinsiId = model.ProvinsiId
                    };
                    _context.KabupatenKotas.Add(kab);
                    _context.SaveChanges();
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
            }
        }

        // POST: api/GeoData/Kecamatan
        [HttpPost("Kecamatan")]
        public async Task<IActionResult> CreateKecamatan([FromBody] KecamatanViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTimeOffset.Now;
                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.Kecamatans
                    .OrderByDescending(k => k.KecamatanCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KCM{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KecamatanCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KCM{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KCM{setDateNow}" + (Convert.ToInt32(lastCode.KecamatanCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                //cek duplicate data
                var isDuplicate = _context.Kecamatans
                    .Any(c => c.KecamatanCode == kode && c.KecamatanName == model.KecamatanName);
                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });

                }

                if (ModelState.IsValid)
                {
                    var kec = new Kecamatan
                    {
                        KecamatanId = Guid.NewGuid(),
                        KecamatanCode = kode,
                        KecamatanName = model.KecamatanName,
                        KabupatenKotaId = model.KabupatenId,
                    };
                    _context.Kecamatans.Add(kec);
                    _context.SaveChanges();
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
            }
        }

        // POST: api/GeoData/Kelurahan
        [HttpPost("Kelurahan")]
        public async Task<IActionResult> CreateKelurahan([FromBody] KelurahanViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTimeOffset.Now;
                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.Kelurahans
                    .OrderByDescending(k => k.KelurahanCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KLR{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KelurahanCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KLR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KLR{setDateNow}" + (Convert.ToInt32(lastCode.KelurahanCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                //cek duplicate data
                var isDuplicate = _context.Kelurahans
                    .Any(c => c.KelurahanCode == kode && c.KelurahanName == model.KelurahanName);
                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });

                }

                if (ModelState.IsValid)
                {
                    var kel = new Kelurahan
                    {
                        KelurahanId = Guid.NewGuid(),
                        KelurahanCode = kode,
                        KelurahanName = model.KelurahanName,
                        KecamatanId = model.KecamatanId
                    };
                    _context.Kelurahans.Add(kel);
                    _context.SaveChanges();
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", detail = ex.Message });
            }
        }

        //: api/GeoData/{model}/{id}
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
