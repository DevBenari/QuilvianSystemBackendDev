using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabHasilController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabHasilController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabHasilController> logger,
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
            var query = (from a in _applicationDbContext.LabHasils
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         join k in _applicationDbContext.Kunjungans
                         on a.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.HasilLabId,
                             a.KunjunganId,
                             k.JenisKunjungan,
                             a.LabId,
                             a.LabBookingId,
                             a.UserActiveId,
                             a.PenanggungJawabId,
                             a.PenanggungJawabAnalisId,
                             a.TanggalPemeriksaan,
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
            var listdata = (from a in _applicationDbContext.LabHasils
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId

                            join k in _applicationDbContext.Kunjungans
                            on a.KunjunganId equals k.KunjunganID into kGroup
                            from k in kGroup.DefaultIfEmpty()

                            where a.IsDelete == false || a.IsDelete == null
                            orderby a.CreateDateTime descending
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.HasilLabId,
                                a.KunjunganId,
                                k.JenisKunjungan,
                                a.LabId,
                                a.LabBookingId,
                                a.UserActiveId,
                                a.PenanggungJawabId,
                                a.PenanggungJawabAnalisId,
                                a.TanggalPemeriksaan,
                                a.Keterangan,
                            });
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
        public async Task<IActionResult> Create([FromBody] LabHasilViewModel vm)
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

                //// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new LabHasil
                {
                    HasilLabId = Guid.NewGuid(),
                    KunjunganId =  vm.KunjunganId,
                    LabId =  vm.LabId,
                    LabBookingId = vm.LabBookingId,
                    UserActiveId = vm.UserActiveId,
                    PenanggungJawabAnalisId = vm.PenanggungJawabId,
                    PenanggungJawabId = vm.PenanggungJawabId,
                    TanggalPemeriksaan = vm.TanggalPemeriksaan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.LabHasils.Add(data);
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
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LabHasilViewModel vm)
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
                var data = await _applicationDbContext.LabHasils.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.LabId = vm.LabId;
                data.LabBookingId = vm.LabBookingId;
                data.UserActiveId = vm.UserActiveId;
                data.PenanggungJawabAnalisId = vm.PenanggungJawabId;
                data.PenanggungJawabId = vm.PenanggungJawabId;
                data.TanggalPemeriksaan = vm.TanggalPemeriksaan;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabHasils.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
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
                var data = await _applicationDbContext.LabHasils.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabHasils.Update(data);
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
        [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
        [FromQuery] Guid? kunjunganId = null,
        [FromQuery] string? namaLab = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.LabHasils
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         join k in _applicationDbContext.Kunjungans
                         on a.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                         join l in _applicationDbContext.Labs
                         on a.LabId equals l.LabId into lGroup
                         from l in lGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null orderby a.CreateDateTime descending
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.HasilLabId,
                             a.KunjunganId,
                             k.JenisKunjungan,
                             a.LabId,
                             NamaLab=l.NamaLab ?? null,
                             a.LabBookingId,
                             a.UserActiveId,
                             a.PenanggungJawabId,
                             a.PenanggungJawabAnalisId,
                             a.TanggalPemeriksaan,
                             a.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                namaLab = $"%{namaLab.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLab, namaLab)
                );
            }

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // filter based on jenis kunjungan
            if (JenisKunjungan.HasValue) query = query.Where(u => u.JenisKunjungan == JenisKunjungan.Value.ToString());


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



        [HttpGet("pagedRadiologi")]
            public async Task<IActionResult> PagedRadiologi(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =============================
                // 0) Ambil LabId radiologi (sekali)
                // =============================
                var radiologiLabIds = await _applicationDbContext.Labs
                    .AsNoTracking()
                    .Where(l => l.NamaLab != null &&
                                l.NamaLab.ToLower().Replace(" ", "") == "radiologi")
                    .Select(l => l.LabId)
                    .ToListAsync(ct);

                if (radiologiLabIds.Count == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "Data Radiologi retrieved successfully",
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

                // =============================
                // 1) BASE QUERY: LabBookings (filter & EXISTS detail radiologi)
                // =============================
                var baseQuery = _applicationDbContext.LabBookings
                    .AsNoTracking()
                    .Where(b => b.IsDelete == false || b.IsDelete == null);

                if (kunjunganId.HasValue)
                    baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

                if (labBookingId.HasValue)
                    baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

                if (startDate.HasValue && endDate.HasValue)
                {
                    // inclusive start, exclusive end+1 hari (lebih aman daripada ticks)
                    var start = startDate.Value.Date;
                    var endExclusive = endDate.Value.Date.AddDays(1);

                    baseQuery = baseQuery.Where(b =>
                        b.CreateDateTime >= start &&
                        b.CreateDateTime < endExclusive);
                }

                if (periode.HasValue)
                {
                    var today = DateTime.UtcNow.Date;

                    DateTime start;
                    DateTime endExclusive;

                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            start = today;
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Yesterday:
                            start = today.AddDays(-1);
                            endExclusive = today;
                            break;

                        case PeriodeFilter.ThisWeek:
                            // start week: Minggu (Sunday) default .NET DayOfWeek
                            start = today.AddDays(-(int)today.DayOfWeek);
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.LastWeek:
                            var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                            start = startThisWeek.AddDays(-7);
                            endExclusive = startThisWeek;
                            break;

                        case PeriodeFilter.ThisMonth:
                            start = new DateTime(today.Year, today.Month, 1);
                            endExclusive = start.AddMonths(1);
                            break;

                        case PeriodeFilter.LastMonth:
                            var startThisMonth = new DateTime(today.Year, today.Month, 1);
                            start = startThisMonth.AddMonths(-1);
                            endExclusive = startThisMonth;
                            break;

                        case PeriodeFilter.ThisYear:
                            start = new DateTime(today.Year, 1, 1);
                            endExclusive = start.AddYears(1);
                            break;

                        case PeriodeFilter.LastYear:
                            var startThisYear2 = new DateTime(today.Year, 1, 1);
                            start = startThisYear2.AddYears(-1);
                            endExclusive = startThisYear2;
                            break;

                        case PeriodeFilter.Last3Months:
                            start = today.AddMonths(-3);
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Last6Months:
                            start = today.AddMonths(-6);
                            endExclusive = today.AddDays(1);
                            break;

                        default:
                            start = DateTime.MinValue;
                            endExclusive = DateTime.MaxValue;
                            break;
                    }

                    baseQuery = baseQuery.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                }

                // Filter booking yang punya detail radiologi (detail tidak delete)
                baseQuery = baseQuery.Where(b =>
                        _applicationDbContext.LabBookingDetails.Any(d =>
                            d.BookingLabId == b.BookingLabId
                            && (d.IsDelete == false || d.IsDelete == null)
                            && d.LabId.HasValue
                            && radiologiLabIds.Contains(d.LabId.Value)
                        )
                    );

                // =============================
                // 2) TOTAL rows
                // =============================
                var totalRows = await baseQuery.CountAsync(ct);
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                // =============================
                // 3) SORTING (aman)
                // =============================
                bool desc = (sortDirection ?? "desc")
                    .Equals("desc", StringComparison.OrdinalIgnoreCase);

                IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
                {
                    "TglBooking" =>
                        desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                    "TglPemeriksaan" =>
                        desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                    _ =>
                        desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
                };

                // =============================
                // 4) PAGING: ambil ID dulu
                // =============================
                var pagedParentIds = await sortedQuery
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(b => b.BookingLabId)
                    .ToListAsync(ct);

                if (pagedParentIds.Count == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "Data Radiologi retrieved successfully",
                        data = new
                        {
                            Rows = new List<object>(),
                            TotalRows = totalRows,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = totalPages
                        }
                    });
                }

                var pagedIdSet = pagedParentIds.ToHashSet();

                // =============================
                // 5) LOAD PARENT DATA (hanya untuk page ini)
                // =============================
                var parents = await (
                    from b in _applicationDbContext.LabBookings.AsNoTracking()
                    where pagedIdSet.Contains(b.BookingLabId)

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on b.CreateBy equals u.UserActiveId into uJoin
                    from u in uJoin.DefaultIfEmpty()

                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on b.KunjunganId equals k.KunjunganID into kJoin
                    from k in kJoin.DefaultIfEmpty()

                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on b.AsuransiId equals a.AsuransiId into aJoin
                    from a in aJoin.DefaultIfEmpty()

                    join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                        on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                    from p in pJoin.DefaultIfEmpty()

                    join d1 in _applicationDbContext.Dokters.AsNoTracking()
                        on b.DokterId equals d1.DokterId into dJoin
                    from d1 in dJoin.DefaultIfEmpty()

                    join d2 in _applicationDbContext.Dokters.AsNoTracking()
                        on b.DokterKonsulenId equals d2.DokterId into d2Join
                    from d2 in d2Join.DefaultIfEmpty()

                    join po in _applicationDbContext.Polikliniks.AsNoTracking()
                        on k.PoliklinikId equals po.PoliklinikId into poJoin
                    from po in poJoin.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass.AsNoTracking()
                        on b.KelasId equals kl.KelasId into klJoin
                    from kl in klJoin.DefaultIfEmpty()
                    
                    select new
                    {
                        b.BookingLabId,
                        b.KunjunganId,
                        PoliklinikId = (Guid?)k.PoliklinikId,
                        k.AsalKunjungan,
                        b.PasienId,
                        NamaLengkap = p.NamaLengkap,
                        b.NoOrder,
                        NoRekamMedis = p.NoRekamMedis,
                        b.AsuransiId,
                        AsuransiNama = a.NamaAsuransi,
                        b.DokterId,
                        DokterNama = d1.NmDokter,
                        PoliNama = po.NamaPoliklinik,
                        b.TglPemeriksaan,
                        b.TglBooking,
                        b.AlasanPembatalan,
                        b.StatusBookingLab,
                        b.StatusPembayaran,
                        b.KelasId,
                        NamaKelas = kl.NamaKelas,
                        b.HemodialisaKe,
                        b.StatusPemeriksaan,
                        b.NomorSuratJaminan,
                        b.DokterKonsulenId,
                        NamaDokterKonsulen = d2.NmDokter,
                        b.DiagnosaAwal,
                        b.Keterangan,
                        b.TTDPathPembatalan,
                        b.PetugasPembatalan,
                        b.CreateDateTime,
                        b.TindakLanjut,
                        b.HasilPenunjangLab,
                        b.AnjuranDiet,
                        b.IsDelete,
                        b.IsCito,
                        CreateByName = u.FullName
                    }
                ).ToListAsync(ct);

                var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

                // =============================
                // 6) LOAD DETAIL (hanya untuk page ini + radiologi)
                // =============================
                var details = await (
                    from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                    join lab in _applicationDbContext.Labs.AsNoTracking()
                        on d.LabId equals lab.LabId into labJoin
                    from lab in labJoin.DefaultIfEmpty()

                    join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                        on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                    from lp in lpJoin.DefaultIfEmpty()

                    where d.BookingLabId.HasValue
                          && pagedIdSet.Contains(d.BookingLabId.Value)
                          && (d.IsDelete == false || d.IsDelete == null)
                          && d.LabId.HasValue
                          && radiologiLabIds.Contains(d.LabId.Value)

                    select new
                    {
                        BookingLabId = d.BookingLabId.Value,
                        d.DetailBookingLabId,
                        d.NoOrder,
                        NamaPemeriksaan = lp.NamaPemeriksaan,
                        HargaPemeriksaan = lp.HargaPemeriksaan,
                        NamaLab = lab.NamaLab,
                        d.Satuan
                    }
                ).ToListAsync(ct);

                // ✅ ubah jadi Dictionary<Guid, List<object>>
                var detailLookup = details
                    .GroupBy(x => x.BookingLabId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (object)x).ToList()
                    );

                // =============================
                // 7) MERGE (urut sesuai paging)
                // =============================
                var merged = pagedParentIds
                    .Where(id => parentLookup.ContainsKey(id))
                    .Select(id => new
                    {
                        Parent = parentLookup[id],
                        Details = detailLookup.TryGetValue(id, out var det) ? det : new List<object>()
                    })
                    .ToList();

                // =============================
                // 8) RETURN
                // =============================
                return Ok(new
                    {
                        status = "success",
                        message = "Data Radiologi retrieved successfully",
                        data = new
                        {
                            Rows = merged,
                            TotalRows = totalRows,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = totalPages
                        }
                    });
            }

        [HttpGet("pagedRehabMedis")]
            public async Task<IActionResult> PagedRehabMedis(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
                {
                    if (page < 1) page = 1;
                    if (perPage < 1) perPage = 10;

                    // =============================
                    // 0) Ambil LabId rehabmedis (sekali)
                    // =============================
                    var rehabmedisLabIds = await _applicationDbContext.Labs
                            .AsNoTracking()
                            .Where(l => l.NamaLab != null &&
                                        l.NamaLab.ToLower().Replace(" ", "") == "rehabmedis")
                            .Select(l => l.LabId)
                            .ToListAsync(ct);

                    if (rehabmedisLabIds.Count == 0)
                    {
                        return Ok(new
                        {
                            status = "success",
                            message = "Data Rehab Medis retrieved successfully",
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

                    // =============================
                    // 1) BASE QUERY: LabBookings (filter & EXISTS detail radiologi)
                    // =============================
                    var baseQuery = _applicationDbContext.LabBookings
                        .AsNoTracking()
                        .Where(b => b.IsDelete == false || b.IsDelete == null);

                    if (kunjunganId.HasValue)
                        baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

                    if (labBookingId.HasValue)
                        baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        // inclusive start, exclusive end+1 hari (lebih aman daripada ticks)
                        var start = startDate.Value.Date;
                        var endExclusive = endDate.Value.Date.AddDays(1);

                        baseQuery = baseQuery.Where(b =>
                            b.CreateDateTime >= start &&
                            b.CreateDateTime < endExclusive);
                    }

                if (periode.HasValue)
                {
                    var today = DateTime.UtcNow.Date;

                    DateTime start;
                    DateTime endExclusive;

                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            start = today;
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Yesterday:
                            start = today.AddDays(-1);
                            endExclusive = today;
                            break;

                        case PeriodeFilter.ThisWeek:
                            // start week: Minggu (Sunday) default .NET DayOfWeek
                            start = today.AddDays(-(int)today.DayOfWeek);
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.LastWeek:
                            var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                            start = startThisWeek.AddDays(-7);
                            endExclusive = startThisWeek;
                            break;

                        case PeriodeFilter.ThisMonth:
                            start = new DateTime(today.Year, today.Month, 1);
                            endExclusive = start.AddMonths(1);
                            break;

                        case PeriodeFilter.LastMonth:
                            var startThisMonth = new DateTime(today.Year, today.Month, 1);
                            start = startThisMonth.AddMonths(-1);
                            endExclusive = startThisMonth;
                            break;

                        case PeriodeFilter.ThisYear:
                            start = new DateTime(today.Year, 1, 1);
                            endExclusive = start.AddYears(1);
                            break;

                        case PeriodeFilter.LastYear:
                            var startThisYear2 = new DateTime(today.Year, 1, 1);
                            start = startThisYear2.AddYears(-1);
                            endExclusive = startThisYear2;
                            break;

                        case PeriodeFilter.Last3Months:
                            start = today.AddMonths(-3);
                            endExclusive = today.AddDays(1);
                            break;

                        case PeriodeFilter.Last6Months:
                            start = today.AddMonths(-6);
                            endExclusive = today.AddDays(1);
                            break;

                        default:
                            start = DateTime.MinValue;
                            endExclusive = DateTime.MaxValue;
                            break;
                    }

                    baseQuery = baseQuery.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                }

                // Filter booking yang punya detail rehabmedis (detail tidak delete)
                baseQuery = baseQuery.Where(b =>
                                _applicationDbContext.LabBookingDetails.Any(d =>
                                    d.BookingLabId == b.BookingLabId
                                    && (d.IsDelete == false || d.IsDelete == null)
                                    && d.LabId.HasValue
                                    && rehabmedisLabIds.Contains(d.LabId.Value)
                                )
                );

                    // =============================
                    // 2) TOTAL rows
                    // =============================
                    var totalRows = await baseQuery.CountAsync(ct);
                    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                    // =============================
                    // 3) SORTING (aman)
                    // =============================
                    bool desc = (sortDirection ?? "desc")
                        .Equals("desc", StringComparison.OrdinalIgnoreCase);

                    IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
                    {
                        "TglBooking" =>
                            desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                        "TglPemeriksaan" =>
                            desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                        _ =>
                            desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
                    };

                    // =============================
                    // 4) PAGING: ambil ID dulu
                    // =============================
                    var pagedParentIds = await sortedQuery
                        .Skip((page - 1) * perPage)
                        .Take(perPage)
                        .Select(b => b.BookingLabId)
                        .ToListAsync(ct);

                    if (pagedParentIds.Count == 0)
                    {
                        return Ok(new
                        {
                            status = "success",
                            message = "Data Rehab Medis retrieved successfully",
                            data = new
                            {
                                Rows = new List<object>(),
                                TotalRows = totalRows,
                                CurrentPage = page,
                                PerPage = perPage,
                                TotalPages = totalPages
                            }
                        });
                    }

                    var pagedIdSet = pagedParentIds.ToHashSet();

                    // =============================
                    // 5) LOAD PARENT DATA (hanya untuk page ini)
                    // =============================
                    var parents = await (
                        from b in _applicationDbContext.LabBookings.AsNoTracking()
                        where pagedIdSet.Contains(b.BookingLabId)

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                            on b.CreateBy equals u.UserActiveId into uJoin
                        from u in uJoin.DefaultIfEmpty()

                        join k in _applicationDbContext.Kunjungans.AsNoTracking()
                            on b.KunjunganId equals k.KunjunganID into kJoin
                        from k in kJoin.DefaultIfEmpty()

                        join a in _applicationDbContext.Asuransis.AsNoTracking()
                            on b.AsuransiId equals a.AsuransiId into aJoin
                        from a in aJoin.DefaultIfEmpty()

                        join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                            on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                        from p in pJoin.DefaultIfEmpty()

                        join d1 in _applicationDbContext.Dokters.AsNoTracking()
                            on b.DokterId equals d1.DokterId into dJoin
                        from d1 in dJoin.DefaultIfEmpty()

                        join d2 in _applicationDbContext.Dokters.AsNoTracking()
                            on b.DokterKonsulenId equals d2.DokterId into d2Join
                        from d2 in d2Join.DefaultIfEmpty()

                        join po in _applicationDbContext.Polikliniks.AsNoTracking()
                            on k.PoliklinikId equals po.PoliklinikId into poJoin
                        from po in poJoin.DefaultIfEmpty()

                        join kl in _applicationDbContext.Kelass.AsNoTracking()
                            on b.KelasId equals kl.KelasId into klJoin
                        from kl in klJoin.DefaultIfEmpty()

                        select new
                        {
                            b.BookingLabId,
                            b.KunjunganId,
                            PoliklinikId = (Guid?)k.PoliklinikId,
                            k.AsalKunjungan,
                            b.PasienId,
                            NamaLengkap = p.NamaLengkap,
                            b.NoOrder,
                            NoRekamMedis = p.NoRekamMedis,
                            b.AsuransiId,
                            AsuransiNama = a.NamaAsuransi,
                            b.DokterId,
                            DokterNama = d1.NmDokter,
                            PoliNama = po.NamaPoliklinik,
                            b.TglPemeriksaan,
                            b.TglBooking,
                            b.AlasanPembatalan,
                            b.StatusBookingLab,
                            b.StatusPembayaran,
                            b.KelasId,
                            NamaKelas = kl.NamaKelas,
                            b.HemodialisaKe,
                            b.StatusPemeriksaan,
                            b.NomorSuratJaminan,
                            b.DokterKonsulenId,
                            NamaDokterKonsulen = d2.NmDokter,
                            b.DiagnosaAwal,
                            b.Keterangan,
                            b.TTDPathPembatalan,
                            b.PetugasPembatalan,
                            b.CreateDateTime,
                            b.TindakLanjut,
                            b.HasilPenunjangLab,
                            b.AnjuranDiet,
                            b.IsDelete,
                            b.IsCito,
                            CreateByName = u.FullName
                        }
                    ).ToListAsync(ct);

                    var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

                    // =============================
                    // 6) LOAD DETAIL (hanya untuk page ini + rehabmedis)
                    // =============================
                    var details = await (
                        from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                        join lab in _applicationDbContext.Labs.AsNoTracking()
                            on d.LabId equals lab.LabId into labJoin
                        from lab in labJoin.DefaultIfEmpty()

                        join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                            on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                        from lp in lpJoin.DefaultIfEmpty()

                        where d.BookingLabId.HasValue
                              && pagedIdSet.Contains(d.BookingLabId.Value)
                              && (d.IsDelete == false || d.IsDelete == null)
                              && d.LabId.HasValue
                              && rehabmedisLabIds.Contains(d.LabId.Value)

                        select new
                        {
                            BookingLabId = d.BookingLabId.Value,
                            d.DetailBookingLabId,
                            d.NoOrder,
                            NamaPemeriksaan = lp.NamaPemeriksaan,
                            HargaPemeriksaan = lp.HargaPemeriksaan,
                            NamaLab = lab.NamaLab,
                            d.Satuan
                        }
                    ).ToListAsync(ct);

                    // ✅ ubah jadi Dictionary<Guid, List<object>>
                    var detailLookup = details
                        .GroupBy(x => x.BookingLabId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => (object)x).ToList()
                        );

                    // =============================
                    // 7) MERGE (urut sesuai paging)
                    // =============================
                    var merged = pagedParentIds
                        .Where(id => parentLookup.ContainsKey(id))
                        .Select(id => new
                        {
                            Parent = parentLookup[id],
                            Details = detailLookup.TryGetValue(id, out var det) ? det : new List<object>()
                        })
                        .ToList();

                    // =============================
                    // 8) RETURN
                    // =============================
                    return Ok(new
                    {
                        status = "success",
                        message = "Data Rehab Medis retrieved successfully",
                        data = new
                        {
                            Rows = merged,
                            TotalRows = totalRows,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = totalPages
                        }
                    });
                }

        [HttpGet("pagedLabGizi")]
        public async Task<IActionResult> PagedLabGizi(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId gizi (sekali)
            // =============================
            var giziLabIds = await _applicationDbContext.Labs
                    .AsNoTracking()
                    .Where(l => l.NamaLab != null &&
                                l.NamaLab.ToLower().Replace(" ", "") == "gizi")
                    .Select(l => l.LabId)
                    .ToListAsync(ct);

            if (giziLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Gizi retrieved successfully",
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

            // =============================
            // 1) BASE QUERY: LabBookings (filter & EXISTS detail radiologi)
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                // inclusive start, exclusive end+1 hari (lebih aman daripada ticks)
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime < endExclusive);
            }

            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime start;
                DateTime endExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Yesterday:
                        start = today.AddDays(-1);
                        endExclusive = today;
                        break;

                    case PeriodeFilter.ThisWeek:
                        // start week: Minggu (Sunday) default .NET DayOfWeek
                        start = today.AddDays(-(int)today.DayOfWeek);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                        start = startThisWeek.AddDays(-7);
                        endExclusive = startThisWeek;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var startThisMonth = new DateTime(today.Year, today.Month, 1);
                        start = startThisMonth.AddMonths(-1);
                        endExclusive = startThisMonth;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var startThisYear2 = new DateTime(today.Year, 1, 1);
                        start = startThisYear2.AddYears(-1);
                        endExclusive = startThisYear2;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    default:
                        start = DateTime.MinValue;
                        endExclusive = DateTime.MaxValue;
                        break;
                }

                baseQuery = baseQuery.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
            }

            // Filter booking yang punya detail gizi (detail tidak delete)
            baseQuery = baseQuery.Where(b =>
                    _applicationDbContext.LabBookingDetails.Any(d =>
                        d.BookingLabId == b.BookingLabId
                        && (d.IsDelete == false || d.IsDelete == null)
                        && d.LabId.HasValue
                        && giziLabIds.Contains(d.LabId.Value)
                    )
            );

            // =============================
            // 2) TOTAL rows
            // =============================
            var totalRows = await baseQuery.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // =============================
            // 3) SORTING (aman)
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 4) PAGING: ambil ID dulu
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync(ct);

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Gizi retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 5) LOAD PARENT DATA (hanya untuk page ini)
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()
                where pagedIdSet.Contains(b.BookingLabId)

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uJoin
                from u in uJoin.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on b.KunjunganId equals k.KunjunganID into kJoin
                from k in kJoin.DefaultIfEmpty()

                join a in _applicationDbContext.Asuransis.AsNoTracking()
                    on b.AsuransiId equals a.AsuransiId into aJoin
                from a in aJoin.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                from p in pJoin.DefaultIfEmpty()

                join d1 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterId equals d1.DokterId into dJoin
                from d1 in dJoin.DefaultIfEmpty()

                join d2 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterKonsulenId equals d2.DokterId into d2Join
                from d2 in d2Join.DefaultIfEmpty()

                join po in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals po.PoliklinikId into poJoin
                from po in poJoin.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klJoin
                from kl in klJoin.DefaultIfEmpty()

                select new
                {
                    b.BookingLabId,
                    b.KunjunganId,
                    PoliklinikId = (Guid?)k.PoliklinikId,
                    k.AsalKunjungan,
                    b.PasienId,
                    NamaLengkap = p.NamaLengkap,
                    b.NoOrder,
                    NoRekamMedis = p.NoRekamMedis,
                    b.AsuransiId,
                    AsuransiNama = a.NamaAsuransi,
                    b.DokterId,
                    DokterNama = d1.NmDokter,
                    PoliNama = po.NamaPoliklinik,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.StatusPembayaran,
                    b.KelasId,
                    NamaKelas = kl.NamaKelas,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,
                    b.DokterKonsulenId,
                    NamaDokterKonsulen = d2.NmDokter,
                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.TTDPathPembatalan,
                    b.PetugasPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    b.IsCito,
                    CreateByName = u.FullName
                }
            ).ToListAsync(ct);

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 6) LOAD DETAIL (hanya untuk page ini + gizi)
            // =============================
            var details = await (
                from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                join lab in _applicationDbContext.Labs.AsNoTracking()
                    on d.LabId equals lab.LabId into labJoin
                from lab in labJoin.DefaultIfEmpty()

                join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                from lp in lpJoin.DefaultIfEmpty()

                where d.BookingLabId.HasValue
                      && pagedIdSet.Contains(d.BookingLabId.Value)
                      && (d.IsDelete == false || d.IsDelete == null)
                      && d.LabId.HasValue
                      && giziLabIds.Contains(d.LabId.Value)

                select new
                {
                    BookingLabId = d.BookingLabId.Value,
                    d.DetailBookingLabId,
                    d.NoOrder,
                    NamaPemeriksaan = lp.NamaPemeriksaan,
                    HargaPemeriksaan = lp.HargaPemeriksaan,
                    NamaLab = lab.NamaLab,
                    d.Satuan
                }
            ).ToListAsync(ct);

            // ✅ ubah jadi Dictionary<Guid, List<object>>
            var detailLookup = details
                .GroupBy(x => x.BookingLabId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (object)x).ToList()
                );

            // =============================
            // 7) MERGE (urut sesuai paging)
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : new List<object>()
                })
                .ToList();

            // =============================
            // 8) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data Gizi retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }


        [HttpGet("pagedLabMCU")]
        public async Task<IActionResult> PagedLabMCU(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId MCU (sekali)
            // =============================
            var MCULabIds = await _applicationDbContext.Labs
                    .AsNoTracking()
                    .Where(l => l.NamaLab != null &&
                                l.NamaLab.ToLower().Replace(" ", "") == "mcu")
                    .Select(l => l.LabId)
                    .ToListAsync(ct);

            if (MCULabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data MCU retrieved successfully",
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

            // =============================
            // 1) BASE QUERY: LabBookings (filter & EXISTS detail radiologi)
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                // inclusive start, exclusive end+1 hari (lebih aman daripada ticks)
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime < endExclusive);
            }

            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime start;
                DateTime endExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Yesterday:
                        start = today.AddDays(-1);
                        endExclusive = today;
                        break;

                    case PeriodeFilter.ThisWeek:
                        // start week: Minggu (Sunday) default .NET DayOfWeek
                        start = today.AddDays(-(int)today.DayOfWeek);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                        start = startThisWeek.AddDays(-7);
                        endExclusive = startThisWeek;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var startThisMonth = new DateTime(today.Year, today.Month, 1);
                        start = startThisMonth.AddMonths(-1);
                        endExclusive = startThisMonth;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var startThisYear2 = new DateTime(today.Year, 1, 1);
                        start = startThisYear2.AddYears(-1);
                        endExclusive = startThisYear2;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    default:
                        start = DateTime.MinValue;
                        endExclusive = DateTime.MaxValue;
                        break;
                }

                baseQuery = baseQuery.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
            }

            // Filter booking yang punya detail MCU (detail tidak delete)
            baseQuery = baseQuery.Where(b =>
                    _applicationDbContext.LabBookingDetails.Any(d =>
                        d.BookingLabId == b.BookingLabId
                        && (d.IsDelete == false || d.IsDelete == null)
                        && d.LabId.HasValue
                        && MCULabIds.Contains(d.LabId.Value)
                    )
            );

            // =============================
            // 2) TOTAL rows
            // =============================
            var totalRows = await baseQuery.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // =============================
            // 3) SORTING (aman)
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 4) PAGING: ambil ID dulu
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync(ct);

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data MCU retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 5) LOAD PARENT DATA (hanya untuk page ini)
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()
                where pagedIdSet.Contains(b.BookingLabId)

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uJoin
                from u in uJoin.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on b.KunjunganId equals k.KunjunganID into kJoin
                from k in kJoin.DefaultIfEmpty()

                join a in _applicationDbContext.Asuransis.AsNoTracking()
                    on b.AsuransiId equals a.AsuransiId into aJoin
                from a in aJoin.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                from p in pJoin.DefaultIfEmpty()

                join d1 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterId equals d1.DokterId into dJoin
                from d1 in dJoin.DefaultIfEmpty()

                join d2 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterKonsulenId equals d2.DokterId into d2Join
                from d2 in d2Join.DefaultIfEmpty()

                join po in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals po.PoliklinikId into poJoin
                from po in poJoin.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klJoin
                from kl in klJoin.DefaultIfEmpty()

                select new
                {
                    b.BookingLabId,
                    b.KunjunganId,
                    PoliklinikId = (Guid?)k.PoliklinikId,
                    k.AsalKunjungan,
                    b.PasienId,
                    NamaLengkap = p.NamaLengkap,
                    b.NoOrder,
                    NoRekamMedis = p.NoRekamMedis,
                    b.AsuransiId,
                    AsuransiNama = a.NamaAsuransi,
                    b.DokterId,
                    DokterNama = d1.NmDokter,
                    PoliNama = po.NamaPoliklinik,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.StatusPembayaran,
                    b.KelasId,
                    NamaKelas = kl.NamaKelas,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,
                    b.DokterKonsulenId,
                    NamaDokterKonsulen = d2.NmDokter,
                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.TTDPathPembatalan,
                    b.PetugasPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    b.IsCito,
                    CreateByName = u.FullName
                }
            ).ToListAsync(ct);

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 6) LOAD DETAIL (hanya untuk page ini + MCU)
            // =============================
            var details = await (
                from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                join lab in _applicationDbContext.Labs.AsNoTracking()
                    on d.LabId equals lab.LabId into labJoin
                from lab in labJoin.DefaultIfEmpty()

                join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                from lp in lpJoin.DefaultIfEmpty()

                where d.BookingLabId.HasValue
                      && pagedIdSet.Contains(d.BookingLabId.Value)
                      && (d.IsDelete == false || d.IsDelete == null)
                      && d.LabId.HasValue
                      && MCULabIds.Contains(d.LabId.Value)

                select new
                {
                    BookingLabId = d.BookingLabId.Value,
                    d.DetailBookingLabId,
                    d.NoOrder,
                    NamaPemeriksaan = lp.NamaPemeriksaan,
                    HargaPemeriksaan = lp.HargaPemeriksaan,
                    NamaLab = lab.NamaLab,
                    d.Satuan
                }
            ).ToListAsync(ct);

            // ✅ ubah jadi Dictionary<Guid, List<object>>
            var detailLookup = details
                .GroupBy(x => x.BookingLabId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (object)x).ToList()
                );

            // =============================
            // 7) MERGE (urut sesuai paging)
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : new List<object>()
                })
                .ToList();

            // =============================
            // 8) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data MCU retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }

}

