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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
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
    [EnableCors("FrontendCorsPolicy")]
    public class VisitDokterController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<VisitDokterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IKunjunganTransactionGuard _kunjunganTransactionGuard;


        public VisitDokterController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<VisitDokterController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IKunjunganTransactionGuard kunjunganTransactionGuard
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _kunjunganTransactionGuard = kunjunganTransactionGuard;

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
        public async Task<IActionResult> Create([FromBody] VisitDokterViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId, ct);

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
                //var tanggalOnly = vm.TanggalVisit.Value.Date;

                //bool isDuplicate = await _applicationDbContext.VisitDokters
                //    .AnyAsync(c =>
                //        c.IsDelete != true &&
                //        c.KunjunganId == vm.KunjunganId &&
                //        c.TanggalVisit.HasValue &&
                //        c.TanggalVisit.Value.Date == tanggalOnly
                //    );

                //if (isDuplicate)
                //    return Conflict(new { message = "Kunjungan ini telah divisit pada tanggal yang sama." });

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
                    .Where(x => x.DokterId == vm.DokterId)
                    .Select(x => x.NmDokter)
                    .SingleOrDefaultAsync();
                if (dr == null)
                {
                    return NotFound(new
                    {
                        message = $"Dokter tidak ditemukan."
                    });
                }

                var harga = await _applicationDbContext.TarifVisits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DokterId == vm.DokterId && x.KelasId == vm.KelasId);
                //**Cek jika data tarif tidak ditemukan * *
                if (harga == null)
                {
                    return NotFound(new
                    {
                        message = $"Tarif tidak ditemukan untuk Dokter dan Kelas yang dipilih."
                    });
                }

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
                    NamaItem = $"Visit Dokter : {dr ?? null}",
                    HargaItem = harga?.TarifTotal ?? 0m,
                    QtyItem = 1,
                    SubTotalItem = harga?.TarifTotal ?? 0m,
                    BillingKode = $"{billingIndex:D3}",
                    JenisBilling = "Visit Dokter",
                    StatusBilling = false,
                    BillingDate = DateTime.UtcNow,
                    Keterangan = $"Biaya Visit Dokter ID{dr}",
                    TanggalInvoice = DateTime.UtcNow,
                    TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
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
        public async Task<IActionResult> Update(Guid id, [FromBody] VisitDokterViewModel vm, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Id tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.TanggalVisit.HasValue)
                return BadRequest(new { message = "TanggalVisit wajib diisi." });

            await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId, ct);

            try
            {
                // Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil User ID dari JWT Claims
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // Ambil data existing
                var data = await _applicationDbContext.VisitDokters
                    .FirstOrDefaultAsync(x => x.VisitDokterId == id && x.IsDelete != true);

                if (data == null)
                    return NotFound(new { message = "Data Visit Dokter tidak ditemukan." });

                // Cek duplikasi (kecuali record yang sedang diupdate)
                var tanggalOnly = vm.TanggalVisit.Value.Date;

                bool isDuplicate = await _applicationDbContext.VisitDokters
                    .AnyAsync(c =>
                        c.IsDelete != true &&
                        c.VisitDokterId != id &&
                        c.KunjunganId == vm.KunjunganId &&
                        c.TanggalVisit.HasValue &&
                        c.TanggalVisit.Value.Date == tanggalOnly
                    );

                if (isDuplicate)
                    return Conflict(new { message = "Kunjungan ini telah divisit pada tanggal yang sama." });

                // Update VisitDokter
                data.WaktuVisit = vm.WaktuVisit;
                data.TanggalVisit = vm.TanggalVisit;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.KelasId = vm.KelasId;
                data.DokterId = vm.DokterId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId; // pastikan kolom ini ada
                data.UpdateDateTime = DateTimeOffset.UtcNow; // pastikan kolom ini ada

                // ===== Update Billing terkait Visit Dokter =====
                var dr = await _applicationDbContext.Dokters
                    .AsNoTracking()
                    .Where(x => x.DokterId == vm.DokterId)
                    .Select(x => x.NmDokter)
                    .SingleOrDefaultAsync();

                //var harga = await _applicationDbContext.TarifKelass
                //    .AsNoTracking()
                //    .FirstOrDefaultAsync(x => x.DokterId == vm.DokterId && x.KelasId == vm.KelasId);

                // Cari billing yang terkait item visit dokter ini
                var bill = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b =>
                        b.IsDelete != true &&
                        b.KunjunganId == vm.KunjunganId &&
                        b.ItemId == data.VisitDokterId &&
                        b.JenisBilling != null &&
                        b.JenisBilling.ToLower() == "visit dokter"
                    );

                if (bill != null)
                {
                    bill.NamaItem = $"Visit Dokter : {dr ?? null}";
                    //bill.HargaItem = harga?.TarifTotal ?? 0m;
                    bill.QtyItem = 1;
                    //bill.SubTotalItem = harga?.TarifTotal ?? 0m;
                    bill.JenisBilling = "Visit Dokter";
                    bill.BillingDate = DateTime.UtcNow;
                    bill.Keterangan = "Biaya Visit Dokter ID";

                    bill.UpdateBy = userActiveId; // pastikan kolom ini ada
                    bill.UpdateDateTime = DateTimeOffset.UtcNow; // pastikan kolom ini ada
                }
                else
                {
                    // Kalau kamu MAU: bila billing belum ada, buat baru (opsional)
                    // Kalau tidak mau, hapus blok ini.

                    int billingCount = await _applicationDbContext.Billings
                        .CountAsync(b =>
                            b.KunjunganId == vm.KunjunganId &&
                            b.JenisBilling != null &&
                            b.JenisBilling.ToLower() == "visit dokter" &&
                            b.IsDelete != true
                        );

                    int billingIndex = billingCount + 1;

                    var newBill = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = vm.KunjunganId,
                        ItemId = data.VisitDokterId,
                        InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                            (Guid)vm.KunjunganId,
                            DateTime.UtcNow),
                        IsListWhiteOff = false,
                        NamaItem = $"Visit Dokter : {dr ?? null}",
                        //HargaItem = harga?.TarifTotal ?? 0m,
                        QtyItem = 1,
                        //SubTotalItem = harga?.TarifTotal ?? 0m,
                        BillingKode = $"{billingIndex:D3}",
                        JenisBilling = "Visit Dokter",
                        StatusBilling = false,
                        BillingDate = DateTime.UtcNow,
                        //Keterangan = "Biaya Visit Dokter ID",
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    };

                    _applicationDbContext.Billings.Add(newBill);
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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
