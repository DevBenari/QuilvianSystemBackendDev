using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PneumoniaController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PneumoniaController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PneumoniaController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PneumoniaController> logger,
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
            var query =
               from p in _applicationDbContext.Pneumonias
               join u in _applicationDbContext.UserActives on p.CreateBy equals u.UserActiveId into uGroup
               from user in uGroup.DefaultIfEmpty()

               join d1 in _applicationDbContext.Dokters on p.DokterHAPId equals d1.DokterId into dHAPGroup
               from dokterHAP in dHAPGroup.DefaultIfEmpty()

               join d2 in _applicationDbContext.Dokters on p.DokterVAPId equals d2.DokterId into dVAPGroup
               from dokterVAP in dVAPGroup.DefaultIfEmpty()

               where p.IsDelete == false || p.IsDelete == null
               orderby p.CreateDateTime descending

               select new
               {
                   p.PneumoniaId,
                   p.KunjunganId,
                   p.PasienId,
                   p.IsFotoThorax,
                   p.IsHAP,
                   p.HasilFotoThorax,
                   DokterHAP = dokterHAP.NmDokter,
                   p.IsVAP,
                   DokterVAP = dokterVAP.NmDokter,
                   p.IsVentilatorTerpasang,
                   p.TglAwalVT,
                   p.TglAkhirVT,
                   p.HariKe,
                   p.HasilThoraxSebelumVT,
                   p.HasilThoraxSesudahVT,
                   p.TglPencatatan,
                   p.Keterangan,
                   CreateByName = user.FullName,
                   p.CreateDateTime
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var data =
                await (from p in _applicationDbContext.Pneumonias
                       join d1 in _applicationDbContext.Dokters on p.DokterHAPId equals d1.DokterId into dHAPGroup
                       from dokterHAP in dHAPGroup.DefaultIfEmpty()

                       join d2 in _applicationDbContext.Dokters on p.DokterVAPId equals d2.DokterId into dVAPGroup
                       from dokterVAP in dVAPGroup.DefaultIfEmpty()

                       where p.PneumoniaId == id
                       select new
                       {
                           p,
                           DokterHAP = dokterHAP.NmDokter,
                           DokterVAP = dokterVAP.NmDokter
                       }).FirstOrDefaultAsync();

            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PneumoniaViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 🔹 User dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin)) return Unauthorized();

                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null) return Unauthorized();

                // 🔹 Hitung HariKe (TglAkhirVT - TglAwalVT)
                int? hariKe = null;
                if (vm.TglAwalVT.HasValue && vm.TglAkhirVT.HasValue)
                {
                    hariKe = (vm.TglAkhirVT.Value.Date - vm.TglAwalVT.Value.Date).Days;
                    hariKe = hariKe < 0 ? 0 : hariKe;
                }

                var data = new Pneumonia
                {
                    PneumoniaId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    IsFotoThorax = vm.IsFotoThorax,
                    IsHAP = vm.IsHAP,
                    HasilFotoThorax = vm.HasilFotoThorax,
                    DokterHAPId = vm.DokterHAPId,
                    IsVAP = vm.IsVAP,
                    DokterVAPId = vm.DokterVAPId,
                    IsVentilatorTerpasang = vm.IsVentilatorTerpasang,
                    TglAwalVT = vm.TglAwalVT,
                    TglAkhirVT = vm.TglAkhirVT,
                    HariKe = hariKe,
                    HasilThoraxSebelumVT = vm.HasilThoraxSebelumVT,
                    HasilThoraxSesudahVT = vm.HasilThoraxSesudahVT,
                    TglPencatatan = vm.TglPencatatan,
                    Keterangan = vm.Keterangan,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.Pneumonias.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();
                if (result > 0)
                    return Created("", new { message = "Tambah Data Pneumonia Berhasil", data.PneumoniaId });

                return StatusCode(500, new { message = "Gagal menyimpan data" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PneumoniaViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                var data = await _applicationDbContext.Pneumonias.FirstOrDefaultAsync(x => x.PneumoniaId == id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // 🔹 User dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);

                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.IsFotoThorax = vm.IsFotoThorax;
                data.IsHAP = vm.IsHAP;
                data.HasilFotoThorax = vm.HasilFotoThorax;
                data.DokterHAPId = vm.DokterHAPId;
                data.IsVAP = vm.IsVAP;
                data.DokterVAPId = vm.DokterVAPId;
                data.IsVentilatorTerpasang = vm.IsVentilatorTerpasang;
                data.TglAwalVT = vm.TglAwalVT;
                data.TglAkhirVT = vm.TglAkhirVT;

                // 🔹 Hitung ulang HariKe
                if (vm.TglAwalVT.HasValue && vm.TglAkhirVT.HasValue)
                {
                    data.HariKe = (vm.TglAkhirVT.Value.Date - vm.TglAwalVT.Value.Date).Days;
                    data.HariKe = data.HariKe < 0 ? 0 : data.HariKe;
                }

                data.HasilThoraxSebelumVT = vm.HasilThoraxSebelumVT;
                data.HasilThoraxSesudahVT = vm.HasilThoraxSesudahVT;
                data.TglPencatatan = vm.TglPencatatan;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = user.UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Pneumonia berhasil" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
                var data = await _applicationDbContext.Pneumonias.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Pneumonias.Update(data);
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
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query =
               from p in _applicationDbContext.Pneumonias
               join u in _applicationDbContext.UserActives on p.CreateBy equals u.UserActiveId into uGroup
               from user in uGroup.DefaultIfEmpty()

               join d1 in _applicationDbContext.Dokters on p.DokterHAPId equals d1.DokterId into dHAPGroup
               from dokterHAP in dHAPGroup.DefaultIfEmpty()

               join d2 in _applicationDbContext.Dokters on p.DokterVAPId equals d2.DokterId into dVAPGroup
               from dokterVAP in dVAPGroup.DefaultIfEmpty()

               where p.IsDelete == false || p.IsDelete == null
               orderby p.CreateDateTime descending

               select new
               {
                   p.PneumoniaId,
                   p.KunjunganId,
                   p.PasienId,
                   p.IsFotoThorax,
                   p.IsHAP,
                   p.HasilFotoThorax,
                   DokterHAP = dokterHAP.NmDokter,
                   p.IsVAP,
                   DokterVAP = dokterVAP.NmDokter,
                   p.IsVentilatorTerpasang,
                   p.TglAwalVT,
                   p.TglAkhirVT,
                   p.HariKe,
                   p.HasilThoraxSebelumVT,
                   p.HasilThoraxSesudahVT,
                   p.TglPencatatan,
                   p.Keterangan,
                   CreateByName = user.FullName,
                   p.CreateDateTime
               };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
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
