using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class PersalinanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PersalinanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PersalinanController
            (ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PersalinanController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPersalinan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Persalinans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PersalinanId = a.PersalinanId,
                            KodePersalinan = a.KodePersalinan,
                            NamaPersalinan = a.NamaPersalinan,
                            TanggalPersalinan = a.TanggalPersalinan,
                            TipePersalinan = a.TipePersalinan,
                            TindakanPersalinan = a.TindakanPersalinan,
                            SubTindakanPersalinan = a.SubTindakanPersalinan,
                            KomplikasiPersalinan = a.KomplikasiPersalinan,
                            NamaKamar = a.NamaKamar,
                            NoKamar = a.NoKamar,
                            KategoriKamar = a.KategoriKamar,
                            CatatanPersalinan = a.CatatanPersalinan,
                            DokterPersalinan = a.DokterPersalinan,
                            BidanPersalinan = a.BidanPersalinan,
                            AnastesiPersalinan = a.AnastesiPersalinan,
                            ObservasiPersalinan = a.ObservasiPersalinan,
                            NamaBayi = a.NamaBayi,
                            JenisKelaminBayi = a.JenisKelaminBayi,
                            TTLBayi = a.TTLBayi,
                            BeratBayi = a.BeratBayi,
                            PanjangBayi = a.PanjangBayi,
                            NamaAyah = a.NamaAyah,
                            NamaIbu = a.NamaIbu,
                            StatusBayi = a.StatusBayi
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
        public async Task<IActionResult> GetPersalinanById(Guid id)
        {
            var listdata = _applicationDbContext.Persalinans.Find(id);
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
        public async Task<IActionResult> CreatePersalinan([FromBody] PersalinanViewModel vm)
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

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Persalinans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePersalinan)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PRS{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodePersalinan.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PRS{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PRS{setDateNow}" + (Convert.ToInt32(lastCode.KodePersalinan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = _applicationDbContext.Persalinans
                    .Any(c => c.NamaPersalinan.ToLower().Trim() == vm.NamaPersalinan.ToLower().Trim() && !c.IsDelete);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new Persalinan
                    {
                        PersalinanId = Guid.NewGuid(),
                        KodePersalinan = kode,
                        NamaPersalinan = vm.NamaPersalinan,
                        TanggalPersalinan = vm.TanggalPersalinan,
                        TipePersalinan = vm.TipePersalinan,
                        TindakanPersalinan = vm.TindakanPersalinan,
                        SubTindakanPersalinan = vm.SubTindakanPersalinan,
                        KomplikasiPersalinan = vm.KomplikasiPersalinan,
                        NamaKamar = vm.NamaKamar,
                        NoKamar = vm.NoKamar,
                        KategoriKamar = vm.KategoriKamar,
                        CatatanPersalinan = vm.CatatanPersalinan,
                        DokterPersalinan = vm.DokterPersalinan,
                        BidanPersalinan = vm.BidanPersalinan,
                        AnastesiPersalinan = vm.AnastesiPersalinan,
                        ObservasiPersalinan = vm.ObservasiPersalinan,
                        NamaBayi = vm.NamaBayi,
                        JenisKelaminBayi = vm.JenisKelaminBayi,
                        TTLBayi = vm.TTLBayi,
                        BeratBayi = vm.BeratBayi,
                        PanjangBayi = vm.PanjangBayi,
                        NamaAyah = vm.NamaAyah,
                        NamaIbu = vm.NamaIbu,
                        StatusBayi = vm.StatusBayi,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Persalinans.Add(data);
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
        public async Task<IActionResult> UpdatePersalinan(Guid id, [FromBody] PersalinanViewModel vm)
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
                var data = _applicationDbContext.Persalinans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // cek duplikasi
                var isDuplicate = _applicationDbContext.Persalinans
                    .Any(c => c.NamaPersalinan.ToLower().Trim() == vm.NamaPersalinan.ToLower().Trim() && !c.IsDelete && c.PersalinanId !=id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Update Data**
                data.NamaPersalinan = vm.NamaPersalinan;
                data.TanggalPersalinan = vm.TanggalPersalinan;
                data.TipePersalinan = vm.TipePersalinan;
                data.TindakanPersalinan = vm.TindakanPersalinan;
                data.SubTindakanPersalinan = vm.SubTindakanPersalinan;
                data.KomplikasiPersalinan = vm.KomplikasiPersalinan;
                data.NamaKamar = vm.NamaKamar;
                data.NoKamar = vm.NoKamar;
                data.KategoriKamar = vm.KategoriKamar;
                data.CatatanPersalinan = vm.CatatanPersalinan;
                data.DokterPersalinan = vm.DokterPersalinan;
                data.BidanPersalinan = vm.BidanPersalinan;
                data.AnastesiPersalinan = vm.AnastesiPersalinan;
                data.ObservasiPersalinan = vm.ObservasiPersalinan;
                data.NamaBayi = vm.NamaBayi;
                data.JenisKelaminBayi = vm.JenisKelaminBayi;
                data.TTLBayi = vm.TTLBayi;
                data.BeratBayi = vm.BeratBayi;
                data.PanjangBayi = vm.PanjangBayi;
                data.NamaAyah = vm.NamaAyah;
                data.NamaIbu = vm.NamaIbu;
                data.StatusBayi = vm.StatusBayi;
                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;

                _applicationDbContext.Persalinans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                });
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });

            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersalinan(Guid id)
        {
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
                var data = _applicationDbContext.Persalinans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }
                // **Soft Delete Data**
                data.IsDelete = true;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.DeleteBy = UserActiveId;
                _applicationDbContext.Persalinans.Update(data);
                _applicationDbContext.SaveChanges();
                return Ok(new
                {
                    message = "Data berhasil dihapus. || 200 OK",
                });
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }

        }

        [HttpGet("paged")]
        public IActionResult PagedPersalinan(
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
            var query = from a in _applicationDbContext.Persalinans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PersalinanId = a.PersalinanId,
                            KodePersalinan = a.KodePersalinan,
                            NamaPersalinan = a.NamaPersalinan,
                            TanggalPersalinan = a.TanggalPersalinan,
                            TipePersalinan = a.TipePersalinan,
                            TindakanPersalinan = a.TindakanPersalinan,
                            SubTindakanPersalinan = a.SubTindakanPersalinan,
                            KomplikasiPersalinan = a.KomplikasiPersalinan,
                            NamaKamar = a.NamaKamar,
                            NoKamar = a.NoKamar,
                            KategoriKamar = a.KategoriKamar,
                            CatatanPersalinan = a.CatatanPersalinan,
                            DokterPersalinan = a.DokterPersalinan,
                            BidanPersalinan = a.BidanPersalinan,
                            AnastesiPersalinan = a.AnastesiPersalinan,
                            ObservasiPersalinan = a.ObservasiPersalinan,
                            NamaBayi = a.NamaBayi,
                            JenisKelaminBayi = a.JenisKelaminBayi,
                            TTLBayi = a.TTLBayi,
                            BeratBayi = a.BeratBayi,
                            PanjangBayi = a.PanjangBayi,
                            NamaAyah = a.NamaAyah,
                            NamaIbu = a.NamaIbu,
                            StatusBayi = a.StatusBayi
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodePersalinan, search) ||
                    EF.Functions.ILike(u.NamaPersalinan, search)
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
                    "KodePersalinan" => query.OrderByDescending(u => u.KodePersalinan),
                    "NamaPersalinan" => query.OrderByDescending(u => u.NamaPersalinan),
                    "NamaBayi" => query.OrderByDescending(u => u.NamaBayi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePersalinan" => query.OrderByDescending(u => u.KodePersalinan),
                    "NamaPersalinan" => query.OrderByDescending(u => u.NamaPersalinan),
                    "NamaBayi" => query.OrderByDescending(u => u.NamaBayi),
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
