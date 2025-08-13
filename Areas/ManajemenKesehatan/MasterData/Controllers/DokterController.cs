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
                    d.Email,
                    d.UserActiveId,
                    d.IsAsuransi,
                    d.IsActive,
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
                .ToList().OrderByDescending(a => a.CreateDateTime);

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
                    Email = _context.UserActives
                        .Where(u => u.FullName == d.NmDokter)
                        .Select(u => u.Email)
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
                    d.IsActive,
                    d.UserActiveId,
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

        // Ga dipake
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

                var dateNow = DateTime.UtcNow; ;
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

                    // 📤 **Kirim foto ke server Python Flask**
                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        // File utama
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
                        }, "file", fotoFileName },

                        // Nama folder tujuan di server Flask
                        { new StringContent("FotoDokter"), "folderTarget" }
                    };

                    // Ganti IP di bawah dengan alamat Python Flask server Anda
                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);

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
                        IsActive = true,
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

                // ✅ Update juga data di tabel UserActives
                var userActive = await _context.UserActives
                    .FirstOrDefaultAsync(u => u.FullName == data.NmDokter && u.Email == data.Email);

                //update data di tabel ApplicationUser
                var userLogin = await _userManager.FindByEmailAsync(data.Email.ToString());
                if (userLogin == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }
                else
                {
                    userLogin.NamaUser = vm.NmDokter;
                    userLogin.Email = vm.Email;
                    userLogin.UserName = vm.Email;
                    userLogin.PhoneNumber = vm.Nohp;
                }
                var result = await _userManager.UpdateAsync(userLogin);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Email telah terdaftar, silakan gunakan email yang berbeda" });
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

                if (userActive != null)
                {
                    userActive.FullName = vm.NmDokter ?? userActive.FullName;
                    userActive.IdentityNumber = vm.Nik ?? userActive.IdentityNumber;
                    userActive.Email = vm.Email ?? userActive.Email;
                    userActive.Address = vm.Alamat ?? userActive.Address;
                    userActive.Handphone = vm.Nohp ?? userActive.Handphone;

                    userActive.UpdateDateTime = DateTimeOffset.UtcNow;
                    userActive.UpdateBy = UserActiveId;

                    _context.UserActives.Update(userActive);
                }

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

                    var fotoFileName = $"{data.KdDokter}{fileExtension}";
                    var oldFileName = data.FotoName ?? "";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        {
                            new StreamContent(ms)
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
                            }, "file", fotoFileName
                        },
                        { new StringContent("FotoDokter"), "folderTarget" },
                        { new StringContent(oldFileName), "oldFileName" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    data.FotoName = fotoFileName;
                    data.FotoPath = $"/FotoDokter/{fotoFileName}";
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
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        //Ambil User ID dari JWT Claims
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        // **Cari Data Dokter**
        //        var data = _context.Dokters.Find(id);
        //        if (data == null)
        //        {
        //            return NotFound(new { message = "Data tidak ditemukan." });
        //        }

        //        // cari data user di table user active
        //        var user = _context.UserActives
        //                .FirstOrDefault(u => u.FullName == data.NmDokter && u.Email == data.Email);
        //        if (user != null)
        //        {
        //            // Hapus data userdokter dari tabel Dokter
        //            user.IsDelete = true;
        //            user.IsActive = false;
        //            user.DeleteBy = UserActiveId;
        //            user.DeleteDateTime = DateTimeOffset.UtcNow;

        //            _context.UserActives.Update(user);
        //            await _context.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            return NotFound(new { message = "User Active Dokter Ini Tidak Ditemukan" });
        //        }

        //        // Hapus user login dari tabel AspNetUsers (permanen)
        //        var userLogin = await _userManager.FindByEmailAsync(data.Email);
        //        if (userLogin != null)
        //        {
        //            var result = await _userManager.DeleteAsync(userLogin);
        //            if (!result.Succeeded)
        //            {
        //                return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
        //            }
        //        }
        //        // **Soft Delete (Tandai Data sebagai Terhapus)**
        //        data.DeleteBy = UserActiveId;
        //        data.DeleteDateTime = DateTimeOffset.UtcNow;
        //        data.IsDelete = true;
        //        data.IsActive = false;

        //        _context.Dokters.Update(data);
        //        _context.SaveChanges();

        //        return Ok(new { message = "Data berhasil dihapus..." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Ambil Email dari JWT Claims
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Ambil User Active yang login
                var userActive = await _context.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (userActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });
                }

                var userActiveId = userActive.UserActiveId;

                // ===== 1. SOFT DELETE DATA DOKTER =====
                var dokter = await _context.Dokters.FindAsync(id);
                if (dokter == null)
                {
                    return NotFound(new { message = "Data Dokter tidak ditemukan." });
                }

                dokter.IsDelete = true;
                dokter.IsActive = false;
                dokter.DeleteBy = userActiveId;
                dokter.DeleteDateTime = DateTimeOffset.UtcNow;
                _context.Dokters.Update(dokter);

                // ===== 2. SOFT DELETE USERACTIVE DOKTER =====
                var user = await _context.UserActives
                    .FirstOrDefaultAsync(u => u.FullName == dokter.NmDokter && u.Email == dokter.Email);
                if (user != null)
                {
                    user.IsDelete = true;
                    user.IsActive = false;
                    user.DeleteBy = userActiveId;
                    user.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.UserActives.Update(user);
                }
                else
                {
                    return NotFound(new { message = "User Active Dokter ini tidak ditemukan." });
                }

                // ===== 3. DELETE USER LOGIN DARI ASPNETUSERS =====
                var userLogin = await _userManager.FindByEmailAsync(dokter.Email);
                if (userLogin != null)
                {
                    var result = await _userManager.DeleteAsync(userLogin);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
                    }
                }

                // ===== 4. SOFT DELETE DOKTER POLI =====
                var dokterPoli = await _context.DokterPolis.FindAsync(id);
                if (dokterPoli != null)
                {
                    dokterPoli.IsDelete = true;
                    dokterPoli.DeleteBy = userActiveId;
                    dokterPoli.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.DokterPolis.Update(dokterPoli);
                }

                // ===== 5. SOFT DELETE DOKTER ASURANSI =====
                var dokterAsuransi = await _context.DokterAsuransis.FindAsync(id);
                if (dokterAsuransi != null)
                {
                    dokterAsuransi.IsDelete = true;
                    dokterAsuransi.DeleteBy = userActiveId;
                    dokterAsuransi.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.DokterAsuransis.Update(dokterAsuransi);
                }

                // Simpan semua perubahan sekaligus
                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus." });
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
        Guid? AsuransiId = null,
        Guid? PoliId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // 1) Base query ke Dokter (belum projection)
            var baseQuery = _context.Dokters
                .Where(d => !d.IsDelete);

            // 2) FILTER: by AsuransiId (GUID)
            if (AsuransiId.HasValue)
            {
                var asu = AsuransiId.Value;
                baseQuery = baseQuery.Where(d =>
                    _context.DokterAsuransis.Any(da => da.DokterId == d.DokterId && da.AsuransiId == asu));
            }

            // 3) FILTER: by PoliId (GUID)
            if (PoliId.HasValue)
            {
                var poli = PoliId.Value;
                baseQuery = baseQuery.Where(d =>
                    _context.DokterPolis.Any(dp => dp.DokterId == d.DokterId && dp.PoliId == poli));
            }

            // 4) SEARCH: kode/nama/email (seperti semula) + (opsional) nama asuransi/poli
            if (!string.IsNullOrWhiteSpace(search))
            {
                string q = $"%{search.ToLower()}%";

                baseQuery = baseQuery.Where(d =>
                    EF.Functions.ILike(d.KdDokter, q) ||
                    EF.Functions.ILike(d.NmDokter, q) ||
                    EF.Functions.ILike(d.Email, q) ||
                    // cari di NAMA ASURANSI (join via DokterAsuransis -> Asuransis)
                    _context.DokterAsuransis
                        .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => new { da.DokterId, a.NamaAsuransi })
                        .Any(x => x.DokterId == d.DokterId && EF.Functions.ILike(x.NamaAsuransi, q)) ||
                    // cari di NAMA POLI (join via DokterPolis -> Polikliniks)
                    _context.DokterPolis
                        .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => new { dp.DokterId, p.NamaPoliklinik })
                        .Any(x => x.DokterId == d.DokterId && EF.Functions.ILike(x.NamaPoliklinik, q))
                );
            }

            // 5) FILTER tanggal & periode (tetap)
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                baseQuery = baseQuery.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Date == today); break;
                    case PeriodeFilter.ThisWeek:
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Date >= weekStart && d.CreateDateTime.Date <= today); break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Date >= lastWeekStart && d.CreateDateTime.Date <= lastWeekEnd); break;
                    case PeriodeFilter.ThisMonth:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Month == today.Month && d.CreateDateTime.Year == today.Year); break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Month == lastMonth.Month && d.CreateDateTime.Year == lastMonth.Year); break;
                    case PeriodeFilter.ThisYear:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Year == today.Year); break;
                    case PeriodeFilter.LastYear:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime.Year == today.Year - 1); break;
                    case PeriodeFilter.Last3Months:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime >= today.AddMonths(-3)); break;
                    case PeriodeFilter.Last6Months:
                        baseQuery = baseQuery.Where(d => d.CreateDateTime >= today.AddMonths(-6)); break;
                }
            }

            // 6) PROJECTION akhir (baru setelah semua filter)
            var query = baseQuery.Select(d => new
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
                d.Email,
                d.UserActiveId,
                d.IsAsuransi,
                d.IsActive,
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

            // 7) SORTING (tetap; kecil catatan: pakai key lowercase biar konsisten)
            query = sortDirection?.ToLower() == "desc"
                ? (orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderByDescending(d => d.CreateDateTime),
                    "createbyname" => query.OrderByDescending(d => d.CreateByName),
                    "Kode Dokter" => query.OrderByDescending(d => d.KdDokter),
                    "Nama Dokter" => query.OrderByDescending(d => d.NmDokter),
                    "Email" => query.OrderByDescending(d => d.Email),
                    _ => query.OrderByDescending(d => d.CreateDateTime)
                })
                : (orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderBy(d => d.CreateDateTime),
                    "createbyname" => query.OrderBy(d => d.CreateByName),
                    "Kode Dokter" => query.OrderBy(d => d.KdDokter),
                    "Nama Dokter" => query.OrderBy(d => d.NmDokter),
                    "Email" => query.OrderBy(d => d.Email),
                    _ => query.OrderBy(d => d.CreateDateTime)
                });

            // 8) Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
                return NotFound(new { message = "Page not found." });

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

        //[HttpGet("paged")]
        //public IActionResult PagedDokter(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null,
        //    Guid? AsuransiId = null,
        //    Guid? PoliId = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null,
        //    [FromQuery] PeriodeFilter? periode = null)
        //{
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // Ambil data dari Dokters yang belum dihapus
        //    var query = _context.Dokters
        //        .Where(d => !d.IsDelete)
        //        .Select(d => new
        //        {
        //            d.CreateDateTime,
        //            d.CreateBy,
        //            CreateByName = _context.UserActives
        //                .Where(u => u.UserActiveId == d.CreateBy)
        //                .Select(u => u.FullName)
        //                .FirstOrDefault(),
        //            d.DokterId,
        //            d.KdDokter,
        //            d.NmDokter,
        //            d.Sip,
        //            d.Str,
        //            d.TglSip,
        //            d.TglStr,
        //            d.Spesialis,
        //            d.Nik,
        //            d.Nohp,
        //            d.Alamat,
        //            d.Email,
        //            d.UserActiveId,
        //            d.IsAsuransi,
        //            d.IsActive,
        //            d.FotoName,
        //            d.FotoPath,
        //            imageUrl = !string.IsNullOrEmpty(d.FotoName)
        //                ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
        //                : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

        //            AsuransiIds = _context.DokterAsuransis
        //                .Where(da => da.DokterId == d.DokterId)
        //                .Select(da => da.AsuransiId)
        //                .Distinct()
        //                .ToList(),

        //            NamaAsuransi = _context.DokterAsuransis
        //                .Where(da => da.DokterId == d.DokterId)
        //                .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => a.NamaAsuransi)
        //                .Distinct()
        //                .ToList(),

        //            PoliIds = _context.DokterPolis
        //                .Where(dp => dp.DokterId == d.DokterId)
        //                .Select(dp => dp.PoliId)
        //                .Distinct()
        //                .ToList(),

        //            NamaPoli = _context.DokterPolis
        //                .Where(dp => dp.DokterId == d.DokterId)
        //                .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => p.NamaPoliklinik)
        //                .Distinct()
        //                .ToList()
        //        });

        //    // Search
        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        string searchLower = search.ToLower();
        //        query = query.Where(d =>
        //            EF.Functions.ILike(d.KdDokter, $"%{searchLower}%") ||
        //            EF.Functions.ILike(d.NmDokter, $"%{searchLower}%") ||
        //            EF.Functions.ILike(d.Email, $"%{searchLower}%") 
        //            );
        //    }

        //    // Filter tanggal
        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //        DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
        //        query = query.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
        //    }

        //    // Filter berdasarkan periode waktu
        //    if (periode.HasValue)
        //    {
        //        DateTime today = DateTime.UtcNow.Date;
        //        switch (periode)
        //        {
        //            case PeriodeFilter.Today:
        //                query = query.Where(d => d.CreateDateTime.Date == today);
        //                break;
        //            case PeriodeFilter.ThisWeek:
        //                var weekStart = today.AddDays(-(int)today.DayOfWeek);
        //                query = query.Where(d => d.CreateDateTime.Date >= weekStart && d.CreateDateTime.Date <= today);
        //                break;
        //            case PeriodeFilter.LastWeek:
        //                var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
        //                var lastWeekEnd = lastWeekStart.AddDays(6);
        //                query = query.Where(d => d.CreateDateTime.Date >= lastWeekStart && d.CreateDateTime.Date <= lastWeekEnd);
        //                break;
        //            case PeriodeFilter.ThisMonth:
        //                query = query.Where(d => d.CreateDateTime.Month == today.Month && d.CreateDateTime.Year == today.Year);
        //                break;
        //            case PeriodeFilter.LastMonth:
        //                var lastMonth = today.AddMonths(-1);
        //                query = query.Where(d => d.CreateDateTime.Month == lastMonth.Month && d.CreateDateTime.Year == lastMonth.Year);
        //                break;
        //            case PeriodeFilter.ThisYear:
        //                query = query.Where(d => d.CreateDateTime.Year == today.Year);
        //                break;
        //            case PeriodeFilter.LastYear:
        //                query = query.Where(d => d.CreateDateTime.Year == today.Year - 1);
        //                break;
        //            case PeriodeFilter.Last3Months:
        //                query = query.Where(d => d.CreateDateTime >= today.AddMonths(-3));
        //                break;
        //            case PeriodeFilter.Last6Months:
        //                query = query.Where(d => d.CreateDateTime >= today.AddMonths(-6));
        //                break;
        //        }
        //    }

        //    // Sorting
        //    query = sortDirection?.ToLower() == "desc"
        //        ? orderBy?.ToLower() switch
        //        {
        //            "createdatetime" => query.OrderByDescending(d => d.CreateDateTime),
        //            "createbyname" => query.OrderByDescending(d => d.CreateByName),
        //            "Kode Dokter" => query.OrderByDescending(d => d.KdDokter),
        //            "Nama Dokter" => query.OrderByDescending(d => d.NmDokter),
        //            "Email" => query.OrderByDescending(d => d.Email),
        //            _ => query.OrderByDescending(d => d.CreateDateTime)
        //        }
        //        : orderBy?.ToLower() switch
        //        {
        //            "createdatetime" => query.OrderBy(d => d.CreateDateTime),
        //            "createbyname" => query.OrderBy(d => d.CreateByName),
        //            "Kode Dokter" => query.OrderBy(d => d.KdDokter),
        //            "Nama Dokter" => query.OrderBy(d => d.NmDokter),
        //            "Email" => query.OrderBy(d => d.Email),
        //            _ => query.OrderBy(d => d.CreateDateTime)
        //        };

        //    // pagination
        //    var totalRows = query.Count();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
        //    var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

        //    if (rows.Count == 0 && page > totalPages)
        //    {
        //        return NotFound(new { message = "Page not found." });
        //    }
        //    return Ok(new
        //    {
        //        status = "success",
        //        message = "Data retrieved successfully",
        //        data = new
        //        {
        //            Rows = rows,
        //            TotalRows = totalRows,
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalPages = totalPages
        //        }
        //    });
        //}
    }
}
