using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Helper;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using static QRCoder.PayloadGenerator;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class UserActiveController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserActiveController> _logger;
        private readonly string _uploadUrl;

        public UserActiveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<UserActiveController> logger,
            IConfiguration configuration
            )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }
        private string GeneratePinPegawai(DateTime? tanggalLahir)
        {
            if (tanggalLahir.HasValue)
            {
                // Mengambil dua digit tanggal dan dua digit bulan dari tanggal lahir  
                string hari = tanggalLahir.Value.Day.ToString("D2");   // Format 2 digit  
                string bulan = tanggalLahir.Value.Month.ToString("D2"); // Format 2 digit  

                // Menggabungkan menjadi PIN 4 digit  
                string pinPegawai = hari + bulan;

                return pinPegawai;
            }

            return string.Empty; // Return empty string if tanggalLahir is null  
        }

        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                tanggal,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserActive(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            try
            {
                var query =
                    from a in _applicationDbContext.UserActives.AsNoTracking()

                    join t in _applicationDbContext.TipeUsers.AsNoTracking()
                        on a.TipeUserId equals t.TipeUserId into tipeJoin
                    from tipe in tipeJoin.DefaultIfEmpty()

                    join dept in _applicationDbContext.Departements.AsNoTracking()
                        on a.DepartemenId equals dept.DepartementId into deptJoin
                    from dept in deptJoin.DefaultIfEmpty()

                    join pos in _applicationDbContext.Positions.AsNoTracking()
                        on a.PositionId equals pos.PositionId into posJoin
                    from pos in posJoin.DefaultIfEmpty()

                    join jabatan in _applicationDbContext.Jabatans.AsNoTracking()
                        on a.JabatanId equals jabatan.JabatanId into jabatanJoin
                    from jabatan in jabatanJoin.DefaultIfEmpty()

                    join creator in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals creator.UserActiveId into creatorJoin
                    from creator in creatorJoin.DefaultIfEmpty()

                    join updater in _applicationDbContext.UserActives.AsNoTracking()
                        on a.UpdateBy equals updater.UserActiveId into updaterJoin
                    from updater in updaterJoin.DefaultIfEmpty()

                    join dok in _applicationDbContext.Dokters.AsNoTracking().Where(x => x.IsDelete == false)
                        on a.UserActiveId equals dok.UserActiveId into dokJoin
                    from dok in dokJoin.DefaultIfEmpty()

                    join td in _applicationDbContext.MasterTTDs.AsNoTracking()
                        on a.UserActiveId equals td.UserActiveId into tdJoin
                    from td in tdJoin.DefaultIfEmpty()

                    join agama in _applicationDbContext.Agamas.AsNoTracking()
                        on a.AgamaId equals agama.AgamaId into agamaJoin
                    from agama in agamaJoin.DefaultIfEmpty()

                    join pendidikan in _applicationDbContext.Pendidikans.AsNoTracking()
                        on a.PendidikanTerakhirId equals pendidikan.PendidikanId into pendidikanJoin
                    from pendidikan in pendidikanJoin.DefaultIfEmpty()

                    join provinsi in _applicationDbContext.Provinsis.AsNoTracking()
                        on a.ProvinsiId equals provinsi.ProvinsiId into provinsiJoin
                    from provinsi in provinsiJoin.DefaultIfEmpty()

                    join kota in _applicationDbContext.KabupatenKotas.AsNoTracking()
                        on a.KotaId equals kota.KabupatenKotaId into kotaJoin
                    from kota in kotaJoin.DefaultIfEmpty()

                    join kec in _applicationDbContext.Kecamatans.AsNoTracking()
                        on a.KecId equals kec.KecamatanId into kecJoin
                    from kec in kecJoin.DefaultIfEmpty()

                    join kewarganegaraan in _applicationDbContext.Negaras.AsNoTracking()
                        on a.KewarganegaraanId equals kewarganegaraan.NegaraId into kewarganegaraanJoin
                    from kewarganegaraan in kewarganegaraanJoin.DefaultIfEmpty()

                    where a.IsDelete == false || a.IsDelete == null

                    select new
                    {
                        // =========================
                        // AUDIT
                        // =========================
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = creator != null ? creator.FullName : "-",

                        a.UpdateDateTime,
                        a.UpdateBy,
                        UpdateByName = updater != null ? updater.FullName : "-",

                        // =========================
                        // ID UTAMA
                        // =========================
                        a.UserActiveId,
                        KaryawanId = a.UserActiveId,
                        DokterId = dok != null ? dok.DokterId : (Guid?)null,

                        a.UserActiveCode,

                        // =========================
                        // DATA USER ACTIVE
                        // =========================
                        a.FullName,
                        NamaKaryawan = a.FullName,

                        a.IdentityNumber,
                        a.PlaceOfBirth,
                        a.DateOfBirth,
                        a.Gender,
                        a.Address,
                        a.Handphone,
                        a.Email,
                        a.IsActive,

                        a.TipeUserId,
                        NamaTipeUser = tipe != null ? tipe.NamaTipeUser : "-",

                        a.DepartemenId,
                        NamaDepartemen = dept != null ? dept.NamaDepartement : "-",
                        NamaDepartement = dept != null ? dept.NamaDepartement : "-",

                        a.PositionId,
                        NamaPosisi = pos != null ? pos.PositionName : "-",

                        a.InstalasiUnitId,

                        a.NoSTR,
                        a.StatusPegawai,
                        a.PinPegawai,

                        // =========================
                        // FOTO
                        // =========================
                        FotoName = a.FotoName,
                        FotoPath = a.FotoPath,

                        // =========================
                        // TTD
                        // =========================
                        TTDId = td != null ? td.TTDId : (Guid?)null,
                        TTDPath = td != null ? td.TTDPath : null,

                        // =========================
                        // DATA KARYAWAN
                        // =========================
                        a.JabatanId,
                        NamaJabatan = jabatan != null ? jabatan.NamaJabatan : "-",

                        a.NoIdentitas,
                        a.KodeKaryawan,
                        a.NoKaryawan,
                        a.NoRekening,
                        a.BankId,

                        a.TanggalKontrak,
                        a.TanggalAwalKerja,
                        a.TanggalAkhirKerja,

                        a.NoHandphone,
                        a.Alamat,
                        IsKaryawanMedis = a.IsKaryawanMedis ?? false,

                        // =========================
                        // DATA TAMBAHAN
                        // =========================
                        a.StatusPerkawinan,

                        a.AgamaId,
                        NamaAgama = agama != null ? agama.NamaAgama : "-",

                        a.PendidikanTerakhirId,
                        NamaPendidikanTerakhir = pendidikan != null ? pendidikan.NamaPendidikan : "-",

                        a.AlamatDomisili,

                        a.ProvinsiId,
                        NamaProvinsi = provinsi != null ? provinsi.NamaProvinsi : "-",

                        a.KotaId,
                        NamaKota = kota != null ? kota.NamaKabupatenKota : "-",

                        a.KecId,
                        NamaKecamatan = kec != null ? kec.NamaKecamatan : "-",

                        a.KewarganegaraanId,
                        NamaKewarganegaraan = kewarganegaraan != null ? kewarganegaraan.NamaNegara : "-",

                        a.StatusKewarganegaraan
                    };

                query = query.OrderByDescending(x => x.CreateDateTime);

                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (rows.Count == 0 && page > totalPages && totalRows > 0)
                {
                    return NotFound(new
                    {
                        message = "Page not found."
                    });
                }

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = rows,
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
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var data = await (
                from a in _applicationDbContext.UserActives.AsNoTracking()

                join t in _applicationDbContext.TipeUsers.AsNoTracking()
                    on a.TipeUserId equals t.TipeUserId into tipeJoin
                from tipe in tipeJoin.DefaultIfEmpty()

                join dept in _applicationDbContext.Departements.AsNoTracking()
                    on a.DepartemenId equals dept.DepartementId into deptJoin
                from dept in deptJoin.DefaultIfEmpty()

                join deptKaryawan in _applicationDbContext.Departements.AsNoTracking()
                    on a.DepartemenId equals deptKaryawan.DepartementId into deptKaryawanJoin
                from deptKaryawan in deptKaryawanJoin.DefaultIfEmpty()

                join pos in _applicationDbContext.Positions.AsNoTracking()
                    on a.PositionId equals pos.PositionId into posJoin
                from pos in posJoin.DefaultIfEmpty()

                join jabatan in _applicationDbContext.Jabatans.AsNoTracking()
                    on a.JabatanId equals jabatan.JabatanId into jabatanJoin
                from jabatan in jabatanJoin.DefaultIfEmpty()

                join creator in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals creator.UserActiveId into creatorJoin
                from creator in creatorJoin.DefaultIfEmpty()

                join updater in _applicationDbContext.UserActives.AsNoTracking()
                    on a.UpdateBy equals updater.UserActiveId into updaterJoin
                from updater in updaterJoin.DefaultIfEmpty()

                join dok in _applicationDbContext.Dokters.AsNoTracking()
                    .Where(x => x.IsDelete == false)
                    on a.UserActiveId equals dok.UserActiveId into dokJoin
                from dok in dokJoin.DefaultIfEmpty()

                join td in _applicationDbContext.MasterTTDs.AsNoTracking()
                    on a.UserActiveId equals td.UserActiveId into tdJoin
                from td in tdJoin.DefaultIfEmpty()

                join agama in _applicationDbContext.Agamas.AsNoTracking()
                    on a.AgamaId equals agama.AgamaId into agamaJoin
                from agama in agamaJoin.DefaultIfEmpty()

                join pendidikan in _applicationDbContext.Pendidikans.AsNoTracking()
                    on a.PendidikanTerakhirId equals pendidikan.PendidikanId into pendidikanJoin
                from pendidikan in pendidikanJoin.DefaultIfEmpty()

                join provinsi in _applicationDbContext.Provinsis.AsNoTracking()
                    on a.ProvinsiId equals provinsi.ProvinsiId into provinsiJoin
                from provinsi in provinsiJoin.DefaultIfEmpty()

                join kota in _applicationDbContext.KabupatenKotas.AsNoTracking()
                    on a.KotaId equals kota.KabupatenKotaId into kotaJoin
                from kota in kotaJoin.DefaultIfEmpty()

                join kec in _applicationDbContext.Kecamatans.AsNoTracking()
                    on a.KecId equals kec.KecamatanId into kecJoin
                from kec in kecJoin.DefaultIfEmpty()

                join kewarganegaraan in _applicationDbContext.Negaras.AsNoTracking()
                    on a.KewarganegaraanId equals kewarganegaraan.NegaraId into kewarganegaraanJoin
                from kewarganegaraan in kewarganegaraanJoin.DefaultIfEmpty()

                where a.IsDelete == false
                      && a.UserActiveId == id

                select new
                {
                    // =========================
                    // AUDIT
                    // =========================
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = creator != null ? creator.FullName : "-",

                    a.UpdateDateTime,
                    a.UpdateBy,
                    UpdateByName = updater != null ? updater.FullName : "-",

                    // =========================
                    // ID UTAMA
                    // =========================
                    a.UserActiveId,
                    KaryawanId = a.UserActiveId,
                    DokterId = dok != null ? dok.DokterId : (Guid?)null,

                    a.UserActiveCode,

                    // =========================
                    // DATA USER ACTIVE
                    // =========================
                    a.FullName,
                    NamaKaryawan = a.FullName,
                    a.IdentityNumber,
                    a.PlaceOfBirth,
                    a.DateOfBirth,
                    a.Gender,
                    a.Address,
                    a.Handphone,
                    a.Email,
                    a.IsActive,

                    a.TipeUserId,
                    NamaTipeUser = tipe != null ? tipe.NamaTipeUser : "-",

                    a.DepartemenId,
                    NamaDepartemen = dept != null ? dept.NamaDepartement : "-",

                    a.PositionId,
                    NamaPosisi = pos != null ? pos.PositionName : "-",

                    a.InstalasiUnitId,

                    a.NoSTR,
                    a.StatusPegawai,
                    a.PinPegawai,

                    // =========================
                    // FOTO
                    // =========================
                    FotoName = a.FotoName,
                    FotoPath = a.FotoPath,

                    // =========================
                    // TTD
                    // =========================
                    TTDId = td != null ? td.TTDId : (Guid?)null,
                    TTDPath = td != null ? td.TTDPath : null,

                    // =========================
                    // DATA KARYAWAN
                    // =========================
                    NamaDepartement = deptKaryawan != null ? deptKaryawan.NamaDepartement : "-",

                    a.JabatanId,
                    NamaJabatan = jabatan != null ? jabatan.NamaJabatan : "-",

                    a.NoIdentitas,
                    a.KodeKaryawan,
                    a.NoKaryawan,
                    a.NoRekening,
                    a.BankId,

                    a.TanggalKontrak,
                    a.TanggalAwalKerja,
                    a.TanggalAkhirKerja,

                    a.NoHandphone,
                    a.Alamat,
                    a.IsKaryawanMedis,

                    // =========================
                    // DATA TAMBAHAN
                    // =========================
                    a.StatusPerkawinan,

                    a.AgamaId,
                    NamaAgama = agama != null ? agama.NamaAgama : "-",

                    a.PendidikanTerakhirId,
                    NamaPendidikanTerakhir = pendidikan != null ? pendidikan.NamaPendidikan : "-",

                    a.AlamatDomisili,

                    a.ProvinsiId,
                    NamaProvinsi = provinsi != null ? provinsi.NamaProvinsi : "-",

                    a.KotaId,
                    NamaKota = kota != null ? kota.NamaKabupatenKota : "-",

                    a.KecId,
                    NamaKecamatan = kec != null ? kec.NamaKecamatan : "-",

                    a.KewarganegaraanId,
                    NamaKewarganegaraan = kewarganegaraan != null ? kewarganegaraan.NamaNegara : "-",

                    a.StatusKewarganegaraan
                }
            ).FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Data tidak ditemukan"
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }

        [HttpGet("UserActiveDoctors/{id}")]
        public async Task<IActionResult> GetUserActiveDokterById(Guid id)
        {
            try
            {
                // Ambil data UserActive berdasarkan ID
                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.UserActiveId == id && (u.IsDelete == null || u.IsDelete == false));

                if (user == null)
                {
                    return NotFound(new { message = "UserActive tidak ditemukan." });
                }

                // Ambil nama tipe user dari TipeUsers
                var tipeUser = await _applicationDbContext.TipeUsers
                    .FirstOrDefaultAsync(t => t.TipeUserId == user.TipeUserId);
                var tipeUserName = tipeUser?.NamaTipeUser ?? "Unknown";

                // Jika user adalah dokter, cari data dari tabel Dokter
                object dataDokter = null;
                if (tipeUserName.ToLower() == "dokter")
                {
                    var dokter = await _applicationDbContext.Dokters
                        .FirstOrDefaultAsync(d =>
                            d.UserActiveId == user.UserActiveId &&
                            (d.IsDelete == false));

                    if (dokter != null)
                    {
                        dataDokter = new
                        {
                            dokter.DokterId,
                            //dokter.KdDokter,
                            //dokter.NmDokter,
                            //dokter.Nik,
                            //dokter.Email,
                            //dokter.Nohp,
                            //dokter.Alamat,
                            //dokter.Spesialis,
                            //dokter.Str,
                            //dokter.Sip,
                            //dokter.TglSip,
                            //dokter.TglStr,
                            //dokter.IsActive,
                            //dokter.FotoName,
                            //dokter.FotoPath
                        };
                    }
                }

                return Ok(new
                {
                    message = "Berhasil mengambil data user active dokter",
                    data = new
                    {
                        user.UserActiveId,
                        user.UserActiveCode,
                        user.FullName,
                        user.Email,
                        user.IdentityNumber,
                        user.PlaceOfBirth,
                        user.DateOfBirth,
                        user.Gender,
                        user.Handphone,
                        user.Address,
                        user.IsActive,
                        user.TipeUserId,
                        NamaTipeUser = tipeUserName,
                        user.DepartemenId,
                        user.PositionId,
                        //FotoPath = string.IsNullOrWhiteSpace(user.FotoPath)
                        //    ? "/FotoDokter/dokter.jpg"
                        //    : user.FotoPath,
                        dataDokter,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPost("UserActive")]
        public async Task<IActionResult> CreateUserActive([FromForm] UserActiveViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
            }

            await using var transaction = await _applicationDbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                {
                    await transaction.RollbackAsync();
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u =>
                        u.Email == emailLogin &&
                        (u.IsDelete == false || u.IsDelete == null));

                if (getUserActive == null)
                {
                    await transaction.RollbackAsync();
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveLoginId = getUserActive.UserActiveId;

                var fullName = vm.FullName?.Trim();
                var identityNumber = vm.IdentityNumber?.Trim();
                var email = vm.Email?.Trim();
                var handphone = vm.Handphone?.Trim();

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nama lengkap wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(identityNumber))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nomor identitas wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Email wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(handphone))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nomor handphone wajib diisi." });
                }

                if (!DateTime.TryParseExact(
                        vm.DateOfBirth,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDateOfBirth))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }

                parsedDateOfBirth = DateTime.SpecifyKind(parsedDateOfBirth, DateTimeKind.Utc);

                var normalizedEmail = email.ToLowerInvariant();
                var normalizedIdentityNumber = identityNumber.ToLowerInvariant();
                var normalizedFullName = fullName.ToLowerInvariant();

                var isDuplicateUserActive = await _applicationDbContext.UserActives
                    .AnyAsync(c =>
                        (c.IsDelete == false || c.IsDelete == null) &&
                        (
                            c.Email.ToLower().Trim() == normalizedEmail ||
                            c.IdentityNumber.ToLower().Trim() == normalizedIdentityNumber ||
                            c.FullName.ToLower().Trim() == normalizedFullName
                        ));

                if (isDuplicateUserActive)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new { message = "User ini telah terdaftar." });
                }

                var localNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
                var setDateNow = localNow.ToString("yyMMdd");

                // =========================
                // GENERATE USER ACTIVE CODE
                // Format: USR2607060001
                // =========================
                var userCodePrefix = $"USR{setDateNow}";

                var existingUserCodes = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .Where(x =>
                        x.UserActiveCode != null &&
                        x.UserActiveCode.StartsWith(userCodePrefix))
                    .Select(x => x.UserActiveCode)
                    .ToListAsync();

                var lastUserRunningNumber = existingUserCodes
                    .Select(code =>
                    {
                        if (string.IsNullOrWhiteSpace(code) || code.Length < userCodePrefix.Length + 4)
                            return 0;

                        var runningText = code.Substring(userCodePrefix.Length);

                        return int.TryParse(runningText, out var parsed)
                            ? parsed
                            : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                var kode = $"{userCodePrefix}{lastUserRunningNumber + 1:D4}";

                // =========================
                // CEK TIPE USER
                // =========================
                var tipeUser = await _applicationDbContext.TipeUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);

                var isDokter = string.Equals(
                    tipeUser?.NamaTipeUser?.Trim(),
                    "dokter",
                    StringComparison.OrdinalIgnoreCase);

                // =========================
                // GENERATE KODE DOKTER JIKA DOKTER
                // Format: DKR2607060001
                // =========================
                string? kodeDokter = null;

                if (isDokter)
                {
                    var dokterCodePrefix = $"DKR{setDateNow}";

                    var existingDokterCodes = await _applicationDbContext.Dokters
                        .AsNoTracking()
                        .Where(x =>
                            x.KdDokter != null &&
                            x.KdDokter.StartsWith(dokterCodePrefix))
                        .Select(x => x.KdDokter)
                        .ToListAsync();

                    var lastDokterRunningNumber = existingDokterCodes
                        .Select(code =>
                        {
                            if (string.IsNullOrWhiteSpace(code) || code.Length < dokterCodePrefix.Length + 4)
                                return 0;

                            var runningText = code.Substring(dokterCodePrefix.Length);

                            return int.TryParse(runningText, out var parsed)
                                ? parsed
                                : 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();

                    kodeDokter = $"{dokterCodePrefix}{lastDokterRunningNumber + 1:D4}";
                }

                // =========================
                // GENERATE NO KARYAWAN JIKA BUKAN DOKTER
                // Format: EMP2026-KODEDEPT-0706-00001
                // Running number reset setiap tahun
                // =========================

                string? noKaryawan = null;

                if (!isDokter)
                {
                    var departemenId = vm.DepartemenId;

                    if (!departemenId.HasValue || departemenId.Value == Guid.Empty)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Departemen wajib dipilih untuk membuat nomor karyawan." });
                    }

                    var kodeDepartement = await _applicationDbContext.Departements
                        .AsNoTracking()
                        .Where(x => x.DepartementId == departemenId.Value)
                        .Select(x => x.KodeDepartement)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrWhiteSpace(kodeDepartement))
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Kode departement tidak ditemukan." });
                    }

                    kodeDepartement = new string(
                        kodeDepartement
                            .Trim()
                            .ToUpperInvariant()
                            .Where(char.IsLetterOrDigit)
                            .ToArray()
                    );

                    if (string.IsNullOrWhiteSpace(kodeDepartement))
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Kode departement tidak valid." });
                    }

                    var tahun = localNow.Year;
                    var bulan = localNow.Month.ToString("D2");
                    var hari = localNow.Day.ToString("D2");

                    var prefixTahun = $"EMP{tahun}-";

                    var existingNoKaryawan = await _applicationDbContext.UserActives
                        .AsNoTracking()
                        .Where(x =>
                            (x.IsDelete == false || x.IsDelete == null) &&
                            x.NoKaryawan != null &&
                            x.NoKaryawan.StartsWith(prefixTahun))
                        .Select(x => x.NoKaryawan)
                        .ToListAsync();

                    var lastRunningNumber = existingNoKaryawan
                        .Select(no =>
                        {
                            if (string.IsNullOrWhiteSpace(no))
                                return 0;

                            var lastSegment = no.Split('-').LastOrDefault();

                            return int.TryParse(lastSegment, out var parsed)
                                ? parsed
                                : 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();

                    noKaryawan = $"EMP{tahun}-{kodeDepartement}-{bulan}{hari}-{lastRunningNumber + 1:D5}";
                }

                // =========================
                // FOTO DEFAULT
                // =========================
                var fotoFileName = "user.jpeg";
                var fotoPath = "/default-photo/user.jpeg";

                var id = Guid.NewGuid();

                var userLogin = new ApplicationUser
                {
                    KodeUser = kode,
                    NamaUser = fullName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = handphone,
                    IsActive = true
                };

                var user = new UserActive
                {
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveLoginId,
                    UserActiveId = id,

                    UserActiveCode = kode,
                    FullName = fullName,
                    PinPegawai = DelegasiVerifikasi.ComputeSha256Hash(
                        GeneratePinPegawai(parsedDateOfBirth)
                    ),

                    IdentityNumber = identityNumber,
                    PlaceOfBirth = vm.PlaceOfBirth?.Trim(),
                    DateOfBirth = parsedDateOfBirth,
                    Gender = vm.Gender?.Trim(),
                    Address = vm.Address?.Trim(),
                    Handphone = handphone,
                    Email = email,

                    IsActive = true,
                    IsDelete = false,

                    DepartemenId = vm.DepartemenId,
                    PositionId = vm.PositionId,
                    TipeUserId = vm.TipeUserId,
                    InstalasiUnitId = vm.InstalasiUnitId,

                    NoSTR = vm.NoSTR?.Trim(),
                    StatusPegawai = vm.StatusPegawai?.Trim(),

                    FotoName = fotoFileName,
                    FotoPath = fotoPath,

                    // =========================
                    // DATA KARYAWAN DI USERACTIVE
                    // =========================
                    JabatanId = vm.JabatanId,

                    NoIdentitas = string.IsNullOrWhiteSpace(vm.NoIdentitas)
                        ? identityNumber
                        : vm.NoIdentitas.Trim(),

                    KodeKaryawan = !string.IsNullOrWhiteSpace(noKaryawan)
                        ? noKaryawan
                        : vm.KodeKaryawan?.Trim(),

                    NoKaryawan = !string.IsNullOrWhiteSpace(noKaryawan)
                        ? noKaryawan
                        : vm.NoKaryawan?.Trim(),

                    NoRekening = vm.NoRekening?.Trim(),
                    BankId = vm.BankId?.Trim(),

                    TanggalKontrak = vm.TanggalKontrak,
                    TanggalAwalKerja = vm.TanggalAwalKerja,
                    TanggalAkhirKerja = vm.TanggalAkhirKerja,

                    NoHandphone = string.IsNullOrWhiteSpace(vm.NoHandphone)
                        ? handphone
                        : vm.NoHandphone.Trim(),

                    Alamat = string.IsNullOrWhiteSpace(vm.Alamat)
                        ? vm.Address?.Trim()
                        : vm.Alamat.Trim(),

                    IsKaryawanMedis = vm.IsKaryawanMedis ?? false,

                    // =========================
                    // DATA TAMBAHAN
                    // =========================
                    StatusPerkawinan = vm.StatusPerkawinan?.Trim(),
                    AgamaId = vm.AgamaId,
                    PendidikanTerakhirId = vm.PendidikanTerakhirId,
                    AlamatDomisili = vm.AlamatDomisili?.Trim(),
                    ProvinsiId = vm.ProvinsiId,
                    KotaId = vm.KotaId,
                    KecId = vm.KecId,
                    KewarganegaraanId = vm.KewarganegaraanId,
                    StatusKewarganegaraan = vm.StatusKewarganegaraan?.Trim()
                };

                var passwordDefault = parsedDateOfBirth.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);

                var resultLogin = await _userManager.CreateAsync(userLogin, passwordDefault);

                if (!resultLogin.Succeeded)
                {
                    var errorMessage = string.Join(", ", resultLogin.Errors.Select(e => e.Description));
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        message = $"Gagal membuat user login: {errorMessage}"
                    });
                }

                _applicationDbContext.UserActives.Add(user);

                Guid? createdDokterId = null;

                if (isDokter)
                {
                    var dokter = new Dokter
                    {
                        DokterId = Guid.NewGuid(),
                        KdDokter = kodeDokter,
                        NmDokter = fullName,
                        Email = email,
                        Nohp = handphone,
                        Nik = identityNumber,
                        Alamat = vm.Address?.Trim(),
                        UserActiveId = user.UserActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveLoginId,
                        IsActive = true,
                        IsDelete = false
                    };

                    createdDokterId = dokter.DokterId;
                    _applicationDbContext.Dokters.Add(dokter);
                }

                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // =========================
                // ROLE USER
                // =========================
                var identityUser = await _applicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (identityUser != null && vm.PositionId.HasValue)
                {
                    await CreateRole(vm.PositionId.Value, identityUser.Id);
                }

                return Created("", new
                {
                    message = "Tambah Data Berhasil || 201 Created",
                    data = new
                    {
                        user.UserActiveId,
                        KaryawanId = user.UserActiveId,
                        DokterId = createdDokterId,

                        user.UserActiveCode,
                        user.FullName,
                        user.Email,
                        user.IdentityNumber,

                        user.NoKaryawan,
                        user.KodeKaryawan,
                        user.IsKaryawanMedis,

                        user.FotoName,
                        user.FotoPath,

                        user.DepartemenId,
                        user.PositionId,
                        user.JabatanId,
                        user.TipeUserId,
                        user.InstalasiUnitId,

                        user.StatusPerkawinan,
                        user.AgamaId,
                        user.PendidikanTerakhirId,
                        user.AlamatDomisili,
                        user.ProvinsiId,
                        user.KotaId,
                        user.KecId,
                        user.KewarganegaraanId,
                        user.StatusKewarganegaraan,

                        user.CreateDateTime
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }
        //[HttpPost("UserActive")]
        //public async Task<IActionResult> CreateUserActive([FromForm] UserActiveViewModel vm)
        //{
        //    if (vm == null)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Ambil User ID dari JWT Claims**
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var dateNow = DateTime.UtcNow; ;
        //        var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

        //        // Ambil data terakhir untuk hari ini (tanpa ToString di query)
        //        var lastCode = _applicationDbContext.UserActives
        //            .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //            .OrderByDescending(k => k.UserActiveCode)
        //            .FirstOrDefault();

        //        string kode;
        //        if (lastCode == null)
        //        {
        //            kode = "USR" + setDateNow + "0001";
        //        }
        //        else
        //        {
        //            var lastCodeTrim = lastCode.UserActiveCode.Substring(3, 6);

        //            if (lastCodeTrim != setDateNow)
        //            {
        //                kode = "USR" + setDateNow + "0001";
        //            }
        //            else
        //            {
        //                kode = "USR" + setDateNow +
        //                    (Convert.ToInt32(lastCode.UserActiveCode.Substring(9)) + 1).ToString("D4");
        //            }
        //        }

        //        // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
        //        if (!DateTime.TryParseExact(vm.DateOfBirth, "yyyy-MM-dd",
        //            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
        //        }
        //        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //        // Ambil tipe user
        //        var tipeUser = await _applicationDbContext.TipeUsers.FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);
        //        var isDokter = tipeUser?.NamaTipeUser.ToLower() == "dokter";

        //        // Jika dokter, generate kode dokter untuk nama file
        //        string kodeDokter = "";

        //        if (isDokter)
        //        {
        //            var lastDr = _applicationDbContext.Dokters
        //                .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //                .OrderByDescending(k => k.KdDokter)
        //                .FirstOrDefault();

        //            if (lastDr == null)
        //            {
        //                kodeDokter = "DKR" + setDateNow + "0001";
        //            }
        //            else
        //            {
        //                var lastCodeTrim = lastDr.KdDokter.Substring(3, 6);

        //                if (lastCodeTrim != setDateNow)
        //                {
        //                    kodeDokter = "DKR" + setDateNow + "0001";
        //                }
        //                else
        //                {
        //                    kodeDokter = "DKR" + setDateNow +
        //                        (Convert.ToInt32(lastDr.KdDokter.Substring(9)) + 1).ToString("D4");
        //                }
        //            }
        //        }

        //        // set foto default
        //        var fotoPath = isDokter ? "/default-photo/user.jpeg" : "/default-photo/user.jpeg";
        //        var fotoFileName = isDokter ? "user.jpeg" : "user.jpeg";

        //        // Cek Duplikasi
        //        var id = Guid.NewGuid();
        //        var isDuplicateUserActive = await _applicationDbContext.UserActives
        //            .AnyAsync(c => c.UserActiveCode == kode 
        //            && c.Email == vm.Email
        //            && c.IdentityNumber == vm.IdentityNumber
        //            && c.FullName.Trim().ToLower() == c.FullName.Trim().ToLower()
        //            );

        //        if (isDuplicateUserActive) 
        //        {
        //            return Conflict(new { message = "User ini telah terdaftar" });
        //        }

        //        // Validate ModelState
        //        if (ModelState.IsValid)
        //        {

        //            var userLogin = new ApplicationUser
        //            {
        //                KodeUser = kode,
        //                NamaUser = vm.FullName,
        //                Email = vm.Email,
        //                UserName = vm.Email,
        //                PhoneNumber = vm.Handphone,
        //                IsActive = true
        //            };

        //            var user = new UserActive
        //            {
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = UserActiveId,
        //                UserActiveId = id,
        //                UserActiveCode = kode,
        //                FullName = vm.FullName,
        //                PinPegawai = DelegasiVerifikasi.ComputeSha256Hash(GeneratePinPegawai(TryParseTanggalToUtc(vm.DateOfBirth))),
        //                IdentityNumber = vm.IdentityNumber,
        //                PlaceOfBirth = vm.PlaceOfBirth,
        //                DateOfBirth = (DateTime)TryParseTanggalToUtc(vm.DateOfBirth),
        //                Gender = vm.Gender,
        //                Address = vm.Address,
        //                Handphone = vm.Handphone,
        //                Email = vm.Email,
        //                IsActive = true,
        //                DepartemenId = vm.DepartemenId,
        //                PositionId = vm.PositionId,
        //                TipeUserId = vm.TipeUserId,
        //                InstalasiUnitId = vm.InstalasiUnitId,
        //                NoSTR = vm.NoSTR,
        //                StatusPegawai = vm.StatusPegawai,
        //                FotoName = fotoFileName,
        //                FotoPath = fotoPath,
        //            };

        //            var passTglLahir = parsedDate.ToString("ddMMMyyyy");

        //            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

        //            var resultLogin = await _userManager.CreateAsync(userLogin, passTglLahir);

        //            if (!resultLogin.Succeeded)
        //            {
        //                var errorMessage = string.Join(", ", resultLogin.Errors.Select(e => e.Description));
        //                await transaction.RollbackAsync();
        //                return BadRequest(new { message = $"Gagal membuat user login: {errorMessage}" });
        //            }

        //            _applicationDbContext.UserActives.Add(user);


        //            // Jika tipe user adalah dokter → buat entri di tabel Dokter
        //            Guid? createdDokterId = null;
        //            if (isDokter)
        //            {
        //                var dokter = new Dokter
        //                {
        //                    DokterId = Guid.NewGuid(),
        //                    KdDokter = kodeDokter,
        //                    NmDokter = vm.FullName,
        //                    Email = vm.Email,
        //                    Nohp = vm.Handphone,
        //                    Nik = vm.IdentityNumber,
        //                    Alamat = vm.Address,
        //                    UserActiveId = user.UserActiveId,
        //                    CreateDateTime = DateTimeOffset.UtcNow,
        //                    CreateBy = UserActiveId,
        //                    IsActive = true,
        //                    IsDelete = false
        //                };
        //                createdDokterId = dokter.DokterId;
        //                _applicationDbContext.Dokters.Add(dokter);
        //            }
        //            else
        //            {
        //                string noKaryawan = "";

        //                var lastKaryawan = _applicationDbContext.UserActives
        //                    .Where(k => k.CreateDateTime.Date == dateNow.Date)
        //                    .OrderByDescending(k => k.NoKaryawan)
        //                    .FirstOrDefault();

        //                if (lastKaryawan == null)
        //                {
        //                    noKaryawan = "KRY" + setDateNow + "0001";
        //                }
        //                else
        //                {
        //                    var lastCodeTrim = lastKaryawan.NoKaryawan.Substring(3, 6);

        //                    if (lastCodeTrim != setDateNow)
        //                    {
        //                        noKaryawan = "KRY" + setDateNow + "0001";
        //                    }
        //                    else
        //                    {
        //                        noKaryawan = "KRY" + setDateNow +
        //                            (Convert.ToInt32(lastKaryawan.NoKaryawan.Substring(9)) + 1).ToString("D4");
        //                    }
        //                }

        //                user.NoKaryawan = noKaryawan;
        //                user.NoIdentitas = user.IdentityNumber;
        //                user.Alamat = user.Address;
        //                user.NoHandphone = user.Handphone;
        //                user.FotoName = fotoFileName;
        //                user.FotoPath = fotoPath;
        //                user.DepartementId = user.DepartemenId;
        //                user.InstalasiUnitId = user.InstalasiUnitId;
        //                // tidak perlu Add lagi!
        //            }

        //            await _applicationDbContext.SaveChangesAsync();
        //            await transaction.CommitAsync();

        //            // ROLE USER
        //            var idnetuser = await _applicationDbContext.Users
        //            .FirstOrDefaultAsync(u => u.Email == vm.Email);

        //            if (idnetuser != null) // ← ini yang benar
        //            {
        //                // Memastikan PositionId tidak kosong
        //                if (vm.PositionId.HasValue)
        //                {
        //                    var createRoleResponse = CreateRole(vm.PositionId.Value, idnetuser.Id);
        //                    // Jika CreateRole mengembalikan Task/IActionResult, jangan lupa pakai await
        //                }
        //            }
        //            // END ROLE USER

        //            return Created("", new 
        //            {   message = "Tambah Data Berhasil || 201 Created",
        //                dokterId = createdDokterId
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });

        //    }
        //}


        // GET: api/Administrator/UserActive/ByDepartemen/{departemenId}
        [HttpGet("ByDepartemen/{departemenId}")]
        public async Task<IActionResult> GetUsersByDepartemen(Guid departemenId)
        {
            var users = await (from u in _applicationDbContext.UserActives
                               where u.DepartemenId == departemenId && u.IsDelete == false
                               select new
                               {
                                   FullName = u.FullName,
                                   IdentityNumber = u.IdentityNumber,
                                   PlaceOfBirth = u.PlaceOfBirth,
                                   DateOfBirth = u.DateOfBirth.ToString("yyyy-MM-dd"),
                                   Gender = u.Gender,
                                   Address = u.Address,
                                   Handphone = u.Handphone,
                                   Email = u.Email,
                                   IsActive = u.IsActive,
                                   DepartemenId = u.DepartemenId,
                                   PositionId = u.PositionId,
                                   TipeUserId = u.TipeUserId,

                                   NoSTR = u.NoSTR,
                                   StatusPegawai = u.StatusPegawai,
                               }).ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound(new { message = "Data user dengan DepartemenId tersebut tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = users
            });
        }


        [HttpPut("UserActive/{id}")]
        public async Task<IActionResult> UpdateUserActive(Guid id, [FromForm] UpdateUserActiveViewModel vm)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "UserActiveId tidak valid." });
            }

            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
            }

            await using var transaction = await _applicationDbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                {
                    await transaction.RollbackAsync();
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var loginUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u =>
                        u.Email == emailLogin &&
                        (u.IsDelete == false || u.IsDelete == null));

                if (loginUserActive == null)
                {
                    await transaction.RollbackAsync();
                    return Unauthorized(new { message = "Data user login tidak ditemukan!" });
                }

                var userActiveLoginId = loginUserActive.UserActiveId;

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u =>
                        u.UserActiveId == id &&
                        (u.IsDelete == false || u.IsDelete == null));

                if (user == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = "Data UserActive tidak ditemukan." });
                }

                var fullName = vm.FullName?.Trim();
                var identityNumber = vm.IdentityNumber?.Trim();
                var email = vm.Email?.Trim();
                var handphone = vm.Handphone?.Trim();

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nama lengkap wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(identityNumber))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nomor identitas wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Email wajib diisi." });
                }

                if (string.IsNullOrWhiteSpace(handphone))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Nomor handphone wajib diisi." });
                }

                if (!DateTime.TryParseExact(
                        vm.DateOfBirth,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime parsedDate))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }

                parsedDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);

                var normalizedEmail = email.ToLowerInvariant();
                var normalizedIdentityNumber = identityNumber.ToLowerInvariant();

                var isDuplicateEmail = await _applicationDbContext.UserActives
                    .AnyAsync(x =>
                        (x.IsDelete == false || x.IsDelete == null) &&
                        x.UserActiveId != id &&
                        x.Email != null &&
                        x.Email.ToLower().Trim() == normalizedEmail);

                if (isDuplicateEmail)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new { message = "Email sudah dipakai oleh user lain! || 409 Conflict" });
                }

                var isDuplicateIdentity = await _applicationDbContext.UserActives
                    .AnyAsync(x =>
                        (x.IsDelete == false || x.IsDelete == null) &&
                        x.UserActiveId != id &&
                        x.IdentityNumber != null &&
                        x.IdentityNumber.ToLower().Trim() == normalizedIdentityNumber);

                if (isDuplicateIdentity)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new { message = "Nomor identitas sudah dipakai oleh user lain! || 409 Conflict" });
                }

                var tipeUser = await _applicationDbContext.TipeUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);

                var isDokter = string.Equals(
                    tipeUser?.NamaTipeUser?.Trim(),
                    "dokter",
                    StringComparison.OrdinalIgnoreCase);

                // =========================
                // UPDATE ASP.NET IDENTITY
                // =========================
                var identityUser = await _userManager.FindByEmailAsync(user.Email);

                if (identityUser == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "User login tidak ditemukan." });
                }

                if (!string.Equals(identityUser.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    identityUser.Email = email;
                    identityUser.UserName = email;
                }

                identityUser.NamaUser = fullName;
                identityUser.PhoneNumber = handphone;
                identityUser.IsActive = true;

                var resultUpdateIdentity = await _userManager.UpdateAsync(identityUser);

                if (!resultUpdateIdentity.Succeeded)
                {
                    var err = string.Join(", ", resultUpdateIdentity.Errors.Select(e => e.Description));
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        message = $"Gagal update user login: {err}"
                    });
                }

                // =========================
                // UPDATE USERACTIVE UTAMA
                // =========================
                user.FullName = fullName;
                user.IdentityNumber = identityNumber;
                user.PlaceOfBirth = vm.PlaceOfBirth?.Trim();
                user.DateOfBirth = parsedDate;
                user.Gender = vm.Gender?.Trim();
                user.Address = vm.Address?.Trim();
                user.Handphone = handphone;
                user.Email = email;

                user.DepartemenId = vm.DepartemenId;
                user.PositionId = vm.PositionId;
                user.TipeUserId = vm.TipeUserId;
                user.InstalasiUnitId = vm.InstalasiUnitId;

                user.NoSTR = vm.NoSTR?.Trim();
                user.StatusPegawai = vm.StatusPegawai?.Trim();
                user.IsActive = true;

                user.PinPegawai = DelegasiVerifikasi.ComputeSha256Hash(
                    GeneratePinPegawai(parsedDate)
                );

                // =========================
                // UPDATE DATA KARYAWAN
                // =========================
                user.JabatanId = vm.JabatanId;

                user.NoIdentitas = string.IsNullOrWhiteSpace(vm.NoIdentitas)
                    ? identityNumber
                    : vm.NoIdentitas.Trim();

                user.NoRekening = vm.NoRekening?.Trim();
                user.BankId = vm.BankId?.Trim();

                user.TanggalKontrak = vm.TanggalKontrak;
                user.TanggalAwalKerja = vm.TanggalAwalKerja;
                user.TanggalAkhirKerja = vm.TanggalAkhirKerja;

                user.NoHandphone = string.IsNullOrWhiteSpace(vm.NoHandphone)
                    ? handphone
                    : vm.NoHandphone.Trim();

                user.Alamat = string.IsNullOrWhiteSpace(vm.Alamat)
                    ? vm.Address?.Trim()
                    : vm.Alamat.Trim();

                user.IsKaryawanMedis = vm.IsKaryawanMedis ?? false;

                // =========================
                // UPDATE DATA TAMBAHAN
                // =========================
                user.StatusPerkawinan = vm.StatusPerkawinan?.Trim();
                user.AgamaId = vm.AgamaId;
                user.PendidikanTerakhirId = vm.PendidikanTerakhirId;
                user.AlamatDomisili = vm.AlamatDomisili?.Trim();
                user.ProvinsiId = vm.ProvinsiId;
                user.KotaId = vm.KotaId;
                user.KecId = vm.KecId;
                user.KewarganegaraanId = vm.KewarganegaraanId;
                user.StatusKewarganegaraan = vm.StatusKewarganegaraan?.Trim();

                user.UpdateDateTime = DateTimeOffset.UtcNow;
                user.UpdateBy = userActiveLoginId;

                // =========================
                // SYNC DOKTER
                // =========================
                var dokterExisting = await _applicationDbContext.Dokters
                    .Where(d => d.UserActiveId == user.UserActiveId)
                    .OrderBy(d => d.IsDelete)
                    .ThenByDescending(d => d.CreateDateTime)
                    .FirstOrDefaultAsync();

                string kdDokter = dokterExisting?.KdDokter ?? string.Empty;
                string noKaryawan = user.NoKaryawan ?? string.Empty;

                if (isDokter)
                {
                    if (dokterExisting == null)
                    {
                        var localNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
                        var setDateNow = localNow.ToString("yyMMdd");
                        var dokterCodePrefix = $"DKR{setDateNow}";

                        var existingDokterCodes = await _applicationDbContext.Dokters
                            .AsNoTracking()
                            .Where(x =>
                                x.KdDokter != null &&
                                x.KdDokter.StartsWith(dokterCodePrefix))
                            .Select(x => x.KdDokter)
                            .ToListAsync();

                        var lastDokterRunningNumber = existingDokterCodes
                            .Select(code =>
                            {
                                if (string.IsNullOrWhiteSpace(code) || code.Length < dokterCodePrefix.Length + 4)
                                    return 0;

                                var runningText = code.Substring(dokterCodePrefix.Length);

                                return int.TryParse(runningText, out var parsed)
                                    ? parsed
                                    : 0;
                            })
                            .DefaultIfEmpty(0)
                            .Max();

                        kdDokter = $"{dokterCodePrefix}{lastDokterRunningNumber + 1:D4}";

                        var dokterBaru = new Dokter
                        {
                            DokterId = Guid.NewGuid(),
                            KdDokter = kdDokter,
                            NmDokter = fullName,
                            Email = email,
                            Nohp = handphone,
                            Nik = identityNumber,
                            Alamat = vm.Address?.Trim(),
                            UserActiveId = user.UserActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = userActiveLoginId,
                            IsActive = true,
                            IsDelete = false
                        };

                        _applicationDbContext.Dokters.Add(dokterBaru);
                    }
                    else
                    {
                        kdDokter = dokterExisting.KdDokter;

                        dokterExisting.NmDokter = fullName;
                        dokterExisting.Email = email;
                        dokterExisting.Nohp = handphone;
                        dokterExisting.Nik = identityNumber;
                        dokterExisting.Alamat = vm.Address?.Trim();
                        dokterExisting.IsActive = true;
                        dokterExisting.IsDelete = false;
                        dokterExisting.UpdateDateTime = DateTimeOffset.UtcNow;
                        dokterExisting.UpdateBy = userActiveLoginId;

                        _applicationDbContext.Dokters.Update(dokterExisting);
                    }
                }
                else
                {
                    // Jika sebelumnya dokter, maka data dokter dinonaktifkan
                    if (dokterExisting != null && dokterExisting.IsDelete == false)
                    {
                        dokterExisting.IsDelete = true;
                        dokterExisting.IsActive = false;
                        dokterExisting.UpdateDateTime = DateTimeOffset.UtcNow;
                        dokterExisting.UpdateBy = userActiveLoginId;

                        _applicationDbContext.Dokters.Update(dokterExisting);
                    }

                    // Generate NoKaryawan hanya jika belum ada
                    if (string.IsNullOrWhiteSpace(user.NoKaryawan))
                    {
                        var departemenId = vm.DepartemenId;

                        if (!departemenId.HasValue || departemenId.Value == Guid.Empty)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = "Departemen wajib dipilih untuk membuat nomor karyawan." });
                        }

                        var kodeDepartement = await _applicationDbContext.Departements
                            .AsNoTracking()
                            .Where(x => x.DepartementId == departemenId.Value)
                            .Select(x => x.KodeDepartement)
                            .FirstOrDefaultAsync();

                        if (string.IsNullOrWhiteSpace(kodeDepartement))
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = "Kode departement tidak ditemukan." });
                        }

                        kodeDepartement = new string(
                            kodeDepartement
                                .Trim()
                                .ToUpperInvariant()
                                .Where(char.IsLetterOrDigit)
                                .ToArray()
                        );

                        if (string.IsNullOrWhiteSpace(kodeDepartement))
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = "Kode departement tidak valid." });
                        }

                        var localNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));

                        var tahun = localNow.Year;
                        var bulan = localNow.Month.ToString("D2");
                        var hari = localNow.Day.ToString("D2");
                        var prefixTahun = $"EMP{tahun}-";

                        var existingNoKaryawan = await _applicationDbContext.UserActives
                            .AsNoTracking()
                            .Where(x =>
                                (x.IsDelete == false || x.IsDelete == null) &&
                                x.NoKaryawan != null &&
                                x.NoKaryawan.StartsWith(prefixTahun))
                            .Select(x => x.NoKaryawan)
                            .ToListAsync();

                        var lastRunningNumber = existingNoKaryawan
                            .Select(no =>
                            {
                                if (string.IsNullOrWhiteSpace(no))
                                    return 0;

                                var lastSegment = no.Split('-').LastOrDefault();

                                return int.TryParse(lastSegment, out var parsed)
                                    ? parsed
                                    : 0;
                            })
                            .DefaultIfEmpty(0)
                            .Max();

                        noKaryawan = $"EMP{tahun}-{kodeDepartement}-{bulan}{hari}-{lastRunningNumber + 1:D5}";

                        user.NoKaryawan = noKaryawan;
                        user.KodeKaryawan = noKaryawan;
                    }
                    else
                    {
                        noKaryawan = user.NoKaryawan;
                        user.KodeKaryawan = string.IsNullOrWhiteSpace(user.KodeKaryawan)
                            ? user.NoKaryawan
                            : user.KodeKaryawan;
                    }
                }

                // =========================
                // UPLOAD FOTO OPTIONAL
                // =========================
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    const long maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLowerInvariant();

                    if (vm.Foto.Length > maxSize)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG, JPEG, atau PNG." });
                    }

                    var contentType = string.IsNullOrWhiteSpace(vm.Foto.ContentType)
                        ? "image/jpeg"
                        : vm.Foto.ContentType;

                    string namaDasarFoto;

                    if (isDokter)
                    {
                        namaDasarFoto = !string.IsNullOrWhiteSpace(kdDokter)
                            ? kdDokter
                            : user.UserActiveCode;
                    }
                    else
                    {
                        namaDasarFoto = !string.IsNullOrWhiteSpace(noKaryawan)
                            ? noKaryawan
                            : user.UserActiveCode;
                    }

                    namaDasarFoto = new string(
                        namaDasarFoto
                            .Trim()
                            .ToUpperInvariant()
                            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                            .ToArray()
                    );

                    while (namaDasarFoto.Contains("--"))
                    {
                        namaDasarFoto = namaDasarFoto.Replace("--", "-");
                    }

                    namaDasarFoto = namaDasarFoto.Trim('-');

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fotoFileName = $"{namaDasarFoto}_{safeTime}{fileExtension}";
                    var oldFileName = user.FotoName ?? string.Empty;

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();

                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var streamContent = new StreamContent(ms);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var content = new MultipartFormDataContent();
                    content.Add(streamContent, "file", fotoFileName);
                    content.Add(new StringContent("FotoKaryawan"), "folderTarget");

                    if (!string.IsNullOrWhiteSpace(oldFileName) && oldFileName != "user.jpeg")
                    {
                        content.Add(new StringContent(oldFileName), "oldFileName");
                    }

                    var flaskResponse = await client.PostAsync(_uploadUrl, content);

                    if (!flaskResponse.IsSuccessStatusCode)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    user.FotoName = fotoFileName;
                    user.FotoPath = $"/FotoKaryawan/{fotoFileName}";
                    user.UpdateDateTime = DateTimeOffset.UtcNow;
                    user.UpdateBy = userActiveLoginId;
                }

                _applicationDbContext.UserActives.Update(user);

                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    data = new
                    {
                        user.UserActiveId,
                        KaryawanId = user.UserActiveId,
                        user.UserActiveCode,
                        user.FullName,
                        user.Email,
                        user.IdentityNumber,
                        user.NoKaryawan,
                        user.KodeKaryawan,
                        user.IsKaryawanMedis,
                        user.FotoName,
                        user.FotoPath,
                        user.StatusPerkawinan,
                        user.AgamaId,
                        user.PendidikanTerakhirId,
                        user.AlamatDomisili,
                        user.ProvinsiId,
                        user.KotaId,
                        user.KecId,
                        user.KewarganegaraanId,
                        user.StatusKewarganegaraan,
                        user.UpdateDateTime
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }
        //public async Task<IActionResult> UpdateUserActive(Guid id, [FromForm] UpdateUserActiveViewModel vm)
        //{
        //    if (vm == null)
        //        return BadRequest(new { message = "Data tidak valid." });

        //    try
        //    {
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });

        //        var loginUserActive = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.Email == emailLogin);

        //        if (loginUserActive == null)
        //            return Unauthorized(new { message = "Data user login tidak ditemukan!" });

        //        var userActiveLoginId = loginUserActive.UserActiveId;

        //        var user = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.UserActiveId == id);

        //        if (user == null)
        //            return NotFound(new { message = "Data UserActive tidak ditemukan." });

        //        if (!DateTime.TryParseExact(
        //                vm.DateOfBirth,
        //                "yyyy-MM-dd",
        //                CultureInfo.InvariantCulture,
        //                DateTimeStyles.None,
        //                out DateTime parsedDate))
        //        {
        //            return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
        //        }

        //        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //        var isDuplicateEmail = await _applicationDbContext.UserActives
        //            .AnyAsync(x => x.Email == vm.Email && x.UserActiveId != id);

        //        if (isDuplicateEmail)
        //            return Conflict(new { message = "Email sudah dipakai oleh user lain! || 409 Conflict" });

        //        var tipeUser = await _applicationDbContext.TipeUsers
        //            .FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);

        //        var isDokter = tipeUser?.NamaTipeUser?.Trim().ToLower() == "dokter";

        //        using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

        //        // =========================
        //        // UPDATE ASP.NET IDENTITY
        //        // =========================
        //        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        //        if (identityUser == null)
        //            return BadRequest(new { message = "User login tidak ditemukan." });

        //        if (!string.Equals(identityUser.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        //        {
        //            identityUser.Email = vm.Email;
        //            identityUser.UserName = vm.Email;
        //        }

        //        if (identityUser is ApplicationUser appUser)
        //        {
        //            appUser.NamaUser = vm.FullName;
        //            appUser.PhoneNumber = vm.Handphone;
        //            appUser.IsActive = true;
        //        }

        //        var resultUpdateIdentity = await _userManager.UpdateAsync(identityUser);
        //        if (!resultUpdateIdentity.Succeeded)
        //        {
        //            var err = string.Join(", ", resultUpdateIdentity.Errors.Select(e => e.Description));
        //            await transaction.RollbackAsync();
        //            return BadRequest(new { message = $"Gagal update user login: {err}" });
        //        }

        //        // =========================
        //        // UPDATE USERACTIVE
        //        // =========================
        //        user.FullName = vm.FullName;
        //        user.IdentityNumber = vm.IdentityNumber;
        //        user.PlaceOfBirth = vm.PlaceOfBirth;
        //        user.DateOfBirth = parsedDate;
        //        user.Gender = vm.Gender;
        //        user.Address = vm.Address;
        //        user.Handphone = vm.Handphone;
        //        user.Email = vm.Email;
        //        user.DepartemenId = vm.DepartemenId;
        //        user.PositionId = vm.PositionId;
        //        user.TipeUserId = vm.TipeUserId;
        //        user.InstalasiUnitId = vm.InstalasiUnitId;
        //        user.NoSTR = vm.NoSTR;
        //        user.StatusPegawai = vm.StatusPegawai;
        //        user.IsActive = true;

        //        user.NoIdentitas = vm.IdentityNumber;
        //        user.Alamat = vm.Address;
        //        user.NoHandphone = vm.Handphone;
        //        user.UpdateDateTime = DateTimeOffset.UtcNow;
        //        user.UpdateBy = userActiveLoginId;

        //        user.PinPegawai = DelegasiVerifikasi.ComputeSha256Hash(
        //            GeneratePinPegawai(parsedDate)
        //        );

        //        // =========================
        //        // SYNC DOKTER
        //        // =========================
        //        var dokterExisting = await _applicationDbContext.Dokters
        //            .Where(d => d.UserActiveId == user.UserActiveId)
        //            .OrderBy(d => d.IsDelete)
        //            .ThenByDescending(d => d.CreateDateTime)
        //            .FirstOrDefaultAsync();

        //        string kdDokter = dokterExisting?.KdDokter ?? "";
        //        string noKaryawan = user.NoKaryawan ?? "";

        //        if (isDokter)
        //        {
        //            if (dokterExisting == null)
        //            {
        //                var dateNow = DateTime.UtcNow;
        //                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

        //                var lastDr = await _applicationDbContext.Dokters
        //                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //                    .OrderByDescending(k => k.KdDokter)
        //                    .FirstOrDefaultAsync();

        //                if (lastDr == null)
        //                {
        //                    kdDokter = "DKR" + setDateNow + "0001";
        //                }
        //                else
        //                {
        //                    var lastTrim = lastDr.KdDokter.Substring(3, 6);
        //                    if (lastTrim != setDateNow)
        //                        kdDokter = "DKR" + setDateNow + "0001";
        //                    else
        //                        kdDokter = "DKR" + setDateNow +
        //                                   (Convert.ToInt32(lastDr.KdDokter.Substring(9)) + 1).ToString("D4");
        //                }

        //                var dokterBaru = new Dokter
        //                {
        //                    DokterId = Guid.NewGuid(),
        //                    KdDokter = kdDokter,
        //                    NmDokter = vm.FullName,
        //                    Email = vm.Email,
        //                    Nohp = vm.Handphone,
        //                    Nik = vm.IdentityNumber,
        //                    Alamat = vm.Address,
        //                    UserActiveId = user.UserActiveId,
        //                    CreateDateTime = DateTimeOffset.UtcNow,
        //                    CreateBy = userActiveLoginId,
        //                    IsActive = true,
        //                    IsDelete = false
        //                };

        //                _applicationDbContext.Dokters.Add(dokterBaru);
        //            }
        //            else
        //            {
        //                kdDokter = dokterExisting.KdDokter;

        //                dokterExisting.NmDokter = vm.FullName;
        //                dokterExisting.Email = vm.Email;
        //                dokterExisting.Nohp = vm.Handphone;
        //                dokterExisting.Nik = vm.IdentityNumber;
        //                dokterExisting.Alamat = vm.Address;
        //                dokterExisting.IsActive = true;
        //                dokterExisting.IsDelete = false;
        //                dokterExisting.UpdateDateTime = DateTimeOffset.UtcNow;
        //                dokterExisting.UpdateBy = userActiveLoginId;

        //                _applicationDbContext.Dokters.Update(dokterExisting);
        //            }
        //        }
        //        else
        //        {
        //            if (string.IsNullOrWhiteSpace(user.NoKaryawan))
        //            {
        //                var dateNow = DateTime.UtcNow;
        //                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

        //                var lastKaryawan = await _applicationDbContext.UserActives
        //                    .Where(k => k.CreateDateTime.Date == dateNow.Date)
        //                    .OrderByDescending(k => k.NoKaryawan)
        //                    .FirstOrDefaultAsync();

        //                if (lastKaryawan == null || string.IsNullOrEmpty(lastKaryawan.NoKaryawan))
        //                {
        //                    noKaryawan = "KRY" + setDateNow + "0001";
        //                }
        //                else
        //                {
        //                    var lastTrim = lastKaryawan.NoKaryawan.Substring(3, 6);
        //                    if (lastTrim != setDateNow)
        //                        noKaryawan = "KRY" + setDateNow + "0001";
        //                    else
        //                        noKaryawan = "KRY" + setDateNow +
        //                                     (Convert.ToInt32(lastKaryawan.NoKaryawan.Substring(9)) + 1).ToString("D4");
        //                }

        //                user.NoKaryawan = noKaryawan;
        //            }
        //            else
        //            {
        //                noKaryawan = user.NoKaryawan;
        //            }

        //            if (dokterExisting != null && dokterExisting.IsDelete == false)
        //            {
        //                dokterExisting.IsDelete = true;
        //                dokterExisting.IsActive = false;
        //                dokterExisting.UpdateDateTime = DateTimeOffset.UtcNow;
        //                dokterExisting.UpdateBy = userActiveLoginId;

        //                _applicationDbContext.Dokters.Update(dokterExisting);
        //            }
        //        }

        //        // =========================
        //        // UPLOAD FOTO
        //        // =========================
        //        if (vm.Foto != null && vm.Foto.Length > 0)
        //        {
        //            var maxSize = 2 * 1024 * 1024;
        //            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        //            var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

        //            if (vm.Foto.Length > maxSize)
        //                return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });

        //            if (!allowedExtensions.Contains(fileExtension))
        //                return BadRequest(new { message = "Format file tidak valid! Gunakan JPG, JPEG, atau PNG." });

        //            string namaDasarFoto;
        //            if (isDokter)
        //            {
        //                namaDasarFoto = !string.IsNullOrWhiteSpace(kdDokter)
        //                    ? kdDokter
        //                    : user.UserActiveCode;
        //            }
        //            else
        //            {
        //                namaDasarFoto = !string.IsNullOrWhiteSpace(noKaryawan)
        //                    ? noKaryawan
        //                    : user.UserActiveCode;
        //            }

        //            var fotoFileName = $"{namaDasarFoto}{fileExtension}";
        //            var oldFileName = user.FotoName ?? "";

        //            using var client = new HttpClient();
        //            using var ms = new MemoryStream();
        //            await vm.Foto.CopyToAsync(ms);
        //            ms.Position = 0;

        //            var content = new MultipartFormDataContent
        //            {
        //                {
        //                    new StreamContent(ms)
        //                    {
        //                        Headers =
        //                        {
        //                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType)
        //                        }
        //                    },
        //                    "file",
        //                    fotoFileName
        //                },
        //                { new StringContent("FotoKaryawan"), "folderTarget" },
        //                { new StringContent(oldFileName), "oldFileName" }
        //            };

        //            var flaskResponse = await client.PostAsync(_uploadUrl, content);
        //            if (!flaskResponse.IsSuccessStatusCode)
        //            {
        //                await transaction.RollbackAsync();
        //                return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
        //            }

        //            user.FotoName = fotoFileName;
        //            user.FotoPath = $"/FotoKaryawan/{fotoFileName}";
        //            user.UpdateDateTime = DateTimeOffset.UtcNow;
        //            user.UpdateBy = userActiveLoginId;

        //        }

        //        _applicationDbContext.UserActives.Update(user);

        //        await _applicationDbContext.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new { message = "Update Data Berhasil || 200 OK" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}


        [HttpPost("UbahPassword")]
        public async Task<IActionResult> UbahPassword(ResetPasswordViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari user 
                var user = await _userManager.FindByEmailAsync(EmailLogin);
                if (user == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }

                // Generate token dan ubah password langsung
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Gagal mengubah password.", errors = result.Errors.Select(e => e.Description) });
                }

                return Ok(new { message = "Password berhasil diubah." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                // cari user 
                var user = await _userManager.FindByEmailAsync(EmailLogin);
                if (user == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }
                // cari data user active
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                var NewPassword = data.DateOfBirth.ToString("ddMMMyyyy");

                // Generate token dan ubah password langsung
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Gagal mengubah password.", errors = result.Errors.Select(e => e.Description) });
                }
                return Ok(new { message = "Reset Password Berhasil" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("UbahPassword/{userActiveId}")]
        public async Task<IActionResult> UbahPasswordById(Guid userActiveId, [FromBody] UbahPasswordViewModel vm)
        {
            if (userActiveId == Guid.Empty)
            {
                return BadRequest(new { message = "UserActiveId tidak boleh kosong." });
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                return BadRequest(new { message = "Password baru harus diisi." });
            }

            try
            {
                // 🔍 Cari user aktif di tabel UserActives
                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.UserActiveId == userActiveId && (u.IsDelete == false || u.IsDelete == null));

                if (getUserActive == null)
                {
                    return NotFound(new { message = "User aktif tidak ditemukan." });
                }

                // 🔍 Cari user di ASP.NET Identity
                var user = await _userManager.FindByEmailAsync(getUserActive.Email);
                if (user == null)
                {
                    return NotFound(new { message = "User identity tidak ditemukan untuk email terkait." });
                }

                // 🔑 Generate token dan ubah password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, vm.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Gagal mengubah password.",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // 🔄 Update waktu perubahan password
                getUserActive.UpdateDateTime = DateTimeOffset.UtcNow;
                _applicationDbContext.UserActives.Update(getUserActive);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Password berhasil diubah.",
                    user = new
                    {
                        getUserActive.UserActiveId,
                        getUserActive.FullName,
                        getUserActive.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("UploadFotoUser/{id}")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UploadFotoUser(Guid id, [FromForm] UploadFotoKaryawanViewModel vm)
        {
            if (vm == null || vm.FotoKaryawan == null || vm.FotoKaryawan.Length == 0)
            {
                return BadRequest(new { message = "File foto karyawan tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var data = await _applicationDbContext.UserActives.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data karyawan tidak ditemukan." });
                }

                var fileName = "";

                // ✅ Proses upload file TTD
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var allowedExt = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    if (file.Length > 5 * 1024 * 1024)
                        throw new Exception($"{prefix} maksimal 5MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

                    fileName = $"KRY{Guid.NewGuid().ToString("N").Substring(0, 8)}_{safeTime}{ext}";

                    // 👉 Sesuaikan nama folder dengan kebutuhan kamu
                    var folderTarget = "FotoKaryawan";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "image/jpeg"
                        : file.ContentType;

                    var fileContent = new StreamContent(ms);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", fileName);
                    form.Add(new StringContent(folderTarget), "folderTarget");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(_uploadUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke Flask.");

                    // ⚠ Di sini kita pakai pola yang sama seperti UpdatePenandaan:
                    //     tidak baca JSON dari Flask, tapi pakai path lokal yang sudah dibentuk
                    return filePath;
                }


                // Upload file → folder TTDUser
                var path = await UploadToFlaskAsync(vm.FotoKaryawan, "FotoKaryawan");

                // ✅ Update PraOperasi
                data.FotoPath = path;
                data.FotoName = fileName;

                _applicationDbContext.UserActives.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Foto Karyawan berhasil diupload", path, karyawanId = data.UserActiveId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
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

                // **Cari Data User**
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Hapus user login dari tabel AspNetUsers (permanen)
                var userLogin = await _userManager.FindByEmailAsync(data.Email);
                if (userLogin != null)
                {
                    var result = await _userManager.DeleteAsync(userLogin);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
                    }
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;
                data.IsActive = false;

                _applicationDbContext.UserActives.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                // Tangani error jika ada masalah
                return StatusCode(500, $"Terjadi kesalahan saat menghapus data: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedUserActive(
        int page = 1,
        int perPage = 10,
        Guid? id = null,
        string? search = null,
        string? email = null,
        string? pathTTD = null,
        string? namaDept = null,
        string? namaPosisi = null,
        string? namaTipe = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
        [FromQuery] EnumJenisUser? tipeUser = null)
        {
            // Query data
            var query =
                (from a in _applicationDbContext.UserActives.AsNoTracking()

                 join t in _applicationDbContext.TipeUsers.AsNoTracking()
                     on a.TipeUserId equals t.TipeUserId into tipeJoin
                 from tipe in tipeJoin.DefaultIfEmpty()

                 join dept in _applicationDbContext.Departements.AsNoTracking()
                     on a.DepartemenId equals dept.DepartementId into deptJoin
                 from dept in deptJoin.DefaultIfEmpty()

                 join pos in _applicationDbContext.Positions.AsNoTracking()
                     on a.PositionId equals pos.PositionId into posJoin
                 from pos in posJoin.DefaultIfEmpty()

                 join creator in _applicationDbContext.UserActives.AsNoTracking()
                     on a.CreateBy equals creator.UserActiveId into creatorJoin
                 from creator in creatorJoin.DefaultIfEmpty()

                 join dok in _applicationDbContext.Dokters.AsNoTracking().Where(x => x.IsDelete == false)
                     on a.UserActiveId equals dok.UserActiveId into dokJoin
                 from dok in dokJoin.DefaultIfEmpty()

                 join td in _applicationDbContext.MasterTTDs.AsNoTracking()
                    on a.UserActiveId equals td.UserActiveId into tdJoin
                 from td in tdJoin.DefaultIfEmpty()
                 where a.IsDelete == false


                 join agama in _applicationDbContext.Agamas.AsNoTracking()
                     on a.AgamaId equals agama.AgamaId into agamaJoin
                 from agama in agamaJoin.DefaultIfEmpty()

                 join pendidikan in _applicationDbContext.Pendidikans.AsNoTracking()
                     on a.PendidikanTerakhirId equals pendidikan.PendidikanId into pendidikanJoin
                 from pendidikan in pendidikanJoin.DefaultIfEmpty()

                 join provinsi in _applicationDbContext.Provinsis.AsNoTracking()
                     on a.ProvinsiId equals provinsi.ProvinsiId into provinsiJoin
                 from provinsi in provinsiJoin.DefaultIfEmpty()

                 join kota in _applicationDbContext.KabupatenKotas.AsNoTracking()
                     on a.KotaId equals kota.KabupatenKotaId into kotaJoin
                 from kota in kotaJoin.DefaultIfEmpty()

                 join kec in _applicationDbContext.Kecamatans.AsNoTracking()
                     on a.KecId equals kec.KecamatanId into kecJoin
                 from kec in kecJoin.DefaultIfEmpty()

                 join kewarganegaraan in _applicationDbContext.Negaras.AsNoTracking()
                     on a.KewarganegaraanId equals kewarganegaraan.NegaraId into kewarganegaraanJoin
                 from kewarganegaraan in kewarganegaraanJoin.DefaultIfEmpty()

                 orderby a.CreateDateTime descending

                 select new
                 {
                     a.CreateDateTime,
                     a.CreateBy,
                     CreateByName = creator != null ? creator.FullName : "-",

                     a.UserActiveId,
                     a.UserActiveCode,
                     a.FullName,
                     a.Gender,
                     a.Email,

                     a.TipeUserId,
                     NamaTipeUser = tipe != null ? tipe.NamaTipeUser : "-",

                     a.DepartemenId,
                     NamaDepartemen = dept != null ? dept.NamaDepartement : "-",

                     a.PositionId,
                     NamaPosisi = pos != null ? pos.PositionName : "-",

                     a.IdentityNumber,
                     a.PlaceOfBirth,
                     a.DateOfBirth,
                     a.Address,
                     a.Handphone,
                     a.StatusPegawai,
                     a.NoSTR,

                     FotoName = a != null ? a.FotoName : null,
                     FotoPath = a != null ? a.FotoPath : null,

                     TTDId = td != null ? td.TTDId : (Guid?)null,
                     TTDPath = td != null ? td.TTDPath : null,

                     // id
                     DokterId = dok != null ? dok.DokterId : (Guid?)null,
                     KaryawanId = a != null ? a.UserActiveId : (Guid?)null,

                     a.IsKaryawanMedis,

                     // =========================
                     // DATA TAMBAHAN
                     // =========================
                     a.StatusPerkawinan,

                     a.AgamaId,
                     NamaAgama = agama != null ? agama.NamaAgama : "-",

                     a.PendidikanTerakhirId,
                     NamaPendidikanTerakhir = pendidikan != null ? pendidikan.NamaPendidikan : "-",

                     a.AlamatDomisili,

                     a.ProvinsiId,
                     NamaProvinsi = provinsi != null ? provinsi.NamaProvinsi : "-",

                     a.KotaId,
                     NamaKota = kota != null ? kota.NamaKabupatenKota : "-",
                     a.KecId,
                     NamaKecamatan = kec != null ? kec.NamaKecamatan : "-",

                     a.KewarganegaraanId,
                     NamaKewarganegaraan = kewarganegaraan != null ? kewarganegaraan.NamaNegara : "-",

                     a.StatusKewarganegaraan
                 });

            // filter based on user active id
            if (id.HasValue)
            {
                query = query.Where(u => u.UserActiveId == id.Value);
            }

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, search) ||
                    EF.Functions.ILike(u.Email, search) ||
                    EF.Functions.ILike(u.CreateByName, search)
                );
            }

            // Filter berdasarkan email
            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.ToLower();
                query = query.Where(u => u.Email.ToLower() == email);
            }

            if (!string.IsNullOrWhiteSpace(pathTTD))
            {
                pathTTD = $"%{pathTTD.ToLower()}%";
                query = query.Where(u => EF.Functions.ILike(u.TTDPath, pathTTD));
            }

            /// filter nama dept
            if (!string.IsNullOrWhiteSpace(namaDept))
            {
                namaDept = $"%{namaDept.ToLower()}%";
                query = query.Where(u => EF.Functions.ILike(u.NamaDepartemen, namaDept));
            }

            /// filter nama posisi
            if (!string.IsNullOrWhiteSpace(namaPosisi))
            {
                namaPosisi = $"%{namaPosisi.ToLower()}%";
                query = query.Where(u => EF.Functions.ILike(u.NamaPosisi, namaPosisi));
            }

            /// filter nama tipe user
            if (!string.IsNullOrWhiteSpace(namaTipe))
            {
                namaTipe = $"%{namaTipe.ToLower()}%";
                query = query.Where(u => EF.Functions.ILike(u.NamaTipeUser, namaTipe));
            }


            // === Filter Tipe User (enum -> Display(Name)) ===
            if (tipeUser.HasValue)
            {
                var memberInfo = typeof(EnumJenisUser)
                    .GetMember(tipeUser.Value.ToString())
                    .FirstOrDefault();

                var displayAttr = memberInfo?
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .Cast<DisplayAttribute>()
                    .FirstOrDefault();

                // Jika tidak ada Display, fallback ke nama enum
                string displayName = displayAttr?.Name ?? tipeUser.Value.ToString();

                // bandingkan ke kolom string di DB (terjemahan ke SQL aman)
                query = query.Where(u => u.NamaTipeUser == displayName);
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
                    "UserActiveCode" => query.OrderByDescending(u => u.UserActiveCode),
                    "FullName" => query.OrderByDescending(u => u.FullName),
                    "TipeUser" => query.OrderByDescending(u => u.NamaTipeUser),

                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "UserActiveCode" => query.OrderBy(u => u.UserActiveCode),
                    "FullName" => query.OrderBy(u => u.FullName),
                    "TipeUser" => query.OrderBy(u => u.NamaTipeUser),
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

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(Guid positionId, string pendaftaranPasienBaruId)
        {
            var newRolePositions = new List<IdentityUserRole<string>>();

            var existingRoles = _applicationDbContext.UserRoles
            .Where(ur => ur.UserId == pendaftaranPasienBaruId.ToString())
            .ToList();

            // Ambil RoleId yang terkait dengan PositionId dari RolePositions
            var rolePosition = await _applicationDbContext.RolePositions
                .Where(r => r.PositionId == positionId.ToString())
                .Select(r => r.RoleId)
                .ToListAsync();

            foreach (var roleId in rolePosition)
            {
                // Cek apakah role-position sudah ada di IdentityUserRole
                var exists = await _applicationDbContext.UserRoles
                    .AnyAsync(ur => ur.RoleId == roleId && ur.UserId == pendaftaranPasienBaruId.ToString());

                if (!exists)
                {
                    // Jika belum ada, tambahkan role ke IdentityUserRole
                    newRolePositions.Add(new IdentityUserRole<string>
                    {
                        UserId = pendaftaranPasienBaruId.ToString(), // User yang terdaftar (gunakan string karena IdentityUserRole menggunakan string)
                        RoleId = roleId.ToString() // Role yang terkait dengan PositionId
                    });
                }
            }

            // Menambahkan data ke IdentityUserRole
            if (newRolePositions.Any())
            {
                _applicationDbContext.UserRoles.AddRangeAsync(newRolePositions);
                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Role untuk posisi berhasil ditambahkan ke user." });
            }

            return BadRequest(new { message = "Tidak ada role baru yang perlu ditambahkan." });
        }

    }
}
