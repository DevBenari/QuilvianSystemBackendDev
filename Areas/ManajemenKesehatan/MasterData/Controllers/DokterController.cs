using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using ZXing.QrCode.Internal;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Migrations;
using Microsoft.AspNetCore.Components.Routing;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DokterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DokterController
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DokterController> logger,
            IWebHostEnvironment webHostEnvironment


            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAllDokter(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = _context.Dokters
                .Where(d => !d.IsDelete)
                .Select(d => new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = _context.UserActives
                        .Where(u => u.UserActiveId == d.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.IsAsuransi,
                    d.FotoName,
                    d.FotoPath,
                    imageUrl = !string.IsNullOrEmpty(d.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",
                    // Menambahkan daftar ID Asuransi
                    AsuransiIds = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Select(da => da.AsuransiId)
                        .Distinct()
                        .ToList(),

                    NamaAsuransi = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => a.NamaAsuransi)
                        .Distinct()
                        .ToList(),

                                // Menambahkan daftar ID Poli
                    PoliIds = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Select(dp => dp.PoliId)
                        .Distinct()
                        .ToList(),

                    NamaPoli = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => p.NamaPoliklinik)
                        .Distinct()
                        .ToList()
                })
                .ToList();

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

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDokterById(Guid id)
        {
            var dokter = await _context.Dokters
                .Where(d => !d.IsDelete && d.DokterId == id)
                .Select(d => new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = _context.UserActives
                        .Where(u => u.UserActiveId == d.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Spesialis,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.IsAsuransi,
                    d.FotoName,
                    d.FotoPath,
                    imageUrl = !string.IsNullOrEmpty(d.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                    // Menambahkan daftar ID Asuransi
                    AsuransiIds = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Select(da => da.AsuransiId)
                        .Distinct()
                        .ToList(),

                    NamaAsuransi = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => a.NamaAsuransi)
                        .Distinct()
                        .ToList(),

                    // Menambahkan daftar ID Poli
                    PoliIds = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Select(dp => dp.PoliId)
                        .Distinct()
                        .ToList(),

                    NamaPoli = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => p.NamaPoliklinik)
                        .Distinct()
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (dokter == null)
            {
                return NotFound(new { message = $"Dokter dengan ID {id} tidak ditemukan || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil mengambil data dokter || 200 OK",
                data = dokter
            });
        }


        [HttpGet("get-image/{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var fotoPath = _context.Dokters
                .Where(p => p.DokterId == id)
                .Select(p => p.FotoPath)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fotoPath))
            {
                return NotFound(new { message = "Foto tidak ditemukan." });
            }

            // Pastikan path lengkap menggunakan wwwroot
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, fotoPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "File tidak ditemukan di server." });
            }

            var image = System.IO.File.OpenRead(fullPath);
            var contentType = GetContentType(fullPath);
            return File(image, contentType);
        }

        // Fungsi untuk mendapatkan MIME Type
        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" }
        };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }


        // POST: api/Dokter
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DokterViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
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

                var dateNow = DateTime.UtcNow;;
                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.Dokters
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KdDokter)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"DKR{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KdDokter.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"DKR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"DKR{setDateNow}" + (Convert.ToInt32(lastCode.KdDokter.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _context.Dokters
                    .Any(c => c.KdDokter == kode && c.NmDokter == vm.NmDokter);

                // **Validasi & Simpan Foto Profil**
                string fotoPath = null;
                string fotoFileName = null;
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    fotoFileName = $"{kode}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                    {
                        vm.Foto.CopyTo(stream);
                    }

                    fotoPath = $"/FotoDokter/{fotoFileName}";
                }
                else
                {
                    //Jika user tidak upload foto, gunakan foto default
                    fotoPath = "/FotoDokter/dokter.jpg";
                    fotoFileName = "dokter.jpg";
                }

                if (ModelState.IsValid)
                {
                    var dokter = new Dokter
                    {
                        DokterId = Guid.NewGuid(),
                        NmDokter = vm.NmDokter,
                        Sip = vm.Sip,
                        Str = vm.Str,
                        TglSip = vm.TglSip,
                        TglStr = vm.TglStr,
                        FotoPath = fotoPath,
                        FotoName = fotoFileName,
                        Spesialis = vm.Spesialis,
                        Nik = vm.Nik,
                        KdDokter = kode,
                        Email = vm.Email,
                        Nohp = vm.Nohp,
                        Alamat = vm.Alamat,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false,
                        IsAsuransi = vm.IsAsuransi,
                    };
                    _context.Dokters.Add(dokter);
                    _context.SaveChanges();

                    if (vm.AsuransiId != null && vm.AsuransiId.Any())
                    {
                        var dokterAsuransiList = vm.AsuransiId.Select(asuransiId => new DokterAsuransi
                        {
                            DokterAsuransiId = Guid.NewGuid(),
                            DokterId = dokter.DokterId, // Gunakan ID dokter yang baru dibuat
                            AsuransiId = asuransiId, // Ambil ID asuransi dari list
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false,
                        }).ToList();

                        _context.DokterAsuransis.AddRange(dokterAsuransiList);
                        await _context.SaveChangesAsync();
                    }

                    if (vm.PoliId != null && vm.PoliId.Any())
                    {
                        var dokterPoliList = vm.PoliId.Select(poliId => new DokterPoli
                        {
                            DokterPoliId = Guid.NewGuid(),
                            DokterId = dokter.DokterId, // Gunakan ID dokter yang baru dibuat
                            PoliId = poliId, // Ambil ID Poli dari list
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false,
                        }).ToList();

                        _context.DokterPolis.AddRange(dokterPoliList);
                        await _context.SaveChangesAsync();
                    }

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        //uploadFotoUrl = fotoPath != null ? $"{Request.Scheme}://{Request.Host}{fotoPath}" : null
                    });

                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/Dokter/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] DokterViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid."});
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

                // cari data
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.NmDokter = vm.NmDokter ?? data.NmDokter;
                data.Sip = vm.Sip ?? data.Sip;
                data.Str = vm.Str ?? data.Str;
                data.TglSip = vm.TglSip ?? data.TglSip;
                data.TglStr = vm.TglStr ?? data.TglStr;
                data.Nik = vm.Nik ?? data.Nik;
                data.Email = vm.Email ?? data.Email;
                data.Nohp = vm.Nohp ?? data.Nohp;
                data.Alamat = vm.Alamat ?? data.Alamat;
                data.Spesialis = vm.Spesialis ?? data.Spesialis;
                data.IsAsuransi = vm.IsAsuransi ?? data.IsAsuransi;

                // **Update Foto Profil Jika Ada**
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024; // Maksimum 2MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    // **Hapus Foto Lama Jika Ada**
                    if (!string.IsNullOrEmpty(data.FotoPath) && !data.FotoPath.Contains("dokter.jpg"))
                    {
                        var oldFotoPath = Path.Combine(_webHostEnvironment.WebRootPath, data.FotoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFotoPath))
                        {
                            System.IO.File.Delete(oldFotoPath);
                        }
                    }

                    // **Simpan Foto Baru**
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fotoFileName = $"{data.KdDokter}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                    {
                        await vm.Foto.CopyToAsync(stream);
                    }

                    data.FotoName = fotoFileName;
                    data.FotoPath = $"/FotoDokter/{fotoFileName}"; // Simpan path relatif
                }

                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;

                _context.Dokters.Update(data);
                _context.SaveChanges();

                // **Asuransi**
                var asuransiLama = await _context.DokterAsuransis
                    .Where(da => da.DokterId == data.DokterId)
                    .ToListAsync();

                if (vm.AsuransiId == null || !vm.AsuransiId.Any())
                {
                    // Jika daftar asuransi baru kosong, hapus semua asuransi lama
                    _context.DokterAsuransis.RemoveRange(asuransiLama);
                }
                else
                {
                    // Hapus Asuransi Lama yang Tidak Ada dalam Daftar Baru
                    var asuransiYangDihapus = asuransiLama
                        .Where(da => !vm.AsuransiId.Contains(da.AsuransiId))
                        .ToList();

                    _context.DokterAsuransis.RemoveRange(asuransiYangDihapus);

                    // Tambahkan Asuransi Baru yang Belum Ada
                    var asuransiBaru = vm.AsuransiId
                        .Where(asuransiId => !asuransiLama.Any(da => da.AsuransiId == asuransiId))
                        .Select(asuransiId => new DokterAsuransi
                        {
                            DokterAsuransiId = Guid.NewGuid(),
                            DokterId = data.DokterId,
                            AsuransiId = asuransiId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false
                        })
                        .ToList();

                    _context.DokterAsuransis.AddRange(asuransiBaru);
                }

                // **Poli**
                var poliLama = await _context.DokterPolis
                    .Where(dp => dp.DokterId == data.DokterId)
                    .ToListAsync();

                if (vm.PoliId == null || !vm.PoliId.Any())
                {
                    // Jika daftar poli baru kosong, hapus semua poli lama
                    _context.DokterPolis.RemoveRange(poliLama);
                }
                else
                {
                    // Hapus Poli Lama yang Tidak Ada dalam Daftar Baru
                    var poliYangDihapus = poliLama
                        .Where(dp => !vm.PoliId.Contains(dp.PoliId))
                        .ToList();

                    _context.DokterPolis.RemoveRange(poliYangDihapus);

                    // Tambahkan Poli Baru yang Belum Ada
                    var poliBaru = vm.PoliId
                        .Where(poliId => !poliLama.Any(dp => dp.PoliId == poliId))
                        .Select(poliId => new DokterPoli
                        {
                            DokterPoliId = Guid.NewGuid(),
                            DokterId = data.DokterId,
                            PoliId = poliId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false
                        })
                        .ToList();

                    _context.DokterPolis.AddRange(poliBaru);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil diupdate..." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/Dokter/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Dokter**
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _context.Dokters.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public IActionResult PagedDokter(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Ambil data dari Dokters yang belum dihapus
            var query = _context.Dokters
                .Where(d => !d.IsDelete)
                .Select(d => new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = _context.UserActives
                        .Where(u => u.UserActiveId == d.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Spesialis,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.IsAsuransi,
                    d.FotoName,
                    d.FotoPath,
                    imageUrl = !string.IsNullOrEmpty(d.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                    AsuransiIds = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Select(da => da.AsuransiId)
                        .Distinct()
                        .ToList(),

                    NamaAsuransi = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => a.NamaAsuransi)
                        .Distinct()
                        .ToList(),

                    PoliIds = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Select(dp => dp.PoliId)
                        .Distinct()
                        .ToList(),

                    NamaPoli = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => p.NamaPoliklinik)
                        .Distinct()
                        .ToList()
                });

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(d =>
                    EF.Functions.ILike(d.KdDokter, $"%{searchLower}%") ||
                    EF.Functions.ILike(d.NmDokter, $"%{searchLower}%"));
            }

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode waktu
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(d => d.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(d => d.CreateDateTime.Date >= weekStart && d.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(d => d.CreateDateTime.Date >= lastWeekStart && d.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(d => d.CreateDateTime.Month == today.Month && d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(d => d.CreateDateTime.Month == lastMonth.Month && d.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(d => d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(d => d.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(d => d.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(d => d.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderByDescending(d => d.CreateDateTime),
                    "createbyname" => query.OrderByDescending(d => d.CreateByName),
                    "kddokter" => query.OrderByDescending(d => d.KdDokter),
                    "nmdokter" => query.OrderByDescending(d => d.NmDokter),
                    _ => query.OrderByDescending(d => d.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderBy(d => d.CreateDateTime),
                    "createbyname" => query.OrderBy(d => d.CreateByName),
                    "kddokter" => query.OrderBy(d => d.KdDokter),
                    "nmdokter" => query.OrderBy(d => d.NmDokter),
                    _ => query.OrderBy(d => d.CreateDateTime)
                };

            // pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
