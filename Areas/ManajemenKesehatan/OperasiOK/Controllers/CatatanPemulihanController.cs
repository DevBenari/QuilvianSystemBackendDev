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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class CatatanPemulihanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<CatatanPemulihanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatatanPemulihanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CatatanPemulihanController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            // Header
            var header = await _applicationDbContext.CatatanPemulihans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CatatanPemulihanId == id &&
                    (x.IsDelete == false || x.IsDelete == null), ct);

            if (header == null)
                return NotFound(new { message = "Catatan pemulihan tidak ditemukan." });

            // Details (ambil semua detail yang terkait header id ini)
            var details = await _applicationDbContext.CatatanPemulihanDetails
                .AsNoTracking()
                .Where(d =>
                    d.CatatanPemulihanId == id &&                     
                    (d.IsDelete == false || d.IsDelete == null))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync(ct);

            return Ok(new
            {
                message = "Data retrieved successfully",
                header,            // entity penuh
                details,           // entity penuh
                totalDetails = details.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CatatanPemulihanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Details wajib diisi minimal 1 item." });

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // ✅ Generate Id header
                    var catatanPemulihanId = Guid.NewGuid();
                    var now = DateTimeOffset.UtcNow;

                    // 1) HEADER
                    var header = new CatatanPemulihan
                    {
                        CatatanPemulihanId = catatanPemulihanId,
                        KunjunganId = vm.KunjunganId,
                        PasienId = vm.PasienId,
                        DokterOperatorId = vm.DokterOperatorId,
                        PerawatId = vm.PerawatId,
                        WaktuMasuk = vm.WaktuMasuk,
                        InfusTransfusi = vm.InfusTransfusi,
                        JumlahUrine = vm.JumlahUrine,
                        Komplikasi = vm.Komplikasi,
                        Penatalaksanaan = vm.Penatalaksanaan,
                        InfusSedasi = vm.InfusSedasi,
                        Antibiotika = vm.Antibiotika,
                        Analgetik = vm.Analgetik,
                        AntiMuntah = vm.AntiMuntah,
                        Minum = vm.Minum,
                        PosisiPasien = vm.PosisiPasien,
                        Dipindahkan = vm.Dipindahkan,
                        WaktuKeluar = vm.WaktuKeluar,
                        PathDokterOperator = vm.PathDokterOperator,
                        PathPerawat = vm.PathPerawat,
                        Keterangan = vm.Keterangan,

                        IsDelete = false,
                        CreateBy = userActiveId,
                        CreateDateTime = now
                    };

                    _applicationDbContext.CatatanPemulihans.Add(header);

                    // 2) DETAILS (bisa lebih dari 1)
                    var detailEntities = vm.Details.Select(d => new CatatanPemulihanDetail
                    {
                        DetailCatPemulihanId = Guid.NewGuid(),

                        // ✅ override agar pasti nyambung ke header (abaikan input FE)
                        CatatanPemulihanId = catatanPemulihanId,

                        WaktuPengawasan = d.WaktuPengawasan,
                        PengawasanTDPostOP = d.PengawasanTDPostOP,
                        BilaSistole = d.BilaSistole,
                        PengawasanTerapi = d.PengawasanTerapi,
                        IntruksiKhusus = d.IntruksiKhusus,
                        IntruksiSedasi = d.IntruksiSedasi,
                        NilaiNumeric = d.NilaiNumeric,
                        NilaiKesadaran = d.NilaiKesadaran,
                        NilaiRespirasi = d.NilaiRespirasi,
                        NilaiSirkulasi = d.NilaiSirkulasi,
                        NilaiWarnaKulit = d.NilaiWarnaKulit,
                        JumlahScoreAldrete = d.JumlahScoreAldrete,
                        IsAldreteDewasa = d.IsAldreteDewasa,
                        BromageScore = d.BromageScore,
                        Keterangan = d.Keterangan,

                        IsDelete = false,
                        CreateBy = userActiveId,
                        CreateDateTime = now
                    }).ToList();

                    _applicationDbContext.CatatanPemulihanDetails.AddRange(detailEntities);

                    // 3) Save
                    int result = await _applicationDbContext.SaveChangesAsync();
                    if (result <= 0)
                    {
                        await trx.RollbackAsync();
                        return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                    }

                    await trx.CommitAsync();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        catatanPemulihanId,
                        totalDetails = detailEntities.Count
                    });
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CatatanPemulihanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Details wajib diisi minimal 1 item." });

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // 1) Ambil header lama
                    var header = await _applicationDbContext.CatatanPemulihans
                        .FirstOrDefaultAsync(x =>
                            x.CatatanPemulihanId == id &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (header == null)
                        return NotFound(new { message = "Catatan pemulihan tidak ditemukan." });

                    var now = DateTimeOffset.UtcNow;

                    // 2) Update header
                    header.KunjunganId = vm.KunjunganId;
                    header.PasienId = vm.PasienId;
                    header.DokterOperatorId = vm.DokterOperatorId;
                    header.PerawatId = vm.PerawatId;
                    header.WaktuMasuk = vm.WaktuMasuk;
                    header.InfusTransfusi = vm.InfusTransfusi;
                    header.JumlahUrine = vm.JumlahUrine;
                    header.Komplikasi = vm.Komplikasi;
                    header.Penatalaksanaan = vm.Penatalaksanaan;
                    header.InfusSedasi = vm.InfusSedasi;
                    header.Antibiotika = vm.Antibiotika;
                    header.Analgetik = vm.Analgetik;
                    header.AntiMuntah = vm.AntiMuntah;
                    header.Minum = vm.Minum;
                    header.PosisiPasien = vm.PosisiPasien;
                    header.Dipindahkan = vm.Dipindahkan;
                    header.WaktuKeluar = vm.WaktuKeluar;
                    header.PathDokterOperator = vm.PathDokterOperator;
                    header.PathPerawat = vm.PathPerawat;
                    header.Keterangan = vm.Keterangan;

                    header.UpdateBy = userActiveId;
                    header.UpdateDateTime = now;

                    // 3) Soft delete semua detail lama
                    var oldDetails = await _applicationDbContext.CatatanPemulihanDetails
                        .Where(d =>
                            d.CatatanPemulihanId == id &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .ToListAsync();

                    foreach (var d in oldDetails)
                    {
                        d.IsDelete = true;
                        d.UpdateBy = userActiveId;
                        d.UpdateDateTime = now;
                    }

                    // 4) Insert detail baru (replace all)
                    var newDetails = vm.Details.Select(d => new CatatanPemulihanDetail
                    {
                        DetailCatPemulihanId = Guid.NewGuid(),
                        CatatanPemulihanId = id, // ✅ link ke header

                        WaktuPengawasan = d.WaktuPengawasan,
                        PengawasanTDPostOP = d.PengawasanTDPostOP,
                        BilaSistole = d.BilaSistole,
                        PengawasanTerapi = d.PengawasanTerapi,
                        IntruksiKhusus = d.IntruksiKhusus,
                        IntruksiSedasi = d.IntruksiSedasi,
                        NilaiNumeric = d.NilaiNumeric,
                        NilaiKesadaran = d.NilaiKesadaran,
                        NilaiRespirasi = d.NilaiRespirasi,
                        NilaiSirkulasi = d.NilaiSirkulasi,
                        NilaiWarnaKulit = d.NilaiWarnaKulit,
                        JumlahScoreAldrete = d.JumlahScoreAldrete,
                        IsAldreteDewasa = d.IsAldreteDewasa,
                        BromageScore = d.BromageScore,
                        Keterangan = d.Keterangan,

                        IsDelete = false,
                        CreateBy = userActiveId,
                        CreateDateTime = now
                    }).ToList();

                    _applicationDbContext.CatatanPemulihanDetails.AddRange(newDetails);

                    // 5) Save
                    int result = await _applicationDbContext.SaveChangesAsync();
                    if (result <= 0)
                    {
                        await trx.RollbackAsync();
                        return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
                    }

                    await trx.CommitAsync();

                    return Ok(new
                    {
                        message = "Update Catatan Pemulihan berhasil (replace all details) || 200 OK",
                        id,
                        oldDetailsSoftDeleted = oldDetails.Count,
                        newDetailsInserted = newDetails.Count
                    });
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal update data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
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
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;
                var now = DateTimeOffset.UtcNow;

                await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // 1) Ambil header
                    var header = await _applicationDbContext.CatatanPemulihans
                        .FirstOrDefaultAsync(x =>
                            x.CatatanPemulihanId == id &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (header == null)
                        return NotFound(new { message = "Catatan pemulihan tidak ditemukan atau sudah dihapus." });

                    // 2) Ambil semua detail terkait header
                    var details = await _applicationDbContext.CatatanPemulihanDetails
                        .Where(d =>
                            d.CatatanPemulihanId == id &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .ToListAsync();

                    // 3) Soft delete header
                    header.IsDelete = true;
                    header.DeleteBy = userActiveId;
                    header.DeleteDateTime = now;

                    // 4) Soft delete semua details
                    foreach (var d in details)
                    {
                        d.IsDelete = true;
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = now;
                    }

                    int result = await _applicationDbContext.SaveChangesAsync();
                    if (result <= 0)
                    {
                        await trx.RollbackAsync();
                        return StatusCode(500, new { message = "Data tidak berhasil dihapus (soft delete)." });
                    }

                    await trx.CommitAsync();

                    return Ok(new
                    {
                        message = "Hapus Catatan Pemulihan (header + details) berhasil || 200 OK",
                        id,
                        totalDetailsDeleted = details.Count
                    });
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
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
            //string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100; // limit biar tidak berat

            // 1) Query HEADER (mirip style kamu)
            var query =
                from h in _applicationDbContext.CatatanPemulihans.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on h.CreateBy equals u.UserActiveId
                where h.IsDelete == false || h.IsDelete == null
                select new
                {
                    Header = h, // ✅ ambil entity header utuh (tanpa viewmodel)
                    CreateByName = u.FullName
                };

            // ✅ Filter KunjunganId
            if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
                query = query.Where(x => x.Header.KunjunganId == kunjunganId);

            // ✅ Filter PasienId
            if (pasienId.HasValue && pasienId.Value != Guid.Empty)
                query = query.Where(x => x.Header.PasienId == pasienId);

            // Search (ILIKE)
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%";
            //    query = query.Where(x =>
            //        EF.Functions.ILike(x.CreateByName ?? "", search) ||
            //        EF.Functions.ILike(x.Header.InfusTransfusi ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Komplikasi ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Penatalaksanaan ?? "", search) ||
            //        EF.Functions.ILike(x.Header.InfusSedasi ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Antibiotika ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Analgetik ?? "", search) ||
            //        EF.Functions.ILike(x.Header.AntiMuntah ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Minum ?? "", search) ||
            //        EF.Functions.ILike(x.Header.PosisiPasien ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Dipindahkan ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Keterangan ?? "", search)
            //    );
            //}

            // Filter tanggal (CreateDateTime)
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x => x.Header.CreateDateTime >= startUtc && x.Header.CreateDateTime <= endUtc);
            }

            // Filter periode (mirip contoh kamu)
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.Header.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.Header.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.Header.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.Header.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.Header.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.Header.CreateDateTime.Month == today.Month &&
                            x.Header.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(x =>
                            x.Header.CreateDateTime.Month == today.Month - 1 &&
                            x.Header.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.Header.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.Header.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.Header.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.Header.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.Header.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "WaktuMasuk" => query.OrderByDescending(x => x.Header.WaktuMasuk),
                    "WaktuKeluar" => query.OrderByDescending(x => x.Header.WaktuKeluar),
                    _ => query.OrderByDescending(x => x.Header.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.Header.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "WaktuMasuk" => query.OrderBy(x => x.Header.WaktuMasuk),
                    "WaktuKeluar" => query.OrderBy(x => x.Header.WaktuKeluar),
                    _ => query.OrderBy(x => x.Header.CreateDateTime)
                };

            // Pagination header
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var headerPage = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (headerPage.Count == 0 && page > totalPages)
                return NotFound(new { message = "Page not found." });

            // 2) Query DETAILS hanya untuk header yang tampil di page ini (1 query, bukan N+1)
            var headerIds = headerPage
                .Select(x => x.Header.CatatanPemulihanId)
                .Where(id => id != Guid.Empty)
                .ToList();

            var detailRows = await _applicationDbContext.CatatanPemulihanDetails
                .AsNoTracking()
                .Where(d => (d.IsDelete == false || d.IsDelete == null)
                            && headerIds.Contains((Guid)d.CatatanPemulihanId))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync(ct);

            var detailLookup = detailRows
                .GroupBy(d => d.CatatanPemulihanId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Gabungkan header + details
            var rows = headerPage.Select(x => new
            {
                x.Header,
                x.CreateByName,
                Details = detailLookup.TryGetValue(x.Header.CatatanPemulihanId, out var ds)
                    ? ds
                    : new List<CatatanPemulihanDetail>()
            }).ToList();

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
