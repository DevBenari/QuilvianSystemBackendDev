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
    public class IGDObservasiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<IGDObservasiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IGDObservasiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDObservasiController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                // ------------------------------------------------------------
                // 1. Ambil header
                // ------------------------------------------------------------
                var header = await (
                    from h in _applicationDbContext.IGDObservasis
                    join u in _applicationDbContext.UserActives
                        on h.CreateBy equals u.UserActiveId
                    where (h.IsDelete == false || h.IsDelete == null)
                          && h.ObservasiIgdId == id
                    select new
                    {
                        h.ObservasiIgdId,
                        h.KunjunganId,
                        h.PasienId,
                        h.Airway,
                        h.Breathing,
                        h.Circulation,
                        h.Disability,
                        h.Eye,
                        h.Motor,
                        h.Verbal,
                        h.AlatBantuNapas,
                        h.AlatBantuOksigenasi,
                        h.DokterId,
                        h.PerawatId,
                        h.TglObservasi,
                        h.ATS,
                        h.Keterangan,
                        h.CreateDateTime,
                        CreateByName = u.FullName
                    }
                ).FirstOrDefaultAsync();

                if (header == null)
                {
                    return NotFound(new { message = $"Observasi IGD dengan ID {id} tidak ditemukan." });
                }

                // ------------------------------------------------------------
                // 2. Ambil detail + join obat (tanpa N+1)
                // ------------------------------------------------------------
                var details = await (
                    from d in _applicationDbContext.IGDObservasiDetails
                    join o in _applicationDbContext.Obats
                        on d.ObatId equals o.ObatId into obatGroup
                    from o in obatGroup.DefaultIfEmpty()
                    where (d.IsDelete == false || d.IsDelete == null)
                          && d.ObservasiIgdId == id
                    select new
                    {
                        d.ObservasiIgdId,
                        d.TglObservasi,
                        d.ObatId,
                        NamaObat = o != null ? o.ObatName : null,
                        DosisObat = o != null ? o.Dosis : null,
                        d.GambaranEKG,
                        d.DCShock,
                        d.TekananDarahDiastolic,
                        d.TekananDarahSystolic,
                        d.RR,
                        d.Suhu,
                        d.SPO2,
                        d.Urine,
                        d.Pendarahan,
                        d.Muntah,
                        d.Keterangan
                    }
                ).ToListAsync();

                // ------------------------------------------------------------
                // 3. Bentuk final output seperti paged
                // ------------------------------------------------------------
                var result = new
                {
                    header.ObservasiIgdId,
                    header.KunjunganId,
                    header.PasienId,
                    header.Airway,
                    header.Breathing,
                    header.Circulation,
                    header.Disability,
                    header.Eye,
                    header.Motor,
                    header.Verbal,
                    header.AlatBantuNapas,
                    header.AlatBantuOksigenasi,
                    header.DokterId,
                    header.PerawatId,
                    header.ATS,
                    header.TglObservasi,
                    header.Keterangan,
                    header.CreateDateTime,
                    header.CreateByName,
                    Details = details
                };

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IGDObservasiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // ✔ Ambil user login
                // ============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User tidak ditemukan" });

                var userActiveId = user.UserActiveId;

                // ============================
                // ✔ Insert ke tabel ObservasiIGD
                // ============================
                var observasi = new IGDObservasi
                {
                    ObservasiIgdId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,

                    Airway = vm.Airway,
                    Breathing = vm.Breathing,
                    Circulation = vm.Circulation,
                    Disability = vm.Disability,
                    Eye = vm.Eye,
                    Motor = vm.Motor,
                    Verbal = vm.Verbal,
                    AlatBantuNapas = vm.AlatBantuNapas,
                    AlatBantuOksigenasi = vm.AlatBantuOksigenasi,

                    DokterId = vm.DokterId,
                    PerawatId = vm.PerawatId,
                    TglObservasi = vm.TglObservasi,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                await _applicationDbContext.IGDObservasis.AddAsync(observasi);
                await _applicationDbContext.SaveChangesAsync();

                // ============================
                // ✔ Insert ke tabel ObservasiIGDDetail
                // ============================
                if (vm.Details != null && vm.Details.Any())
                {
                    var detailList = vm.Details.Select(d => new IGDObservasiDetail
                    {
                        ObservasiDetailIgdId = Guid.NewGuid(),
                        ObservasiIgdId = observasi.ObservasiIgdId,

                        TglObservasi = d.TglObservasi,
                        ObatId = d.ObatId,
                        GambaranEKG = d.GambaranEKG,
                        DCShock = d.DCShock,

                        TekananDarahDiastolic = d.TekananDarahDiastolic,
                        TekananDarahSystolic = d.TekananDarahSystolic,
                        RR = d.RR,
                        Suhu = d.Suhu,
                        SPO2 = d.SPO2,
                        Urine = d.Urine,
                        Pendarahan = d.Pendarahan,
                        Muntah = d.Muntah,
                        Keterangan = d.Keterangan,

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    }).ToList();

                    await _applicationDbContext.IGDObservasiDetails.AddRangeAsync(detailList);
                    await _applicationDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Berhasil menambahkan Observasi IGD & Detail",
                    ObservasiIgdId = observasi.ObservasiIgdId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] IGDObservasiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // ✔ Ambil user login
                // ============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User tidak ditemukan" });

                var userActiveId = user.UserActiveId;

                // ============================
                // ✔ Ambil data header
                // ============================
                var data = await _applicationDbContext.IGDObservasis
                    .FirstOrDefaultAsync(x => x.ObservasiIgdId == id);

                if (data == null)
                    return NotFound(new { message = $"Observasi IGD dengan ID {id} tidak ditemukan." });

                // ============================
                // ✔ Update Header
                // ============================
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.Airway = vm.Airway;
                data.Breathing = vm.Breathing;
                data.Circulation = vm.Circulation;
                data.Disability = vm.Disability;
                data.Eye = vm.Eye;
                data.Motor = vm.Motor;
                data.Verbal = vm.Verbal;
                data.AlatBantuNapas = vm.AlatBantuNapas;
                data.AlatBantuOksigenasi = vm.AlatBantuOksigenasi;
                data.DokterId = vm.DokterId;
                data.PerawatId = vm.PerawatId;
                data.TglObservasi = vm.TglObservasi;
                data.ATS = vm.ATS;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDObservasis.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                // ============================
                // ✔ Ambil semua detail lama
                // ============================
                var existingDetails = await _applicationDbContext.IGDObservasiDetails
                    .Where(d => d.ObservasiIgdId == id)
                    .ToListAsync();

                // List ID detail lama
                var existingIds = existingDetails.Select(d => d.ObservasiDetailIgdId).ToList();

                // List ID detail yang dikirim client
                var incomingIds = vm.Details
                    .Where(d => d.IGDObservasiDetailId != null)
                    .Select(d => d.IGDObservasiDetailId.Value)
                    .ToList();

                // ============================
                // ✔ DELETE detail yang hilang dari payload baru
                // ============================
                var toDelete = existingDetails
                    .Where(d => !incomingIds.Contains((Guid)d.ObservasiDetailIgdId))
                    .ToList();

                if (toDelete.Any())
                {
                    _applicationDbContext.IGDObservasiDetails.RemoveRange(toDelete);
                    await _applicationDbContext.SaveChangesAsync();
                }

                // ============================
                // ✔ UPDATE + INSERT Detail baru
                // ============================
                foreach (var detail in vm.Details)
                {
                    if (detail.IGDObservasiDetailId != null &&
                        existingIds.Contains(detail.IGDObservasiDetailId.Value))
                    {
                        // ===================
                        // UPDATE DETAIL
                        // ===================
                        var old = existingDetails
                            .First(d => d.ObservasiDetailIgdId == detail.IGDObservasiDetailId);

                        old.TglObservasi = detail.TglObservasi;
                        old.ObatId = detail.ObatId;
                        old.GambaranEKG = detail.GambaranEKG;
                        old.DCShock = detail.DCShock;
                        old.TekananDarahSystolic = detail.TekananDarahSystolic;
                        old.TekananDarahDiastolic = detail.TekananDarahDiastolic;
                        old.RR = detail.RR;
                        old.Suhu = detail.Suhu;
                        old.SPO2 = detail.SPO2;
                        old.Urine = detail.Urine;
                        old.Pendarahan = detail.Pendarahan;
                        old.Muntah = detail.Muntah;
                        old.Keterangan = detail.Keterangan;

                        old.UpdateBy = userActiveId;
                        old.UpdateDateTime = DateTimeOffset.UtcNow;

                        _applicationDbContext.IGDObservasiDetails.Update(old);
                    }
                    else
                    {
                        // ===================
                        // INSERT DETAIL BARU
                        // ===================
                        var newDetail = new IGDObservasiDetail
                        {
                            ObservasiDetailIgdId = Guid.NewGuid(),
                            ObservasiIgdId = id,

                            TglObservasi = detail.TglObservasi,
                            ObatId = detail.ObatId,
                            GambaranEKG = detail.GambaranEKG,
                            DCShock = detail.DCShock,
                            TekananDarahDiastolic = detail.TekananDarahDiastolic,
                            TekananDarahSystolic = detail.TekananDarahSystolic,
                            RR = detail.RR,
                            Suhu = detail.Suhu,
                            SPO2 = detail.SPO2,
                            Urine = detail.Urine,
                            Pendarahan = detail.Pendarahan,
                            Muntah = detail.Muntah,
                            Keterangan = detail.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        await _applicationDbContext.IGDObservasiDetails.AddAsync(newDetail);
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update Observasi IGD & Detail berhasil",
                    ObservasiIgdId = id
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
            using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ---------- Ambil user login ----------
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;


                // ---------- Cari header ----------
                var header = await _applicationDbContext.IGDObservasis
                    .FirstOrDefaultAsync(o => o.ObservasiIgdId == id && (o.IsDelete == false || o.IsDelete == null));

                if (header == null)
                    return NotFound(new { message = $"Observasi IGD dengan ID {id} tidak ditemukan." });


                // ---------- Soft delete header ----------
                header.IsDelete = true;
                header.DeleteBy = userActiveId;
                header.DeleteDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDObservasis.Update(header);


                // ---------- Cari semua detail ----------
                var details = await _applicationDbContext.IGDObservasiDetails
                    .Where(d => d.ObservasiIgdId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                // ---------- Soft delete detail ----------
                if (details.Any())
                {
                    foreach (var d in details)
                    {
                        d.IsDelete = true;
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = DateTimeOffset.UtcNow;
                    }

                    _applicationDbContext.IGDObservasiDetails.UpdateRange(details);
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();


                return Ok(new
                {
                    message = "Soft delete Observasi IGD dan detail berhasil.",
                    ObservasiIgdId = id
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Gagal soft delete: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganId = null,
        Guid? pasienId = null,
        //string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time")] DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time")] DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // ---------------------------------------------
            // 1. Base query (HEADER + USER)
            // ---------------------------------------------
            var baseQuery =
                from h in _applicationDbContext.IGDObservasis
                join u in _applicationDbContext.UserActives
                    on h.CreateBy equals u.UserActiveId
                where h.IsDelete == false || h.IsDelete == null
                select new
                {
                    h.ObservasiIgdId,
                    h.KunjunganId,
                    h.PasienId,
                    h.Airway,
                    h.Breathing,
                    h.Circulation,
                    h.Disability,
                    h.Eye,
                    h.Motor,
                    h.Verbal,
                    h.AlatBantuNapas,
                    h.AlatBantuOksigenasi,
                    h.DokterId,
                    h.PerawatId,
                    h.TglObservasi,
                    h.Keterangan,
                    h.ATS,
                    h.CreateDateTime,
                    CreateByName = u.FullName
                };

            // ---------------------------------------------
            // 2. Filter: KunjunganId
            // ---------------------------------------------
            if (kunjunganId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.KunjunganId == kunjunganId.Value);
            }

            // ---------------------------------------------
            // 3. Filter: Pasien ID
            // ---------------------------------------------
            if (pasienId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.PasienId == pasienId.Value);
            }
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    string keyword = $"%{search.ToLower()}%";

            //    baseQuery = baseQuery.Where(x =>
            //        EF.Functions.ILike(x.Keterangan, keyword));
            //}

            // ---------------------------------------------
            // 4. Filter tanggal
            // ---------------------------------------------
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                baseQuery = baseQuery.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // ---------------------------------------------
            // 5. Filter Periode
            // ---------------------------------------------
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        baseQuery = baseQuery.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Month == today.Month - 1 &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // ---------------------------------------------
            // 6. Sorting
            // ---------------------------------------------
            baseQuery =
                sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => baseQuery.OrderByDescending(x => x.CreateByName),
                    "TglObservasi" => baseQuery.OrderByDescending(x => x.TglObservasi),
                    _ => baseQuery.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => baseQuery.OrderBy(x => x.CreateByName),
                    "TglObservasi" => baseQuery.OrderBy(x => x.TglObservasi),
                    _ => baseQuery.OrderBy(x => x.CreateDateTime)
                };


            // ---------------------------------------------
            // 7. Hitung Total Rows
            // ---------------------------------------------
            int totalRows = await baseQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // ---------------------------------------------
            // 8. Ambil halaman data
            // ---------------------------------------------
            var headers = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!headers.Any())
                return Ok(new { status = "success", data = new { Rows = new List<object>(), TotalRows = 0 } });


            // ---------------------------------------------
            // 9. Ambil semua ObservasiIgdId dari header
            // ---------------------------------------------
            var headerIds = headers.Select(h => h.ObservasiIgdId).ToList();

            // ---------------------------------------------
            // 10. Query DETAIL + join OBAT (tanpa N+1!)
            // ---------------------------------------------
            var detailQuery =
                from d in _applicationDbContext.IGDObservasiDetails
                join u in _applicationDbContext.UserActives
                on d.CreateBy equals u.UserActiveId
                join o in _applicationDbContext.Obats
                    on d.ObatId equals o.ObatId into obatGroup
                from o in obatGroup.DefaultIfEmpty()
                where headerIds.Contains(d.ObservasiIgdId)
                      && (d.IsDelete == false || d.IsDelete == null)
                select new
                {
                    d.ObservasiIgdId,
                    d.TglObservasi,
                    d.ObatId,
                    NamaObat = o != null ? o.ObatName : null,
                    DosisObat = o != null ? o.Dosis : null,
                    d.GambaranEKG,
                    d.DCShock,
                    d.TekananDarahDiastolic,
                    d.TekananDarahSystolic,
                    d.RR,
                    d.Suhu,
                    d.SPO2,
                    d.Urine,
                    d.Pendarahan,
                    d.Muntah,
                    d.Keterangan,
                    d.CreateDateTime,
                    CreateBy = u.FullName
                };

            var details = await detailQuery.ToListAsync();

            // ---------------------------------------------
            // 11. Group Detail berdasarkan header ID
            // ---------------------------------------------
            var finalResult = headers.Select(h => new
            {
                h.ObservasiIgdId,
                h.KunjunganId,
                h.PasienId,
                h.Airway,
                h.Breathing,
                h.Circulation,
                h.Disability,
                h.Eye,
                h.Motor,
                h.Verbal,
                h.AlatBantuNapas,
                h.AlatBantuOksigenasi,
                h.DokterId,
                h.PerawatId,
                h.TglObservasi,
                h.ATS,
                h.Keterangan,
                h.CreateDateTime,
                h.CreateByName,

                Details = details
                    .Where(d => d.ObservasiIgdId == h.ObservasiIgdId)
                    .ToList()
            });


            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = finalResult,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }





    }
}
