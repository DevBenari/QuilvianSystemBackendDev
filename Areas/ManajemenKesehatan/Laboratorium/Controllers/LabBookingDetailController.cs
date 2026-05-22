using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabBookingDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly string _uploadUrl;
        private readonly IHubContext<LabBookingDetailHub> _hubContext;
        private readonly ITTDService _ttdService;
        private readonly ILogger<LabBookingDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly IAsuransiCoverageService _asuransiCoverageService;

        public LabBookingDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            //IConfiguration configuration,
            ITTDService ttdService,
            IHubContext<LabBookingDetailHub> hubContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IAsuransiCoverageService asuransiCoverageService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            //_uploadUrl = configuration["FileStorage:UploadUrl"];
            _hubContext = hubContext;
            _ttdService = ttdService;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _asuransiCoverageService = asuransiCoverageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from d in _applicationDbContext.LabBookingDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on d.CreateBy equals u.UserActiveId

                         // join lab
                         join l in _applicationDbContext.Labs
                         on d.LabId equals l.LabId into labGroup
                         from l in labGroup.DefaultIfEmpty()


                             // join ke lab booking
                         join b in _applicationDbContext.LabBookings
                         on d.BookingLabId equals b.BookingLabId into labBookings
                         from b in labBookings.DefaultIfEmpty()

                             // joimn ke lab pemeriksaan
                         join p in _applicationDbContext.LabPemeriksaans
                         on d.PemeriksaanLabId equals p.PemeriksaanLabId into labPemeriksaans
                         from p in labPemeriksaans.DefaultIfEmpty()

                         where d.IsDelete == false || d.IsDelete == null
                         select new
                         {
                             d.CreateDateTime,
                             d.CreateBy,
                             CreateByName = u.FullName,
                             d.DetailBookingLabId,
                             d.BookingLabId,
                             d.NoOrder,
                             d.PasienId,
                             b.KunjunganId,
                             d.PemeriksaanLabId,
                             NamaPemeriksaan = p.NamaPemeriksaan ?? "-",
                             HargaPemeriksaan = p.HargaPemeriksaan ?? null,
                             d.LabId,
                             NamaLab = l.NamaLab ?? "-",
                             d.KategoriPatologiAnatomi,
                             d.JenisSpecimen,
                             d.LokasiSpecimen,
                             d.KeteranganKlinik,
                             d.PenyakitSebelumnya,
                             d.PenggunaanFiksasi,
                             d.JenisPemeriksaanGC,
                             d.JenisGC,
                             d.BahanNonGC,
                             d.BahanMicrobiologi,
                             d.MasaHaidTerakhir,
                             d.Diagnosa,
                             d.SpecimenJenisId,
                             d.SpecimenMethodId,
                             d.AsalSpecimenId,
                             d.AlasanPembatalan,
                             d.TTDPembatalanPath,
                             d.StatusPemeriksaan,
                             d.TanggalSelesai,
                             d.StatusVerifikasi,
                             d.Satuan
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
            try
            {
                // ✅ Cek koneksi ke database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Query data lengkap dengan join
                var data = await (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                                 where d.DetailBookingLabId == id && d.IsDelete != true
                                 join u in _applicationDbContext.UserActives.AsNoTracking()
                                     on d.CreateBy equals u.UserActiveId into ug
                                 from u in ug.DefaultIfEmpty()

                                 join l in _applicationDbContext.Labs.AsNoTracking()
                                     on d.LabId equals l.LabId into lg
                                 from l in lg.DefaultIfEmpty()

                                 join b in _applicationDbContext.LabBookings.AsNoTracking()
                                     on d.BookingLabId equals b.BookingLabId into bg
                                 from b in bg.DefaultIfEmpty()

                                 join p in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                                     on d.PemeriksaanLabId equals p.PemeriksaanLabId into pg
                                 from p in pg.DefaultIfEmpty()

                                 join k in _applicationDbContext.Kunjungans.AsNoTracking()
                                     on b.KunjunganId equals k.KunjunganID into kg
                                 from k in kg.DefaultIfEmpty()

                                 join bl in _applicationDbContext.Billings.AsNoTracking()
                                     on d.PemeriksaanLabId equals bl.ItemId into blg
                                 from bl in blg.DefaultIfEmpty()

                                 select new
                                 {
                                     d.CreateDateTime,
                                     d.CreateBy,
                                     CreateByName = u != null ? u.FullName : null,
                                     d.DetailBookingLabId,
                                     d.BookingLabId,
                                     d.PasienId,
                                     d.NoOrder,
                                     d.TipeLayanan,
                                     KunjunganId = b != null ? b.KunjunganId : (Guid?)null,
                                     JenisKunjungan = k != null ? k.JenisKunjungan : null,
                                     d.PemeriksaanLabId,
                                     NamaPemeriksaan = p != null ? (p.NamaPemeriksaan ?? "-") : "-",
                                     HargaPemeriksaan = p != null ? p.HargaPemeriksaan : (decimal?)null,
                                     d.LabId,
                                     NamaLab = l != null ? (l.NamaLab ?? "-") : "-",
                                     bl.BillingId,
                                     bl.StatusBilling,
                                     d.KategoriPatologiAnatomi,
                                     d.JenisSpecimen,
                                     d.LokasiSpecimen,
                                     d.KeteranganKlinik,
                                     d.PenyakitSebelumnya,
                                     d.PenggunaanFiksasi,
                                     d.JenisPemeriksaanGC,
                                     d.JenisGC,
                                     d.BahanNonGC,
                                     d.BahanMicrobiologi,
                                     d.MasaHaidTerakhir,
                                     d.Diagnosa,
                                     d.SpecimenJenisId,
                                     d.SpecimenMethodId,
                                     d.AsalSpecimenId,
                                     d.AlasanPembatalan,
                                     d.TTDPembatalanPath,
                                     d.StatusPemeriksaan,
                                     d.TanggalSelesai,
                                     d.StatusVerifikasi,
                                     d.Satuan,
                                 })
                                  .FirstOrDefaultAsync();

                // ✅ Cek apakah data ditemukan
                if (data == null)
                    return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });

                // ✅ Return sukses
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabBookingDetailViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 🔹 Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==========================================================
                // ✅ Ambil data lab untuk generate NoOrder
                // ==========================================================
                if (vm.LabId == null)
                    return BadRequest(new { message = "LabId wajib diisi untuk menentukan NoOrder." });

                var lab = await _applicationDbContext.Labs.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LabId == vm.LabId);

                if (lab == null)
                    return NotFound(new { message = "Lab dengan ID tersebut tidak ditemukan." });

                var kodeKategori = lab.KodeKategori?.Trim().ToUpper() ?? "UNK";
                string labPrefix = kodeKategori.StartsWith("LAB") && kodeKategori.Length > 3
                    ? kodeKategori.Substring(3, Math.Min(3, kodeKategori.Length - 3))
                    : kodeKategori.Length > 3 ? kodeKategori.Substring(0, 3) : kodeKategori;

                var today = DateTimeOffset.UtcNow.Date;
                var start = today;
                var end = today.AddDays(1);

                var lastOrderToday = await _applicationDbContext.LabBookingDetails
                    .Where(d =>
                        d.CreateDateTime >= start &&
                        d.CreateDateTime < end &&
                        d.NoOrder.StartsWith(labPrefix))
                    .OrderByDescending(d => d.NoOrder)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastOrderToday != null)
                {
                    string lastNo = lastOrderToday.NoOrder;
                    string lastNumStr = lastNo.Substring(lastNo.Length - 4);

                    if (int.TryParse(lastNumStr, out int lastNum))
                        nextNumber = lastNum + 1;
                }

                string newNoOrder = $"{labPrefix}{today:yyyyMMdd}{nextNumber:D4}";


                // ==========================================================
                // ✅ Buat data baru LabBookingDetail
                // ==========================================================
                var data = new LabBookingDetail
                {
                    DetailBookingLabId = Guid.NewGuid(),
                    BookingLabId = vm.BookingLabId,
                    PasienId = vm.PasienId,
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    LabId = vm.LabId,
                    TipeLayanan = vm.TipeLayanan,
                    KategoriPatologiAnatomi = vm.KategoriPatologiAnatomi,
                    JenisSpecimen = vm.JenisSpecimen,
                    LokasiSpecimen = vm.LokasiSpecimen,
                    KeteranganKlinik = vm.KeteranganKlinik,
                    PenyakitSebelumnya = vm.PenyakitSebelumnya,
                    PenggunaanFiksasi = vm.PenggunaanFiksasi,
                    JenisPemeriksaanGC = vm.JenisPemeriksaanGC,
                    JenisGC = vm.JenisGC,
                    BahanNonGC = vm.BahanNonGC,
                    BahanMicrobiologi = vm.BahanMicrobiologi,
                    MasaHaidTerakhir = vm.MasaHaidTerakhir,
                    Diagnosa = vm.Diagnosa,
                    AsalSpecimenId = vm.AsalSpecimenId,
                    SpecimenMethodId = vm.SpecimenMethodId,
                    SpecimenJenisId = vm.SpecimenJenisId,
                    NoOrder = newNoOrder,
                    StatusPemeriksaan = vm.StatusPemeriksaan,
                    StatusVerifikasi = vm.StatusVerifikasi,
                    TanggalSelesai = vm.TanggalSelesai,
                    Satuan = vm.Satuan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.LabBookingDetails.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                // ==========================================================
                // ✅ Tambahkan otomatis ke Billing
                // ==========================================================
                if (vm.PemeriksaanLabId != null && vm.BookingLabId != null)
                {
                    var pemeriksaan = await _applicationDbContext.LabPemeriksaans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PemeriksaanLabId == vm.PemeriksaanLabId);

                    var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                        vm.KunjunganId.Value,
                        "Pemeriksaan Lab",
                        pemeriksaan.PemeriksaanLabId,
                        ct);

                    if (pemeriksaan != null)
                    {
                        var billing = new Billing
                        {
                            BillingId = Guid.NewGuid(),
                            KunjunganId = vm.KunjunganId ?? Guid.Empty, // jika dikirim dari ViewModel
                            ItemId = pemeriksaan.PemeriksaanLabId,
                            NamaItem = pemeriksaan.NamaPemeriksaan,
                            HargaItem = pemeriksaan.HargaPemeriksaan ?? 0,
                            QtyItem = 1,
                            SubTotalItem = pemeriksaan.HargaPemeriksaan ?? 0,
                            InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                            IsListWhiteOff = false,
                            BillingKode = "LAB",
                            JenisBilling = "Pemeriksaan Lab",
                            StatusBilling = false,
                            TipeLayanan = vm.TipeLayanan,
                            BillingDate = DateTime.UtcNow,
                            TanggalInvoice = DateTime.UtcNow,
                            TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                            IsCovered = coverage?.IsCovered,
                            IsCoveredExcess = coverage?.IsCoveredExcess,
                            AsuransiId = coverage?.AsuransiId,
                            AsuransiExcessId = coverage?.AsuransiExcessId,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            Keterangan = $"Booking Lab ({newNoOrder})"
                        };

                        _applicationDbContext.Billings.Add(billing);
                        await _applicationDbContext.SaveChangesAsync();
                    }
                }

                await _hubContext.Clients.All.SendAsync("Lab booking detail created", new
                {
                    Action = "create",
                    Id = data.DetailBookingLabId
                });

                return Created("", new
                {
                    message = "Tambah Data Detail Booking Lab & Billing Berhasil || 201 Created",
                    data = new
                    {
                        data.DetailBookingLabId,
                        data.NoOrder
                    }
                });
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

        [HttpPut("Batal/{id}")]
        public async Task<IActionResult> BatalBooking(Guid id, [FromBody] LabBookingDetailBatalVM vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Data pembatalan tidak valid." });

            var booking = await _applicationDbContext.LabBookingDetails
                .FirstOrDefaultAsync(b => b.DetailBookingLabId == id);

            if (booking == null)
                return NotFound(new { message = "Data booking tidak ditemukan." });

            // 🔐 Ambil user dari JWT Claims
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            // ==================================================
            // ✅ UPDATE DATA BOOKING MENJADI DIBATALKAN
            // ==================================================

            // cek ttd
            var ttd = await _ttdService.CheckTTDAsync(userActiveId);

            booking.AlasanPembatalan = vm.AlasanPembatalan;
            booking.TTDPembatalanPath = ttd.Path;
            booking.UpdateBy = userActiveId;
            booking.UpdateDateTime = DateTimeOffset.UtcNow;

            _applicationDbContext.LabBookingDetails.Update(booking);

            // ==================================================
            // ✅ SOFT DELETE BILLING YANG TERKAIT PEMERIKSAAN LAB
            // ==================================================
            if (booking.PemeriksaanLabId != null)
            {
                var relatedBillings = await _applicationDbContext.Billings
                    .Where(b => b.ItemId == booking.PemeriksaanLabId
                             && b.JenisBilling == "Pemeriksaan Lab"
                             && (b.IsDelete == false || b.IsDelete == null))
                    .ToListAsync();

                if (relatedBillings.Any())
                {
                    foreach (var bill in relatedBillings)
                    {
                        bill.IsDelete = true;
                        bill.UpdateBy = userActiveId;
                        bill.UpdateDateTime = DateTimeOffset.UtcNow;
                    }

                    _applicationDbContext.Billings.UpdateRange(relatedBillings);
                }
            }

            await _applicationDbContext.SaveChangesAsync();

            // ==================================================
            // ✅ RESPONSE
            // ==================================================
            return Ok(new
            {
                message = "Booking lab berhasil dibatalkan dan billing terkait telah dihapus (soft delete).",
                bookingId = booking.DetailBookingLabId,
                alasan = booking.AlasanPembatalan,
                TTDId = ttd.TTDId,
            });
        }

        [HttpPut("Verifikasi-Lab/{id}")]
        public async Task<IActionResult> VerikasiBooking(Guid id, [FromBody] VerifikasiLabViewModel vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Data pembatalan tidak valid." });

            var booking = await _applicationDbContext.LabBookingDetails
                .FirstOrDefaultAsync(b => b.DetailBookingLabId == id);

            if (booking == null)
                return NotFound(new { message = "Data booking tidak ditemukan." });

            // 🔐 Ambil user dari JWT Claims
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            // ==================================================
            // ✅ UPDATE DATA BOOKING MENJADI TERVERIFIKASI
            // ==================================================

            booking.StatusVerifikasi = vm.Status;
            booking.VerifikatorId = vm.VerifkatorId;
            booking.WaktuVerifikasi = DateTime.UtcNow;

            booking.UpdateBy = userActiveId;
            booking.UpdateDateTime = DateTimeOffset.UtcNow;

            _applicationDbContext.LabBookingDetails.Update(booking);

            await _applicationDbContext.SaveChangesAsync();

            // ==================================================
            // ✅ RESPONSE
            // ==================================================
            return Ok(new
            {
                message = "Booking lab berhasil diverifikasi",
                bookingId = booking.DetailBookingLabId,
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingDetailViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ==========================================================
                // 🔐 Validasi koneksi database dan user login
                // ==========================================================
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==========================================================
                // 🔍 Cari data detail booking berdasarkan ID
                // ==========================================================
                var existingData = await _applicationDbContext.LabBookingDetails
                    .FirstOrDefaultAsync(d => d.DetailBookingLabId == id);

                if (existingData == null)
                    return NotFound(new { message = "Data detail booking lab tidak ditemukan." });

                // ==========================================================
                // ✅ Generate NoOrder jika LabId berubah
                // ==========================================================
                string newNoOrder = existingData.NoOrder; // default: tetap
                if (vm.LabId != existingData.LabId && vm.LabId != null)
                {
                    var lab = await _applicationDbContext.Labs.AsNoTracking()
                        .FirstOrDefaultAsync(l => l.LabId == vm.LabId);

                    if (lab == null)
                        return NotFound(new { message = "Lab dengan ID tersebut tidak ditemukan." });

                    var kodeKategori = lab.KodeKategori?.Trim().ToUpper() ?? "UNK";
                    string labPrefix = kodeKategori.StartsWith("LAB") && kodeKategori.Length > 3
                        ? kodeKategori.Substring(3, Math.Min(3, kodeKategori.Length - 3))
                        : kodeKategori.Length > 3 ? kodeKategori.Substring(0, 3) : kodeKategori;

                    var today = DateTimeOffset.UtcNow.Date;
                    var start = today;
                    var end = today.AddDays(1);

                    var lastOrderToday = await _applicationDbContext.LabBookingDetails
                        .Where(d =>
                            d.CreateDateTime >= start &&
                            d.CreateDateTime < end &&
                            d.NoOrder.StartsWith(labPrefix))
                        .OrderByDescending(d => d.NoOrder)
                        .FirstOrDefaultAsync();

                    int nextNumber = 1;

                    if (lastOrderToday != null)
                    {
                        string lastNo = lastOrderToday.NoOrder;
                        string lastNumStr = lastNo.Substring(lastNo.Length - 4);

                        if (int.TryParse(lastNumStr, out int lastNum))
                            nextNumber = lastNum + 1;
                    }

                    newNoOrder = $"{labPrefix}{today:yyyyMMdd}{nextNumber:D4}";

                }

                // ==========================================================
                // ✅ Update field dari ViewModel
                // ==========================================================
                existingData.BookingLabId = vm.BookingLabId;
                existingData.PasienId = vm.PasienId;
                existingData.PemeriksaanLabId = vm.PemeriksaanLabId;
                existingData.LabId = vm.LabId;

                existingData.KategoriPatologiAnatomi = vm.KategoriPatologiAnatomi;
                existingData.JenisSpecimen = vm.JenisSpecimen;
                existingData.LokasiSpecimen = vm.LokasiSpecimen;
                existingData.KeteranganKlinik = vm.KeteranganKlinik;
                existingData.PenyakitSebelumnya = vm.PenyakitSebelumnya;
                existingData.PenggunaanFiksasi = vm.PenggunaanFiksasi;
                existingData.JenisPemeriksaanGC = vm.JenisPemeriksaanGC;
                existingData.JenisGC = vm.JenisGC;
                existingData.BahanNonGC = vm.BahanNonGC;
                existingData.BahanMicrobiologi = vm.BahanMicrobiologi;
                existingData.MasaHaidTerakhir = vm.MasaHaidTerakhir;
                existingData.Diagnosa = vm.Diagnosa;
                existingData.SpecimenJenisId = vm.SpecimenJenisId;
                existingData.SpecimenMethodId = vm.SpecimenMethodId;
                existingData.AsalSpecimenId = vm.AsalSpecimenId;
                existingData.StatusPemeriksaan = vm.StatusPemeriksaan;
                existingData.TanggalSelesai = vm.TanggalSelesai;
                existingData.NoOrder = newNoOrder;
                existingData.Satuan = vm.Satuan;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabBookingDetails.Update(existingData);
                await _applicationDbContext.SaveChangesAsync();

                // ==========================================================
                // ✅ Sinkronisasi Billing Pemeriksaan Lab
                // ==========================================================
                if (vm.PemeriksaanLabId != null)
                {
                    var pemeriksaan = await _applicationDbContext.LabPemeriksaans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PemeriksaanLabId == vm.PemeriksaanLabId);

                    var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                        vm.KunjunganId,
                        "Pemeriksaan Lab",
                        pemeriksaan.PemeriksaanLabId,
                        ct);

                    if (pemeriksaan != null)
                    {
                        // Cek apakah sudah ada billing untuk pemeriksaan ini
                        var existingBilling = await _applicationDbContext.Billings
                            .FirstOrDefaultAsync(b => b.ItemId == vm.PemeriksaanLabId
                                                   && b.JenisBilling == "Pemeriksaan Lab"
                                                   && (b.IsDelete == false || b.IsDelete == null));

                        if (existingBilling == null)
                        {
                            // Tambah billing baru
                            var billing = new Billing
                            {
                                BillingId = Guid.NewGuid(),
                                KunjunganId = vm.KunjunganId ?? Guid.Empty,
                                ItemId = pemeriksaan.PemeriksaanLabId,
                                NamaItem = pemeriksaan.NamaPemeriksaan,
                                HargaItem = pemeriksaan.HargaPemeriksaan ?? 0,
                                InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)vm.KunjunganId,
                                DateTime.UtcNow),
                                IsListWhiteOff = false,
                                QtyItem = 1,
                                SubTotalItem = pemeriksaan.HargaPemeriksaan ?? 0,
                                BillingKode = "LAB",
                                JenisBilling = "Pemeriksaan Lab",
                                BillingDate = DateTime.UtcNow,
                                TanggalInvoice = DateTime.UtcNow,
                                TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                                StatusBilling = false,

                                IsCovered = coverage?.IsCovered,
                                IsCoveredExcess = coverage?.IsCoveredExcess,
                                AsuransiId = coverage?.AsuransiId,
                                AsuransiExcessId = coverage?.AsuransiExcessId,

                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                                StatusPengambilan = false,
                                Keterangan = $"Otomatis dari Update Booking Lab ({newNoOrder})"
                            };

                            _applicationDbContext.Billings.Add(billing);
                        }
                        else
                        {
                            // Update billing lama (jika harga atau nama berubah)
                            existingBilling.NamaItem = pemeriksaan.NamaPemeriksaan;
                            existingBilling.HargaItem = pemeriksaan.HargaPemeriksaan ?? existingBilling.HargaItem;
                            existingBilling.SubTotalItem = existingBilling.HargaItem * (existingBilling.QtyItem ?? 1);
                            existingBilling.UpdateBy = userActiveId;
                            existingBilling.UpdateDateTime = DateTimeOffset.UtcNow;

                            _applicationDbContext.Billings.Update(existingBilling);
                        }

                        await _applicationDbContext.SaveChangesAsync();
                    }
                }

                // ==========================================================
                // ✅ RESPONSE
                // ==========================================================
                await _hubContext.Clients.All.SendAsync("Lab booking detail changed", new
                {
                    Action = "create",
                    id = existingData.DetailBookingLabId
                });
                return Ok(new
                {
                    message = "Update Data Detail Booking Lab & Billing Berhasil || 200 OK",
                    data = new
                    {
                        existingData.DetailBookingLabId,
                        existingData.NoOrder
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
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
                var data = await _applicationDbContext.LabBookingDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabBookingDetails.Update(data);
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
            Guid? labbookingdeatilId = null,
            string? NamaLaboratorium = null,
            Guid? kunjunganId = null,
            bool? isLunas = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            // guard
            if (page <= 0) page = 1;
            if (perPage <= 0) perPage = 10;
            if (perPage > 200) perPage = 200; // hard cap biar server aman

            // Base query (no tracking => ringan)
            var query =
                from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                where d.IsDelete != true
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u.UserActiveId into ug
                from u in ug.DefaultIfEmpty()

                join l in _applicationDbContext.Labs.AsNoTracking()
                    on d.LabId equals l.LabId into lg
                from l in lg.DefaultIfEmpty()

                join b in _applicationDbContext.LabBookings.AsNoTracking()
                    on d.BookingLabId equals b.BookingLabId into bg
                from b in bg.DefaultIfEmpty()

                join p in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals p.PemeriksaanLabId into pg
                from p in pg.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on b.KunjunganId equals k.KunjunganID into kg
                from k in kg.DefaultIfEmpty()

                join bl in _applicationDbContext.Billings.AsNoTracking()
                    on d.PemeriksaanLabId equals bl.ItemId into blg
                from bl in blg.DefaultIfEmpty()

                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    d.DetailBookingLabId,
                    d.BookingLabId,
                    d.PasienId,
                    d.NoOrder,
                    d.TipeLayanan,
                    KunjunganId = b != null ? b.KunjunganId : (Guid?)null,
                    JenisKunjungan = k != null ? k.JenisKunjungan : null,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = p != null ? (p.NamaPemeriksaan ?? "-") : "-",
                    HargaPemeriksaan = p != null ? p.HargaPemeriksaan : (decimal?)null,
                    d.LabId,
                    NamaLab = l != null ? (l.NamaLab ?? "-") : "-",
                    BillingId = bl != null ? (Guid?)bl.BillingId : null,
                    IsLunas = bl != null ? (bool?)bl.StatusBilling : null,
                    d.KategoriPatologiAnatomi,
                    d.JenisSpecimen,
                    d.LokasiSpecimen,
                    d.KeteranganKlinik,
                    d.PenyakitSebelumnya,
                    d.PenggunaanFiksasi,
                    d.JenisPemeriksaanGC,
                    d.JenisGC,
                    d.BahanNonGC,
                    d.BahanMicrobiologi,
                    d.MasaHaidTerakhir,
                    d.Diagnosa,
                    d.SpecimenJenisId,
                    d.SpecimenMethodId,
                    d.AsalSpecimenId,
                    d.AlasanPembatalan,
                    d.TTDPembatalanPath,
                    d.StatusPemeriksaan,
                    d.TanggalSelesai,
                    d.StatusVerifikasi,
                    d.Satuan,
                };

            // Filter kunjunganId
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            // Filter lab booking id
            if (labbookingdeatilId.HasValue)
                query = query.Where(x => x.DetailBookingLabId == labbookingdeatilId.Value);

            // filter based on status billing
            if (isLunas.HasValue)
                query = query.Where(x=>x.IsLunas == isLunas.Value);

            // Filter search NamaLaboratorium (ILike pakai pattern)
            if (!string.IsNullOrWhiteSpace(NamaLaboratorium))
            {
                var pattern = $"%{NamaLaboratorium.Trim().ToLower()}%";
                query = query.Where(x => EF.Functions.ILike(x.NamaLab, pattern));
            }

            // Filter periode/date range (gunakan range, jangan .Date)
            // Kita pakai UTC date boundary agar index CreateDateTime kepakai.
            DateTimeOffset? startUtc = null;
            DateTimeOffset? endUtcExclusive = null;

            if (startDate.HasValue && endDate.HasValue)
            {
                var s = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var e = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc).AddDays(1); // exclusive
                startUtc = new DateTimeOffset(s);
                endUtcExclusive = new DateTimeOffset(e);
            }
            else if (periode.HasValue)
            {
                var todayUtc = DateTime.UtcNow.Date;

                (DateTime s, DateTime e) = periode.Value switch
                {
                    PeriodeFilter.Today => (todayUtc, todayUtc.AddDays(1)),
                    PeriodeFilter.ThisWeek => (todayUtc.AddDays(-(int)todayUtc.DayOfWeek), todayUtc.AddDays(1)),
                    PeriodeFilter.LastWeek => (todayUtc.AddDays(-7 - (int)todayUtc.DayOfWeek), todayUtc.AddDays(-(int)todayUtc.DayOfWeek)),
                    PeriodeFilter.ThisMonth => (new DateTime(todayUtc.Year, todayUtc.Month, 1), new DateTime(todayUtc.Year, todayUtc.Month, 1).AddMonths(1)),
                    PeriodeFilter.LastMonth => (new DateTime(todayUtc.Year, todayUtc.Month, 1).AddMonths(-1), new DateTime(todayUtc.Year, todayUtc.Month, 1)),
                    PeriodeFilter.ThisYear => (new DateTime(todayUtc.Year, 1, 1), new DateTime(todayUtc.Year + 1, 1, 1)),
                    PeriodeFilter.LastYear => (new DateTime(todayUtc.Year - 1, 1, 1), new DateTime(todayUtc.Year, 1, 1)),
                    PeriodeFilter.Last3Months => (todayUtc.AddMonths(-3), todayUtc.AddDays(1)),
                    PeriodeFilter.Last6Months => (todayUtc.AddMonths(-6), todayUtc.AddDays(1)),
                    _ => (todayUtc, todayUtc.AddDays(1))
                };

                startUtc = new DateTimeOffset(DateTime.SpecifyKind(s, DateTimeKind.Utc));
                endUtcExclusive = new DateTimeOffset(DateTime.SpecifyKind(e, DateTimeKind.Utc));
            }

            if (startUtc.HasValue && endUtcExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime >= startUtc.Value && x.CreateDateTime < endUtcExclusive.Value);

            // Sorting whitelist (hindari dynamic string reflection)
            var desc = (sortDirection ?? "desc").Equals("desc", StringComparison.OrdinalIgnoreCase);

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName).ThenByDescending(x => x.CreateDateTime)
                                      : query.OrderBy(x => x.CreateByName).ThenBy(x => x.CreateDateTime),

                _ => desc ? query.OrderByDescending(x => x.CreateDateTime)
                          : query.OrderBy(x => x.CreateDateTime)
            };

            // Total count (async)
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

            // Page data (async)
            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

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
