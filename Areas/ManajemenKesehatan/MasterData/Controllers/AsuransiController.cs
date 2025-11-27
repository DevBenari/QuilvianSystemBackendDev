using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class AsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AsuransiController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<AsuransiController> logger,
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
        public async Task<IActionResult> GetAllAsuransi(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.Asuransis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            // Metadata
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,

                            // Identitas Asuransi
                            AsuransiId = a.AsuransiId,
                            KodeAsuransi = a.KodeAsuransi,

                            // Informasi Asuransi
                            NamaAsuransi = a.NamaAsuransi,
                            JenisAsuransi = a.JenisAsuransi,
                            StatusAsuransi = a.StatusAsuransi,
                            TanggalMulaiKerjasama = a.TanggalMulaiKerjasama,
                            TanggalAkhirKerjasama = a.TanggalAkhirKerjasama,

                            // Informasi Klaim
                            MetodeKlaim = a.MetodeKlaim,

                            // Pertanggungan
                            TambahanTanggungan = a.TambahanTanggungan,

                            // Pembayaran
                            TermOfPayment = a.TermOfPayment,

                            // Kontak Utama
                            NamaPerusahaanAsuransi = a.NamaPerusahaanAsuransi,
                            EmailPusat = a.EmailPusat,
                            namaPIC = a.namaPIC,
                            noPic = a.noPic,
                            noVerificationAdmin = a.noVerificationAdmin,
                            Alamat = a.Alamat,
                            Keterangan = a.Keterangan,

                            // Status PKS
                            IsPKS = a.IsPKS

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
        public async Task<IActionResult> GetAsuransiById(Guid id)
        {
            var listdata = _applicationDbContext.Asuransis.Find(id);
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
        public async Task<IActionResult> CreateAsuransi([FromBody] AsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
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

                var dateNow = DateTime.UtcNow; ;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Asuransis
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeAsuransi)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"ASR{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeAsuransi.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"ASR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"ASR{setDateNow}" + (Convert.ToInt32(lastCode.KodeAsuransi.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = await _applicationDbContext.Asuransis
                    .AnyAsync(c =>c.NamaAsuransi.ToLower().Trim() == vm.NamaAsuransi.ToLower().Trim() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }


                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new Asuransi
                    {
                        AsuransiId = Guid.NewGuid(),
                        KodeAsuransi = kode,

                        // Informasi Asuransi
                        NamaAsuransi = vm.NamaAsuransi,
                        JenisAsuransi = vm.JenisAsuransi,
                        StatusAsuransi = vm.StatusAsuransi,
                        TanggalMulaiKerjasama = vm.TanggalMulaiKerjasama,
                        TanggalAkhirKerjasama = vm.TanggalAkhirKerjasama,

                        // Informasi Klaim
                        MetodeKlaim = vm.MetodeKlaim,

                        // Informasi Pertanggungan
                        TambahanTanggungan = vm.TambahanTanggungan,

                        // Informasi Pembayaran
                        TermOfPayment = vm.TermOfPayment,

                        // Informasi Kontak Utama
                        NamaPerusahaanAsuransi = vm.NamaPerusahaanAsuransi,
                        EmailPusat = vm.EmailPusat,
                        namaPIC = vm.namaPIC,
                        noPic = vm.noPic,
                        noVerificationAdmin = vm.noVerificationAdmin,
                        Alamat = vm.Alamat,
                        Keterangan = vm.Keterangan,

                        // Status PKS
                        IsPKS = vm.IsPKS,

                        // Metadata
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false
                    };


                    _applicationDbContext.Asuransis.Add(data);
                    _applicationDbContext.SaveChanges();
                    return Created("", new
                    {
                        message = "Data berhasil ditambahkan. || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsuransi(Guid id, [FromBody] AsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }

            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "Anda tidak memiliki akses. || 401 Unauthorized" });
                }

                // **Cari Data**
                var asuransi = _applicationDbContext.Asuransis.Find(id);
                if (asuransi == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                if (ModelState.IsValid)
                {
                    // **Update Data**
                    asuransi.NamaAsuransi = vm.NamaAsuransi ?? asuransi.NamaAsuransi;
                    asuransi.JenisAsuransi = vm.JenisAsuransi ?? asuransi.JenisAsuransi;
                    asuransi.StatusAsuransi = vm.StatusAsuransi ?? asuransi.StatusAsuransi;
                    asuransi.TanggalMulaiKerjasama = vm.TanggalMulaiKerjasama ?? asuransi.TanggalMulaiKerjasama;
                    asuransi.TanggalAkhirKerjasama = vm.TanggalAkhirKerjasama ?? asuransi.TanggalAkhirKerjasama;
                    asuransi.IsPKS = vm.IsPKS;
                    asuransi.MetodeKlaim = vm.MetodeKlaim ?? asuransi.MetodeKlaim;
                    asuransi.TambahanTanggungan = vm.TambahanTanggungan ?? asuransi.TambahanTanggungan;
                    asuransi.TermOfPayment = vm.TermOfPayment ?? asuransi.TermOfPayment;
                    asuransi.NamaPerusahaanAsuransi = vm.NamaPerusahaanAsuransi ?? asuransi.NamaPerusahaanAsuransi;
                    asuransi.EmailPusat = vm.EmailPusat ?? asuransi.EmailPusat;
                    asuransi.IsPKS = vm.IsPKS;


                    asuransi.UpdateDateTime = DateTimeOffset.UtcNow;
                    asuransi.UpdateBy = UserActiveId;

                    _applicationDbContext.Asuransis.Update(asuransi);
                    _applicationDbContext.SaveChanges();

                    return Ok(new
                    {
                        message = "Update Data Berhasil || 200 OK",

                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch
            (Exception ex)
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

                // **Cari Data asuransi**
                var asuransi = _applicationDbContext.Asuransis.Find(id);
                if (asuransi == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**

                asuransi.DeleteDateTime = DateTimeOffset.UtcNow;
                asuransi.DeleteBy = UserActiveId;
                asuransi.IsDelete = true;

                _applicationDbContext.Asuransis.Update(asuransi);
                _applicationDbContext.SaveChanges();


                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PegedAsuransi(
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
            var query = from a in _applicationDbContext.Asuransis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            AsuransiId = a.AsuransiId,
                            KodeAsuransi = a.KodeAsuransi,
                            NamaAsuransi = a.NamaAsuransi,
                            JenisAsuransi = a.JenisAsuransi,
                            StatusAsuransi = a.StatusAsuransi,
                            TanggalMulaiKerjasama = a.TanggalMulaiKerjasama,
                            TanggalAkhirKerjasama = a.TanggalAkhirKerjasama,
                            MetodeKlaim = a.MetodeKlaim,
                            TambahanTanggungan = a.TambahanTanggungan,
                            TermOfPayment = a.TermOfPayment,
                            NamaPerusahaanAsuransi = a.NamaPerusahaanAsuransi,
                            EmailPusat = a.EmailPusat,
                            IsPKS = a.IsPKS
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.KodeAsuransi.Contains(search) || u.NamaAsuransi.Contains(search) || u.JenisAsuransi.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
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
                    "KodeAsuransi" => query.OrderByDescending(u => u.KodeAsuransi),
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "JenisAsuransi" => query.OrderByDescending(u => u.JenisAsuransi),
                    "StatusAsuransi" => query.OrderByDescending(u => u.StatusAsuransi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeAsuransi" => query.OrderByDescending(u => u.KodeAsuransi),
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "JenisAsuransi" => query.OrderByDescending(u => u.JenisAsuransi),
                    "StatusAsuransi" => query.OrderByDescending(u => u.StatusAsuransi),
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
