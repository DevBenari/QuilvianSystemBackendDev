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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DarahPermintaanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DarahPermintaanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DarahPermintaanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DarahPermintaanController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
            var query = (from a in _applicationDbContext.DarahPermintaans
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         // join ke table komponen darah id
                         join kd in _applicationDbContext.Darahs
                         on a.KomponenDarahId equals kd.KomponenDarahId into kdGroup
                         from kd in kdGroup.DefaultIfEmpty()

                             // join ke table golongan darah id
                         join gd in _applicationDbContext.GolonganDarahs
                         on a.GolonganDarahId equals gd.GolonganDarahId into gdGroup
                         from gd in gdGroup.DefaultIfEmpty()

                             // join ke table kunjungan
                         join k in _applicationDbContext.Kunjungans
                         on a.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                             // join ke table pasien
                         join p in _applicationDbContext.PendaftaranPasienBarus
                         on a.PasienId equals p.PendaftaranPasienBaruId into pGroup
                         from p in pGroup.DefaultIfEmpty()

                             // join ke table dokter
                         join d1 in _applicationDbContext.Dokters
                         on a.DokterPerujukId equals d1.DokterId into d1Group
                         from d1 in d1Group.DefaultIfEmpty()

                         join d2 in _applicationDbContext.Dokters
                            on a.DokterBDRSId equals d2.DokterId into d2Group
                         from d2 in d2Group.DefaultIfEmpty()

                             // join ke poliklinik
                         join po in _applicationDbContext.Polikliniks
                         on k.PoliklinikId equals po.PoliklinikId into poGroup
                         from po in poGroup.DefaultIfEmpty()

                             // 🔹 JOIN tambahan — data ruangan (BookingBedRanap → Bed → Kamar → Kelas)
                         join bb in _applicationDbContext.BookingBedRanaps
                             on k.KunjunganID equals bb.KunjunganId into bookingGroup
                         from bb in bookingGroup.DefaultIfEmpty()

                         join b in _applicationDbContext.Beds
                             on bb.BedId equals b.BedId into bedGroup
                         from b in bedGroup.DefaultIfEmpty()

                         join km in _applicationDbContext.Kamars
                             on bb.KamarId equals km.KamarId into kamarGroup
                         from km in kamarGroup.DefaultIfEmpty()

                         join kl in _applicationDbContext.Kelass
                             on km.KelasId equals kl.KelasId into kelasGroup
                         from kl in kelasGroup.DefaultIfEmpty()


                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             a.BankDarahId,
                             a.KunjunganId,
                             k.NoRekamMedis,
                             k.DokterId,
                             a.PasienId,
                             p.NamaLengkap,
                             a.KomponenDarahId,
                             kd.NamaKomponenDarah,
                             kd.KodeKomponenDarah,
                             a.GolonganDarahId,
                             gd.NamaGolonganDarah,
                             a.JumlahKantong,
                             a.Rhesus,
                             a.TglPemesanan,
                             a.WaktuPemesanan,
                             a.TglDiperlukan,
                             a.DokterBDRSId,
                             DokterBDRS = d1.NmDokter,
                             a.DokterPerujukId,
                             DokterPerujuk = d2.NmDokter,
                             a.Petugas,
                             NamaPetugas = u.FullName,
                             a.Keterangan,

                             // 🔹 Data tambahan ruangan
                             KelasId = kl != null ? kl.KelasId : (Guid?)null,
                             NamaKelas = kl != null ? kl.NamaKelas : null,
                             KamarId = km != null ? km.KamarId : (Guid?)null,
                             NamaKamar = km != null ? km.NamaKamar : null,
                             LantaiKamar = km != null ? km.Lantai : null,
                             BedId = b != null ? b.BedId : (Guid?)null,
                             NomorBed = b != null ? b.NomorBed : null,
                             StatusBed = bb != null ? bb.StatusBed : null,
                             TglMasuk = bb != null ? bb.TglMasuk : null,
                             TglKeluar = bb != null ? bb.TglKeluar : null
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
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Query utama: ambil data berdasarkan ID
                var data = await (
                    from a in _applicationDbContext.DarahPermintaans

                        // join ke user (pembuat)
                    join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                        // join ke komponen darah
                    join kd in _applicationDbContext.Darahs
                        on a.KomponenDarahId equals kd.KomponenDarahId into kdGroup
                    from kd in kdGroup.DefaultIfEmpty()

                        // join ke golongan darah
                    join gd in _applicationDbContext.GolonganDarahs
                        on a.GolonganDarahId equals gd.GolonganDarahId into gdGroup
                    from gd in gdGroup.DefaultIfEmpty()

                        // join ke kunjungan
                    join k in _applicationDbContext.Kunjungans
                        on a.KunjunganId equals k.KunjunganID into kGroup
                    from k in kGroup.DefaultIfEmpty()

                        // join ke pasien
                    join p in _applicationDbContext.PendaftaranPasienBarus
                        on a.PasienId equals p.PendaftaranPasienBaruId into pGroup
                    from p in pGroup.DefaultIfEmpty()

                        // join ke dokter perujuk
                    join d1 in _applicationDbContext.Dokters
                        on a.DokterPerujukId equals d1.DokterId into d1Group
                    from d1 in d1Group.DefaultIfEmpty()

                        // join ke dokter BDRS
                    join d2 in _applicationDbContext.Dokters
                        on a.DokterBDRSId equals d2.DokterId into d2Group
                    from d2 in d2Group.DefaultIfEmpty()

                        // join ke poliklinik
                    join po in _applicationDbContext.Polikliniks
                        on k.PoliklinikId equals po.PoliklinikId into poGroup
                    from po in poGroup.DefaultIfEmpty()

                        // join ke kamar dan bed untuk info ruangan
                    join bb in _applicationDbContext.BookingBedRanaps
                        on k.KunjunganID equals bb.KunjunganId into bookingGroup
                    from bb in bookingGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Beds
                        on bb.BedId equals b.BedId into bedGroup
                    from b in bedGroup.DefaultIfEmpty()

                    join km in _applicationDbContext.Kamars
                        on bb.KamarId equals km.KamarId into kamarGroup
                    from km in kamarGroup.DefaultIfEmpty()

                    join kl in _applicationDbContext.Kelass
                        on km.KelasId equals kl.KelasId into kelasGroup
                    from kl in kelasGroup.DefaultIfEmpty()

                    where a.IsDelete == false || a.IsDelete == null
                    where a.BankDarahId == id

                    select new
                    {
                        // --- Data Utama ---
                        a.BankDarahId,
                        a.KunjunganId,
                        a.PasienId,
                        a.KomponenDarahId,
                        a.GolonganDarahId,
                        a.JumlahKantong,
                        a.Rhesus,
                        a.TglPemesanan,
                        a.WaktuPemesanan,
                        a.TglDiperlukan,
                        a.DokterPerujukId,
                        a.DokterBDRSId,
                        a.Petugas,
                        a.Keterangan,
                        a.CreateDateTime,
                        a.UpdateDateTime,

                        // --- Data Tambahan Join ---
                        CreateByName = u.FullName,
                        NamaPasien = p.NamaLengkap,
                        p.TanggalLahir,
                        p.JenisKelamin,
                        k.NoRekamMedis,
                        Poliklinik = po.NamaPoliklinik,
                        DokterPerujuk = d1.NmDokter,
                        DokterBDRS = d2.NmDokter,
                        NamaKomponenDarah = kd.NamaKomponenDarah,
                        KodeKomponenDarah = kd.KodeKomponenDarah,
                        NamaGolonganDarah = gd.NamaGolonganDarah,

                        // --- Info Ruangan ---
                        KelasId = kl != null ? kl.KelasId : (Guid?)null,
                        NamaKelas = kl != null ? kl.NamaKelas : null,
                        KamarId = km != null ? km.KamarId : (Guid?)null,
                        NamaKamar = km != null ? km.NamaKamar : null,
                        LantaiKamar = km != null ? km.Lantai : null,
                        BedId = b != null ? b.BedId : (Guid?)null,
                        NomorBed = b != null ? b.NomorBed : null,
                        StatusBed = bb != null ? bb.StatusBed : null,
                        TglMasuk = bb != null ? bb.TglMasuk : null,
                        TglKeluar = bb != null ? bb.TglKeluar : null
                    }
                ).AsNoTracking().FirstOrDefaultAsync();

                // ✅ Jika tidak ditemukan
                if (data == null)
                    return NotFound(new { message = $"Data Darah Permintaan dengan ID {id} tidak ditemukan || 404 Not Found" });

                // ✅ Return hasil
                return Ok(new
                {
                    message = "Berhasil mengambil data Darah Permintaan || 200 OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DarahPermintaanViewModel vm)
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

                ////// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new DarahPermintaan
                {
                    BankDarahId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    KomponenDarahId = vm.KomponenDarahId,
                    GolonganDarahId = vm.GolonganDarahId,
                    JumlahKantong = vm.JumlahKantong,
                    Rhesus = vm.Rhesus,
                    TglPemesanan = TryParseTanggalToUtc(vm.TglPemesanan),
                    WaktuPemesanan = vm.WaktuPemesanan,
                    TglDiperlukan = TryParseTanggalToUtc(vm.TglDiperlukan),
                    DokterPerujukId = vm.DokterPerujukId,
                    DokterBDRSId = vm.DokterBDRSId,
                    Petugas = vm.Petugas,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };
                // **Simpan ke Database**
                _applicationDbContext.DarahPermintaans.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] DarahPermintaanViewModel vm)
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

                // **Cari Data Lama**
                var data = await _applicationDbContext.DarahPermintaans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data permintaan darah tidak ditemukan." });
                }

                // **Update Data**
                data.KomponenDarahId = vm.KomponenDarahId;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.GolonganDarahId = vm.GolonganDarahId;
                data.JumlahKantong = vm.JumlahKantong;
                data.Rhesus = vm.Rhesus;
                data.TglPemesanan = TryParseTanggalToUtc(vm.TglPemesanan);
                data.WaktuPemesanan = vm.WaktuPemesanan;
                data.TglDiperlukan = TryParseTanggalToUtc(vm.TglDiperlukan);
                data.DokterPerujukId = vm.DokterPerujukId;
                data.DokterBDRSId = vm.DokterBDRSId;
                data.Petugas = vm.Petugas;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DarahPermintaans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
                }
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
                var data = await _applicationDbContext.DarahPermintaans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DarahPermintaans.Update(data);
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

            // Query data
            var query = (from a in _applicationDbContext.DarahPermintaans
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         // join ke table komponen darah id
                         join kd in _applicationDbContext.Darahs
                         on a.KomponenDarahId equals kd.KomponenDarahId into kdGroup
                         from kd in kdGroup.DefaultIfEmpty()

                         // join ke table golongan darah id
                         join gd in _applicationDbContext.GolonganDarahs 
                         on a.GolonganDarahId equals gd.GolonganDarahId into gdGroup
                         from gd in gdGroup.DefaultIfEmpty()

                         // join ke table kunjungan
                         join k in _applicationDbContext.Kunjungans
                         on a.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                         // join ke table pasien
                         join p in _applicationDbContext.PendaftaranPasienBarus
                         on a.PasienId equals p.PendaftaranPasienBaruId into pGroup
                         from p in pGroup.DefaultIfEmpty()

                         // join ke table dokter
                         join d1 in _applicationDbContext.Dokters
                         on a.DokterPerujukId equals d1.DokterId into d1Group
                         from d1 in d1Group.DefaultIfEmpty()

                         join d2 in _applicationDbContext.Dokters
                            on a.DokterBDRSId equals d2.DokterId into d2Group
                         from d2 in d2Group.DefaultIfEmpty()
                         
                         // join ke poliklinik
                         join po in _applicationDbContext.Polikliniks
                         on k.PoliklinikId equals po.PoliklinikId into poGroup
                         from po in poGroup.DefaultIfEmpty()
                             
                         // 🔹 JOIN tambahan — data ruangan (BookingBedRanap → Bed → Kamar → Kelas)
                         join bb in _applicationDbContext.BookingBedRanaps
                             on k.KunjunganID equals bb.KunjunganId into bookingGroup
                         from bb in bookingGroup.DefaultIfEmpty()

                         join b in _applicationDbContext.Beds
                             on bb.BedId equals b.BedId into bedGroup
                         from b in bedGroup.DefaultIfEmpty()

                         join km in _applicationDbContext.Kamars
                             on bb.KamarId equals km.KamarId into kamarGroup
                         from km in kamarGroup.DefaultIfEmpty()

                         join kl in _applicationDbContext.Kelass
                             on km.KelasId equals kl.KelasId into kelasGroup
                         from kl in kelasGroup.DefaultIfEmpty()


                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             a.BankDarahId,
                             a.KunjunganId,
                             k.NoRekamMedis,
                             k.DokterId,
                             a.PasienId,
                             p.NamaLengkap,
                             a.KomponenDarahId,
                             kd.NamaKomponenDarah,
                             kd.KodeKomponenDarah,
                             a.GolonganDarahId,
                             gd.NamaGolonganDarah,
                             a.JumlahKantong,
                             a.Rhesus,
                             a.TglPemesanan,
                             a.WaktuPemesanan,
                             a.TglDiperlukan,
                             a.DokterBDRSId,
                             DokterBDRS = d1.NmDokter,
                             a.DokterPerujukId,
                             DokterPerujuk = d2.NmDokter,
                             a.Petugas,
                             NamaPetugas = u.FullName,
                             a.Keterangan,

                             // 🔹 Data tambahan ruangan
                             KelasId = kl != null ? kl.KelasId : (Guid?)null,
                             NamaKelas = kl != null ? kl.NamaKelas : null,
                             KamarId = km != null ? km.KamarId : (Guid?)null,
                             NamaKamar = km != null ? km.NamaKamar : null,
                             LantaiKamar = km != null ? km.Lantai : null,
                             BedId = b != null ? b.BedId : (Guid?)null,
                             NomorBed = b != null ? b.NomorBed : null,
                             StatusBed = bb != null ? bb.StatusBed : null,
                             TglMasuk = bb != null ? bb.TglMasuk : null,
                             TglKeluar = bb != null ? bb.TglKeluar : null
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter berdasarkan kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // filter berdasarkan pasien id
            if (pasienId.HasValue) { 
                query = query.Where(u=>u.PasienId== pasienId.Value);
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
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
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
