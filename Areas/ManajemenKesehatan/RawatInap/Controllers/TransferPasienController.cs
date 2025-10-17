using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TransferPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;
        private readonly ILogger<TransferPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TransferPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TransferPasienController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.DarahPermintaans
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BankDarahId,
                             a.KomponenDarahId,
                             a.GolonganDarahId,
                             a.JumlahKantong,
                             a.Rhesus,
                             a.TglPemesanan,
                             a.WaktuPemesanan,
                             a.TglDiperlukan,
                             a.DokterBDRSId,
                             a.DokterPerujukId,
                             a.Petugas,
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
            var listdata = _applicationDbContext.TransferPasiens.Find(id);
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
        [RequestSizeLimit(10_000_000)] // 10 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> Create([FromForm] TransferPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ✅ Cek koneksi ke database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==================================================
                // 🔹 FUNGSI HELPER UPLOAD FILE KE SERVER FLASK
                // ==================================================
                async Task<(string? fileUrl, Guid? ttdId)> UploadTTDAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return (null, null);

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"Ukuran file {prefix} terlalu besar! Maksimal 1MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"Format file {prefix} tidak valid! Gunakan JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{getUserActive.FullName}_{safeTime}_{prefix}{ext}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                { new StreamContent(ms) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) } }, "file", fileName },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload file {prefix} ke server Flask.");

                    var body = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(body);
                    string fileUrl = json.fileUrl;

                    // Simpan ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = fileUrl,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    return (fileUrl, newTTD.TTDId);
                }

                // ==================================================
                // ✅ UPLOAD 3 FILE TTD
                // ==================================================
                var (menyerahkanPath, menyerahkanId) = await UploadTTDAsync(vm.TTDMenyerahkan, "TTDMenyerahkan", "TTDUser");
                var (mengetahuiPath, mengetahuiId) = await UploadTTDAsync(vm.TTDMengetahui, "TTDMengetahui", "TTDUser");
                var (penerimaPath, penerimaId) = await UploadTTDAsync(vm.TTDPenerima, "TTDPenerima", "TTDUser");

                // ==================================================
                // ✅ BUAT DATA TRANSFER PASIEN
                // ==================================================
                var data = new TransferPasien
                {
                    TransferPasienId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    KamarId = vm.KamarId,
                    DiagnosaUtama = vm.DiagnosaUtama,
                    DiagnosaSekunder = vm.DiagnosaSekunder,
                    DokterId1 = vm.DokterId1,
                    DokterId2 = vm.DokterId2,
                    DokterId3 = vm.DokterId3,
                    IndikasiRanap = vm.IndikasiRanap,
                    IsAlergic = vm.IsAlergic ?? false,
                    AlergicOf = vm.AlergicOf,
                    AlasanPindahPasien = vm.AlasanPindahPasien,
                    TglPindah = vm.TglPindah,
                    PengawasanHarianId = vm.PengawasanHarianId,
                    ObservasiCairanId = vm.ObservasiCairanId,
                    IndikatorPengkajianId = vm.IndikatorPengkajianId,
                    PemberianObatId = vm.PemberianObatId,
                    TotalScoreAldrete = vm.TotalScoreAldrete,
                    TotalScoreSteward = vm.TotalScoreSteward,
                    IsICU = vm.IsICU ?? false,
                    BarangDiserahkan = vm.BarangDiserahkan,
                    IntervensiPerawat = vm.IntervensiPerawat,
                    PlanningTindakan = vm.PlanningTindakan,

                    TTDMenyerahkanId = menyerahkanId,
                    TTDMenyerahkanPath = menyerahkanPath,
                    TTDMengetahuiId = mengetahuiId,
                    TTDMengetahuiPath = mengetahuiPath,
                    TTDPenerimaId = penerimaId,
                    TTDPenerimaPath = penerimaPath,

                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.TransferPasiens.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Transfer Pasien Berhasil || 201 Created" });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.TransferPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TransferPasiens.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


    }
}
