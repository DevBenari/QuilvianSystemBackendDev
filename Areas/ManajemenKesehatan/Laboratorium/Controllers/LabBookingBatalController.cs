using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class LabBookingBatalController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LabBookingBatalController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabBookingBatalController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingBatalController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            var batal = await _applicationDbContext.LabBookingBatals
                .AsNoTracking()
                .Where(x =>
                    x.BatalBookingLabId == id &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new
                {
                    x.BatalBookingLabId,
                    x.LabBookingId,
                    x.DetailLabBookingId,
                    x.JenisPembatalan,
                    x.TglPembatalan,
                    x.Keterangan,

                    x.CreateDateTime,
                    x.CreateBy,
                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    x.UpdateDateTime,
                    x.UpdateBy,
                    UpdateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.UpdateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    x.DeleteDateTime,
                    x.DeleteBy,
                    x.IsDelete,

                    // =========================
                    // Lab Booking terkait
                    // =========================
                    LabBooking = x.LabBooking == null ? null : new
                    {
                        x.LabBooking.BookingLabId,
                        x.LabBooking.KunjunganId,
                        x.LabBooking.PasienId,
                        x.LabBooking.AsuransiId,
                        x.LabBooking.TglPenyerahanSampling,
                        x.LabBooking.TglBooking,
                        x.LabBooking.TglPemeriksaan,
                        x.LabBooking.KelasId,
                        x.LabBooking.DokterKonsulenId,
                        x.LabBooking.TerapisId,
                        x.LabBooking.DokterPerujukId,
                        x.LabBooking.DokterPemeriksaId,
                        x.LabBooking.KonfirmatorId,

                        x.LabBooking.NoOrder,
                        x.LabBooking.NoLab,
                        x.LabBooking.NoPA,
                        x.LabBooking.StatusBookingLab,
                        x.LabBooking.StatusPemeriksaan,
                        x.LabBooking.StatusKonfirmasi,
                        x.LabBooking.ProsesBooking,
                        x.LabBooking.TindakLanjut,
                        x.LabBooking.TglKonfirmasi,
                        x.LabBooking.Keterangan,
                        x.LabBooking.DiagnosaAwal,
                        x.LabBooking.IsLunas,
                        x.LabBooking.IsPasienPersiapan,
                        x.LabBooking.SuratRujukan,
                        x.LabBooking.AlasanPembatalan,
                        x.LabBooking.TTDPathPembatalan,
                        x.LabBooking.PetugasPembatalan,
                        x.LabBooking.WaktuPemeriksaan,
                        x.LabBooking.WaktuPemeriksaanPersiapan,

                        NamaPasien = x.LabBooking.Pasien != null
                            ? x.LabBooking.Pasien.NamaLengkap
                            : null,

                        NoRekamMedis = x.LabBooking.Pasien != null
                            ? x.LabBooking.Pasien.NoRekamMedis
                            : null,

                        JenisKelamin = x.LabBooking.Pasien != null
                            ? x.LabBooking.Pasien.JenisKelamin
                            : null,

                        NoRegistrasi = x.LabBooking.Kunjungan != null
                            ? x.LabBooking.Kunjungan.NoRegistrasi
                            : null,

                        JenisKunjungan = x.LabBooking.Kunjungan != null
                            ? x.LabBooking.Kunjungan.JenisKunjungan
                            : null,

                        AsalKunjungan = x.LabBooking.Kunjungan != null
                            ? x.LabBooking.Kunjungan.AsalKunjungan
                            : null,

                        NamaKelas = x.LabBooking.Kelas != null
                            ? x.LabBooking.Kelas.NamaKelas
                            : null,

                        NamaAsuransi = x.LabBooking.Asuransi != null
                            ? x.LabBooking.Asuransi.NamaAsuransi
                            : null,

                        NamaDokterPerujuk = x.LabBooking.DokterPerujuk != null
                            ? x.LabBooking.DokterPerujuk.NmDokter
                            : null,

                        NamaDokterPemeriksa = x.LabBooking.DokterPemeriksa != null
                            ? x.LabBooking.DokterPemeriksa.NmDokter
                            : null,

                        NamaKonfirmator = x.LabBooking.Konfirmator != null
                            ? x.LabBooking.Konfirmator.FullName
                            : null
                    },

                    // =========================
                    // Detail yang dibatalkan
                    // =========================
                    DetailDibatalkan = x.LabBookingDetail == null ? null : new
                    {
                        x.LabBookingDetail.DetailBookingLabId,
                        x.LabBookingDetail.BookingLabId,
                        x.LabBookingDetail.PasienId,
                        x.LabBookingDetail.PemeriksaanLabId,
                        x.LabBookingDetail.LabId,
                        x.LabBookingDetail.DokterPemeriksaId,

                        NamaLab = x.LabBookingDetail.Lab != null
                            ? x.LabBookingDetail.Lab.NamaLab
                            : null,

                        NamaPemeriksaan = x.LabBookingDetail.PemeriksaanLab != null
                            ? x.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan
                            : null,

                        HargaPemeriksaan = x.LabBookingDetail.PemeriksaanLab != null
                            ? x.LabBookingDetail.PemeriksaanLab.HargaPemeriksaan
                            : null,

                        NamaDokterPemeriksa = x.LabBookingDetail.DokterPemeriksa != null
                            ? x.LabBookingDetail.DokterPemeriksa.NmDokter
                            : null,

                        x.LabBookingDetail.TipeLayanan,
                        x.LabBookingDetail.NoPhoto,
                        x.LabBookingDetail.QtyOrder,
                        x.LabBookingDetail.StatusPemeriksaan,
                        x.LabBookingDetail.TanggalSelesai,
                        x.LabBookingDetail.StatusVerifikasi,
                        x.LabBookingDetail.IsCito,
                        x.LabBookingDetail.AlasanPembatalan,
                        x.LabBookingDetail.TTDPembatalanPath,

                        x.LabBookingDetail.KategoriPatologiAnatomi,
                        x.LabBookingDetail.JenisSpecimen,
                        x.LabBookingDetail.LokasiSpecimen,
                        x.LabBookingDetail.KeteranganKlinik,
                        x.LabBookingDetail.PenyakitSebelumnya,
                        x.LabBookingDetail.PenggunaanFiksasi,
                        x.LabBookingDetail.JenisPemeriksaanGC,
                        x.LabBookingDetail.JenisGC,
                        x.LabBookingDetail.BahanNonGC,
                        x.LabBookingDetail.BahanMicrobiologi,
                        x.LabBookingDetail.MasaHaidTerakhir,

                        x.LabBookingDetail.CreateDateTime,
                        x.LabBookingDetail.CreateBy,
                        x.LabBookingDetail.UpdateDateTime,
                        x.LabBookingDetail.UpdateBy,
                        x.LabBookingDetail.IsDelete
                    }
                })
                .FirstOrDefaultAsync(ct);

            if (batal == null)
            {
                return NotFound(new
                {
                    message = "Data pembatalan booking lab tidak ditemukan. || 404 Not Found"
                });
            }

            // ==========================================================
            // Ambil semua detail dalam BookingLabId yang sama
            // Dipisah dari query utama agar tidak membuat query besar/cartesian.
            // ==========================================================
            var labBookingDetails = new List<object>();

            if (batal.LabBookingId.HasValue)
            {
                labBookingDetails = await _applicationDbContext.LabBookingDetails
                    .AsNoTracking()
                    .Where(d =>
                        d.BookingLabId == batal.LabBookingId.Value &&
                        (d.IsDelete == false || d.IsDelete == null))
                    .OrderBy(d => d.CreateDateTime)
                    .Select(d => new
                    {
                        d.DetailBookingLabId,
                        d.BookingLabId,
                        d.PasienId,
                        d.PemeriksaanLabId,
                        d.LabId,
                        d.DokterPemeriksaId,

                        NamaLab = d.Lab != null
                            ? d.Lab.NamaLab
                            : null,

                        NamaPemeriksaan = d.PemeriksaanLab != null
                            ? d.PemeriksaanLab.NamaPemeriksaan
                            : null,

                        KodePemeriksaan = d.PemeriksaanLab != null
                            ? d.PemeriksaanLab.KodePemeriksaan
                            : null,

                        HargaPemeriksaan = d.PemeriksaanLab != null
                            ? d.PemeriksaanLab.HargaPemeriksaan
                            : null,

                        NamaDokterPemeriksa = d.DokterPemeriksa != null
                            ? d.DokterPemeriksa.NmDokter
                            : null,

                        d.TipeLayanan,
                        d.NoPhoto,
                        d.QtyOrder,
                        d.StatusPemeriksaan,
                        d.TanggalSelesai,
                        d.StatusVerifikasi,
                        d.IsCito,

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

                        d.AlasanPembatalan,
                        d.TTDPembatalanPath,

                        IsDetailYangDibatalkan = d.DetailBookingLabId == batal.DetailLabBookingId,

                        d.CreateDateTime,
                        d.CreateBy,
                        CreateByName = _applicationDbContext.UserActives
                            .Where(u => u.UserActiveId == d.CreateBy)
                            .Select(u => u.FullName)
                            .FirstOrDefault(),

                        d.UpdateDateTime,
                        d.UpdateBy,
                        d.DeleteDateTime,
                        d.DeleteBy,
                        d.IsDelete
                    })
                    .Cast<object>()
                    .ToListAsync(ct);
            }

            return Ok(new
            {
                status = "success",
                message = "Data ditemukan. || 200 OK",
                data = new
                {
                    batal.BatalBookingLabId,
                    batal.LabBookingId,
                    batal.DetailLabBookingId,
                    batal.JenisPembatalan,
                    batal.TglPembatalan,
                    batal.Keterangan,

                    batal.CreateDateTime,
                    batal.CreateBy,
                    batal.CreateByName,
                    batal.UpdateDateTime,
                    batal.UpdateBy,
                    batal.UpdateByName,
                    batal.DeleteDateTime,
                    batal.DeleteBy,
                    batal.IsDelete,

                    LabBooking = batal.LabBooking,
                    DetailDibatalkan = batal.DetailDibatalkan,
                    LabBookingDetails = labBookingDetails
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabBookingBatalViewModel vm)
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

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.LabBookingBatals
                    .AnyAsync(c => c.LabBookingId == vm.LabBookingId &&
                    c.DetailLabBookingId == vm.DetailLabBookingId &&
                    c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Pemesanan lab ini telah dibatalkan" });
                }

                // **Buat Data Baru**
                var data = new LabBookingBatal
                {
                    BatalBookingLabId = Guid.NewGuid(),
                    LabBookingId = vm.LabBookingId,
                    DetailLabBookingId = vm.DetailLabBookingId,
                    JenisPembatalan = vm.JenisPembatalan,
                    TglPembatalan = vm.TglPembatalan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.LabBookingBatals.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingBatalViewModel vm)
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
                var data = await _applicationDbContext.LabBookingBatals.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.LabBookingBatals
                    .AnyAsync(c => c.LabBookingId == vm.LabBookingId &&
                    c.DetailLabBookingId == vm.DetailLabBookingId &&
                    c.BatalBookingLabId != id &&
                    c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Pemesanan lab ini telah dibatalkan" });
                }

                // **Update Data**
                data.LabBookingId = vm.LabBookingId;
                data.DetailLabBookingId = vm.DetailLabBookingId;
                data.JenisPembatalan = vm.JenisPembatalan;
                data.TglPembatalan = vm.TglPembatalan;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabBookingBatals.Update(data);
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
                var data = await _applicationDbContext.LabBookingBatals.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabBookingBatals.Update(data);
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
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            var query = _applicationDbContext.LabBookingBatals
                .AsNoTracking()
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .Select(x => new
                {
                    x.BatalBookingLabId,
                    x.LabBookingId,
                    x.DetailLabBookingId,
                    x.JenisPembatalan,
                    x.TglPembatalan,
                    x.Keterangan,

                    x.CreateDateTime,
                    x.CreateBy,

                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    x.UpdateDateTime,
                    x.UpdateBy,
                    x.DeleteDateTime,
                    x.DeleteBy,
                    x.IsDelete,

                    // =========================
                    // Data Lab Booking
                    // =========================

                    NoOrder = x.LabBooking != null
                        ? x.LabBooking.NoOrder
                        : null,

                    NoLab = x.LabBooking != null
                        ? x.LabBooking.NoLab
                        : null,

                    NoPA = x.LabBooking != null
                        ? x.LabBooking.NoPA
                        : null,

                    StatusBookingLab = x.LabBooking != null
                        ? x.LabBooking.StatusBookingLab
                        : null,

                    StatusPemeriksaanBooking = x.LabBooking != null
                        ? x.LabBooking.StatusPemeriksaan
                        : null,

                    StatusKonfirmasi = x.LabBooking != null
                        ? x.LabBooking.StatusKonfirmasi
                        : null,

                    ProsesBooking = x.LabBooking != null
                        ? x.LabBooking.ProsesBooking
                        : null,

                    TglBooking = x.LabBooking != null
                        ? x.LabBooking.TglBooking
                        : null,

                    TglPemeriksaan = x.LabBooking != null
                        ? x.LabBooking.TglPemeriksaan
                        : null,

                    TglKonfirmasi = x.LabBooking != null
                        ? x.LabBooking.TglKonfirmasi
                        : null,

                    KunjunganId = x.LabBooking != null
                        ? x.LabBooking.KunjunganId
                        : null,

                    NoRegistrasi = x.LabBooking != null && x.LabBooking.Kunjungan != null
                        ? x.LabBooking.Kunjungan.NoRegistrasi
                        : null,

                    JenisKunjungan = x.LabBooking != null && x.LabBooking.Kunjungan != null
                        ? x.LabBooking.Kunjungan.JenisKunjungan
                        : null,

                    AsalKunjungan = x.LabBooking != null && x.LabBooking.Kunjungan != null
                        ? x.LabBooking.Kunjungan.AsalKunjungan
                        : null,

                    PasienId = x.LabBooking != null
                        ? x.LabBooking.PasienId
                        : null,

                    NamaPasien = x.LabBooking != null && x.LabBooking.Pasien != null
                        ? x.LabBooking.Pasien.NamaLengkap
                        : null,

                    NoRekamMedis = x.LabBooking != null && x.LabBooking.Pasien != null
                        ? x.LabBooking.Pasien.NoRekamMedis
                        : null,

                    JenisKelamin = x.LabBooking != null && x.LabBooking.Pasien != null
                        ? x.LabBooking.Pasien.JenisKelamin
                        : null,

                    KelasId = x.LabBooking != null
                        ? x.LabBooking.KelasId
                        : null,

                    NamaKelas = x.LabBooking != null && x.LabBooking.Kelas != null
                        ? x.LabBooking.Kelas.NamaKelas
                        : null,

                    AsuransiId = x.LabBooking != null
                        ? x.LabBooking.AsuransiId
                        : null,

                    NamaAsuransi = x.LabBooking != null && x.LabBooking.Asuransi != null
                        ? x.LabBooking.Asuransi.NamaAsuransi
                        : null,

                    DokterPerujukId = x.LabBooking != null
                        ? x.LabBooking.DokterPerujukId
                        : null,

                    NamaDokterPerujuk = x.LabBooking != null && x.LabBooking.DokterPerujuk != null
                        ? x.LabBooking.DokterPerujuk.NmDokter
                        : null,

                    DokterPemeriksaBookingId = x.LabBooking != null
                        ? x.LabBooking.DokterPemeriksaId
                        : null,

                    NamaDokterPemeriksaBooking = x.LabBooking != null && x.LabBooking.DokterPemeriksa != null
                        ? x.LabBooking.DokterPemeriksa.NmDokter
                        : null,

                    // =========================
                    // Data Detail Lab Booking
                    // =========================

                    DetailBookingLabId = x.LabBookingDetail != null
                        ? x.LabBookingDetail.DetailBookingLabId
                        : (Guid?)null,

                    PemeriksaanLabId = x.LabBookingDetail != null
                        ? x.LabBookingDetail.PemeriksaanLabId
                        : null,

                    NamaPemeriksaan = x.LabBookingDetail != null && x.LabBookingDetail.PemeriksaanLab != null
                        ? x.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan
                        : null,

                    KodePemeriksaan = x.LabBookingDetail != null && x.LabBookingDetail.PemeriksaanLab != null
                        ? x.LabBookingDetail.PemeriksaanLab.KodePemeriksaan
                        : null,

                    HargaPemeriksaan = x.LabBookingDetail != null && x.LabBookingDetail.PemeriksaanLab != null
                        ? x.LabBookingDetail.PemeriksaanLab.HargaPemeriksaan
                        : null,

                    LabId = x.LabBookingDetail != null
                        ? x.LabBookingDetail.LabId
                        : null,

                    NamaLab = x.LabBookingDetail != null && x.LabBookingDetail.Lab != null
                        ? x.LabBookingDetail.Lab.NamaLab
                        : null,

                    DokterPemeriksaDetailId = x.LabBookingDetail != null
                        ? x.LabBookingDetail.DokterPemeriksaId
                        : null,

                    NamaDokterPemeriksaDetail = x.LabBookingDetail != null && x.LabBookingDetail.DokterPemeriksa != null
                        ? x.LabBookingDetail.DokterPemeriksa.NmDokter
                        : null,

                    NoPhoto = x.LabBookingDetail != null
                        ? x.LabBookingDetail.NoPhoto
                        : null,

                    TipeLayanan = x.LabBookingDetail != null
                        ? x.LabBookingDetail.TipeLayanan
                        : null,

                    QtyOrder = x.LabBookingDetail != null
                        ? x.LabBookingDetail.QtyOrder
                        : null,

                    StatusPemeriksaanDetail = x.LabBookingDetail != null
                        ? x.LabBookingDetail.StatusPemeriksaan
                        : null,

                    TanggalSelesai = x.LabBookingDetail != null
                        ? x.LabBookingDetail.TanggalSelesai
                        : null,

                    IsCito = x.LabBookingDetail != null
                        ? x.LabBookingDetail.IsCito
                        : null,

                    AlasanPembatalanDetail = x.LabBookingDetail != null
                        ? x.LabBookingDetail.AlasanPembatalan
                        : null,

                    TTDPembatalanPath = x.LabBookingDetail != null
                        ? x.LabBookingDetail.TTDPembatalanPath
                        : null
                });

            // ======================================================
            // Search
            // ======================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NoRegistrasi ?? "", keyword) ||
                    EF.Functions.ILike(x.NamaPasien ?? "", keyword) ||
                    EF.Functions.ILike(x.NoRekamMedis ?? "", keyword) 
                );
            }

            // ======================================================
            // Filter tanggal berdasarkan CreateDateTime
            // ======================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // ======================================================
            // Filter periode berdasarkan CreateDateTime
            // ======================================================
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        {
                            var lastMonth = today.AddMonths(-1);

                            query = query.Where(x =>
                                x.CreateDateTime.Month == lastMonth.Month &&
                                x.CreateDateTime.Year == lastMonth.Year);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // ======================================================
            // Sorting
            // ======================================================
            var isDesc = sortDirection?.ToLower() == "desc";

            query = isDesc
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // ======================================================
            // Pagination
            // ======================================================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = Array.Empty<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

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
