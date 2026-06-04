using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
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
    [EnableCors("FrontendCorsPolicy")]
    public class LaporanAnestesiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LaporanAnestesiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LaporanAnestesiController(
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<LaporanAnestesiController> logger,
        IWebHostEnvironment webHostEnvironment)
        {
                _applicationDbContext = applicationDbContext;
                _userManager = userManager;
                _signInManager = signInManager;
                _logger = logger;
                _webHostEnvironment = webHostEnvironment;
        }

        private static TimeSpan? HitungDurasi(DateTime? mulai, DateTime? selesai)
        {
            if (!mulai.HasValue || !selesai.HasValue)
                return null;

            var start = mulai.Value;
            var end = selesai.Value;

            // kalau end lebih kecil (lewat tengah malam), tambahkan 1 hari
            if (end < start)
                end = end.AddDays(1);

            return end - start;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var header = await _applicationDbContext.LaporanAnestesis
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LaporanAnestesiId == id && !x.IsDelete, ct);

            if (header == null)
                return NotFound(new { message = "Laporan anestesi tidak ditemukan." });

            var details = await _applicationDbContext.LaporanAnestesiDetails
                .AsNoTracking()
                .Where(d => d.LaporanAnestesiId == id && !d.IsDelete)
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync(ct);

            return Ok(new
            {
                message = "OK",
                data = header,        // semua kolom header
                details = details,    // semua kolom detail
                totalDetails = details.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LaporanAnestesiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.KunjunganId.HasValue || vm.KunjunganId.Value == Guid.Empty)
                return BadRequest(new { message = "KunjunganId wajib diisi." });

            if (!vm.PasienId.HasValue || vm.PasienId.Value == Guid.Empty)
                return BadRequest(new { message = "PasienId wajib diisi." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Details wajib diisi minimal 1 item." });

            if (!await _applicationDbContext.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            // Ambil User ID dari JWT Claims
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(u => u.Email == emailLogin)
                .Select(u => (Guid?)u.UserActiveId)
                .FirstOrDefaultAsync();

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // (Opsional) Cegah duplikasi per kunjungan
                // kalau kamu ingin boleh lebih dari 1 laporan per kunjungan, hapus blok ini.
                //var exists = await _applicationDbContext.LaporanAnestesis
                //    .AsNoTracking()
                //    .AnyAsync(x => x.KunjunganId == vm.KunjunganId && !x.IsDelete);

                //if (exists)
                //    return Conflict(new { message = "Laporan anestesi untuk kunjungan ini sudah pernah dibuat." });

                var now = DateTimeOffset.UtcNow;

                // Generate ID header
                var laporanAnestesiId = Guid.NewGuid();

                // Hitung durasi otomatis kalau null
                var durasiOperasi = vm.DurasiOperasi ?? HitungDurasi(vm.WaktuMulaiOperasi, vm.WaktuSelesaiOperasi);
                var durasiAnestesi = vm.DurasiAnestesi ?? HitungDurasi(vm.WaktuMulaiAnestesi, vm.WaktuSelesaiAnestesi);

                // 1) HEADER
                var header = new LaporanAnestesi
                {
                    LaporanAnestesiId = laporanAnestesiId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TindakanId = vm.TindakanId,
                    DetailTindakan = vm.DetailTindakan,

                    DokterOperatorId = vm.DokterOperatorId,
                    DokterAnestesiId = vm.DokterAnestesiId,
                    DokterAsistenId = vm.DokterAsistenId,
                    AsistenAnestesiId = vm.AsistenAnestesiId,
                    PerawatId = vm.PerawatId,

                    Premidikasi = vm.Premidikasi,
                    TanggalOperasi = vm.TanggalOperasi,
                    WaktuMulaiOperasi = vm.WaktuMulaiOperasi,
                    WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi,
                    DurasiOperasi = durasiOperasi,

                    WaktuMulaiAnestesi = vm.WaktuMulaiAnestesi,
                    WaktuSelesaiAnestesi = vm.WaktuSelesaiAnestesi,
                    DurasiAnestesi = durasiAnestesi,

                    PosisiOperasi = vm.PosisiOperasi,
                    Oksigenasi = vm.Oksigenasi,

                    Induksi = vm.Induksi,
                    Intubasi = vm.Intubasi,
                    NoIntubasi = vm.NoIntubasi,
                    ProsesIntubasi = vm.ProsesIntubasi,
                    AlasanProsesIntubasi = vm.AlasanProsesIntubasi,

                    GenderBayiLahir = vm.GenderBayiLahir,
                    WaktuCesar = vm.WaktuCesar,
                    APGARScore = vm.APGARScore,

                    PathTTDDokterAnestesi = vm.PathTTDDokterAnestesi,
                    Keterangan = vm.Keterangan,

                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = now
                };

                _applicationDbContext.LaporanAnestesis.Add(header);

                // 2) DETAILS (bisa lebih dari 1)
                var detailEntities = vm.Details.Select(d => new LaporanAnestesiDetail
                {
                    DetailLaporanAnestesiId = Guid.NewGuid(),
                    LaporanAnestesiId = laporanAnestesiId, // ✅ link ke header

                    VMSevoflurane = d.VMSevoflurane,
                    TotalSevoflurane = d.TotalSevoflurane,
                    VMIsoflurane = d.VMIsoflurane,
                    TotalIsoflurane = d.TotalIsoflurane,
                    VMEnflurane = d.VMEnflurane,
                    TotalEnflurane = d.TotalEnflurane,

                    FlowO2 = d.FlowO2,
                    FlowN2O = d.FlowN2O,
                    GolonganDarah = d.GolonganDarah,
                    TransfusiSebelumnya = d.TransfusiSebelumnya,

                    Cairan = d.Cairan,
                    Kristaloid = d.Kristaloid,
                    Koloid = d.Koloid,

                    KeadaanPernapasan = d.KeadaanPernapasan,
                    StatusGizi = d.StatusGizi,
                    ASA = d.ASA,
                    Pendarahan = d.Pendarahan,

                    Keterangan = d.Keterangan,

                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = now
                }).ToList();

                _applicationDbContext.LaporanAnestesiDetails.AddRange(detailEntities);

                // Save sekali
                var saved = await _applicationDbContext.SaveChangesAsync();
                if (saved <= 0)
                {
                    await trx.RollbackAsync();
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                await trx.CommitAsync();

                return Created("", new
                {
                    message = "Tambah Laporan Anestesi (Header + Details) berhasil || 201 Created",
                    laporanAnestesiId,
                    totalDetails = detailEntities.Count
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReplaceAllDetails(
        Guid id,
        [FromBody] LaporanAnestesiViewModel vm,
        CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Details wajib diisi minimal 1 item." });

            // Auth user
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(u => u.Email == emailLogin)
                .Select(u => (Guid?)u.UserActiveId)
                .FirstOrDefaultAsync(ct);

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync(ct);
            try
            {
                // 1) Ambil header (tracking)
                var header = await _applicationDbContext.LaporanAnestesis
                    .FirstOrDefaultAsync(x => x.LaporanAnestesiId == id && !x.IsDelete, ct);

                if (header == null)
                    return NotFound(new { message = "Laporan anestesi tidak ditemukan." });

                // (Opsional) Validasi kalau client mengirim LaporanAnestesiId di setiap detail harus sama
                // (VM detail kamu punya LaporanAnestesiId)
                var anyMismatch = vm.Details.Any(d => d.LaporanAnestesiId.HasValue && d.LaporanAnestesiId.Value != id);
                if (anyMismatch)
                    return BadRequest(new { message = "Ada detail dengan LaporanAnestesiId yang tidak sesuai dengan id endpoint." });

                // 2) Update header (hanya overwrite kalau value dikirim)
                if (vm.KunjunganId.HasValue) header.KunjunganId = vm.KunjunganId;
                if (vm.PasienId.HasValue) header.PasienId = vm.PasienId;
                if (vm.TindakanId.HasValue) header.TindakanId = vm.TindakanId;

                if (vm.DetailTindakan != null) header.DetailTindakan = vm.DetailTindakan;

                if (vm.DokterOperatorId.HasValue) header.DokterOperatorId = vm.DokterOperatorId;
                if (vm.DokterAnestesiId.HasValue) header.DokterAnestesiId = vm.DokterAnestesiId;
                if (vm.DokterAsistenId.HasValue) header.DokterAsistenId = vm.DokterAsistenId;
                if (vm.AsistenAnestesiId.HasValue) header.AsistenAnestesiId = vm.AsistenAnestesiId;
                if (vm.PerawatId.HasValue) header.PerawatId = vm.PerawatId;

                if (vm.Premidikasi != null) header.Premidikasi = vm.Premidikasi;

                if (vm.TanggalOperasi.HasValue) header.TanggalOperasi = vm.TanggalOperasi;
                if (vm.WaktuMulaiOperasi.HasValue) header.WaktuMulaiOperasi = vm.WaktuMulaiOperasi;
                if (vm.WaktuSelesaiOperasi.HasValue) header.WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi;

                // Durasi operasi: jika tidak dikirim tapi mulai & selesai ada, hitung otomatis
                if (vm.DurasiOperasi.HasValue)
                    header.DurasiOperasi = vm.DurasiOperasi;
                else if (vm.WaktuMulaiOperasi.HasValue && vm.WaktuSelesaiOperasi.HasValue)
                    header.DurasiOperasi = HitungDurasi(vm.WaktuMulaiOperasi, vm.WaktuSelesaiOperasi);

                if (vm.WaktuMulaiAnestesi.HasValue) header.WaktuMulaiAnestesi = vm.WaktuMulaiAnestesi;
                if (vm.WaktuSelesaiAnestesi.HasValue) header.WaktuSelesaiAnestesi = vm.WaktuSelesaiAnestesi;

                // Durasi anestesi: auto hitung jika perlu
                if (vm.DurasiAnestesi.HasValue)
                    header.DurasiAnestesi = vm.DurasiAnestesi;
                else if (vm.WaktuMulaiAnestesi.HasValue && vm.WaktuSelesaiAnestesi.HasValue)
                    header.DurasiAnestesi = HitungDurasi(vm.WaktuMulaiAnestesi, vm.WaktuSelesaiAnestesi);

                if (vm.PosisiOperasi != null) header.PosisiOperasi = vm.PosisiOperasi;
                if (vm.Oksigenasi != null) header.Oksigenasi = vm.Oksigenasi;

                if (vm.Induksi != null) header.Induksi = vm.Induksi;
                if (vm.Intubasi != null) header.Intubasi = vm.Intubasi;

                // Karena di VM non-nullable, ini akan selalu overwrite.
                // (Kalau kamu ingin bisa "tidak diubah", ubah tipe jadi decimal? di VM.)
                header.NoIntubasi = vm.NoIntubasi;
                header.APGARScore = vm.APGARScore;

                if (vm.ProsesIntubasi != null) header.ProsesIntubasi = vm.ProsesIntubasi;
                if (vm.AlasanProsesIntubasi != null) header.AlasanProsesIntubasi = vm.AlasanProsesIntubasi;

                if (vm.GenderBayiLahir != null) header.GenderBayiLahir = vm.GenderBayiLahir;
                if (vm.WaktuCesar.HasValue) header.WaktuCesar = vm.WaktuCesar;

                if (vm.PathTTDDokterAnestesi != null) header.PathTTDDokterAnestesi = vm.PathTTDDokterAnestesi;
                if (vm.Keterangan != null) header.Keterangan = vm.Keterangan;

                header.UpdateBy = userActiveId.Value;
                header.UpdateDateTime = DateTimeOffset.UtcNow;

                // 3) Soft delete ALL detail lama
                var oldDetails = await _applicationDbContext.LaporanAnestesiDetails
                    .Where(d => d.LaporanAnestesiId == id && !d.IsDelete)
                    .ToListAsync(ct);

                var now = DateTimeOffset.UtcNow;
                foreach (var od in oldDetails)
                {
                    od.IsDelete = true;
                    od.UpdateBy = userActiveId.Value;
                    od.UpdateDateTime = now;
                }

                // 4) Insert detail baru (REPLACE ALL)
                var newDetails = vm.Details.Select(d => new LaporanAnestesiDetail
                {
                    DetailLaporanAnestesiId = Guid.NewGuid(),
                    LaporanAnestesiId = id,

                    VMSevoflurane = d.VMSevoflurane,
                    TotalSevoflurane = d.TotalSevoflurane,
                    VMIsoflurane = d.VMIsoflurane,
                    TotalIsoflurane = d.TotalIsoflurane,
                    VMEnflurane = d.VMEnflurane,
                    TotalEnflurane = d.TotalEnflurane,

                    FlowO2 = d.FlowO2,
                    FlowN2O = d.FlowN2O,
                    GolonganDarah = d.GolonganDarah,
                    TransfusiSebelumnya = d.TransfusiSebelumnya,

                    Cairan = d.Cairan,
                    Kristaloid = d.Kristaloid,
                    Koloid = d.Koloid,

                    KeadaanPernapasan = d.KeadaanPernapasan,
                    StatusGizi = d.StatusGizi,
                    ASA = d.ASA,
                    Pendarahan = d.Pendarahan,

                    Keterangan = d.Keterangan,

                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = now
                }).ToList();

                _applicationDbContext.LaporanAnestesiDetails.AddRange(newDetails);

                // 5) Save sekali
                var saved = await _applicationDbContext.SaveChangesAsync(ct);
                if (saved <= 0)
                {
                    await trx.RollbackAsync(ct);
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                await trx.CommitAsync(ct);

                return Ok(new
                {
                    message = "Update berhasil (replace all details) || 200 OK",
                    id,
                    oldDetailsSoftDeleted = oldDetails.Count,
                    newDetailsInserted = newDetails.Count
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Auth user
        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(emailLogin))
            return Unauthorized(new { message = "User tidak terautentikasi!" });

        var userActiveId = await _applicationDbContext.UserActives
            .Where(u => u.Email == emailLogin)
            .Select(u => (Guid?)u.UserActiveId)
            .FirstOrDefaultAsync(ct);

        if (!userActiveId.HasValue)
            return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        await using var trx = await _applicationDbContext.Database.BeginTransactionAsync(ct);
        try
        {
            // 1) Ambil header (tracking)
            var header = await _applicationDbContext.LaporanAnestesis
                .FirstOrDefaultAsync(x => x.LaporanAnestesiId == id && !x.IsDelete, ct);

            if (header == null)
                return NotFound(new { message = "Laporan anestesi tidak ditemukan atau sudah dihapus." });

            // 2) Ambil semua detail aktif
            var details = await _applicationDbContext.LaporanAnestesiDetails
                .Where(d => d.LaporanAnestesiId == id && !d.IsDelete)
                .ToListAsync(ct);

            var now = DateTimeOffset.UtcNow;

            // 3) Soft delete header
            header.IsDelete = true;
            header.UpdateBy = userActiveId.Value;
            header.UpdateDateTime = now;

            // 4) Soft delete semua detail
            foreach (var d in details)
            {
                d.IsDelete = true;
                d.UpdateBy = userActiveId.Value;
                d.UpdateDateTime = now;
            }

            var saved = await _applicationDbContext.SaveChangesAsync(ct);
            if (saved <= 0)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = "Data tidak berhasil dihapus (soft delete)." });
            }

            await trx.CommitAsync(ct);

            return Ok(new
            {
                message = "Hapus laporan anestesi (header + details) berhasil || 200 OK",
                id,
                totalDetailsDeleted = details.Count
            });
        }
        catch (DbUpdateException dbEx)
        {
            await trx.RollbackAsync(ct);
            return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
        }
        catch (Exception ex)
        {
            await trx.RollbackAsync(ct);
            return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        }
    }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
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
            if (perPage > 100) perPage = 100;

            var query =
                from a in _applicationDbContext.LaporanAnestesis.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId
                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    Header = a,
                    CreateByName = u.FullName,
                };

            // ✅ Filter berdasarkan KunjunganId
            if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
            {
                query = query.Where(x => x.Header.KunjunganId == kunjunganId);
            }

            // ✅ Filter berdasarkan PasienId
            if (pasienId.HasValue && pasienId.Value != Guid.Empty)
            {
                query = query.Where(x => x.Header.PasienId == pasienId);
            }

            // Search
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%";
            //    query = query.Where(x =>
            //        EF.Functions.ILike(x.CreateByName, search) ||
            //        EF.Functions.ILike(x.Header.DetailTindakan ?? "", search) ||
            //        EF.Functions.ILike(x.Header.Keterangan ?? "", search)
            //    );
            //}

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x => x.Header.CreateDateTime >= startUtc && x.Header.CreateDateTime <= endUtc);
            }

            // Filter periode (tetap seperti punyamu)
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

            // Sorting
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

            // Pagination
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new { Rows = Array.Empty<object>(), TotalRows = 0, CurrentPage = page, PerPage = perPage, TotalPages = 0 }
                });
            }

            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            var headerRows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync(ct);

            var headerIds = headerRows
                .Select(x => x.Header.LaporanAnestesiId)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id!.Value)
                .ToList();

            var detailRows = await _applicationDbContext.LaporanAnestesiDetails
                .AsNoTracking()
                .Where(d => (d.IsDelete == false || d.IsDelete == null)
                            && d.LaporanAnestesiId.HasValue
                            && headerIds.Contains(d.LaporanAnestesiId.Value))
                .OrderBy(d => d.CreateDateTime)
                .ToListAsync(ct);

            var detailLookup = detailRows
                .GroupBy(d => d.LaporanAnestesiId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rows = headerRows.Select(x => new
            {
                x.Header,
                x.CreateByName,
                Details = detailLookup.TryGetValue(x.Header.LaporanAnestesiId!.Value, out var ds)
                    ? ds
                    : new List<LaporanAnestesiDetail>()
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
