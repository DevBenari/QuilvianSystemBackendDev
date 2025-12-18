using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class IGDTindakLanjutController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<IGDTindakLanjutController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IGDTindakLanjutController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDTindakLanjutController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await (from a in _applicationDbContext.IGDTindakLanjuts
                                  join u1 in _applicationDbContext.UserActives
                                      on a.CreateBy equals u1.UserActiveId into u1Group
                                  from createBy in u1Group.DefaultIfEmpty()

                                  join u2 in _applicationDbContext.UserActives
                                      on a.UpdateBy equals u2.UserActiveId into u2Group
                                  from updateBy in u2Group.DefaultIfEmpty()

                                  where a.TindakLanjutIgdId == id
                                        && (a.IsDelete == false || a.IsDelete == null)

                                  select new
                                  {
                                      a.TindakLanjutIgdId,
                                      a.KunjunganId,
                                      a.PasienId,
                                      a.KamarId,

                                      a.WaktuPindah,
                                      a.TindakanLanjutan,
                                      a.StatusPasien,
                                      a.WaktuStatus,
                                      a.KontrolKe,
                                      a.WaktuKontrol,
                                      a.Transportasi,
                                      a.AlasanMenolakDirawat,
                                      a.RsRujukan,
                                      a.AlasanDirujuk,

                                      // Kondisi Pasien
                                      a.TingkatKesadaran,
                                      a.Eyes,
                                      a.Motorik,
                                      a.Verbal,
                                      a.Pupil,
                                      a.Reaksi,

                                      // Vital Sign
                                      a.Suhu,
                                      a.TekananDarahSystolic,
                                      a.TekananDarahDiastolic,
                                      a.Nadi,
                                      a.RR,
                                      a.SPO2,

                                      // Pemeriksaan
                                      a.HasilLabId,
                                      a.HasilCTScanId,
                                      a.HasilEKGId,
                                      a.HasilRontgenId,
                                      a.HasilUSGId,

                                      // Lembar hasil
                                      a.LembarLab,
                                      a.LembarCTScan,
                                      a.LembarEKG,
                                      a.LembarRontgen,
                                      a.LembarUSG,

                                      a.PerawatIgdId,
                                      a.PerawatKamarId,

                                      a.Keterangan,

                                      // Audit
                                      a.CreateBy,
                                      CreateByName = createBy.FullName,
                                      a.CreateDateTime,

                                      a.UpdateBy,
                                      UpdateByName = updateBy.FullName,
                                      a.UpdateDateTime
                                  }).FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { message = $"Data Tindak Lanjut IGD dengan ID {id} tidak ditemukan." });

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data
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
        public async Task<IActionResult> CreateAsync([FromBody] IGDTindakLanjutViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // --- Create Data ---
                var data = new IGDTindakLanjut
                {
                    TindakLanjutIgdId = Guid.NewGuid(),

                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    KamarId = vm.KamarId,

                    WaktuPindah = vm.WaktuPindah,

                    TindakanLanjutan = vm.TindakanLanjutan,
                    StatusPasien = vm.StatusPasien,
                    WaktuStatus = vm.WaktuStatus,

                    KontrolKe = vm.KontrolKe,
                    WaktuKontrol = vm.WaktuKontrol,

                    Transportasi = vm.Transportasi,
                    AlasanMenolakDirawat = vm.AlasanMenolakDirawat,
                    RsRujukan = vm.RsRujukan,
                    AlasanDirujuk = vm.AlasanDirujuk,

                    TingkatKesadaran = vm.TingkatKesadaran,
                    Eyes = vm.Eyes,
                    Motorik = vm.Motorik,
                    Verbal = vm.Verbal,
                    Pupil = vm.Pupil,
                    Reaksi = vm.Reaksi,

                    Suhu = vm.Suhu,
                    TekananDarahDiastolic = vm.TekananDarahDiastolic,
                    TekananDarahSystolic = vm.TekananDarahSystolic,
                    Nadi = vm.Nadi,
                    RR = vm.RR,
                    SPO2 = vm.SPO2,

                    HasilLabId = vm.HasilLabId,
                    HasilCTScanId = vm.HasilCTScanId,
                    HasilEKGId = vm.HasilEKGId,
                    HasilRontgenId = vm.HasilRontgenId,
                    HasilUSGId = vm.HasilUSGId,

                    LembarLab = vm.LembarLab,
                    LembarCTScan = vm.LembarCTScan,
                    LembarEKG = vm.LembarEKG,
                    LembarRontgen = vm.LembarRontgen,
                    LembarUSG = vm.LembarUSG,

                    PerawatIgdId = vm.PerawatIgdId,
                    PerawatKamarId = vm.PerawatKamarId,


                    TindakLanjut = vm.TindakLanjut,
                    KeadaanPasienPulang = vm.KeadaanPasienPulang,
                    KesimpulanAkhir = vm.KesimpulanAkhir,
                    WaktuDipulangkan = vm.WaktuDipulangkan,
                    UPF = vm. UPF,
                    Bangsal = vm.Bangsal,

                    KelasId = vm.KelasId,
                    IndikasiRanap = vm.IndikasiRanap,
                    WaktuDirujuk = vm.WaktuDirujuk,
                    Observasi = vm.Observasi,
                    TempatMeninggal = vm.TempatMeninggal,
                    TanggalMeninggal = vm.TanggalMeninggal,
                    PenyebabMeninggal =vm.PenyebabMeninggal,
                    MobilisasiSaatPulang = vm.MobilisasiSaatPulang,
                    IsVisum = vm.IsVisum,
                    JumlahHariIzin = vm.JumlahHariIzin,
                    TanggalAkhirIzin = vm.TanggalAkhirIzin,
                    TanggalAwalIzin = vm.TanggalAwalIzin,
                    TTDDokterId = vm.TTDDokterId,
                    TTDPerawatId = vm.TTDPerawatId,

                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.IGDTindakLanjuts.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Tindak Lanjut IGD Berhasil || 201 Created",
                        data.TindakLanjutIgdId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] IGDTindakLanjutViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.IGDTindakLanjuts
                    .FirstOrDefaultAsync(a => a.TindakLanjutIgdId == id && (a.IsDelete == false || a.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Tindak Lanjut IGD dengan ID {id} tidak ditemukan." });

                // --- Update fields ---
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.KamarId = vm.KamarId;

                data.WaktuPindah = vm.WaktuPindah;

                data.TindakanLanjutan = vm.TindakanLanjutan;
                data.StatusPasien = vm.StatusPasien;
                data.WaktuStatus = vm.WaktuStatus;

                data.KontrolKe = vm.KontrolKe;
                data.WaktuKontrol = vm.WaktuKontrol;

                data.Transportasi = vm.Transportasi;
                data.AlasanMenolakDirawat = vm.AlasanMenolakDirawat;
                data.RsRujukan = vm.RsRujukan;
                data.AlasanDirujuk = vm.AlasanDirujuk;

                data.TingkatKesadaran = vm.TingkatKesadaran;
                data.Eyes = vm.Eyes;
                data.Motorik = vm.Motorik;
                data.Verbal = vm.Verbal;
                data.Pupil = vm.Pupil;
                data.Reaksi = vm.Reaksi;

                data.Suhu = vm.Suhu;
                data.TekananDarahDiastolic = vm.TekananDarahDiastolic;
                data.TekananDarahSystolic = vm.TekananDarahSystolic;
                data.Nadi = vm.Nadi;
                data.RR = vm.RR;
                data.SPO2 = vm.SPO2;

                data.HasilLabId = vm.HasilLabId;
                data.HasilCTScanId = vm.HasilCTScanId;
                data.HasilEKGId = vm.HasilEKGId;
                data.HasilRontgenId = vm.HasilRontgenId;
                data.HasilUSGId = vm.HasilUSGId;

                data.LembarLab = vm.LembarLab;
                data.LembarCTScan = vm.LembarCTScan;
                data.LembarEKG = vm.LembarEKG;
                data.LembarRontgen = vm.LembarRontgen;
                data.LembarUSG = vm.LembarUSG;

                data.PerawatIgdId = vm.PerawatIgdId;
                data.PerawatKamarId = vm.PerawatKamarId;

                data.TindakLanjut = vm.TindakLanjut;
                data.KeadaanPasienPulang = vm.KeadaanPasienPulang;
                data.KesimpulanAkhir = vm.KesimpulanAkhir;
                data.WaktuDipulangkan = vm.WaktuDipulangkan;
                data.UPF = vm.UPF;
                data.Bangsal = vm.Bangsal;

                data.KelasId = vm.KelasId;
                data.IndikasiRanap = vm.IndikasiRanap;
                data.WaktuDirujuk = vm.WaktuDirujuk;
                data.Observasi = vm.Observasi;
                data.TempatMeninggal = vm.TempatMeninggal;
                data.TanggalMeninggal = vm.TanggalMeninggal;
                data.PenyebabMeninggal = vm.PenyebabMeninggal;
                data.MobilisasiSaatPulang = vm.MobilisasiSaatPulang;
                data.IsVisum = vm.IsVisum;
                data.JumlahHariIzin = vm.JumlahHariIzin;
                data.TanggalAkhirIzin = vm.TanggalAkhirIzin;
                data.TanggalAwalIzin = vm.TanggalAwalIzin;
                data.TTDDokterId = vm.TTDDokterId;
                data.TTDPerawatId = vm.TTDPerawatId;

                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDTindakLanjuts.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Tindak Lanjut IGD Berhasil || 200 OK",
                        id = data.TindakLanjutIgdId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("IGDPulang/{id}")]
        public async Task<IActionResult> UpdateWaktuPulangIGD(Guid id, [FromBody] DateTime? date)
        {
            if (date == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.IGDTindakLanjuts
                    .FirstOrDefaultAsync(a => a.TindakLanjutIgdId == id && (a.IsDelete == false || a.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Tindak Lanjut IGD dengan ID {id} tidak ditemukan." });

                // --- Update fields ---
                data.WaktuDipulangkan = date;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDTindakLanjuts.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Tindak Lanjut IGD Berhasil || 200 OK",
                        id = data.TindakLanjutIgdId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("IGDMeninggal/{id}")]
        public async Task<IActionResult> UpdateWaktuMeninggalIGD(Guid id, [FromBody] DateTime? date)
        {
            if (date == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.IGDTindakLanjuts
                    .FirstOrDefaultAsync(a => a.TindakLanjutIgdId == id && (a.IsDelete == false || a.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Tindak Lanjut IGD dengan ID {id} tidak ditemukan." });

                // --- Update fields ---
                data.TanggalMeninggal = date;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDTindakLanjuts.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Tindak Lanjut IGD Berhasil || 200 OK",
                        id = data.TindakLanjutIgdId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null
        )
        {
            var query = from a in _applicationDbContext.IGDTindakLanjuts
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where a.IsDelete == false || a.IsDelete == null
                        select new
                        {
                            a.TindakLanjutIgdId,
                            a.KunjunganId,
                            a.PasienId,
                            a.KamarId,
                            a.WaktuPindah,
                            a.TindakanLanjutan,
                            a.StatusPasien,
                            a.WaktuStatus,
                            a.KontrolKe,
                            a.WaktuKontrol,
                            a.Transportasi,
                            a.AlasanMenolakDirawat,
                            a.RsRujukan,
                            a.AlasanDirujuk,

                            // Fisik
                            a.TingkatKesadaran,
                            a.Eyes,
                            a.Motorik,
                            a.Verbal,
                            a.Pupil,
                            a.Reaksi,

                            // Vital Sign
                            a.Suhu,
                            a.TekananDarahDiastolic,
                            a.TekananDarahSystolic,
                            a.Nadi,
                            a.RR,
                            a.SPO2,

                            // Pemeriksaan
                            a.HasilLabId,
                            a.HasilCTScanId,
                            a.HasilEKGId,
                            a.HasilRontgenId,
                            a.HasilUSGId,

                            // Lembar
                            a.LembarLab,
                            a.LembarCTScan,
                            a.LembarEKG,
                            a.LembarRontgen,
                            a.LembarUSG,

                            a.PerawatIgdId,
                            a.PerawatKamarId,

                            TindakLanjut = a.TindakLanjut,
                            KeadaanPasienPulang = a.KeadaanPasienPulang,
                            KesimpulanAkhir = a.KesimpulanAkhir,
                            WaktuDipulangkan = a.WaktuDipulangkan,
                            UPF = a.UPF,
                            Bangsal = a.Bangsal,

                            KelasId = a.KelasId,
                            IndikasiRanap = a.IndikasiRanap,
                            WaktuDirujuk = a.WaktuDirujuk,
                            Observasi = a.Observasi,
                            TempatMeninggal = a.TempatMeninggal,
                            TanggalMeninggal = a.TanggalMeninggal,
                            PenyebabMeninggal = a.PenyebabMeninggal,
                            MobilisasiSaatPulang = a.MobilisasiSaatPulang,
                            IsVisum = a.IsVisum,
                            JumlahHariIzin = a.JumlahHariIzin,
                            TanggalAkhirIzin = a.TanggalAkhirIzin,
                            TanggalAwalIzin = a.TanggalAwalIzin,
                            TTDDokterId = a.TTDDokterId,
                            TTDPerawatId = a.TTDPerawatId,
                            a.Keterangan,

                            a.CreateDateTime,
                            CreateByName = u.FullName
                        };

            // -----------------------------
            // 🔎 FILTERING
            // -----------------------------

            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (pasienId.HasValue)
                query = query.Where(x => x.PasienId == pasienId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime <= end);
            }

            // 🔥 Filter periode (Today, ThisWeek, etc)
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= startWeek &&
                            x.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= lastWeekStart &&
                            x.CreateDateTime.Date < lastWeekEnd);
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime.Month == lastMonth.Month &&
                            x.CreateDateTime.Year == lastMonth.Year);
                        break;

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


            // -----------------------------
            // 🔽 SORTING
            // -----------------------------
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "StatusPasien" => query.OrderByDescending(x => x.StatusPasien),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "StatusPasien" => query.OrderBy(x => x.StatusPasien),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };


            // -----------------------------
            // 📄 PAGING
            // -----------------------------
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

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
