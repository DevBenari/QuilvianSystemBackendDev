using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Migrations;
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
                            FotoDokter = a.FotoDokter,
                            ImageBytes = a.ImageBytes,
                            Nik = a.Nik,
                            Nohp = a.Nohp,
                            Alamat = a.Alamat,
                            IsAsuransi = a.IsAsuransi,
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
            var data = await _context.Dokters.FindAsync(id);

            if (data == null || data.ImageBytes == null || data.ImageBytes.Length == 0)
            {
                return NotFound(new { message = "Data tidak ditemukan atau tidak memiliki gambar." });
            }

            string detectedFormat = GetImageFormat(data.ImageBytes);
            string mimeType = detectedFormat == "image/png" ? "image/png" : "image/jpeg";

            return File(data.ImageBytes, mimeType); // Mengembalikan gambar dengan format yang sesuai
        }

        public static string GetImageFormat(byte[] fileBytes)
        {
            if (fileBytes.Length < 4) return "Unknown";

            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF) return "image/jpeg";
            if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47) return "image/png";

            return "Unknown";
        }

        // POST: api/Dokter
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DokterViewModel model)
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
                    .Any(c => c.KdDokter == kode && c.NmDokter == model.NmDokter);

                // kode upload gambar dgn Base64
                // Dapatkan ekstensi file berdasarkan Base64
                //string extension = GetImageExtension(request.Base64Data);
                //if (string.IsNullOrEmpty(extension))
                //{
                //    return BadRequest(new { message = "Invalid image format. Allowed formats: jpg, jpeg, png, gif, bmp, webp." });
                //}

                //// Folder penyimpanan (wwwroot/uploads)
                //var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                //if (!Directory.Exists(uploadsFolder))
                //{
                //    Directory.CreateDirectory(uploadsFolder);
                //}

                //// Nama file unik
                //var fileName = $"image_{Guid.NewGuid()}.{extension}";
                //var filePath = Path.Combine(uploadsFolder, fileName);

                //// Hapus prefix base64 sebelum decoding
                //var base64Data = Regex.Replace(request.Base64Data, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

                //// Konversi base64 menjadi byte array
                //var imageBytes = Convert.FromBase64String(base64Data);

                //// Simpan file ke server
                //await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                //// URL file yang disimpan
                //var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

                //// **Validasi & Simpan Foto Profil**
                //string fotoPath = null;
                //if (model.FotoDokter != null && model.FotoDokter.Length > 0)
                //{
                //    var maxSize = 2 * 1024 * 1024;
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                //    var fileExtension = Path.GetExtension(model.FotoDokter.FileName).ToLower();

                //    if (model.FotoDokter.Length > maxSize)
                //    {
                //        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                //    }

                //    if (!allowedExtensions.Contains(fileExtension))
                //    {
                //        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                //    }

                //    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
                //    if (!Directory.Exists(uploadFolder))
                //    {
                //        Directory.CreateDirectory(uploadFolder);
                //    }

                //    var fotoFileName = $"{kode}{fileExtension}";
                //    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                //    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                //    {
                //        model.FotoDokter.CopyTo(stream);
                //    }

                //    fotoPath = $"/FotoDokter/{fotoFileName}";
                //}
                //else
                //{
                //    //Jika user tidak upload foto, gunakan foto default
                //    fotoPath = "/FotoDokter/dokter.jpg";
                //}

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                byte[] imageBytes = model.FotoByte.ToArray();
                string detectedFormat = GetImageFormat(imageBytes);

                if (detectedFormat == "Unknown")
                {
                    return BadRequest(new { message = "Format gambar tidak didukung. Hanya menerima JPG dan PNG." });
                }

                string uniqueFileName = $"{kode}_{model.NmDokter}";


                if (ModelState.IsValid)
                {
                    var dokter = new Dokter
                    {
                        DokterId = Guid.NewGuid(),
                        NmDokter = model.NmDokter,
                        Sip = model.Sip,
                        Str = model.Str,
                        TglSip = model.TglSip,
                        TglStr = model.TglStr,
                        FotoPath = $"/FotoDokter/{uniqueFileName}",
                        ImageBytes = imageBytes,
                        FotoDokter = uniqueFileName,
                        Nik = model.Nik,
                        KdDokter = kode,
                        Email = model.Email,
                        Nohp = model.Nohp,
                        Alamat = model.Alamat,
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = UserActiveId,
                        IsDelete = false,
                        IsAsuransi = model.IsAsuransi,
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
        public async Task<IActionResult> Update(Guid id, [FromForm] DokterViewModel model)
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

                // cari data
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.NmDokter = model.NmDokter ?? data.NmDokter;
                data.Sip = model.Sip ?? data.Sip;
                data.Str = model.Str ?? data.Str;
                data.TglSip = model.TglSip ?? data.TglSip;
                data.TglStr = model.TglStr ?? data.TglStr;
                data.Nik = model.Nik ?? data.Nik;
                data.IsAsuransi = model.IsAsuransi ?? data.IsAsuransi;

                byte[] imageBytes = model.FotoByte.ToArray();
                string detectedFormat = GetImageFormat(imageBytes);

                if (detectedFormat == "Unknown")
                {
                    return BadRequest(new { message = "Format gambar tidak didukung. Hanya menerima JPG dan PNG." });
                }

                data.ImageBytes = imageBytes;



                // update foto
                //if (model.FotoDokter != null && model.FotoDokter.Length > 0)
                //{
                //    var maxSize = 2 * 1024 * 1024;
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                //    var fileExtension = Path.GetExtension(model.FotoDokter.FileName).ToLower();
                //    if (model.FotoDokter.Length > maxSize)
                //    {
                //        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                //    }
                //    if (!allowedExtensions.Contains(fileExtension))
                //    {
                //        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                //    }
                //    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
                //    if (!Directory.Exists(uploadFolder))
                //    {
                //        Directory.CreateDirectory(uploadFolder);
                //    }
                //    var fotoFileName = $"{data.KdDokter}{fileExtension}";
                //    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);
                //    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                //    {
                //        model.FotoDokter.CopyTo(stream);
                //    }
                //    data.FotoDokter = $"/FotoDokter/{fotoFileName}";
                //}

                data.UpdateDateTime = DateTimeOffset.Now;
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
                data.DeleteDateTime = DateTimeOffset.Now;
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


        // Pagination
        [HttpGet("paged")]
        public IActionResult PagedDokter(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "asc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "StartDate tidak boleh lebih besar dari EndDate." });
            }

            var query = _context.Dokters.Where(a => a.IsDelete == false).AsQueryable();

            // 🔍 Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.KdDokter.Contains(search) ||
                                         u.NmDokter.Contains(search) ||
                                         u.Str.Contains(search));
            }

            // 📅 Filter berdasarkan daterange
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u => u.CreateDateTime.Date >= startDate.Value.Date &&
                                         u.CreateDateTime.Date <= endDate.Value.Date);
            }



            // Sorting Data
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, orderBy))
                    : query.OrderBy(e => EF.Property<object>(e, orderBy));
            }

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
