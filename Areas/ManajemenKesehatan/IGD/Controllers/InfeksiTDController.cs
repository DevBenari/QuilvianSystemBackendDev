using System.Linq;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class InfeksiTDController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InfeksiTDController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InfeksiTDController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InfeksiTDController> logger,
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
            var query = (from a in _applicationDbContext.InfeksiTDs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InfeksiTransfusiId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TglTransfusi,
                             a.JenisTransfusi,
                             a.Jumlah,
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
            var listdata = _applicationDbContext.InfeksiTDs.Find(id);
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
        public async Task<IActionResult> Create([FromBody] InfeksiTDViewMOdel vm)
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
                // 🔹 Insert Parent: InfeksiTD
                // ===========================
                var infeksiId = Guid.NewGuid();

                var infeksi = new InfeksiTD
                {
                    InfeksiTransfusiId = infeksiId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TglTransfusi = vm.TglTransfusi,
                    JenisTransfusi = vm.JenisTransfusi,
                    Jumlah = vm.Jumlah,
                    TglPencatatan = vm.TglPencatatan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiTDs.Add(infeksi);
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
                            IsDemam = d.IsDemam ?? suhu >= 38, // fallback jika suhu ≥ 38
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


                // ===========================
                // 🔹 Commit Transaction
                // ===========================
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Tambah Data Infeksi TD + Detail berhasil",
                    InfeksiTDId = infeksiId,
                    JumlahDetail = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiTDViewMOdel vm)
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
                // 🔹 Ambil Parent InfeksiTD
                // ===========================
                var infeksi = await _applicationDbContext.InfeksiTDs
                    .FirstOrDefaultAsync(x => x.InfeksiTransfusiId == id);

                if (infeksi == null)
                    return NotFound(new { message = "Data Infeksi TD tidak ditemukan!" });


                // ===========================
                // 🔹 Update Parent
                // ===========================
                infeksi.KunjunganId = vm.KunjunganId;
                infeksi.PasienId = vm.PasienId;
                infeksi.TglTransfusi = vm.TglTransfusi;
                infeksi.JenisTransfusi = vm.JenisTransfusi;
                infeksi.Jumlah = vm.Jumlah;
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
                // 🔹 UPDATE DETAIL LAMA SAJA
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // ❗ Jika tidak punya DetailInfeksiId → SKIP (Karena tidak boleh tambah baru)
                        if (d.InfeksiId == null)
                            continue;

                        var existing = existingDetails
                            .FirstOrDefault(x => x.InfeksiId == id);

                        // ❗ Jika detail lama tidak ditemukan → SKIP
                        if (existing == null)
                            continue;

                        // ===========================
                        // 🔹 Ambil suhu vital sign terbaru
                        // ===========================
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        // ===========================
                        // 🔹 UPDATE DETAIL
                        // ===========================
                        existing.LokasiReaksi = d.LokasiReaksi;
                        existing.TglMulaiReaksi = d.TglMulaiReaksi;
                        existing.TglAkhirReaksi = d.TglAkhirReaksi;
                        existing.Nyeri = d.Nyeri;
                        existing.Merah = d.Merah;
                        existing.Bengkak = d.Bengkak;
                        existing.PUS = d.PUS;
                        existing.Menggigil = d.Menggigil;
                        existing.IsDemam = d.IsDemam ?? suhu >= 38;
                        existing.HariKe = d.HariKe;

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
                    message = "Update Infeksi TD + Detail berhasil",
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
                var data = await _applicationDbContext.InfeksiTDs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InfeksiTDs.Update(data);
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
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // ===================================================
            // 1) BASE QUERY (Parent Only)
            // ===================================================
            var baseQuery =
                from a in _applicationDbContext.InfeksiTDs
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId into userJoin
                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null

                select new
                {
                    a.InfeksiTransfusiId,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.KunjunganId,
                    a.PasienId,
                    a.TglTransfusi,
                    a.JenisTransfusi,
                    a.Jumlah,
                    a.TglPencatatan,
                    a.Keterangan
                };

            // ===================================================
            // 2) SQL FILTERING (Parent-level only)
            // ===================================================
            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(x => x.KunjunganId == kunjunganId);

            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                baseQuery = baseQuery.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // ===================================================
            // 3) SQL SORTING
            // ===================================================
            var sorted = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => baseQuery.OrderByDescending(x => x.CreateByName),
                    _ => baseQuery.OrderByDescending(x => x.CreateDateTime)
                    
                }
                : orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => baseQuery.OrderBy(x => x.CreateByName),
                    _ => baseQuery.OrderBy(x => x.CreateDateTime)
                };

            // ===================================================
            // 4) PAGING PARENT (NO DETAIL HERE)
            // ===================================================
            var totalRows = sorted.Count();

            var parentRows = sorted
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!parentRows.Any())
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
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

            // ===================================================
            // 5) LOAD DETAIL SEKALIGUS (No N+1 Query)
            // ===================================================
            var parentIds = parentRows.Select(x => x.InfeksiTransfusiId).ToList();

            var details = _applicationDbContext.InfeksiDetails
                .Where(d => parentIds.Contains((Guid)d.InfeksiId))
                .OrderBy(d => d.CreateDateTime)
                .ToList();

            // ===================================================
            // 6) MERGE PARENT + DETAIL (In Memory)
            // ===================================================
            var merged = parentRows.Select(p => new
            {
                p.InfeksiTransfusiId,
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.KunjunganId,
                p.PasienId,
                p.TglTransfusi,
                p.JenisTransfusi,
                p.Jumlah,
                p.TglPencatatan,
                p.Keterangan,

                Details = details
                    .Where(d => d.InfeksiId == p.InfeksiTransfusiId)
                    .Select(d => new
                    {
                        d.DetailInfeksiId,
                        d.HariKe,
                        d.LokasiReaksi,
                        d.TglMulaiReaksi,
                        d.TglAkhirReaksi,
                        d.Nyeri,
                        d.Merah,
                        d.Bengkak,
                        d.PUS,
                        d.Menggigil,
                        d.IsDemam,
                        d.Drainase,
                        d.Perforasi,
                        d.Fistula,
                        d.NyeriSupraPublik,
                        d.NyeriSaatBerkemih,
                        d.PasangDCKe,
                        d.AnyangAnyangan,
                        d.Gatal,
                        d.Keterangan,
                        d.CreateDateTime
                    })
                    .ToList()
            }).ToList();

            // ===================================================
            // 7) MEMORY FILTERING (AFTER MERGE)
            // ===================================================
            if (kunjunganId.HasValue)
                merged = merged.Where(x => x.KunjunganId == kunjunganId).ToList();

            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                merged = merged
                    .Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc)
                    .ToList();
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        merged = merged.Where(x => x.CreateDateTime.Date == today).ToList();
                        break;

                    case PeriodeFilter.ThisWeek:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today).ToList();
                        break;

                    case PeriodeFilter.LastWeek:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)).ToList();
                        break;

                    case PeriodeFilter.ThisMonth:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year).ToList();
                        break;

                    case PeriodeFilter.LastMonth:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Month == today.Month - 1 &&
                            x.CreateDateTime.Year == today.Year).ToList();
                        break;

                    case PeriodeFilter.ThisYear:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Year == today.Year).ToList();
                        break;

                    case PeriodeFilter.LastYear:
                        merged = merged.Where(x =>
                            x.CreateDateTime.Year == today.Year - 1).ToList();
                        break;

                    case PeriodeFilter.Last3Months:
                        merged = merged.Where(x =>
                            x.CreateDateTime >= today.AddMonths(-3)).ToList();
                        break;

                    case PeriodeFilter.Last6Months:
                        merged = merged.Where(x =>
                            x.CreateDateTime >= today.AddMonths(-6)).ToList();
                        break;
                }
            }

            // ===================================================
            // 8) FINAL PAGING AFTER MERGE
            // ===================================================
            var filteredTotal = merged.Count;

            var finalPaged = merged
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            var totalPages = (int)Math.Ceiling(filteredTotal / (double)perPage);

            // ===================================================
            // 9) RETURN RESPONSE
            // ===================================================
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = finalPaged,
                    TotalRows = filteredTotal,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }

    }
}
