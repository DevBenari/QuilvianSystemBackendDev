using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;

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

        private readonly ILogger<KunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<KunjunganController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKunjungan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Hitung jumlah kunjungan per PasienId + JenisKunjungan
            var jumlahPerJenis = _applicationDbContext.Kunjungans
                .Where(k => !k.IsDelete)
                .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                .Select(g => new
                {
                    g.Key.PasienId,
                    g.Key.JenisKunjungan,
                    JumlahJenis = g.Count()
                });

            var query = (from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                        join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId
                        join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
                        from o in asuransiGroup.DefaultIfEmpty() // Left Join Asuransi
                        join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId
                        join j in jumlahPerJenis on new { a.PasienId, a.JenisKunjungan } equals new { j.PasienId, j.JenisKunjungan }
                        where a.IsDelete == false 
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai", // Cek apakah ada asuransi
                            a.PoliklinikId,
                            p.NamaPoliklinik,
                            a.DokterId,
                            a.PasienId,
                            ps.NamaLengkap,
                            ps.TanggalLahir,    
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.JenisKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            a.IsFinished,
                            a.IsScreening,
                            a.IsPresent,
                            a.Antrian,
                            d.NmDokter,
                            gambardokter = !string.IsNullOrEmpty(d.FotoName)
                                ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                                : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                            CreateByName = u.FullName,

                            // ⬅️ Tambahan jumlah jenis kunjungan
                            JumlahJenisKunjungan = j.JumlahJenis
                        }).OrderByDescending(a => a.CreateDateTime);

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rawData = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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
        public async Task<IActionResult> GetKunjunganById(Guid id)
        {

            // Hitung jumlah kunjungan per PasienId + JenisKunjungan
            var jumlahPerJenis = _applicationDbContext.Kunjungans
                .Where(k => !k.IsDelete)
                .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                .Select(g => new
                {
                    g.Key.PasienId,
                    g.Key.JenisKunjungan,
                    JumlahJenis = g.Count()
                });

            var query = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                        join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId
                        join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
                        from o in asuransiGroup.DefaultIfEmpty() // Left Join Asuransi
                        join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId
                        join j in jumlahPerJenis on new { a.PasienId, a.JenisKunjungan } equals new { j.PasienId, j.JenisKunjungan }
                        where a.IsDelete == false && a.KunjunganID == id
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai", // Cek apakah ada asuransi
                            a.PoliklinikId,
                            p.NamaPoliklinik,
                            a.DokterId,
                            a.PasienId,
                            ps.NamaLengkap,
                            ps.TanggalLahir,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.JenisKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            a.IsFinished,
                            a.IsScreening,
                            a.IsPresent,
                            a.IsFinishedKasir, 
                            a.Antrian,
                            d.NmDokter,
                            gambardokter = !string.IsNullOrEmpty(d.FotoName)
                                ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                                : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                            CreateByName = u.FullName,

                            // ⬅️ Tambahan jumlah jenis kunjungan
                            JumlahJenisKunjungan = j.JumlahJenis
                        };

            return Ok(new
            {
                message = "Data kunjungan berhasil ditemukan.",
                data = query,
            });
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

                // Validasi jenis kunjungan
                // jika tidak diisi automatis "Rawat Jalan"
                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) || request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";


                // Ambil kode antrean dari tabel Poliklinik
                var kodePoli = _applicationDbContext.Polikliniks
                    .Where(p => p.PoliklinikId == request.PoliklinikId)
                    .Select(p => p.KodeAntreanPoli)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(kodePoli))
                    return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                // Hitung nomor antrian hari ini berdasarkan Poliklinik
                var today = DateTime.UtcNow.Date;
                var isAlreadyRegistered = _applicationDbContext.Kunjungans.Any(k =>
                    k.PoliklinikId == request.PoliklinikId &&
                    k.DokterId == request.DokterId &&
                    k.PasienId == request.PasienId &&
                    k.CreateDateTime.Date == today &&
                    !k.IsDelete && k.IsFinished == false);

                if (isAlreadyRegistered)
                {
                    return BadRequest(new { message = "Pasien sudah terdaftar untuk kunjungan dengan poli dan dokter yang sama pada hari ini." });
                }
                var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                    .Count(k => k.PoliklinikId == request.PoliklinikId
                                && k.CreateDateTime.Date == today
                                && !k.IsDelete);

                int nomorAntrian = jumlahAntrianHariIni + 1;
                string nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}"; // Contoh: BU001

                var newKunjungan = new Kunjungan
                {
                    KunjunganID = Guid.NewGuid(),
                    PasienId = request.PasienId,
                    DokterId = request.DokterId,
                    PoliklinikId = request.PoliklinikId,
                    AsuransiId = request.AsuransiId,
                    //JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan),
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
                    IsFinishedKasir = false, // Default value
                    Antrian = nomorAntrianFormatted   // Format akhir: BU001
                };

                _applicationDbContext.Kunjungans.Add(newKunjungan);

                // cari data biaya admin berdasarkan jenis kunjungan
                var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
                    .Where(b => b.BiayaAdministrasiKode == kodeJenis)
                    .FirstOrDefaultAsync();

                var bill = new Billing
                {
                    BillingId = Guid.NewGuid(),
                    KunjunganId = newKunjungan.KunjunganID,
                    DiskonId = null, // Atur sesuai kebutuhan
                    ItemId = biayaAdmin?.BiayaAdministrasiId ?? Guid.Empty,
                    NamaItem = biayaAdmin?.NamaBiayaAdministrasi ?? "Biaya Administrasi",
                    HargaItem = biayaAdmin?.NominalBiayaAdministrasi ?? 0,
                    QtyItem = 1,
                    SubTotalItem = biayaAdmin?.NominalBiayaAdministrasi ?? 0,
                    BillingKode = "Biaya Admin",
                    BillingDate = DateTime.UtcNow,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = UserActiveId
                };
                _applicationDbContext.Billings.Add(bill);

                await _applicationDbContext.SaveChangesAsync();
                return Ok(new
                {
                    message = "Kunjungan baru berhasil ditambahkan.",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        JenisKunjungan = inputJenis,
                        NomorAntrian = nomorAntrianFormatted
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKunjunganPasien(Guid id, [FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
                return BadRequest(new { message = "Data tidak boleh kosong!" });

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                var userActiveId = getUserActive?.UserActiveId ?? Guid.Empty;

                var existingKunjungan = await _applicationDbContext.Kunjungans.FindAsync(id);
                if (existingKunjungan == null)
                    return NotFound(new { message = "Data kunjungan tidak ditemukan!" });

                // Validasi tipe pasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });

                var inputJenis = string.IsNullOrWhiteSpace(request.JenisKunjungan) || request.JenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                    ? "Rawat Jalan"
                    : request.JenisKunjungan;

                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });

                string kodeJenis = inputJenis == "Rawat Inap" ? "IP" : "OP";

                // Ambil kode antrean dari tabel Poliklinik
                var kodePoli = _applicationDbContext.Polikliniks
                    .Where(p => p.PoliklinikId == request.PoliklinikId)
                    .Select(p => p.KodeAntreanPoli)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(kodePoli))
                    return BadRequest(new { message = "Kode antrean poli tidak ditemukan untuk poliklinik ini!" });

                // Hitung nomor antrian hari ini berdasarkan Poliklinik
                var today = DateTime.UtcNow.Date;

                var jumlahAntrianHariIni = _applicationDbContext.Kunjungans
                    .Count(k => k.PoliklinikId == request.PoliklinikId
                                && k.CreateDateTime.Date == today
                                && !k.IsDelete);

                int nomorAntrian = jumlahAntrianHariIni + 1;
                string nomorAntrianFormatted = $"{kodePoli}{nomorAntrian:000}";

                // Hitung jumlah kunjungan berdasarkan poliklinik hari ini
                //var allKunjunganPasien = _applicationDbContext.Kunjungans
                //    .Where(k => k.PoliklinikId == request.PoliklinikId && k.CreateDateTime.Date == today && !k.IsDelete)
                //    .ToList();

                //List<KunjunganRiwayat> jumlahKunjungan = new()
                //{
                //    new KunjunganRiwayat
                //    {
                //        Jenis = "IP",
                //        Jumlah = allKunjunganPasien
                //            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                //            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                //            .Where(k => k.Jenis == "IP")
                //            .Sum(k => k.Jumlah)
                //    },
                //    new KunjunganRiwayat
                //    {
                //        Jenis = "OP",
                //        Jumlah = allKunjunganPasien
                //            .Where(k => !string.IsNullOrEmpty(k.JumlahKunjungan))
                //            .SelectMany(k => JsonSerializer.Deserialize<List<KunjunganRiwayat>>(k.JumlahKunjungan) ?? new List<KunjunganRiwayat>())
                //            .Where(k => k.Jenis == "OP")
                //            .Sum(k => k.Jumlah)
                //    }
                //};

                //var currentJenis = jumlahKunjungan.FirstOrDefault(k => k.Jenis == kodeJenis);
                //if (currentJenis != null)
                //    currentJenis.Jumlah += 1;
                //else
                //    jumlahKunjungan.Add(new KunjunganRiwayat { Jenis = kodeJenis, Jumlah = 1 });

                // Update semua field
                existingKunjungan.PasienId = request.PasienId;
                existingKunjungan.DokterId = request.DokterId;
                existingKunjungan.PoliklinikId = request.PoliklinikId;
                existingKunjungan.AsuransiId = request.AsuransiId;
                existingKunjungan.NoRekamMedis = request.NoRekamMedis;
                existingKunjungan.TipePasien = request.TipePasien;
                existingKunjungan.TipePembayaran = request.TipePembayaran;

                existingKunjungan.JenisKunjungan = kodeJenis;
                existingKunjungan.Antrian = nomorAntrianFormatted;
                existingKunjungan.UpdateDateTime = DateTimeOffset.UtcNow;
                existingKunjungan.UpdateBy = userActiveId;

                // Simpan nilai jenis kunjungan lama untuk deteksi perubahan
                var jenisKunjunganLama = existingKunjungan.JenisKunjungan;
                // Update billing "Biaya Admin" jika ada dan jenis kunjungan berubah
                if (!string.Equals(jenisKunjunganLama, kodeJenis, StringComparison.OrdinalIgnoreCase))
                {
                    var existingBilling = await _applicationDbContext.Billings
                        .FirstOrDefaultAsync(b => b.KunjunganId == existingKunjungan.KunjunganID && b.BillingKode == "Biaya Admin");

                    if (existingBilling != null)
                    {
                        var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
                            .FirstOrDefaultAsync(b => b.BiayaAdministrasiKode == kodeJenis);

                        if (biayaAdmin != null)
                        {
                            existingBilling.ItemId = biayaAdmin.BiayaAdministrasiId;
                            existingBilling.NamaItem = biayaAdmin.NamaBiayaAdministrasi;
                            existingBilling.HargaItem = biayaAdmin.NominalBiayaAdministrasi;
                            existingBilling.SubTotalItem = biayaAdmin.NominalBiayaAdministrasi;
                            existingBilling.BillingDate = DateTime.UtcNow;
                        }
                    }
                }

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Data kunjungan berhasil diperbarui.",
                    data = new
                    {
                        request.PasienId,
                        request.DokterId,
                        request.PoliklinikId,
                        JenisKunjungan = inputJenis,
                        NomorAntrian = nomorAntrianFormatted
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

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
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

            return Ok(new { message = "Status IsScreening berhasil diperbarui." });
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

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedKunjunganAsync(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null,
        [FromQuery] bool? isFinished = null, 
        [FromQuery] bool? isScreening = null, 
        [FromQuery] bool? isPresent = null,
        [FromQuery] bool? isFinishedKasir = null,
        [FromQuery] TipePasienFilter? TipePasien = null
        )

        {

            //Hitung jumlah kunjungan per PasienId + JenisKunjungan
            var jumlahPerJenis = _applicationDbContext.Kunjungans
                .Where(k => !k.IsDelete)
                .GroupBy(k => new { k.PasienId, k.JenisKunjungan })
                .Select(g => new
                {
                    g.Key.PasienId,
                    g.Key.JenisKunjungan,
                    JumlahJenis = g.Count()
                });
            

            var query = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                        join p in _applicationDbContext.Polikliniks on a.PoliklinikId equals p.PoliklinikId
                        join o in _applicationDbContext.Asuransis on a.AsuransiId equals o.AsuransiId into asuransiGroup
                        from o in asuransiGroup.DefaultIfEmpty() // Left Join Asuransi
                        join ps in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals ps.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters on a.DokterId equals d.DokterId
                        join j in jumlahPerJenis on new { a.PasienId, a.JenisKunjungan } equals new { j.PasienId, j.JenisKunjungan }
                        where a.IsDelete == false
                        select new
                        {
                            a.KunjunganID,
                            a.AsuransiId,
                            NamaAsuransi = o != null && o.NamaAsuransi != null ? o.NamaAsuransi : "Tunai", // Cek apakah ada asuransi
                            a.PoliklinikId,
                            p.NamaPoliklinik,
                            a.DokterId,
                            a.PasienId,
                            ps.NamaLengkap,
                            ps.TanggalLahir,
                            a.NoRekamMedis,
                            a.TipePasien,
                            a.TipePembayaran,
                            a.JenisKunjungan,
                            a.CreateDateTime,
                            a.CreateBy,
                            a.IsFinished,
                            a.IsScreening,
                            a.IsPresent,
                            a.Antrian,
                            a.IsFinishedKasir,
                            d.NmDokter,
                            gambardokter = !string.IsNullOrEmpty(d.FotoName)
                                ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                                : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

                            CreateByName = u.FullName,

                            // ⬅️ Tambahan jumlah jenis kunjungan
                            JumlahJenisKunjungan = j.JumlahJenis
                        };
            // ✅ Filter berdasarkan isFinished jika diberikan
            if (isFinished.HasValue)
            {
                query = query.Where(u => u.IsFinished == isFinished.Value);
            }

            // ✅ Filter berdasarkan IsPresent jika diberikan
            if (isPresent.HasValue)
            {
                query = query.Where(u => u.IsPresent == isPresent.Value);
            }

            // ✅ Filter berdasarkan isFinished jika diberikan
            if (isScreening.HasValue)
            {
                query = query.Where(u => u.IsScreening == isScreening.Value);
            }

            // ✅ Filter berdasarkan isFinished jika diberikan
            if (TipePasien.HasValue)
            {
                query = query.Where(u => u.TipePasien == TipePasien.Value.ToString());
            }

            // ✅ Filter berdasarkan isFinished jika diberikan
            if (isFinishedKasir.HasValue)
            {
                query = query.Where(u => u.IsFinishedKasir == isFinishedKasir.Value);
            }


            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLengkap, search) ||
                    EF.Functions.ILike(u.NmDokter, search) ||
                    EF.Functions.ILike(u.NoRekamMedis, search) ||
                    EF.Functions.ILike(u.NamaPoliklinik, search) ||
                    EF.Functions.ILike(u.Antrian, search)
                );
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

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
                            u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek)));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.AddMonths(-1).Month &&
                            u.CreateDateTime.Year == today.AddMonths(-1).Year);
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

            query = sortDirection?.ToLower() == "asc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderBy(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderBy(u => u.TipePasien),
                    "Nama Dokter" => query.OrderBy(u => u.NmDokter),
                    "Nama Poliklinik" => query.OrderBy(u => u.NamaPoliklinik),
                    _ => query.OrderBy(u => u.CreateDateTime)

                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderByDescending(u => u.TipePasien),
                    "Nama Dokter" => query.OrderByDescending(u => u.NmDokter),
                    "Nama Poliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            // 🔸 Jumlah Jenis Kunjungan per pasien (terpisah, sebagai summary)

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
                    TotalPages = totalPages,
                }
            });
        }
    }
}
