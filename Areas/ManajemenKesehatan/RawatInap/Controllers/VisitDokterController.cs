using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
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
    public class VisitDokterController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<VisitDokterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VisitDokterController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<VisitDokterController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateInvoiceBillingService generateInvoiceBillingService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local
                ); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.VisitDokters
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.VisitDokterId,
                             a.WaktuVisit,
                             a.TanggalVisit,
                             a.KelasId,
                             a.KunjunganId,
                             a.PasienId,
                             a.DokterId,
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
            var listdata = _applicationDbContext.VisitDokters.Find(id);
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
        public async Task<IActionResult> Create([FromBody] VisitDokterViewModel vm)
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

                ////// **Cek Duplikasi**
                var tanggalOnly = vm.TanggalVisit.Value.Date;

                bool isDuplicate = await _applicationDbContext.VisitDokters
                    .AnyAsync(c =>
                        c.IsDelete != true &&
                        c.KunjunganId == vm.KunjunganId &&
                        c.TanggalVisit.HasValue &&
                        c.TanggalVisit.Value.Date == tanggalOnly
                    );

                if (isDuplicate)
                    return Conflict(new { message = "Kunjungan ini telah divisit pada tanggal yang sama." });

                // **Buat Data Baru**
                var data = new VisitDokter
                {
                    VisitDokterId = Guid.NewGuid(),
                    WaktuVisit = vm.WaktuVisit,
                    TanggalVisit = (vm.TanggalVisit),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    KelasId = vm.KelasId,
                    DokterId = vm.DokterId,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };
                // **Simpan ke Database**
                _applicationDbContext.VisitDokters.Add(data);

                // simpan ke tabel billing buat visit dokter
                var dr = await _applicationDbContext.Dokters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DokterId == vm.DokterId);

                int billingCount = await _applicationDbContext.Billings
                    .CountAsync(b =>
                        b.KunjunganId == vm.KunjunganId &&
                        b.JenisBilling != null &&
                        b.JenisBilling.ToLower() == "visit dokter" &&
                        b.IsDelete != true
                    );

                int billingIndex = billingCount + 1;

                var bill = new Billing
                {
                    BillingId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    ItemId = data.VisitDokterId,
                    InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                    IsListWhiteOff = false,
                    NamaItem = $"Visit Dokter : {dr?.NmDokter ?? null}",
                    HargaItem = dr?.HargaVisit ?? 0m,
                    QtyItem = 1,
                    SubTotalItem = dr?.HargaVisit ?? 0m,
                    BillingKode = $"{billingIndex:D3}",
                    JenisBilling = "Visit Dokter",
                    StatusBilling = false,
                    BillingDate = DateTime.UtcNow,
                    Keterangan = "Biaya Visit Dokter ID",

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };
                _applicationDbContext.Billings.Add(bill);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] VisitDokterViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.KunjunganId == null || vm.PasienId == null || vm.DokterId == null)
                return BadRequest(new { message = "KunjunganId, PasienId, DokterId wajib diisi." });

            // Auth
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = await _applicationDbContext.UserActives
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // =========================
                // 1) Ambil data existing VisitDokter
                // =========================
                var data = await _applicationDbContext.VisitDokters
                    .FirstOrDefaultAsync(x => x.VisitDokterId == id && x.IsDelete != true);

                if (data == null)
                    return NotFound(new { message = "Data VisitDokter tidak ditemukan." });

                var oldDokterId = data.DokterId;
                var oldKunjunganId = data.KunjunganId;

                // =========================
                // 2) Parse tanggal (opsional)
                // =========================
                DateTime? tanggalVisitUtc = null;
                if (vm.TanggalVisit != null)
                {
                    tanggalVisitUtc = (vm.TanggalVisit);
                    if (tanggalVisitUtc == null)
                        return BadRequest(new { message = "Format TanggalVisit tidak valid (yyyy-MM-dd)." });
                }
                else
                {
                    tanggalVisitUtc = data.TanggalVisit; // keep old
                }

                // =========================
                // 3) Cek duplikasi (kunjungan + tanggal) selain record ini
                // =========================
                if (tanggalVisitUtc.HasValue)
                {
                    var tanggalOnly = tanggalVisitUtc.Value.Date;

                    bool isDuplicate = await _applicationDbContext.VisitDokters.AnyAsync(c =>
                        c.IsDelete != true &&
                        c.VisitDokterId != id &&
                        c.KunjunganId == vm.KunjunganId &&
                        c.TanggalVisit.HasValue &&
                        c.TanggalVisit.Value.Date == tanggalOnly
                    );

                    if (isDuplicate)
                        return Conflict(new { message = "Kunjungan ini sudah memiliki visit dokter pada tanggal yang sama." });
                }

                // =========================
                // 4) Update VisitDokter
                // =========================
                data.WaktuVisit = vm.WaktuVisit;
                data.TanggalVisit = tanggalVisitUtc;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.KelasId = vm.KelasId;
                data.DokterId = vm.DokterId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // =========================
                // 5) Billing handling
                // =========================
                // Ambil billing yang terkait VisitDokter ini (aktif)
                var existingBillings = await _applicationDbContext.Billings
                    .Where(b =>
                        b.ItemId == id &&
                        b.JenisBilling != null &&
                        b.JenisBilling.ToLower() == "visit dokter" &&
                        b.IsDelete != true
                    )
                    .ToListAsync();

                bool dokterChanged = oldDokterId != vm.DokterId;
                bool kunjunganChanged = oldKunjunganId != vm.KunjunganId;

                // Load dokter baru (buat nama + harga)
                var dr = await _applicationDbContext.Dokters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DokterId == vm.DokterId);

                if (dr == null)
                    return NotFound(new { message = "Dokter tidak ditemukan." });

                var harga = dr.HargaVisit ?? 0m;
                var namaItem = $"Visit Dokter : {dr.NmDokter}";

                if (dokterChanged || kunjunganChanged)
                {
                    // 5a) Soft delete billing lama (kalau ada)
                    if (existingBillings.Count > 0)
                    {
                        foreach (var b in existingBillings)
                        {
                            b.IsDelete = true;
                            b.DeleteBy = userActiveId;
                            b.DeleteDateTime = DateTimeOffset.UtcNow;
                        }
                    }

                    // 5b) Buat billing baru untuk dokter baru
                    int billingCount = await _applicationDbContext.Billings.CountAsync(b =>
                        b.KunjunganId == vm.KunjunganId &&
                        b.JenisBilling != null &&
                        b.JenisBilling.ToLower() == "visit dokter" &&
                        b.IsDelete != true
                    );

                    int billingIndex = billingCount + 1;

                    var bill = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = vm.KunjunganId,
                        BillingDate = DateTime.UtcNow,
                        BillingKode = $"{billingIndex:D3}",
                        InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                        IsListWhiteOff = false,
                        ItemId = data.VisitDokterId, // sama id visit, billing lama tetap ada tapi soft delete
                        NamaItem = namaItem,
                        HargaItem = harga,
                        QtyItem = 1,
                        SubTotalItem = harga,

                        JenisBilling = "Visit Dokter",
                        Keterangan = "Biaya Visit Dokter (update dokter / pindah kunjungan)",

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    };

                    _applicationDbContext.Billings.Add(bill);
                }
                else
                {
                    // 5c) Dokter tidak berubah → update billing yang ada (kalau ada), kalau tidak ada → buat baru
                    var b = existingBillings.FirstOrDefault();
                    if (b == null)
                    {
                        int billingCount = await _applicationDbContext.Billings.CountAsync(x =>
                            x.KunjunganId == vm.KunjunganId &&
                            x.JenisBilling != null &&
                            x.JenisBilling.ToLower() == "visit dokter" &&
                            x.IsDelete != true
                        );

                        int billingIndex = billingCount + 1;

                        b = new Billing
                        {
                            BillingId = Guid.NewGuid(),
                            KunjunganId = vm.KunjunganId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = $"{billingIndex:D3}",
                            InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                            IsListWhiteOff = false,
                            ItemId = data.VisitDokterId,
                            JenisBilling = "Visit Dokter",

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                        };

                        _applicationDbContext.Billings.Add(b);
                    }

                    
                    b.NamaItem = namaItem;
                    b.HargaItem = harga;
                    b.QtyItem = 1;
                    b.SubTotalItem = harga;
                    b.Keterangan = vm.Keterangan;

                    b.UpdateBy = userActiveId;
                    b.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Update Visit Dokter berhasil || 200 OK",
                    VisitDokterId = data.VisitDokterId,
                    KunjunganId = data.KunjunganId,
                    DokterId = data.DokterId
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
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
                var data = await _applicationDbContext.VisitDokters.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.VisitDokters.Update(data);
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
        Guid? dokterId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.VisitDokters
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.VisitDokterId,
                             a.WaktuVisit,
                             a.TanggalVisit,
                             a.KelasId,
                             a.KunjunganId,
                             a.PasienId,
                             a.DokterId,
                             a.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter by kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // filter by dokter id
            if (dokterId.HasValue)
            {
                query = query.Where(u=>u.DokterId == dokterId.Value);
            }


            // **Filter berdasarkan tanggal**
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
    }

}
