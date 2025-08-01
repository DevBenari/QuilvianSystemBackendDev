using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Helper;
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
    public class DelegasiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DelegasiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DelegasiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DelegasiController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        private bool ValidatePin(string inputPin, string storedHashedPin)
        {
            string inputHashed = DelegasiVerifikasi.ComputeSha256Hash(inputPin);
            return inputHashed == storedHashedPin;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Delegasis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DelegasiId = a.DelegasiId,
                            IsDelegated = a.IsDelegated,
                            UserDelegasiId = a.UserDelegasiId,
                            UserActiveId = a.UserActiveId,
                            Tugas = a.Tugas
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DelegasiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
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

                // **Cek User Delegasi **
                var pinDelegasi = _applicationDbContext.UserActives
                    .Where(d => d.UserActiveId == vm.UserDelegasiId)
                    .Select(d => d.PinPegawai)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(pinDelegasi))
                {
                    return BadRequest(new { message = "User Delegasi tidak ditemukan atau belum memiliki PIN." });
                }

                // **Validasi PIN**
                if (!ValidatePin(vm.Pin, pinDelegasi))
                {
                    return BadRequest(new { message = "PIN yang dimasukkan tidak valid." });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Delegasi
                    {
                        DelegasiId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelegated = true,
                        UserDelegasiId = vm.UserDelegasiId,
                        UserActiveId = UserActiveId,
                        Tugas = vm.Tugas?.ToString()
                    };

                    _applicationDbContext.Delegasis.Add(data);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
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

        [HttpPut("UpdateDelegasi/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateIsDelegatedVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var data = _applicationDbContext.Delegasis.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Pasien**
                data.IsDelegated = vm.Status;

                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Delegasis.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
