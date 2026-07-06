using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
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
    [EnableCors("FrontendCorsPolicy")]
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
        private readonly INoPhotoGeneratorService _noPhotoGeneratorService;

        public LabBookingDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            //IConfiguration configuration,
            ITTDService ttdService,
            IHubContext<LabBookingDetailHub> hubContext,
            INoPhotoGeneratorService noPhotoGeneratorService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            //_uploadUrl = configuration["FileStorage:UploadUrl"];
            _hubContext = hubContext;
            _ttdService = ttdService;
            _noPhotoGeneratorService = noPhotoGeneratorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = 
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into userJoin
                from u in userJoin.DefaultIfEmpty()

                    // Join ke Lab Pemeriksaan
                join p0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals p0.PemeriksaanLabId into labPemeriksaans
                from p in labPemeriksaans.DefaultIfEmpty()

                    // Join ke Booking Bed
                join bb0 in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                    on d.LabBooking.KunjunganId equals bb0.KunjunganId into labBookingBedRanaps
                from bb in labBookingBedRanaps.DefaultIfEmpty()

                    // Join ke Asuransi Pasien untuk ambil NoPolis, IsUtama, IsExcess
                join ap0 in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                    on d.LabBooking.Kunjungan.AsuransiPasienId equals (Guid?)ap0.AsuransiPasienId into asuransiPasienJoin
                from ap in asuransiPasienJoin.DefaultIfEmpty()

                where d.IsDelete == false || d.IsDelete == null

                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    d.DetailBookingLabId,
                    d.BookingLabId,
                    NoOrder = d.LabBooking != null ? d.LabBooking.NoOrder : null,
                    NamaLab = d.Lab != null
                        ? d.Lab.NamaLab
                        : null,
                    NamaKonfirmator = d.LabBooking != null ? d.LabBooking.Konfirmator.FullName : null,
                    TglBooking = d.LabBooking != null ? d.LabBooking.TglBooking : null,
                    TglPemeriksaan = d.LabBooking != null ? d.LabBooking.TglPemeriksaan : null,
                    TglKonfirmasi = d.LabBooking != null ? d.LabBooking.TglKonfirmasi : null,
                    TglSampling = d.LabBooking != null ? d.LabBooking.TglSampling : null,

                    // informasi dokter
                    DokterPemeriksa = d.LabBooking != null ? d.LabBooking.DokterPemeriksa.NmDokter : null,
                    DokterKonsulen = d.LabBooking != null ? d.LabBooking.DokterKonsulen.NmDokter : null,
                    DokterRujukan = d.LabBooking != null ? d.LabBooking.DokterPerujuk.NmDokter : null,

                    // Informasi Pasien
                    d.PasienId,

                    NamaPasien = d.LabBooking != null && d.LabBooking.Pasien != null
                        ? d.LabBooking.Pasien.NamaLengkap
                        : null,

                    NoRM = d.LabBooking != null && d.LabBooking.Pasien != null
                        ? d.LabBooking.Pasien.NoRekamMedis
                        : null,

                    JenisKelamin = d.LabBooking != null && d.LabBooking.Pasien != null
                        ? d.LabBooking.Pasien.JenisKelamin
                        : null,

                    Email = d.LabBooking != null && d.LabBooking.Pasien != null
                        ? d.LabBooking.Pasien.Email
                        : null,

                    d.IsCito,

                    // Informasi Kunjungan
                    KunjunganId = d.LabBooking != null
                        ? d.LabBooking.KunjunganId
                        : null,

                    NoRegistrasi = d.LabBooking != null && d.LabBooking.Kunjungan != null
                        ? d.LabBooking.Kunjungan.NoRegistrasi
                        : null,

                    NamaPoli = d.LabBooking != null &&
                               d.LabBooking.Kunjungan != null &&
                               d.LabBooking.Kunjungan.Poliklinik != null
                        ? d.LabBooking.Kunjungan.Poliklinik.NamaPoliklinik
                        : null,
                    JenisKunjungan = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.JenisKunjungan
                      : null,

                    // kamar
                    Kamarid = bb.KamarId,
                    NamaKamar = bb.Kamar != null ? bb.Kamar.NamaKamar : null,

                    // Informasi Asuransi
                    AsuransiId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                        ? d.LabBooking.Kunjungan.AsuransiId
                        : null,

                    AsuransiPasienId = ap != null
                        ? ap.AsuransiPasienId
                        : d.LabBooking.Kunjungan.AsuransiPasienId,

                    NamaAsuransi = d.LabBooking != null &&
                                   d.LabBooking.Kunjungan != null &&
                                   d.LabBooking.Kunjungan.Asuransi != null
                        ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                        : null,

                    NoPolis = ap != null
                        ? ap.NoPolis
                        : null,

                    IsUtama = ap != null
                        ? ap.IsUtama
                        : null,

                    // asuransi excess
                    AsuransiExcessId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                        ? d.LabBooking.Kunjungan.AsuransiExcessId
                        : null,

                    AsuransiPasienExcessId = ap != null
                        ? ap.AsuransiPasienId
                        : d.LabBooking.Kunjungan.AsuransiPasienExcessId,

                    NamaAsuransiExcess = d.LabBooking != null &&
                                   d.LabBooking.Kunjungan != null &&
                                   d.LabBooking.Kunjungan.Asuransi != null
                        ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                        : null,
                    NoPolisExcess = ap != null
                        ? ap.NoPolis
                        : null,
                    IsUtamaExcess = ap != null
                        ? ap.IsUtama
                        : null,
                    IsExcess = ap != null
                        ? ap.IsExcess
                        : null,


                    // =========================
                    // Informasi Pemeriksaan
                    // =========================

                    d.PemeriksaanLabId,

                    NamaPemeriksaan = p != null
                        ? p.NamaPemeriksaan
                        : null,

                    HargaPemeriksaan = p != null
                        ? p.HargaPemeriksaan
                        : null,

                    d.NoPhoto,
                    d.StatusPemeriksaan,
                    d.TanggalSelesai,
                    d.QtyOrder
                }).OrderByDescending(d=>d.CreateDateTime);

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
                var data = await 
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                 join u0 in _applicationDbContext.UserActives.AsNoTracking()
                     on d.CreateBy equals u0.UserActiveId into userJoin
                 from u in userJoin.DefaultIfEmpty()

                     // Join ke Lab Pemeriksaan
                 join p0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals p0.PemeriksaanLabId into labPemeriksaans
                 from p in labPemeriksaans.DefaultIfEmpty()

                     // Join ke Booking Bed
                 join bb0 in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                     on d.LabBooking.KunjunganId equals bb0.KunjunganId into labBookingBedRanaps
                 from bb in labBookingBedRanaps.DefaultIfEmpty()

                     // Join ke Asuransi Pasien untuk ambil NoPolis, IsUtama, IsExcess
                 join ap0 in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                     on d.LabBooking.Kunjungan.AsuransiPasienId equals (Guid?)ap0.AsuransiPasienId into asuransiPasienJoin
                 from ap in asuransiPasienJoin.DefaultIfEmpty()

                 where d.IsDelete == false && d.DetailBookingLabId == id

                 select new
                 {
                     d.CreateDateTime,
                     d.CreateBy,
                     CreateByName = u != null ? u.FullName : null,

                     d.DetailBookingLabId,
                     d.BookingLabId,
                     NoOrder = d.LabBooking != null ? d.LabBooking.NoOrder : null,
                     NamaLab = d.Lab != null
                         ? d.Lab.NamaLab
                         : null,
                     NamaKonfirmator = d.LabBooking != null ? d.LabBooking.Konfirmator.FullName : null,
                     TglBooking = d.LabBooking != null ? d.LabBooking.TglBooking : null,
                     TglPemeriksaan = d.LabBooking != null ? d.LabBooking.TglPemeriksaan : null,
                     TglKonfirmasi = d.LabBooking != null ? d.LabBooking.TglKonfirmasi : null,
                     TglSampling = d.LabBooking != null ? d.LabBooking.TglSampling : null,

                     // Informasi dokter
                     DokterPemeriksa = d.LabBooking != null ? d.LabBooking.DokterPemeriksa.NmDokter : null,
                     DokterKonsulen = d.LabBooking != null ? d.LabBooking.DokterKonsulen.NmDokter : null,
                     DokterRujukan = d.LabBooking != null ? d.LabBooking.DokterPerujuk.NmDokter : null,

                     // Informasi Pasien
                     d.PasienId,
                     NamaPasien = d.LabBooking != null && d.LabBooking.Pasien != null
                         ? d.LabBooking.Pasien.NamaLengkap
                         : null,

                     NoRM = d.LabBooking != null && d.LabBooking.Pasien != null
                         ? d.LabBooking.Pasien.NoRekamMedis
                         : null,

                     JenisKelamin = d.LabBooking != null && d.LabBooking.Pasien != null
                         ? d.LabBooking.Pasien.JenisKelamin
                         : null,

                     Email = d.LabBooking != null && d.LabBooking.Pasien != null
                         ? d.LabBooking.Pasien.Email
                         : null,

                     d.IsCito,

                     // Informasi Kunjungan
                     KunjunganId = d.LabBooking != null
                         ? d.LabBooking.KunjunganId
                         : null,

                     NoRegistrasi = d.LabBooking != null && d.LabBooking.Kunjungan != null
                         ? d.LabBooking.Kunjungan.NoRegistrasi
                         : null,

                     NamaPoli = d.LabBooking != null &&
                                d.LabBooking.Kunjungan != null &&
                                d.LabBooking.Kunjungan.Poliklinik != null
                         ? d.LabBooking.Kunjungan.Poliklinik.NamaPoliklinik
                         : null,
                     JenisKunjungan = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.JenisKunjungan
                      : null,

                     // kamar
                     Kamarid = bb.KamarId,
                     NamaKamar = bb.Kamar != null ? bb.Kamar.NamaKamar : null,

                     // Informasi Asuransi
                     AsuransiId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                         ? d.LabBooking.Kunjungan.AsuransiId
                         : null,

                     AsuransiPasienId = ap != null
                         ? ap.AsuransiPasienId
                         : d.LabBooking.Kunjungan.AsuransiPasienId,

                     NamaAsuransi = d.LabBooking != null &&
                                    d.LabBooking.Kunjungan != null &&
                                    d.LabBooking.Kunjungan.Asuransi != null
                         ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                         : null,

                     NoPolis = ap != null
                         ? ap.NoPolis
                         : null,

                     IsUtama = ap != null
                         ? ap.IsUtama
                         : null,

                     // asuransi excess
                     AsuransiExcessId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                         ? d.LabBooking.Kunjungan.AsuransiExcessId
                         : null,

                     AsuransiPasienExcessId = ap != null
                         ? ap.AsuransiPasienId
                         : d.LabBooking.Kunjungan.AsuransiPasienExcessId,

                     NamaAsuransiExcess = d.LabBooking != null &&
                                    d.LabBooking.Kunjungan != null &&
                                    d.LabBooking.Kunjungan.Asuransi != null
                         ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                         : null,
                     NoPolisExcess = ap != null
                         ? ap.NoPolis
                         : null,
                     IsUtamaExcess = ap != null
                         ? ap.IsUtama
                         : null,
                     IsExcess = ap != null
                         ? ap.IsExcess
                         : null,


                     // =========================
                     // Informasi Pemeriksaan
                     // =========================

                     d.PemeriksaanLabId,

                     NamaPemeriksaan = p != null
                         ? p.NamaPemeriksaan
                         : null,

                     NamaKategori = p != null ? p.KategoriPemeriksaan.NamaKategori : null,

                     HargaPemeriksaan = p != null
                         ? p.HargaPemeriksaan
                         : null,

                     d.NoPhoto,
                     d.StatusPemeriksaan,
                     d.TanggalSelesai,
                     d.QtyOrder
                 }).FirstOrDefaultAsync();

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
        public async Task<IActionResult> Create(
            [FromBody] LabBookingDetailViewModel vm,
            CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            await using var transaction = await _applicationDbContext.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                // Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                if (!vm.BookingLabId.HasValue || vm.BookingLabId == Guid.Empty)
                    return BadRequest(new { message = "BookingLabId wajib diisi." });

                if (!vm.LabId.HasValue || vm.LabId == Guid.Empty)
                    return BadRequest(new { message = "LabId wajib diisi." });

                if (!vm.PemeriksaanLabId.HasValue || vm.PemeriksaanLabId == Guid.Empty)
                    return BadRequest(new { message = "PemeriksaanLabId wajib diisi." });

                // Pastikan BookingLab ada
                var bookingExists = await _applicationDbContext.LabBookings
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.BookingLabId == vm.BookingLabId.Value &&
                        !x.IsDelete,
                        ct);

                if (!bookingExists)
                    return BadRequest(new { message = "Data booking lab tidak ditemukan." });

                // Pastikan Lab ada
                //var labExists = await _applicationDbContext.Labs
                //    .AsNoTracking()
                //    .AnyAsync(x =>
                //        x.LabId == vm.LabId.Value &&
                //        !x.IsDelete,
                //        ct);

                //if (!labExists)
                //    return BadRequest(new { message = "Data lab tidak ditemukan." });

                //// Pastikan Pemeriksaan Lab ada
                //var pemeriksaanExists = await _applicationDbContext.LabPemeriksaans
                //    .AsNoTracking()
                //    .AnyAsync(x =>
                //        x.PemeriksaanLabId == vm.PemeriksaanLabId.Value &&
                //        !x.IsDelete,
                //        ct);

                //if (!pemeriksaanExists)
                //    return BadRequest(new { message = "Data pemeriksaan lab tidak ditemukan." });

                var noOrder = await _noPhotoGeneratorService.EnsureNoOrderForBookingAsync(
                    vm.BookingLabId.Value,
                    vm.LabId.Value,
                    userActiveId,
                    ct
                );

                var data = new LabBookingDetail
                {
                    DetailBookingLabId = Guid.NewGuid(),
                    BookingLabId = vm.BookingLabId,
                    PasienId = vm.PasienId,
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    LabId = vm.LabId,
                    IsCito = vm.IsCito,
                    DokterPemeriksaId = vm.DokterPemeriksaId,
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
                    AsalSpecimenId = vm.AsalSpecimenId,
                    SpecimenMethodId = vm.SpecimenMethodId,
                    SpecimenJenisId = vm.SpecimenJenisId,
                    StatusPemeriksaan = vm.StatusPemeriksaan,
                    TanggalSelesai = vm.TanggalSelesai,
                    QtyOrder = vm.QtyOrder,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.LabBookingDetails.Add(data);

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                await _hubContext.Clients.All.SendAsync("Lab booking detail created", new
                {
                    Action = "create",
                    Id = data.DetailBookingLabId
                }, ct);

                return Created("", new
                {
                    message = "Tambah Data Detail Booking Lab & Billing Berhasil || 201 Created",
                    data = new
                    {
                        data.DetailBookingLabId,
                        NoOrder = noOrder
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = "Gagal menyimpan data.",
                    error = dbEx.Message,
                    innerError = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan internal.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
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

        //[HttpPut("Verifikasi-Lab/{id}")]
        //public async Task<IActionResult> VerikasiBooking(Guid id, [FromBody] VerifikasiLabViewModel vm)
        //{
        //    if (vm == null)
        //        return BadRequest(new { message = "Data pembatalan tidak valid." });

        //    var booking = await _applicationDbContext.LabBookingDetails
        //        .FirstOrDefaultAsync(b => b.DetailBookingLabId == id);

        //    if (booking == null)
        //        return NotFound(new { message = "Data booking tidak ditemukan." });

        //    // 🔐 Ambil user dari JWT Claims
        //    var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(emailLogin))
        //        return Unauthorized(new { message = "User tidak terautentikasi!" });

        //    var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
        //    if (getUserActive == null)
        //        return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //    var userActiveId = getUserActive.UserActiveId;

        //    // ==================================================
        //    // ✅ UPDATE DATA BOOKING MENJADI TERVERIFIKASI
        //    // ==================================================

        //    booking.StatusVerifikasi = vm.Status;
        //    booking.VerifikatorId = vm.VerifkatorId;
        //    booking.WaktuVerifikasi = DateTime.UtcNow;

        //    booking.UpdateBy = userActiveId;
        //    booking.UpdateDateTime = DateTimeOffset.UtcNow;

        //    _applicationDbContext.LabBookingDetails.Update(booking);

        //    await _applicationDbContext.SaveChangesAsync();

        //    // ==================================================
        //    // ✅ RESPONSE
        //    // ==================================================
        //    return Ok(new
        //    {
        //        message = "Booking lab berhasil diverifikasi",
        //        bookingId = booking.DetailBookingLabId,
        //    });
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingDetailEditViewModel vm, CancellationToken ct)
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
                // ✅ Update field dari ViewModel
                // ==========================================================
                existingData.BookingLabId = vm.BookingLabId;
                existingData.PasienId = vm.PasienId;
                existingData.PemeriksaanLabId = vm.PemeriksaanLabId;
                existingData.LabId = vm.LabId;
                existingData.DokterPemeriksaId = vm.DokterPemeriksaId;
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
                existingData.SpecimenJenisId = vm.SpecimenJenisId;
                existingData.SpecimenMethodId = vm.SpecimenMethodId;
                existingData.AsalSpecimenId = vm.AsalSpecimenId;
                existingData.StatusPemeriksaan = vm.StatusPemeriksaan;
                existingData.TanggalSelesai = vm.TanggalSelesai;
                existingData.QtyOrder = vm.QtyOrder;
                existingData.IsCito = vm.IsCito;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabBookingDetails.Update(existingData);
                await _applicationDbContext.SaveChangesAsync();

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
                        existingData.DetailBookingLabId
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
            Guid? kunjunganId = null,
            bool? isLunas = null,
            Guid? kamarId = null,
            Guid? labId = null,
            string? namaKamar = null,
            string? noRM = null,
            string? namaLab = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
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
             (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

              join u0 in _applicationDbContext.UserActives.AsNoTracking()
                  on d.CreateBy equals u0.UserActiveId into userJoin
              from u in userJoin.DefaultIfEmpty()

                  // Join ke Lab Pemeriksaan
              join p0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                  on d.PemeriksaanLabId equals p0.PemeriksaanLabId into labPemeriksaans
              from p in labPemeriksaans.DefaultIfEmpty()

                  // Join ke Booking Bed
              join bb0 in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                  on d.LabBooking.KunjunganId equals bb0.KunjunganId into labBookingBedRanaps
              from bb in labBookingBedRanaps.DefaultIfEmpty()

                  // Join ke Asuransi Pasien untuk ambil NoPolis, IsUtama, IsExcess
              join ap0 in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                  on d.LabBooking.Kunjungan.AsuransiPasienId equals (Guid?)ap0.AsuransiPasienId into asuransiPasienJoin
              from ap in asuransiPasienJoin.DefaultIfEmpty()
              where d.IsDelete == false || d.IsDelete == null

              select new
              {
                  d.CreateDateTime,
                  d.CreateBy,
                  CreateByName = u != null ? u.FullName : null,

                  d.DetailBookingLabId,
                  d.BookingLabId,
                  NoOrder = d.LabBooking != null ? d.LabBooking.NoOrder : null,
                  d.LabId,
                  NamaLab = d.Lab != null
                      ? d.Lab.NamaLab
                      : null,
                  NamaKonfirmator = d.LabBooking != null ? d.LabBooking.Konfirmator.FullName : null,
                  TglBooking = d.LabBooking != null ? d.LabBooking.TglBooking : null,
                  TglPemeriksaan = d.LabBooking != null ? d.LabBooking.TglPemeriksaan : null,
                  TglKonfirmasi = d.LabBooking != null ? d.LabBooking.TglKonfirmasi : null,
                  TglSampling = d.LabBooking != null ? d.LabBooking.TglSampling : null,

                  // informasi dokter
                  DokterPemeriksa = d.LabBooking != null ? d.LabBooking.DokterPemeriksa.NmDokter : null,
                  DokterKonsulen = d.LabBooking != null ? d.LabBooking.DokterKonsulen.NmDokter : null,
                  DokterRujukan = d.LabBooking != null ? d.LabBooking.DokterPerujuk.NmDokter : null,

                  // Informasi Pasien
                  d.PasienId,

                  NamaPasien = d.LabBooking != null && d.LabBooking.Pasien != null
                      ? d.LabBooking.Pasien.NamaLengkap
                      : null,

                  NoRM = d.LabBooking != null && d.LabBooking.Pasien != null
                      ? d.LabBooking.Pasien.NoRekamMedis
                      : null,

                  NoIdentitas = d.LabBooking != null && d.LabBooking.Pasien != null
                      ? d.LabBooking.Pasien.NoIdentitas
                      : null,

                  JenisKelamin = d.LabBooking != null && d.LabBooking.Pasien != null
                      ? d.LabBooking.Pasien.JenisKelamin
                      : null,

                  Email = d.LabBooking != null && d.LabBooking.Pasien != null
                      ? d.LabBooking.Pasien.Email
                      : null,

                  d.IsCito,

                  // Informasi Kunjungan
                  KunjunganId = d.LabBooking != null
                      ? d.LabBooking.KunjunganId
                      : null,

                  NoRegistrasi = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.NoRegistrasi
                      : null,
                  NamaPoli = d.LabBooking != null &&
                             d.LabBooking.Kunjungan != null &&
                             d.LabBooking.Kunjungan.Poliklinik != null
                      ? d.LabBooking.Kunjungan.Poliklinik.NamaPoliklinik
                      : null,
                JenisKunjungan = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.JenisKunjungan
                      : null,

                  // kamar
                  Kamarid =bb.KamarId,
                  NamaKamar = bb.Kamar != null ? bb.Kamar.NamaKamar : null,

                  // Informasi Asuransi
                  AsuransiId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.AsuransiId
                      : null,

                  AsuransiPasienId = ap != null
                      ? ap.AsuransiPasienId
                      : d.LabBooking.Kunjungan.AsuransiPasienId,

                  NamaAsuransi = d.LabBooking != null &&
                                 d.LabBooking.Kunjungan != null &&
                                 d.LabBooking.Kunjungan.Asuransi != null
                      ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                      : null,

                  NoPolis = ap != null
                      ? ap.NoPolis
                      : null,

                  IsUtama = ap != null
                      ? ap.IsUtama
                      : null,

                  // asuransi excess
                  AsuransiExcessId = d.LabBooking != null && d.LabBooking.Kunjungan != null
                      ? d.LabBooking.Kunjungan.AsuransiExcessId
                      : null,

                  AsuransiPasienExcessId = ap != null
                      ? ap.AsuransiPasienId
                      : d.LabBooking.Kunjungan.AsuransiPasienExcessId,

                  NamaAsuransiExcess = d.LabBooking != null &&
                                 d.LabBooking.Kunjungan != null &&
                                 d.LabBooking.Kunjungan.Asuransi != null
                      ? d.LabBooking.Kunjungan.Asuransi.NamaAsuransi
                      : null,
                  NoPolisExcess = ap != null
                      ? ap.NoPolis
                      : null,
                  IsUtamaExcess = ap != null
                      ? ap.IsUtama
                      : null,
                  IsExcess = ap != null
                      ? ap.IsExcess
                      : null,


                  // =========================
                  // Informasi Pemeriksaan
                  // =========================

                  d.PemeriksaanLabId,

                  NamaPemeriksaan = p != null
                      ? p.NamaPemeriksaan
                      : null,
                  
                  NamaKategori = p!= null ? p.KategoriPemeriksaan.NamaKategori :null,

                  HargaPemeriksaan = p != null
                      ? p.HargaPemeriksaan
                      : null,

                  d.NoPhoto,
                  d.StatusPemeriksaan,
                  d.TanggalSelesai,
                  IsLunas = _applicationDbContext.Billings
                        .AsNoTracking()
                        .Where(b =>
                            b.KunjunganId == d.LabBooking.KunjunganId &&
                            b.ItemId == d.PemeriksaanLabId &&
                            b.JenisBilling == "Pemeriksaan Lab" &&
                            (b.IsDelete == false || b.IsDelete == null))
                        .Select(b => (bool?)b.StatusBilling)
                        .FirstOrDefault(),
                  d.QtyOrder,
                  
              });

            // Filter kunjunganId
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            // Filter lab booking id
            if (labbookingdeatilId.HasValue)
                query = query.Where(x => x.DetailBookingLabId == labbookingdeatilId.Value);

            // filter based on status billing
            if (isLunas.HasValue)
                query = query.Where(x=>x.IsLunas == isLunas.Value);

            if (kamarId.HasValue)
                query = query.Where(x=>x.Kamarid == kamarId.Value);

            if (labId.HasValue)
                query = query.Where(x=>x.LabId == labId.Value);

            if (!string.IsNullOrWhiteSpace(namaKamar))
            {
                var pattern = $"%{namaKamar.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaKamar ?? "", pattern));
            }

            if (!string.IsNullOrWhiteSpace(noRM))
            {
                var pattern = $"%{noRM.Trim()}%";

                query = query.Where
                    (x =>EF.Functions.ILike(x.NoRM ?? "", pattern) ||
                    EF.Functions.ILike(x.NoIdentitas ?? "", pattern));
            }

            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                var pattern = $"%{namaLab.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaLab ?? "", pattern));
            }

            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value.ToString();

                query = query.Where(b =>
                    b.JenisKunjungan == jk);
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

            var desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            var selectedOrderBy = orderBy?.Trim() ?? "CreateDateTime";

            query = desc
                ? selectedOrderBy switch
                {
                    "CreateDateTime" => query
                        .OrderByDescending(x => x.CreateDateTime),

                    "TglBooking" => query
                        .OrderBy(x => x.TglBooking == null)
                        .ThenByDescending(x => x.TglBooking)
                        .ThenByDescending(x => x.CreateDateTime),

                    "TglSampling" => query
                        .OrderBy(x => x.TglSampling == null)
                        .ThenByDescending(x => x.TglSampling)
                        .ThenByDescending(x => x.CreateDateTime),

                    "TglPemeriksaan" => query
                        .OrderBy(x => x.TglPemeriksaan == null)
                        .ThenByDescending(x => x.TglPemeriksaan)
                        .ThenByDescending(x => x.CreateDateTime),

                    "TglKonfirmasi" => query
                        .OrderBy(x => x.TglKonfirmasi == null)
                        .ThenByDescending(x => x.TglKonfirmasi)
                        .ThenByDescending(x => x.CreateDateTime),

                    _ => query
                        .OrderByDescending(x => x.CreateDateTime)
                }
                : selectedOrderBy switch
                {
                    "CreateDateTime" => query
                        .OrderBy(x => x.CreateDateTime),

                    "TglBooking" => query
                        .OrderBy(x => x.TglBooking == null)
                        .ThenBy(x => x.TglBooking)
                        .ThenBy(x => x.CreateDateTime),

                    "TglSampling" => query
                        .OrderBy(x => x.TglSampling == null)
                        .ThenBy(x => x.TglSampling)
                        .ThenBy(x => x.CreateDateTime),

                    "TglPemeriksaan" => query
                        .OrderBy(x => x.TglPemeriksaan == null)
                        .ThenBy(x => x.TglPemeriksaan)
                        .ThenBy(x => x.CreateDateTime),

                    "TglKonfirmasi" => query
                        .OrderBy(x => x.TglKonfirmasi == null)
                        .ThenBy(x => x.TglKonfirmasi)
                        .ThenBy(x => x.CreateDateTime),

                    _ => query
                        .OrderBy(x => x.CreateDateTime)
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
