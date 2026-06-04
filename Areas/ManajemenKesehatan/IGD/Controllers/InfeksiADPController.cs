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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
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
    public class InfeksiADPController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InfeksiADPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InfeksiADPController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InfeksiADPController> logger,
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
            var query = (from a in _applicationDbContext.InfeksiADPs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InfeksiADPId,
                             a.KunjunganId,
                             a.PasienId,
                             a.IsInfusVenaPerifer,
                             a.IsCVP,
                             a.IsKateterDarah,
                             a.HasilLabHB,
                             a.HasilLabLeokosit,
                             a.TglPencatatan,
                             a.Keterangan,
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
            var listdata = _applicationDbContext.InfeksiADPs.Find(id);
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
        public async Task<IActionResult> Create([FromBody] InfeksiADPViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ===========================
                // 🔹 Ambil User Login
                // ===========================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (emailLogin == null)
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = user.UserActiveId;

                // ===========================
                // 🔹 Create parent : InfeksiADP
                // ===========================
                var infeksiADP = new InfeksiADP
                {
                    InfeksiADPId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    IsInfusVenaPerifer = vm.IsInfusVenaPerifer,
                    IsCVP = vm.IsCVP,
                    IsKateterDarah = vm.IsKateterDarah,
                    HasilLabLeokosit = vm.HasilLabLeokosit,
                    HasilLabHB = vm.HasilLabHB,
                    TglPencatatan = vm.TglPencatatan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiADPs.Add(infeksiADP);
                await _applicationDbContext.SaveChangesAsync();

                // ===========================
                // 🔹 Insert Child : InfeksiDetail
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // Hitung HariKe dari DB
                        int hariKe = await _applicationDbContext.InfeksiDetails
                            .CountAsync(x => x.InfeksiId == infeksiADP.InfeksiADPId
                                             && x.KunjunganId == vm.KunjunganId) + 1;

                        // Ambil suhu vital sign terbaru
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        var detail = new InfeksiDetail
                        {
                            DetailInfeksiId = Guid.NewGuid(),
                            InfeksiId = infeksiADP.InfeksiADPId,
                            KunjunganId = vm.KunjunganId,
                            PasienId = vm.PasienId,

                            HariKe = hariKe,
                            LokasiReaksi = d.LokasiReaksi,
                            TglMulaiReaksi = d.TglMulaiReaksi,
                            TglAkhirReaksi = d.TglAkhirReaksi,
                            Nyeri = d.Nyeri,
                            Merah = d.Merah,
                            Bengkak = d.Bengkak,
                            PUS = d.PUS,
                            Menggigil = d.Menggigil,
                            IsDemam = d.IsDemam ?? suhu >= 38,  // fallback dari suhu vital sign
                            Drainase = d.Drainase,
                            Perforasi = d.Perforasi,
                            Fistula = d.Fistula,
                            NyeriSupraPublik = d.NyeriSupraPublik,
                            NyeriSaatBerkemih = d.NyeriSaatBerkemih,
                            PasangDCKe = d.PasangDCKe,
                            AnyangAnyangan = d.AnyangAnyangan,
                            Gatal = d.Gatal,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.InfeksiDetails.Add(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Tambah Data Infeksi ADP + Detail berhasil",
                    InfeksiADPId = infeksiADP.InfeksiADPId,
                    JumlahDetail = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiADPViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ===============================
                // 🔹 Ambil user login
                // ===============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (emailLogin == null)
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = user.UserActiveId;

                // ===============================
                // 🔹 Ambil data ADP dari DB
                // ===============================
                var adp = await _applicationDbContext.InfeksiADPs
                    .FirstOrDefaultAsync(x => x.InfeksiADPId == id && (x.IsDelete == false || x.IsDelete == null));

                if (adp == null)
                    return NotFound(new { message = "Data Infeksi ADP tidak ditemukan" });

                // ===============================
                // 🔹 Update parent ADP
                // ===============================
                adp.KunjunganId = vm.KunjunganId;
                adp.PasienId = vm.PasienId;
                adp.IsInfusVenaPerifer = vm.IsInfusVenaPerifer;
                adp.IsCVP = vm.IsCVP;
                adp.IsKateterDarah = vm.IsKateterDarah;
                adp.HasilLabLeokosit = vm.HasilLabLeokosit;
                adp.HasilLabHB = vm.HasilLabHB;
                adp.TglPencatatan = vm.TglPencatatan;
                adp.Keterangan = vm.Keterangan;

                adp.UpdateBy = userActiveId;
                adp.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                // ===============================
                // 🔹 Hapus semua detail lama
                // ===============================
                var oldDetails = _applicationDbContext.InfeksiDetails
                    .Where(x => x.InfeksiId == id);

                _applicationDbContext.InfeksiDetails.RemoveRange(oldDetails);
                await _applicationDbContext.SaveChangesAsync();

                // ===============================
                // 🔹 Tambahkan detail baru
                // ===============================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // Hitung HariKe berdasarkan Kunjungan & Infeksi
                        int hariKe = await _applicationDbContext.InfeksiDetails
                            .CountAsync(x => x.KunjunganId == vm.KunjunganId &&
                                             x.InfeksiId == id) + 1;

                        // Ambil suhu vital sign terbaru
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        var detail = new InfeksiDetail
                        {
                            DetailInfeksiId = Guid.NewGuid(),
                            InfeksiId = id,
                            KunjunganId = vm.KunjunganId,
                            PasienId = vm.PasienId,

                            HariKe = hariKe,

                            LokasiReaksi = d.LokasiReaksi,
                            TglMulaiReaksi = d.TglMulaiReaksi,
                            TglAkhirReaksi = d.TglAkhirReaksi,
                            Nyeri = d.Nyeri,
                            Merah = d.Merah,
                            Bengkak = d.Bengkak,
                            PUS = d.PUS,
                            Menggigil = d.Menggigil,
                            IsDemam = d.IsDemam ?? (suhu >= 38),
                            Drainase = d.Drainase,
                            Perforasi = d.Perforasi,
                            Fistula = d.Fistula,
                            NyeriSupraPublik = d.NyeriSupraPublik,
                            NyeriSaatBerkemih = d.NyeriSaatBerkemih,
                            PasangDCKe = d.PasangDCKe,
                            AnyangAnyangan = d.AnyangAnyangan,
                            Gatal = d.Gatal,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.InfeksiDetails.Add(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update Data Infeksi ADP + Detail berhasil",
                    InfeksiADPId = id,
                    JumlahDetailBaru = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                var data = await _applicationDbContext.InfeksiADPs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InfeksiADPs.Update(data);

                // ================================
                // 🔹 Soft Delete Semua Detail
                // ================================
                var details = await _applicationDbContext.InfeksiDetails
                    .Where(d => d.InfeksiId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                if (details.Any())
                {
                    foreach (var d in details)
                    {
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = DateTimeOffset.UtcNow;
                        d.IsDelete = true;
                    }

                    _applicationDbContext.InfeksiDetails.UpdateRange(details);
                }
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
        public async Task<IActionResult> Paged(
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
            if (page <= 0) page = 1;
            if (perPage <= 0) perPage = 10;

            // === 1. Base Query (all columns) ===
            var query = _applicationDbContext.InfeksiADPs
                .AsNoTracking()
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .AsQueryable();

            // === 2. Filters (DB side) ===
            if (kunjunganId.HasValue)
                query = query.Where(a => a.KunjunganId == kunjunganId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(a => a.CreateDateTime >= startUtc && a.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime startPeriod;
                DateTime endPeriod = today.AddDays(1).AddTicks(-1);

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        startPeriod = today;
                        break;
                    case PeriodeFilter.ThisWeek:
                        startPeriod = today.AddDays(-(int)today.DayOfWeek);
                        break;
                    case PeriodeFilter.LastWeek:
                        startPeriod = today.AddDays(-7 - (int)today.DayOfWeek);
                        endPeriod = today.AddDays(-(int)today.DayOfWeek).AddTicks(-1);
                        break;
                    case PeriodeFilter.ThisMonth:
                        startPeriod = new DateTime(today.Year, today.Month, 1);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        startPeriod = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                        endPeriod = new DateTime(lastMonth.Year, lastMonth.Month,
                            DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)).AddDays(1).AddTicks(-1);
                        break;
                    case PeriodeFilter.ThisYear:
                        startPeriod = new DateTime(today.Year, 1, 1);
                        break;
                    case PeriodeFilter.LastYear:
                        startPeriod = new DateTime(today.Year - 1, 1, 1);
                        endPeriod = new DateTime(today.Year - 1, 12, 31).AddDays(1).AddTicks(-1);
                        break;
                    case PeriodeFilter.Last3Months:
                        startPeriod = today.AddMonths(-3);
                        break;
                    case PeriodeFilter.Last6Months:
                        startPeriod = today.AddMonths(-6);
                        break;
                    default:
                        startPeriod = DateTime.MinValue;
                        break;
                }

                var startUtc = startPeriod.Date.ToUniversalTime();
                var endUtc = endPeriod.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(a => a.CreateDateTime >= startUtc && a.CreateDateTime <= endUtc);
            }

            // === 3. Sorting (DB side) ===
            bool desc = sortDirection?.ToLower() == "desc";

            query = (orderBy, desc) switch
            {

                ("CreateDateTime", true) => query.OrderByDescending(a => a.CreateDateTime),
                ("CreateDateTime", false) => query.OrderBy(a => a.CreateDateTime),
                _ => desc ? query.OrderByDescending(a => a.CreateDateTime)
                         : query.OrderBy(a => a.CreateDateTime)
            };

            // === 4. Count BEFORE paging ===
            var filteredTotal = await query.CountAsync();

            if (filteredTotal == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            // === 5. Apply paging (only once) ===
            var parents = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(); // all columns included here

            // === 6. Fetch details for these parents ===
            var parentIds = parents.Select(x => x.InfeksiADPId).ToList();

            var details = await _applicationDbContext.InfeksiDetails
                .AsNoTracking()
                .Where(d => parentIds.Contains((Guid)d.InfeksiId))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync();

            var detailLookup = details.GroupBy(d => d.InfeksiId)
                                      .ToDictionary(g => g.Key, g => g.ToList());

            // === 7. Merge parents + details ===
            var final = parents.Select(p => new
            {
                Rows = p, // BERISI SEMUA KOLOM INFEKSIADP
                Details = detailLookup.ContainsKey(p.InfeksiADPId)
                    ? detailLookup[p.InfeksiADPId]
                    : new List<InfeksiDetail>()
            }).ToList();

            // === 8. Response ===
            return Ok(new
            {
                status = "success",
                message = "Data retrieved",
                data = new
                {
                    Rows = final,
                    TotalRows = filteredTotal,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(filteredTotal / (double)perPage)
                }
            });
        }


    }
}
