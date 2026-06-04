using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]

    public class DepositRanapController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDepositRanapNumberService _depositRanapNumberService;
        private readonly ILogger<DepositRanapController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DepositRanapController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DepositRanapController> logger,
            IWebHostEnvironment webHostEnvironment,
            IDepositRanapNumberService depositRanapNumberService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _depositRanapNumberService = depositRanapNumberService;
        }

        [HttpGet("by-KunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganId)
        {
            var listdata = await (
                    from a in _applicationDbContext.DepositRanaps.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k.KunjunganID into kGroup
                    from k in kGroup.DefaultIfEmpty()

                    join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p.PendaftaranPasienBaruId into pGroup
                    from p in pGroup.DefaultIfEmpty()

                    where a.KunjunganId == kunjunganId && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.DepositRanapId,
                        a.KunjunganId,
                        NamaPasien = p != null ? p.NamaLengkap : null,
                        k.JenisKunjungan,
                        k.AsalKunjungan,
                        a.NoKwitansi,
                        a.TglTransaksi,
                        a.NominalMasuk,
                        a.NominalKeluar,
                        a.SaldoDeposit,
                        a.StatusDeposit,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    }
                ).ToListAsync();
            if (listdata == null)
            {
                return NotFound(new { message = "Tidak ada riwayat deposit untuk kunjungan ini." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = await (
                    from a in _applicationDbContext.DepositRanaps.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k.KunjunganID into kGroup
                    from k in kGroup.DefaultIfEmpty()

                    join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p.PendaftaranPasienBaruId into pGroup
                    from p in pGroup.DefaultIfEmpty()

                    where a.DepositRanapId == id && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.DepositRanapId,
                        a.KunjunganId,
                        NamaPasien = p != null ? p.NamaLengkap : null,
                        NoRekamMedis = p != null ? p.NoRekamMedis : null, 
                        k.JenisKunjungan,
                        k.AsalKunjungan,
                        a.NoKwitansi,
                        a.TglTransaksi,
                        a.NominalMasuk,
                        a.NominalKeluar,
                        a.SaldoDeposit,
                        a.StatusDeposit,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    }
                ).FirstOrDefaultAsync();
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
        public async Task<IActionResult> CreateDepositRanap([FromBody] DepositoRanapViewModel vm, CancellationToken ct)
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

                // **Ambil Kode Terakhir**
                var lastSaldo = await _applicationDbContext.DepositRanaps
                    .Where(d => d.KunjunganId == vm.KunjunganId)
                    .OrderByDescending(d => d.CreateDateTime) // Urutkan dari yang paling baru
                    .Select(d => d.SaldoDeposit)
                    .FirstOrDefaultAsync() ?? 0m; // Ambil satu yang paling atas

                decimal nominalMasuk = vm.NominalMasuk ?? 0m;
                decimal nominalKeluar = vm.NominalKeluar ?? 0m;
                decimal currentSaldo = lastSaldo;
                decimal saldo;

                if (nominalMasuk > 0m && nominalKeluar == 0m)
                {
                    saldo = currentSaldo + nominalMasuk;
                }
                else if (nominalKeluar > 0m && nominalMasuk == 0m)
                {
                    if (nominalKeluar > currentSaldo)
                    {
                        return BadRequest(new
                        {
                            message = "Nominal keluar tidak boleh melebihi saldo terakhir."
                        });
                    }

                    saldo = currentSaldo - nominalKeluar;
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Hanya salah satu nominal masuk atau nominal keluar yang boleh diisi lebih dari 0."
                    });
                }

                var noKwitansi = await _depositRanapNumberService.GenerateNoKwitansiAsync(ct);

                // **Buat Data Baru**
                var data = new DepositRanap
                {
                   DepositRanapId = Guid.NewGuid(),
                   KunjunganId = vm.KunjunganId,
                   TglTransaksi = DateTime.UtcNow,
                   NominalMasuk = vm.NominalMasuk,
                   NominalKeluar = vm.NominalKeluar,
                   SaldoDeposit = saldo,
                   NoKwitansi = noKwitansi,
                   StatusDeposit = vm.StatusDeposit,
                   Keterangan = vm.Keterangan,

                   CreateDateTime = DateTimeOffset.UtcNow,// Konversi ke UTC,
                   CreateBy = userActiveId,
                };

                // **Simpan ke Database**
                _applicationDbContext.DepositRanaps.Add(data);
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
                var data = await _applicationDbContext.DepositRanaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DepositRanaps.Update(data);
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
        string? statusDeposit = null,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        DateTime? startDate = null,
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
        CancellationToken ct = default)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;
                if (perPage > 100) perPage = 100;

                // ======================================================
                // 1) BASE QUERY (Join 1-to-1 aman, tidak duplikasi)
                // ======================================================
                var q =
                    from a in _applicationDbContext.DepositRanaps.AsNoTracking()
                    where (a.IsDelete == false || a.IsDelete == null)

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u0.UserActiveId into ug
                    from u in ug.DefaultIfEmpty()

                    join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                        on a.KunjunganId equals k0.KunjunganID into kg
                    from k in kg.DefaultIfEmpty()

                    join p0 in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                        on k.PasienId equals p0.PendaftaranPasienBaruId into pg
                    from p in pg.DefaultIfEmpty()

                    select new
                    {
                        a.DepositRanapId,
                        a.KunjunganId,
                        NamaPasien = p != null ? p.NamaLengkap : null,
                        NoRekamMedis = p != null ? p.NoRekamMedis : null,
                        k.JenisKunjungan,
                        k.AsalKunjungan,
                        a.NoKwitansi,
                        a.TglTransaksi,
                        a.NominalMasuk,
                        a.NominalKeluar,
                        a.SaldoDeposit,
                        a.StatusDeposit,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime

                    };

                // ======================================================
                // 2) FILTERS
                // ======================================================
                if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
                    q = q.Where(x => x.KunjunganId == kunjunganId.Value);

                if (!string.IsNullOrWhiteSpace(statusDeposit))
                {
                    var st = statusDeposit.Trim();
                    q = q.Where(x => x.StatusDeposit != null && EF.Functions.ILike(x.StatusDeposit, st));
                    // kalau kamu mau "contains", ganti jadi: EF.Functions.ILike(x.StatusDeposit, $"%{st}%")
                }

                // date range (sargable): >= start && < endExclusive
                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = new DateTimeOffset(DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc));
                    var endExclusive = new DateTimeOffset(DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc));

                    q = q.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                    // kalau kamu ingin berdasarkan TglTransaksi, ganti CreateDateTime -> TglTransaksi
                }

                // periode filter (dibuat range supaya tetap sargable)
                if (periode.HasValue)
                {
                    var today = DateTimeOffset.UtcNow.Date;
                    DateTimeOffset pStart;
                    DateTimeOffset pEndExclusive;

                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            pStart = today;
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.ThisWeek:
                            // minggu dimulai Sunday sesuai DayOfWeek bawaan .NET
                            pStart = today.AddDays(-(int)today.DayOfWeek);
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.LastWeek:
                            var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                            pStart = startThisWeek.AddDays(-7);
                            pEndExclusive = startThisWeek;
                            break;

                        case PeriodeFilter.ThisMonth:
                            pStart = new DateTimeOffset(new DateTime(today.Year, today.Month, 1), TimeSpan.Zero);
                            pEndExclusive = pStart.AddMonths(1);
                            break;

                        case PeriodeFilter.LastMonth:
                            var thisMonth = new DateTimeOffset(new DateTime(today.Year, today.Month, 1), TimeSpan.Zero);
                            pStart = thisMonth.AddMonths(-1);
                            pEndExclusive = thisMonth;
                            break;

                        case PeriodeFilter.ThisYear:
                            pStart = new DateTimeOffset(new DateTime(today.Year, 1, 1), TimeSpan.Zero);
                            pEndExclusive = pStart.AddYears(1);
                            break;

                        case PeriodeFilter.LastYear:
                            var thisYear = new DateTimeOffset(new DateTime(today.Year, 1, 1), TimeSpan.Zero);
                            pStart = thisYear.AddYears(-1);
                            pEndExclusive = thisYear;
                            break;

                        case PeriodeFilter.Last3Months:
                            pStart = today.AddMonths(-3);
                            pEndExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Last6Months:
                            pStart = today.AddMonths(-6);
                            pEndExclusive = today.AddDays(1);
                            break;

                        default:
                            pStart = today;
                            pEndExclusive = today.AddDays(1);
                            break;
                    }

                    q = q.Where(x => x.CreateDateTime >= pStart && x.CreateDateTime < pEndExclusive);
                }

                // search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    var pattern = $"%{s}%";

                    q = q.Where(x =>
                        (x.NamaPasien != null && EF.Functions.ILike(x.NamaPasien, pattern)) ||
                        (x.NoRekamMedis != null && EF.Functions.ILike(x.NoRekamMedis, pattern)) 
                    );
                }

                // ======================================================
                // 3) COUNT
                // ======================================================
                var totalRows = await q.CountAsync(ct);
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                if (totalRows == 0)
                {
                    return Ok(new
                    {
                        message = "No data found || 200 OK",
                        pagination = new
                        {
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalRows = 0,
                            TotalPages = 0
                        },
                        data = Array.Empty<object>()
                    });
                }

                if (page > totalPages)
                    return NotFound(new { message = "Page not found." });

                // ======================================================
                // 4) SORTING (di DB)
                // ======================================================
                var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

                q = orderBy?.Trim() switch
                {
                    "TglTransaksi" => desc ? q.OrderByDescending(x => x.TglTransaksi) : q.OrderBy(x => x.TglTransaksi),
                    "SaldoDeposit" => desc ? q.OrderByDescending(x => x.SaldoDeposit) : q.OrderBy(x => x.SaldoDeposit),
                    "NamaPasien" => desc ? q.OrderByDescending(x => x.NamaPasien) : q.OrderBy(x => x.NamaPasien),
                    _ => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                };

                // ======================================================
                // 5) PAGING
                // ======================================================
                var rows = await q
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync(ct);

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    },
                    data = rows
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}",
                    inner = ex.InnerException?.Message
                });
            }
        }

    }
}
