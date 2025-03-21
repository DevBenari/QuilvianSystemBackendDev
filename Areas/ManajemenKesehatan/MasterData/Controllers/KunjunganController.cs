using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KunjunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<KunjunganController> logger,
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
        public async Task<IActionResult> GetAllKunjungan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            // Query data
            var result = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            FullName = u.FullName,
                            KunjunganId = a.KunjunganID,
                            AsuransiId = a.AsuransiId,
                            PoliklinikId = a.PoliklinikId,
                            DokterId = a.DokterId,
                            TindakanId = a.TindakanId,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            TipePasien = a.TipePasien,
                            TipePembayaran = a.TipePembayaran,
                            Antrian = a.Antrian,
                            JumlahKunjungan = a.JumlahKunjungan
                        };

            // Hitung total data sebelum paginasi
            var totalRows = result.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = result
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
        public async Task<IActionResult> GetAsuransiPasienById(Guid id)
        {
            var listdata = _applicationDbContext.Kunjungans.Find(id);
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
        public async Task<IActionResult> CreateAsuransiPasien([FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Periksa apakah pasien dan asuransi ada di database
                //var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                //                      .Any(p => p.PendaftaranPasienBaruId.ToString() == request.PasienId);

                //var asuransiExists = _applicationDbContext.Asuransis
                //                      .Any(a => a.AsuransiId.ToString() == request.AsuransiId);

                //if (!pasienExists || !asuransiExists)
                //{
                //    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                //}

                // Generate Urutan Kunjungan
                // 🔹 Validasi tipe pasien (harus "Rujukan" atau "Umum")
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // 🔹 Ambil antrian terakhir berdasarkan tipe pasien
                var lastAntrian = _applicationDbContext.Kunjungans
                    .Where(a => a.TipePasien == request.TipePasien)
                    .OrderByDescending(a => a.CreateDateTime)
                    .FirstOrDefault();

                // 🔹 Reset antrian jika hari berganti
                bool isNewDay = lastAntrian == null || lastAntrian.CreateDateTime.UtcDateTime.Date != DateTime.UtcNow.Date;
                int nextNumber = isNewDay ? 1 : int.Parse(lastAntrian.Antrian.Split('-')[1]) + 1;

                // Format nomor antrian
                string prefix = request.TipePasien.Equals("Rujukan", StringComparison.OrdinalIgnoreCase) ? "R" : "U";
                string nomorAntrian = $"{prefix}-{nextNumber:D3}";

                //validate model state
                if (ModelState.IsValid)
                {
                    var newKunjungan = new Kunjungan
                    {

                        
                    };

                    _applicationDbContext.Kunjungans.Add(newKunjungan);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Data berhasil ditambahkan!", data = newKunjungan });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
