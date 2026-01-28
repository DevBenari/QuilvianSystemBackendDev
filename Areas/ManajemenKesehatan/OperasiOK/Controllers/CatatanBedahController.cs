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
    public class CatatanBedahController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<CatatanBedahController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatatanBedahController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CatatanBedahController> logger,
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
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 1) Ambil HEADER (AsNoTracking biar ringan)
                var header = await _applicationDbContext.CatatanBedahs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.CatBedahId == id &&
                        (x.IsDelete == false || x.IsDelete == null), ct);

                if (header == null)
                    return NotFound(new { message = "Catatan bedah tidak ditemukan." });

                // 2) Ambil DETAILS (bisa lebih dari 1)
                var details = await _applicationDbContext.CatatanBedahLokals
                    .AsNoTracking()
                    .Where(d =>
                        d.CatBedahId == id &&
                        (d.IsDelete == false || d.IsDelete == null))
                    .OrderBy(d => d.CreateDateTime)
                    .ToListAsync(ct);

                // 3) Return tanpa VM (header + details)
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    header,
                    details,
                    totalDetails = details.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CatatanBedahViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

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
                    var now = DateTime.UtcNow;
                    var catBedahId = Guid.NewGuid();

                    // 1) HEADER
                    var header = new CatatanBedah
                    {
                        CatBedahId = catBedahId,
                        KunjunganId = vm.KunjunganId,
                        PasienId = vm.PasienId,
                        DokterOperatorId = vm.DokterOperatorId,
                        AsistenDokterId = vm.AsistenDokterId,
                        DokterAnestesiId = vm.DokterAnestesiId,
                        AsistenAnestesiId = vm.AsistenAnestesiId,
                        PerawatId = vm.PerawatId,
                        TindakanId = vm.TindakanId,
                        IcdPraOperasiId = vm.IcdPraOperasiId,
                        DiagnosaPraOperasi = vm.DiagnosaPraOperasi,
                        IcdPostOperasiId = vm.IcdPostOperasiId,
                        DiagnosaPostOperasi = vm.DiagnosaPostOperasi,
                        JenisOperasi = vm.JenisOperasi,
                        UrgensiOperasi = vm.UrgensiOperasi,
                        MacamOperasi = vm.MacamOperasi,
                        TanggalOperasi = vm.TanggalOperasi,
                        Jumlah = vm.Jumlah,
                        WaktuMulaiOperasi = vm.WaktuMulaiOperasi,
                        WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi,
                        WaktuTambahan = vm.WaktuTambahan,
                        LamaOperasi = vm.LamaOperasi,
                        JumlahPendarahan = vm.JumlahPendarahan,
                        IsJaringan = vm.IsJaringan,
                        JenisJaringan = vm.JenisJaringan,
                        IsPA = vm.IsPA,
                        Komplikasi = vm.Komplikasi,
                        CatatanSaatOperasi = vm.CatatanSaatOperasi,
                        PathTTDDokterOperator = vm.PathTTDDokterOperator,

                        CreateBy = userActiveId,
                        CreateDateTime = now
                    };

                    _applicationDbContext.CatatanBedahs.Add(header);

                    // 2) DETAILS (OPTIONAL)
                    // Kalau FE mengirim list kosong/null -> tidak insert detail.
                    // Opsional: buang item detail yang benar-benar kosong agar tidak nyimpan baris sampah.
                    var detailVms = vm.Details?
                        .Where(d =>
                            d != null &&
                            (d.KomplikasiAkut != null ||
                             d.TemuanSaatOperasi != null ||
                             d.Pengawasan != null ||
                             d.Kontrol != null ||
                             d.Terapi != null ||
                             d.Keterangan != null))
                        .ToList();

                    var detailEntities = new List<CatatanBedahLokal>();

                    if (detailVms != null && detailVms.Any())
                    {
                        detailEntities = detailVms.Select(d => new CatatanBedahLokal
                        {
                            CatBedahLokalId = Guid.NewGuid(),

                            // ✅ override FK agar pasti nyambung ke header baru
                            CatBedahId = catBedahId,

                            KomplikasiAkut = d.KomplikasiAkut,
                            TemuanSaatOperasi = d.TemuanSaatOperasi,
                            Pengawasan = d.Pengawasan,
                            Kontrol = d.Kontrol,
                            Terapi = d.Terapi,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = now
                        }).ToList();

                        _applicationDbContext.CatatanBedahLokals.AddRange(detailEntities);
                    }

                    // 3) Save sekali
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
                        catBedahId,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] CatatanBedahViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

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
                    // 1) Ambil header lama
                    var header = await _applicationDbContext.CatatanBedahs
                        .FirstOrDefaultAsync(x =>
                            x.CatBedahId == id &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (header == null)
                        return NotFound(new { message = "Catatan bedah tidak ditemukan." });

                    // 2) Update header
                    header.KunjunganId = vm.KunjunganId;
                    header.PasienId = vm.PasienId;
                    header.DokterOperatorId = vm.DokterOperatorId;
                    header.AsistenDokterId = vm.AsistenDokterId;
                    header.DokterAnestesiId = vm.DokterAnestesiId;
                    header.AsistenAnestesiId = vm.AsistenAnestesiId;
                    header.PerawatId = vm.PerawatId;
                    header.TindakanId = vm.TindakanId;
                    header.IcdPraOperasiId = vm.IcdPraOperasiId;
                    header.DiagnosaPraOperasi = vm.DiagnosaPraOperasi;
                    header.IcdPostOperasiId = vm.IcdPostOperasiId;
                    header.DiagnosaPostOperasi = vm.DiagnosaPostOperasi;
                    header.JenisOperasi = vm.JenisOperasi;
                    header.UrgensiOperasi = vm.UrgensiOperasi;
                    header.MacamOperasi = vm.MacamOperasi;
                    header.TanggalOperasi = vm.TanggalOperasi;
                    header.Jumlah = vm.Jumlah;
                    header.WaktuMulaiOperasi = vm.WaktuMulaiOperasi;
                    header.WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi;
                    header.WaktuTambahan = vm.WaktuTambahan;
                    header.LamaOperasi = vm.LamaOperasi;
                    header.JumlahPendarahan = vm.JumlahPendarahan;
                    header.IsJaringan = vm.IsJaringan;
                    header.JenisJaringan = vm.JenisJaringan;
                    header.IsPA = vm.IsPA;
                    header.Komplikasi = vm.Komplikasi;
                    header.CatatanSaatOperasi = vm.CatatanSaatOperasi;
                    header.PathTTDDokterOperator = vm.PathTTDDokterOperator;

                    header.UpdateBy = userActiveId;
                    header.UpdateDateTime = now;

                    // 3) DETAILS OPTIONAL
                    // - Jika Details null atau kosong: tidak melakukan apa-apa pada detail.
                    // - Jika Details ada isinya: replace all detail.
                    var detailVms = vm.Details?
                        .Where(d =>
                            d != null &&
                            (d.KomplikasiAkut != null ||
                             d.TemuanSaatOperasi != null ||
                             d.Pengawasan != null ||
                             d.Kontrol != null ||
                             d.Terapi != null ||
                             d.Keterangan != null))
                        .ToList();

                    int oldDetailsSoftDeleted = 0;
                    int newDetailsInserted = 0;

                    if (detailVms != null && detailVms.Any())
                    {
                        // 3a) soft delete detail lama
                        var oldDetails = await _applicationDbContext.CatatanBedahLokals
                            .Where(d =>
                                d.CatBedahId == id &&
                                (d.IsDelete == false || d.IsDelete == null))
                            .ToListAsync();

                        foreach (var d in oldDetails)
                        {
                            d.IsDelete = true;
                            d.DeleteBy = userActiveId;
                            d.DeleteDateTime = now;
                        }

                        oldDetailsSoftDeleted = oldDetails.Count;

                        // 3b) insert detail baru (replace all)
                        var newDetails = detailVms.Select(d => new CatatanBedahLokal
                        {
                            CatBedahLokalId = Guid.NewGuid(),
                            CatBedahId = id, // ✅ override FK

                            KomplikasiAkut = d.KomplikasiAkut,
                            TemuanSaatOperasi = d.TemuanSaatOperasi,
                            Pengawasan = d.Pengawasan,
                            Kontrol = d.Kontrol,
                            Terapi = d.Terapi,
                            Keterangan = d.Keterangan,

                            IsDelete = false,
                            CreateBy = userActiveId,
                            CreateDateTime = now
                        }).ToList();

                        _applicationDbContext.CatatanBedahLokals.AddRange(newDetails);
                        newDetailsInserted = newDetails.Count;
                    }

                    // 4) Save sekali
                    int result = await _applicationDbContext.SaveChangesAsync();
                    if (result <= 0)
                    {
                        await trx.RollbackAsync();
                        return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
                    }

                    await trx.CommitAsync();

                    return Ok(new
                    {
                        message = "Update berhasil || 200 OK",
                        id,
                        detailsUpdated = (detailVms != null && detailVms.Any()),
                        oldDetailsSoftDeleted,
                        newDetailsInserted
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
                    // 1) Header
                    var header = await _applicationDbContext.CatatanBedahs
                        .FirstOrDefaultAsync(x =>
                            x.CatBedahId == id &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (header == null)
                        return NotFound(new { message = "Catatan bedah tidak ditemukan atau sudah dihapus." });

                    // 2) Details
                    var details = await _applicationDbContext.CatatanBedahLokals
                        .Where(d =>
                            d.CatBedahId == id &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .ToListAsync();

                    // 3) Soft delete header
                    header.IsDelete = true;
                    header.DeleteBy = userActiveId;
                    header.DeleteDateTime = now;

                    // 4) Soft delete details
                    foreach (var d in details)
                    {
                        d.IsDelete = true;
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = now;
                    }

                    var result = await _applicationDbContext.SaveChangesAsync();
                    if (result <= 0)
                    {
                        await trx.RollbackAsync();
                        return StatusCode(500, new { message = "Data tidak berhasil dihapus (soft delete)." });
                    }

                    await trx.CommitAsync();

                    return Ok(new
                    {
                        message = "Hapus Catatan Bedah (header + details) berhasil || 200 OK",
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
            string? search = null,
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

            // 1) Query HEADER (mirip gaya kamu)
            var query =
                from h in _applicationDbContext.CatatanBedahs.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on h.CreateBy equals u.UserActiveId
                where h.IsDelete == false || h.IsDelete == null
                select new
                {
                    Header = h, // entity header full
                    CreateByName = u.FullName
                };

            // Filter KunjunganId / PasienId
            if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
                query = query.Where(x => x.Header.KunjunganId == kunjunganId);

            if (pasienId.HasValue && pasienId.Value != Guid.Empty)
                query = query.Where(x => x.Header.PasienId == pasienId);

            // Search (ILIKE)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.CreateByName ?? "", search) ||
                    EF.Functions.ILike(x.Header.DiagnosaPraOperasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.DiagnosaPostOperasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.JenisOperasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.UrgensiOperasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.MacamOperasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.Komplikasi ?? "", search) ||
                    EF.Functions.ILike(x.Header.CatatanSaatOperasi ?? "", search)
                );
            }

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
                    "TanggalOperasi" => query.OrderByDescending(x => x.Header.TanggalOperasi),
                    _ => query.OrderByDescending(x => x.Header.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.Header.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "TanggalOperasi" => query.OrderBy(x => x.Header.TanggalOperasi),
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

            // 2) Query DETAILS hanya untuk header yang tampil di page ini
            var headerIds = headerPage.Select(x => x.Header.CatBedahId).ToList();

            var detailRows = await _applicationDbContext.CatatanBedahLokals
                .AsNoTracking()
                .Where(d => (d.IsDelete == false || d.IsDelete == null) && headerIds.Contains((Guid)d.CatBedahId))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync(ct);

            var detailLookup = detailRows
                .GroupBy(d => d.CatBedahId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rows = headerPage.Select(x => new
            {
                x.Header,
                x.CreateByName,
                Details = detailLookup.TryGetValue(x.Header.CatBedahId, out var ds)
                    ? ds
                    : new List<CatatanBedahLokal>()
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
