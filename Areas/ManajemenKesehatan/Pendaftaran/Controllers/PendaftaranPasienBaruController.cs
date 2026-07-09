using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Humanizer;
using System.Text.RegularExpressions;
using System.Globalization;
using ZXing.QrCode.Internal;
using System.IO;
using System.Net.Http.Headers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PendaftaranPasienBaruController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;
        private readonly INoRMGeneratorService _noRmGenerator;
        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienBaruController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<PendaftaranPasienBaruController> logger,
            IWebHostEnvironment webHostEnvironment,
            INoRMGeneratorService noRmGenerator
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _noRmGenerator = noRmGenerator;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }
        private static bool IsFilled(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private IActionResult? ValidateDataWali(
            string? namaWali,
            string? noWali,
            string? hubunganKeluarga,
            string labelWali)
        {
            bool hasNama = IsFilled(namaWali);
            bool hasNo = IsFilled(noWali);
            bool hasHubungan = IsFilled(hubunganKeluarga);

            // Kalau semua kosong => boleh
            if (!hasNama && !hasNo && !hasHubungan)
                return null;

            // Kalau nama wali kosong, tapi field lain ada isi => tidak boleh
            if (!hasNama && (hasNo || hasHubungan))
            {
                return BadRequest(new
                {
                    message = $"{labelWali} tidak valid. Jika Nama {labelWali} kosong, maka No {labelWali} dan Hubungan Keluarga juga harus kosong."
                });
            }

            // Kalau nama wali diisi, maka semua field wajib lengkap
            if (hasNama && (!hasNo || !hasHubungan))
            {
                return BadRequest(new
                {
                    message = $"{labelWali} tidak lengkap. Jika Nama {labelWali} diisi, maka No {labelWali} dan Hubungan Keluarga harus diisi semua."
                });
            }

            return null;
        }

        //test
        public static string HitungUmurLengkap(DateTime? tanggalLahir)
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
        public async Task<IActionResult> GetAllPendaftaranPasienBaru(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.PendaftaranPasienBarus
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienBaruId = a.PendaftaranPasienBaruId,
                            KodePasien = a.KodePasien,
                            NoRekamMedis = a.NoRekamMedis,
                            TipePasien = a.TipePasien,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            CatatanKhusus = a.CatatanKhusus,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
                            TitleId = a.TitleId,
                            IdentitasId = a.IdentitasId,
                            NoIdentitas = a.NoIdentitas,
                            TempatLahir = a.TempatLahir,
                            TipePendaftaran = a.TipePendaftaran,
                            TanggalLahir = a.TanggalLahir.HasValue ? a.TanggalLahir.Value.ToString("yyyy-MM-dd") : null,
                            Umur = HitungUmurLengkap(a.TanggalLahir),
                            StatusPerkawinan = a.StatusPerkawinan,
                            AgamaId = a.AgamaId,
                            NamaAgama = a.NamaAgama,
                            PendidikanTerakhirId = a.PendidikanTerakhirId,
                            AlamatIdentitas = a.AlamatIdentitas,
                            AlamatDomisili = a.AlamatDomisili,
                            NegaraId = a.NegaraId,
                            ProvinsiId = a.ProvinsiId,
                            KotaId = a.KotaId,
                            KecKabId = a.KecKabId,
                            KelurahanId = a.KelurahanId,
                            KodePos = a.KodePos,
                            Email = a.Email,
                            NoPasien = a.NoPasien,
                            NoWali1 = a.NoWali1,
                            NoWali2 = a.NoWali2,
                            NamaWali1 = a.NamaWali1,
                            NamaWali2 = a.NamaWali2,
                            Kewarganegaraan = a.Kewarganegaraan,
                            Suku = a.Suku,
                            StatusKewarganegaraan = a.StatusKewarganegaraan,
                            PekerjaanId = a.PekerjaanId,
                            NamaPerusahaan = a.NamaPerusahaan,
                            AlamatPerusahaan = a.AlamatPerusahaan,
                            NoTeleponPerusahaan = a.NoTeleponPerusahaan,
                            GolonganDarahId = a.GolonganDarahId,
                            Alergi = a.Alergi,
                            RiwayatPenyakit = a.RiwayatPenyakit,
                            RiwayatOperasi = a.RiwayatOperasi,
                            RiwayatPenyakitKeluarga = a.RiwayatPenyakitKeluarga,
                            HubunganKeluarga1 = a.HubunganKeluarga1,
                            HubunganPasien = a.HubunganPasien,
                            AlamatDarurat = a.AlamatDarurat,
                            NoTeleponDarurat = a.NoTeleponDarurat,
                            NamaOrangTua = a.NamaOrangTua,
                            IdentitasOrangTua = a.IdentitasOrangTua,
                            PekerjaanWali = a.PekerjaanWali,
                            HubunganKeluarga2 = a.HubunganKeluarga2,
                            HubunganKeluarga3 = a.HubunganKeluarga3,
                            NamaKontakDarurat = a.NamaKontakDarurat,
                            MembershipId = a.MembershipId,
                            a.TinggalBersama,
                            imageUrl = !string.IsNullOrEmpty(a.FotoName)
                                        ? $"/FotoPasienBaru/{a.FotoName}"
                                        : $"/FotoPasienBaru/user.jpg",
                            QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(a.QrCode)}",
                            
                        };

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
        public IActionResult GetPendaftraanPasienBaruById(Guid id)
        {
            var listdata = _applicationDbContext.PendaftaranPasienBarus
            .FirstOrDefault(p => p.PendaftaranPasienBaruId == id && !p.IsDelete);

            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }
            var parsed = listdata.TanggalLahir?.ToString("yyyy-MM-dd");

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = new
                {
                    listdata.PendaftaranPasienBaruId,
                    listdata.KodePasien,
                    listdata.NoRekamMedis,
                    listdata.TipePasien,
                    listdata.TipePendaftaran,
                    listdata.TitleId,
                    listdata.NamaLengkap,
                    listdata.KaryawanId,
                    listdata.NoKaryawan,
                    listdata.IdentitasId,
                    listdata.NoIdentitas,
                    listdata.TempatLahir,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(listdata.TanggalLahir),
                    listdata.JenisKelamin,
                    listdata.CatatanKhusus,
                    listdata.StatusPerkawinan,
                    listdata.AgamaId,
                    listdata.NamaAgama,
                    listdata.PendidikanTerakhirId,
                    listdata.AlamatIdentitas,
                    listdata.AlamatDomisili,
                    listdata.NegaraId,
                    listdata.ProvinsiId,
                    listdata.KotaId,
                    listdata.KecKabId,
                    listdata.KelurahanId,
                    listdata.KodePos,
                    listdata.Email,
                    listdata.NoPasien,
                    listdata.NoWali1,
                    listdata.NoWali2,
                    listdata.NamaWali1,
                    listdata.NamaWali2,
                    listdata.Kewarganegaraan,
                    listdata.Suku,
                    listdata.StatusKewarganegaraan,
                    listdata.PekerjaanId,
                    listdata.NamaPerusahaan,
                    listdata.AlamatPerusahaan,
                    listdata.NoTeleponPerusahaan,
                    listdata.GolonganDarahId,
                    listdata.Alergi,
                    listdata.RiwayatPenyakit,
                    listdata.RiwayatOperasi,
                    listdata.RiwayatPenyakitKeluarga,
                    listdata.HubunganKeluarga1,
                    listdata.HubunganPasien,
                    listdata.AlamatDarurat,
                    listdata.NoTeleponDarurat,
                    listdata.NamaOrangTua,
                    listdata.IdentitasOrangTua,
                    listdata.PekerjaanWali,
                    listdata.NamaKontakDarurat,
                    listdata.HubunganKeluarga2,
                    listdata.HubunganKeluarga3,
                    listdata.FotoName,
                    listdata.FotoPath,
                    listdata.TinggalBersama,
                    listdata.MembershipId,
                    imageUrl = !string.IsNullOrEmpty(listdata.FotoName)
                        ? $"/FotoPasienBaru/{listdata.FotoName}"
                        : $"/FotoPasienBaru/user.jpg",
                    QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(listdata.QrCode)}",
                }
            });
        }

        [HttpGet("No-RekamMedis/{noRm}")]
        public async Task<IActionResult> GetPendaftaranPasienBaruByNoRm(string noRm)
        {
            if (string.IsNullOrWhiteSpace(noRm))
                return BadRequest(new { message = "NoRM wajib diisi." });

            // 1) Cari pasien by NoRekamMedis
            var pasien = await _applicationDbContext.PendaftaranPasienBarus
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.NoRekamMedis == noRm && !p.IsDelete);

            if (pasien == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            // 2) Hitung "hari ini" berdasarkan WIB lalu ubah ke range UTC (biar query akurat & efisien)
            TimeZoneInfo tzJakarta;
            try { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
            catch { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } // Windows fallback

            var nowWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzJakarta);
            var startWib = nowWib.Date;               // 00:00 WIB hari ini
            var endWib = startWib.AddDays(1);         // 00:00 WIB besok

            var startUtc = new DateTimeOffset(startWib, tzJakarta.GetUtcOffset(startWib)).ToUniversalTime();
            var endUtc = new DateTimeOffset(endWib, tzJakarta.GetUtcOffset(endWib)).ToUniversalTime();

            // =============================
            // 🔎 Cek apakah pasien masih punya kunjungan aktif
            // =============================
            // OP: aktif hari ini (tanpa poli karena endpoint hanya NoRM)
            var hasActiveOPToday = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == pasien.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "OP" &&
                    k.CreateDateTime >= startUtc &&
                    k.CreateDateTime < endUtc);

            // IP: aktif kapan pun
            var hasActiveIP = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == pasien.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "IP");

            var hasActiveVisit = hasActiveOPToday || hasActiveIP;

            string? jenisAktif = null;
            if (hasActiveOPToday && hasActiveIP) jenisAktif = "OP,IP";
            else if (hasActiveOPToday) jenisAktif = "OP";
            else if (hasActiveIP) jenisAktif = "IP";

            // bad request jika pasien masih punya kunjungan aktif
            if (hasActiveVisit)
                return BadRequest(new { message = $"Pasien masih memiliki kunjungan aktif ({jenisAktif})." });

            var parsed = pasien.TanggalLahir?.ToString("yyyy-MM-dd");

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                hasActiveVisit,
                activeVisitType = jenisAktif,
                data = new
                {
                    pasien.PendaftaranPasienBaruId,
                    pasien.KodePasien,
                    pasien.NoRekamMedis,
                    pasien.TipePasien,
                    pasien.TitleId,
                    pasien.NamaLengkap,
                    pasien.KaryawanId,
                    pasien.NoKaryawan,
                    pasien.IdentitasId,
                    pasien.NoIdentitas,
                    pasien.TempatLahir,
                    pasien.CatatanKhusus,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(pasien.TanggalLahir),
                    pasien.JenisKelamin,
                    pasien.StatusPerkawinan,
                    pasien.AgamaId,
                    pasien.NamaAgama,
                    pasien.PendidikanTerakhirId,
                    pasien.AlamatIdentitas,
                    pasien.AlamatDomisili,
                    pasien.NegaraId,
                    pasien.ProvinsiId,
                    pasien.KotaId,
                    pasien.KecKabId,
                    pasien.KelurahanId,
                    pasien.KodePos,
                    pasien.Email,
                    pasien.NoPasien,
                    pasien.NoWali1,
                    pasien.NoWali2,
                    pasien.NamaWali1,
                    pasien.NamaWali2,
                    pasien.Kewarganegaraan,
                    pasien.Suku,
                    pasien.StatusKewarganegaraan,
                    pasien.PekerjaanId,
                    pasien.NamaPerusahaan,
                    pasien.AlamatPerusahaan,
                    pasien.NoTeleponPerusahaan,
                    pasien.GolonganDarahId,
                    pasien.Alergi,
                    pasien.RiwayatPenyakit,
                    pasien.RiwayatOperasi,
                    pasien.RiwayatPenyakitKeluarga,
                    pasien.HubunganKeluarga1,
                    pasien.HubunganPasien,
                    pasien.AlamatDarurat,
                    pasien.NoTeleponDarurat,
                    pasien.NamaKontakDarurat,
                    pasien.NamaOrangTua,
                    pasien.IdentitasOrangTua,
                    pasien.PekerjaanWali,
                    pasien.HubunganKeluarga2,
                    pasien.HubunganKeluarga3,
                    pasien.FotoName,
                    pasien.FotoPath,
                    pasien.MembershipId,
                    pasien.TinggalBersama,
                    imageUrl = !string.IsNullOrEmpty(pasien.FotoName)
                        ? $"/FotoPasienBaru/{pasien.FotoName}"
                        : $"/FotoPasienBaru/user.jpg",
                    QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(pasien.QrCode)}"
                }
            });
        }

        [HttpGet("Karyawan/{noKaryawan}")]
        public async Task<IActionResult> GetPendaftaranPasienBaruByNoKaryawan(string noKaryawan)
        {
            if (string.IsNullOrWhiteSpace(noKaryawan))
                return BadRequest(new { message = "NoRM wajib diisi." });

            // 1) Cari pasien by NoRekamMedis
            var pasien = await _applicationDbContext.PendaftaranPasienBarus
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.NoKaryawan == noKaryawan && !p.IsDelete);

            if (pasien == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            // 2) Hitung "hari ini" berdasarkan WIB lalu ubah ke range UTC (biar query akurat & efisien)
            TimeZoneInfo tzJakarta;
            try { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
            catch { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } // Windows fallback

            var nowWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzJakarta);
            var startWib = nowWib.Date;               // 00:00 WIB hari ini
            var endWib = startWib.AddDays(1);         // 00:00 WIB besok

            var startUtc = new DateTimeOffset(startWib, tzJakarta.GetUtcOffset(startWib)).ToUniversalTime();
            var endUtc = new DateTimeOffset(endWib, tzJakarta.GetUtcOffset(endWib)).ToUniversalTime();

            // =============================
            // 🔎 Cek apakah pasien masih punya kunjungan aktif
            // =============================
            // OP: aktif hari ini (tanpa poli karena endpoint hanya NoRM)
            var hasActiveOPToday = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == pasien.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "OP" &&
                    k.CreateDateTime >= startUtc &&
                    k.CreateDateTime < endUtc);

            // IP: aktif kapan pun
            var hasActiveIP = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == pasien.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "IP");

            var hasActiveVisit = hasActiveOPToday || hasActiveIP;

            string? jenisAktif = null;
            if (hasActiveOPToday && hasActiveIP) jenisAktif = "OP,IP";
            else if (hasActiveOPToday) jenisAktif = "OP";
            else if (hasActiveIP) jenisAktif = "IP";

            // bad request jika pasien masih punya kunjungan aktif
            if (hasActiveVisit)
                return BadRequest(new { message = $"Pasien masih memiliki kunjungan aktif ({jenisAktif})." });

            var parsed = pasien.TanggalLahir?.ToString("yyyy-MM-dd");

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                hasActiveVisit,
                activeVisitType = jenisAktif,
                data = new
                {
                    pasien.PendaftaranPasienBaruId,
                    pasien.KodePasien,
                    pasien.NoRekamMedis,
                    pasien.TipePasien,
                    pasien.TitleId,
                    pasien.NamaLengkap,
                    pasien.KaryawanId,
                    pasien.NoKaryawan,
                    pasien.IdentitasId,
                    pasien.NoIdentitas,
                    pasien.TempatLahir,
                    pasien.CatatanKhusus,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(pasien.TanggalLahir),
                    pasien.JenisKelamin,
                    pasien.StatusPerkawinan,
                    pasien.AgamaId,
                    pasien.NamaAgama,
                    pasien.PendidikanTerakhirId,
                    pasien.AlamatIdentitas,
                    pasien.AlamatDomisili,
                    pasien.NegaraId,
                    pasien.ProvinsiId,
                    pasien.KotaId,
                    pasien.KecKabId,
                    pasien.KelurahanId,
                    pasien.KodePos,
                    pasien.Email,
                    pasien.NoPasien,
                    pasien.NoWali1,
                    pasien.NoWali2,
                    pasien.NamaWali1,
                    pasien.NamaWali2,
                    pasien.Kewarganegaraan,
                    pasien.Suku,
                    pasien.StatusKewarganegaraan,
                    pasien.PekerjaanId,
                    pasien.NamaPerusahaan,
                    pasien.AlamatPerusahaan,
                    pasien.NoTeleponPerusahaan,
                    pasien.GolonganDarahId,
                    pasien.Alergi,
                    pasien.RiwayatPenyakit,
                    pasien.RiwayatOperasi,
                    pasien.RiwayatPenyakitKeluarga,
                    pasien.HubunganKeluarga1,
                    pasien.HubunganPasien,
                    pasien.AlamatDarurat,
                    pasien.NoTeleponDarurat,
                    pasien.NamaKontakDarurat,
                    pasien.NamaOrangTua,
                    pasien.IdentitasOrangTua,
                    pasien.PekerjaanWali,
                    pasien.HubunganKeluarga2,
                    pasien.HubunganKeluarga3,
                    pasien.FotoName,
                    pasien.FotoPath,
                    pasien.MembershipId,
                    pasien.TinggalBersama,
                    imageUrl = !string.IsNullOrEmpty(pasien.FotoName)
                        ? $"/FotoPasienBaru/{pasien.FotoName}"
                        : $"/FotoPasienBaru/user.jpg",
                    QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(pasien.QrCode)}"
                }
            });
        }

        [HttpGet("nik/{nik}")]
        public async Task<IActionResult> GetPendaftraanPasienBaruByNik(string nik)
        {
            var listdata = await _applicationDbContext.PendaftaranPasienBarus
                .Where(p => p.NoIdentitas == nik && !p.IsDelete)
                .FirstOrDefaultAsync();

            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            // =============================
            // 🔎 Cek apakah pasien masih punya kunjungan aktif
            // =============================
            // OP: aktif hari ini (tanpa poli karena endpoint hanya NoRM)
            TimeZoneInfo tzJakarta;
            try { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
            catch { tzJakarta = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } // Windows fallback

            var nowWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzJakarta);
            var startWib = nowWib.Date;               // 00:00 WIB hari ini
            var endWib = startWib.AddDays(1);         // 00:00 WIB besok

            var startUtc = new DateTimeOffset(startWib, tzJakarta.GetUtcOffset(startWib)).ToUniversalTime();
            var endUtc = new DateTimeOffset(endWib, tzJakarta.GetUtcOffset(endWib)).ToUniversalTime();

            var hasActiveOPToday = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == listdata.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "OP" &&
                    k.CreateDateTime >= startUtc &&
                    k.CreateDateTime < endUtc);

            // IP: aktif kapan pun
            var hasActiveIP = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .AnyAsync(k =>
                    k.PasienId == listdata.PendaftaranPasienBaruId &&
                    !k.IsDelete &&
                    k.IsFinished == false &&
                    k.IsFinishedKasir == false &&
                    k.JenisKunjungan == "IP");

            var hasActiveVisit = hasActiveOPToday || hasActiveIP;

            string? jenisAktif = null;
            if (hasActiveOPToday && hasActiveIP) jenisAktif = "OP,IP";
            else if (hasActiveOPToday) jenisAktif = "OP";
            else if (hasActiveIP) jenisAktif = "IP";

            // bad request jika pasien masih punya kunjungan aktif
            if (hasActiveVisit)
                return BadRequest(new { message = $"Pasien masih memiliki kunjungan aktif ({jenisAktif})." });

            var parsed = listdata.TanggalLahir?.ToString("yyyy-MM-dd");

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = new
                {
                    listdata.PendaftaranPasienBaruId,
                    listdata.KodePasien,
                    listdata.NoRekamMedis,
                    listdata.TipePasien,
                    listdata.TitleId,
                    listdata.NamaLengkap,
                    listdata.KaryawanId,
                    listdata.NoKaryawan,
                    listdata.IdentitasId,
                    listdata.NoIdentitas,
                    listdata.TempatLahir,
                    listdata.CatatanKhusus,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(listdata.TanggalLahir),
                    listdata.JenisKelamin,
                    listdata.StatusPerkawinan,
                    listdata.AgamaId,
                    listdata.NamaAgama,
                    listdata.PendidikanTerakhirId,
                    listdata.AlamatIdentitas,
                    listdata.AlamatDomisili,
                    listdata.NegaraId,
                    listdata.ProvinsiId,
                    listdata.KotaId,
                    listdata.KecKabId,
                    listdata.KelurahanId,
                    listdata.KodePos,
                    listdata.Email,
                    listdata.NoPasien,
                    listdata.NoWali1,
                    listdata.NoWali2,
                    listdata.NamaWali1,
                    listdata.NamaWali2,
                    listdata.Kewarganegaraan,
                    listdata.Suku,
                    listdata.StatusKewarganegaraan,
                    listdata.PekerjaanId,
                    listdata.NamaPerusahaan,
                    listdata.AlamatPerusahaan,
                    listdata.NoTeleponPerusahaan,
                    listdata.GolonganDarahId,
                    listdata.Alergi,
                    listdata.RiwayatPenyakit,
                    listdata.RiwayatOperasi,
                    listdata.RiwayatPenyakitKeluarga,
                    listdata.HubunganKeluarga1,
                    listdata.HubunganPasien,
                    listdata.AlamatDarurat,
                    listdata.NoTeleponDarurat,
                    listdata.NamaKontakDarurat,
                    listdata.NamaOrangTua,
                    listdata.IdentitasOrangTua,
                    listdata.PekerjaanWali,
                    listdata.HubunganKeluarga2,
                    listdata.HubunganKeluarga3,
                    listdata.FotoName,
                    listdata.FotoPath,
                    listdata.MembershipId,
                    listdata.TinggalBersama,
                    imageUrl = !string.IsNullOrEmpty(listdata.FotoName)
                        ? $"/FotoPasienBaru/{listdata.FotoName}"
                        : $"/FotoPasienBaru/user.jpg",
                    QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(listdata.QrCode)}"
                }
            });
        }

        [HttpGet("get-image/{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var fotoPath = _applicationDbContext.PendaftaranPasienBarus
                .Where(p => p.PendaftaranPasienBaruId == id)
                .Select(p => p.FotoPath)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fotoPath))
            {
                return NotFound(new { message = "Foto tidak ditemukan." });
            }

            // Pastikan path lengkap menggunakan wwwroot
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, fotoPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "File tidak ditemukan di server." });
            }

            var image = System.IO.File.OpenRead(fullPath);
            var contentType = GetContentType(fullPath);
            return File(image, contentType);
        }

        // Fungsi untuk mendapatkan MIME Type
        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" }
        };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        [HttpPost]
        public async Task<IActionResult> CreatePendaftaranPasienBaru([FromForm] PendaftaranPasienBaruViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            // Validasi Wali 1
            var validasiWali1 = ValidateDataWali(
                vm.NamaWali1,
                vm.NoWali1,
                vm.HubunganKeluarga1,
                "Wali 1");

            if (validasiWali1 != null)
                return validasiWali1;

            // Validasi Wali 2
            var validasiWali2 = ValidateDataWali(
                vm.NamaWali2,
                vm.NoWali2,
                vm.HubunganKeluarga2,
                "Wali 2");

            if (validasiWali2 != null)
                return validasiWali2;

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ==============================
                // 🔐 Ambil User Aktif dari JWT
                // ==============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                // ==========================================
                // ✅ Generate KodePasien (lebih aman pakai range, bukan .Date)
                // ==========================================
                var startToday = dateNow.Date;
                var endTodayEx = dateNow.Date.AddDays(1);

                var lastCode = await _applicationDbContext.PendaftaranPasienBarus
                    .Where(d => d.CreateDateTime >= startToday && d.CreateDateTime < endTodayEx)
                    .OrderByDescending(k => k.KodePasien)
                    .FirstOrDefaultAsync(ct);

                string kodePasien;
                if (lastCode == null)
                {
                    kodePasien = $"PSN{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodePasien.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kodePasien = $"PSN{setDateNow}0001";
                    }
                    else
                    {
                        kodePasien = $"PSN{setDateNow}" + (Convert.ToInt32(lastCode.KodePasien.Substring(9)) + 1).ToString("D4");
                    }
                }

                // =============================
                // VALIDASI KHUSUS KARYAWAN KIOSK
                // =============================
                if (!string.IsNullOrWhiteSpace(vm.NoKaryawan))
                {
                    vm.NoKaryawan = vm.NoKaryawan.Trim();

                    var isNoKaryawanExists = await _applicationDbContext.PendaftaranPasienBarus
                        .AnyAsync(x => x.NoKaryawan == vm.NoKaryawan, ct);

                    if (isNoKaryawanExists)
                    {
                        return Conflict(new
                        {
                            message = "Data sudah tersedia"
                        });
                    }
                }

                // =============================
                // ✅ Cek Duplikasi 
                // =============================
                var isDuplicate = await _applicationDbContext.PendaftaranPasienBarus
                    .AnyAsync(c => c.NoIdentitas == vm.NoIdentitas, ct);

                if (isDuplicate)
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });

                // =============================
                // ✅ Upload Foto ke Flask (mirip Lab)
                // =============================
                string fotoPath = "/FotoPasienBaru/user.jpg"; // default
                string fotoFileName = "user.jpg";

                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var maxSize = 2 * 1024 * 1024; // 2MB
                    var ext = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                        return BadRequest(new { message = "Format foto tidak valid. Gunakan JPG/PNG." });

                    if (vm.Foto.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    fotoFileName = $"{kodePasien}_{safeTime}{ext}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms, ct);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        {
                            new StreamContent(ms)
                            {
                                Headers = { ContentType = new MediaTypeHeaderValue(vm.Foto.ContentType) }
                            },
                            "file", fotoFileName
                        },
                        { new StringContent("FotoPasienBaru"), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync(_uploadUrl, content, ct);
                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload foto pasien ke server Flask." });

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync(ct);
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);

                    // fleksibel: tergantung response flask kamu pakai key apa
                    fotoPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? fotoPath;
                }

                // =============================
                // ✅ Parse TanggalLahir
                // =============================
                if (!DateTime.TryParseExact(vm.TanggalLahir, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                // =============================
                // ✅ Generate NoRM (setelah validasi duplikasi)
                // =============================
                var noRekamMedis = await _noRmGenerator.GenerateNoRekamMedisAsync(ct);

                // =============================
                // ✅ Generate QR Bytes + Upload ke Flask (tanpa /uploads)
                // =============================
                var folderTarget = "QRCodePasienBaru";
                var safeTimeQR = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                var qrCodeFileName = $"{noRekamMedis}_{safeTimeQR}.png";

                // Path yang kamu simpan di DB / response API (tanpa /uploads)
                var qrPath = $"/{folderTarget}/{qrCodeFileName}";

                // lokasi logo
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
                var qrCodeBytes = QrCodeHelper.GenerateQrCodeWithLogoPngBytes(noRekamMedis, logoPath);

                using var clientQR = new HttpClient();
                using var qrUploadStream = new MemoryStream(qrCodeBytes);
                qrUploadStream.Position = 0;

                var fileContent = new StreamContent(qrUploadStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                using var form = new MultipartFormDataContent();
                form.Add(fileContent, "file", qrCodeFileName);
                form.Add(new StringContent(folderTarget), "folderTarget");

                var flaskRespQR = await clientQR.PostAsync(_uploadUrl, form, ct);
                if (!flaskRespQR.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Gagal upload QR Code pasien ke server Flask." });

                // =============================
                // ✅ Simpan Data
                // =============================
                var daftar = new PendaftaranPasienBaru
                {
                    PendaftaranPasienBaruId = Guid.NewGuid(),
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId,
                    KodePasien = kodePasien,
                    NoRekamMedis = noRekamMedis,
                    TipePasien = vm.TipePasien,
                    TipePendaftaran = vm.TipePendaftaran,
                    TitleId = vm.TitleId,
                    NamaLengkap = vm.NamaLengkap,
                    IdentitasId = vm.IdentitasId,
                    NoIdentitas = vm.NoIdentitas,
                    TempatLahir = vm.TempatLahir,
                    CatatanKhusus = vm.CatatanKhusus,
                    TanggalLahir = parsedDate,
                    JenisKelamin = vm.JenisKelamin,
                    StatusPerkawinan = vm.StatusPerkawinan,
                    AgamaId = vm.AgamaId,
                    NamaAgama = vm.NamaAgama,
                    PendidikanTerakhirId = vm.PendidikanTerakhirId,
                    AlamatIdentitas = vm.AlamatIdentitas,
                    AlamatDomisili = vm.AlamatDomisili,
                    NegaraId = vm.NegaraId,
                    ProvinsiId = vm.ProvinsiId,
                    KotaId = vm.KotaId,
                    KecKabId = vm.KecKabId,
                    KelurahanId = vm.KelurahanId,
                    KodePos = vm.KodePos,
                    Email = vm.Email,
                    NoPasien = vm.NoPasien,
                    NoWali1 = vm.NoWali1,
                    NoWali2 = vm.NoWali2,
                    NamaWali1 = vm.NamaWali1,
                    NamaWali2 = vm.NamaWali2,
                    Kewarganegaraan = vm.Kewarganegaraan,
                    Suku = vm.Suku,
                    StatusKewarganegaraan = vm.StatusKewarganegaraan,
                    PekerjaanId = vm.PekerjaanId,
                    NamaPerusahaan = vm.NamaPerusahaan,
                    AlamatPerusahaan = vm.AlamatPerusahaan,
                    NoTeleponPerusahaan = vm.NoTeleponPerusahaan,
                    GolonganDarahId = vm.GolonganDarahId,
                    Alergi = vm.Alergi,
                    RiwayatPenyakit = vm.RiwayatPenyakit,
                    RiwayatOperasi = vm.RiwayatOperasi,
                    RiwayatPenyakitKeluarga = vm.RiwayatPenyakitKeluarga,
                    HubunganKeluarga1 = vm.HubunganKeluarga1,
                    HubunganPasien = vm.HubunganPasien,
                    NamaKontakDarurat = vm.NamaKontakDarurat,
                    AlamatDarurat = vm.AlamatDarurat,
                    NoTeleponDarurat = vm.NoTeleponDarurat,
                    NamaOrangTua = vm.NamaOrangTua,
                    IdentitasOrangTua = vm.IdentitasOrangTua,
                    PekerjaanWali = vm.PekerjaanWali,
                    HubunganKeluarga2 = vm.HubunganKeluarga2,
                    HubunganKeluarga3 = vm.HubunganKeluarga3,
                    MembershipId = vm.MembershipId,
                    TinggalBersama = vm.TinggalBersama,

                    // jika pasien adalah karyawan
                    NoKaryawan = vm.NoKaryawan,
                    KaryawanId = vm.KaryawanId,

                    // ✅ sesuai pola Lab (path hasil dari Flask)
                    FotoName = fotoFileName,
                    FotoPath = fotoPath,
                    QrCode = qrPath,
                };

                _applicationDbContext.PendaftaranPasienBarus.Add(daftar);
                await _applicationDbContext.SaveChangesAsync(ct);

                return Created("", new
                {
                    message = "Tambah Data Berhasil || 201 Created",
                    PasienBaruId = daftar.PendaftaranPasienBaruId,
                    NomorRekamMedis = daftar.NoRekamMedis,
                    qrCodeUrl = daftar.QrCode,
                    url = daftar.FotoPath
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePendaftaranPasien(Guid id, [FromForm] PendaftaranPasienBaruViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Ambil User ID dari JWT Claims
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // Cari Data Pasien
                var pasien = _applicationDbContext.PendaftaranPasienBarus.Find(id);
                if (pasien == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Konversi TanggalLahir dari string "yyyy-MM-dd" ke DateTime
                if (!DateTime.TryParseExact(
                    vm.TanggalLahir,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }

                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                // ==========================
                // FINAL VALUE UNTUK VALIDASI
                // ==========================
                // pakai nilai dari vm kalau dikirim
                // kalau tidak dikirim, pakai nilai lama dari database
                var namaWali1Final = vm.NamaWali1 != null ? vm.NamaWali1.Trim() : pasien.NamaWali1?.Trim();
                var noWali1Final = vm.NoWali1 != null ? vm.NoWali1.Trim() : pasien.NoWali1?.Trim();
                var hubunganKeluarga1Final = vm.HubunganKeluarga1 != null ? vm.HubunganKeluarga1.Trim() : pasien.HubunganKeluarga1?.Trim();

                var namaWali2Final = vm.NamaWali2 != null ? vm.NamaWali2.Trim() : pasien.NamaWali2?.Trim();
                var noWali2Final = vm.NoWali2 != null ? vm.NoWali2.Trim() : pasien.NoWali2?.Trim();
                var hubunganKeluarga2Final = vm.HubunganKeluarga2 != null ? vm.HubunganKeluarga2.Trim() : pasien.HubunganKeluarga2?.Trim();

                // validasi Wali 1
                var validasiWali1 = ValidateDataWali(
                    namaWali1Final,
                    noWali1Final,
                    hubunganKeluarga1Final,
                    "Wali 1");

                if (validasiWali1 != null)
                    return validasiWali1;

                // validasi Wali 2
                var validasiWali2 = ValidateDataWali(
                    namaWali2Final,
                    noWali2Final,
                    hubunganKeluarga2Final,
                    "Wali 2");

                if (validasiWali2 != null)
                    return validasiWali2;

                // Update Data Pasien
                pasien.TipePasien = vm.TipePasien;
                pasien.TipePendaftaran = vm.TipePendaftaran ?? pasien.TipePendaftaran;
                pasien.TitleId = vm.TitleId ?? pasien.TitleId;
                pasien.NamaLengkap = vm.NamaLengkap;
                pasien.IdentitasId = vm.IdentitasId;
                pasien.NoIdentitas = vm.NoIdentitas;
                pasien.TempatLahir = vm.TempatLahir ?? pasien.TempatLahir;
                pasien.TanggalLahir = vm.TanggalLahir != default ? parsedDate : pasien.TanggalLahir;
                pasien.JenisKelamin = vm.JenisKelamin ?? pasien.JenisKelamin;
                pasien.CatatanKhusus = vm.CatatanKhusus ?? pasien.CatatanKhusus;
                pasien.StatusPerkawinan = vm.StatusPerkawinan ?? pasien.StatusPerkawinan;
                pasien.AgamaId = vm.AgamaId ?? pasien.AgamaId;
                pasien.NamaAgama = vm.NamaAgama ?? pasien.NamaAgama;
                pasien.PendidikanTerakhirId = vm.PendidikanTerakhirId ?? pasien.PendidikanTerakhirId;
                pasien.AlamatIdentitas = vm.AlamatIdentitas ?? pasien.AlamatIdentitas;
                pasien.AlamatDomisili = vm.AlamatDomisili ?? pasien.AlamatDomisili;
                pasien.NegaraId = vm.NegaraId ?? pasien.NegaraId;
                pasien.ProvinsiId = vm.ProvinsiId ?? pasien.ProvinsiId;
                pasien.KotaId = vm.KotaId ?? pasien.KotaId;
                pasien.KecKabId = vm.KecKabId ?? pasien.KecKabId;
                pasien.KelurahanId = vm.KelurahanId ?? pasien.KelurahanId;
                pasien.KodePos = vm.KodePos ?? pasien.KodePos;
                pasien.Email = vm.Email ?? pasien.Email;
                pasien.NoPasien = vm.NoPasien ?? pasien.NoPasien;
                pasien.NoWali1 = vm.NoWali1 ?? pasien.NoWali1;
                pasien.NamaWali1 = vm.NamaWali1 ?? pasien.NamaWali1;
                pasien.NamaWali1 = namaWali1Final;
                pasien.NoWali1 = noWali1Final;
                pasien.HubunganKeluarga1 = hubunganKeluarga1Final;
                pasien.NamaWali2 = namaWali2Final;
                pasien.NoWali2 = noWali2Final;
                pasien.HubunganKeluarga2 = hubunganKeluarga2Final;
                pasien.Kewarganegaraan = vm.Kewarganegaraan ?? pasien.Kewarganegaraan;
                pasien.Suku = vm.Suku ?? pasien.Suku;
                pasien.StatusKewarganegaraan = vm.StatusKewarganegaraan ?? pasien.StatusKewarganegaraan;
                pasien.PekerjaanId = vm.PekerjaanId ?? pasien.PekerjaanId;
                pasien.NamaPerusahaan = vm.NamaPerusahaan ?? pasien.NamaPerusahaan;
                pasien.AlamatPerusahaan = vm.AlamatPerusahaan ?? pasien.AlamatPerusahaan;
                pasien.NoTeleponPerusahaan = vm.NoTeleponPerusahaan ?? pasien.NoTeleponPerusahaan;
                pasien.GolonganDarahId = vm.GolonganDarahId ?? pasien.GolonganDarahId;
                pasien.Alergi = vm.Alergi ?? pasien.Alergi;
                pasien.RiwayatPenyakit = vm.RiwayatPenyakit ?? pasien.RiwayatPenyakit;
                pasien.RiwayatOperasi = vm.RiwayatOperasi ?? pasien.RiwayatOperasi;
                pasien.RiwayatPenyakitKeluarga = vm.RiwayatPenyakitKeluarga ?? pasien.RiwayatPenyakitKeluarga;
                pasien.HubunganKeluarga1 = vm.HubunganKeluarga1 ?? pasien.HubunganKeluarga1;
                pasien.HubunganPasien = vm.HubunganPasien ?? pasien.HubunganPasien;
                pasien.AlamatDarurat = vm.AlamatDarurat ?? pasien.AlamatDarurat;
                pasien.NoTeleponDarurat = vm.NoTeleponDarurat ?? pasien.NoTeleponDarurat;
                pasien.NamaKontakDarurat = vm.NamaKontakDarurat ?? pasien.NamaKontakDarurat;
                pasien.NamaOrangTua = vm.NamaOrangTua ?? pasien.NamaOrangTua;
                pasien.IdentitasOrangTua = vm.IdentitasOrangTua ?? pasien.IdentitasOrangTua;
                pasien.PekerjaanWali = vm.PekerjaanWali ?? pasien.PekerjaanWali;
                pasien.MembershipId = vm.MembershipId ?? pasien.MembershipId;
                pasien.TinggalBersama = vm.TinggalBersama ?? pasien.TinggalBersama;

                // Update Foto Profil Jika Ada
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024; // Maksimum 2MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var fotoFileName = $"{pasien.KodePasien}{fileExtension}";
                    var oldFileName = pasien.FotoName ?? "";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        {
                            new StreamContent(ms)
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
                            }, "file", fotoFileName
                        },
                        { new StringContent("FotoPasienBaru"), "folderTarget" },
                        { new StringContent(oldFileName), "oldFileName" }
                    };

                    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                    if (!flaskResponse.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    pasien.FotoName = fotoFileName;
                    pasien.FotoPath = $"/FotoPasienBaru/{fotoFileName}";
                }

                pasien.UpdateBy = userActiveId;
                pasien.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    qrCodeUrl = $"{pasien.QrCode}",
                    uploadFotoUrl = $"{pasien.FotoPath}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePendaftaranPasien(Guid id)
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

                // **Cari Data Pasien**
                var pasien = _applicationDbContext.PendaftaranPasienBarus.Find(id);
                if (pasien == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                pasien.DeleteBy = UserActiveId;
                pasien.DeleteDateTime = DateTimeOffset.UtcNow;
                pasien.IsDelete = true;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedPendaftaranPasienBaru(
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
            var query = from a in _applicationDbContext.PendaftaranPasienBarus
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienBaruId = a.PendaftaranPasienBaruId,
                            KodePasien = a.KodePasien,
                            NoRekamMedis = a.NoRekamMedis,
                            TipePasien = a.TipePasien,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            KaryawanId = a.KaryawanId,
                            NoKaryawan = a.NoKaryawan,
                            CatatanKhusus = a.CatatanKhusus,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
                            TitleId = a.TitleId,
                            IdentitasId = a.IdentitasId,
                            NoIdentitas = a.NoIdentitas,
                            TempatLahir = a.TempatLahir,
                            TipePendaftaran = a.TipePendaftaran,
                            TanggalLahir = a.TanggalLahir.HasValue ? a.TanggalLahir.Value.ToString("yyyy-MM-dd") : null,
                            Umur = HitungUmurLengkap(a.TanggalLahir),
                            StatusPerkawinan = a.StatusPerkawinan,
                            AgamaId = a.AgamaId,
                            NamaAgama = a.NamaAgama,
                            PendidikanTerakhirId = a.PendidikanTerakhirId,
                            AlamatIdentitas = a.AlamatIdentitas,
                            AlamatDomisili = a.AlamatDomisili,
                            NegaraId = a.NegaraId,
                            ProvinsiId = a.ProvinsiId,
                            KotaId = a.KotaId,
                            KecKabId = a.KecKabId,
                            KelurahanId = a.KelurahanId,
                            KodePos = a.KodePos,
                            Email = a.Email,
                            NoPasien = a.NoPasien,
                            NoWali1 = a.NoWali1,
                            NoWali2 = a.NoWali2,
                            NamaWali1 = a.NamaWali1,
                            NamaWali2 = a.NamaWali2,
                            Kewarganegaraan = a.Kewarganegaraan,
                            Suku = a.Suku,
                            StatusKewarganegaraan = a.StatusKewarganegaraan,
                            PekerjaanId = a.PekerjaanId,
                            NamaPerusahaan = a.NamaPerusahaan,
                            AlamatPerusahaan = a.AlamatPerusahaan,
                            NoTeleponPerusahaan = a.NoTeleponPerusahaan,
                            GolonganDarahId = a.GolonganDarahId,
                            Alergi = a.Alergi,
                            RiwayatPenyakit = a.RiwayatPenyakit,
                            RiwayatOperasi = a.RiwayatOperasi,
                            RiwayatPenyakitKeluarga = a.RiwayatPenyakitKeluarga,
                            HubunganKeluarga1 = a.HubunganKeluarga1,
                            HubunganPasien = a.HubunganPasien,
                            AlamatDarurat = a.AlamatDarurat,
                            NoTeleponDarurat = a.NoTeleponDarurat,
                            NamaKontakDarurat = a.NamaKontakDarurat,
                            NamaOrangTua = a.NamaOrangTua,
                            IdentitasOrangTua = a.IdentitasOrangTua,
                            PekerjaanWali = a.PekerjaanWali,
                            HubunganKeluarga2 = a.HubunganKeluarga2,
                            HubunganKeluarga3 = a.HubunganKeluarga3,
                            MembershipId = a.MembershipId,
                            a.TinggalBersama,
                            imageUrl = !string.IsNullOrEmpty(a.FotoName)
                                        ? $"/FotoPasienBaru/{a.FotoName}"
                                        : $"/FotoPasienBaru/user.jpg",
                            QRUrl = $"/QRCodePasienBaru/{Path.GetFileName(a.QrCode)}",
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLengkap, search) ||
                    EF.Functions.ILike(u.KodePasien, search) ||
                    EF.Functions.ILike(u.NoRekamMedis, search) ||
                    EF.Functions.ILike(u.NoIdentitas, search)
                );
            }

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
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
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
                    "KodePasien" => query.OrderByDescending(u => u.KodePasien),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "NoIdentitas" => query.OrderByDescending(u => u.NoIdentitas),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePasien" => query.OrderByDescending(u => u.KodePasien),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "NoIdentitas" => query.OrderByDescending(u => u.NoIdentitas),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            //Pagination
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
