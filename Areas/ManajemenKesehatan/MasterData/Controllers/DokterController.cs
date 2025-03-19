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
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using ZXing.QrCode.Internal;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;

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

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DokterController
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

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAllDokter(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _context.Dokters
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreatedDate = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DokterId = a.DokterId,
                            KdDokter = a.KdDokter,
                            NmDokter = a.NmDokter,
                            Sip = a.Sip,
                            Str = a.Str,
                            TglSip = a.TglSip,
                            TglStr = a.TglStr,
                            Nik = a.Nik,
                            Nohp = a.Nohp,
                            Alamat = a.Alamat,
                            IsAsuransi = a.IsAsuransi,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
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

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
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
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
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
                    data.FotoPath = $"/FotoPasienBaru/{fotoFileName}"; // Simpan path relatif
                }

                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;

                _context.Dokters.Update(data);
                _context.SaveChanges();

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
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // Query data
            var query = from a in _context.Dokters
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreatedDate = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DokterId = a.DokterId,
                            KdDokter = a.KdDokter,
                            NmDokter = a.NmDokter,
                            Sip = a.Sip,
                            Str = a.Str,
                            TglSip = a.TglSip,
                            TglStr = a.TglStr,
                            Nik = a.Nik,
                            Nohp = a.Nohp,
                            Alamat = a.Alamat,
                            IsAsuransi = a.IsAsuransi,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.KdDokter.Contains(search) || u.NmDokter.Contains(search) 
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u =>
                    u.CreatedDate.Date >= startDate.Value.Date &&
                    u.CreatedDate.Date <= endDate.Value.Date
                );
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreatedDate.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreatedDate.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreatedDate.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreatedDate.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreatedDate.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreatedDate.Month == today.Month &&
                            u.CreatedDate.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreatedDate.Month == today.Month - 1 &&
                            u.CreatedDate.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreatedDate.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreatedDate.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreatedDate >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreatedDate >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreatedDate),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KdDokter" => query.OrderByDescending(u => u.KdDokter),
                    "NmDokter" => query.OrderByDescending(u => u.NmDokter),
                    _ => query.OrderByDescending(u => u.CreatedDate)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreatedDate),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KdDokter" => query.OrderByDescending(u => u.KdDokter),
                    "NmDokter" => query.OrderByDescending(u => u.NmDokter),
                    _ => query.OrderByDescending(u => u.CreatedDate)
                };

            // Pagination
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
