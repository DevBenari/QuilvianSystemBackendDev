using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class DiskonDireksiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHubContext<DiskonApprovedHub> _hubContext;
        private readonly ILogger<DiskonDireksiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DiskonDireksiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DiskonDireksiController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<DiskonApprovedHub> hubContext)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }
        //test
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = 
                from a in _applicationDbContext.DiskonDireksis.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into uG
                from u in uG.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on a.PasienId equals p.PendaftaranPasienBaruId into pG
                from p in pG.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k.KunjunganID into kG
                from k in kG.DefaultIfEmpty()

                join d in _applicationDbContext.Diskons.AsNoTracking()
                    on a.DiskonId equals d.DiskonId into dG
                from d in dG.DefaultIfEmpty()

                join a1 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved1Id equals a1.UserActiveId into a1G
                from a1 in a1G.DefaultIfEmpty()

                join a2 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved2Id equals a2.UserActiveId into a2G
                from a2 in a2G.DefaultIfEmpty()

                join a3 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved3Id equals a3.UserActiveId into a3G
                from a3 in a3G.DefaultIfEmpty()

                where a.IsDelete == false && a.DiskonAprrovedId == id
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    a.DiskonAprrovedId,

                    a.DiskonId,
                    NamaDiskon = d != null ? d.NamaDiskon : null,

                    a.KunjunganId,
                    JenisKunjungan = k != null ? k.JenisKunjungan : null,
                    AsalKunjungan = k != null ? k.AsalKunjungan : null,

                    a.PasienId,
                    NamaLengkap = p != null ? p.NamaLengkap : null,
                    NoRekamMedis = p != null ? p.NoRekamMedis : null,

                    a.Approved1Id,
                    Approved1Name = a1 != null ? a1.FullName : null,
                    a.IsApproved1,
                    a.ApprovedDate1,

                    a.Approved2Id,
                    Approved2Name = a2 != null ? a2.FullName : null,
                    a.IsApproved2,
                    a.ApprovedDate2,

                    a.Approved3Id,
                    Approved3Name = a3 != null ? a3.FullName : null,
                    a.IsApproved3,
                    a.ApprovedDate3
                };
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
        //test
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiskonDireksiViewModel vm)
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

                //cek duplikasi
                //bool isDuplicate = await _applicationDbContext.AnastesiTipes
                //    .AnyAsync(c => c.NamaTipeAnastesi.ToLower().Trim()
                //    == vm.NamaTipeAnastesi.ToLower().Trim() && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Tipe Anastesi ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new DiskonDireksi
                {
                    DiskonAprrovedId = Guid.NewGuid(),
                    DiskonId = vm.DiskonId,
                    PasienId = vm.PasienId,
                    KunjunganId = vm.KunjunganId,

                    IsApproved1 = false,
                    IsApproved2 = false,
                    IsApproved3 = false,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.DiskonDireksis.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon Approved ditambah", new
                    {
                        action = "create",
                        diskonAprrovedId = data.DiskonAprrovedId,
                    });

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

        [HttpPut("Diskon-Approval1/{id}")]
        public async Task<IActionResult> DiskonApproval1(Guid id, [FromBody] DiskonApprovalViewModel vm)
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
                var data = await _applicationDbContext.DiskonDireksis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.Approved1Id = vm.ApprovedId;
                data.IsApproved1 = true;
                data.ApprovedDate1 = vm.ApprovedDate;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DiskonDireksis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon Approved 1 telah diupdate", new
                    {
                        action = "update",
                        diskonAprrovedId = data.DiskonAprrovedId,
                        approvalId1 = data.Approved1Id,
                    });

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

        [HttpPut("Diskon-Approval2/{id}")]
        public async Task<IActionResult> DiskonApproval2(Guid id, [FromBody] DiskonApprovalViewModel vm)
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
                var data = await _applicationDbContext.DiskonDireksis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.Approved2Id = vm.ApprovedId;
                data.IsApproved2 = true;
                data.ApprovedDate2 = vm.ApprovedDate;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DiskonDireksis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon Approved 2 telah diupdate", new
                    {
                        action = "update",
                        diskonAprrovedId = data.DiskonAprrovedId,
                        approvalId2 = data.Approved2Id,
                    });

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
        
        [HttpPut("Diskon-Approval3/{id}")]
        public async Task<IActionResult> DiskonApproval3(Guid id, [FromBody] DiskonApprovalViewModel vm)
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
                var data = await _applicationDbContext.DiskonDireksis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.Approved3Id = vm.ApprovedId;
                data.IsApproved3 = true;
                data.ApprovedDate3 = vm.ApprovedDate;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DiskonDireksis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon Approved 3 telah diupdate", new
                    {
                        action = "update",
                        diskonAprrovedId = data.DiskonAprrovedId,
                        approvalId3 = data.Approved3Id,
                    });

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
                var data = await _applicationDbContext.DiskonDireksis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DiskonDireksis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon approved dihapus", new
                    {
                        action = "delete",
                        diskonApprovedID = id
                    });

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
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        Guid? diskonId = null,
        Guid? kunjunganId = null,
        Guid? pasienId = null,
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
            var query =
                from a in _applicationDbContext.DiskonDireksis.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into uG
                from u in uG.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on a.PasienId equals p.PendaftaranPasienBaruId into pG
                from p in pG.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k.KunjunganID into kG
                from k in kG.DefaultIfEmpty()

                join d in _applicationDbContext.Diskons.AsNoTracking()
                    on a.DiskonId equals d.DiskonId into dG
                from d in dG.DefaultIfEmpty()

                join a1 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved1Id equals a1.UserActiveId into a1G
                from a1 in a1G.DefaultIfEmpty()

                join a2 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved2Id equals a2.UserActiveId into a2G
                from a2 in a2G.DefaultIfEmpty()

                join a3 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.Approved3Id equals a3.UserActiveId into a3G
                from a3 in a3G.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    a.DiskonAprrovedId,

                    a.DiskonId,
                    NamaDiskon = d != null ? d.NamaDiskon : null,

                    a.KunjunganId,
                    JenisKunjungan = k != null ? k.JenisKunjungan : null,
                    AsalKunjungan = k != null ? k.AsalKunjungan : null,

                    a.PasienId,
                    NamaLengkap = p != null ? p.NamaLengkap : null,
                    NoRekamMedis = p != null ? p.NoRekamMedis : null,

                    a.Approved1Id,
                    Approved1Name = a1 != null ? a1.FullName : null,
                    a.IsApproved1,
                    a.ApprovedDate1,

                    a.Approved2Id,
                    Approved2Name = a2 != null ? a2.FullName : null,
                    a.IsApproved2,
                    a.ApprovedDate2,

                    a.Approved3Id,
                    Approved3Name = a3 != null ? a3.FullName : null,
                    a.IsApproved3,
                    a.ApprovedDate3
                };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaDiskon, search) ||
                    EF.Functions.ILike(u.NamaLengkap, search) ||
                    EF.Functions.ILike(u.NoRekamMedis, search)
                );
            }

            // filter based on diskon id
            if (diskonId.HasValue)
            {
                query = query.Where(u=>u.DiskonId == diskonId.Value);
            }

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u => u.KunjunganId == kunjunganId.Value);
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
