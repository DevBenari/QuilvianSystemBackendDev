using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
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

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            ILogger<KunjunganController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<KunjunganHub> hubContext
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
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
                // ✅ Ambil semua data alergi (tanpa N+1 query)
                var allAlergic = await _applicationDbContext.PainAssessments
                    .Where(x => !x.IsDelete)
                    .GroupBy(x => x.KunjunganId)
                    .Select(g => new
                    {
                        KunjunganId = g.Key,
                        Alergic = string.Join(", ", g.Select(x => x.Alergic).Distinct())
                    })
                    .ToListAsync();

                // ✅ Hitung jumlah kunjungan per pasien + jenis kunjungan
                var jumlahPerJenis = _applicationDbContext.Kunjungans
                    .Where(k => !k.IsDelete)
                    .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                    .Select(g => new
                    {
                        g.Key.PasienId,
                        g.Key.JenisKunjungan,
                        JumlahJenis = g.Count()
                    });

                // ✅ Query utama dengan LEFT JOIN agar data tidak hilang
                var query =
                    from a in _applicationDbContext.Kunjungans
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId into poliGroup
                    from p in poliGroup.DefaultIfEmpty()

                    join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
                    from o in asuransiGroup.DefaultIfEmpty()

                    join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId into pasienGroup
                    from ps in pasienGroup.DefaultIfEmpty()

                    join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId into dokterGroup
                    from d in dokterGroup.DefaultIfEmpty()

                    join j in jumlahPerJenis on new { a.PasienId, a.JenisKunjungan } equals new { j.PasienId, j.JenisKunjungan }

                    join bb in _applicationDbContext.BookingBedRanaps on a.KunjunganID equals bb.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Beds on bb.BedId equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kamars on bb.KamarId equals k.KamarId into kamarGroup
                    from k in kamarGroup.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass on k.KelasId equals kl.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganID equals sp.KunjunganId into suratGroup
                    from sp in suratGroup.DefaultIfEmpty()

                    where a.IsDelete == false
                    select new
                    {
                        a.KunjunganID,
                        a.AsuransiId,
                        NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai",
                        a.PoliklinikId,
                        NamaPoliklinik = p != null ? p.NamaPoliklinik : null,
                        a.DokterId,
                        NamaDokter = d != null ? d.NmDokter : null,
                        a.PasienId,
                        a.AsalKunjungan,
                        NamaLengkap = ps != null ? ps.NamaLengkap : null,
                        ps.TanggalLahir,
                        ps.JenisKelamin,
                        ps.NoPasien,
                        ps.NoWali2,
                        ps.NoWali3,
                        ps.NamaWali2,
                        ps.NamaWali3,
                        ps.NamaKontakDarurat,
                        ps.NoTeleponDarurat,
                        ps.Email,
                        AlamatDomisili = ps != null ? ps.AlamatDomisili :null,
                        AlamatDarurat = ps != null ? ps.AlamatDarurat : null,
                        AlamatIdentitas = ps != null ? ps.AlamatIdentitas : null,
                        Umur = ps != null ? HitungUmurLengkap(ps.TanggalLahir) : null,
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
                        a.DepositRanap,
                        d.NmDokter,
                        gambardokter = !string.IsNullOrEmpty(d.FotoName)
                            ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                            : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",
                        CreateByName = u != null ? u.FullName : null,
                        KelasId = kl != null ? (Guid?)kl.KelasId : null,
                        JumlahJenisKunjungan = j.JumlahJenis,
                        BookingBedRanapId = bb != null ? (Guid?)bb.BookingBedRanapId : null,
                        KamarId = bb != null ? bb.KamarId : null,
                        KamarNama = k != null ? k.NamaKamar : null,
                        LantaiKamar = k != null ? k.Lantai : null,
                        KelasNama = kl != null ? kl.NamaKelas : null,
                        BedId = bb != null ? bb.BedId : null,
                        NomorKamar = bb != null ? bb.NoKamar : null,
                        NomorBed = b != null ? b.NomorBed : null,
                        StatusBed = bb != null ? bb.StatusBed : null,
                        Keterangan = bb != null ? bb.Keterangan : null,
                        TglKeluar = bb != null ? bb.TglKeluar : null,
                        TglMasuk = bb != null ? bb.TglMasuk : null,
                        NomorSuratPengantar = sp != null ? sp.NomorSuratPengantar : null,
                        Diagnosa = sp != null ? sp.Diagnosa : null,
                        AsalUnit = sp != null ? sp.AsalUnit : null
                    };

                // ✅ Eksekusi query & urutkan berdasarkan tanggal
                var list = await query.OrderByDescending(a => a.CreateDateTime).ToListAsync();

                // ✅ Hilangkan duplikat
                var uniqueList = list
                    .GroupBy(x => x.KunjunganID)
                    .Select(g => g.First())
                    .ToList();

                // ✅ Gabungkan data alergi
                var result = uniqueList.Select(r =>
                {
                    var alergi = allAlergic.FirstOrDefault(a => a.KunjunganId == r.KunjunganID);
                    return new
                    {
                        r.KunjunganID,
                        r.AsuransiId,
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
                        r.NoWali2,
                        r.NoWali3,
                        r.NamaWali2,
                        r.NamaWali3,
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
                        TglMasukKunjungan = r.TglMasuk,
                        r.CaraMasukRS,
                        r.KondisiKeluar,
                        r.IsFinishedKasir,
                        r.NmDokter,
                        r.gambardokter,
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

                // ✅ Pagination
                var totalRows = result.Count;
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var pagedData = result.Skip((page - 1) * perPage).Take(perPage).ToList();

                if (!pagedData.Any())
                {
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
                }

                // ✅ Return hasil
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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKunjunganById(Guid id)
        {
            try
            {
                // Cek apakah Kunjungan ada (minimal data utama)
                var kunjungan = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(k => k.KunjunganID == id && !k.IsDelete);

                if (kunjungan == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });
                }

                // Ambil data alergi (kalau ada)
                var alergiList = await _applicationDbContext.PainAssessments
                    .Where(x => x.KunjunganId == id && !x.IsDelete)
                    .Select(x => x.Alergic)
                    .Distinct()
                    .ToListAsync();

                // Hitung jumlah kunjungan pasien per jenis
                var jumlahPerJenis = await _applicationDbContext.Kunjungans
                    .Where(k => !k.IsDelete && k.PasienId == kunjungan.PasienId)
                    .GroupBy(k => k.JenisKunjungan)
                    .Select(g => new
                    {
                        JenisKunjungan = g.Key,
                        Jumlah = g.Count()
                    })
                    .ToListAsync();

                // ===================== LEFT JOIN VERSI AMAN =====================
                var result = (
                    from a in _applicationDbContext.Kunjungans
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId into poliGroup
                    from p in poliGroup.DefaultIfEmpty()

                    join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
                    from o in asuransiGroup.DefaultIfEmpty()

                    join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId into pasienGroup
                    from ps in pasienGroup.DefaultIfEmpty()

                    join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId into dokterGroup
                    from d in dokterGroup.DefaultIfEmpty()

                    join bb in _applicationDbContext.BookingBedRanaps on a.KunjunganID equals bb.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Beds on bb.BedId equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.Kamars on bb.KamarId equals k.KamarId into kamarGroup
                    from k in kamarGroup.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass on k.KelasId equals kl.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganID equals sp.KunjunganId into suratGroup
                    from sp in suratGroup.DefaultIfEmpty()

                    where a.KunjunganID == id && a.IsDelete == false
                    select new
                    {
                        a.KunjunganID,
                        a.AsuransiId,
                        NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai",
                        a.PoliklinikId,
                        NamaPoliklinik = p != null ? p.NamaPoliklinik : null,
                        a.DokterId,
                        NamaDokter = d != null ? d.NmDokter : null,
                        a.PasienId,
                        a.AsalKunjungan,
                        NamaPasien = ps != null ? ps.NamaLengkap : null,
                        ps.TanggalLahir,
                        ps.JenisKelamin,
                        ps.NoPasien,
                        ps.NoWali2,
                        ps.NoWali3,
                        ps.NamaWali2,
                        ps.NamaWali3,
                        ps.NamaKontakDarurat,
                        ps.NoTeleponDarurat,
                        ps.Email,
                        AlamatDomisili = ps != null ? ps.AlamatDomisili : null,
                        AlamatDarurat = ps != null ? ps.AlamatDarurat : null,
                        AlamatIdentitas = ps != null ? ps.AlamatIdentitas : null,
                        Umur = ps != null ? HitungUmurLengkap(ps.TanggalLahir) : null,
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
                        a.IsTriage,
                        a.IsClosed,
                        a.IsCTTPasienIGD,
                        a.Antrian,
                        a.DepositRanap,
                        TglMasukKunjungan = a.TglMasuk,
                        a.CaraMasukRS,
                        a.KondisiKeluar,
                        a.IsFinishedKasir,
                        gambardokter = !string.IsNullOrEmpty(d.FotoName)
                            ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                            : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",
                        CreateByName = u != null ? u.FullName : null,
                        KelasId = kl != null ? (Guid?)kl.KelasId : null,
                        BookingBedRanapId = bb != null ? (Guid?)bb.BookingBedRanapId : null,
                        KamarId = bb != null ? bb.KamarId : null,
                        KamarNama = k != null ? k.NamaKamar : null,
                        LantaiKamar = k != null ? k.Lantai : null,
                        KelasNama = kl != null ? kl.NamaKelas : null,
                        BedId = bb != null ? bb.BedId : null,
                        NomorKamar = bb != null ? bb.NoKamar : null,
                        NomorBed = b != null ? b.NomorBed : null,
                        StatusBed = bb != null ? bb.StatusBed : null,
                        Keterangan = bb != null ? bb.Keterangan : null,
                        TglKeluar = bb != null ? bb.TglKeluar : null,
                        TglMasuk = bb != null ? bb.TglMasuk : null,
                        NomorSuratPengantar = sp != null ? sp.NomorSuratPengantar : null,
                        Diagnosa = sp != null ? sp.Diagnosa : null,
                        AsalUnit = sp != null ? sp.AsalUnit : null
                    }).FirstOrDefault();

                if (result == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });
                }

                // Gabungkan hasil
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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateKunjunganPasien([FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive?.UserActiveId ?? Guid.Empty;

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // Validasi jenis kunjungan (default "Rawat Jalan" jika kosong/"string")
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) ||
                                 request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";
                var today = DateTime.UtcNow.Date;

                // =============================
                // 🔎 Cek apakah pasien masih punya kunjungan aktif
                // =============================
                bool isAlreadyRegistered = false;

                if (kodeJenis == "OP")
                {
                    isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
                        k.PasienId == request.PasienId &&
                        k.PoliklinikId == request.PoliklinikId &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.IsFinishedKasir == false &&
                        k.JenisKunjungan == "OP" &&
                        k.CreateDateTime.Date == today);
                }
                else if (kodeJenis == "IP")
                {
                    isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
                        k.PasienId == request.PasienId &&
                        !k.IsDelete &&
                        k.IsFinished == false &&
                        k.IsFinishedKasir == false &&
                        k.JenisKunjungan == "IP");
                }

                if (isAlreadyRegistered)
                {
                    return BadRequest(new { message = "Pasien sudah terdaftar untuk kunjungan aktif yang belum selesai." });
                }

                // =============================
                // 🩺 Penentuan Nomor Antrean
                // =============================
                string nomorAntrianFormatted = null;
                string kodePoli = null;

                // Hanya generate antrean jika AsalKunjungan bukan IGD
                if (!string.Equals(request.AsalKunjungan?.Trim(), "igd", StringComparison.OrdinalIgnoreCase))
                {
                    // Pastikan PoliklinikId ada
                    if (request.PoliklinikId == null || request.PoliklinikId == Guid.Empty)
                    {
                        return BadRequest(new { message = "Poliklinik wajib dipilih untuk kunjungan non-IGD." });
                    }

                    // Ambil kode antrean poli
                    kodePoli = _applicationDbContext.Polikliniks
                        .Where(p => p.PoliklinikId == request.PoliklinikId)
                        .Select(p => p.KodeAntreanPoli)
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(kodePoli))
                        return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                    // Hitung jumlah antrean hari ini
                    var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                        .Count(k => k.PoliklinikId == request.PoliklinikId &&
                                    k.CreateDateTime.Date == today &&
                                    !k.IsDelete);

                    int nomorAntrian = jumlahAntrianHariIni + 1;
                    nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}";
                }

                // =============================
                // 🔐 Generate ID unik untuk kunjungan
                // =============================
                Guid newKunjunganId;
                int attempt = 0;
                do
                {
                    newKunjunganId = Guid.NewGuid();
                    attempt++;
                } while (await _applicationDbContext.Kunjungans.AnyAsync(k => k.KunjunganID == newKunjunganId) && attempt < 5);

                if (await _applicationDbContext.Kunjungans.AnyAsync(k => k.KunjunganID == newKunjunganId))
                {
                    return StatusCode(500, new { message = "Gagal membuat KunjunganID unik. Silakan coba lagi." });
                }

                // =============================
                // 💾 Simpan data kunjungan
                // =============================
                var newKunjungan = new Kunjungan
                {
                    KunjunganID = newKunjunganId,
                    PasienId = request.PasienId,
                    DokterId = request.DokterId,
                    PoliklinikId = request.PoliklinikId,
                    AsuransiId = request.AsuransiId,
                    JenisKunjungan = kodeJenis,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = UserActiveId,
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
                    Antrian = nomorAntrianFormatted, // null jika IGD
                    AsalKunjungan = request.AsalKunjungan,
                    TglMasuk = request.TglMasuk,
                    CaraMasukRS = request.CaraMasukRS,
                    KondisiKeluar = request.KondisiKeluar,
                    DepositRanap = request.DepositRanap,
                };

                _applicationDbContext.Kunjungans.Add(newKunjungan);

                // =============================
                // 💰 Tambahkan Biaya Administrasi (jika ada)
                // =============================
                var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
                    .Where(b => b.BiayaAdministrasiKode == kodeJenis)
                    .FirstOrDefaultAsync();

                // Ambil / buat invoice sekali saja untuk semua billing yang akan dibuat
                var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                    newKunjungan.KunjunganID,
                    DateTime.UtcNow
                );

                // Jika ada biaya admin, tambahkan billingnya
                if (biayaAdmin != null)
                {
                    var bill = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = newKunjungan.KunjunganID,
                        ItemId = biayaAdmin.BiayaAdministrasiId,
                        NamaItem = biayaAdmin.NamaBiayaAdministrasi,
                        HargaItem = biayaAdmin.NominalBiayaAdministrasi,
                        QtyItem = 1,
                        SubTotalItem = biayaAdmin.NominalBiayaAdministrasi,

                        InvoiceBilling = invoice,
                        IsListWhiteOff = false,
                        BillingKode = "001",
                        JenisBilling = "Biaya Admin",
                        StatusBilling = false,
                        BillingDate = DateTime.UtcNow,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId
                    };

                    _applicationDbContext.Billings.Add(bill);
                }

                // =============================================
                // 🏥 Deposit wajib untuk kunjungan IP
                // =============================================
                if (kodeJenis == "IP")
                {
                    // WARNING / VALIDASI
                    if (newKunjungan.DepositRanap == null || newKunjungan.DepositRanap <= 0)
                    {
                        // kamu bisa ganti dengan return sesuai pola API kamu (ProblemDetails / ModelState)
                        return BadRequest(new
                        {
                            message = "Kunjungan IP (rawat inap) wajib mengisi nominal deposit. "
                        });
                    }

                    var depo = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = newKunjungan.KunjunganID,
                        ItemId = null,
                        NamaItem = "Deposito Ranap",
                        HargaItem = newKunjungan.DepositRanap.Value,
                        QtyItem = 1,
                        SubTotalItem = newKunjungan.DepositRanap.Value,
                        //InvoiceBilling = invoice,
                        IsListWhiteOff = false,
                        BillingKode = "DP",
                        JenisBilling = "DepositRanap",
                        StatusBilling = false,
                        BillingDate = DateTime.UtcNow,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId
                    };

                    _applicationDbContext.Billings.Add(depo);
                }

                await _applicationDbContext.SaveChangesAsync();

                // =============================
                // 🔔 Kirim notifikasi SignalR
                // =============================
                await _hubContext.Clients.All.SendAsync("Kunjungan ditambah", new
                {
                    action = "create",
                    kunjunganId = newKunjungan.KunjunganID,
                    pasienId = request.PasienId,
                    dokterId = request.DokterId,
                    NomorAntrian = nomorAntrianFormatted
                });

                return Ok(new
                {
                    message = "Kunjungan baru berhasil ditambahkan.",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        newKunjungan.KunjunganID,
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

                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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
                existing.AsuransiId = request.AsuransiId;
                existing.JenisKunjungan = kodeJenis;
                existing.NoRekamMedis = request.NoRekamMedis;
                existing.TipePasien = request.TipePasien;
                existing.TipePembayaran = request.TipePembayaran;
                existing.AsalKunjungan = request.AsalKunjungan;
                existing.Antrian = nomorAntrianFormatted;
                existing.TglMasuk = request.TglMasuk;
                existing.CaraMasukRS = request.CaraMasukRS;
                existing.KondisiKeluar = request.KondisiKeluar;
                existing.DepositRanap = request.DepositRanap;

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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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
                IsClosed = request.IsClosed
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            kunjungan.IsFinishedKasir = request.IsFinishedKasir;
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
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
            [FromQuery] Guid? dokterId = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =====================================================
                // 0) Ambil user login + tipe user (JOIN MstTipeUser)
                // =====================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi." });

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
                        TipeUserName = t.NamaTipeUser,
                    }
                ).FirstOrDefaultAsync();

                if (login == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });

                var tipeName = (login.TipeUserName ?? "").Trim().ToLowerInvariant();
                bool isDokter = tipeName == "dokter";
                bool isPerawat = tipeName == "perawat";
                bool isAdmin = tipeName.Contains("admin") || tipeName.Contains("superadmin");

                if (!isDokter && !isPerawat && !isAdmin)
                    return StatusCode(403, new { message = "Anda tidak memiliki akses untuk melihat daftar pasien." });

                // =====================================================
                // 1) Jika Dokter login → ambil DokterId dari MstDokter.UserActiveId
                // =====================================================
                Guid? dokterLoginId = null;
                if (isDokter)
                {
                    dokterLoginId = await _applicationDbContext.Dokters.AsNoTracking()
                        .Where(d => d.UserActiveId == login.UserActiveId && (d.IsDelete == false || d.IsDelete == null))
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
                // 2) Subquery: jumlah kunjungan per pasien per jenis
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
                // 3) Base query dengan LEFT JOIN lengkap (IQueryable)
                // =====================================================
                var baseQuery =
                    from a in _applicationDbContext.Kunjungans.AsNoTracking()
                    where a.IsDelete == false

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u0.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join p0 in _applicationDbContext.Polikliniks.AsNoTracking()
                        on a.PoliklinikId equals p0.PoliklinikId into poliGroup
                    from p in poliGroup.DefaultIfEmpty()

                    join o0 in _applicationDbContext.Asuransis.AsNoTracking()
                        on a.AsuransiId equals o0.AsuransiId into asuransiGroup
                    from o in asuransiGroup.DefaultIfEmpty()

                    join ps0 in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                        on a.PasienId equals ps0.PendaftaranPasienBaruId into pasienGroup
                    from ps in pasienGroup.DefaultIfEmpty()

                    join d0 in _applicationDbContext.Dokters.AsNoTracking()
                        on a.DokterId equals d0.DokterId into dokterGroup
                    from d in dokterGroup.DefaultIfEmpty()

                    join j in jumlahPerJenis
                        on new { a.PasienId, a.JenisKunjungan }
                        equals new { j.PasienId, j.JenisKunjungan }

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

                        a.AsuransiId,
                        NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai",

                        a.PoliklinikId,
                        NamaPoliklinik = p != null ? p.NamaPoliklinik : null,

                        a.DokterId,
                        NamaDokter = d != null ? d.NmDokter : null,
                        FotoDokter = d != null ? d.FotoName : null,

                        a.PasienId,
                        a.AsalKunjungan,
                        NamaPasien = ps != null ? ps.NamaLengkap : null,
                        TanggalLahir = ps != null ? ps.TanggalLahir : null,
                        JenisKelamin = ps != null ? ps.JenisKelamin : null,
                        NoPasien = ps != null ? ps.NoPasien : null,
                        NoWali2 = ps != null ? ps.NoWali2 : null,
                        NoWali3 = ps != null ? ps.NoWali3 : null,
                        NamaWali2 = ps != null ? ps.NamaWali2 : null,
                        NamaWali3 = ps != null ? ps.NamaWali3 : null,
                        NamaKontakDarurat = ps != null ? ps.NamaKontakDarurat : null,
                        NoTeleponDarurat = ps != null ? ps.NoTeleponDarurat : null,
                        EmailPasien = ps != null ? ps.Email : null,
                        AlamatDomisili = ps != null ? ps.AlamatDomisili : null,
                        AlamatDarurat = ps != null ? ps.AlamatDarurat : null,
                        AlamatIdentitas = ps != null ? ps.AlamatIdentitas : null,

                        a.NoRekamMedis,
                        a.TipePasien,
                        a.TipePembayaran,
                        a.JenisKunjungan,
                        a.StatusPengkajian,

                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,

                        a.IsFinished,
                        a.IsScreening,
                        a.IsPresent,
                        a.IsTriage,
                        a.IsClosed,
                        a.IsCTTPasienIGD,
                        a.Antrian,
                        a.DepositRanap,
                        TglMasukKunjungan = a.TglMasuk,
                        a.CaraMasukRS,
                        a.KondisiKeluar,
                        a.IsFinishedKasir,

                        JumlahJenisKunjungan = j.JumlahJenis,

                        BookingBedRanapId = bb != null ? (Guid?)bb.BookingBedRanapId : null,
                        KelasId = kl != null ? (Guid?)kl.KelasId : null,
                        KamarId = bb != null ? (Guid?)bb.KamarId : null,
                        KamarNama = k != null ? k.NamaKamar : null,
                        LantaiKamar = k != null ? k.Lantai : null,
                        KelasNama = kl != null ? kl.NamaKelas : null,
                        BedId = bb != null ? (Guid?)bb.BedId : null,
                        NomorKamar = bb != null ? bb.NoKamar : null,
                        NomorBed = b != null ? b.NomorBed : null,
                        StatusBed = bb != null ? bb.StatusBed : null,
                        KeteranganBed = bb != null ? bb.Keterangan : null,
                        TglKeluar = bb != null ? bb.TglKeluar : null,
                        TglMasuk = bb != null ? bb.TglMasuk : null,

                        NomorSuratPengantar = sp != null ? sp.NomorSuratPengantar : null,
                        Diagnosa = sp != null ? sp.Diagnosa : null,
                        AsalUnit = sp != null ? sp.AsalUnit : null
                    };

                // =====================================================
                // 4) Rule akses: dokter hanya lihat pasiennya
                // =====================================================
                if (isDokter)
                {
                    baseQuery = baseQuery.Where(x => x.DokterId == dokterLoginId!.Value);
                }
                else
                {
                    if (dokterId.HasValue && dokterId.Value != Guid.Empty)
                        baseQuery = baseQuery.Where(x => x.DokterId == dokterId.Value);
                }

                // =====================================================
                // 5) Filter dinamis
                // =====================================================
                if (isFinished.HasValue) baseQuery = baseQuery.Where(x => x.IsFinished == isFinished.Value);
                if (isPresent.HasValue) baseQuery = baseQuery.Where(x => x.IsPresent == isPresent.Value);
                if (isScreening.HasValue) baseQuery = baseQuery.Where(x => x.IsScreening == isScreening.Value);
                if (isFinishedKasir.HasValue) baseQuery = baseQuery.Where(x => x.IsFinishedKasir == isFinishedKasir.Value);
                if (isClosed.HasValue) baseQuery = baseQuery.Where(x => x.IsClosed == isClosed.Value);

                if (TipePasien.HasValue) baseQuery = baseQuery.Where(x => x.TipePasien == TipePasien.Value.ToString());
                if (JenisKunjungan.HasValue) baseQuery = baseQuery.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    baseQuery = baseQuery.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
                }

                if (!string.IsNullOrWhiteSpace(AsalKunjungan))
                {
                    var pattern = $"%{AsalKunjungan.ToLower()}%";
                    baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.AsalKunjungan ?? "", pattern));
                }

                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    baseQuery = periode.Value switch
                    {
                        PeriodeFilter.Today =>
                            baseQuery.Where(x => x.CreateDateTime.Date == today),

                        PeriodeFilter.Yesterday =>
                            baseQuery.Where(x=>x.CreateDateTime.Date == today.AddDays(-1)),

                        PeriodeFilter.ThisWeek =>
                            baseQuery.Where(x => x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek)
                                              && x.CreateDateTime.Date <= today),

                        PeriodeFilter.LastWeek =>
                            baseQuery.Where(x => x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek)
                                              && x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)),

                        PeriodeFilter.ThisMonth =>
                            baseQuery.Where(x => x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year),

                        PeriodeFilter.LastMonth =>
                            baseQuery.Where(x => x.CreateDateTime >= new DateTime(today.Year, today.Month, 1).AddMonths(-1)
                                              && x.CreateDateTime < new DateTime(today.Year, today.Month, 1)),

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
                    var pattern = $"%{search.ToLower()}%";
                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.NamaPasien ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaDokter ?? "", pattern) ||
                        EF.Functions.ILike(x.NoRekamMedis ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaPoliklinik ?? "", pattern) ||
                        EF.Functions.ILike(x.Antrian ?? "", pattern));
                }

                // =====================================================
                // 6) Sorting
                // =====================================================
                bool desc = (sortDirection ?? "desc").ToLower() == "desc";

                baseQuery = (orderBy ?? "CreateDateTime") switch
                {
                    "NamaPasien" => desc ? baseQuery.OrderByDescending(x => x.NamaPasien) : baseQuery.OrderBy(x => x.NamaPasien),
                    "NamaDokter" => desc ? baseQuery.OrderByDescending(x => x.NamaDokter) : baseQuery.OrderBy(x => x.NamaDokter),
                    "CreateDateTime" or _ => desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
                };

                // =====================================================
                // 7) Paging di DB
                // =====================================================
                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await baseQuery
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                // =====================================================
                // 8) Alergi hanya untuk data yang tampil (anti N+1 & hemat)
                // =====================================================
                var kunjunganIds = rows.Select(x => x.KunjunganID).ToList();

                var alergiMap = await _applicationDbContext.PainAssessments
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && kunjunganIds.Contains((Guid)x.KunjunganId))
                    .GroupBy(x => x.KunjunganId)
                    .Select(g => new
                    {
                        KunjunganId = g.Key,
                        AlergicList = g.Select(x => x.Alergic).Distinct().ToList()
                    })
                    .ToDictionaryAsync(x => x.KunjunganId, x => x.AlergicList);

                // =====================================================
                // 9) Response
                // =====================================================
                var host = $"{Request.Scheme}://{Request.Host}";

                var result = rows.Select(r =>
                {
                    alergiMap.TryGetValue(r.KunjunganID, out var al);

                    return new
                    {
                        r.KunjunganID,
                        r.AsuransiId,
                        r.NamaAsuransi,
                        r.PoliklinikId,
                        r.NamaPoliklinik,
                        r.DokterId,
                        r.NamaDokter,
                        r.PasienId,
                        r.AsalKunjungan,
                        r.NamaPasien,
                        r.TanggalLahir,
                        r.JenisKelamin,
                        r.NoPasien,
                        r.NoWali2,
                        r.NoWali3,
                        r.NamaWali2,
                        r.NamaWali3,
                        r.NamaKontakDarurat,
                        r.NoTeleponDarurat,
                        Email = r.EmailPasien,
                        r.AlamatDomisili,
                        r.AlamatDarurat,
                        r.AlamatIdentitas,
                        Umur = r.TanggalLahir.HasValue ? HitungUmurLengkap(r.TanggalLahir) : null,

                        r.NoRekamMedis,
                        r.TipePasien,
                        r.TipePembayaran,
                        r.JenisKunjungan,
                        r.StatusPengkajian,

                        r.CreateDateTime,
                        r.CreateBy,
                        r.CreateByName,

                        r.IsFinished,
                        r.IsScreening,
                        r.IsPresent,
                        r.IsTriage,
                        r.IsClosed,
                        r.IsCTTPasienIGD,
                        r.Antrian,
                        r.DepositRanap,
                        r.TglMasukKunjungan,
                        r.CaraMasukRS,
                        r.KondisiKeluar,
                        r.IsFinishedKasir,

                        gambardokter = !string.IsNullOrEmpty(r.FotoDokter)
                            ? $"{host}/FotoDokter/{r.FotoDokter}"
                            : $"{host}/FotoDokter/dokter.jpg",

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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


    }
}
