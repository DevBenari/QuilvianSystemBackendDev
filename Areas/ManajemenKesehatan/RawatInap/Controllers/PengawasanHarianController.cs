using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PengawasanHarianController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PengawasanHarianController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PengawasanHarianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PengawasanHarianController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local
                ); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }
            return null;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.PengawasanHarians
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        where a.IsDelete == false || a.IsDelete == null
                        orderby a.CreateDateTime descending
                        select new
                        {
                            a.PengawasanHarianId,
                            a.KunjunganId,
                            a.PasienId,
                            a.PainAssesment,
                            a.VitalSign,
                            a.Resep,
                            a.TglPengawasanHarian,
                            a.WaktuPengawasan,
                            a.IsRelaksasi,
                            a.IsKompres,
                            a.IsDetailKompres,
                            a.IsPijatan,
                            a.IsTens,
                            a.IsIstirahat,
                            a.IsMusik,
                            a.IsTeraphyAktivitas,
                            a.IsLatihanOtot,
                            a.IntakeInfuse,
                            a.IntakeOral,
                            a.IntakeNGT,
                            a.IntakeDarah,
                            a.IntakeObat,
                            a.TotalIntake,
                            a.OutputUrin,
                            a.OutputFeses,
                            a.OutputNGT,
                            a.OutputWL,
                            a.TotalOutput,
                            a.BalanceShift,
                            a.Balance24H,
                            a.GulaDarah,
                            a.AsupanMakanan,
                            a.Diet,
                            a.LingkarPerut,
                            a.MobilisasiPasien,
                            a.Keterangan,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName
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
            var listdata = _applicationDbContext.PengawasanHarians.Find(id);
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
        public async Task<IActionResult> Create([FromBody] PengawasanHarianViewModel vm)
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

                //// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new PengawasanHarian
                {
                    PengawasanHarianId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    PainAssesment = vm.PainAssesment,
                    VitalSign = vm.VitalSign,
                    Resep = vm.Resep,
                    TglPengawasanHarian = TryParseTanggalToUtc(vm.TglPengawasanHarian),
                    WaktuPengawasan = vm.WaktuPengawasan,
                    IsRelaksasi = vm.IsRelaksasi,
                    IsKompres = vm.IsKompres,
                    IsDetailKompres = vm.IsDetailKompres,
                    IsPijatan = vm.IsPijatan,
                    IsTens = vm.IsTens,
                    IsIstirahat = vm.IsIstirahat,
                    IsMusik = vm.IsMusik,
                    IsTeraphyAktivitas = vm.IsTeraphyAktivitas,
                    IntakeInfuse = vm.IntakeInfuse,
                    IntakeOral = vm.IntakeOral,
                    IntakeNGT = vm.IntakeNGT,
                    IntakeDarah = vm.IntakeDarah,
                    IntakeObat = vm.IntakeObat,
                    TotalIntake = vm.TotalIntake,
                    OutputUrin = vm.OutputUrin,
                    OutputFeses = vm.OutputFeses,
                    OutputNGT = vm.OutputNGT,
                    OutputWL = vm.OutputWL,
                    TotalOutput = vm.TotalOutput,
                    Balance24H = vm.Balance24H,
                    BalanceShift = vm.BalanceShift,
                    GulaDarah = vm.GulaDarah,
                    AsupanMakanan = vm.AsupanMakanan,
                    Diet = vm.Diet,
                    LingkarPerut = vm.LingkarPerut,
                    MobilisasiPasien = vm.MobilisasiPasien,
                    Keterangan = vm.Keterangan,
    
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.PengawasanHarians.Add(data);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PengawasanHarianViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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
                var data = await _applicationDbContext.PengawasanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.PainAssesment = vm.PainAssesment;
                data.VitalSign = vm.VitalSign;
                data.Resep = vm.Resep;
                data.TglPengawasanHarian = TryParseTanggalToUtc(vm.TglPengawasanHarian);
                data.WaktuPengawasan = vm.WaktuPengawasan;

                data.IsRelaksasi = vm.IsRelaksasi;
                data.IsKompres = vm.IsKompres;
                data.IsDetailKompres = vm.IsDetailKompres;
                data.IsPijatan = vm.IsPijatan;
                data.IsTens = vm.IsTens;
                data.IsIstirahat = vm.IsIstirahat;
                data.IsMusik = vm.IsMusik;
                data.IsTeraphyAktivitas = vm.IsTeraphyAktivitas;
                data.IsLatihanOtot = vm.IsLatihanOtot;

                data.IntakeInfuse = vm.IntakeInfuse;
                data.IntakeOral = vm.IntakeOral;
                data.IntakeNGT = vm.IntakeNGT;
                data.IntakeDarah = vm.IntakeDarah;
                data.IntakeObat = vm.IntakeObat;
                data.TotalIntake = vm.TotalIntake;

                data.OutputUrin = vm.OutputUrin;
                data.OutputFeses = vm.OutputFeses;
                data.OutputNGT = vm.OutputNGT;
                data.OutputWL = vm.OutputWL;
                data.TotalOutput = vm.TotalOutput;

                data.BalanceShift = vm.BalanceShift;
                data.Balance24H = vm.Balance24H;
                data.GulaDarah = vm.GulaDarah;
                data.AsupanMakanan = vm.AsupanMakanan;
                data.Diet = vm.Diet;
                data.LingkarPerut = vm.LingkarPerut;
                data.MobilisasiPasien = vm.MobilisasiPasien;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.PengawasanHarians.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
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
                var data = await _applicationDbContext.PengawasanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PengawasanHarians.Update(data);
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

        [HttpGet("paged")]
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganId = null,
        Guid? pasienId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.PengawasanHarians
                         join u in _applicationDbContext.UserActives
                             on a.CreateBy equals u.UserActiveId into userJoin
                         from u in userJoin.DefaultIfEmpty()
                         where a.IsDelete == false || a.IsDelete == null
                         orderby a.CreateDateTime descending
                         select new
                         {
                             a.PengawasanHarianId,
                             a.KunjunganId,
                             a.PasienId,
                             a.PainAssesment,
                             a.VitalSign,
                             a.Resep,
                             a.TglPengawasanHarian,
                             a.WaktuPengawasan,
                             a.IsRelaksasi,
                             a.IsKompres,
                             a.IsDetailKompres,
                             a.IsPijatan,
                             a.IsTens,
                             a.IsIstirahat,
                             a.IsMusik,
                             a.IsTeraphyAktivitas,
                             a.IsLatihanOtot,
                             a.IntakeInfuse,
                             a.IntakeOral,
                             a.IntakeNGT,
                             a.IntakeDarah,
                             a.IntakeObat,
                             a.TotalIntake,
                             a.OutputUrin,
                             a.OutputFeses,
                             a.OutputNGT,
                             a.OutputWL,
                             a.TotalOutput,
                             a.BalanceShift,
                             a.Balance24H,
                             a.GulaDarah,
                             a.AsupanMakanan,
                             a.Diet,
                             a.LingkarPerut,
                             a.MobilisasiPasien,
                             a.Keterangan,
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter berdasarkan kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u => u.KunjunganId == kunjunganId.Value);
            }

            // filter berdasarkan pasien id
            if (pasienId.HasValue)
            {
                query = query.Where(u => u.PasienId == pasienId.Value);
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
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
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
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    _ => query.OrderBy(u => u.CreateDateTime)
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
