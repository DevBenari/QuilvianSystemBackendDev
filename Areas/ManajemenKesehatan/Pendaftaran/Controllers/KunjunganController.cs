using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KunjunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<KunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<KunjunganHub> _hubContext;
        private readonly IDepositRanapNumberService _depositRanapNumberService;
        private readonly IKunjunganAdminBillingService _kunjunganAdminBillingService;
        private readonly IConfiguration _configuration;
        private readonly IKunjunganNoRegistrasiService _kunjunganNoRegistrasiService;
        private readonly INoBillService _noBillService;
        private readonly IAsuransiCoverageService _asuransiCoverageService;

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            ILogger<KunjunganController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<KunjunganHub> hubContext,
            IDepositRanapNumberService depositRanapNumberService,
            IKunjunganAdminBillingService kunjunganAdminBillingService,
            IConfiguration configuration,
            IKunjunganNoRegistrasiService kunjunganNoRegistrasiService,
            INoBillService noBillService,
            IAsuransiCoverageService asuransiCoverageService

        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _depositRanapNumberService = depositRanapNumberService;
            _kunjunganAdminBillingService = kunjunganAdminBillingService;
            _configuration = configuration;
            _kunjunganNoRegistrasiService = kunjunganNoRegistrasiService;
            _noBillService = noBillService;
            _asuransiCoverageService = asuransiCoverageService;

        }
        private DateTime? TryParseTanggalLahir(string dateString)
        {
            if (DateTime.TryParseExact(
                dateString,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
            {
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }

            return null;
        }

        private static DateTime? GetTanggalKunjungan(Kunjungan kunjungan)
        {
            if (kunjungan == null)
                return null;

            if (kunjungan.TglMasuk.HasValue)
            {
                return kunjungan.TglMasuk.Value;
            }

            if (kunjungan.CreateDateTime != default)
            {
                return kunjungan.CreateDateTime.LocalDateTime;
            }

            return null;
        }

        private static string? HitungUmurLengkap(DateTime? tanggalLahir)
        {
            if (!tanggalLahir.HasValue) return "-";

            var today = DateTime.Today;
            int tahun = today.Year - tanggalLahir.Value.Year;
            int bulan = today.Month - tanggalLahir.Value.Month;
            int hari = today.Day - tanggalLahir.Value.Day;

            if (hari < 0)
            {
                bulan--;
                var prevMonth = today.AddMonths(-1);
                hari += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            }

            if (bulan < 0)
            {
                tahun--;
                bulan += 12;
            }

            return $"{tahun} tahun {bulan} bulan {hari} hari";
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKunjungan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            try
            {
                // Ambil semua data alergi
                var allAlergic = (await _applicationDbContext.PainAssessments
                    .AsNoTracking()
                    .Where(x => !x.IsDelete)
                    .Select(x => new
                    {
                        x.KunjunganId,
                        x.Alergic
                    })
                    .ToListAsync())
                    .GroupBy(x => x.KunjunganId)
                    .Select(g => new
                    {
                        KunjunganId = g.Key,
                        Alergic = string.Join(", ", g
                            .Where(x => !string.IsNullOrWhiteSpace(x.Alergic))
                            .Select(x => x.Alergic)
                            .Distinct())
                    })
                    .ToList();

                // Hitung jumlah kunjungan per pasien + jenis kunjungan
                var jumlahPerJenis = _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .Where(k => !k.IsDelete)
                    .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                    .Select(g => new
                    {
                        g.Key.PasienId,
                        g.Key.JenisKunjungan,
                        JumlahJenis = g.Count()
                    });

                // Query utama:
                // Poliklinik, Asuransi, Pasien, Dokter memakai navigation property dari Kunjungan
                var query =
                    from a in _applicationDbContext.Kunjungans.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join da in _applicationDbContext.UserActives.AsNoTracking()
                        on a.Dokter.UserActiveId equals da.UserActiveId into daGroup
                    from da in daGroup.DefaultIfEmpty()

                    join j in jumlahPerJenis
                        on new { a.PasienId, a.JenisKunjungan }
                        equals new { j.PasienId, j.JenisKunjungan } into jumlahGroup
                    from j in jumlahGroup.DefaultIfEmpty()

                    join bb in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                        on a.KunjunganID equals bb.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Beds.AsNoTracking()
                        on bb.BedId equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kamars.AsNoTracking()
                        on bb.KamarId equals k.KamarId into kamarGroup
                    from k in kamarGroup.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass.AsNoTracking()
                        on k.KelasId equals kl.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    join sp in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                        on a.KunjunganID equals sp.KunjunganId into suratGroup
                    from sp in suratGroup.DefaultIfEmpty()

                    where a.IsDelete == false

                    select new
                    {
                        a.KunjunganID,

                        a.AsuransiId,
                        a.AsuransiPasienId,
                        NamaAsuransi = a.Asuransi != null && a.Asuransi.NamaAsuransi != null
                            ? a.Asuransi.NamaAsuransi
                            : "Tunai",

                        a.PoliklinikId,
                        NamaPoliklinik = a.Poliklinik != null
                            ? a.Poliklinik.NamaPoliklinik
                            : null,

                        a.DokterId,
                        NamaDokter = a.Dokter != null
                            ? a.Dokter.NmDokter
                            : null,

                        a.PasienId,
                        a.AsalKunjungan,
                        a.NoRegistrasi,
                        NamaLengkap = a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                        TanggalLahir = a.Pasien != null
                            ? a.Pasien.TanggalLahir
                            : null,

                        JenisKelamin = a.Pasien != null
                            ? a.Pasien.JenisKelamin
                            : null,

                        NoPasien = a.Pasien != null
                            ? a.Pasien.NoPasien
                            : null,

                        NoWali1 = a.Pasien != null
                            ? a.Pasien.NoWali1
                            : null,

                        NoWali2 = a.Pasien != null
                            ? a.Pasien.NoWali2
                            : null,

                        NamaWali1 = a.Pasien != null
                            ? a.Pasien.NamaWali1
                            : null,

                        NamaWali2 = a.Pasien != null
                            ? a.Pasien.NamaWali2
                            : null,

                        NamaKontakDarurat = a.Pasien != null
                            ? a.Pasien.NamaKontakDarurat
                            : null,

                        NoTeleponDarurat = a.Pasien != null
                            ? a.Pasien.NoTeleponDarurat
                            : null,

                        Email = a.Pasien != null
                            ? a.Pasien.Email
                            : null,

                        AlamatDomisili = a.Pasien != null
                            ? a.Pasien.AlamatDomisili
                            : null,

                        AlamatDarurat = a.Pasien != null
                            ? a.Pasien.AlamatDarurat
                            : null,

                        AlamatIdentitas = a.Pasien != null
                            ? a.Pasien.AlamatIdentitas
                            : null,

                        Umur = a.Pasien != null
                            ? HitungUmurLengkap(a.Pasien.TanggalLahir)
                            : null,

                        a.NoRekamMedis,
                        a.TipePasien,
                        a.TipePembayaran,
                        a.JenisKunjungan,
                        a.StatusPengkajian,
                        a.CreateDateTime,
                        a.CreateBy,
                        a.IsFinished,
                        a.IsScreening,
                        a.IsPresent,
                        a.Antrian,
                        a.IsFinishedKasir,
                        a.IsTriage,
                        a.IsCTTPasienIGD,

                        TglMasukKunjungan = a.TglMasuk,

                        a.CaraMasukRS,
                        a.KondisiKeluar,

                        NmDokter = a.Dokter != null
                            ? a.Dokter.NmDokter
                            : null,

                        FotoPath = da != null
                            ? da.FotoPath
                            : null,

                        FotoName = da != null
                            ? da.FotoName
                            : null,

                        CreateByName = u != null
                            ? u.FullName
                            : null,

                        KelasId = kl != null
                            ? (Guid?)kl.KelasId
                            : null,

                        JumlahJenisKunjungan = j != null
                            ? j.JumlahJenis
                            : 0,

                        BookingBedRanapId = bb != null
                            ? (Guid?)bb.BookingBedRanapId
                            : null,

                        KamarId = bb != null
                            ? bb.KamarId
                            : null,

                        KamarNama = k != null
                            ? k.NamaKamar
                            : null,

                        LantaiKamar = k != null
                            ? k.Lantai
                            : null,

                        KelasNama = kl != null
                            ? kl.NamaKelas
                            : null,

                        BedId = bb != null
                            ? bb.BedId
                            : null,

                        NomorKamar = bb != null
                            ? bb.NoKamar
                            : null,

                        NomorBed = b != null
                            ? b.NomorBed
                            : null,

                        StatusBed = bb != null
                            ? bb.StatusBed
                            : null,

                        Keterangan = bb != null
                            ? bb.Keterangan
                            : null,

                        TglKeluar = bb != null
                            ? bb.TglKeluar
                            : null,

                        TglMasuk = bb != null
                            ? bb.TglMasuk
                            : null,

                        NomorSuratPengantar = sp != null
                            ? sp.NomorSuratPengantar
                            : null,

                        Diagnosa = sp != null
                            ? sp.Diagnosa
                            : null,

                        AsalUnit = sp != null
                            ? sp.AsalUnit
                            : null
                    };

                // Eksekusi query & urutkan berdasarkan tanggal
                var list = await query
                    .OrderByDescending(a => a.CreateDateTime)
                    .ToListAsync();

                // Hilangkan duplikat
                var uniqueList = list
                    .GroupBy(x => x.KunjunganID)
                    .Select(g => g.First())
                    .ToList();

                // Gabungkan data alergi
                var result = uniqueList.Select(r =>
                {
                    var alergi = allAlergic.FirstOrDefault(a => a.KunjunganId == r.KunjunganID);

                    return new
                    {
                        r.KunjunganID,
                        r.AsuransiId,
                        r.AsuransiPasienId,
                        r.NamaAsuransi,
                        r.PoliklinikId,
                        r.NamaPoliklinik,
                        r.DokterId,
                        r.NamaDokter,
                        r.PasienId,
                        r.AsalKunjungan,
                        r.NamaLengkap,
                        r.TanggalLahir,
                        r.JenisKelamin,
                        r.NoPasien,
                        r.NoWali1,
                        r.NoWali2,
                        r.NamaWali1,
                        r.NamaWali2,
                        r.NamaKontakDarurat,
                        r.NoTeleponDarurat,
                        r.Email,
                        r.AlamatIdentitas,
                        r.AlamatDomisili,
                        r.AlamatDarurat,
                        r.Umur,
                        r.NoRekamMedis,
                        r.TipePasien,
                        r.TipePembayaran,
                        r.JenisKunjungan,
                        r.StatusPengkajian,
                        r.CreateDateTime,
                        r.CreateBy,
                        r.IsFinished,
                        r.IsScreening,
                        r.IsPresent,
                        r.IsTriage,
                        r.IsCTTPasienIGD,
                        r.Antrian,

                        TglMasukKunjungan = r.TglMasukKunjungan,

                        r.CaraMasukRS,
                        r.KondisiKeluar,
                        r.IsFinishedKasir,
                        r.NmDokter,
                        r.FotoName,
                        r.FotoPath,
                        r.CreateByName,
                        r.JumlahJenisKunjungan,
                        r.BookingBedRanapId,
                        r.KelasId,
                        r.KamarId,
                        r.KamarNama,
                        r.LantaiKamar,
                        r.KelasNama,
                        r.BedId,
                        r.NomorKamar,
                        r.NomorBed,
                        r.StatusBed,
                        r.Keterangan,
                        r.TglKeluar,
                        r.TglMasuk,
                        r.NomorSuratPengantar,
                        r.Diagnosa,
                        r.AsalUnit,

                        Alergic = alergi?.Alergic ?? ""
                    };
                }).ToList();

                // Pagination
                var totalRows = result.Count;
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var pagedData = result.Skip((page - 1) * perPage).Take(perPage).ToList();

                if (!pagedData.Any())
                {
                    return NotFound(new
                    {
                        message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found"
                    });
                }

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = pagedData,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKunjunganById(Guid id)
        {
            try
            {
                // Cek apakah Kunjungan ada
                var kunjungan = await _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.KunjunganID == id && !k.IsDelete);

                if (kunjungan == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });
                }

                // Ambil data alergi
                var alergiList = await _applicationDbContext.PainAssessments
                    .AsNoTracking()
                    .Where(x => x.KunjunganId == id && !x.IsDelete)
                    .Select(x => x.Alergic)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToListAsync();

                // Hitung jumlah kunjungan pasien per jenis
                var jumlahPerJenis = await _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .Where(k => !k.IsDelete && k.PasienId == kunjungan.PasienId)
                    .GroupBy(k => k.JenisKunjungan)
                    .Select(g => new
                    {
                        JenisKunjungan = g.Key,
                        Jumlah = g.Count()
                    })
                    .ToListAsync();

                // ===================== QUERY UTAMA PAKAI NAVIGATION PROPERTY =====================
                var result = await (
                    from a in _applicationDbContext.Kunjungans.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join da in _applicationDbContext.UserActives.AsNoTracking()
                        on a.Dokter.UserActiveId equals da.UserActiveId into daGroup
                    from da in daGroup.DefaultIfEmpty()

                    join bb in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                        on a.KunjunganID equals bb.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Beds.AsNoTracking()
                        on bb.BedId equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kamars.AsNoTracking()
                        on bb.KamarId equals k.KamarId into kamarGroup
                    from k in kamarGroup.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass.AsNoTracking()
                        on k.KelasId equals kl.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    join sp in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                        on a.KunjunganID equals sp.KunjunganId into suratGroup
                    from sp in suratGroup.DefaultIfEmpty()

                    where a.KunjunganID == id && a.IsDelete == false

                    select new
                    {
                        a.KunjunganID,

                        // =====================
                        // ASURANSI UTAMA
                        // =====================
                        a.AsuransiId,
                        a.AsuransiPasienId,

                        NamaAsuransi = a.Asuransi != null && a.Asuransi.NamaAsuransi != null
                            ? a.Asuransi.NamaAsuransi
                            : null,

                        IsUtama = a.AsuransiPasien != null && a.AsuransiPasien.IsUtama != null
                            ? a.AsuransiPasien.IsUtama
                            : false,

                        NoPolis = a.AsuransiPasien != null
                            ? a.AsuransiPasien.NoPolis
                            : null,
                        a.NoRegistrasi,

                        // =====================
                        // ASURANSI EXCESS
                        // =====================
                        a.AsuransiExcessId,
                        a.AsuransiPasienExcessId,

                        NamaAsuransiExcess = a.AsuransiExcess != null && a.AsuransiExcess.NamaAsuransi != null
                            ? a.AsuransiExcess.NamaAsuransi
                            : null,

                        IsUtamaExcess = a.AsuransiPasienExcess != null && a.AsuransiPasienExcess.IsUtama != null
                            ? a.AsuransiPasienExcess.IsUtama
                            : false,

                        NoPolisExcess = a.AsuransiPasienExcess != null
                            ? a.AsuransiPasienExcess.NoPolis
                            : null,

                        IsExcess = a.AsuransiPasienExcess != null
                            ? a.AsuransiPasienExcess.IsExcess
                            : null,

                        // =====================
                        // POLIKLINIK
                        // =====================
                        a.PoliklinikId,

                        NamaPoliklinik = a.Poliklinik != null
                            ? a.Poliklinik.NamaPoliklinik
                            : null,

                        // =====================
                        // DOKTER
                        // =====================
                        a.DokterId,

                        NamaDokter = a.Dokter != null
                            ? a.Dokter.NmDokter
                            : null,

                        // =====================
                        // PASIEN
                        // =====================
                        a.PasienId,
                        a.AsalKunjungan,

                        NamaPasien = a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                        TanggalLahir = a.Pasien != null
                            ? a.Pasien.TanggalLahir
                            : null,

                        JenisKelamin = a.Pasien != null
                            ? a.Pasien.JenisKelamin
                            : null,

                        NoPasien = a.Pasien != null
                            ? a.Pasien.NoPasien
                            : null,

                        NoWali1 = a.Pasien != null
                            ? a.Pasien.NoWali1
                            : null,

                        NoWali2 = a.Pasien != null
                            ? a.Pasien.NoWali2
                            : null,

                        NamaWali1 = a.Pasien != null
                            ? a.Pasien.NamaWali1
                            : null,

                        NamaWali2 = a.Pasien != null
                            ? a.Pasien.NamaWali2
                            : null,

                        NamaKontakDarurat = a.Pasien != null
                            ? a.Pasien.NamaKontakDarurat
                            : null,

                        NoTeleponDarurat = a.Pasien != null
                            ? a.Pasien.NoTeleponDarurat
                            : null,

                        Email = a.Pasien != null
                            ? a.Pasien.Email
                            : null,

                        AlamatDomisili = a.Pasien != null
                            ? a.Pasien.AlamatDomisili
                            : null,

                        AlamatDarurat = a.Pasien != null
                            ? a.Pasien.AlamatDarurat
                            : null,

                        AlamatIdentitas = a.Pasien != null
                            ? a.Pasien.AlamatIdentitas
                            : null,

                        Umur = a.Pasien != null
                            ? HitungUmurLengkap(a.Pasien.TanggalLahir)
                            : null,

                        // =====================
                        // DATA KUNJUNGAN
                        // =====================
                        a.NoRekamMedis,
                        a.TipePasien,
                        a.TipePembayaran,
                        a.JenisKunjungan,
                        a.StatusPengkajian,
                        a.CreateDateTime,
                        a.CreateBy,
                        a.IsFinished,
                        a.TglFinishedKasir,
                        a.IsScreening,
                        a.IsPresent,
                        a.IsTriage,
                        a.IsClosed,
                        a.IsCTTPasienIGD,
                        a.Antrian,

                        TglMasukKunjungan = a.TglMasuk,

                        a.CaraMasukRS,
                        a.KondisiKeluar,
                        a.IsFinishedKasir,

                        FotoPath = da != null
                            ? da.FotoPath
                            : null,

                        FotoName = da != null
                            ? da.FotoName
                            : null,

                        CreateByName = u != null
                            ? u.FullName
                            : null,

                        // =====================
                        // BOOKING BED / RANAP
                        // =====================
                        KelasId = kl != null
                            ? (Guid?)kl.KelasId
                            : null,

                        BookingBedRanapId = bb != null
                            ? (Guid?)bb.BookingBedRanapId
                            : null,

                        KamarId = bb != null
                            ? bb.KamarId
                            : null,

                        KamarNama = k != null
                            ? k.NamaKamar
                            : null,

                        LantaiKamar = k != null
                            ? k.Lantai
                            : null,

                        KelasNama = kl != null
                            ? kl.NamaKelas
                            : null,

                        BedId = bb != null
                            ? bb.BedId
                            : null,

                        NomorKamar = bb != null
                            ? bb.NoKamar
                            : null,

                        NomorBed = b != null
                            ? b.NomorBed
                            : null,

                        StatusBed = bb != null
                            ? bb.StatusBed
                            : null,

                        Keterangan = bb != null
                            ? bb.Keterangan
                            : null,

                        TglKeluar = bb != null
                            ? bb.TglKeluar
                            : null,

                        TglMasuk = bb != null
                            ? bb.TglMasuk
                            : null,

                        // =====================
                        // SURAT PENGANTAR RANAP
                        // =====================
                        NomorSuratPengantar = sp != null
                            ? sp.NomorSuratPengantar
                            : null,

                        Diagnosa = sp != null
                            ? sp.Diagnosa
                            : null,

                        AsalUnit = sp != null
                            ? sp.AsalUnit
                            : null
                    }
                ).FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });
                }

                return Ok(new
                {
                    status = "success",
                    message = "Data kunjungan berhasil diambil",
                    data = new
                    {
                        result,
                        JumlahJenisKunjungan = jumlahPerJenis,
                        Alergic = alergiList
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateKunjunganPasien(
            [FromBody] KunjunganViewModel request,
            CancellationToken ct)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                var userActiveId = getUserActive?.UserActiveId ?? Guid.Empty;

                if (userActiveId == Guid.Empty)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }
                    .Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'."
                    });
                }

                // Validasi jenis kunjungan
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) ||
                                 request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }
                    .Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'."
                    });
                }

                string kodeJenis = inputJenis.Equals("Rawat Inap", StringComparison.OrdinalIgnoreCase)
                    ? "IP"
                    : "OP";

                if (kodeJenis == "IP" &&
                    (request.DepositRanap == null || request.DepositRanap <= 0))
                {
                    return BadRequest(new
                    {
                        message = "Kunjungan IP (rawat inap) wajib mengisi nominal deposit."
                    });
                }

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // =============================
                // Cek apakah pasien masih punya kunjungan aktif
                // =============================
                bool isAlreadyRegistered;

                if (kodeJenis == "OP")
                {
                    isAlreadyRegistered = await _applicationDbContext.Kunjungans.AnyAsync(k =>
                        k.PasienId == request.PasienId &&
                        k.PoliklinikId == request.PoliklinikId &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.IsFinishedKasir == false &&
                        k.JenisKunjungan == "OP" &&
                        k.CreateDateTime >= today &&
                        k.CreateDateTime < tomorrow,
                        ct);
                }
                else
                {
                    isAlreadyRegistered = await _applicationDbContext.Kunjungans.AnyAsync(k =>
                        k.PasienId == request.PasienId &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.IsFinishedKasir == false &&
                        k.JenisKunjungan == "IP",
                        ct);
                }

                if (isAlreadyRegistered)
                {
                    return BadRequest(new
                    {
                        message = "Pasien sudah terdaftar untuk kunjungan aktif yang belum selesai."
                    });
                }

                // =============================
                // Penentuan Nomor Antrean
                // =============================
                string? nomorAntrianFormatted = null;
                string? kodePoli = null;

                if (!string.Equals(request.AsalKunjungan?.Trim(), "igd", StringComparison.OrdinalIgnoreCase))
                {
                    if (request.PoliklinikId == null || request.PoliklinikId == Guid.Empty)
                    {
                        return BadRequest(new
                        {
                            message = "Poliklinik wajib dipilih untuk kunjungan non-IGD."
                        });
                    }

                    kodePoli = await _applicationDbContext.Polikliniks
                        .Where(p => p.PoliklinikId == request.PoliklinikId)
                        .Select(p => p.KodeAntreanPoli)
                        .FirstOrDefaultAsync(ct);

                    if (string.IsNullOrEmpty(kodePoli))
                    {
                        return BadRequest(new
                        {
                            message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!"
                        });
                    }

                    var jumlahAntrianHariIni = await _applicationDbContext.Kunjungans
                        .CountAsync(k =>
                            k.PoliklinikId == request.PoliklinikId &&
                            k.CreateDateTime >= today &&
                            k.CreateDateTime < tomorrow &&
                            !k.IsDelete,
                            ct);

                    int nomorAntrian = jumlahAntrianHariIni + 1;
                    nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}";
                }

                // =============================
                // Generate ID unik untuk kunjungan
                // =============================
                Guid newKunjunganId;
                int attempt = 0;

                do
                {
                    newKunjunganId = Guid.NewGuid();
                    attempt++;
                }
                while (await _applicationDbContext.Kunjungans
                           .AnyAsync(k => k.KunjunganID == newKunjunganId, ct)
                       && attempt < 5);

                if (await _applicationDbContext.Kunjungans
                        .AnyAsync(k => k.KunjunganID == newKunjunganId, ct))
                {
                    return StatusCode(500, new
                    {
                        message = "Gagal membuat KunjunganID unik. Silakan coba lagi."
                    });
                }

                await using var trx = await _applicationDbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, ct);

                try
                {
                    var noRegistrasi = await _kunjunganNoRegistrasiService
                        .GenerateNoRegistrasiAsync(ct);
                    // =============================
                    // Simpan data kunjungan
                    // =============================
                    var newKunjungan = new Kunjungan
                    {
                        KunjunganID = newKunjunganId,
                        PasienId = request.PasienId,
                        DokterId = request.DokterId,
                        PoliklinikId = request.PoliklinikId,
                        AsuransiId = request.AsuransiId,
                        AsuransiPasienId = request.AsuransiPasienId,
                        AsuransiExcessId = request.AsuransiExcessId,
                        JenisKunjungan = kodeJenis,
                        NoRegistrasi = noRegistrasi,
                        NoRekamMedis = request.NoRekamMedis,
                        TipePasien = request.TipePasien,
                        TipePembayaran = request.TipePembayaran,
                        IsFinished = false,
                        IsDelete = false,
                        IsScreening = false,
                        IsPresent = true,
                        IsFinishedKasir = false,
                        IsTriage = false,
                        IsCTTPasienIGD = false,
                        IsClosed = false,
                        Antrian = nomorAntrianFormatted,
                        AsalKunjungan = request.AsalKunjungan,
                        TglMasuk = request.TglMasuk,
                        CaraMasukRS = request.CaraMasukRS,
                        KondisiKeluar = request.KondisiKeluar,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId,
                    };

                    _applicationDbContext.Kunjungans.Add(newKunjungan);

                    await _applicationDbContext.SaveChangesAsync(ct);

                    // =============================================
                    // Deposit wajib untuk kunjungan IP / Rawat Inap
                    // =============================================
                    if (kodeJenis == "IP")
                    {
                        var noKwitansi = await _depositRanapNumberService.GenerateNoKwitansiAsync();

                        var depo = new DepositRanap
                        {
                            DepositRanapId = Guid.NewGuid(),
                            KunjunganId = newKunjungan.KunjunganID,
                            TglTransaksi = DateTime.UtcNow,
                            NominalMasuk = request.DepositRanap,
                            SaldoDeposit = request.DepositRanap,
                            NoKwitansi = noKwitansi,
                            StatusDeposit = "Pemasukkan",
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = userActiveId
                        };

                        _applicationDbContext.DepositRanaps.Add(depo);

                        await _kunjunganAdminBillingService.ApplyBillingAdmisiRanapBaruAsync(
                                newKunjungan.KunjunganID,
                                userActiveId,
                                ct
                        );
                    }

                    var noBill = await _noBillService.GenerateNoBillAsync(
                        newKunjungan.KunjunganID,
                        ct
                    );

                    var kasir = new MainKasir
                    {
                        KasirId = Guid.NewGuid(),
                        KunjunganId = newKunjungan.KunjunganID,
                        StatusPembayaran = "Belum Lunas",
                        NoBill = noBill,
                        CreateDateTime = DateTime.UtcNow
                    };

                    _applicationDbContext.MainKasirs.Add(kasir);

                    await _applicationDbContext.SaveChangesAsync(ct);
                    await trx.CommitAsync(ct);

                    // =============================
                    // Kirim notifikasi setelah commit berhasil
                    // =============================
                    await _hubContext.Clients.All.SendAsync("Kunjungan ditambah", new
                    {
                        action = "create",
                        kunjunganId = newKunjungan.KunjunganID,
                        pasienId = request.PasienId,
                        dokterId = request.DokterId,
                        kasirId = kasir.KasirId,
                        NomorAntrian = nomorAntrianFormatted
                    }, ct);

                    return Ok(new
                    {
                        message = "Kunjungan baru berhasil ditambahkan.",
                        data = new
                        {
                            request.PasienId,
                            request.DokterId,
                            newKunjungan.KunjunganID,
                            JenisKunjungan = inputJenis,
                            NomorAntrian = nomorAntrianFormatted ?? "Tanpa antrean (IGD)",
                            kasirId = kasir.KasirId
                        }
                    });
                }
                catch
                {
                    await trx.RollbackAsync(ct);
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan: {ex.Message}"
                });
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> CreateKunjunganPasien([FromBody] KunjunganViewModel request)
        //{
        //    if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
        //    {
        //        return BadRequest(new { message = "Data tidak boleh kosong!" });
        //    }

        //    try
        //    {
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
        //        var UserActiveId = GetUserActive?.UserActiveId ?? Guid.Empty;

        //        // Validasi tipe pasien
        //        if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
        //        {
        //            return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
        //        }

        //        // Validasi jenis kunjungan
        //        // jika tidak diisi automatis "Rawat Jalan"
        //        var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) || request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
        //            ? "Rawat Jalan"
        //            : request.JenisKunjungan;

        //        if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
        //        {
        //            return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
        //        }

        //        string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";


        //        // Ambil kode antrean dari tabel Poliklinik
        //        var kodePoli = _applicationDbContext.Polikliniks
        //            .Where(p => p.PoliklinikId == request.PoliklinikId)
        //            .Select(p => p.KodeAntreanPoli)
        //            .FirstOrDefault();

        //        if (string.IsNullOrEmpty(kodePoli))
        //            return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

        //        // Hitung nomor antrian hari ini berdasarkan Poliklinik
        //        var today = DateTime.UtcNow.Date;
        //        var isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
        //            //k.PoliklinikId == request.PoliklinikId &&
        //            //k.DokterId == request.DokterId &&
        //            k.PasienId == request.PasienId &&
        //            //k.CreateDateTime.Date == today &&
        //            !k.IsDelete && k.IsFinished == false);

        //        if (isAlreadyRegistered)
        //        {
        //            return BadRequest(new { message = "Pasien sudah terdaftar untuk kunjungan dengan poli dan dokter yang sama pada hari ini." });
        //        }

        //        var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
        //            .Count(k => k.PoliklinikId == request.PoliklinikId
        //                        && k.CreateDateTime.Date == today
        //                        && !k.IsDelete);

        //        int nomorAntrian = jumlahAntrianHariIni + 1;
        //        string nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}"; // Contoh: BU001

        //        var newKunjungan = new Kunjungan
        //        {
        //            KunjunganID = Guid.NewGuid(),
        //            PasienId = request.PasienId,
        //            DokterId = request.DokterId,
        //            PoliklinikId = request.PoliklinikId,
        //            AsuransiId = request.AsuransiId,
        //            //JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan),
        //            JenisKunjungan = kodeJenis,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //            CreateBy = UserActiveId,
        //            NoRekamMedis = request.NoRekamMedis,
        //            TipePasien = request.TipePasien,
        //            TipePembayaran = request.TipePembayaran,
        //            IsFinished = false,
        //            IsDelete = false,
        //            IsScreening = false,
        //            IsPresent = true,
        //            IsFinishedKasir = false, // Default value
        //            Antrian = nomorAntrianFormatted   // Format akhir: BU001
        //        };

        //        // validasi supaya ga ada kunjunganId yang sama
        //        _applicationDbContext.Kunjungans.Add(newKunjungan);

        //        // cari data biaya admin berdasarkan jenis kunjungan
        //        var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
        //            .Where(b => b.BiayaAdministrasiKode == kodeJenis)
        //            .FirstOrDefaultAsync();

        //        // Hitung jumlah billing kunjungan sebelumnya
        //        int billingKunjunganCount = await _applicationDbContext.Billings
        //            .Where(b => b.KunjunganId == newKunjungan.KunjunganID && b.BillingKode.ToLower() == "Biaya Admin")
        //            .CountAsync();
        //        int billingIndex = billingKunjunganCount;
        //        // increment billoing kode untuk setiap kunjunga
        //        billingIndex++;
        //        string billingKode = $"{billingIndex.ToString("D3")}";

        //        var bill = new Billing
        //        {
        //            BillingId = Guid.NewGuid(),
        //            KunjunganId = newKunjungan.KunjunganID,
        //            DiskonId = null, // Atur sesuai kebutuhan
        //            ItemId = biayaAdmin?.BiayaAdministrasiId ?? Guid.Empty,
        //            NamaItem = biayaAdmin?.NamaBiayaAdministrasi ?? "Biaya Administrasi",
        //            HargaItem = biayaAdmin?.NominalBiayaAdministrasi ?? 0,
        //            QtyItem = 1,
        //            SubTotalItem = biayaAdmin?.NominalBiayaAdministrasi ?? 0,
        //            BillingKode = billingKode,
        //            JenisBilling = "Biaya Admin",
        //            BillingDate = DateTime.UtcNow,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //            CreateBy = UserActiveId
        //        };
        //        _applicationDbContext.Billings.Add(bill);

        //        await _applicationDbContext.SaveChangesAsync();
        //        await _hubContext.Clients.All.SendAsync("Kunjungan ditambah", new
        //        {
        //            action = "create",
        //            kunjunganId = newKunjungan.KunjunganID,
        //            pasienId = request.PasienId,
        //            dokterId = request.DokterId,
        //            NomorAntrian = nomorAntrianFormatted
        //        });


        //        return Ok(new
        //        {
        //            message = "Kunjungan baru berhasil ditambahkan.",
        //            data = new
        //            {
        //                request.PasienId,
        //                request.DokterId,
        //                newKunjungan.KunjunganID,
        //                JenisKunjungan = inputJenis,
        //                NomorAntrian = nomorAntrianFormatted
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
        //    }
        //}
        //test
        [HttpPost("broadcast-default")]
        public async Task<IActionResult> BroadcastDefault()
        {
            await _hubContext.Clients.All.SendAsync("Kunjungan ditambah", new
            {
                action = "create",
                kunjunganId = 999,
                pasienId = 1001,
                dokterId = 2002
            });

            return Ok(new { message = "✅ Broadcast default berhasil." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKunjunganPasien(Guid id, [FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
                return BadRequest(new { message = "Data tidak boleh kosong!" });

            try
            {
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(EmailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var GetUserActive = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive?.UserActiveId ?? Guid.Empty;

                var existing = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(k => k.KunjunganID == id && (k.IsDelete == false || k.IsDelete == null));

                if (existing == null)
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });

                // ========================================
                // 🩺 Validasi tipe pasien
                // ========================================
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });

                // ========================================
                // 🏥 Validasi jenis kunjungan
                // ========================================
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) ||
                                 request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";
                var today = DateTime.UtcNow.Date;

                // ========================================
                // 🔎 Cek kunjungan aktif duplikat
                // ========================================
                bool isAlreadyRegistered = false;

                if (kodeJenis == "OP")
                {
                    isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
                        k.PasienId == request.PasienId &&
                        k.PoliklinikId == request.PoliklinikId &&
                        k.KunjunganID != id &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.JenisKunjungan == "OP" &&
                        k.CreateDateTime.Date == today);
                }
                else if (kodeJenis == "IP")
                {
                    isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
                        k.PasienId == request.PasienId &&
                        k.KunjunganID != id &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.JenisKunjungan == "IP");
                }

                if (isAlreadyRegistered)
                    return BadRequest(new { message = "Pasien sudah terdaftar untuk kunjungan aktif yang belum selesai." });

                // ========================================
                // 🩺 Penentuan nomor antrean
                // ========================================
                string nomorAntrianFormatted = existing.Antrian; // default: pakai antrean lama
                string kodePoli = null;

                // Hanya generate antrean baru jika AsalKunjungan bukan IGD
                if (!string.Equals(request.AsalKunjungan?.Trim(), "igd", StringComparison.OrdinalIgnoreCase))
                {
                    if (request.PoliklinikId == null || request.PoliklinikId == Guid.Empty)
                        return BadRequest(new { message = "Poliklinik wajib dipilih untuk kunjungan non-IGD." });

                    kodePoli = _applicationDbContext.Polikliniks
                        .Where(p => p.PoliklinikId == request.PoliklinikId)
                        .Select(p => p.KodeAntreanPoli)
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(kodePoli))
                        return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                    var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                        .Count(k => k.PoliklinikId == request.PoliklinikId &&
                                    k.CreateDateTime.Date == today &&
                                    !k.IsDelete);

                    int nomorAntrian = jumlahAntrianHariIni + 1;
                    nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}";
                }
                else
                {
                    nomorAntrianFormatted = null; // IGD tidak pakai antrean
                }

                // ========================================
                // 💾 Update data kunjungan
                // ========================================
                existing.PasienId = request.PasienId;
                existing.DokterId = request.DokterId;
                existing.PoliklinikId = request.PoliklinikId;
                existing.AsuransiExcessId = request.AsuransiExcessId;
                existing.AsuransiId = request.AsuransiId;
                existing.AsuransiPasienId = request.AsuransiPasienId;
                existing.JenisKunjungan = kodeJenis;
                existing.NoRekamMedis = request.NoRekamMedis;
                existing.TipePasien = request.TipePasien;
                existing.TipePembayaran = request.TipePembayaran;
                existing.AsalKunjungan = request.AsalKunjungan;
                existing.Antrian = nomorAntrianFormatted;
                existing.TglMasuk = request.TglMasuk;
                existing.CaraMasukRS = request.CaraMasukRS;
                existing.KondisiKeluar = request.KondisiKeluar;

                existing.UpdateDateTime = DateTimeOffset.UtcNow;
                existing.UpdateBy = UserActiveId;

                _applicationDbContext.Kunjungans.Update(existing);

                // ========================================
                // 💰 Update atau tambahkan biaya administrasi
                // ========================================
                var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
                    .Where(b => b.BiayaAdministrasiKode == kodeJenis)
                    .FirstOrDefaultAsync();

                if (biayaAdmin != null)
                {
                    var existingBill = await _applicationDbContext.Billings
                        .FirstOrDefaultAsync(b => b.KunjunganId == existing.KunjunganID && b.JenisBilling == "Biaya Admin");

                    if (existingBill != null)
                    {
                        existingBill.ItemId = biayaAdmin.BiayaAdministrasiId;
                        existingBill.NamaItem = biayaAdmin.NamaBiayaAdministrasi;
                        existingBill.HargaItem = biayaAdmin.NominalBiayaAdministrasi;
                        existingBill.SubTotalItem = biayaAdmin.NominalBiayaAdministrasi;
                        existingBill.UpdateDateTime = DateTimeOffset.UtcNow;
                        existingBill.UpdateBy = UserActiveId;
                        _applicationDbContext.Billings.Update(existingBill);
                    }
                    else
                    {
                        var newBill = new Billing
                        {
                            BillingId = Guid.NewGuid(),
                            KunjunganId = existing.KunjunganID,
                            ItemId = biayaAdmin.BiayaAdministrasiId,
                            NamaItem = biayaAdmin.NamaBiayaAdministrasi,
                            HargaItem = biayaAdmin.NominalBiayaAdministrasi,
                            QtyItem = 1,
                            SubTotalItem = biayaAdmin.NominalBiayaAdministrasi,
                            BillingKode = "001",
                            JenisBilling = "Biaya Admin",
                            StatusBilling = false,
                            TanggalInvoice = DateTime.UtcNow,
                            TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                            BillingDate = DateTime.UtcNow,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId
                        };
                        _applicationDbContext.Billings.Add(newBill);
                    }
                }

                await _applicationDbContext.SaveChangesAsync();

                // ========================================
                // 🔔 Kirim notifikasi SignalR
                // ========================================
                await _hubContext.Clients.All.SendAsync("Kunjungan diupdate", new
                {
                    action = "update",
                    kunjunganId = existing.KunjunganID,
                    pasienId = existing.PasienId,
                    dokterId = existing.DokterId,
                    NomorAntrian = nomorAntrianFormatted
                });

                return Ok(new
                {
                    message = "Kunjungan berhasil diperbarui.",
                    data = new
                    {
                        existing.KunjunganID,
                        existing.PasienId,
                        existing.DokterId,
                        existing.PoliklinikId,
                        existing.AsuransiId,
                        JenisKunjungan = inputJenis,
                        NomorAntrian = nomorAntrianFormatted ?? "Tanpa antrean (IGD)"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}/is-finished")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] UpdateIsFinishedViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsFinished = request.IsFinished;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsFinishedChanged", new
            {
                action = "updateIsFinished",
                kunjunganId = kunjungan.KunjunganID,
                isFinished = request.IsFinished
            });

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/is-closed")]
        public async Task<IActionResult> UpdateIsClosed(Guid id, [FromBody] UpdateIsClosedViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsClosed = request.IsClosed;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsClosedChanged", new
            {
                action = "updateIsClosed",
                kunjunganId = kunjungan.KunjunganID,
                request.IsClosed
            });

            return Ok(new { message = "Status IsClosed berhasil diperbarui." });
        }

        [HttpPut("{id}/is-screening")]
        public async Task<IActionResult> UpdateIsScreening(Guid id, [FromBody] UpdateIsScreeningViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsScreening = request.IsScreening;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsScreeningChanged", new
            {
                action = "updateIsScreening",
                kunjunganId = kunjungan.KunjunganID,
                isScreening = request.IsScreening
            });

            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpPut("{id}/is-present")]
        public async Task<IActionResult> UpdateIsPresent(Guid id, [FromBody] UpdateIsPresentViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsPresent = request.IsPresent;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsPresentChanged", new
            {
                action = "updateIsPresent",
                kunjunganId = kunjungan.KunjunganID,
                isPresent = request.IsPresent
            });

            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpPut("{id}/is-finishedKasir")]
        public async Task<IActionResult> UpdateIsFinishedKasir(Guid id, [FromBody] UpdateIsFinishedKasirViewModel request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsFinishedKasir = request.IsFinishedKasir;
            kunjungan.TglFinishedKasir = DateTime.UtcNow;

            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsFinishedKasirChanged", new
            {
                action = "updateIsFinishedKasir",
                kunjunganId = kunjungan.KunjunganID,
                isFinishedKasir = request.IsFinishedKasir
            });
            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpPut("{id}/StatusPengkajian")]
        public async Task<IActionResult> UpdateStatusPengkajian(Guid id, [FromBody] StatusPengkajianVM request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.StatusPengkajian = request.Status;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;

            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("StatusPengkajianChanged", new
            {
                action = "updateStatusPengkajian",
                kunjunganId = kunjungan.KunjunganID,
                StatusPengkajian = request.Status
            });
            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
        }

        [HttpPut("{id}/is-Triage")]
        public async Task<IActionResult> UpdateIsTriage(Guid id, [FromBody] UpdateStatusKunjungan request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsTriage = request.Status;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("isTriage Changed", new
            {
                action = "updateIsCTTPasienIGD",
                kunjunganId = kunjungan.KunjunganID,
                isTriage = request.Status
            });

            return Ok(new { message = "Status isTriage berhasil diperbarui." });
        }

        [HttpPut("{id}/is-CTTPasienIGD")]
        public async Task<IActionResult> UpdateIsCTTPasienIGD(Guid id, [FromBody] UpdateStatusKunjungan request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsCTTPasienIGD = request.Status;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("IsCTTPasienIGD Changed", new
            {
                action = "updateIsCTTPasienIGD",
                kunjunganId = kunjungan.KunjunganID,
                IsCTTPasienIGD = request.Status
            });

            return Ok(new { message = "Status IsCTTPasienIGD berhasil diperbarui." });
        }

        [HttpPut("{id}/Status-TransferPasien")]
        public async Task<IActionResult> UpdateStatusTransferPasien(Guid id, [FromBody] StatusTransferPasienVM request)
        {
            var kunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
            if (kunjungan == null)
                return NotFound(new { message = "Kunjungan tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.AsalKunjungan = request.AsalKunjungan;
            kunjungan.JenisKunjungan = request.JenisKunjungan;
            kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
            kunjungan.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("Status-TransferPasien Changed", new
            {
                action = "updateStatus-TransferPasien",
                kunjunganId = kunjungan.KunjunganID,
            });

            return Ok(new { message = "Status TransferPasien berhasil diperbarui." });
        }

        [HttpPut("{id}/Ubah-Asuransi")]
        public async Task<IActionResult> UpdateAsuransiKunjungan(
            Guid id,
            [FromBody] UbahAsuransiViewModel vm,
            CancellationToken ct)
        {
            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var userActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (userActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var kunjungan = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(x =>
                        x.KunjunganID == id &&
                        !x.IsDelete,
                        ct);

                if (kunjungan == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });
                }

                if (!kunjungan.PasienId.HasValue || kunjungan.PasienId.Value == Guid.Empty)
                {
                    return BadRequest(new { message = "PasienId pada kunjungan tidak valid." });
                }

                // =====================================================
                // VALIDASI BATAS WAKTU UBAH ASURANSI
                // =====================================================
                var tanggalKunjungan = GetTanggalKunjungan(kunjungan);

                if (!tanggalKunjungan.HasValue)
                {
                    return BadRequest(new
                    {
                        message = "Tanggal kunjungan tidak ditemukan, perubahan asuransi tidak dapat dilakukan."
                    });
                }

                var tanggalKunjunganDate = tanggalKunjungan.Value.Date;

                // Boleh ubah sampai H+2 jam 23:59:59
                var batasAkhirPerubahan = tanggalKunjunganDate
                    .AddDays(2)
                    .AddDays(1)
                    .AddTicks(-1);

                var now = DateTime.Now;

                if (now > batasAkhirPerubahan)
                {
                    return BadRequest(new
                    {
                        alert = true,
                        message = "Perubahan asuransi hanya dapat dilakukan maksimal 2 hari dari tanggal kunjungan.",
                        tanggalKunjungan = tanggalKunjunganDate,
                        batasAkhirPerubahan,
                        waktuSekarang = now
                    });
                }

                var oldAsuransiId = kunjungan.AsuransiId;
                var oldAsuransiPasienId = kunjungan.AsuransiPasienId;
                var oldTipePembayaran = kunjungan.TipePembayaran;

                Guid? finalAsuransiPasienId = null;

                // =====================================================
                // JIKA ASURANSI NULL / EMPTY => UBAH KE MANDIRI
                // =====================================================
                if (!vm.AsuransiId.HasValue || vm.AsuransiId.Value == Guid.Empty)
                {
                    kunjungan.AsuransiId = null;
                    kunjungan.AsuransiPasienId = null;
                    kunjungan.TipePembayaran = "Mandiri";
                }
                else
                {
                    // =====================================================
                    // VALIDASI ASURANSI MASTER
                    // =====================================================
                    var asuransi = await _applicationDbContext.Asuransis
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.AsuransiId == vm.AsuransiId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (asuransi == null)
                    {
                        return BadRequest(new
                        {
                            message = "Asuransi tidak ditemukan atau sudah tidak aktif."
                        });
                    }

                    // =====================================================
                    // CEK APAKAH PASIEN SUDAH TERDAFTAR DI ASURANSI PASIEN
                    // =====================================================
                    var asuransiPasien = await _applicationDbContext.AsuransiPasiens
                        .FirstOrDefaultAsync(x =>
                            x.PasienId == kunjungan.PasienId.Value &&
                            x.AsuransiId == vm.AsuransiId.Value &&
                            x.NoPolis == vm.NoPolis &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (asuransiPasien == null)
                    {
                        // =====================================================
                        // JIKA BELUM ADA, DAFTARKAN KE ASURANSI PASIEN
                        // =====================================================
                        asuransiPasien = new AsuransiPasien
                        {
                            AsuransiPasienId = Guid.NewGuid(),
                            PasienId = kunjungan.PasienId.Value,
                            AsuransiId = vm.AsuransiId.Value,
                            NoPolis = vm.NoPolis,

                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = userActive.UserActiveId,
                            IsDelete = false
                        };

                        _applicationDbContext.AsuransiPasiens.Add(asuransiPasien);
                    }

                    finalAsuransiPasienId = asuransiPasien.AsuransiPasienId;

                    kunjungan.AsuransiId = vm.AsuransiId.Value;
                    kunjungan.AsuransiPasienId = finalAsuransiPasienId;
                    kunjungan.TipePembayaran = "Asuransi";
                }

                kunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
                kunjungan.UpdateBy = userActive.UserActiveId;

                /*
                 * Save dulu supaya AsuransiCoverageService membaca AsuransiId terbaru
                 * dari tabel Kunjungan.
                 */
                await _applicationDbContext.SaveChangesAsync(ct);

                await _asuransiCoverageService.RefreshCoverageBillingByKunjunganAsync(
                    id,
                    userActive.UserActiveId,
                    ct
                );

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Ok(new
                {
                    message = "Perubahan asuransi kunjungan berhasil.",
                    data = new
                    {
                        kunjungan.KunjunganID,
                        kunjungan.PasienId,

                        before = new
                        {
                            AsuransiId = oldAsuransiId,
                            AsuransiPasienId = oldAsuransiPasienId,
                            TipePembayaran = oldTipePembayaran
                        },

                        after = new
                        {
                            kunjungan.AsuransiId,
                            kunjungan.AsuransiPasienId,
                            kunjungan.TipePembayaran
                        },

                        tanggalKunjungan = tanggalKunjunganDate,
                        batasAkhirPerubahan
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = $"Gagal mengubah asuransi: {dbEx.InnerException?.Message}"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);

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
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Dokter**
                var data = _applicationDbContext.Kunjungans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Kunjungans.Update(data);

                // Soft Delete Semua Billing Terkait
                var billings = _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == id && !b.IsDelete)
                    .ToList();

                foreach (var billing in billings)
                {
                    billing.IsDelete = true;
                    billing.DeleteBy = UserActiveId;
                    billing.DeleteDateTime = DateTimeOffset.UtcNow;
                    _applicationDbContext.Billings.Update(billing);
                }
                _applicationDbContext.SaveChanges();
                await _hubContext.Clients.All.SendAsync("Kunjungan dihapus", new
                {
                    action = "delete",
                    kunjunganId = id
                });

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        //[HttpGet("paged")]
        //public async Task<IActionResult> PagedKunjunganAsync(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null,
        //    [FromQuery] PeriodeFilter? periode = null,
        //    [FromQuery] bool? isFinished = null,
        //    [FromQuery] bool? isScreening = null,
        //    [FromQuery] bool? isPresent = null,
        //    [FromQuery] bool? isFinishedKasir = null,
        //    [FromQuery] TipePasienFilter? TipePasien = null,
        //    [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
        //    [FromQuery] string? AsalKunjungan = null,
        //    [FromQuery] Guid? dokterId = null
        //)
        //{
        //    try
        //    {
        //        // ✅ Ambil data alergi (anti N+1)
        //        var allAlergic = await _applicationDbContext.PainAssessments
        //            .Where(x => !x.IsDelete)
        //            .GroupBy(x => x.KunjunganId)
        //            .Select(g => new
        //            {
        //                KunjunganId = g.Key,
        //                AlergicList = g.Select(x => x.Alergic).Distinct().ToList()
        //            })
        //            .ToListAsync();

        //        // ✅ Hitung jumlah kunjungan per pasien per jenis
        //        var jumlahPerJenis = _applicationDbContext.Kunjungans
        //            .Where(k => !k.IsDelete)
        //            .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
        //            .Select(g => new
        //            {
        //                g.Key.PasienId,
        //                g.Key.JenisKunjungan,
        //                JumlahJenis = g.Count()
        //            });

        //        // ✅ Base query dengan LEFT JOIN (DefaultIfEmpty)
        //        var baseQuery =
        //            from a in _applicationDbContext.Kunjungans
        //            join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
        //            from u in userGroup.DefaultIfEmpty()

        //            join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId into poliGroup
        //            from p in poliGroup.DefaultIfEmpty()

        //            join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
        //            from o in asuransiGroup.DefaultIfEmpty()

        //            join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId into pasienGroup
        //            from ps in pasienGroup.DefaultIfEmpty()

        //            join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId into dokterGroup
        //            from d in dokterGroup.DefaultIfEmpty()

        //            join j in jumlahPerJenis on new { a.PasienId, a.JenisKunjungan } equals new { j.PasienId, j.JenisKunjungan }

        //            join bb in _applicationDbContext.BookingBedRanaps on a.KunjunganID equals bb.KunjunganId into bookingGroup
        //            from bb in bookingGroup.DefaultIfEmpty()

        //            join b in _applicationDbContext.Beds on bb.BedId equals b.BedId into bedGroup
        //            from b in bedGroup.DefaultIfEmpty()

        //            join k in _applicationDbContext.Kamars on bb.KamarId equals k.KamarId into kamarGroup
        //            from k in kamarGroup.DefaultIfEmpty()

        //            join kl in _applicationDbContext.Kelass on k.KelasId equals kl.KelasId into kelasGroup
        //            from kl in kelasGroup.DefaultIfEmpty()

        //            join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganID equals sp.KunjunganId into suratGroup
        //            from sp in suratGroup.DefaultIfEmpty()

        //            where a.IsDelete == false
        //            select new
        //            {
        //                a.KunjunganID,
        //                a.AsuransiId,
        //                NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai",
        //                a.PoliklinikId,
        //                NamaPoliklinik = p != null ? p.NamaPoliklinik : null,
        //                a.DokterId,
        //                NamaDokter = d != null ? d.NmDokter : null,
        //                a.PasienId,
        //                a.AsalKunjungan,
        //                NamaPasien = ps != null ? ps.NamaLengkap : null,
        //                ps.TanggalLahir,
        //                ps.JenisKelamin,
        //                ps.NoPasien,
        //                ps.NoWali2,
        //                ps.NoWali3,
        //                ps.NamaWali2,
        //                ps.NamaWali3,
        //                ps.NamaKontakDarurat,
        //                ps.NoTeleponDarurat,
        //                ps.Email,
        //                AlamatDomisili = ps != null ? ps.AlamatDomisili : null,
        //                AlamatDarurat = ps != null ? ps.AlamatDarurat : null,
        //                AlamatIdentitas = ps != null ? ps.AlamatIdentitas : null,
        //                Umur = ps != null ? HitungUmurLengkap(ps.TanggalLahir) : null,
        //                a.NoRekamMedis,
        //                a.TipePasien,
        //                a.TipePembayaran,
        //                a.JenisKunjungan,
        //                a.StatusPengkajian,
        //                a.CreateDateTime,
        //                a.CreateBy,
        //                a.IsFinished,
        //                a.IsScreening,
        //                a.IsPresent,
        //                a.IsTriage,
        //                a.IsCTTPasienIGD,
        //                a.Antrian,
        //                a.DepositRanap,
        //                TglMasukKunjungan = a.TglMasuk,
        //                a.CaraMasukRS,
        //                a.KondisiKeluar,
        //                a.IsFinishedKasir,
        //                d.NmDokter,
        //                gambardokter = !string.IsNullOrEmpty(d.FotoName)
        //                    ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
        //                    : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",
        //                CreateByName = u != null ? u.FullName : null,
        //                JumlahJenisKunjungan = j.JumlahJenis,
        //                BookingBedRanapId = bb != null ? (Guid?)bb.BookingBedRanapId : null,
        //                KelasId = kl != null ? (Guid?)kl.KelasId : null,
        //                KamarId = bb != null ? bb.KamarId : null,
        //                KamarNama = k != null ? k.NamaKamar : null,
        //                LantaiKamar = k != null ? k.Lantai : null,
        //                KelasNama = kl != null ? kl.NamaKelas : null,
        //                BedId = bb != null ? bb.BedId : null,
        //                NomorKamar = bb != null ? bb.NoKamar : null,
        //                NomorBed = b != null ? b.NomorBed : null,
        //                StatusBed = bb != null ? bb.StatusBed : null,
        //                Keterangan = bb != null ? bb.Keterangan : null,
        //                TglKeluar = bb != null ? bb.TglKeluar : null,
        //                TglMasuk = bb != null ? bb.TglMasuk : null,
        //                NomorSuratPengantar = sp != null ? sp.NomorSuratPengantar : null,
        //                Diagnosa = sp != null ? sp.Diagnosa : null,
        //                AsalUnit = sp != null ? sp.AsalUnit : null
        //            };

        //        // ✅ Filter dinamis
        //        if (isFinished.HasValue) baseQuery = baseQuery.Where(u => u.IsFinished == isFinished.Value);
        //        if (isPresent.HasValue) baseQuery = baseQuery.Where(u => u.IsPresent == isPresent.Value);
        //        if (isScreening.HasValue) baseQuery = baseQuery.Where(u => u.IsScreening == isScreening.Value);
        //        if (isFinishedKasir.HasValue) baseQuery = baseQuery.Where(u => u.IsFinishedKasir == isFinishedKasir.Value);
        //        if (TipePasien.HasValue) baseQuery = baseQuery.Where(u => u.TipePasien == TipePasien.Value.ToString());
        //        if (JenisKunjungan.HasValue) baseQuery = baseQuery.Where(u => u.JenisKunjungan == JenisKunjungan.Value.ToString());
        //        if (dokterId.HasValue) baseQuery = baseQuery.Where(u => u.DokterId == dokterId.Value);

        //        // ✅ Filter tanggal
        //        if (startDate.HasValue && endDate.HasValue)
        //        {
        //            DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //            DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
        //            baseQuery = baseQuery.Where(u => u.CreateDateTime >= startUtc && u.CreateDateTime <= endUtc);
        //        }

        //        // ✅ Filter asal kunjungan
        //        if (!string.IsNullOrWhiteSpace(AsalKunjungan))
        //        {
        //            string pattern = $"%{AsalKunjungan.ToLower()}%";
        //            baseQuery = baseQuery.Where(u =>
        //                EF.Functions.ILike(u.AsalKunjungan, pattern));
        //        }

        //        // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
        //        if (periode.HasValue)
        //        {
        //            DateTime today = DateTime.UtcNow.Date;

        //            switch (periode)
        //            {
        //                case PeriodeFilter.Today:
        //                    baseQuery = baseQuery.Where(u => u.CreateDateTime.Date == today);
        //                    break;
        //                case PeriodeFilter.ThisWeek:
        //                    baseQuery = baseQuery.Where(u =>
        //                        u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
        //                        u.CreateDateTime.Date <= today
        //                    );
        //                    break;
        //                case PeriodeFilter.LastWeek:
        //                    baseQuery = baseQuery.Where(u =>
        //                        u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
        //                        u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
        //                    );
        //                    break;
        //                case PeriodeFilter.ThisMonth:
        //                    baseQuery = baseQuery.Where(u =>
        //                        u.CreateDateTime.Month == today.Month &&
        //                        u.CreateDateTime.Year == today.Year
        //                    );
        //                    break;
        //                case PeriodeFilter.LastMonth:
        //                    baseQuery = baseQuery.Where(u =>
        //                        u.CreateDateTime.Month == today.Month - 1 &&
        //                        u.CreateDateTime.Year == today.Year
        //                    );
        //                    break;
        //                case PeriodeFilter.ThisYear:
        //                    baseQuery = baseQuery.Where(u => u.CreateDateTime.Year == today.Year);
        //                    break;
        //                case PeriodeFilter.LastYear:
        //                    baseQuery = baseQuery.Where(u => u.CreateDateTime.Year == today.Year - 1);
        //                    break;
        //                case PeriodeFilter.Last3Months:
        //                    baseQuery = baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-3));
        //                    break;
        //                case PeriodeFilter.Last6Months:
        //                    baseQuery = baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-6));
        //                    break;
        //            }
        //        }

        //        // ✅ Filter pencarian
        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            string pattern = $"%{search.ToLower()}%";
        //            baseQuery = baseQuery.Where(u =>
        //                EF.Functions.ILike(u.NamaPasien, pattern) ||
        //                EF.Functions.ILike(u.NmDokter, pattern) ||
        //                EF.Functions.ILike(u.NoRekamMedis, pattern) ||
        //                EF.Functions.ILike(u.NamaPoliklinik, pattern) ||
        //                EF.Functions.ILike(u.Antrian, pattern));
        //        }

        //        // ✅ Eksekusi & paging
        //        var list = await baseQuery.OrderByDescending(u => u.CreateDateTime).ToListAsync();

        //        var totalRows = list.Count;
        //        var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
        //        var rows = list.Skip((page - 1) * perPage).Take(perPage).ToList();

        //        // ✅ Tambahkan data alergi
        //        var result = rows.Select(r =>
        //        {
        //            var alergi = allAlergic.FirstOrDefault(a => a.KunjunganId == r.KunjunganID);
        //            return new
        //            {
        //                r.KunjunganID,
        //                r.AsuransiId,
        //                r.NamaAsuransi,
        //                r.PoliklinikId,
        //                r.NamaPoliklinik,
        //                r.DokterId,
        //                r.NamaDokter,
        //                r.PasienId,
        //                r.AsalKunjungan,
        //                r.NamaPasien,
        //                r.TanggalLahir,
        //                r.JenisKelamin,
        //                r.NoPasien,
        //                r.NoWali2,
        //                r.NoWali3,
        //                r.NamaWali2,
        //                r.NamaWali3,
        //                r.NamaKontakDarurat,
        //                r.NoTeleponDarurat,
        //                r.Email,
        //                r.Umur,
        //                r.AlamatDarurat,
        //                r.AlamatDomisili,
        //                r.AlamatIdentitas,
        //                r.NoRekamMedis,
        //                r.TipePasien,
        //                r.TipePembayaran,
        //                r.JenisKunjungan,
        //                r.StatusPengkajian,
        //                r.CreateDateTime,
        //                r.CreateBy,
        //                r.IsFinished,
        //                r.IsScreening,
        //                r.IsPresent,
        //                r.IsTriage,
        //                r.IsCTTPasienIGD,
        //                TglMasukKunjungan = r.TglMasuk,
        //                r.CaraMasukRS,
        //                r.KondisiKeluar,
        //                r.Antrian,
        //                r.DepositRanap,
        //                r.IsFinishedKasir,
        //                r.NmDokter,
        //                r.gambardokter,
        //                r.CreateByName,
        //                r.JumlahJenisKunjungan,
        //                r.BookingBedRanapId,
        //                r.KelasId,
        //                r.KamarId,
        //                r.KamarNama,
        //                r.LantaiKamar,
        //                r.KelasNama,
        //                r.BedId,
        //                r.NomorKamar,
        //                r.NomorBed,
        //                r.StatusBed,
        //                r.Keterangan,
        //                r.TglKeluar,
        //                r.TglMasuk,
        //                r.NomorSuratPengantar,
        //                r.Diagnosa,
        //                r.AsalUnit,
        //                Alergic = alergi?.AlergicList ?? new List<string>()
        //            };
        //        }).ToList();

        //        return Ok(new
        //        {
        //            status = "success",
        //            message = "Data kunjungan berhasil diambil.",
        //            data = new
        //            {
        //                Rows = result,
        //                TotalRows = totalRows,
        //                CurrentPage = page,
        //                PerPage = perPage,
        //                TotalPages = totalPages
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}


        [HttpGet("paged")]
        public async Task<IActionResult> PagedKunjunganAsync(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] bool? isFinished = null,
            [FromQuery] bool? isScreening = null,
            [FromQuery] bool? isPresent = null,
            [FromQuery] bool? isFinishedKasir = null,
            [FromQuery] bool? isClosed = null,
            [FromQuery] TipePasienFilter? TipePasien = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] string? AsalKunjungan = null,
            [FromQuery] Guid? dokterId = null,
            [FromQuery] Guid? pasienId = null,
            [FromQuery] string? namaKamar = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =====================================================
                // 0) Ambil user login + tipe user
                // =====================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi." });
                }

                var login = await (
                    from u in _applicationDbContext.UserActives.AsNoTracking()
                    join t in _applicationDbContext.TipeUsers.AsNoTracking()
                        on u.TipeUserId equals t.TipeUserId
                    where u.Email == emailLogin
                          && (u.IsDelete == false || u.IsDelete == null)
                          && (t.IsDelete == false || t.IsDelete == null)
                    select new
                    {
                        u.UserActiveId,
                        u.FullName,
                        u.TipeUserId,
                        TipeUserName = t.NamaTipeUser
                    }
                ).FirstOrDefaultAsync();

                if (login == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });
                }

                var tipeName = (login.TipeUserName ?? "").Trim().ToLowerInvariant();
                bool isDokter = tipeName == "dokter";

                // =====================================================
                // 1) Jika dokter login, ambil DokterId dari MstDokter.UserActiveId
                // =====================================================
                Guid? dokterLoginId = null;

                if (isDokter)
                {
                    dokterLoginId = await _applicationDbContext.Dokters
                        .AsNoTracking()
                        .Where(d =>
                            d.UserActiveId == login.UserActiveId &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .Select(d => (Guid?)d.DokterId)
                        .FirstOrDefaultAsync();

                    if (!dokterLoginId.HasValue)
                    {
                        return StatusCode(403, new
                        {
                            message = "Akun dokter Anda belum terhubung ke master dokter. Hubungi admin untuk menghubungkan UserActiveId ke data dokter."
                        });
                    }
                }

                // =====================================================
                // 2) Subquery jumlah kunjungan per pasien + jenis kunjungan
                // =====================================================
                var jumlahPerJenis = _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .Where(k => k.IsDelete == false)
                    .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                    .Select(g => new
                    {
                        g.Key.PasienId,
                        g.Key.JenisKunjungan,
                        JumlahJenis = g.Count()
                    });

                // =====================================================
                // 3) Base query memakai navigation property
                // =====================================================
                var baseQuery =
                    from a in _applicationDbContext.Kunjungans.AsNoTracking()
                    where a.IsDelete == false

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u0.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join da0 in _applicationDbContext.UserActives.AsNoTracking()
                        on a.Dokter.UserActiveId equals da0.UserActiveId into daGroup
                    from da in daGroup.DefaultIfEmpty()

                    join j0 in jumlahPerJenis
                        on new { a.PasienId, a.JenisKunjungan }
                        equals new { j0.PasienId, j0.JenisKunjungan } into jumlahGroup
                    from j in jumlahGroup.DefaultIfEmpty()

                    join bb0 in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                        on a.KunjunganID equals bb0.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b0 in _applicationDbContext.Beds.AsNoTracking()
                        on bb.BedId equals b0.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join k0 in _applicationDbContext.Kamars.AsNoTracking()
                        on bb.KamarId equals k0.KamarId into kamarGroup
                    from k in kamarGroup.DefaultIfEmpty()

                    join kl0 in _applicationDbContext.Kelass.AsNoTracking()
                        on k.KelasId equals kl0.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    join sp0 in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                        on a.KunjunganID equals sp0.KunjunganId into suratGroup
                    from sp in suratGroup.DefaultIfEmpty()

                    select new
                    {
                        a.KunjunganID,

                        // =====================
                        // Asuransi utama
                        // =====================
                        a.AsuransiId,
                        a.AsuransiPasienId,

                        NamaAsuransi = a.Asuransi != null && a.Asuransi.NamaAsuransi != null
                            ? a.Asuransi.NamaAsuransi
                            : null,

                        IsUtama = a.AsuransiPasien != null
                            ? (a.AsuransiPasien.IsUtama ?? false)
                            : false,

                        NoPolis = a.AsuransiPasien != null
                            ? a.AsuransiPasien.NoPolis
                            : null,
                        a.NoRegistrasi,

                        // =====================
                        // Asuransi excess
                        // =====================
                        a.AsuransiExcessId,
                        a.AsuransiPasienExcessId,

                        NamaAsuransiExcess = a.AsuransiExcess != null && a.AsuransiExcess.NamaAsuransi != null
                            ? a.AsuransiExcess.NamaAsuransi
                            : null,

                        IsUtamaExcess = a.AsuransiPasienExcess != null
                            ? (a.AsuransiPasienExcess.IsUtama ?? false)
                            : false,

                        NoPolisExcess = a.AsuransiPasienExcess != null
                            ? a.AsuransiPasienExcess.NoPolis
                            : null,

                        IsExcess = a.AsuransiPasienExcess != null
                            ? a.AsuransiPasienExcess.IsExcess
                            : null,

                        // =====================
                        // Poliklinik
                        // =====================
                        a.PoliklinikId,

                        NamaPoliklinik = a.Poliklinik != null
                            ? a.Poliklinik.NamaPoliklinik
                            : null,

                        // =====================
                        // Dokter
                        // =====================
                        a.DokterId,

                        NamaDokter = a.Dokter != null
                            ? a.Dokter.NmDokter
                            : null,

                        FotoPath = da != null
                            ? da.FotoPath
                            : null,

                        FotoName = da != null
                            ? da.FotoName
                            : null,

                        // =====================
                        // Pasien
                        // =====================
                        a.PasienId,
                        a.AsalKunjungan,

                        NamaPasien = a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                        TanggalLahir = a.Pasien != null
                            ? a.Pasien.TanggalLahir
                            : null,

                        JenisKelamin = a.Pasien != null
                            ? a.Pasien.JenisKelamin
                            : null,

                        NoPasien = a.Pasien != null
                            ? a.Pasien.NoPasien
                            : null,

                        NoWali1 = a.Pasien != null
                            ? a.Pasien.NoWali1
                            : null,

                        NoWali2 = a.Pasien != null
                            ? a.Pasien.NoWali2
                            : null,

                        NamaWali1 = a.Pasien != null
                            ? a.Pasien.NamaWali1
                            : null,

                        NamaWali2 = a.Pasien != null
                            ? a.Pasien.NamaWali2
                            : null,

                        NamaKontakDarurat = a.Pasien != null
                            ? a.Pasien.NamaKontakDarurat
                            : null,

                        NoTeleponDarurat = a.Pasien != null
                            ? a.Pasien.NoTeleponDarurat
                            : null,

                        EmailPasien = a.Pasien != null
                            ? a.Pasien.Email
                            : null,

                        AlamatDomisili = a.Pasien != null
                            ? a.Pasien.AlamatDomisili
                            : null,

                        AlamatDarurat = a.Pasien != null
                            ? a.Pasien.AlamatDarurat
                            : null,

                        AlamatIdentitas = a.Pasien != null
                            ? a.Pasien.AlamatIdentitas
                            : null,

                        // =====================
                        // Kunjungan
                        // =====================
                        a.NoRekamMedis,
                        a.TipePasien,
                        a.TipePembayaran,
                        a.JenisKunjungan,
                        a.StatusPengkajian,

                        a.CreateDateTime,
                        a.CreateBy,

                        CreateByName = u != null
                            ? u.FullName
                            : null,

                        a.IsFinished,
                        a.TglFinishedKasir,
                        a.IsScreening,
                        a.IsPresent,
                        a.IsTriage,
                        a.IsClosed,
                        a.IsCTTPasienIGD,
                        a.Antrian,

                        TglMasukKunjungan = a.TglMasuk,

                        a.CaraMasukRS,
                        a.KondisiKeluar,
                        a.IsFinishedKasir,

                        JumlahJenisKunjungan = j != null
                            ? j.JumlahJenis
                            : 0,

                        // =====================
                        // Booking bed / kamar
                        // =====================
                        BookingBedRanapId = bb != null
                            ? (Guid?)bb.BookingBedRanapId
                            : null,

                        KelasId = kl != null
                            ? (Guid?)kl.KelasId
                            : null,

                        KamarId = bb != null
                            ? bb.KamarId
                            : null,

                        KamarNama = k != null
                            ? k.NamaKamar
                            : null,

                        LantaiKamar = k != null
                            ? k.Lantai
                            : null,

                        KelasNama = kl != null
                            ? kl.NamaKelas
                            : null,

                        BedId = bb != null
                            ? bb.BedId
                            : null,

                        NomorKamar = bb != null
                            ? bb.NoKamar
                            : null,

                        NomorBed = b != null
                            ? b.NomorBed
                            : null,

                        StatusBed = bb != null
                            ? bb.StatusBed
                            : null,

                        KeteranganBed = bb != null
                            ? bb.Keterangan
                            : null,

                        TglKeluar = bb != null
                            ? bb.TglKeluar
                            : null,

                        TglMasuk = bb != null
                            ? bb.TglMasuk
                            : null,

                        // =====================
                        // Surat pengantar ranap
                        // =====================
                        NomorSuratPengantar = sp != null
                            ? sp.NomorSuratPengantar
                            : null,

                        Diagnosa = sp != null
                            ? sp.Diagnosa
                            : null,

                        AsalUnit = sp != null
                            ? sp.AsalUnit
                            : null
                    };

                // =====================================================
                // 4) Rule akses dokter
                // =====================================================
                if (isDokter)
                {
                    baseQuery = baseQuery.Where(x => x.DokterId == dokterLoginId!.Value);
                }
                else
                {
                    if (dokterId.HasValue && dokterId.Value != Guid.Empty)
                    {
                        baseQuery = baseQuery.Where(x => x.DokterId == dokterId.Value);
                    }
                }

                // =====================================================
                // 5) Filter dinamis
                // =====================================================
                if (isFinished.HasValue)
                    baseQuery = baseQuery.Where(x => x.IsFinished == isFinished.Value);

                if (isPresent.HasValue)
                    baseQuery = baseQuery.Where(x => x.IsPresent == isPresent.Value);

                if (isScreening.HasValue)
                    baseQuery = baseQuery.Where(x => x.IsScreening == isScreening.Value);

                if (isFinishedKasir.HasValue)
                    baseQuery = baseQuery.Where(x => x.IsFinishedKasir == isFinishedKasir.Value);

                if (isClosed.HasValue)
                    baseQuery = baseQuery.Where(x => x.IsClosed == isClosed.Value);

                if (TipePasien.HasValue)
                    baseQuery = baseQuery.Where(x => x.TipePasien == TipePasien.Value.ToString());

                if (JenisKunjungan.HasValue)
                    baseQuery = baseQuery.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

                if (pasienId.HasValue && pasienId.Value != Guid.Empty)
                    baseQuery = baseQuery.Where(x => x.PasienId == pasienId.Value);

                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    baseQuery = baseQuery.Where(x =>
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                if (!string.IsNullOrWhiteSpace(AsalKunjungan))
                {
                    var pattern = $"%{AsalKunjungan.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.AsalKunjungan ?? "", pattern));
                }

                if (!string.IsNullOrWhiteSpace(namaKamar))
                {
                    var pattern = $"%{namaKamar.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.KamarNama ?? "", pattern));
                }

                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    baseQuery = periode.Value switch
                    {
                        PeriodeFilter.Today =>
                            baseQuery.Where(x => x.CreateDateTime.Date == today),

                        PeriodeFilter.Yesterday =>
                            baseQuery.Where(x => x.CreateDateTime.Date == today.AddDays(-1)),

                        PeriodeFilter.ThisWeek =>
                            baseQuery.Where(x =>
                                x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                                x.CreateDateTime.Date <= today),

                        PeriodeFilter.LastWeek =>
                            baseQuery.Where(x =>
                                x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)),

                        PeriodeFilter.ThisMonth =>
                            baseQuery.Where(x =>
                                x.CreateDateTime.Month == today.Month &&
                                x.CreateDateTime.Year == today.Year),

                        PeriodeFilter.LastMonth =>
                            baseQuery.Where(x =>
                                x.CreateDateTime >= new DateTime(today.Year, today.Month, 1).AddMonths(-1) &&
                                x.CreateDateTime < new DateTime(today.Year, today.Month, 1)),

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

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.NamaPasien ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaDokter ?? "", pattern) ||
                        EF.Functions.ILike(x.NoRekamMedis ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaPoliklinik ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaAsuransi ?? "", pattern) ||
                        EF.Functions.ILike(x.Antrian ?? "", pattern));
                }

                // =====================================================
                // 6) Sorting
                // =====================================================
                bool desc = (sortDirection ?? "desc").ToLower() == "desc";

                baseQuery = (orderBy ?? "CreateDateTime") switch
                {
                    "NamaPasien" =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.NamaPasien)
                            : baseQuery.OrderBy(x => x.NamaPasien),

                    "NamaDokter" =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.NamaDokter)
                            : baseQuery.OrderBy(x => x.NamaDokter),

                    "NamaPoliklinik" =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.NamaPoliklinik)
                            : baseQuery.OrderBy(x => x.NamaPoliklinik),

                    "NoRekamMedis" =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.NoRekamMedis)
                            : baseQuery.OrderBy(x => x.NoRekamMedis),

                    "TglMasukKunjungan" =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.TglMasukKunjungan)
                            : baseQuery.OrderBy(x => x.TglMasukKunjungan),

                    "CreateDateTime" or _ =>
                        desc
                            ? baseQuery.OrderByDescending(x => x.CreateDateTime)
                            : baseQuery.OrderBy(x => x.CreateDateTime)
                };

                // =====================================================
                // 7) Paging di database
                // =====================================================
                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await baseQuery
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                // =====================================================
                // 8) Ambil alergi hanya untuk rows yang tampil
                // =====================================================
                var kunjunganIds = rows
                    .Select(x => x.KunjunganID)
                    .ToList();

                var alergiRaw = await _applicationDbContext.PainAssessments
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.KunjunganId.HasValue &&
                        kunjunganIds.Contains(x.KunjunganId.Value))
                    .Select(x => new
                    {
                        KunjunganId = x.KunjunganId.Value,
                        x.Alergic
                    })
                    .ToListAsync();

                var alergiMap = alergiRaw
                    .GroupBy(x => x.KunjunganId)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .Where(x => !string.IsNullOrWhiteSpace(x.Alergic))
                            .Select(x => x.Alergic)
                            .Distinct()
                            .ToList()
                    );

                // =====================================================
                // 9) Response
                // =====================================================
                var result = rows.Select(r =>
                {
                    alergiMap.TryGetValue(r.KunjunganID, out var al);

                    return new
                    {
                        r.KunjunganID,

                        r.AsuransiId,
                        r.AsuransiPasienId,
                        r.NamaAsuransi,
                        r.NoPolis,
                        r.IsUtama,

                        r.AsuransiExcessId,
                        r.AsuransiPasienExcessId,
                        r.NamaAsuransiExcess,
                        r.NoPolisExcess,
                        r.IsUtamaExcess,
                        r.IsExcess,

                        r.PoliklinikId,
                        r.NamaPoliklinik,

                        r.DokterId,
                        r.NamaDokter,

                        r.PasienId,
                        r.AsalKunjungan,
                        r.NamaPasien,
                        r.TanggalLahir,
                        r.JenisKelamin,
                        r.NoRegistrasi,
                        r.NoPasien,
                        r.NoWali1,
                        r.NoWali2,
                        r.NamaWali1,
                        r.NamaWali2,
                        r.NamaKontakDarurat,
                        r.NoTeleponDarurat,

                        Email = r.EmailPasien,

                        r.AlamatDomisili,
                        r.AlamatDarurat,
                        r.AlamatIdentitas,

                        Umur = r.TanggalLahir.HasValue
                            ? HitungUmurLengkap(r.TanggalLahir)
                            : null,

                        r.NoRekamMedis,
                        r.TipePasien,
                        r.TipePembayaran,
                        r.JenisKunjungan,
                        r.StatusPengkajian,

                        r.CreateDateTime,
                        r.CreateBy,
                        r.CreateByName,

                        r.IsFinished,
                        r.TglFinishedKasir,
                        r.IsScreening,
                        r.IsPresent,
                        r.IsTriage,
                        r.IsClosed,
                        r.IsCTTPasienIGD,
                        r.Antrian,
                        r.TglMasukKunjungan,
                        r.CaraMasukRS,
                        r.KondisiKeluar,
                        r.IsFinishedKasir,

                        r.FotoName,
                        r.FotoPath,

                        r.JumlahJenisKunjungan,

                        r.BookingBedRanapId,
                        r.KelasId,
                        r.KamarId,
                        r.KamarNama,
                        r.LantaiKamar,
                        r.KelasNama,
                        r.BedId,
                        r.NomorKamar,
                        r.NomorBed,
                        r.StatusBed,

                        Keterangan = r.KeteranganBed,

                        r.TglKeluar,
                        r.TglMasuk,

                        r.NomorSuratPengantar,
                        r.Diagnosa,
                        r.AsalUnit,

                        Alergic = al ?? new List<string>()
                    };
                }).ToList();

                return Ok(new
                {
                    status = "success",
                    message = "Data kunjungan berhasil diambil.",
                    data = new
                    {
                        Rows = result,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("KunjunganLunasToday/paged")]
        public async Task<IActionResult> GetKunjunganLunasToday(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? dokterId = null,
            [FromQuery] EnumJenisKunjungan? jenisKunjungan = null,
            [FromQuery] string? asalKunjungan = null,
            [FromQuery] string? search = null,
            [FromQuery] Guid? pasienId = null,
            [FromQuery] Guid? kunjunganId = null,
            CancellationToken ct = default,
            [FromQuery] string? sortDirection = "desc")
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var query =
                from k in _applicationDbContext.Kunjungans.AsNoTracking()
                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p.PendaftaranPasienBaruId into pasienGroup
                from p in pasienGroup.DefaultIfEmpty()
                where !k.IsDelete
                      && k.IsFinishedKasir == true
                      && k.TglFinishedKasir >= today
                      && k.TglFinishedKasir < tomorrow
                select new
                {
                    k.CreateDateTime,
                    k.KunjunganID,
                    k.PasienId,
                    k.DokterId,
                    k.IsFinishedKasir,
                    k.TglFinishedKasir,
                    k.JenisKunjungan,
                    k.AsalKunjungan,
                    NamaPasien = p != null ? p.NamaLengkap : null,
                    NoRM = p != null ? p.NoRekamMedis : null,
                };

            if (dokterId.HasValue)
            {
                query = query.Where(x => x.DokterId == dokterId.Value);
            }

            if (jenisKunjungan.HasValue)
            {
                query = query.Where(x => x.JenisKunjungan == jenisKunjungan.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(asalKunjungan))
            {
                query = query.Where(x => x.AsalKunjungan != null &&
                                         x.AsalKunjungan.ToLower() == asalKunjungan.ToLower());
            }

            if (pasienId.HasValue)
            {
                query = query.Where(x => x.PasienId == pasienId.Value);
            }

            if (kunjunganId.HasValue)
            {
                query = query.Where(x => x.KunjunganID == kunjunganId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaPasien, search)
                );
            }

            query = sortDirection?.ToLower() == "asc"
                ? query.OrderBy(x => x.CreateDateTime)
                : query.OrderByDescending(x => x.CreateDateTime);

            var totalData = await query.CountAsync(ct);

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Ok(new
            {
                page,
                pageSize,
                totalData,
                totalPage = (int)Math.Ceiling((double)totalData / pageSize),
                items = data
            });
        }

    }
}
