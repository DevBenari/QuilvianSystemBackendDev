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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class HandoverPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<HandoverPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HandoverPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HandoverPasienController> logger,
            IWebHostEnvironment webHostEnvironment,
            ITTDService ttdService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =========================
            // 1) Header
            // =========================
            var handover = await _applicationDbContext.HandoverPasiens
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HandoverPasienId == id && (h.IsDelete == false || h.IsDelete == null));

            if (handover == null)
                return NotFound(new { message = "Data handover pasien tidak ditemukan." });

            // =========================
            // 2) Details + Checklist name
            // =========================
            var details = await _applicationDbContext.HandoverPasienDetails
                .AsNoTracking()
                .Where(d => d.HandoverPasienId == id && (d.IsDelete == false || d.IsDelete == null))
                .Join(
                    _applicationDbContext.ChecklistItems.AsNoTracking(),
                    d => d.ChecklistItemId,
                    c => c.ChecklistItemId,
                    (d, c) => new
                    {
                        d.DetailHandoverPasienId,
                        d.HandoverPasienId,
                        d.ChecklistItemId,
                        ChecklistItemName = c.NamaChecklistItem,
                        d.IsSudah,
                        d.Keterangan
                    }
                )
                .ToListAsync();

            // =========================
            // 3) Ambil semua userId yang dibutuhkan (1 query)
            // =========================
            var userIds = new HashSet<Guid>();

            if (handover.CreateBy != Guid.Empty) userIds.Add(handover.CreateBy);

            if (handover.CROId.HasValue && handover.CROId.Value != Guid.Empty)
                userIds.Add(handover.CROId.Value);

            if (handover.AdministrationId.HasValue && handover.AdministrationId.Value != Guid.Empty)
                userIds.Add(handover.AdministrationId.Value);

            if (handover.PerawatId.HasValue && handover.PerawatId.Value != Guid.Empty)
                userIds.Add(handover.PerawatId.Value);

            // kalau entity kamu ada UpdateBy/ModifiedBy dari UserActivity, bisa aktifkan:
            // if (handover.UpdateBy.HasValue && handover.UpdateBy.Value != Guid.Empty)
            //     userIds.Add(handover.UpdateBy.Value);

            var userMap = userIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.UserActiveId))
                    .Select(u => new { u.UserActiveId, u.FullName })
                    .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName);

            string? GetName(Guid? id2)
            {
                if (!id2.HasValue || id2.Value == Guid.Empty) return null;
                return userMap.TryGetValue(id2.Value, out var name) ? name : null;
            }

            string? GetNameNonNull(Guid id2)
            {
                if (id2 == Guid.Empty) return null;
                return userMap.TryGetValue(id2, out var name) ? name : null;
            }

            // =========================
            // 4) Response versi "enriched"
            // =========================
            var handoverEnriched = new
            {
                handover.HandoverPasienId,
                handover.KunjunganId,
                handover.PasienId,
                handover.TanggalSerahTerima,

                handover.AdministrationId,
                AdministrationName = GetName(handover.AdministrationId),
                handover.PathTTDAdministration,

                handover.CROId,
                CROName = GetName(handover.CROId),
                handover.PathTTDCRO,

                handover.PerawatId,
                PerawatName = GetName(handover.PerawatId),
                handover.PathTTDPerawat,

                handover.Keterangan,

                handover.CreateDateTime,
                handover.CreateBy,
                CreateByName = GetNameNonNull(handover.CreateBy),

                // kalau ada:
                // handover.UpdateDateTime,
                // handover.UpdateBy,
                // UpdateByName = GetName(handover.UpdateBy),
            };

            return Ok(new
            {
                message = "Berhasil",
                data = new
                {
                    handover = handoverEnriched,
                    details
                }
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HandoverPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            // Ambil user login (opsional, mengikuti pola kamu)
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            // transaksi biar parent+detail aman
            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                var handoverId = Guid.NewGuid();

                var ttdPerawat = await _ttdService.CheckTTDAsync((Guid)(vm.PerawatId));
                var ttdCro = await _ttdService.CheckTTDAsync((Guid)(vm.CROId));
                var ttdAdmin = await _ttdService.CheckTTDAsync((Guid)(vm.AdministrationId));


                // 1) Insert parent
                var handover = new HandoverPasien
                {
                    HandoverPasienId = handoverId, // sesuaikan kalau PK kamu beda
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TanggalSerahTerima = vm.TanggalSerahTerima,
                    AdministrationId = vm.AdministrationId,
                    PathTTDAdministration = ttdAdmin?.Path,
                    CROId = vm.CROId,
                    PathTTDCRO = ttdCro?.Path,
                    PerawatId = vm.PerawatId,
                    PathTTDPerawat = ttdPerawat?.Path,
                    Keterangan = vm.Keterangan,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = getUserActive.UserActiveId
                };

                _applicationDbContext.HandoverPasiens.Add(handover);

                // 2) Insert details (pakai HandoverPasienId dari parent)
                if (vm.Details != null && vm.Details.Count > 0)
                {
                    var detailEntities = vm.Details.Select(d => new HandoverPasienDetail
                    {
                        // kalau ada PK detail, isi (sesuaikan)
                        DetailHandoverPasienId = Guid.NewGuid(),
                        HandoverPasienId = handoverId,
                        ChecklistItemId = d.ChecklistItemId,
                        IsSudah = d.IsSudah,
                        Keterangan = d.Keterangan,

                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = getUserActive.UserActiveId
                    }).ToList();

                    _applicationDbContext.HandoverPasienDetails.AddRange(detailEntities);
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = handoverId }, new
                {
                    message = "Berhasil membuat handover pasien.",
                    id = handoverId
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = "Gagal membuat handover pasien.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HandoverPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // 1) Parent
                var handover = await _applicationDbContext.HandoverPasiens
                    .FirstOrDefaultAsync(h => h.HandoverPasienId == id);

                if (handover == null)
                    return NotFound(new { message = "Data handover pasien tidak ditemukan." });

                var ttdPerawat = await _ttdService.CheckTTDAsync((Guid)(vm.PerawatId));
                var ttdCro = await _ttdService.CheckTTDAsync((Guid)(vm.CROId));
                var ttdAdmin = await _ttdService.CheckTTDAsync((Guid)(vm.AdministrationId));

                // Update kolom parent
                handover.KunjunganId = vm.KunjunganId;
                handover.PasienId = vm.PasienId;
                handover.TanggalSerahTerima = vm.TanggalSerahTerima;
                handover.AdministrationId = vm.AdministrationId;
                handover.PathTTDAdministration = ttdAdmin?.Path;
                handover.CROId = vm.CROId;
                handover.PathTTDCRO = ttdCro?.Path;
                handover.PerawatId = vm.PerawatId;
                handover.PathTTDPerawat = ttdPerawat?.Path;
                handover.Keterangan = vm.Keterangan;

                handover.UpdateDateTime = DateTimeOffset.UtcNow;
                handover.UpdateBy = getUserActive.UserActiveId;

                // 2) Detail (TIDAK DIHAPUS, hanya UPSERT berdasarkan ChecklistItemId)
                if (vm.Details != null && vm.Details.Count > 0)
                {
                    var existingDetails = await _applicationDbContext.HandoverPasienDetails
                        .Where(d => d.HandoverPasienId == id)
                        .ToListAsync();

                    foreach (var d in vm.Details)
                    {
                        // cari detail lama berdasarkan ChecklistItemId (tanpa butuh DetailHandoverPasienId)
                        var existing = existingDetails
                            .FirstOrDefault(x => x.ChecklistItemId == d.ChecklistItemId);

                        if (existing != null)
                        {
                            existing.IsSudah = d.IsSudah;
                            existing.Keterangan = d.Keterangan;
                            existing.UpdateDateTime = DateTimeOffset.UtcNow;
                            existing.UpdateBy = getUserActive.UserActiveId;
                        }
                        else
                        {
                            // INSERT detail baru
                            var newDetail = new HandoverPasienDetail
                            {
                                DetailHandoverPasienId = Guid.NewGuid(),
                                HandoverPasienId = id,
                                ChecklistItemId = d.ChecklistItemId,
                                IsSudah = d.IsSudah,
                                Keterangan = d.Keterangan,
                                CreateDateTime = DateTimeOffset.UtcNow,
                                CreateBy = getUserActive.UserActiveId
                            };
                            _applicationDbContext.HandoverPasienDetails.Add(newDetail);
                        }
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new { message = "Berhasil update handover pasien (detail lama tidak dihapus).", id });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = "Gagal update handover pasien.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;

                // Jika kamu pakai Global Query Filter (mis. IsDeleted == false),
                // dan ingin tetap bisa delete data yang sudah terfilter, pakai IgnoreQueryFilters().
                var handover = await _applicationDbContext.HandoverPasiens
                    //.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(h => h.HandoverPasienId == id);

                if (handover == null)
                    return NotFound(new { message = "Data handover pasien tidak ditemukan." });

                // Jika sudah terhapus, boleh langsung return OK (idempotent)
                if (handover.IsDelete)
                {
                    return Ok(new
                    {
                        message = "Data sudah dalam kondisi terhapus (soft delete).",
                        id
                    });
                }

                // Soft delete parent
                handover.IsDelete = true;
                handover.DeleteDateTime = now;
                handover.DeleteBy = getUserActive.UserActiveId;

                // Soft delete details
                var details = await _applicationDbContext.HandoverPasienDetails
                    //.IgnoreQueryFilters()
                    .Where(d => d.HandoverPasienId == id && !d.IsDelete)
                    .ToListAsync();

                foreach (var d in details)
                {
                    d.IsDelete = true;
                    d.DeleteDateTime = now;
                    d.DeleteBy = getUserActive.UserActiveId;
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Berhasil soft delete handover pasien dan detailnya.",
                    id,
                    deletedDetailCount = details.Count
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = "Gagal soft delete data.", error = ex.Message });
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
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var baseQuery =
                from h in _applicationDbContext.HandoverPasiens.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on h.CreateBy equals u.UserActiveId
                where (h.IsDelete == false || h.IsDelete == null) // sesuaikan: IsDelete / IsDeleted
                select new
                {
                    h.HandoverPasienId,
                    h.KunjunganId,
                    h.PasienId,
                    h.TanggalSerahTerima,
                    h.AdministrationId,
                    h.PathTTDAdministration,
                    h.CROId,
                    h.PathTTDCRO,
                    h.PerawatId,
                    h.PathTTDPerawat,
                    h.Keterangan,
                    h.CreateDateTime,
                    h.CreateBy,
                    CreateByName = u.FullName
                };

            // FILTER kunjunganId & pasienId (baru)
            if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
                baseQuery = baseQuery.Where(x => x.KunjunganId == kunjunganId.Value);

            if (pasienId.HasValue && pasienId.Value != Guid.Empty)
                baseQuery = baseQuery.Where(x => x.PasienId == pasienId.Value);

            // SEARCH
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    var s = $"%{search.ToLower()}%";
            //    baseQuery = baseQuery.Where(x =>
            //        EF.Functions.ILike(x.CROId ?? "", s) ||
            //        EF.Functions.ILike(x.Keterangan ?? "", s) ||
            //        EF.Functions.ILike(x.CreateByName ?? "", s)
            //    );
            //}

            // FILTER tanggal custom
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                baseQuery = baseQuery.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
            }

            // FILTER periode
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                baseQuery = periode.Value switch
                {
                    PeriodeFilter.Today =>
                        baseQuery.Where(x => x.CreateDateTime.Date == today),

                    PeriodeFilter.ThisWeek =>
                        baseQuery.Where(x => x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek)
                                          && x.CreateDateTime.Date <= today),

                    PeriodeFilter.LastWeek =>
                        baseQuery.Where(x => x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek)
                                          && x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)),

                    PeriodeFilter.ThisMonth =>
                        baseQuery.Where(x => x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastMonth =>
                        baseQuery.Where(x => x.CreateDateTime >= new DateTime(today.Year, today.Month, 1).AddMonths(-1)
                                          && x.CreateDateTime < new DateTime(today.Year, today.Month, 1)),

                    PeriodeFilter.ThisYear =>
                        baseQuery.Where(x => x.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastYear =>
                        baseQuery.Where(x => x.CreateDateTime.Year == today.Year - 1),

                    PeriodeFilter.Last3Months =>
                        baseQuery.Where(x => x.CreateDateTime >= today.AddMonths(-3)),

                    PeriodeFilter.Last6Months =>
                        baseQuery.Where(x => x.CreateDateTime >= today.AddMonths(-6)),

                    _ => baseQuery
                };
            }

            // SORTING
            baseQuery = (sortDirection?.ToLower() == "desc")
                ? orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => baseQuery.OrderByDescending(x => x.CreateByName),
                    "TanggalSerahTerima" => baseQuery.OrderByDescending(x => x.TanggalSerahTerima),
                    "CROId" => baseQuery.OrderByDescending(x => x.CROId),
                    _ => baseQuery.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => baseQuery.OrderBy(x => x.CreateByName),
                    "TanggalSerahTerima" => baseQuery.OrderBy(x => x.TanggalSerahTerima),
                    "CROId" => baseQuery.OrderBy(x => x.CROId),
                    _ => baseQuery.OrderBy(x => x.CreateDateTime)
                };

            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
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

            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            // Ambil page parent
            var parents = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            var parentIds = parents.Select(x => x.HandoverPasienId).ToList();

            // ==============================
            // ✅ Ambil semua UserId unik di page
            // ==============================
            var userIds = new HashSet<Guid>();

            foreach (var p in parents)
            {
                if (p.CreateBy != Guid.Empty) userIds.Add(p.CreateBy);

                if (p.CROId.HasValue && p.CROId.Value != Guid.Empty)
                    userIds.Add(p.CROId.Value);

                if (p.AdministrationId.HasValue && p.AdministrationId.Value != Guid.Empty)
                    userIds.Add(p.AdministrationId.Value);

                if (p.PerawatId.HasValue && p.PerawatId.Value != Guid.Empty)
                    userIds.Add(p.PerawatId.Value);
            }

            // ==============================
            // ✅ 1 query ambil semua user name
            // ==============================
            var userMap = await _applicationDbContext.UserActives
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserActiveId))
                .Select(u => new { u.UserActiveId, u.FullName })
                .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName);

            // helper local untuk ambil nama dari map
            string? GetUserName(Guid? id)
            {
                if (!id.HasValue || id.Value == Guid.Empty) return null;
                return userMap.TryGetValue(id.Value, out var name) ? name : null;
            }

            string? GetUserNameNonNull(Guid id)
            {
                if (id == Guid.Empty) return null;
                return userMap.TryGetValue(id, out var name) ? name : null;
            }

            // ==============================
            // Ambil detail untuk semua parent di page (1 query)
            // ==============================
            var details = await (from d in _applicationDbContext.HandoverPasienDetails.AsNoTracking()
                                 join c in _applicationDbContext.ChecklistItems.AsNoTracking()
                                    on d.ChecklistItemId equals c.ChecklistItemId
                                 where parentIds.Contains((Guid)d.HandoverPasienId)
                                       && (d.IsDelete == false || d.IsDelete == null)
                                 select new
                                 {
                                     d.DetailHandoverPasienId,
                                     d.HandoverPasienId,
                                     d.ChecklistItemId,
                                     ChecklistItemName = c.NamaChecklistItem,
                                     d.IsSudah,
                                     d.Keterangan,
                                     d.CreateDateTime,
                                     d.CreateBy
                                 }).ToListAsync();

            var detailMap = details
                .GroupBy(d => d.HandoverPasienId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ==============================
            // ✅ Build rows + tambahkan nama
            // ==============================
            var rows = parents.Select(p => new
            {
                p.HandoverPasienId,
                p.KunjunganId,
                p.PasienId,
                p.TanggalSerahTerima,

                p.AdministrationId,
                AdministrationName = GetUserName(p.AdministrationId),

                p.PathTTDAdministration,

                p.CROId,
                CROName = GetUserName(p.CROId),

                p.PathTTDCRO,

                p.PerawatId,
                PerawatName = GetUserName(p.PerawatId),

                p.PathTTDPerawat,

                p.Keterangan,
                p.CreateDateTime,

                p.CreateBy,
                CreateByName = GetUserNameNonNull(p.CreateBy),

                Details = detailMap.TryGetValue(p.HandoverPasienId, out var det)
                    ? det.Select(x => (object)x).ToList()
                    : new List<object>()
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
