using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsuransiPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AsuransiPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AsuransiPasienController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<AsuransiPasienController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAsuransiPasien()
        {
            var result = await (from ap in _applicationDbContext.AsuransiPasiens
                                join p in _applicationDbContext.PendaftaranPasienBarus on ap.PasienId equals p.PendaftaranPasienBaruId.ToString()
                                join a in _applicationDbContext.Asuransis on ap.AsuransiId equals a.AsuransiId.ToString()
                                select new
                                {
                                    ap.PasienId,
                                    ap.AsuransiId,
                                    NamaPasien = p.NamaLengkap,
                                    NamaAsuransi = a.NamaAsuransi,
                                    ap.NoPolis
                                }).ToListAsync();

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsuransiPasienById(Guid id)
        {
            var listdata = _applicationDbContext.AsuransiPasiens.Find(id);
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
        public async Task<IActionResult> CreateAsuransiPasien([FromBody] AsuransiPasien request)
        {
            if (request == null || string.IsNullOrEmpty(request.PasienId) || string.IsNullOrEmpty(request.AsuransiId))
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            // Periksa apakah pasien dan asuransi ada di database
            var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                                  .Any(p => p.PendaftaranPasienBaruId.ToString() == request.PasienId);

            var asuransiExists = _applicationDbContext.Asuransis
                                  .Any(a => a.AsuransiId.ToString() == request.AsuransiId);

            if (!pasienExists || !asuransiExists)
            {
                return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
            }

            // Membuat objek baru untuk ditambahkan ke database
            var newAsuransiPasien = new AsuransiPasien
            {
                AsuransiPasienId = Guid.NewGuid(),
                PasienId = request.PasienId,
                AsuransiId = request.AsuransiId,
                NoPolis = request.NoPolis
            };

            _applicationDbContext.AsuransiPasiens.Add(newAsuransiPasien);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Data berhasil ditambahkan!", data = newAsuransiPasien });
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateAsuransiPasien([FromBody] AsuransiPasienViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Ambil User ID dari JWT Claims**
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var dateNow = DateTimeOffset.UtcNow;
        //        var setDateNow = dateNow.ToString("yyMMdd");

        //        // Ambil data terakhir untuk hari ini (tanpa ToString di query)
        //        var lastCode = _applicationDbContext.AsuransiPasiens
        //            .Where(d => d.CreateDateTime.Date == dateNow.UtcDateTime.Date)
        //            .OrderByDescending(k => k.KodeAsuransiPasien)
        //            .FirstOrDefault();

        //        string kode;
        //        if (lastCode == null)
        //        {
        //            kode = $"AGM{setDateNow}0001";
        //        }
        //        else
        //        {
        //            var lastCodeTrim = lastCode.KodeAsuransiPasien.Substring(3, 6);

        //            if (lastCodeTrim != setDateNow)
        //            {
        //                kode = $"AGM{setDateNow}0001";
        //            }
        //            else
        //            {
        //                kode = $"AGM{setDateNow}" + (Convert.ToInt32(lastCode.KodeAsuransiPasien.Substring(9)) + 1).ToString("D4");
        //            }
        //        }

        //        // Cek Duplikasi
        //        var isDuplicate = _applicationDbContext.AsuransiPasiens
        //            .Any(c => c.KodeAsuransiPasien == kode && c.NamaAsuransiPasien == vm.NamaAsuransiPasien);

        //        if (isDuplicate)
        //        {
        //            return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
        //        }

        //        // Validate ModelState
        //        if (ModelState.IsValid)
        //        {
        //            // Simpan Data
        //            var data = new AsuransiPasien
        //            {
        //                AsuransiPasienId = Guid.NewGuid(),
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = UserActiveId,
        //                KodeAsuransiPasien = kode,
        //                NamaAsuransiPasien = vm.NamaAsuransiPasien
        //            };

        //            _applicationDbContext.AsuransiPasiens.Add(data);
        //            _applicationDbContext.SaveChanges();

        //            return Created("", new
        //            {
        //                message = "Tambah Data Berhasil || 201 Created",
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}
    }
}
