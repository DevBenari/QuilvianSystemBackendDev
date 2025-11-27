using System.Globalization;
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
    [EnableCors("AllowSpecific")]
    public class InfeksiSKController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InfeksiSKController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InfeksiSKController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InfeksiSKController> logger,
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
            var query = (from a in _applicationDbContext.InfeksiSKs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InfeksiSKId,
                             a.KunjunganId,
                             a.PasienId,
                             a.KateterUrin,
                             a.TglLeukositUrin1,
                             a.TglLeukositUrin2,
                             a.TglBiakanUrin1,
                             a.TglBiakanUrin2,
                             a.HasilBiakanUrin1,
                             a.HasilBiakanUrin2,
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
            var listdata = _applicationDbContext.InfeksiSKs.Find(id);
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
        public async Task<IActionResult> Create([FromBody] InfeksiSKViewModel vm)
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
                // 🔹 Insert Parent: Infeksi SK
                // ===========================
                var infeksiId = Guid.NewGuid();

                var infeksi = new InfeksiSK
                {
                    InfeksiSKId = infeksiId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    KateterUrin = vm.KateterUrin,
                    TglLeukositUrin1 = vm.TglLeukositUrin1,
                    TglLeukositUrin2 = vm.TglLeukositUrin2,
                    TglBiakanUrin1 = vm.TglBiakanUrin1,
                    TglBiakanUrin2 = vm.TglBiakanUrin2,
                    HasilBiakanUrin1 = vm.HasilBiakanUrin1,
                    HasilBiakanUrin2 = vm.HasilBiakanUrin2,
                    TglPencatatan = vm.TglPencatatan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiSKs.Add(infeksi);
                await _applicationDbContext.SaveChangesAsync();


                // ===========================
                // 🔹 Insert Child: InfeksiDetail
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // 🔹 Hitung Hari Ke otomatis
                        //int hariKe = await _applicationDbContext.InfeksiDetails
                        //    .CountAsync(x => x.KunjunganId == vm.KunjunganId) + 1;

                        // 🔹 Ambil suhu vital sign terbaru
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        var detail = new InfeksiDetail
                        {
                            DetailInfeksiId = Guid.NewGuid(),
                            InfeksiId = infeksiId,
                            KunjunganId = vm.KunjunganId,
                            PasienId = vm.PasienId,

                            HariKe = d.HariKe,

                            LokasiReaksi = d.LokasiReaksi,
                            TglMulaiReaksi = d.TglMulaiReaksi,
                            TglAkhirReaksi = d.TglAkhirReaksi,
                            Nyeri = d.Nyeri,
                            Merah = d.Merah,
                            Bengkak = d.Bengkak,
                            PUS = d.PUS,
                            Menggigil = d.Menggigil,
                            IsDemam = d.IsDemam ?? suhu >= 38,   // fallback from suhu
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

                        await _applicationDbContext.InfeksiDetails.AddAsync(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Tambah Data Infeksi SK + Detail berhasil",
                    InfeksiSKId = infeksiId,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiSKViewModel vm)
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
                // 🔹 Ambil Parent
                // ===========================
                var infeksi = await _applicationDbContext.InfeksiSKs
                    .FirstOrDefaultAsync(x => x.InfeksiSKId == id);

                if (infeksi == null)
                    return NotFound(new { message = "Data Infeksi SK tidak ditemukan!" });


                // ===========================
                // 🔹 Update Parent Infeksi SK
                // ===========================
                infeksi.KunjunganId = vm.KunjunganId;
                infeksi.PasienId = vm.PasienId;
                infeksi.KateterUrin = vm.KateterUrin;
                infeksi.TglLeukositUrin1 = vm.TglLeukositUrin1;
                infeksi.TglLeukositUrin2 = vm.TglLeukositUrin2;
                infeksi.TglBiakanUrin1 = vm.TglBiakanUrin1;
                infeksi.TglBiakanUrin2 = vm.TglBiakanUrin2;
                infeksi.HasilBiakanUrin1 = vm.HasilBiakanUrin1;
                infeksi.HasilBiakanUrin2 = vm.HasilBiakanUrin2;
                infeksi.TglPencatatan = vm.TglPencatatan;
                infeksi.Keterangan = vm.Keterangan;

                infeksi.UpdateBy = userActiveId;
                infeksi.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();


                // ===========================
                // 🔹 Ambil semua detail lama
                // ===========================
                var existingDetails = await _applicationDbContext.InfeksiDetails
                    .Where(x => x.InfeksiId == id)
                    .ToListAsync();


                // ===========================
                // 🔹 Update DETAIL LAMA saja
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // Jika DetailInfeksiId tidak ada, abaikan (tidak boleh tambah baru)
                        if (d.InfeksiId == null)
                            continue;

                        var existing = existingDetails
                            .FirstOrDefault(x => x.InfeksiId == d.InfeksiId);

                        // Jika detail tidak ditemukan → skip (tidak add new)
                        if (existing == null)
                            continue;

                        // ===========================
                        // 🔹 UPDATE DETAIL EXISTING
                        // ===========================
                        existing.LokasiReaksi = d.LokasiReaksi;
                        existing.TglMulaiReaksi = d.TglMulaiReaksi;
                        existing.TglAkhirReaksi = d.TglAkhirReaksi;
                        existing.Nyeri = d.Nyeri;
                        existing.Merah = d.Merah;
                        existing.Bengkak = d.Bengkak;
                        existing.PUS = d.PUS;
                        existing.Menggigil = d.Menggigil;
                        existing.HariKe = d.HariKe;

                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        existing.IsDemam = d.IsDemam ?? (vital?.Suhu >= 38);

                        existing.Drainase = d.Drainase;
                        existing.Perforasi = d.Perforasi;
                        existing.Fistula = d.Fistula;
                        existing.NyeriSupraPublik = d.NyeriSupraPublik;
                        existing.NyeriSaatBerkemih = d.NyeriSaatBerkemih;
                        existing.PasangDCKe = d.PasangDCKe;
                        existing.AnyangAnyangan = d.AnyangAnyangan;
                        existing.Gatal = d.Gatal;
                        existing.Keterangan = d.Keterangan;

                        existing.UpdateBy = userActiveId;
                        existing.UpdateDateTime = DateTimeOffset.UtcNow;
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                // ===========================
                // 🔹 Commit
                // ===========================
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update parent & detail berhasil",
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
                var data = await _applicationDbContext.InfeksiSKs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InfeksiSKs.Update(data);

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

            // === 1. Base Query (all parent columns) ===
            var query = _applicationDbContext.InfeksiSKs
                .AsNoTracking()
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .AsQueryable();

            // === 2. DB Side Filtering ===
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

                switch (periode)
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

                var startUtc = startPeriod.ToUniversalTime();
                var endUtcFinal = endPeriod.ToUniversalTime();

                query = query.Where(a => a.CreateDateTime >= startUtc && a.CreateDateTime <= endUtcFinal);
            }

            // === 3. Sorting (DB-side) ===
            bool desc = sortDirection?.ToLower() == "desc";

            query = (orderBy, desc) switch
            {
                ("CreateDateTime", true) => query.OrderByDescending(a => a.CreateDateTime),
                ("CreateDateTime", false) => query.OrderBy(a => a.CreateDateTime),
                ("CreateByName", true) => query.OrderByDescending(a => a.CreateBy),
                ("CreateByName", false) => query.OrderBy(a => a.CreateBy),
                _ => desc ? query.OrderByDescending(a => a.CreateDateTime)
                         : query.OrderBy(a => a.CreateDateTime)
            };

            // === 4. Count before paging ===
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

            // === 5. Apply paging ===
            var parents = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            // === 6. Fetch details ===
            var parentIds = parents.Select(x => x.InfeksiSKId).ToList();

            var details = await _applicationDbContext.InfeksiDetails
                .AsNoTracking()
                .Where(d => parentIds.Contains((Guid)d.InfeksiId))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync();

            var detailLookup = details
                .GroupBy(d => d.InfeksiId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // === 7. Merge parent + details ===
            var final = parents.Select(p => new
            {
                Rows = p,  // All columns, no filtering
                Details = detailLookup.ContainsKey(p.InfeksiSKId)
                    ? detailLookup[p.InfeksiSKId]
                    : new List<InfeksiDetail>()
            }).ToList();

            // === 8. Return response ===
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
