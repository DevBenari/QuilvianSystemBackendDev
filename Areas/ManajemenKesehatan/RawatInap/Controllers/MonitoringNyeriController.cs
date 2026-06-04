using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Migrations;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class MonitoringNyeriController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHubContext<MonitoringNyeriHub> _hubContext;
        private readonly ILogger<MonitoringNyeriController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MonitoringNyeriController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<MonitoringNyeriController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<MonitoringNyeriHub> hubContext)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from x in _applicationDbContext.MonitoringNyeris
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on x.CreateBy equals u.UserActiveId
                         where x.IsDelete == false || x.IsDelete == null
                         select new
                         {
                             CreateBy = u.FullName,
                             x.CreateDateTime,
                             MonitoringNyeriId = x.MonitoringNyeriId,
                             KunjunganId = x.KunjunganId,
                             PasienId = x.PasienId,
                             WaktuMonitoringNyeri = x.WaktuMonitoringNyeri,

                             SkorNyeri = x.SkorNyeri,
                             SkorSedasi = x.SkorSedasi,
                             Sistolik = x.Sistolik,
                             Diastolic = x.Diastolic,
                             Nadi = x.Nadi,
                             Respirasi = x.Respirasi,
                             Suhu = x.Suhu,

                             PerawatMonitoringId = x.PerawatMonitoringId,
                             ParafPerawatMonitoring = x.ParafPerawatMonitoring,

                             WaktuIntervensi = x.WaktuIntervensi,
                             ObatId = x.ObatId,
                             Dosis = x.Dosis,
                             Rute = x.Rute,
                             IntervensiNonFarmakologi = x.IntervensiNonFarmakologi,

                             PerawatIntervensiId = x.PerawatIntervensiId,
                             ParafPerawatIntervensi = x.ParafPerawatIntervensi,

                             WaktuKajianUlang = x.WaktuKajianUlang,
                             Keterangan = x.Keterangan
                         }).OrderByDescending(x => x.CreateDateTime);

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
            var listdata = _applicationDbContext.MonitoringNyeris.Find(id);
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
        public async Task<IActionResult> Create([FromBody] MonitoringNyeriViewModel vm)
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
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                //                    && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new MonitoringNyeri
                {
                    MonitoringNyeriId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    WaktuMonitoringNyeri = vm.WaktuMonitoringNyeri,
                    SkorNyeri = vm.SkorNyeri,
                    SkorSedasi = vm.SkorSedasi,
                    Sistolik = vm.Sistolik,
                    Diastolic = vm.Diastolic,
                    Nadi = vm.Nadi,
                    Respirasi = vm.Respirasi,
                    Suhu = vm.Suhu,

                    PerawatMonitoringId = vm.PerawatMonitoringId,
                    ParafPerawatMonitoring = vm.ParafPerawatMonitoring,

                    WaktuIntervensi = vm.WaktuIntervensi,
                    ObatId = vm.ObatId,
                    Dosis = vm.Dosis,
                    Rute = vm.Rute,
                    IntervensiNonFarmakologi = vm.IntervensiNonFarmakologi,

                    PerawatIntervensiId = vm.PerawatIntervensiId,
                    ParafPerawatIntervensi = vm.ParafPerawatIntervensi,

                    WaktuKajianUlang = vm.WaktuKajianUlang,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.MonitoringNyeris.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Monitoring Nyeri Created", new
                    {
                        Action = "create",
                        id = data.MonitoringNyeriId
                    });

                    return Created("", new { message = "Tambah Data Berhasil || 201 Created", id = data.MonitoringNyeriId });
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
        public async Task<IActionResult> Update(Guid id, [FromBody] MonitoringNyeriViewModel vm)
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
                var entity = await _applicationDbContext.MonitoringNyeris.FindAsync(id);
                if (entity == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                //                    && c.IsDelete == false && c.DiskonId != id);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Update Data**
                entity.WaktuMonitoringNyeri = vm.WaktuMonitoringNyeri;
                entity.SkorNyeri = vm.SkorNyeri;
                entity.SkorSedasi = vm.SkorSedasi;
                entity.Sistolik = vm.Sistolik;
                entity.Diastolic = vm.Diastolic;
                entity.Nadi = vm.Nadi;
                entity.Respirasi = vm.Respirasi;
                entity.Suhu = vm.Suhu;

                entity.PerawatMonitoringId = vm.PerawatMonitoringId;
                entity.ParafPerawatMonitoring = vm.ParafPerawatMonitoring;

                entity.WaktuIntervensi = vm.WaktuIntervensi;
                entity.ObatId = vm.ObatId;
                entity.Dosis = vm.Dosis;
                entity.Rute = vm.Rute;
                entity.IntervensiNonFarmakologi = vm.IntervensiNonFarmakologi;

                entity.PerawatIntervensiId = vm.PerawatIntervensiId;
                entity.ParafPerawatIntervensi = vm.ParafPerawatIntervensi;

                entity.WaktuKajianUlang = vm.WaktuKajianUlang;
                entity.Keterangan = vm.Keterangan;

                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.MonitoringNyeris.Update(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Monitoring Nyeri Updated", new
                    {
                        Action = "update",
                        id = entity.MonitoringNyeriId
                    });

                    return Ok(new { message = "Update Data Berhasil || 200 OK", id = entity.MonitoringNyeriId });
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
                var data = await _applicationDbContext.MonitoringNyeris.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.MonitoringNyeris.Update(data);
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
            var query = (from x in _applicationDbContext.MonitoringNyeris
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on x.CreateBy equals u.UserActiveId

                         join o in _applicationDbContext.Obats
                         on x.ObatId equals o.ObatId into oGroup
                         from o in oGroup.DefaultIfEmpty()

                         where x.IsDelete == false || x.IsDelete == null
                         select new
                         {
                             CreateByName = u.FullName,
                             x.CreateDateTime,
                             MonitoringNyeriId = x.MonitoringNyeriId,
                             KunjunganId = x.KunjunganId,
                             PasienId = x.PasienId,
                             WaktuMonitoringNyeri = x.WaktuMonitoringNyeri,

                             SkorNyeri = x.SkorNyeri,
                             SkorSedasi = x.SkorSedasi,
                             Sistolik = x.Sistolik,
                             Diastolic = x.Diastolic,
                             Nadi = x.Nadi,
                             Respirasi = x.Respirasi,
                             Suhu = x.Suhu,

                             PerawatMonitoringId = x.PerawatMonitoringId,
                             ParafPerawatMonitoring = x.ParafPerawatMonitoring,

                             WaktuIntervensi = x.WaktuIntervensi,
                             ObatId = x.ObatId,
                             NamaObat = o.ObatName,
                             Dosis = x.Dosis,
                             Rute = x.Rute,
                             IntervensiNonFarmakologi = x.IntervensiNonFarmakologi,

                             PerawatIntervensiId = x.PerawatIntervensiId,
                             ParafPerawatIntervensi = x.ParafPerawatIntervensi,

                             WaktuKajianUlang = x.WaktuKajianUlang,
                             Keterangan = x.Keterangan
                         });

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


            // filter based on pasien id 
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
