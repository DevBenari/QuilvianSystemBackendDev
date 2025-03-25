using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PendaftaranPasienUGDController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienUGDController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienUGDController
            (ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PendaftaranPasienUGDController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPendaftaranPasienUGD(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.PendaftaranPasienUGDs
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienUGDId = a.PendaftaranPasienUGDId,
                            KodePasienUGD = a.KodePasienUGD,
                            NamaPasien = a.NamaPasien,
                            Title = a.Title,
                            TTL = a.TTL,
                            Umur = a.Umur,
                            NoTelp = a.NoTelp,
                            NamaDokterUGD = a.NamaDokterUGD,
                            Diagnosa = a.Diagnosa,
                            Tindakan = a.Tindakan,
                            BiayaAdmin = a.BiayaAdmin,
                            Kelas = a.Kelas,
                            //a.AsuransiId,
                            NoPolis = a.NoPolis,
                            NamaAsuransi = a.NamaAsuransi,
                            Afliasi = a.Afliasi
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPendaftaranPasienUGDById(Guid id)
        {
            var listdata = _applicationDbContext.PendaftaranPasienUGDs.Find(id);
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
        public async Task<IActionResult> CreatePendaftaranPasienUGD([FromBody] PendaftaranPasienUGDViewModel vm)
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
                var lastCode = _applicationDbContext.PendaftaranPasienUGDs
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePasienUGD)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"UGD{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KodePasienUGD.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"UGD{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"UGD{setDateNow}" + (Convert.ToInt32(lastCode.KodePasienUGD.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.PendaftaranPasienUGDs
                    .Any(c => c.KodePasienUGD == kode);

                if (ModelState.IsValid)
                {
                    var data = new PendaftaranPasienUGD
                    {
                        PendaftaranPasienUGDId = Guid.NewGuid(),
                        KodePasienUGD = kode,
                        NamaPasien = vm.NamaPasien,
                        Title = vm.Title,
                        TTL = vm.TTL,
                        Umur = vm.Umur,
                        NoTelp = vm.NoTelp,
                        NamaDokterUGD = vm.NamaDokterUGD,
                        Diagnosa = vm.Diagnosa,
                        Tindakan = vm.Tindakan,
                        BiayaAdmin = vm.BiayaAdmin,
                        Kelas = vm.Kelas,
                        AsuransiId = vm.AsuransiId,
                        NoPolis = vm.NoPolis,
                        NamaAsuransi = vm.NamaAsuransi,
                        Afliasi = vm.Afliasi,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.PendaftaranPasienUGDs.Add(data);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created"

                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePendaftaranPasienUGD(Guid id, [FromBody] PendaftaranPasienUGDViewModel vm)
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

                var data = _applicationDbContext.PendaftaranPasienUGDs.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                data.NamaPasien = vm.NamaPasien;
                data.Title = vm.Title;
                data.TTL = vm.TTL;
                data.Umur = vm.Umur;
                data.NoTelp = vm.NoTelp;
                data.NamaDokterUGD = vm.NamaDokterUGD;
                data.Diagnosa = vm.Diagnosa;
                data.Tindakan = vm.Tindakan;
                data.BiayaAdmin = vm.BiayaAdmin;
                data.Kelas = vm.Kelas;
                data.AsuransiId = vm.AsuransiId;
                data.NoPolis = vm.NoPolis;
                data.NamaAsuransi = vm.NamaAsuransi;
                data.Afliasi = vm.Afliasi;

                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;


                _applicationDbContext.PendaftaranPasienUGDs.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil diupdate..." });
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
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Dokter**
                var data = _applicationDbContext.PendaftaranPasienUGDs.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.PendaftaranPasienUGDs.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedPosition(
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
            var query = from a in _applicationDbContext.PendaftaranPasienUGDs
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienUGDId = a.PendaftaranPasienUGDId,
                            KodePasienUGD = a.KodePasienUGD,
                            NamaPasien = a.NamaPasien,
                            Title = a.Title,
                            TTL = a.TTL,
                            Umur = a.Umur,
                            NoTelp = a.NoTelp,
                            NamaDokterUGD = a.NamaDokterUGD,
                            Diagnosa = a.Diagnosa,
                            Tindakan = a.Tindakan,
                            BiayaAdmin = a.BiayaAdmin,
                            Kelas = a.Kelas,
                            //a.AsuransiId,
                            NoPolis = a.NoPolis,
                            NamaAsuransi = a.NamaAsuransi,
                            Afliasi = a.Afliasi
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.KodePasienUGD.Contains(search) || u.NamaPasien.Contains(search) || u.Tindakan.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u =>
                    u.CreateDateTime.Date >= startDate.Value.Date &&
                    u.CreateDateTime.Date <= endDate.Value.Date
                );
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
                    "KodePasienUGD" => query.OrderByDescending(u => u.
                    KodePasienUGD),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "Tindakan" => query.OrderByDescending(u => u.Tindakan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePasienUGD" => query.OrderByDescending(u => u.
                    KodePasienUGD),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "Tindakan" => query.OrderByDescending(u => u.Tindakan),
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