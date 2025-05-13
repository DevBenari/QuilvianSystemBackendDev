using System.Dynamic;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class WilayahController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<WilayahController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WilayahController
        (
             ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<WilayahController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
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

            // Query data
            var query = (from a in _applicationDbContext.Provinsis
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             ProvinsiId = a.ProvinsiId,
                             KodeProvinsi = a.KodeProvinsi,
                             NamaProvinsi = a.NamaProvinsi,
                             NegaraId = a.NegaraId,
                             NamaNegara = a.Negara.NamaNegara
                         });

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

        [HttpGet("KabupatenKota")]
        public async Task<IActionResult> GetAllKabupatenKota(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.KabupatenKotas
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KabupatenKotaId = a.KabupatenKotaId,
                            KodeKabupatenKota = a.KodeKabupatenKota,
                            NamaKabupatenKota = a.NamaKabupatenKota,
                            ProvinsiId = a.ProvinsiId,
                            NamaProvinsi = a.Provinsi.NamaProvinsi
                        });

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

        [HttpGet("Kecamatan")]
        public async Task<IActionResult> GetAllKecamatan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Kecamatans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join k in _applicationDbContext.KabupatenKotas
                        on a.KabupatenKotaId equals k.KabupatenKotaId
                        join p in _applicationDbContext.Provinsis
                        on k.ProvinsiId equals p.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KecamatanId = a.KecamatanId,
                            KodeKecamatan = a.KodeKecamatan,
                            NamaKecamatan = a.NamaKecamatan,
                            KabupatenKotaId = a.KabupatenKotaId,
                            NamaKabupatenKota = k.NamaKabupatenKota,
                            ProvinsiId = p.ProvinsiId,
                            NamaProvinsi = p.NamaProvinsi
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

        [HttpGet("Kelurahan")]
        public async Task<IActionResult> GetAllKelurahan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Kelurahans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join k in _applicationDbContext.Kecamatans
                        on a.KecamatanId equals k.KecamatanId
                        join kab in _applicationDbContext.KabupatenKotas
                        on k.KabupatenKotaId equals kab.KabupatenKotaId
                        join p in _applicationDbContext.Provinsis
                        on kab.ProvinsiId equals p.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KodeKelurahan = a.KodeKelurahan,
                            NamaKelurahan = a.NamaKelurahan,
                            KelurahanId = a.KelurahanId,
                            KecamatanId = a.KecamatanId,
                            NamaKecamatan = k.NamaKecamatan,
                            KabupatenKotaId = kab.KabupatenKotaId,
                            NamaKabupatenKota = kab.NamaKabupatenKota,
                            ProvinsiId = p.ProvinsiId,
                            NamaProvinsi = p.NamaProvinsi
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

        [HttpGet("ProvinsiById/{id}")]
        public async Task<IActionResult> GetProvinsiById(Guid id)
        {
            var listdata = _applicationDbContext.Provinsis.Find(id);
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

        [HttpGet("KabupatenKotaById/{id}")]
        public async Task<IActionResult> GetKabupatenKotaById(Guid id)
        {
            var listdata = _applicationDbContext.KabupatenKotas.Find(id);
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

        [HttpGet("KecamatanById/{id}")]
        public async Task<IActionResult> GetKecamatanById(Guid id)
        {
            var listdata = _applicationDbContext.Kecamatans.Find(id);
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

        [HttpGet("KelurahanById/{id}")]
        public async Task<IActionResult> GetKelurahanById(Guid id)
        {
            var listdata = _applicationDbContext.Kelurahans.Find(id);
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

        [HttpPost("Provinsi")]
        public async Task<IActionResult> CreateProvinsi([FromBody] ProvinsiViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Provinsis
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeProvinsi)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PRV{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeProvinsi.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PRV{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PRV{setDateNow}" + (Convert.ToInt32(lastCode.KodeProvinsi.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.Provinsis
                    .Any(c => c.KodeProvinsi == kode && c.NamaProvinsi == vm.NamaProvinsi);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Provinsi
                    {
                        ProvinsiId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        KodeProvinsi = kode,
                        NamaProvinsi = vm.NamaProvinsi,
                        NegaraId = vm.NegaraId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Provinsis.Add(data);
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

        [HttpPost("KabupatenKota")]
        public async Task<IActionResult> CreateKabupatenKota([FromBody] KabupatenKotaViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.KabupatenKotas
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeKabupatenKota)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KKT{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeKabupatenKota.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KKT{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KKT{setDateNow}" + (Convert.ToInt32(lastCode.KodeKabupatenKota.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.KabupatenKotas
                    .Any(c => c.KodeKabupatenKota == kode && c.NamaKabupatenKota == vm.NamaKabupatenKota && c.ProvinsiId == vm.ProvinsiId);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new KabupatenKota
                    {
                        KabupatenKotaId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        KodeKabupatenKota = kode,
                        NamaKabupatenKota = vm.NamaKabupatenKota,
                        ProvinsiId = vm.ProvinsiId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.KabupatenKotas.Add(data);
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

        [HttpPost("Kecamatan")]
        public async Task<IActionResult> CreateKecamatan([FromBody] KecamatanViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Kecamatans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeKecamatan)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KCT{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeKecamatan.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KCT{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KCT{setDateNow}" + (Convert.ToInt32(lastCode.KodeKecamatan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.Kecamatans
                    .Any(c => c.KodeKecamatan == kode && c.NamaKecamatan == vm.NamaKecamatan && c.KabupatenKotaId == vm.KabupatenKotaId);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Kecamatan
                    {
                        KecamatanId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        KodeKecamatan = kode,
                        NamaKecamatan = vm.NamaKecamatan,
                        KabupatenKotaId = vm.KabupatenKotaId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Kecamatans.Add(data);
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

        [HttpPost("Kelurahan")]
        public async Task<IActionResult> CreateKelurahan([FromBody] KelurahanViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Kelurahans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeKelurahan)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KLH{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeKelurahan.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KLH{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KLH{setDateNow}" + (Convert.ToInt32(lastCode.KodeKelurahan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.Kelurahans
                    .Any(c => c.KodeKelurahan == kode && c.NamaKelurahan == vm.NamaKelurahan && c.KecamatanId == vm.KecamatanId);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Kelurahan
                    {
                        KelurahanId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        KodeKelurahan = kode,
                        NamaKelurahan = vm.NamaKelurahan,
                        KecamatanId = vm.KecamatanId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Kelurahans.Add(data);
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

        [HttpPut("Provinsi/{id}")]
        public async Task<IActionResult> UpdateProvinsi(Guid id, [FromBody] ProvinsiViewModel vm)
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
                var data = _applicationDbContext.Provinsis.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Pasien**
                data.NamaProvinsi = vm.NamaProvinsi;
                data.NegaraId = vm.NegaraId;

                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Provinsis.Update(data);
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

        [HttpPut("KabupatenKota/{id}")]
        public async Task<IActionResult> UpdateKabupatenKota(Guid id, [FromBody] KabupatenKotaViewModel vm)
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
                var data = _applicationDbContext.KabupatenKotas.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Wilayah**
                data.NamaKabupatenKota = vm.NamaKabupatenKota;
                data.ProvinsiId = vm.ProvinsiId;

                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.KabupatenKotas.Update(data);
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

        [HttpPut("Kecamatan/{id}")]
        public async Task<IActionResult> UpdateKecamatan(Guid id, [FromBody] KecamatanViewModel vm)
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
                var data = _applicationDbContext.Kecamatans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Pasien**
                data.NamaKecamatan = vm.NamaKecamatan;
                data.KabupatenKotaId = vm.KabupatenKotaId;


                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Kecamatans.Update(data);
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

        [HttpPut("Kelurahan/{id}")]
        public async Task<IActionResult> UpdateKelurahan(Guid id, [FromBody] KelurahanViewModel vm)
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
                var data = _applicationDbContext.Kelurahans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Pasien**
                data.NamaKelurahan = vm.NamaKelurahan;
                data.KecamatanId = vm.KecamatanId;

                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Kelurahans.Update(data);
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

        [HttpDelete("Provinsi/{id}")]
        public async Task<IActionResult> DeleteProvinsi(Guid id)
        {
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
                var data = _applicationDbContext.Provinsis.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Provinsis.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("KabupatenKota/{id}")]
        public async Task<IActionResult> DeleteKabupatenKota(Guid id)
        {
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
                var data = _applicationDbContext.KabupatenKotas.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.KabupatenKotas.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("Kecamatan/{id}")]
        public async Task<IActionResult> DeleteKecamatan(Guid id)
        {
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
                var data = _applicationDbContext.Kecamatans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Kecamatans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("Kelurahan/{id}")]
        public async Task<IActionResult> DeleteKelurahan(Guid id)
        {
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
                var data = _applicationDbContext.Kelurahans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Kelurahans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("PagedProvinsi")]
        public IActionResult PagedProvinsi(
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
            var query = from a in _applicationDbContext.Provinsis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            ProvinsiId = a.ProvinsiId,
                            KodeProvinsi = a.KodeProvinsi,
                            NamaProvinsi = a.NamaProvinsi,
                            NegaraId = a.NegaraId,
                            NamaNegara = a.Negara.NamaNegara
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeProvinsi, search) ||
                    EF.Functions.ILike(u.NamaProvinsi, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeProvinsi" => query.OrderByDescending(u => u.KodeProvinsi),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    "NamaNegara" => query.OrderByDescending(u => u.NamaNegara),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeProvinsi" => query.OrderByDescending(u => u.KodeProvinsi),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    "NamaNegara" => query.OrderByDescending(u => u.NamaNegara),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            //Pagination
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

        [HttpGet("PagedKabupatenKota")]
        public IActionResult PagedKabupatenKota(
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
            var query = from a in _applicationDbContext.KabupatenKotas
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KabupatenKotaId = a.KabupatenKotaId,
                            KodeKabupatenKota = a.KodeKabupatenKota,
                            NamaKabupatenKota = a.NamaKabupatenKota,
                            ProvinsiId = a.ProvinsiId,
                            NamaProvinsi = a.Provinsi.NamaProvinsi
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeKabupatenKota, search) ||
                    EF.Functions.ILike(u.NamaKabupatenKota, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKabupatenKota" => query.OrderByDescending(u => u.KodeKabupatenKota),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKabupatenKota" => query.OrderByDescending(u => u.KodeKabupatenKota),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            //Pagination
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

        [HttpGet("PagedKecamatan")]
        public IActionResult PagedKecamatan(
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
            var query = from a in _applicationDbContext.Kecamatans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join k in _applicationDbContext.KabupatenKotas
                        on a.KabupatenKotaId equals k.KabupatenKotaId
                        join p in _applicationDbContext.Provinsis
                        on k.ProvinsiId equals p.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KecamatanId = a.KecamatanId,
                            KodeKecamatan = a.KodeKecamatan,
                            NamaKecamatan = a.NamaKecamatan,
                            KabupatenKotaId = a.KabupatenKotaId,
                            NamaKabupatenKota = k.NamaKabupatenKota,
                            ProvinsiId = p.ProvinsiId,
                            NamaProvinsi = p.NamaProvinsi
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeKecamatan, search) ||
                    EF.Functions.ILike(u.NamaKecamatan, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKecamatan" => query.OrderByDescending(u => u.KodeKecamatan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKecamatan" => query.OrderByDescending(u => u.KodeKecamatan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            //Pagination
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

        [HttpGet("PagedKelurahan")]
        public IActionResult PagedKelurahan(
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
            var query = from a in _applicationDbContext.Kelurahans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        join k in _applicationDbContext.Kecamatans
                        on a.KecamatanId equals k.KecamatanId
                        join kab in _applicationDbContext.KabupatenKotas
                        on k.KabupatenKotaId equals kab.KabupatenKotaId
                        join p in _applicationDbContext.Provinsis
                        on kab.ProvinsiId equals p.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KodeKelurahan = a.KodeKelurahan,
                            NamaKelurahan = a.NamaKelurahan,
                            KelurahanId = a.KelurahanId,
                            KecamatanId = a.KecamatanId,
                            NamaKecamatan = k.NamaKecamatan,
                            KabupatenKotaId = kab.KabupatenKotaId,
                            NamaKabupatenKota = kab.NamaKabupatenKota,
                            ProvinsiId = p.ProvinsiId,
                            NamaProvinsi = p.NamaProvinsi
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeKelurahan, search) ||
                    EF.Functions.ILike(u.NamaKelurahan, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKelurahan" => query.OrderByDescending(u => u.KodeKelurahan),
                    "NamaKelurahan" => query.OrderByDescending(u => u.NamaKelurahan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeKelurahan" => query.OrderByDescending(u => u.KodeKelurahan),
                    "NamaKelurahan" => query.OrderByDescending(u => u.NamaKelurahan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            //Pagination
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

        [HttpGet("KodePos")]
        public async Task<IActionResult> GetAllKodePos(int page = 1, int perPage = 10)
        {
            // validasi pagging
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.KodePoss
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        join kel in _applicationDbContext.Kelurahans
                            on a.KelurahanId equals kel.KelurahanId
                        join kec in _applicationDbContext.Kecamatans
                            on kel.KecamatanId equals kec.KecamatanId
                        join kab in _applicationDbContext.KabupatenKotas
                            on kec.KabupatenKotaId equals kab.KabupatenKotaId
                        join prov in _applicationDbContext.Provinsis
                            on kab.ProvinsiId equals prov.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KodePosId = a.KodePosId,
                            UniqueKodePos = a.UniqueKodePos,
                            KelurahanId = a.KelurahanId,
                            NamaKodePos = a.NamaKodePos,

                            // Data Kelurahan
                            NamaKelurahan = kel.NamaKelurahan,

                            // Data Kecamatan
                            KecamatanId = kel.KecamatanId,
                            NamaKecamatan = kec.NamaKecamatan,

                            // Data Kabupaten
                            KabupatenKotaId = kec.KabupatenKotaId,
                            NamaKabupatenKota = kab.NamaKabupatenKota,

                            // Data Provinsi
                            ProvinsiId = kab.ProvinsiId,
                            NamaProvinsi = prov.NamaProvinsi
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

        [HttpGet("KodePosById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _applicationDbContext.KodePoss.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }


        [HttpPost("KodePos")]
        public async Task<IActionResult> Create([FromBody] KodePosViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _applicationDbContext.KodePoss
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.UniqueKodePos)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"KDP{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.UniqueKodePos.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"KDP{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"KDP{setDateNow}" + (Convert.ToInt32(lastCode.UniqueKodePos.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.KodePoss
                    .Any(c => c.UniqueKodePos == kode);

                if (ModelState.IsValid)
                {
                    var data = new KodePos
                    {
                        KodePosId = Guid.NewGuid(),
                        UniqueKodePos = kode,
                        KelurahanId = vm.KelurahanId,
                        NamaKodePos = vm.NamaKodePos,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false,
                    };

                    _applicationDbContext.KodePoss.Add(data);
                    _applicationDbContext.SaveChanges();

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

        [HttpPut("KodePos/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] KodePosViewModel vm)
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

                // cari data
                var data = _applicationDbContext.KodePoss.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.KelurahanId = vm.KelurahanId;
                data.NamaKodePos = vm.NamaKodePos;

                _applicationDbContext.KodePoss.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil diupdate..." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("KodePos/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
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

                // **Cari Data Dokter**
                var data = _applicationDbContext.KodePoss.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.KodePoss.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("PagedKodePos")]
        public IActionResult PagedOperasi(
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
            var query = from a in _applicationDbContext.KodePoss
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        join kel in _applicationDbContext.Kelurahans
                            on a.KelurahanId equals kel.KelurahanId
                        join kec in _applicationDbContext.Kecamatans
                            on kel.KecamatanId equals kec.KecamatanId
                        join kab in _applicationDbContext.KabupatenKotas
                            on kec.KabupatenKotaId equals kab.KabupatenKotaId
                        join prov in _applicationDbContext.Provinsis
                            on kab.ProvinsiId equals prov.ProvinsiId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            KodePosId = a.KodePosId,
                            UniqueKodePos = a.UniqueKodePos,
                            KelurahanId = a.KelurahanId,
                            NamaKodePos = a.NamaKodePos,

                            // Data Kelurahan
                            NamaKelurahan = kel.NamaKelurahan,

                            // Data Kecamatan
                            KecamatanId = kel.KecamatanId,
                            NamaKecamatan = kec.NamaKecamatan,

                            // Data Kabupaten
                            KabupatenKotaId = kec.KabupatenKotaId,
                            NamaKabupatenKota = kab.NamaKabupatenKota,

                            // Data Provinsi
                            ProvinsiId = kab.ProvinsiId,
                            NamaProvinsi = prov.NamaProvinsi
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.UniqueKodePos, search) ||
                    EF.Functions.ILike(u.NamaKodePos, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "UniqueKodePos" => query.OrderByDescending(u => u.UniqueKodePos),
                    "NamaKodePos" => query.OrderByDescending(u => u.NamaKodePos),
                    "NamaKelurahan" => query.OrderByDescending(u => u.NamaKelurahan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "UniqueKodePos" => query.OrderByDescending(u => u.UniqueKodePos),
                    "NamaKodePos" => query.OrderByDescending(u => u.NamaKodePos),
                    "NamaKelurahan" => query.OrderByDescending(u => u.NamaKelurahan),
                    "NamaKecamatan" => query.OrderByDescending(u => u.NamaKecamatan),
                    "NamaKabupatenKota" => query.OrderByDescending(u => u.NamaKabupatenKota),
                    "NamaProvinsi" => query.OrderByDescending(u => u.NamaProvinsi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
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

