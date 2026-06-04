using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PainAssessmentController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PainAssessmentController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<PainAssesmentHub> _hubContext;

        public PainAssessmentController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PainAssessmentController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<PainAssesmentHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPainAssessment(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.PainAssessments
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PainAssessmentId = a.PainAssessmentId,
                            KunjunganId = a.KunjunganId,
                            KeluhanUtama = a.KeluhanUtama,
                            IsPain = a.IsPain,
                            Pemicu = a.Pemicu,
                            Kualitas = a.Kualitas,
                            Lokasi = a.Lokasi,
                            SkalaPainId = a.SkalaPainId,
                            Frekuensi = a.Frekuensi,
                            PainManagement = a.PainManagement,
                            IsInheritedDisease = a.IsInheritedDisease,
                            InheritedDisease = a.InheritedDisease,
                            IsAlergic = a.IsAlergic,
                            Alergic = a.Alergic,
                            NafsuMakan = a.NafsuMakan,
                            IsMual = a.IsMual,
                            IsMuntah = a.IsMuntah,
                            IsFallRisk = a.IsFallRisk,
                            FallRisk = a.FallRisk,
                            IsBCGimunisasi = a.IsBCGimunisasi,
                            IsHepatitisBImunisasi = a.IsHepatitisBImunisasi,
                            IsPolioImunisasi = a.IsPolioImunisasi,
                            IsDPTImunisasi = a.IsDPTImunisasi,
                            IsCampakImunisasi = a.IsCampakImunisasi,
                            IsAsiEksklusif = a.IsAsiEksklusif,
                            StatusMpasi = a.StatusMpasi,
                            IsAtaksia = a.IsAtaksia,
                            IsPosturalInstability = a.IsPosturalInstability,
                            HasilResikoJatuh = a.HasilResikoJatuh,
                            IsMotorikAktif = a.IsMotorikAktif,
                            IsResponsAuditori = a.IsResponsAuditori,
                            IsInteraksiSosial = a.IsInteraksiSosial,
                            RanapId = a.RanapId,
                            RPD = a.RPD,
                            RPS = a.RPS,
                            CurrentMedication = a.CurrentMedication,
                            a.RiwayatPenyakit,
                            a.IsIGD,
                            a.MasukIGD,
                            a.KondisiMasukIGD,
                            a.IsPengobatanSaatIni,
                            a.IsTubuhTidakSeimbang,
                            a.IsMenggunakanPenopang,
                            a.KeluhanTambahan,
                            a.IsFarmakologi,
                            a.KeadaanUmum,
                            a.IsKonjungtiva,
                            a.Ekstremitas,
                            a.SkorSedasi,

                        }).OrderByDescending(a => a.CreateDateTime); ;

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
            var listdata = _applicationDbContext.PainAssessments.Find(id);
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

        [HttpGet("PainAssessment-ByPasien/{pasienId}")]
        public async Task<IActionResult> GetPainAssessmentByPasienId(Guid pasienId)
        {
            var data = (from a in _applicationDbContext.PainAssessments
                        join k in _applicationDbContext.Kunjungans
                            on a.KunjunganId equals k.KunjunganID
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false && k.PasienId == pasienId
                        orderby a.CreateDateTime descending
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PainAssessmentId = a.PainAssessmentId,
                            KunjunganId = a.KunjunganId,
                            KeluhanUtama = a.KeluhanUtama,
                            IsPain = a.IsPain,
                            Pemicu = a.Pemicu,
                            Kualitas = a.Kualitas,
                            Lokasi = a.Lokasi,
                            SkalaPainId = a.SkalaPainId,
                            Frekuensi = a.Frekuensi,
                            PainManagement = a.PainManagement,
                            IsInheritedDisease = a.IsInheritedDisease,
                            InheritedDisease = a.InheritedDisease,
                            IsAlergic = a.IsAlergic,
                            Alergic = a.Alergic,
                            NafsuMakan = a.NafsuMakan,
                            IsMual = a.IsMual,
                            IsMuntah = a.IsMuntah,
                            IsFallRisk = a.IsFallRisk,
                            FallRisk = a.FallRisk,
                            IsBCGimunisasi = a.IsBCGimunisasi,
                            IsHepatitisBImunisasi = a.IsHepatitisBImunisasi,
                            IsPolioImunisasi = a.IsPolioImunisasi,
                            IsDPTImunisasi = a.IsDPTImunisasi,
                            IsCampakImunisasi = a.IsCampakImunisasi,
                            IsAsiEksklusif = a.IsAsiEksklusif,
                            StatusMpasi = a.StatusMpasi,
                            IsAtaksia = a.IsAtaksia,
                            IsPosturalInstability = a.IsPosturalInstability,
                            HasilResikoJatuh = a.HasilResikoJatuh,
                            IsMotorikAktif = a.IsMotorikAktif,
                            IsResponsAuditori = a.IsResponsAuditori,
                            IsInteraksiSosial = a.IsInteraksiSosial,
                            RanapId = a.RanapId,
                            RPD = a.RPD,
                            RPS = a.RPS,
                            CurrentMedication = a.CurrentMedication,
                            a.RiwayatPenyakit,
                            a.IsIGD,
                            a.MasukIGD,
                            a.KondisiMasukIGD,
                            a.IsPengobatanSaatIni,
                            a.IsTubuhTidakSeimbang,
                            a.IsMenggunakanPenopang,
                            a.KeluhanTambahan,
                            a.IsFarmakologi,
                            a.KeadaanUmum,
                            a.IsKonjungtiva,
                            a.Ekstremitas,
                            a.SkorSedasi,
                        }).ToList();

            if (!data.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan untuk pasien ini. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePainAssessment([FromBody] PainAssessmentViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                Guid createBy;

                // 🔁 Cek apakah ini dari delegasi
                if (vm.DelegasiId.HasValue)
                {
                    var delegasi = await _applicationDbContext.Delegasis
                        .FirstOrDefaultAsync(d =>
                            d.DelegasiId == vm.DelegasiId.Value &&
                            d.IsDelegated == true);

                    if (delegasi == null)
                    {
                        return BadRequest(new { message = "Delegasi tidak ditemukan atau sudah tidak aktif." });
                    }

                    //  Gunakan user delegasi sebagai pencatat
                    createBy = delegasi.UserDelegasiId.Value;

                    // Tandai delegasi selesai
                    //delegasi.IsDelegated = false;
                }
                else
                {
                    //  Bukan delegasi, gunakan user login
                    createBy = getUserActive.UserActiveId;
                }

                var data = new PainAssessment
                {
                    PainAssessmentId = Guid.NewGuid(),
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = createBy,
                    KunjunganId = vm.KunjunganId,
                    KeluhanUtama = vm.KeluhanUtama,
                    IsPain = vm.IsPain,
                    Pemicu = vm.Pemicu,
                    Kualitas = vm.Kualitas,
                    Lokasi = vm.Lokasi,
                    SkalaPainId = vm.SkalaPainId,
                    Frekuensi = vm.Frekuensi,
                    PainManagement = vm.PainManagement,
                    IsInheritedDisease = vm.IsInheritedDisease,
                    InheritedDisease = vm.InheritedDisease,
                    IsAlergic = vm.IsAlergic,
                    Alergic = vm.Alergic,
                    NafsuMakan = vm.NafsuMakan,
                    IsMual = vm.IsMual,
                    IsMuntah = vm.IsMuntah,
                    IsFallRisk = vm.IsFallRisk,
                    FallRisk = vm.FallRisk,
                    IsBCGimunisasi = vm.IsBCGimunisasi,
                    IsHepatitisBImunisasi = vm.IsHepatitisBImunisasi,
                    IsPolioImunisasi = vm.IsPolioImunisasi,
                    IsDPTImunisasi = vm.IsDPTImunisasi,
                    IsCampakImunisasi = vm.IsCampakImunisasi,
                    IsAsiEksklusif = vm.IsAsiEksklusif,
                    StatusMpasi = vm.StatusMpasi,
                    IsAtaksia = vm.IsAtaksia,
                    IsPosturalInstability = vm.IsPosturalInstability,
                    HasilResikoJatuh = vm.HasilResikoJatuh,
                    IsMotorikAktif = vm.IsMotorikAktif,
                    IsResponsAuditori = vm.IsResponsAuditori,
                    IsInteraksiSosial = vm.IsInteraksiSosial,
                    RanapId = vm.RanapId,
                    RPD = vm.RPD,
                    RPS = vm.RPS,
                    CurrentMedication = vm.CurrentMedication,
                    RiwayatPenyakit = vm.RiwayatPenyakit,
                    IsIGD = vm.IsIGD,
                    MasukIGD = vm.MasukIGD,
                    KondisiMasukIGD = vm.KondisiMasukIGD,
                    IsPengobatanSaatIni = vm.IsPengobatanSaatIni,
                    IsTubuhTidakSeimbang = vm.IsTubuhTidakSeimbang,
                    IsMenggunakanPenopang = vm.IsMenggunakanPenopang,
                    KeluhanTambahan = vm.KeluhanTambahan,
                    IsFarmakologi = vm.IsFarmakologi,
                    KeadaanUmum = vm.KeadaanUmum,
                    IsKonjungtiva = vm.IsKonjungtiva,
                    Ekstremitas = vm.Ekstremitas,
                    SkorSedasi = vm.SkorSedasi,
                };

                _applicationDbContext.PainAssessments.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                // notifikasi signalR
                await _hubContext.Clients.All.SendAsync("PainAssesment ditambah", new
                {
                    action = "create",
                    PainAssessmentId = data.PainAssessmentId,
                });

                if (result > 0)
                {
                    return Created("", new 
                    { 
                        message = "Tambah Data Berhasil || 201 Created" ,
                        PainAssessmentId = data.PainAssessmentId,
                    });
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
        public async Task<IActionResult> EditPainAssessment(Guid id, [FromBody] PainAssessmentViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var user = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);

                if (user == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = user.UserActiveId;

                // 🔍 Ambil data yang akan diupdate
                var data = await _applicationDbContext.PainAssessments
                    .FirstOrDefaultAsync(p => p.PainAssessmentId == id);

                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                Guid modifyBy;

                // 🔁 Cek apakah delegasi digunakan
                if (vm.DelegasiId.HasValue)
                {
                    var delegasi = await _applicationDbContext.Delegasis
                        .FirstOrDefaultAsync(d =>
                            d.DelegasiId == vm.DelegasiId.Value &&
                            d.IsDelegated == true);

                    if (delegasi == null)
                        return BadRequest(new { message = "Delegasi tidak valid atau sudah tidak aktif." });

                    if (delegasi.UserDelegasiId != userActiveId)
                        return Unauthorized(new { message = "Delegasi ini bukan milik Anda." });

                    modifyBy = delegasi.UserDelegasiId.Value;
                    delegasi.IsDelegated = false;
                }
                else
                {
                    modifyBy = userActiveId;
                }

                // 📝 Update data
                data.KeluhanUtama = vm.KeluhanUtama;
                data.IsPain = vm.IsPain;
                data.Pemicu = vm.Pemicu;
                data.Kualitas = vm.Kualitas;
                data.Lokasi = vm.Lokasi;
                data.SkalaPainId = vm.SkalaPainId;
                data.Frekuensi = vm.Frekuensi;
                data.PainManagement = vm.PainManagement;
                data.IsInheritedDisease = vm.IsInheritedDisease;
                data.InheritedDisease = vm.InheritedDisease;
                data.IsAlergic = vm.IsAlergic;
                data.Alergic = vm.Alergic;
                data.NafsuMakan = vm.NafsuMakan;
                data.IsMual = vm.IsMual;
                data.IsMuntah = vm.IsMuntah;
                data.IsFallRisk = vm.IsFallRisk;
                data.FallRisk = vm.FallRisk;
                data.IsBCGimunisasi = vm.IsBCGimunisasi;
                data.IsHepatitisBImunisasi = vm.IsHepatitisBImunisasi;
                data.IsPolioImunisasi = vm.IsPolioImunisasi;
                data.IsDPTImunisasi = vm.IsDPTImunisasi;
                data.IsCampakImunisasi = vm.IsCampakImunisasi;
                data.IsAsiEksklusif = vm.IsAsiEksklusif;
                data.StatusMpasi = vm.StatusMpasi;
                data.IsAtaksia = vm.IsAtaksia;
                data.IsPosturalInstability = vm.IsPosturalInstability;
                data.HasilResikoJatuh = vm.HasilResikoJatuh;
                data.IsMotorikAktif = vm.IsMotorikAktif;
                data.IsResponsAuditori = vm.IsResponsAuditori;
                data.IsInteraksiSosial = vm.IsInteraksiSosial;
                data.RPD = vm.RPD;
                data.RPS = vm.RPS;
                data.CurrentMedication = vm.CurrentMedication;
                data.RanapId = vm.RanapId;
                data.KunjunganId = vm.KunjunganId;
                data.CurrentMedication = vm.CurrentMedication;
                data.RiwayatPenyakit = vm.RiwayatPenyakit;
                data.IsIGD = vm.IsIGD;
                data.MasukIGD = vm.MasukIGD;
                data.KondisiMasukIGD = vm.KondisiMasukIGD;
                data.IsPengobatanSaatIni = vm.IsPengobatanSaatIni;
                data.IsTubuhTidakSeimbang = vm.IsTubuhTidakSeimbang;
                data.IsMenggunakanPenopang = vm.IsMenggunakanPenopang;
                data.KeluhanTambahan = vm.KeluhanTambahan;
                data.IsFarmakologi = vm.IsFarmakologi;
                data.KeadaanUmum = vm.KeadaanUmum;
                data.IsKonjungtiva = vm.IsKonjungtiva;
                data.Ekstremitas = vm.Ekstremitas;
                data.SkorSedasi = vm.SkorSedasi;

                data.UpdateBy = modifyBy;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                // notifikasi signalR
                await _hubContext.Clients.All.SendAsync("PainAssesment diupdate", new
                {
                    action = "update",
                    PainAssessmentId = data.PainAssessmentId,
                });

                if (result > 0)
                {
                    return Ok(new { message = "Update berhasil." });
                }
                else
                {
                    return StatusCode(500, new { message = "Tidak ada data yang berubah." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal mengupdate data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> CreatePainAssessment([FromBody] PainAssessmentViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        // **Ambil Tanggal Sekarang**
        //        var dateNow = DateTime.UtcNow;
        //        var setDateNow = dateNow.ToString("yyMMdd"); // Format: YYMMDD

        //        // **Buat Data Baru**
        //        var data = new PainAssessment
        //        {
        //            PainAssessmentId = Guid.NewGuid(),
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //            CreateBy = (Guid)vm.UserActiveId,
        //            KunjunganId = vm.KunjunganId,
        //            KeluhanUtama = vm.KeluhanUtama,
        //            IsPain = vm.IsPain,
        //            Pemicu = vm.Pemicu,
        //            Kualitas = vm.Kualitas,
        //            Lokasi = vm.Lokasi,
        //            SkalaPainId = vm.SkalaPainId,
        //            Frekuensi = vm.Frekuensi,
        //            PainManagement = vm.PainManagement,
        //            IsInheritedDisease = vm.IsInheritedDisease,
        //            InheritedDisease = vm.InheritedDisease,
        //            IsAlergic = vm.IsAlergic,
        //            Alergic = vm.Alergic,
        //            NafsuMakan = vm.NafsuMakan,
        //            IsMual = vm.IsMual,
        //            IsMuntah = vm.IsMuntah,
        //            IsFallRisk = vm.IsFallRisk,
        //            FallRisk = vm.FallRisk,
        //            IsBCGimunisasi = vm.IsBCGimunisasi,
        //            IsHepatitisBImunisasi = vm.IsHepatitisBImunisasi,
        //            IsPolioImunisasi = vm.IsPolioImunisasi,
        //            IsDPTImunisasi = vm.IsDPTImunisasi,
        //            IsCampakImunisasi = vm.IsCampakImunisasi,
        //            IsAsiEksklusif = vm.IsAsiEksklusif,
        //            StatusMpasi = vm.StatusMpasi,
        //            IsAtaksia = vm.IsAtaksia,
        //            IsPosturalInstability = vm.IsPosturalInstability,
        //            HasilResikoJatuh = vm.HasilResikoJatuh,
        //            IsMotorikAktif = vm.IsMotorikAktif,
        //            IsResponsAuditori = vm.IsResponsAuditori,
        //            IsInteraksiSosial = vm.IsInteraksiSosial,
        //            RanapId = vm.RanapId,
        //            RPD = vm.RPD,
        //            RPS = vm.RPS,
        //            CurrentMedication = vm.CurrentMedication,
        //        };

        //        // **Simpan ke Database**
        //        _applicationDbContext.PainAssessments.Add(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created"  });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdatePainAssessment(Guid id, [FromBody] PainAssessmentViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!await _applicationDbContext.Database.CanConnectAsync())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        // **Cari Data**
        //        var data = await _applicationDbContext.PainAssessments.FindAsync(id);
        //        if (data == null)
        //        {
        //            return NotFound(new { message = "Data tidak ditemukan." });
        //        }


        //        // **Update Data**
        //        data.KunjunganId = vm.KunjunganId;
        //        data.KeluhanUtama = vm.KeluhanUtama;
        //        data.IsPain = vm.IsPain;
        //        data.Pemicu = vm.Pemicu;
        //        data.Kualitas = vm.Kualitas;
        //        data.Lokasi = vm.Lokasi;
        //        data.SkalaPainId = vm.SkalaPainId;
        //        data.Frekuensi = vm.Frekuensi;
        //        data.PainManagement = vm.PainManagement;
        //        data.IsInheritedDisease = vm.IsInheritedDisease;
        //        data.InheritedDisease = vm.InheritedDisease;
        //        data.IsAlergic = vm.IsAlergic;
        //        data.Alergic = vm.Alergic;
        //        data.NafsuMakan = vm.NafsuMakan;
        //        data.IsMual = vm.IsMual;
        //        data.IsMuntah = vm.IsMuntah;
        //        data.IsFallRisk = vm.IsFallRisk;
        //        data.FallRisk = vm.FallRisk;
        //        data.IsBCGimunisasi = vm.IsBCGimunisasi;
        //        data.IsHepatitisBImunisasi = vm.IsHepatitisBImunisasi;
        //        data.IsPolioImunisasi = vm.IsPolioImunisasi;
        //        data.IsDPTImunisasi = vm.IsDPTImunisasi;
        //        data.IsCampakImunisasi = vm.IsCampakImunisasi;
        //        data.IsAsiEksklusif = vm.IsAsiEksklusif;
        //        data.StatusMpasi = vm.StatusMpasi;
        //        data.IsAtaksia = vm.IsAtaksia;
        //        data.IsPosturalInstability = vm.IsPosturalInstability;
        //        data.HasilResikoJatuh = vm.HasilResikoJatuh;
        //        data.IsMotorikAktif = vm.IsMotorikAktif;
        //        data.IsResponsAuditori = vm.IsResponsAuditori;
        //        data.IsInteraksiSosial = vm.IsInteraksiSosial;
        //        data.RanapId = vm.RanapId;
        //        data.RPD = vm.RPD;
        //        data.RPS = vm.RPS;
        //        data.CurrentMedication = vm.CurrentMedication;

        //        data.UpdateBy = userActiveId;
        //        data.UpdateDateTime = DateTimeOffset.UtcNow;

        //        _applicationDbContext.PainAssessments.Update(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Ok(new { message = "Update Data Berhasil || 200 OK" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgama(Guid id)
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
                var data = await _applicationDbContext.PainAssessments.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PainAssessments.Update(data);
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
        public async Task<IActionResult> PagedPainAssessment(
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
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // Query data
                var query = from a in _applicationDbContext.PainAssessments
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                PainAssessmentId = a.PainAssessmentId,
                                KunjunganId = a.KunjunganId,
                                KeluhanUtama = a.KeluhanUtama,
                                IsPain = a.IsPain,
                                Pemicu = a.Pemicu,
                                Kualitas = a.Kualitas,
                                Lokasi = a.Lokasi,
                                SkalaPainId = a.SkalaPainId,
                                Frekuensi = a.Frekuensi,
                                PainManagement = a.PainManagement,
                                IsInheritedDisease = a.IsInheritedDisease,
                                InheritedDisease = a.InheritedDisease,
                                IsAlergic = a.IsAlergic,
                                Alergic = a.Alergic,
                                NafsuMakan = a.NafsuMakan,
                                IsMual = a.IsMual,
                                IsMuntah = a.IsMuntah,
                                IsFallRisk = a.IsFallRisk,
                                FallRisk = a.FallRisk,
                                IsBCGimunisasi = a.IsBCGimunisasi,
                                IsHepatitisBImunisasi = a.IsHepatitisBImunisasi,
                                IsPolioImunisasi = a.IsPolioImunisasi,
                                IsDPTImunisasi = a.IsDPTImunisasi,
                                IsCampakImunisasi = a.IsCampakImunisasi,
                                IsAsiEksklusif = a.IsAsiEksklusif,
                                StatusMpasi = a.StatusMpasi,
                                IsAtaksia = a.IsAtaksia,
                                IsPosturalInstability = a.IsPosturalInstability,
                                HasilResikoJatuh = a.HasilResikoJatuh,
                                IsMotorikAktif = a.IsMotorikAktif,
                                IsResponsAuditori = a.IsResponsAuditori,
                                IsInteraksiSosial = a.IsInteraksiSosial,
                                RanapId = a.RanapId,
                                RPD = a.RPD,
                                RPS = a.RPS,
                                CurrentMedication = a.CurrentMedication,
                                a.RiwayatPenyakit,
                                a.IsIGD,
                                a.MasukIGD,
                                a.KondisiMasukIGD,
                                a.IsPengobatanSaatIni,
                                a.IsTubuhTidakSeimbang,
                                a.IsMenggunakanPenopang,
                                a.KeluhanTambahan,
                                a.IsFarmakologi,
                                a.KeadaanUmum,
                                a.IsKonjungtiva,
                                a.Ekstremitas,
                                a.SkorSedasi
                            };

                // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                    query = query.Where(u =>
                        EF.Functions.ILike(u.KeluhanUtama, search) ||
                        EF.Functions.ILike(u.KunjunganId.ToString().ToLower(), search)
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

                // **Sorting Data dengan cara yang lebih aman**
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "createdatetime" => isDescending ? query.OrderByDescending(u => u.CreateDateTime) : query.OrderBy(u => u.CreateDateTime),
                    "createbyname" => isDescending ? query.OrderByDescending(u => u.CreateByName) : query.OrderBy(u => u.CreateByName),
                    "KeluhanUtama" => isDescending ? query.OrderByDescending(u => u.KeluhanUtama) : query.OrderBy(u => u.KeluhanUtama),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

                // **Pagination**
                int totalRows = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
