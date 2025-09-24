using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObservasiCairanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ObservasiCairanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObservasiCairanController(
            ApplicationDbContext applicationDbContext, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ILogger<ObservasiCairanController> logger, 
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.ObservasiCairans
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.ObservasiCairanId,
                             a.KunjunganId,
                             a.PasienId,
                             a.UserActivePerawatId,
                             a.TglObservasi,
                             a.CairanMasuk,
                             a.CairanSisa,
                             a.JumlahUrin,
                             a.TTDId,
                             a.TTDPath,
                             a.Keterangan,

                         }).OrderByDescending(a => a.CreateDateTime);

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.ObservasiCairans.Find(id);
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ObservasiCairanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                ////// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // ==================================================
                // ✅ PROSES UPLOAD TTD
                // ==================================================
                Guid ttdId;
                string ttdPath;

                if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                {
                    var maxSize = 1 * 1024 * 1024; // max 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                    if (vm.TTDFile.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                    // Nama file unik
                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_CttESO{fileExtension}";

                    // 📤 Upload ke Flask
                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.TTDFile.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                        }, "file", ttdFileName },

                        { new StringContent("TTDUser"), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);

                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                    // Ambil URL/path hasil upload dari response Flask
                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    // Anggap Flask balikin JSON {"fileUrl": "/uploads/TTDUser/namafile.jpg"}
                    dynamic jsonResp = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                    ttdPath = jsonResp.fileUrl;

                    // Simpan ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = ttdPath, // langsung pakai path dari Flask
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();
                    ttdId = newTTD.TTDId;
                }
                else
                {
                    return BadRequest(new { message = "TTD harus diisi." });
                }

                // **Buat Data Baru**
                var data = new ObservasiCairan
                {
                    ObservasiCairanId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    UserActivePerawatId = vm.UserActivePerawatId,
                    TglObservasi = DateTime.UtcNow,
                    CairanMasuk =vm.CairanMasuk,
                    CairanKeluar = vm.CairanKeluar,
                    CairanSisa = vm.CairanSisa,
                    JumlahUrin = vm.JumlahUrin,



                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.ObservasiCairans.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
