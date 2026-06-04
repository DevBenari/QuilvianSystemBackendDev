using System.Security.Claims;
using Humanizer;
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
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class KajianPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KajianPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KajianPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KajianPasienController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================
            // 0) Base untuk filter utama (lebih murah untuk COUNT)
            // =========================
            var kajianBase = _applicationDbContext.KajianPasiens
                .AsNoTracking()
                .Where(a => a.IsDelete != true);

            // COUNT tanpa join (lebih cepat)
            var totalRows = await kajianBase.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // =========================
            // 1) Query page data (baru join untuk kebutuhan tampilan)
            // =========================
            var pageQuery =
                from a in kajianBase

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kunjunganGroup
                from k in kunjunganGroup.DefaultIfEmpty()

                join v0 in _applicationDbContext.VitalSigns.AsNoTracking()
                    on a.VitalSignId equals v0.VitalSignId into vitalSignGroup
                from v in vitalSignGroup.DefaultIfEmpty()

                orderby a.CreateDateTime descending
                select new
                {
                    a.KajianPasienId,
                    a.KunjunganId,
                    a.VitalSignId,

                    v.Suhu,
                    v.Nadi,
                    v.SaturasiOksigen,
                    v.TekananDarahDiastolic,
                    v.TekananDarahSystolic,
                    v.Kesadaran,
                    v.BBKering,
                    v.Height,

                    a.DokterId,
                    a.UserActiveId,
                    a.KeluhanUtama,
                    a.KeadaanUmum,
                    a.KeadaanKulit,
                    a.KeadaanKepalaLeher,
                    a.KeadaanDada,
                    a.KeadaanJantung,
                    a.KeadaanParuParu,
                    a.KeadaanAbdomen,
                    a.KeadaanGenitalia,
                    a.KeadaanAnggotaGerak,
                    a.KeadaanLainnya,
                    a.StatusLokalis,
                    a.PemeriksaanPenunjang,
                    a.DiagnosaSaatIni,
                    a.DiagnosaBanding,
                    a.DaftarMasalah,
                    a.Program,
                    a.Terapi,
                    a.Edukasi,
                    a.EdukasiKepada,
                    a.Keterangan,
                    a.TglKajian,
                    a.TglTindakLanjut,
                    a.IndikasiTindakLanjut,
                    a.KamarId,
                    a.NamaTempat,
                    a.PenyampaianEdukasi,
                    a.CreateBy,
                    a.CreateDateTime,
                    a.KajianUtamaPengkajian,
                    a.CurrentMedicationId,
                    a.BahasaDigunakan,
                    a.JenisHambatan,

                    a.IsDBNKepala,
                    a.IsDBNMata,
                    a.IsDBNMulut,
                    a.IsDBNTHT,
                    a.IsDBNLeher,
                    a.IsDBNThorak,
                    a.IsDBNJantung,
                    a.IsDBNParu,
                    a.IsDBNPunggung,
                    a.IsDBNAbdomen,
                    a.IsDBNGenital,
                    a.IsDBNEkstremitas,

                    a.KeadaanKepala,
                    a.KeadaanLeher,
                    a.KeadaanMata,
                    a.KeadaanMulut,
                    a.KeadaanTHT,
                    a.KeadaanThorak,
                    a.KeadaanPunggung,
                    a.KeadaanEkstremitas,
                    a.IsAsing,
                    a.IsDaerah,

                    CreateByName = u != null ? u.FullName : null,
                    NoRekamMedis = k != null ? k.NoRekamMedis : null,
                };

            var listData = await pageQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (listData.Count == 0)
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // =========================
            // 2) Ambil data PainAssessment & SuratPengantar (batch)
            // =========================
            var kunjunganIds = listData
                .Select(x => x.KunjunganId)
                .Where(id => id != Guid.Empty)      // kalau nullable, sesuaikan jadi: .Where(id => id.HasValue).Select(id => id.Value)
                .Distinct()
                .ToList();

            // Pain Assessment (filter IsDelete jika ada)
            var painAssessments = await _applicationDbContext.PainAssessments
                .AsNoTracking()
                .Where(p => kunjunganIds.Contains(p.KunjunganId) && p.IsDelete != true)
                .Select(p => new
                {
                    p.PainAssessmentId,
                    p.KunjunganId,
                    p.InheritedDisease,
                    p.CreateDateTime
                })
                .OrderByDescending(p => p.CreateDateTime)
                .ToListAsync();

            // Surat Pengantar (filter IsDelete jika ada)
            var suratPengantar = await _applicationDbContext.SuratPengantarRawatInaps
                .AsNoTracking()
                .Where(s => kunjunganIds.Contains(s.KunjunganId) && s.IsDelete != true)
                .Select(s => new
                {
                    s.KunjunganId,
                    s.AsalUnit
                })
                .ToListAsync();

            // =========================
            // 3) Map sekali (lebih hemat daripada ToLookup + ToList per row)
            // =========================
            var painMap = painAssessments
                .GroupBy(p => p.KunjunganId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (object)x).ToList()
                );

            var suratMap = suratPengantar
                .GroupBy(s => s.KunjunganId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.AsalUnit);

            // =========================
            // 4) Gabungkan hasil
            // =========================
            var result = listData.Select(x =>
            {
                painMap.TryGetValue(x.KunjunganId, out var painList);
                suratMap.TryGetValue(x.KunjunganId, out var asalUnit);

                return new
                {
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.KajianPasienId,
                    x.KunjunganId,
                    x.VitalSignId,

                    x.Suhu,
                    x.Nadi,
                    x.SaturasiOksigen,
                    x.TekananDarahDiastolic,
                    x.TekananDarahSystolic,
                    x.Kesadaran,
                    x.BBKering,
                    x.Height,

                    x.NoRekamMedis,
                    x.DokterId,
                    x.UserActiveId,
                    x.KeluhanUtama,
                    x.KeadaanUmum,
                    x.KeadaanKulit,
                    x.KeadaanKepalaLeher,
                    x.KeadaanDada,
                    x.KeadaanJantung,
                    x.KeadaanParuParu,
                    x.KeadaanAbdomen,
                    x.KeadaanGenitalia,
                    x.KeadaanAnggotaGerak,
                    x.KeadaanLainnya,
                    x.StatusLokalis,
                    x.PemeriksaanPenunjang,
                    x.DiagnosaSaatIni,
                    x.DiagnosaBanding,
                    x.DaftarMasalah,
                    x.Program,
                    x.Terapi,
                    x.Edukasi,
                    x.EdukasiKepada,
                    x.Keterangan,
                    x.TglKajian,
                    x.KajianUtamaPengkajian,
                    x.CurrentMedicationId,
                    x.TglTindakLanjut,
                    x.IndikasiTindakLanjut,
                    x.KamarId,
                    x.NamaTempat,
                    x.PenyampaianEdukasi,
                    x.BahasaDigunakan,
                    x.JenisHambatan,

                    x.IsDBNKepala,
                    x.IsDBNMata,
                    x.IsDBNMulut,
                    x.IsDBNTHT,
                    x.IsDBNLeher,
                    x.IsDBNThorak,
                    x.IsDBNJantung,
                    x.IsDBNParu,
                    x.IsDBNPunggung,
                    x.IsDBNAbdomen,
                    x.IsDBNGenital,
                    x.IsDBNEkstremitas,

                    x.KeadaanKepala,
                    x.KeadaanLeher,
                    x.KeadaanMata,
                    x.KeadaanMulut,
                    x.KeadaanTHT,
                    x.KeadaanThorak,
                    x.KeadaanPunggung,
                    x.KeadaanEkstremitas,
                    x.IsAsing,
                    x.IsDaerah,

                    PainAssessments = painList ?? new List<object>(),
                    AsalUnit = asalUnit
                };
            }).ToList();

            // =========================
            // 5) Return
            // =========================
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = result,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }


        //[HttpGet]
        //public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        //{
        //    // Validasi agar page dan perPage minimal bernilai 1
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;


        //    var query = (from a in _applicationDbContext.KajianPasiens
        //                 join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
        //                 from u in userGroup.DefaultIfEmpty()

        //                 join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID into kunjunganGroup
        //                 from k in kunjunganGroup.DefaultIfEmpty()

        //                 join pa in _applicationDbContext.PainAssessments on a.KunjunganId equals pa.KunjunganId into painGroup
        //                 from pa in painGroup.DefaultIfEmpty()

        //                 join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganId equals sp.KunjunganId into suratGroup
        //                 from sp in suratGroup.DefaultIfEmpty()

        //                 where a.IsDelete == false || a.IsDelete == null
        //                 select new
        //                 {
        //                     a.CreateDateTime,
        //                     a.CreateBy,
        //                     CreateByName = u.FullName,
        //                     a.KajianPasienId,
        //                     a.KunjunganId,
        //                     a.VitalSignId,
        //                     k.NoRekamMedis,
        //                     a.DokterId,
        //                     a.UserActiveId,
        //                     a.KeadaanUmum,
        //                     a.KeadaanKulit,
        //                     a.KeadaanKepalaLeher,
        //                     a.KeadaanDada,
        //                     a.KeadaanJantung,
        //                     a.KeadaanParuParu,
        //                     a.KeadaanAbdomen,
        //                     a.KeadaanGenitalia,
        //                     a.KeadaanAnggotaGerak,
        //                     a.KeadaanLainnya,
        //                     a.StatusLokalis,
        //                     a.PemeriksaanPenunjang,
        //                     a.DiagnosaSaatIni,
        //                     a.DiagnosaBanding,
        //                     a.DaftarMasalah,
        //                     a.Program,
        //                     a.Terapi,
        //                     a.Edukasi,
        //                     a.EdukasiKepada,
        //                     a.Keterangan,
        //                     a.TglKajian,
        //                     a.KajianUtamaPengkajian,
        //                     a.CurrentMedicationId,

        //                     // info pain assessment
        //                     pa.InheritedDisease,

        //                     // info surat pengantar
        //                     sp.AsalUnit,

        //                 }).OrderByDescending(a => a.CreateDateTime);

        //    // Hitung total data sebelum paginasi
        //    var totalRows = query.Count();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    // Ambil data sesuai paging
        //    var listdata = query
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToList();

        //    if (!listdata.Any())
        //    {
        //        return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
        //    }

        //    // Return hasil dengan paging info
        //    return Ok(new
        //    {
        //        message = "Berhasil || 200 OK",
        //        data = listdata,
        //        pagination = new
        //        {
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalRows = totalRows,
        //            TotalPages = totalPages
        //        }
        //    });
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.KajianPasiens.Find(id);
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
        public async Task<IActionResult> Create([FromBody] KajianPasienViewModel vm)
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

                //// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new KajianPasien
                {
                    KajianPasienId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    VitalSignId = vm.VitalSignId,
                    DokterId = vm.DokterId,
                    UserActiveId = userActiveId,
                    KeluhanUtama = vm.KeluhanUtama,
                    KeadaanUmum = vm.KeadaanUmum,
                    KeadaanKulit = vm.KeadaanKulit,
                    KeadaanKepalaLeher = vm.KeadaanKepalaLeher,
                    KeadaanDada = vm.KeadaanDada,
                    KeadaanJantung = vm.KeadaanJantung,
                    KeadaanParuParu = vm.KeadaanParuParu,
                    KeadaanAbdomen = vm.KeadaanAbdomen,
                    KeadaanGenitalia = vm.KeadaanGenitalia,
                    KeadaanAnggotaGerak = vm.KeadaanAnggotaGerak,
                    KeadaanLainnya = vm.KeadaanLainnya,
                    StatusLokalis = vm.StatusLokalis,
                    PemeriksaanPenunjang = vm.PemeriksaanPenunjang,
                    DiagnosaSaatIni = vm.DiagnosaSaatIni,
                    DiagnosaBanding = vm.DiagnosaBanding,
                    DaftarMasalah = vm.DaftarMasalah,
                    Program = vm.Program,
                    Terapi = vm.Terapi,
                    Edukasi = true,
                    EdukasiKepada = vm.EdukasiKepada,
                    Keterangan = vm.Keterangan,
                    TglKajian = DateTime.UtcNow, // Atau gunakan TglKajian dari VM jika ada
                    TglTindakLanjut = vm.TglTindakLanjut,
                    IndikasiTindakLanjut = vm.IndikasiTindakLanjut,
                    KamarId = vm.KamarId,
                    NamaTempat = vm.NamaTempat,
                    PenyampaianEdukasi = vm.PenyampaianEdukasi,

                    BahasaDigunakan = vm.BahasaDigunakan,
                    JenisHambatan = vm.JenisHambatan,
                    IsDaerah = vm.IsDaerah,
                    IsAsing = vm.IsAsing,

                    IsDBNKepala = vm.IsDBNKepala,
                    IsDBNMata = vm.IsDBNMata,
                    IsDBNMulut = vm.IsDBNMulut,
                    IsDBNTHT = vm.IsDBNTHT,
                    IsDBNLeher = vm.IsDBNLeher,
                    IsDBNThorak = vm.IsDBNThorak,
                    IsDBNJantung = vm.IsDBNJantung,
                    IsDBNParu = vm.IsDBNParu,
                    IsDBNPunggung = vm.IsDBNPunggung,
                    IsDBNAbdomen = vm.IsDBNAbdomen,
                    IsDBNGenital = vm.IsDBNGenital,
                    IsDBNEkstremitas = vm.IsDBNEkstremitas,

                    KeadaanKepala = vm.KeadaanKepala,
                    KeadaanLeher = vm.KeadaanLeher,
                    KeadaanMata = vm.KeadaanMata,
                    KeadaanMulut = vm.KeadaanMulut,
                    KeadaanTHT = vm.KeadaanTHT,
                    KeadaanThorak = vm.KeadaanThorak,
                    KeadaanPunggung = vm.KeadaanPunggung,
                    KeadaanEkstremitas = vm.KeadaanEkstremitas,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // **Simpan ke Database**
                _applicationDbContext.KajianPasiens.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KajianPasienViewModel vm)
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
                var data = await _applicationDbContext.KajianPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.VitalSignId = vm.VitalSignId;
                data.DokterId = vm.DokterId;
                data.UserActiveId = userActiveId;
                data.KeluhanUtama = vm.KeluhanUtama;
                data.KeadaanUmum = vm.KeadaanUmum;
                data.KeadaanKulit = vm.KeadaanKulit;
                data.KeadaanKepalaLeher = vm.KeadaanKepalaLeher;
                data.KeadaanDada = vm.KeadaanDada;
                data.KeadaanJantung = vm.KeadaanJantung;
                data.KeadaanParuParu = vm.KeadaanParuParu;
                data.KeadaanAbdomen = vm.KeadaanAbdomen;
                data.KeadaanGenitalia = vm.KeadaanGenitalia;
                data.KeadaanAnggotaGerak = vm.KeadaanAnggotaGerak;
                data.KeadaanLainnya = vm.KeadaanLainnya;
                data.StatusLokalis = vm.StatusLokalis;
                data.PemeriksaanPenunjang = vm.PemeriksaanPenunjang;
                data.DiagnosaSaatIni = vm.DiagnosaSaatIni;
                data.DiagnosaBanding = vm.DiagnosaBanding;
                data.DaftarMasalah = vm.DaftarMasalah;
                data.Program = vm.Program;
                data.Terapi = vm.Terapi;
                data.Edukasi = true; // Asumsikan edukasi selalu true saat update
                data.EdukasiKepada = vm.EdukasiKepada;
                data.Keterangan = vm.Keterangan;
                data.TglKajian = DateTime.UtcNow; // Atau gunakan TglKajian dari VM jika ada
                data.TglTindakLanjut = vm.TglTindakLanjut;
                data.IndikasiTindakLanjut = vm.IndikasiTindakLanjut;
                data.KamarId = vm.KamarId;
                data.NamaTempat = vm.NamaTempat;
                data.PenyampaianEdukasi = vm.PenyampaianEdukasi;
                data.BahasaDigunakan = vm.BahasaDigunakan;
                data.JenisHambatan = vm.JenisHambatan;
                data.IsAsing = vm.IsAsing;
                data.IsDaerah = vm.IsDaerah;
                data.IsDBNKepala = vm.IsDBNKepala;
                data.IsDBNMata = vm.IsDBNMata;
                data.IsDBNMulut = vm.IsDBNMulut;
                data.IsDBNTHT = vm.IsDBNTHT;
                data.IsDBNLeher = vm.IsDBNLeher;
                data.IsDBNThorak = vm.IsDBNThorak;
                data.IsDBNJantung = vm.IsDBNJantung;
                data.IsDBNParu = vm.IsDBNParu;
                data.IsDBNPunggung = vm.IsDBNPunggung;
                data.IsDBNAbdomen = vm.IsDBNAbdomen;
                data.IsDBNGenital = vm.IsDBNGenital;
                data.IsDBNEkstremitas = vm.IsDBNEkstremitas;
                data.KeadaanKepala = vm.KeadaanKepala;
                data.KeadaanLeher = vm.KeadaanLeher;
                data.KeadaanMata = vm.KeadaanMata;
                data.KeadaanMulut = vm.KeadaanMulut;
                data.KeadaanTHT = vm.KeadaanTHT;
                data.KeadaanThorak = vm.KeadaanThorak;
                data.KeadaanPunggung = vm.KeadaanPunggung;
                data.KeadaanEkstremitas = vm.KeadaanEkstremitas;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.KajianPasiens.Update(data);
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
                var data = await _applicationDbContext.KajianPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.KajianPasiens.Update(data);
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
            string? noRM = null,
            Guid? kunjunganId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        )
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =====================================================
            // 1) BASE QUERY (tanpa join ke tabel 1..N biar tidak dobel)
            // =====================================================
            var query =
                from a in _applicationDbContext.KajianPasiens.AsNoTracking()
                where a.IsDelete != true
                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kunjunganGroup
                from k in kunjunganGroup.DefaultIfEmpty()

                join v0 in _applicationDbContext.VitalSigns.AsNoTracking()
                    on a.VitalSignId equals v0.VitalSignId into vitalSignGroup
                from v in vitalSignGroup.DefaultIfEmpty()

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    a.KajianPasienId,
                    a.KunjunganId,
                    a.VitalSignId,

                    v.Suhu,
                    v.Nadi,
                    v.SaturasiOksigen,
                    v.TekananDarahDiastolic,
                    v.TekananDarahSystolic,
                    v.Kesadaran,
                    v.BBKering,
                    v.Height,

                    NoRekamMedis = k != null ? k.NoRekamMedis : null,

                    a.DokterId,
                    a.UserActiveId,
                    a.KeluhanUtama,
                    a.KeadaanUmum,
                    a.KeadaanKulit,
                    a.KeadaanKepalaLeher,
                    a.KeadaanDada,
                    a.KeadaanJantung,
                    a.KeadaanParuParu,
                    a.KeadaanAbdomen,
                    a.KeadaanGenitalia,
                    a.KeadaanAnggotaGerak,
                    a.KeadaanLainnya,
                    a.StatusLokalis,
                    a.PemeriksaanPenunjang,
                    a.DiagnosaSaatIni,
                    a.DiagnosaBanding,
                    a.DaftarMasalah,
                    a.Program,
                    a.Terapi,
                    a.Edukasi,
                    a.EdukasiKepada,
                    a.Keterangan,
                    a.TglKajian,
                    a.KajianUtamaPengkajian,
                    a.CurrentMedicationId,
                    a.TglTindakLanjut,
                    a.IndikasiTindakLanjut,
                    a.KamarId,
                    a.NamaTempat,
                    a.PenyampaianEdukasi,
                    a.BahasaDigunakan,
                    a.JenisHambatan,

                    a.IsDBNKepala,
                    a.IsDBNMata,
                    a.IsDBNMulut,
                    a.IsDBNTHT,
                    a.IsDBNLeher,
                    a.IsDBNThorak,
                    a.IsDBNJantung,
                    a.IsDBNParu,
                    a.IsDBNPunggung,
                    a.IsDBNAbdomen,
                    a.IsDBNGenital,
                    a.IsDBNEkstremitas,

                    a.KeadaanKepala,
                    a.KeadaanLeher,
                    a.KeadaanMata,
                    a.KeadaanMulut,
                    a.KeadaanTHT,
                    a.KeadaanThorak,
                    a.KeadaanPunggung,
                    a.KeadaanEkstremitas,
                    a.IsAsing,
                    a.IsDaerah
                };

            // =====================================================
            // 2) FILTER
            // =====================================================
            if (!string.IsNullOrWhiteSpace(noRM))
            {
                var pattern = $"%{noRM.ToLower()}%";
                query = query.Where(x => EF.Functions.ILike(x.NoRekamMedis ?? "", pattern));
            }

            if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                query = periode.Value switch
                {
                    PeriodeFilter.Today =>
                        query.Where(x => x.CreateDateTime.Date == today),

                    PeriodeFilter.ThisWeek =>
                        query.Where(x => x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek)
                                      && x.CreateDateTime.Date <= today),

                    PeriodeFilter.LastWeek =>
                        query.Where(x => x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek)
                                      && x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)),

                    PeriodeFilter.ThisMonth =>
                        query.Where(x => x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastMonth =>
                        query.Where(x => x.CreateDateTime >= new DateTime(today.Year, today.Month, 1).AddMonths(-1)
                                      && x.CreateDateTime < new DateTime(today.Year, today.Month, 1)),

                    PeriodeFilter.ThisYear =>
                        query.Where(x => x.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastYear =>
                        query.Where(x => x.CreateDateTime.Year == today.Year - 1),

                    PeriodeFilter.Last3Months =>
                        query.Where(x => x.CreateDateTime >= today.AddMonths(-3)),

                    PeriodeFilter.Last6Months =>
                        query.Where(x => x.CreateDateTime >= today.AddMonths(-6)),

                    _ => query
                };
            }

            // =====================================================
            // 3) SORTING (aman)
            // =====================================================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "CreateDateTime" or _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
            };

            // =====================================================
            // 4) PAGING (DB-side)
            // =====================================================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalPages > 0 && page > totalPages)
                return NotFound(new { message = "Page not found." });

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            // =====================================================
            // 5) Ambil PainAssessment & SuratPengantar batch untuk page ini
            // =====================================================
            var kunjunganIdsOnPage = rows
                .Select(r => r.KunjunganId)
                .Distinct()
                .ToList();

            // 5a) Ambil 1 PainAssessment terakhir per Kunjungan (kalau kamu cuma butuh InheritedDisease 1 saja)
            var painLast = await _applicationDbContext.PainAssessments
                .AsNoTracking()
                .Where(p => p.IsDelete != true && kunjunganIdsOnPage.Contains(p.KunjunganId))
                .GroupBy(p => p.KunjunganId)
                .Select(g => new
                {
                    KunjunganId = g.Key,
                    InheritedDisease = g.OrderByDescending(x => x.CreateDateTime)
                                        .Select(x => x.InheritedDisease)
                                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.KunjunganId, x => x.InheritedDisease);

            // 5b) Surat pengantar (ambil 1 AsalUnit)
            var suratMap = await _applicationDbContext.SuratPengantarRawatInaps
                .AsNoTracking()
                .Where(s => s.IsDelete != true && kunjunganIdsOnPage.Contains(s.KunjunganId))
                .GroupBy(s => s.KunjunganId)
                .Select(g => new
                {
                    KunjunganId = g.Key,
                    AsalUnit = g.Select(x => x.AsalUnit).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.KunjunganId, x => x.AsalUnit);

            // =====================================================
            // 6) Gabungkan (tanpa row explosion)
            // =====================================================
            var result = rows.Select(r =>
            {
                painLast.TryGetValue(r.KunjunganId, out var inheritedDisease);
                suratMap.TryGetValue(r.KunjunganId, out var asalUnit);

                return new
                {
                    r.CreateDateTime,
                    r.CreateBy,
                    r.CreateByName,
                    r.KajianPasienId,
                    r.KunjunganId,
                    r.VitalSignId,

                    r.Suhu,
                    r.Nadi,
                    r.SaturasiOksigen,
                    r.TekananDarahDiastolic,
                    r.TekananDarahSystolic,
                    r.Kesadaran,
                    r.BBKering,
                    r.Height,

                    r.NoRekamMedis,

                    r.DokterId,
                    r.UserActiveId,
                    r.KeluhanUtama,
                    r.KeadaanUmum,
                    r.KeadaanKulit,
                    r.KeadaanKepalaLeher,
                    r.KeadaanDada,
                    r.KeadaanJantung,
                    r.KeadaanParuParu,
                    r.KeadaanAbdomen,
                    r.KeadaanGenitalia,
                    r.KeadaanAnggotaGerak,
                    r.KeadaanLainnya,
                    r.StatusLokalis,
                    r.PemeriksaanPenunjang,
                    r.DiagnosaSaatIni,
                    r.DiagnosaBanding,
                    r.DaftarMasalah,
                    r.Program,
                    r.Terapi,
                    r.Edukasi,
                    r.EdukasiKepada,
                    r.Keterangan,
                    r.TglKajian,
                    r.KajianUtamaPengkajian,
                    r.CurrentMedicationId,
                    r.TglTindakLanjut,
                    r.IndikasiTindakLanjut,
                    r.KamarId,
                    r.NamaTempat,
                    r.PenyampaianEdukasi,
                    r.BahasaDigunakan,
                    r.JenisHambatan,

                    r.IsDBNKepala,
                    r.IsDBNMata,
                    r.IsDBNMulut,
                    r.IsDBNTHT,
                    r.IsDBNLeher,
                    r.IsDBNThorak,
                    r.IsDBNJantung,
                    r.IsDBNParu,
                    r.IsDBNPunggung,
                    r.IsDBNAbdomen,
                    r.IsDBNGenital,
                    r.IsDBNEkstremitas,

                    r.KeadaanKepala,
                    r.KeadaanLeher,
                    r.KeadaanMata,
                    r.KeadaanMulut,
                    r.KeadaanTHT,
                    r.KeadaanThorak,
                    r.KeadaanPunggung,
                    r.KeadaanEkstremitas,
                    r.IsAsing,
                    r.IsDaerah,

                    // info tambahan
                    InheritedDisease = inheritedDisease,
                    AsalUnit = asalUnit
                };
            }).ToList();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
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

    }
}
