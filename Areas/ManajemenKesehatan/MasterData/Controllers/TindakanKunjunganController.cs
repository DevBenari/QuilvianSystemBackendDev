using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Tindakan.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TindakanKunjunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<TindakanKunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAsuransiCoverageService _asuransiCoverageService;

        public TindakanKunjunganController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanKunjunganController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IAsuransiCoverageService asuransiCoverageService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _asuransiCoverageService = asuransiCoverageService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAlL(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from a in _applicationDbContext.TindakanKunjungans
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId

                        // join ke table tindakan
                        join t in _applicationDbContext.Tindakans.AsNoTracking()
                        on a.TindakanId equals t.TindakanId into tindakanGroup
                        from t in tindakanGroup.DefaultIfEmpty()

                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.TindakanKunjunganId,
                            a.KunjunganId,
                            a.DepartementId,
                            a.DokterPemeriksaId,
                            a.KelasId,
                            a.TindakanId,
                            t.NamaTindakan,
                            a.Quantity,
                            a.Total,
                            a.TanggalPemeriksaan,
                            a.Disposition,
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
            var listdata = _applicationDbContext.TindakanKunjungans.Find(id);
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
        public async Task<IActionResult> CreateTindakanKunjungan([FromBody] TindakanKunjunganViewModel vm, CancellationToken ct)
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

                // **Ambil Kunjungan berdasarkan KunjunganId dari ViewModel**
                var kunjungan = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(k => k.KunjunganID == vm.KunjunganId);

                if (kunjungan == null)
                {
                    return NotFound(new { message = "Kunjungan tidak ditemukan." });
                }

                // **Tentukan Kelas berdasarkan JenisKunjungan**
                string kelasKode = "";
                if (kunjungan.JenisKunjungan == "OP")  // Jenis Kunjungan adalah rawat jalan
                {
                    kelasKode = "OP";  // Kode kelas untuk rawat jalan
                }
                else if (kunjungan.JenisKunjungan == "IP")  // Jenis Kunjungan adalah rawat inap
                {
                    kelasKode = "IP";  // Kode kelas untuk rawat inap
                }

                // Cari kelas berdasarkan kode kelas
                var kelas = await _applicationDbContext.Kelass
                    .FirstOrDefaultAsync(k => k.KodeKelas == kelasKode);

                if (kelas == null)
                {
                    return NotFound(new { message = "Kelas untuk jenis kunjungan ini tidak ditemukan." });
                }

                // **Ambil Tarif berdasarkan TindakanId dan KelasId**
                var tarifKelas = await _applicationDbContext.TarifKelass
                    .FirstOrDefaultAsync(t => t.TindakanId == vm.TindakanId && t.KelasId == kelas.KelasId);

                if (tarifKelas == null)
                {
                    return NotFound(new { message = "Tarif untuk tindakan dan kelas ini tidak ditemukan." });
                }

                // **Hitung Total berdasarkan TarifTotal dari TarifKelas dan Quantity**
                var totalqty = tarifKelas.TarifTotal.HasValue
                    ? tarifKelas.TarifTotal.Value * vm.Quantity  // Mengalikan tarif total dengan jumlah Quantity
                    : 0; // Jika TarifTotal tidak ada, set total menjadi 0

                // **Buat Data Baru**
                var data = new TindakanKunjungan
                {
                    TindakanKunjunganId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    TindakanId = vm.TindakanId,
                    DepartementId = vm.DepartementId,
                    DokterPemeriksaId = vm.DokterPemeriksaId,
                    KelasId = vm.KelasId,
                    Quantity = vm.Quantity,
                    Total = totalqty, // Masukkan nilai Total yang telah dihitung
                    RanapId = vm.RanapId,
                    TanggalPemeriksaan = vm.TanggalPemeriksaan,
                    Keterangan = vm.Keterangan,
                    Disposition = vm.Disposition,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // **Simpan ke Database**
                _applicationDbContext.TindakanKunjungans.Add(data);

                // cari data tentang tindakan Id
                var tindakan = await _applicationDbContext.Tindakans
                    .FirstOrDefaultAsync(t => t.TindakanId == vm.TindakanId);

                if (tindakan == null)
                {
                    return NotFound(new { message = "Data tindakan tidak ditemukan." });
                }

                var status = await _asuransiCoverageService.GetIsCoveredAsync((Guid)vm.KunjunganId, "Tindakan", vm.TindakanId, ct);

                // Hitung jumlah billing sebelumnya untuk kunjungan ini
                int billingTindakanCount = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == vm.KunjunganId && b.JenisBilling.ToLower()=="tindakan")
                    .CountAsync();
                int billingIndex = billingTindakanCount;

                // buat BillingKode untuk setiap tindakan
                billingIndex++;
                string billingKode = $"{billingIndex.ToString("D3")}";

                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    BillingDate = DateTime.UtcNow,
                    BillingKode = billingKode,
                    DiskonId = vm.DiskonId,
                    ItemId = vm.TindakanId,
                    InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                    IsListWhiteOff = false,
                    NamaItem = tindakan.NamaTindakan,
                    QtyItem = vm.Quantity,
                    HargaItem = tarifKelas.TarifTotal,
                    SubTotalItem = totalqty,
                    JenisBilling = "Tindakan", // Menandakan ini adalah billing untuk tindakan
                    StatusBilling= false,
                    IsCovered = status,
                    TanggalInvoice = DateTime.UtcNow,
                    TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    Keterangan = vm.Keterangan,
                };

                _applicationDbContext.Billings.Add(billing);

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
        public async Task<IActionResult> UpdateTindakanKunjungan(Guid id, [FromBody] TindakanKunjunganViewModel vm, CancellationToken ct)
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
                var data = await _applicationDbContext.TindakanKunjungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Ambil Kunjungan berdasarkan KunjunganId dari ViewModel**
                var kunjungan = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(k => k.KunjunganID == vm.KunjunganId);

                if (kunjungan == null)
                {
                    return NotFound(new { message = "Kunjungan tidak ditemukan." });
                }

                // Cari kelas berdasarkan kode kelas
                var kelas = await _applicationDbContext.Kelass
                    .FirstOrDefaultAsync(k => k.KodeKelas == kunjungan.JenisKunjungan);

                if (kelas == null)
                {
                    return NotFound(new { message = "Kelas untuk jenis kunjungan ini tidak ditemukan." });
                }

                // **Ambil Tarif berdasarkan TindakanId dan KelasId**
                var tarifKelas = await _applicationDbContext.TarifKelass
                    .FirstOrDefaultAsync(t => t.TindakanId == vm.TindakanId && t.KelasId == kelas.KelasId);

                if (tarifKelas == null)
                {
                    return NotFound(new { message = "Tarif untuk tindakan dan kelas ini tidak ditemukan." });
                }

                // **Hitung Total berdasarkan TarifTotal dari TarifKelas dan Quantity**
                var totalqty = tarifKelas.TarifTotal.HasValue
                    ? tarifKelas.TarifTotal.Value * vm.Quantity  // Mengalikan tarif total dengan jumlah Quantity
                    : 0; // Jika TarifTotal tidak ada, set total menjadi 0


                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.TindakanId = vm.TindakanId;
                data.KelasId = vm.KelasId;
                data.DokterPemeriksaId = vm.DokterPemeriksaId;
                data.DepartementId = vm.DepartementId;
                data.Quantity = vm.Quantity;
                data.Total = totalqty;
                data.RanapId = vm.RanapId;
                data.TanggalPemeriksaan = vm.TanggalPemeriksaan;
                data.Disposition = vm.Disposition;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.TindakanKunjungans.Update(data);

                // Cek tindakan sebagai item billing
                var tindakan = await _applicationDbContext.Tindakans
                    .FirstOrDefaultAsync(t => t.TindakanId == vm.TindakanId);
                if (tindakan == null)
                    return NotFound(new { message = "Data tindakan tidak ditemukan." });

                var existingBilling = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b => b.KunjunganId == vm.KunjunganId && b.ItemId == vm.TindakanId );

                if (existingBilling == null)
                {
                    // Hitung jumlah billing sebelumnya untuk kunjungan ini
                    int billingTindakanCount = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == vm.KunjunganId && b.BillingKode.ToLower()=="tindakan")
                        .CountAsync();
                    int billingIndex = billingTindakanCount;

                    // buat BillingKode untuk setiap tindakan
                    billingIndex++;
                    string billingKode = $"{billingIndex.ToString("D3")}";

                    var status = await _asuransiCoverageService.GetIsCoveredAsync((Guid)vm.KunjunganId, "Tindakan", vm.TindakanId, ct);

                    var newBilling = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = vm.KunjunganId,
                        BillingDate = DateTime.UtcNow,
                        BillingKode = billingKode,
                        DiskonId = vm.DiskonId,
                        ItemId = vm.TindakanId,
                        InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                        IsListWhiteOff = false,
                        NamaItem = tindakan.NamaTindakan,
                        HargaItem = tarifKelas.TarifTotal,
                        QtyItem = vm.Quantity,
                        SubTotalItem = totalqty,
                        Keterangan = vm.Keterangan,
                        JenisBilling = "Tindakan",
                        StatusPengambilan = true,
                        StatusBilling = false,
                        IsCovered = status,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    };

                    _applicationDbContext.Billings.Add(newBilling);
                }
                else
                {
                    existingBilling.HargaItem = tarifKelas.TarifTotal;
                    existingBilling.SubTotalItem = totalqty;
                    existingBilling.QtyItem = vm.Quantity;
                    existingBilling.Keterangan = vm.Keterangan;
                    existingBilling.DiskonId = vm.DiskonId;
                    existingBilling.UpdateBy = userActiveId;
                    existingBilling.UpdateDateTime = DateTimeOffset.UtcNow;

                    _applicationDbContext.Billings.Update(existingBilling);
                }
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
        public async Task<IActionResult> DeleteTindakanKunjungan(Guid id)
        {
            try
            {
                // Autentikasi user dari JWT
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

                // Ambil tindakan kunjungan
                var tindakan = await _applicationDbContext.TindakanKunjungans
                    .FirstOrDefaultAsync(tk => tk.TindakanKunjunganId == id && tk.IsDelete == false);

                if (tindakan == null)
                {
                    return NotFound(new { message = "Tindakan kunjungan tidak ditemukan atau sudah dihapus." });
                }

                // Soft delete tindakan kunjungan
                tindakan.IsDelete = true;
                tindakan.DeleteBy = userActiveId;
                tindakan.DeleteDateTime = DateTimeOffset.UtcNow;

                // Soft delete billing yang terkait (jika ada)
                var billing = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b =>
                        b.KunjunganId == tindakan.KunjunganId &&
                        b.ItemId == tindakan.TindakanId &&
                        b.IsDelete == false );

                if (billing != null)
                {
                    billing.IsDelete = true;
                    billing.DeleteBy = userActiveId;
                    billing.DeleteDateTime = DateTimeOffset.UtcNow;
                }

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Tindakan kunjungan dan billing berhasil dihapus (soft delete)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ======================================================
                // 1) BASE QUERY (FILTER DULU sebelum JOIN) ✅
                // ======================================================
                var baseQuery = _applicationDbContext.TindakanKunjungans
                    .AsNoTracking()
                    .Where(a => a.IsDelete == false);

                // filter based on kunjungan id
                if (kunjunganId.HasValue)
                {
                    baseQuery = baseQuery.Where(a => a.KunjunganId == kunjunganId.Value);
                }

                // filter based on pasien id (via Kunjungan)
                if (pasienId.HasValue)
                {
                    var pid = pasienId.Value;

                    baseQuery = baseQuery.Where(a =>
                        _applicationDbContext.Kunjungans.Any(k =>
                            k.KunjunganID == a.KunjunganId &&
                            k.PasienId == pid));
                }

                // Filter berdasarkan tanggal
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    baseQuery = baseQuery.Where(a =>
                        a.CreateDateTime >= startUtc &&
                        a.CreateDateTime <= endUtc);
                }

                // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Date == today);
                            break;

                        case PeriodeFilter.ThisWeek:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                                a.CreateDateTime.Date <= today
                            );
                            break;

                        case PeriodeFilter.LastWeek:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                a.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                            );
                            break;

                        case PeriodeFilter.ThisMonth:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Month == today.Month &&
                                a.CreateDateTime.Year == today.Year
                            );
                            break;

                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Month == lastMonth.Month &&
                                a.CreateDateTime.Year == lastMonth.Year
                            );
                            break;

                        case PeriodeFilter.ThisYear:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastYear:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Year == today.Year - 1);
                            break;

                        case PeriodeFilter.Last3Months:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime >= today.AddMonths(-3));
                            break;

                        case PeriodeFilter.Last6Months:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime >= today.AddMonths(-6));
                            break;
                    }
                }

                // ======================================================
                // 2) JOIN QUERY (baru join setelah filter) ✅
                // ======================================================
                var query =
                    from a in baseQuery
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId

                    // join ke table tindakan (LEFT JOIN)
                    join t in _applicationDbContext.Tindakans.AsNoTracking()
                        on a.TindakanId equals t.TindakanId into tindakanGroup
                    from t in tindakanGroup.DefaultIfEmpty()

                        // join ke table kunjungan (LEFT JOIN)
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on a.KunjunganId equals k.KunjunganID into kunjunganGroup
                    from k in kunjunganGroup.DefaultIfEmpty()

                        // join ke table asuransi (LEFT JOIN)
                    join ar in _applicationDbContext.Asuransis.AsNoTracking()
                        on k.AsuransiId equals ar.AsuransiId into arGroup
                    from ar in arGroup.DefaultIfEmpty()

                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        a.TindakanKunjunganId,
                        a.KunjunganId,
                        a.Disposition,
                        k.PasienId,
                        JenisKunjungan = k != null ? k.JenisKunjungan : null,
                        AsuransiId = k != null ? k.AsuransiId : null,
                        NamaAsuransi = ar != null ? ar.NamaAsuransi : null,
                        a.DepartementId,
                        a.DokterPemeriksaId,
                        a.KelasId,
                        a.TindakanId,
                        NamaTindakan = t != null ? t.NamaTindakan : null,
                        a.Quantity,
                        a.Total,
                        a.TanggalPemeriksaan,
                        a.Keterangan,
                    };

                // ======================================================
                // 3) SORTING ✅ (PERBAIKAN: ASC vs DESC)
                // ======================================================
                bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

                query = desc
                    ? orderBy switch
                    {
                        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                        "NamaTindakan" => query.OrderByDescending(u => u.NamaTindakan),
                        _ => query.OrderByDescending(u => u.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                        "CreateByName" => query.OrderBy(u => u.CreateByName),
                        "NamaTindakan" => query.OrderBy(u => u.NamaTindakan),
                        _ => query.OrderBy(u => u.CreateDateTime)
                    };

                // ======================================================
                // 4) PAGINATION ✅ (ASYNC)
                // ======================================================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                if (totalRows == 0)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                if (page > totalPages)
                {
                    return NotFound(new { message = "Page not found." });
                }

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                // ======================================================
                // 5) RETURN RESPONSE ✅
                // ======================================================
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
