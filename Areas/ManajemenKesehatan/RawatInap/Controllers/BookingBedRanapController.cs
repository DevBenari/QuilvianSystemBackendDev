using System.Globalization;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
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
    [EnableCors("FrontendCorsPolicy")]
    public class BookingBedRanapController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<BookingBedRanapController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAsuransiCoverageService _asuransiCoverageService;


        public BookingBedRanapController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BookingBedRanapController> logger,
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
            var query = (from a in _applicationDbContext.BookingBedRanaps
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BookingBedRanapId,
                             a.KunjunganId,
                             a.KamarId,
                             a.BedId,
                             a.TglMasuk,
                             a.TglKeluar,
                             a.StatusBed,
                             a.NoKamar,
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
            var listdata = _applicationDbContext.BookingBedRanaps.Find(id);
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
        public async Task<IActionResult> Create([FromBody] BookingBedRanapViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.KunjunganId == null || vm.KamarId == null || vm.BedId == null)
                return BadRequest(new { message = "KunjunganId, KamarId, dan BedId wajib diisi." });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // cek koneksi db
                if (!_applicationDbContext.Database.CanConnect())
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

                // validasi tanggal masuk
                var parsedTglMasukRanap = TryParseTanggalToUtc(vm.TglMasuk);
                if (parsedTglMasukRanap == null)
                    return BadRequest(new { message = "Format tanggal masuk ranap tidak valid! Gunakan yyyy-MM-dd." });

                // validasi kunjungan ada
                var kunjunganExist = await _applicationDbContext.Kunjungans
                    .AnyAsync(k => k.KunjunganID == vm.KunjunganId && k.IsDelete != true);

                if (!kunjunganExist)
                    return NotFound(new { message = "Kunjungan tidak ditemukan." });

                // validasi kamar + ambil tarif & kelas
                var kamar = await _applicationDbContext.Kamars
                    .Where(k => k.KamarId == vm.KamarId && k.IsDelete != true)
                    .Select(k => new
                    {
                        k.KamarId,
                        k.KelasId,
                        k.KodeKamar,
                        k.NamaKamar,
                        k.TarifHarian
                    })
                    .FirstOrDefaultAsync();

                if (kamar == null)
                    return NotFound(new { message = "Data kamar tidak ditemukan." });

                if (kamar.TarifHarian == null || kamar.TarifHarian <= 0)
                    return BadRequest(new { message = "TarifHarian kamar belum di-set / tidak valid." });

                // cek bed tersedia
                var bed = await _applicationDbContext.Beds
                    .FirstOrDefaultAsync(b => b.BedId == vm.BedId && b.IsDelete != true);

                if (bed == null)
                    return NotFound(new { message = "Data bed tidak ditemukan." });

                if (bed.Status == true)
                    return BadRequest(new { message = "Bed sudah terpakai / tidak tersedia." });

                // validasi kunjungan belum punya booking aktif
                var existingBooking = await _applicationDbContext.BookingBedRanaps
                    .FirstOrDefaultAsync(b =>
                        b.KunjunganId == vm.KunjunganId &&
                        b.TglKeluar == null &&
                        b.IsDelete != true);

                if (existingBooking != null)
                    return BadRequest(new { message = "Kunjungan ini sudah memiliki booking bed." });

                // lock bed
                bed.Status = true;

                // insert booking
                var bookingId = Guid.NewGuid();

                var data = new BookingBedRanap
                {
                    BookingBedRanapId = bookingId,
                    KunjunganId = vm.KunjunganId,
                    KamarId = vm.KamarId,
                    BedId = vm.BedId,
                    TglMasuk = parsedTglMasukRanap,
                    NoKamar = vm.NoKamar,
                    StatusBed = true,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.BookingBedRanaps.Add(data);

                // =========================
                // ✅ BILLING KAMAR RANAP
                // =========================
                const string jenisBilling = "Kamar Ranap";

                // billing kode incremental per kunjungan untuk jenis ini
                var billingCount = await _applicationDbContext.Billings
                    .Where(b =>
                        b.KunjunganId == vm.KunjunganId &&
                        b.JenisBilling == jenisBilling &&
                        (b.IsDelete == false || b.IsDelete == null))
                    .CountAsync();

                var billingKode = $"{(billingCount + 1):D3}";

                // qty awal 1 hari (prakiraan hari pertama)
                var qty = 1;
                var harga = kamar.TarifHarian.Value;
                var subtotal = harga * qty;

                var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                    vm.KunjunganId,
                    "Kamar Ranap",
                    kamar.KamarId,
                    ct);

                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    BillingDate = DateTime.UtcNow,
                    BillingKode = billingKode,
                    InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                    IsListWhiteOff = false,
                    // Item kamar
                    ItemId = kamar.KamarId,
                    NamaItem = $"Kamar Ranap - {(kamar.NamaKamar ?? kamar.KodeKamar ?? vm.NoKamar)}",
                    HargaItem = harga,
                    QtyItem = qty,
                    SubTotalItem = subtotal,
                    StatusBilling = false,
                    JenisBilling = jenisBilling,
                    TanggalInvoice = DateTime.UtcNow,
                    TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                    IsCovered = coverage?.IsCovered,
                    IsCoveredExcess = coverage?.IsCoveredExcess,
                    AsuransiId = coverage?.AsuransiId,
                    AsuransiExcessId = coverage?.AsuransiExcessId,

                    Keterangan = $"BookingBedRanapId={bookingId};Start={parsedTglMasukRanap:yyyy-MM-dd}",

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.Billings.Add(billing);

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Created("", new
                {
                    message = "Booking bed ranap + billing kamar harian berhasil dibuat || 201 Created",
                    bookingBedRanapId = bookingId,
                    kamarId = kamar.KamarId,
                    kelasId = kamar.KelasId,
                    tarifHarian = harga,
                    billingId = billing.BillingId,
                    billingKode = billingKode,
                    qtyAwal = qty,
                    subtotalAwal = subtotal
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] BookingBedRanapViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        // Validasi tanggal masuk ranap
        //        var parsedTglMasukRanap = TryParseTanggalToUtc(vm.TglMasuk);
        //        if (parsedTglMasukRanap == null)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "Format tanngal masuk ranap tidak valid! Gunakan format yyyy-MM-dd."
        //            });
        //        }

        //        // cek data bed yang tersedia
        //        var dataBed = _applicationDbContext.Beds
        //            .FirstOrDefault(b => b.BedId == vm.BedId);
        //        if (dataBed == null)
        //        {
        //            return NotFound(new { message = "Data bed tidak ditemukan." });
        //        }
        //        else
        //        {
        //            dataBed.Status = true; // Tandai bed sebagai tidak tersedia
        //        }

        //        // validasi kunjunganId yang sama tidak boleh membuat booking bed lagi
        //        var existingBooking = _applicationDbContext.BookingBedRanaps
        //            .FirstOrDefault(b => b.KunjunganId == vm.KunjunganId && b.TglKeluar == null);

        //        if (existingBooking != null)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "Kunjungan ini sudah memiliki booking bed."
        //            });
        //        }

        //        // **Buat Data Baru**
        //        var data = new BookingBedRanap
        //        {
        //            BookingBedRanapId = Guid.NewGuid(),
        //            KunjunganId = vm.KunjunganId,
        //            KamarId = vm.KamarId,
        //            BedId = vm.BedId,
        //            TglMasuk = parsedTglMasukRanap,
        //            //TglKeluar = parsedTglKeluarRanap,
        //            NoKamar = vm.NoKamar,
        //            StatusBed = true,
        //            Keterangan = vm.Keterangan,
        //            CreateBy = userActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //        };

        //        // **Simpan ke Database**
        //        _applicationDbContext.BookingBedRanaps.Add(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpPut("KeluarKamarRawatInap/{id}")]
        public async Task<IActionResult> Keluar(Guid id, [FromBody] KeluarRanapViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            await using var tx = await _applicationDbContext.Database.BeginTransactionAsync();

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
                    .FirstOrDefaultAsync(u => u.Email == emailLogin && u.IsDelete == false);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data Booking Bed**
                var data = await _applicationDbContext.BookingBedRanaps
                    .FirstOrDefaultAsync(x => x.BookingBedRanapId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Validasi tanggal keluar
                var parsedTglKeluar = TryParseTanggalToUtc(vm.TglKeluar);
                if (parsedTglKeluar == null)
                {
                    return BadRequest(new { message = "Format tanggal keluar tidak valid! Gunakan format yyyy-MM-dd." });
                }

                if (data.TglMasuk == null)
                {
                    return BadRequest(new { message = "Tanggal masuk ranap belum terisi. Tidak dapat menghitung biaya kamar." });
                }

                if (parsedTglKeluar.Value < data.TglMasuk.Value)
                {
                    return BadRequest(new { message = "Tanggal keluar tidak boleh lebih kecil dari tanggal masuk." });
                }

                // ✅ Update booking keluar
                data.TglKeluar = parsedTglKeluar;
                data.StatusBed = false;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;
                _applicationDbContext.BookingBedRanaps.Update(data);

                // ✅ Update bed jadi available
                var dataBed = await _applicationDbContext.Beds
                    .FirstOrDefaultAsync(b => b.BedId == data.BedId && b.IsDelete == false);

                if (dataBed == null)
                {
                    return NotFound(new { message = "Data bed tidak ditemukan." });
                }

                dataBed.Status = false;
                _applicationDbContext.Beds.Update(dataBed);

                // =====================================================
                // ✅ HITUNG JUMLAH HARI PEMAKAIAN KAMAR
                // =====================================================
                var durasi = parsedTglKeluar.Value - data.TglMasuk.Value;

                int jumlahHari = (int)Math.Ceiling(durasi.TotalDays);
                if (jumlahHari < 1) jumlahHari = 1;


                // =====================================================
                // ✅ CLOSE BILLING KAMAR RANAP
                // =====================================================
                const string jenisBilling = "Kamar Ranap";
                var bookingIdString = data.BookingBedRanapId.ToString();

                var billing = await _applicationDbContext.Billings
                    .Where(b =>
                        b.KunjunganId == data.KunjunganId &&
                        b.JenisBilling == jenisBilling &&
                        (b.IsDelete == false || b.IsDelete == null) &&
                        b.Keterangan != null &&
                        b.Keterangan.Contains(bookingIdString))
                    .OrderByDescending(b => b.CreateDateTime)
                    .FirstOrDefaultAsync();

                if (billing == null)
                {
                    // Kalau kamu ingin tetap boleh keluar walaupun billing tidak ketemu
                    // bisa ganti jadi return Ok dengan warning.
                    return NotFound(new
                    {
                        message = "Billing kamar ranap tidak ditemukan untuk booking ini. Pastikan billing dibuat saat booking."
                    });
                }

                if (billing.HargaItem == null)
                {
                    return BadRequest(new
                    {
                        message = "HargaItem pada billing kamar ranap kosong. Tidak bisa menghitung subtotal."
                    });
                }

                // ✅ Update billing
                //billing.QtyItem = jumlahHari;
                billing.SubTotalItem = billing.HargaItem.Value * jumlahHari;
                billing.UpdateBy = userActiveId;
                billing.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Billings.Update(billing);

                // =====================================================
                // ✅ SAVE + COMMIT
                // =====================================================
                var result = await _applicationDbContext.SaveChangesAsync();
                await tx.CommitAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Pasien berhasil keluar ranap + billing kamar berhasil di-close dan diperbarui || 200 OK",
                        jumlahHari,
                        hargaHarian = billing.HargaItem,
                        subtotal = billing.SubTotalItem
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (DbUpdateException dbEx)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BookingBedRanapViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // =====================================================
                // 🔹 Ambil User ID dari JWT Claims
                // =====================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin && u.IsDelete == false);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // =====================================================
                // 🔹 Cari data booking (Pastikan tidak deleted)
                // =====================================================
                var data = await _applicationDbContext.BookingBedRanaps
                    .FirstOrDefaultAsync(x => x.BookingBedRanapId == id && x.IsDelete != true);

                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // =====================================================
                // 🔹 Rekomendasi keamanan: block update kalau sudah selesai
                // =====================================================
                if (data.TglKeluar != null)
                    return BadRequest(new { message = "Booking sudah selesai (TglKeluar terisi), tidak bisa diupdate." });

                // =====================================================
                // 🔹 Validasi tanggal masuk
                // =====================================================
                var parsedTglMasukRanap = TryParseTanggalToUtc(vm.TglMasuk);
                if (parsedTglMasukRanap == null)
                {
                    return BadRequest(new
                    {
                        message = "Format tanggal masuk ranap tidak valid! Gunakan format yyyy-MM-dd."
                    });
                }

                // =====================================================
                // 🔹 Validasi kunjunganId tidak boleh punya booking lain aktif
                // =====================================================
                var existingBooking = await _applicationDbContext.BookingBedRanaps
                    .FirstOrDefaultAsync(b =>
                        b.KunjunganId == vm.KunjunganId &&
                        b.TglKeluar == null &&
                        b.IsDelete == false &&
                        b.BookingBedRanapId != id);

                if (existingBooking != null)
                {
                    return BadRequest(new
                    {
                        message = "Kunjungan ini sudah memiliki booking bed."
                    });
                }

                // =====================================================
                // 🔹 Cek apakah BedId berubah
                // =====================================================
                var bedLamaId = data.BedId;
                var bedBaruId = vm.BedId;
                bool bedBerubah = bedBaruId != bedLamaId;

                // =====================================================
                // 🔹 Mulai transaksi
                // =====================================================
                await using var tx = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // =====================================================
                    // 🔹 Update field umum BookingBedRanap
                    // =====================================================
                    data.KunjunganId = vm.KunjunganId;
                    data.KamarId = vm.KamarId;
                    data.TglMasuk = parsedTglMasukRanap;
                    data.NoKamar = vm.NoKamar;
                    data.Keterangan = vm.Keterangan;
                    data.UpdateBy = userActiveId;
                    data.UpdateDateTime = DateTimeOffset.UtcNow;

                    // =====================================================
                    // 🔹 Jika bed berubah → unlock old bed + lock new bed
                    // =====================================================
                    if (bedBerubah)
                    {
                        Bed? bedLama = null;

                        if (bedLamaId.HasValue)
                        {
                            bedLama = await _applicationDbContext.Beds
                                .FirstOrDefaultAsync(b => b.BedId == bedLamaId.Value && b.IsDelete != true);
                        }

                        var bedBaru = await _applicationDbContext.Beds
                            .FirstOrDefaultAsync(b => b.BedId == bedBaruId && b.IsDelete != true);

                        if (bedBaru == null)
                            return NotFound(new { message = "Bed baru tidak ditemukan." });

                        // ✅ Tolak jika bed baru sudah terisi
                        if (bedBaru.Status == true)
                            return Conflict(new { message = "Bed baru sedang terisi. Pilih bed lain." });

                        // pindahkan bed
                        data.BedId = bedBaruId;
                        data.StatusBed = true;

                        // unlock old bed
                        if (bedLama != null)
                        {
                            bedLama.Status = false;
                        }

                        // lock new bed
                        bedBaru.Status = true;
                    }
                    else
                    {
                        // bed tidak berubah
                        data.StatusBed = true;
                    }

                    // =====================================================
                    // 🔹 Save + Commit
                    // =====================================================
                    var result = await _applicationDbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    if (result > 0)
                        return Ok(new { message = "Update Data Berhasil || 200 OK" });

                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
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


        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(Guid id, [FromBody] BookingBedRanapViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid." });

        //    try
        //    {
        //        if (!await _applicationDbContext.Database.CanConnectAsync())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        // Ambil User ID dari JWT Claims
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });

        //        var getUserActive = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.Email == emailLogin);

        //        if (getUserActive == null)
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //        var userActiveId = getUserActive.UserActiveId;

        //        // Cari data booking
        //        var data = await _applicationDbContext.BookingBedRanaps.FindAsync(id);
        //        if (data == null)
        //            return NotFound(new { message = "Data tidak ditemukan." });

        //        // Validasi tanggal masuk
        //        var parsedTglMasukRanap = TryParseTanggalToUtc(vm.TglMasuk);
        //        if (parsedTglMasukRanap == null)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "Format tanggal masuk ranap tidak valid! Gunakan format yyyy-MM-dd."
        //            });
        //        }

        //        // Validasi kunjunganId yang sama tidak boleh membuat booking bed lagi
        //        var existingBooking = await _applicationDbContext.BookingBedRanaps
        //            .FirstOrDefaultAsync(b => b.KunjunganId == vm.KunjunganId && b.IsDelete == false && b.BookingBedRanapId != id);
        //        if (existingBooking != null)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "Kunjungan ini sudah memiliki booking bed."
        //            });
        //        }

        //        // Cek apakah BedId berubah
        //        var bedLamaId = data.BedId;
        //        var bedBaruId = vm.BedId;
        //        bool bedBerubah = bedBaruId != bedLamaId;

        //        // Mulai transaksi agar konsisten
        //        await using var tx = await _applicationDbContext.Database.BeginTransactionAsync();
        //        try
        //        {
        //            // Update field umum BookingBedRanap
        //            data.KunjunganId = vm.KunjunganId;
        //            data.KamarId = vm.KamarId;
        //            data.TglMasuk = parsedTglMasukRanap;
        //            data.NoKamar = vm.NoKamar;
        //            data.Keterangan = vm.Keterangan;
        //            data.UpdateBy = userActiveId;
        //            data.UpdateDateTime = DateTimeOffset.UtcNow;

        //            if (bedBerubah)
        //            {
        //                // Ambil bed lama & baru
        //                Bed? bedLama = null;
        //                if (bedLamaId.HasValue)
        //                {
        //                    bedLama = await _applicationDbContext.Beds
        //                        .FirstOrDefaultAsync(b => b.BedId == bedLamaId.Value);
        //                }

        //                var bedBaru = await _applicationDbContext.Beds
        //                    .FirstOrDefaultAsync(b => b.BedId == bedBaruId);

        //                if (bedBaru == null)
        //                    return NotFound(new { message = "Bed baru tidak ditemukan." });

        //                // Opsional: tolak jika bed baru sudah terisi
        //                if (bedBaru.Status == true)
        //                    return Conflict(new { message = "Bed baru sedang terisi. Pilih bed lain." });

        //                // Pindahkan bed di booking
        //                data.BedId = bedBaruId;
        //                // sinkronkan status aktif di booking jika diperlukan
        //                data.StatusBed = true;

        //                // Ubah status di tabel Beds (atomik dalam transaksi)
        //                if (bedLama != null)
        //                {
        //                    bedLama.Status = false;
        //                    _applicationDbContext.Beds.Update(bedLama);
        //                }

        //                bedBaru.Status = true;
        //                _applicationDbContext.Beds.Update(bedBaru);
        //            }
        //            else
        //            {
        //                // Jika bed tidak berubah dan kamu ingin mengikuti flag dari VM:
        //                data.StatusBed = true;
        //            }

        //            _applicationDbContext.BookingBedRanaps.Update(data);

        //            var result = await _applicationDbContext.SaveChangesAsync();
        //            await tx.CommitAsync();

        //            if (result > 0)
        //                return Ok(new { message = "Update Data Berhasil || 200 OK" });

        //            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        //        }
        //        catch
        //        {
        //            await tx.RollbackAsync();
        //            throw;
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

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
                var data = await _applicationDbContext.BookingBedRanaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // cari data bed dalam tabel beds
                var dataBed = await _applicationDbContext.Beds
                    .FirstOrDefaultAsync(b => b.BedId == data.BedId);
                if (dataBed == null)
                {
                    return NotFound(new { message = "Data bed tidak ditemukan." });
                }
                else
                {
                    dataBed.Status = false; // Tandai bed sebagai tidak tersedia
                    _applicationDbContext.Beds.Update(dataBed);
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.BookingBedRanaps.Update(data);
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
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.BookingBedRanaps
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BookingBedRanapId,
                             a.KunjunganId,
                             a.KamarId,
                             a.BedId,
                             a.TglMasuk,
                             a.TglKeluar,
                             a.StatusBed,
                             a.NoKamar,
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
