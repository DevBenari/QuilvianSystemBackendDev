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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class RuangBedahBookingDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<RuangBedahBookingDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RuangBedahBookingDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RuangBedahBookingDetailController> logger,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.RuangBedahBookingDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailBookingBedahId,
                             a.BookingRuanganBedahId,
                             a.JenisOperasiId,
                             a.TindakanId,
                             TeamOP = a.UserActiveId, // List<Guid>
                                             // Optional: tampilkan nama user jika ingin decode (lihat catatan bawah)
                             a.PersentaseTindakan,
                             a.DiskonDokter,
                             a.Keterangan
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
            var listdata = _applicationDbContext.RuangBedahBookingDetails.Find(id);
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
        public async Task<IActionResult> Create([FromBody] RuangBedahBookingDetailVM vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // cek koneksi
                if (!await _applicationDbContext.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // validasi tindakan
                if (vm.TindakanId == null || !vm.TindakanId.Any())
                    return BadRequest(new { message = "TindakanId wajib diisi minimal 1 item." });

                // ambil parent booking untuk mendapatkan KunjunganId dan KelasId
                var parent = await _applicationDbContext.RuangBedahBookings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.BookingRuanganBedahId == vm.BookingRuanganBedahId);

                if (parent == null)
                    return NotFound(new { message = "Parent RuangBedahBooking tidak ditemukan." });

                if (!parent.KunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId pada parent booking tidak ditemukan." });

                if (!parent.KelasId.HasValue)
                    return BadRequest(new { message = "KelasId pada parent booking tidak ditemukan." });

                var kunjunganId = parent.KunjunganId.Value;
                var kelasId = parent.KelasId.Value;

                // siapkan data master tindakan
                var tindakanIds = vm.TindakanId.Distinct().ToList();

                var tindakanDict = await _applicationDbContext.Tindakans
                    .Where(t => tindakanIds.Contains(t.TindakanId))
                    .Select(t => new { t.TindakanId, t.NamaTindakan })
                    .ToDictionaryAsync(x => x.TindakanId, x => x.NamaTindakan);

                var tarifDict = await _applicationDbContext.TarifKelass
                    .Where(tk => tk.TindakanId != null
                                 && tindakanIds.Contains(tk.TindakanId.Value)
                                 && tk.KelasId == kelasId)
                    .Select(tk => new
                    {
                        TindakanId = tk.TindakanId!.Value,
                        tk.TarifTotal
                    })
                    .ToDictionaryAsync(x => x.TindakanId, x => x.TarifTotal);

                // buat detail booking
                var detailId = Guid.NewGuid();

                var data = new RuangBedahBookingDetail
                {
                    DetailBookingBedahId = detailId,
                    BookingRuanganBedahId = vm.BookingRuanganBedahId,
                    JenisOperasiId = vm.JenisOperasiId,
                    TindakanId = vm.TindakanId,
                    UserActiveId = vm.UserActiveId ?? new List<Guid>(),
                    PersentaseTindakan = vm.PersentaseTindakan,
                    DiskonDokter = vm.DiskonDokter,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                await _applicationDbContext.RuangBedahBookingDetails.AddAsync(data);

                // billing index per kunjungan + jenis
                var jenisBillingOperasi = "Tindakan";

                int billingIndex = await _applicationDbContext.Billings
                    .CountAsync(b =>
                        b.KunjunganId == kunjunganId &&
                        (b.IsDelete == false || b.IsDelete == null) &&
                        b.JenisBilling.ToLower() == jenisBillingOperasi.ToLower());

                // ambil / buat invoice sekali saja
                var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId,
                    DateTime.UtcNow
                );

                var billingList = new List<Billing>();

                foreach (var tindakanId in tindakanIds)
                {
                    tindakanDict.TryGetValue(tindakanId, out var namaTindakan);
                    namaTindakan ??= "Operasi";

                    if (!tarifDict.TryGetValue(tindakanId, out var tarifTotal) || tarifTotal == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new
                        {
                            message = $"Tarif tidak ditemukan untuk TindakanId={tindakanId} pada KelasId={kelasId}."
                        });
                    }

                    var qty = 1;
                    var subtotal = tarifTotal.Value * qty;

                    if (vm.DiskonDokter.HasValue && vm.DiskonDokter.Value > 0)
                    {
                        var disc = vm.DiskonDokter.Value;
                        if (disc > 100) disc = 100;
                        subtotal = subtotal - (subtotal * (disc / 100m));
                    }

                    billingIndex++;
                    string billingKode = $"{billingIndex:D3}";

                    billingList.Add(new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = kunjunganId,
                        BillingDate = DateTime.UtcNow,
                        BillingKode = billingKode,
                        ItemId = tindakanId,
                        NamaItem = namaTindakan,
                        InvoiceBilling = invoice,
                        IsListWhiteOff = false,
                        IsDelete = false,

                        QtyItem = qty,
                        HargaItem = tarifTotal,
                        SubTotalItem = subtotal,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        JenisBilling = jenisBillingOperasi,
                        Keterangan = vm.Keterangan,
                        StatusBilling = false,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });
                }

                if (billingList.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Billing tidak terbentuk dari TindakanId yang dikirim." });
                }

                await _applicationDbContext.Billings.AddRangeAsync(billingList);

                var result = await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Created("", new
                {
                    message = "Tambah detail ruang bedah + billing berhasil.",
                    detailBookingBedahId = detailId,
                    kunjunganId = kunjunganId,
                    jumlahBilling = billingList.Count,
                    affectedRows = result
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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
        public async Task<IActionResult> Update(Guid id, [FromBody] RuangBedahBookingDetailVM vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // cek koneksi
                if (!await _applicationDbContext.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // auth
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ambil detail lama
                var existingDetail = await _applicationDbContext.RuangBedahBookingDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetailBookingBedahId == id &&
                        (x.IsDelete == false || x.IsDelete == null));

                if (existingDetail == null)
                    return NotFound(new { message = "Detail booking ruang bedah tidak ditemukan." });

                // tentukan parent booking
                var bookingRuanganBedahId = vm.BookingRuanganBedahId != Guid.Empty
                    ? vm.BookingRuanganBedahId
                    : existingDetail.BookingRuanganBedahId;

                var parent = await _applicationDbContext.RuangBedahBookings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.BookingRuanganBedahId == bookingRuanganBedahId &&
                        (x.IsDelete == false || x.IsDelete == null));

                if (parent == null)
                    return NotFound(new { message = "Parent booking ruang bedah tidak ditemukan." });

                if (!parent.KunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId pada parent booking tidak ditemukan." });

                if (!parent.KelasId.HasValue)
                    return BadRequest(new { message = "KelasId pada parent booking tidak ditemukan." });

                var kunjunganId = parent.KunjunganId.Value;
                var kelasId = parent.KelasId.Value;

                // kumpulkan tindakan lama dan baru
                var oldTindakanIds = (existingDetail.TindakanId ?? new List<Guid>())
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                var newTindakanIds = (vm.TindakanId ?? new List<Guid>())
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (!newTindakanIds.Any())
                    return BadRequest(new { message = "TindakanId wajib diisi minimal 1 item." });

                // konsisten dengan Create parent + GET billing
                const string jenisBilling = "Tindakan";

                // soft delete billing lama berdasarkan kunjungan + item tindakan lama
                // catatan: karena tabel Billing tidak menyimpan DetailBookingBedahId,
                // pencocokan terbaik yang bisa dilakukan saat ini adalah via KunjunganId + ItemId.
                if (oldTindakanIds.Any())
                {
                    var oldBillings = await _applicationDbContext.Billings
                        .Where(b =>
                            b.KunjunganId == kunjunganId &&
                            (b.IsDelete == false || b.IsDelete == null) &&
                            b.JenisBilling.ToLower() == jenisBilling.ToLower() &&
                            b.ItemId != null &&
                            oldTindakanIds.Contains(b.ItemId.Value))
                        .ToListAsync();

                    foreach (var ob in oldBillings)
                    {
                        ob.IsDelete = true;
                        ob.UpdateBy = userActiveId;
                        ob.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                }

                // preload master tindakan
                var tindakanDict = await _applicationDbContext.Tindakans
                    .Where(t => newTindakanIds.Contains(t.TindakanId))
                    .Select(t => new { t.TindakanId, t.NamaTindakan })
                    .ToDictionaryAsync(x => x.TindakanId, x => x.NamaTindakan);

                // preload tarif kelas
                var tarifDict = await _applicationDbContext.TarifKelass
                    .Where(tk =>
                        tk.TindakanId != null &&
                        newTindakanIds.Contains(tk.TindakanId.Value) &&
                        tk.KelasId == kelasId)
                    .Select(tk => new
                    {
                        TindakanId = tk.TindakanId!.Value,
                        tk.TarifTotal
                    })
                    .ToDictionaryAsync(x => x.TindakanId, x => x.TarifTotal);

                // update detail
                existingDetail.BookingRuanganBedahId = bookingRuanganBedahId;
                existingDetail.JenisOperasiId = vm.JenisOperasiId;
                existingDetail.TindakanId = vm.TindakanId ?? new List<Guid>();
                existingDetail.UserActiveId = vm.UserActiveId ?? new List<Guid>();
                existingDetail.PersentaseTindakan = vm.PersentaseTindakan;
                existingDetail.DiskonDokter = vm.DiskonDokter;
                existingDetail.Keterangan = vm.Keterangan;
                existingDetail.UpdateBy = userActiveId;
                existingDetail.UpdateDateTime = DateTimeOffset.UtcNow;
                existingDetail.IsDelete = false;

                // hitung index billing aktif
                var currentActiveBillingCount = await _applicationDbContext.Billings
                    .CountAsync(b =>
                        b.KunjunganId == kunjunganId &&
                        (b.IsDelete == false || b.IsDelete == null) &&
                        b.JenisBilling.ToLower() == jenisBilling.ToLower());

                var billingIndex = currentActiveBillingCount - oldTindakanIds.Count;
                if (billingIndex < 0) billingIndex = 0;

                // invoice sekali saja
                var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId,
                    DateTime.UtcNow
                );

                var newBillingList = new List<Billing>();

                foreach (var tindakanId in newTindakanIds)
                {
                    if (!tindakanDict.TryGetValue(tindakanId, out var namaTindakan))
                        return NotFound(new { message = $"Master tindakan tidak ditemukan. TindakanId={tindakanId}" });

                    if (!tarifDict.TryGetValue(tindakanId, out var tarifTotal) || tarifTotal == null)
                        return NotFound(new { message = $"Tarif tidak ditemukan untuk TindakanId={tindakanId}, KelasId={kelasId}" });

                    var qty = 1;
                    var subTotal = tarifTotal.Value * qty;

                    if (vm.DiskonDokter.HasValue && vm.DiskonDokter.Value > 0)
                    {
                        var disc = vm.DiskonDokter.Value;
                        if (disc > 100) disc = 100;
                        subTotal -= (subTotal * (disc / 100m));
                    }

                    billingIndex++;
                    var billingKode = $"{billingIndex:D3}";

                    newBillingList.Add(new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = kunjunganId,
                        BillingDate = DateTime.UtcNow,
                        BillingKode = billingKode,
                        InvoiceBilling = invoice,
                        IsListWhiteOff = false,
                        ItemId = tindakanId,
                        NamaItem = namaTindakan,
                        QtyItem = qty,
                        HargaItem = tarifTotal,
                        SubTotalItem = subTotal,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        JenisBilling = jenisBilling,
                        Keterangan = vm.Keterangan,
                        StatusBilling = false,
                        IsDelete = false,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });
                }

                if (!newBillingList.Any())
                    return BadRequest(new { message = "Billing baru tidak terbentuk." });

                await _applicationDbContext.Billings.AddRangeAsync(newBillingList);

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Update detail booking ruang bedah + billing berhasil",
                    DetailBookingBedahId = id,
                    BookingRuanganBedahId = bookingRuanganBedahId,
                    KunjunganId = kunjunganId,
                    TotalBilling = newBillingList.Count
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
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
                var data = await _applicationDbContext.RuangBedahBookingDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.RuangBedahBookingDetails.Update(data);
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
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
                {

            // Query data
            var query = (from a in _applicationDbContext.RuangBedahBookingDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailBookingBedahId,
                             a.BookingRuanganBedahId,
                             a.JenisOperasiId,
                             a.TindakanId,
                             TeamOP = a.UserActiveId, // List<Guid>
                                                      // Optional: tampilkan nama user jika ingin decode (lihat catatan bawah)
                             a.PersentaseTindakan,
                             a.DiskonDokter,
                             a.Keterangan
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

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
    }
}
