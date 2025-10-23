using System.Diagnostics;
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
    [EnableCors("AllowSpecific")]
    public class SOAPController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<SOAPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<SOAPHub> _hubContext;

        public SOAPController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SOAPController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<SOAPHub> hubContext
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
        public async Task<IActionResult> GetAlLSOAP(int page = 1, int perPage = 10)
        {
            try
            {
                // Normalisasi parameter
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =========================
                // 1) Ambil & rapikan kamus ICD (Kode -> Nama) aman dari duplikat
                // =========================
                var icdRows = await _applicationDbContext.ICD10s
                    .AsNoTracking()
                    .Select(x => new { x.ICDCode, x.ICDName })
                    .ToListAsync();

                // Normalisasi + dedup di memory: Trim + case-insensitive
                var icdDict = icdRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.ICDCode))
                    .Select(x => new { Code = x.ICDCode.Trim(), x.ICDName })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,                 // key sudah unik setelah group
                        g => g.First().ICDName,     // ambil entry pertama (ubah aturan jika perlu)
                        StringComparer.OrdinalIgnoreCase // lookup jadi case-insensitive
                    );

                // ambil kamus SDKI
                var sdkiRows = await _applicationDbContext.SDKIDiagnosas
                    .AsNoTracking()
                    .Select(x => new { x.SDKIKodeDiagnosa, x.NamaDiagnosa })
                    .ToListAsync();

                var sdkiDict = sdkiRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.SDKIKodeDiagnosa))
                    .Select(x => new { Code = x.SDKIKodeDiagnosa.Trim(), x.NamaDiagnosa })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().NamaDiagnosa,
                        StringComparer.OrdinalIgnoreCase
                    );


                // =========================
                // 2) Query utama (tanpa parsing ICD di SQL)
                //    - Hitung totalRows di DB
                //    - Ambil data halaman tertentu saja dari DB
                // =========================
                var baseQuery =
                    from a in _applicationDbContext.SOAPs
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                    join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                    join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                    join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                    where a.IsDelete == false || a.IsDelete == null
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        a.SOAPID,
                        a.KunjunganId,
                        PasienId = k.PasienId,
                        a.Subjective,
                        a.Objective,
                        a.DaftarICD10, // CSV
                        a.DaftarSDKI,
                        a.Assessment,
                        a.Planning,
                        a.Evaluasi,
                        a.Intervensi,
                        a.Reevaluasi,
                        NamaProfesi = string.Equals(a.Profesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.Profesi,
                        NamaDokter = d.NmDokter,
                        DokterId = d.DokterId,
                        NamaPasien = p.NamaLengkap
                    };

                // Hitung total rows di DB (sesuai join/filter)
                var totalRows = await baseQuery.CountAsync();

                // Hitung total halaman
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                // Ambil hanya halaman yang diminta (paging di DB)
                var rawPage = await baseQuery
                    .AsNoTracking()
                    .OrderByDescending(a => a.CreateDateTime)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (rawPage.Count == 0)
                {
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
                }

                // =========================
                // 3) Proyeksikan + parsing ICD di memory
                // =========================
                var listdata = rawPage
                    .Select(a =>
                    {
                        // Split CSV kode ICD
                        var codes = (a.DaftarICD10 ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        // Map ke nama ICD (lookup case-insensitive)
                        var namaIcd = codes
                            .Select(code =>
                                icdDict.TryGetValue(code, out var nama)
                                    ? nama
                                    : $"(Kode tidak ditemukan: {code})")
                            .ToList();

                        // Split CSV kode SDKI
                        var sdkiCodes = (a.DaftarSDKI ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        // Map ke nama SDKI (lookup case-insensitive)
                        var namaSdki = sdkiCodes
                            .Select(code =>
                                sdkiDict.TryGetValue(code, out var nama)
                                    ? nama
                                    : $"(Kode tidak ditemukan: {code})")
                            .ToList();

                        return new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            a.CreateByName,
                            a.SOAPID,
                            a.KunjunganId,
                            a.PasienId,
                            a.Subjective,
                            a.Objective,
                            DaftarICD10 = codes,   
                            NamaICD = namaIcd,     
                            DaftarSDKI = sdkiCodes, 
                            NamaSDKI = namaSdki,   
                            a.Assessment,
                            a.Planning,
                            a.Evaluasi,
                            a.Intervensi,
                            a.Reevaluasi,
                            NamaProfesi = string.Equals(a.NamaProfesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.NamaProfesi,
                            a.NamaDokter,
                            a.DokterId,
                            a.NamaPasien
                        };
                    })
                    .ToList();

                // =========================
                // 4) Return hasil
                // =========================
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
            catch (Exception ex)
            {
                // Logging ex di sini jika ada logger
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> GetAlLSOAP(int page = 1, int perPage = 10)
        //{
        //    // Normalisasi parameter
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // 1) Ambil kamus ICD (Kode -> Nama) sekali saja
        //    var icdDict = await _applicationDbContext.ICD10s
        //        .AsNoTracking()
        //        .Select(x => new { x.ICDCode, x.ICDName })
        //        .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

        //    // 2) Ambil data utama (tanpa Split di server, supaya tetap bisa dieksekusi oleh EF/SQL)
        //    var raw = await (from a in _applicationDbContext.SOAPs
        //                     join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
        //                     join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
        //                     join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //                     join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
        //                     where a.IsDelete == false
        //                     select new
        //                     {
        //                         a.CreateDateTime,
        //                         a.CreateBy,
        //                         CreateByName = u.FullName,
        //                         a.SOAPID,
        //                         a.KunjunganId,
        //                         PasienId = k.PasienId,
        //                         a.Subjective,
        //                         a.Objective,
        //                         a.DaftarICD10, // simpan apa adanya dulu (CSV)
        //                         a.Assessment,
        //                         a.Planning,
        //                         a.Evaluasi,
        //                         a.Intervensi,
        //                         a.Reevaluasi,
        //                         a.Profesi,
        //                         NamaDokter = d.NmDokter,
        //                         DokterId = d.DokterId,
        //                         NamaPasien = p.NamaLengkap
        //                     })
        //                    .AsNoTracking()
        //                    .ToListAsync();

        //    var projected = raw
        //        .Select(a =>
        //        {
        //            var codes = (a.DaftarICD10 ?? "")
        //                .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                .Select(s => s.Trim())
        //                .Where(s => !string.IsNullOrWhiteSpace(s))
        //                .Distinct(StringComparer.OrdinalIgnoreCase)
        //                .ToList();

        //            var namaIcd = codes
        //                .Select(code =>
        //                    icdDict.TryGetValue(code, out var nama)
        //                        ? nama
        //                        : $"(Kode tidak ditemukan: {code})")
        //                .ToList();

        //            return new
        //            {
        //                a.CreateDateTime,
        //                a.CreateBy,
        //                a.CreateByName,
        //                a.SOAPID,
        //                a.KunjunganId,
        //                a.PasienId,
        //                a.Subjective,
        //                a.Objective,
        //                DaftarICD10 = codes,   // list kode ICD
        //                NamaICD = namaIcd,     // list nama ICD hasil "join"
        //                a.Assessment,
        //                a.Planning,
        //                a.Evaluasi,
        //                a.Intervensi,
        //                a.Reevaluasi,
        //                a.Profesi,
        //                a.NamaDokter,
        //                a.DokterId,
        //                a.NamaPasien
        //            };
        //        })
        //        .OrderByDescending(a => a.CreateDateTime)
        //        .ToList();

        //    // 4) Paging di sisi memory (karena sudah perlu materialize untuk parsing ICD)
        //    var totalRows = projected.Count;
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    var listdata = projected
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToList();

        //    if (!listdata.Any())
        //    {
        //        return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
        //    }

        //    // 5) Return hasil
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
        public async Task<IActionResult> GetSOAPById(Guid id)
        {
            try
            {
                // =========================
                // 1) Ambil & rapikan kamus ICD (Kode -> Nama) aman dari duplikat
                // =========================
                var icdRows = await _applicationDbContext.ICD10s
                    .AsNoTracking()
                    .Select(x => new { x.ICDCode, x.ICDName })
                    .ToListAsync();

                var icdDict = icdRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.ICDCode))
                    .Select(x => new { Code = x.ICDCode.Trim(), x.ICDName })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,                 // key unik setelah group
                        g => g.First().ICDName,     // ambil salah satu (atur sesuai kebutuhan)
                        StringComparer.OrdinalIgnoreCase
                    );

                // =========================
                // 1b) Ambil kamus SDKI (Kode -> Nama) aman dari duplikat
                // =========================
                var sdkiRows = await _applicationDbContext.SDKIDiagnosas
                    .AsNoTracking()
                    .Select(x => new { x.SDKIKodeDiagnosa, x.NamaDiagnosa })
                    .ToListAsync();

                var sdkiDict = sdkiRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.SDKIKodeDiagnosa))
                    .Select(x => new { Code = x.SDKIKodeDiagnosa.Trim(), x.NamaDiagnosa })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().NamaDiagnosa,
                        StringComparer.OrdinalIgnoreCase
                    );

                // =========================
                // 2) Query utama: 1 record by SOAPID
                // =========================
                var data = await (
                    from a in _applicationDbContext.SOAPs
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                    join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                    join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                    join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                    where a.IsDelete == false && a.SOAPID == id
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        a.SOAPID,
                        a.KunjunganId,
                        PasienId = k.PasienId,
                        a.Subjective,
                        a.Objective,
                        a.DaftarICD10, 
                        a.DaftarSDKI,  
                        a.Assessment,
                        a.Planning,
                        a.Evaluasi,
                        a.Intervensi,
                        a.Reevaluasi,
                        NamaProfesi = string.Equals(a.Profesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.Profesi,
                        NamaDokter = d.NmDokter,
                        DokterId = d.DokterId,
                        NamaPasien = p.NamaLengkap
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
                }

                // =========================
                // 3) Proyeksi + parsing ICD/SDKI di memory (1 record)
                // =========================
                // ICD
                var icdCodes = (data.DaftarICD10 ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var icdNames = icdCodes
                    .Select(code => icdDict.TryGetValue(code, out var nama)
                        ? nama
                        : $"(Kode tidak ditemukan: {code})")
                    .ToList();

                // SDKI
                var sdkiCodes = (data.DaftarSDKI ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var sdkiNames = sdkiCodes
                    .Select(code => sdkiDict.TryGetValue(code, out var nama)
                        ? nama
                        : $"(Kode tidak ditemukan: {code})")
                    .ToList();

                var result = new
                {
                    data.CreateDateTime,
                    data.CreateBy,
                    data.CreateByName,
                    data.SOAPID,
                    data.KunjunganId,
                    data.PasienId,
                    data.Subjective,
                    data.Objective,
                    DaftarICD10 = icdCodes,   
                    NamaICD = icdNames,       
                    DaftarSDKI = sdkiCodes,   
                    NamaSDKI = sdkiNames,     
                    data.Assessment,
                    data.Planning,
                    data.Evaluasi,
                    data.Intervensi,
                    data.Reevaluasi,
                    data.NamaProfesi,
                    data.NamaDokter,
                    data.DokterId,
                    data.NamaPasien
                };

                // =========================
                // 4) Return hasil
                // =========================
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result
                });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }

        //{
        //    // 1) Ambil kamus ICD (Kode -> Nama) sekali saja
        //    var icdDict = await _applicationDbContext.ICD10s
        //        .AsNoTracking()
        //        .Select(x => new { x.ICDCode, x.ICDName })
        //        .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

        //    // 2) Ambil data utama berdasarkan SOAPID
        //    var raw = await (from a in _applicationDbContext.SOAPs
        //                     join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
        //                     join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
        //                     join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //                     join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
        //                     where a.SOAPID == id && a.IsDelete == false // Filter berdasarkan SOAPID
        //                     select new
        //                     {
        //                         a.CreateDateTime,
        //                         a.CreateBy,
        //                         CreateByName = u.FullName,
        //                         a.SOAPID,
        //                         a.KunjunganId,
        //                         PasienId = k.PasienId,
        //                         a.Subjective,
        //                         a.Objective,
        //                         a.DaftarICD10, // simpan apa adanya dulu (CSV)
        //                         a.Assessment,
        //                         a.Planning,
        //                         a.Evaluasi,
        //                         a.Intervensi,
        //                         a.Reevaluasi,
        //                         a.Profesi,
        //                         NamaDokter = d.NmDokter,
        //                         DokterId = d.DokterId,
        //                         NamaPasien = p.NamaLengkap
        //                     })
        //                     .AsNoTracking()
        //                     .FirstOrDefaultAsync(); // Mengambil satu data

        //    // 3) Jika data tidak ditemukan, kembalikan 404 Not Found
        //    if (raw == null)
        //    {
        //        return NotFound(new { message = "Data tidak ditemukan" });
        //    }

        //    // 4) Proses ICD (parsing dan lookup)
        //    var codes = (raw.DaftarICD10 ?? "")
        //        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //        .Select(s => s.Trim())
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Distinct(StringComparer.OrdinalIgnoreCase)
        //        .ToList();

        //    var namaIcd = codes
        //        .Select(code =>
        //        {
        //            icdDict.TryGetValue(code, out var nama);
        //            return nama ?? $"(Kode tidak ditemukan: {code})";
        //        })
        //        .ToList();

        //    // 5) Proyeksi hasil akhir
        //    var resultData = new
        //    {
        //        raw.CreateDateTime,
        //        raw.CreateBy,
        //        raw.CreateByName,
        //        raw.SOAPID,
        //        raw.KunjunganId,
        //        raw.PasienId,
        //        raw.Subjective,
        //        raw.Objective,
        //        DaftarICD10 = codes,    // list kode ICD
        //        NamaICD = namaIcd,      // list nama ICD hasil "join"
        //        raw.Assessment,
        //        raw.Planning,
        //        raw.Evaluasi,
        //        raw.Intervensi,
        //        raw.Reevaluasi,
        //        raw.Profesi,
        //        raw.NamaDokter,
        //        raw.DokterId,
        //        raw.NamaPasien
        //    };

        //    // 6) Return hasil
        //    return Ok(new
        //    {
        //        message = "Berhasil || 200 OK",
        //        data = resultData
        //    });
        //}

        [HttpGet("kunjungan/{kunjunganid}")]
        public async Task<IActionResult> GetAllByKunjunganId(Guid kunjunganid)
        {
            try
            {
                // =========================
                // 1) Ambil & rapikan kamus ICD (Kode -> Nama) aman dari duplikat
                // =========================
                var icdRows = await _applicationDbContext.ICD10s
                    .AsNoTracking()
                    .Select(x => new { x.ICDCode, x.ICDName })
                    .ToListAsync();

                var icdDict = icdRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.ICDCode))
                    .Select(x => new { Code = x.ICDCode.Trim(), x.ICDName })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().ICDName,
                        StringComparer.OrdinalIgnoreCase
                    );

                var sdkiRows = await _applicationDbContext.SDKIDiagnosas
                    .AsNoTracking()
                    .Select(x => new { x.SDKIKodeDiagnosa, x.NamaDiagnosa })
                    .ToListAsync();

                var sdkiDict = sdkiRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.SDKIKodeDiagnosa))
                    .Select(x => new { Code = x.SDKIKodeDiagnosa.Trim(), x.NamaDiagnosa })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().NamaDiagnosa,
                        StringComparer.OrdinalIgnoreCase
                    );

                // =========================
                // 2) Query utama: AMBIL SEMUA RECORD (by kunjungan)
                // =========================
                var dataList = await (
                    from a in _applicationDbContext.SOAPs
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                    join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                    join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                    join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                    where a.IsDelete == false && a.KunjunganId == kunjunganid
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        a.SOAPID,
                        a.KunjunganId,
                        PasienId = k.PasienId,
                        a.Subjective,
                        a.Objective,
                        a.DaftarICD10, // CSV
                        a.DaftarSDKI,  // CSV
                        a.Assessment,
                        a.Planning,
                        a.Evaluasi,
                        a.Intervensi,
                        a.Reevaluasi,
                        NamaProfesi = string.Equals(a.Profesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.Profesi,
                        NamaDokter = d.NmDokter,
                        DokterId = d.DokterId,
                        NamaPasien = p.NamaLengkap
                    })
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreateDateTime)
                    .ToListAsync();

                if (dataList.Count == 0)
                {
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
                }

                // =========================
                // 3) Proyeksi + parsing ICD/SDKI per record
                // =========================
                var result = dataList.Select(data =>
                {
                    // ICD
                    var icdCodes = (data.DaftarICD10 ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var icdNames = icdCodes
                        .Select(code => icdDict.TryGetValue(code, out var nama)
                            ? nama
                            : $"(Kode tidak ditemukan: {code})")
                        .ToList();

                    // SDKI
                    var sdkiCodes = (data.DaftarSDKI ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var sdkiNames = sdkiCodes
                        .Select(code => sdkiDict.TryGetValue(code, out var nama)
                            ? nama
                            : $"(Kode tidak ditemukan: {code})")
                        .ToList();

                    return new
                    {
                        data.CreateDateTime,
                        data.CreateBy,
                        data.CreateByName,
                        data.SOAPID,
                        data.KunjunganId,
                        data.PasienId,
                        data.Subjective,
                        data.Objective,
                        DaftarICD10 = icdCodes,
                        NamaICD = icdNames,
                        DaftarSDKI = sdkiCodes,
                        NamaSDKI = sdkiNames,
                        data.Assessment,
                        data.Planning,
                        data.Evaluasi,
                        data.Intervensi,
                        data.Reevaluasi,
                        data.NamaProfesi,
                        data.NamaDokter,
                        data.DokterId,
                        data.NamaPasien
                    };
                })
                .ToList();

                // =========================
                // 4) Return hasil (semua record)
                // =========================
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("pasien/{pasienid}")]
        public async Task<IActionResult> GetAllSoapByPasienId(Guid pasienid)
        {
            try
            {
                // =========================
                // 1) Ambil & rapikan kamus ICD (Kode -> Nama) aman dari duplikat
                // =========================
                var icdRows = await _applicationDbContext.ICD10s
                    .AsNoTracking()
                    .Select(x => new { x.ICDCode, x.ICDName })
                    .ToListAsync();

                var icdDict = icdRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.ICDCode))
                    .Select(x => new { Code = x.ICDCode.Trim(), x.ICDName })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().ICDName,
                        StringComparer.OrdinalIgnoreCase
                    );

                var sdkiRows = await _applicationDbContext.SDKIDiagnosas
                    .AsNoTracking()
                    .Select(x => new { x.SDKIKodeDiagnosa, x.NamaDiagnosa })
                    .ToListAsync();

                var sdkiDict = sdkiRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.SDKIKodeDiagnosa))
                    .Select(x => new { Code = x.SDKIKodeDiagnosa.Trim(), x.NamaDiagnosa })
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().NamaDiagnosa,
                        StringComparer.OrdinalIgnoreCase
                    );

                // =========================
                // 2) Query utama: AMBIL SEMUA RECORD utk pasien tsb
                // =========================
                var dataList = await (
                    from a in _applicationDbContext.SOAPs
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                    join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                    join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                    join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                    where a.IsDelete == false && k.PasienId == pasienid
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        a.SOAPID,
                        a.KunjunganId,
                        PasienId = k.PasienId,
                        a.Subjective,
                        a.Objective,
                        a.DaftarICD10, // CSV
                        a.DaftarSDKI,  // CSV
                        a.Assessment,
                        a.Planning,
                        a.Evaluasi,
                        a.Intervensi,
                        a.Reevaluasi,
                        NamaProfesi = string.Equals(a.Profesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.Profesi,
                        NamaDokter = d.NmDokter,
                        DokterId = d.DokterId,
                        NamaPasien = p.NamaLengkap
                    })
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreateDateTime) // urut terbaru dulu
                    .ToListAsync();

                if (dataList.Count == 0)
                {
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
                }

                // =========================
                // 3) Proyeksi + parsing ICD/SDKI per record
                // =========================
                var result = dataList.Select(data =>
                {
                    // ICD
                    var icdCodes = (data.DaftarICD10 ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var icdNames = icdCodes
                        .Select(code => icdDict.TryGetValue(code, out var nama)
                            ? nama
                            : $"(Kode tidak ditemukan: {code})")
                        .ToList();

                    // SDKI
                    var sdkiCodes = (data.DaftarSDKI ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var sdkiNames = sdkiCodes
                        .Select(code => sdkiDict.TryGetValue(code, out var nama)
                            ? nama
                            : $"(Kode tidak ditemukan: {code})")
                        .ToList();

                    return new
                    {
                        data.CreateDateTime,
                        data.CreateBy,
                        data.CreateByName,
                        data.SOAPID,
                        data.KunjunganId,
                        data.PasienId,
                        data.Subjective,
                        data.Objective,
                        DaftarICD10 = icdCodes,
                        NamaICD = icdNames,
                        DaftarSDKI = sdkiCodes,
                        NamaSDKI = sdkiNames,
                        data.Assessment,
                        data.Planning,
                        data.Evaluasi,
                        data.Intervensi,
                        data.Reevaluasi,
                        data.NamaProfesi,
                        data.NamaDokter,
                        data.DokterId,
                        data.NamaPasien
                    };
                })
                .ToList();

                // =========================
                // 4) Return hasil (semua record)
                // =========================
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }


        //[HttpGet("SOAPDokter/{dokterid}")]
        //public async Task<IActionResult> GetByDokterId(Guid dokterid)
        //{
        //    var data = (from a in _applicationDbContext.SOAPs
        //                join u in _applicationDbContext.UserActives
        //                    on a.CreateBy equals u.UserActiveId
        //                join k in _applicationDbContext.Kunjungans
        //                    on a.KunjunganId equals k.KunjunganID
        //                join d in _applicationDbContext.Dokters
        //                    on k.DokterId equals d.DokterId
        //                where a.IsDelete == false && k.DokterId == dokterid
        //                select new
        //                {
        //                    CreateDateTime = a.CreateDateTime,
        //                    CreateBy = a.CreateBy,
        //                    CreateByName = u.FullName,
        //                    SOAPID = a.SOAPID,
        //                    KunjunganId = a.KunjunganId,
        //                    PasienId = k.PasienId,
        //                    Subjective = a.Subjective,
        //                    Objective = a.Objective,
        //                    DaftarICD10 = (a.DaftarICD10 ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        //                    Assessment = a.Assessment,
        //                    Planning = a.Planning,
        //                    Profesi = a.Profesi,
        //                    RanapId = a.RanapId,
        //                    NamaDokter = d.NmDokter,
        //                }).ToListAsync(); // Fix: Use ToListAsync() on IQueryable, not on the anonymous type.  

        //    var result = await data; // Await the ToListAsync() result.  

        //    if (!result.Any())
        //    {
        //        return NotFound(new { message = "Data tidak ditemukan." });
        //    }

        //    return Ok(new
        //    {
        //        message = "Ditemukan || 200 OK",
        //        data = result
        //    });
        //}

        [HttpPost]
        public async Task<IActionResult> CreateSOAP([FromBody] SOAPViewModel vm)
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

                // **Buat Data Baru**
                var data = new SOAP
                {
                    SOAPID = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    Subjective = vm.Subjective,
                    Objective = vm.Objective,
                    DaftarICD10 = vm.DaftarICD10 != null ? string.Join(",", vm.DaftarICD10) : null,
                    DaftarSDKI = vm.DaftarSDKI != null ? string.Join(",", vm.DaftarSDKI) : null,
                    Assessment = vm.Assessment,
                    Planning = vm.Planning,
                    Evaluasi = vm.Evaluasi,
                    Intervensi = vm.Intervensi,
                    Reevaluasi = vm.Reevaluasi,
                    Profesi = vm.Profesi,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };
                // **Simpan ke Database**
                _applicationDbContext.SOAPs.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                //Notifikasi ke SignalR Hub
                await _hubContext.Clients.All.SendAsync("SOAP ditambah", new
                {
                    action = "create",
                    soapid = data.SOAPID,
                });

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

        //[HttpPost]
        //public async Task<IActionResult> CreateSOAP([FromBody] SOAPViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // ✅ Cek koneksi ke database
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // ✅ Ambil User aktif dari JWT
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

        //        // ✅ Buat Data SOAP
        //        var soapId = Guid.NewGuid();
        //        var data = new SOAP
        //        {
        //            SOAPID = soapId,
        //            KunjunganId = vm.KunjunganId,
        //            Subjective = vm.Subjective,
        //            Objective = vm.Objective,
        //            DaftarICD10 = vm.DaftarICD10 != null ? string.Join(",", vm.DaftarICD10) : null,
        //            DaftarSDKI = vm.DaftarSDKI != null ? string.Join(",", vm.DaftarSDKI) : null,
        //            Assessment = vm.Assessment,
        //            Planning = vm.Planning,
        //            Evaluasi = vm.Evaluasi,
        //            Intervensi = vm.Intervensi,
        //            Reevaluasi = vm.Reevaluasi,
        //            Profesi = vm.Profesi,
        //            CreateBy = userActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //        };

        //        _applicationDbContext.SOAPs.Add(data);

        //        // ✅ Tambahkan ke tabel DetailICD (jika ada ICD list)
        //        if (vm.DaftarICD10 != null && vm.DaftarICD10.Any())
        //        {
        //            var detailList = vm.DaftarICD10.Select((icdId, index) => new DetailICD
        //            {
        //                DetailICDId = Guid.NewGuid(),
        //                KunjunganId = vm.KunjunganId,
        //                SoapId = soapId,
        //                ICDId = icdId,
        //                isUtama = index == 0, // ICD pertama dianggap utama
        //                CreateBy = userActiveId,
        //                CreateDateTime = DateTimeOffset.UtcNow
        //            }).ToList();

        //            _applicationDbContext.DetailICDs.AddRange(detailList);
        //        }

        //        int result = await _applicationDbContext.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        // ✅ Kirim notifikasi ke SignalR
        //        await _hubContext.Clients.All.SendAsync("SOAP ditambah", new
        //        {
        //            action = "create",
        //            soapid = soapId,
        //        });

        //        if (result > 0)
        //        {
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSOAP(Guid id, [FromBody] SOAPViewModel vm)
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
                var data = await _applicationDbContext.SOAPs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.Subjective = vm.Subjective;
                data.Objective = vm.Objective;
                data.DaftarICD10 = vm.DaftarICD10 != null ? string.Join(",", vm.DaftarICD10) : null;
                data.DaftarSDKI = vm.DaftarSDKI != null ? string.Join(",", vm.DaftarSDKI) : null;
                data.Assessment = vm.Assessment;
                data.Planning = vm.Planning;
                data.Evaluasi = vm.Evaluasi;
                data.Intervensi = vm.Intervensi;
                data.Reevaluasi = vm.Reevaluasi;
                data.Profesi = vm.Profesi;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.SOAPs.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi ke SignalR Hub
                await _hubContext.Clients.All.SendAsync("SOAP diubah", new
                {
                    action = "update",
                    soapid = data.SOAPID,
                });

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

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateSOAP(Guid id, [FromBody] SOAPViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // ✅ Cek koneksi ke database
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // ✅ Ambil user aktif dari JWT
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

        //        // ✅ Cek apakah data SOAP ada
        //        var existingSOAP = await _applicationDbContext.SOAPs
        //            .FirstOrDefaultAsync(s => s.SOAPID == id && (s.IsDelete == false || s.IsDelete == null));

        //        if (existingSOAP == null)
        //        {
        //            return NotFound(new { message = "Data SOAP tidak ditemukan || 404 Not Found" });
        //        }

        //        // ✅ Update field-field SOAP
        //        existingSOAP.Subjective = vm.Subjective;
        //        existingSOAP.Objective = vm.Objective;
        //        existingSOAP.DaftarICD10 = vm.DaftarICD10 != null ? string.Join(",", vm.DaftarICD10) : null;
        //        existingSOAP.DaftarSDKI = vm.DaftarSDKI != null ? string.Join(",", vm.DaftarSDKI) : null;
        //        existingSOAP.Assessment = vm.Assessment;
        //        existingSOAP.Planning = vm.Planning;
        //        existingSOAP.Evaluasi = vm.Evaluasi;
        //        existingSOAP.Intervensi = vm.Intervensi;
        //        existingSOAP.Reevaluasi = vm.Reevaluasi;
        //        existingSOAP.Profesi = vm.Profesi;
        //        existingSOAP.UpdateBy = userActiveId;
        //        existingSOAP.UpdateDateTime = DateTimeOffset.UtcNow;

        //        _applicationDbContext.SOAPs.Update(existingSOAP);

        //        // ✅ Hapus semua detail ICD lama
        //        var oldDetails = _applicationDbContext.DetailICDs
        //            .Where(d => d.SoapId == id);

        //        _applicationDbContext.DetailICDs.RemoveRange(oldDetails);

        //        // ✅ Tambahkan ulang detail ICD baru
        //        if (vm.DaftarICD10 != null && vm.DaftarICD10.Any())
        //        {
        //            var detailList = vm.DaftarICD10.Select((icdId, index) => new DetailICD
        //            {
        //                DetailICDId = Guid.NewGuid(),
        //                KunjunganId = vm.KunjunganId,
        //                SoapId = id,
        //                ICDId = icdId, // ← pastikan vm.DaftarICD10 adalah List<Guid>
        //                isUtama = index == 0,
        //                CreateBy = userActiveId,
        //                CreateDateTime = DateTimeOffset.UtcNow
        //            }).ToList();

        //            await _applicationDbContext.DetailICDs.AddRangeAsync(detailList);
        //        }

        //        int result = await _applicationDbContext.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        // ✅ Kirim notifikasi ke SignalR Hub
        //        await _hubContext.Clients.All.SendAsync("SOAP diperbarui", new
        //        {
        //            action = "update",
        //            soapid = id,
        //        });

        //        if (result > 0)
        //        {
        //            return Ok(new { message = "Update Data Berhasil || 200 OK" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSOAP(Guid id)
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
                var data = await _applicationDbContext.SOAPs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.SOAPs.Update(data);
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
        public async Task<IActionResult> PagedSOAP(
        int page = 1,
        int perPage = 10,
        Guid? search = null,                       
        Guid? kunjunganId = null,
        Guid? dokterId = null,
        Guid? CreateById = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        )
        {

            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;


            // --- Base query: WAJIB filter pasien di sini ---
            var query =
                from a in _applicationDbContext.SOAPs
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                where a.IsDelete == false
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.SOAPID,
                    a.KunjunganId,
                    PasienId = k.PasienId,
                    a.Subjective,
                    a.Objective,
                    a.DaftarICD10,
                    a.DaftarSDKI,
                    a.Assessment,
                    a.Planning,
                    a.Evaluasi,
                    a.Intervensi,
                    a.Reevaluasi,
                    NamaProfesi = string.Equals(a.Profesi, "null", StringComparison.OrdinalIgnoreCase)
                            ? null
                            : a.Profesi,
                    NamaDokter = d.NmDokter,
                    DokterId = d.DokterId,
                    NamaPasien = p.NamaLengkap,
                };

            // filter berdasarkan pasien id
            if (search.HasValue)
            {
                query = query.Where(u=>u.PasienId == search.Value);
            }

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // filter based on dokter id
            if (dokterId.HasValue) 
            { 
                query = query.Where(u=>u.DokterId== dokterId.Value);
            }

            // filter based on create by id
            if (CreateById.HasValue)
            { 
                query = query.Where(u=>u.CreateBy == CreateById.Value);
            }

            // --- Filter tanggal rentang eksplisit ---
            if (startDate.HasValue && endDate.HasValue)
            {
                // Jadikan rentang [start 00:00:00, end 23:59:59.999...] dalam UTC
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

                query = query.Where(u => u.CreateDateTime >= startUtc && u.CreateDateTime <= endUtc);
            }

            // --- Filter periode relatif ---
            if (periode.HasValue)
            {
                var todayUtc = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == todayUtc);
                        break;

                    case PeriodeFilter.ThisWeek:
                        // Minggu ini (Mulai Minggu s/d hari ini)
                        var startOfWeek = todayUtc.AddDays(-((int)todayUtc.DayOfWeek));
                        query = query.Where(u => u.CreateDateTime.Date >= startOfWeek && u.CreateDateTime.Date <= todayUtc);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startOfThisWeek = todayUtc.AddDays(-((int)todayUtc.DayOfWeek));
                        var startOfLastWeek = startOfThisWeek.AddDays(-7);
                        query = query.Where(u => u.CreateDateTime.Date >= startOfLastWeek && u.CreateDateTime.Date < startOfThisWeek);
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u => u.CreateDateTime.Month == todayUtc.Month && u.CreateDateTime.Year == todayUtc.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        var lastMonthDate = todayUtc.AddMonths(-1);
                        query = query.Where(u => u.CreateDateTime.Month == lastMonthDate.Month && u.CreateDateTime.Year == lastMonthDate.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == todayUtc.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == todayUtc.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= todayUtc.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= todayUtc.AddMonths(-6));
                        break;
                }
            }

            // --- Sorting ---
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = desc
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "Subjective" => query.OrderByDescending(u => u.Subjective),
                    "Objective" => query.OrderByDescending(u => u.Objective),
                    "Assessment" => query.OrderByDescending(u => u.Assessment),
                    "Planning" => query.OrderByDescending(u => u.Planning),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "Subjective" => query.OrderBy(u => u.Subjective),
                    "Objective" => query.OrderBy(u => u.Objective),
                    "Assessment" => query.OrderBy(u => u.Assessment),
                    "Planning" => query.OrderBy(u => u.Planning),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // --- Paging ---
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return NotFound(new { message = "Data untuk pasien ini tidak ditemukan." });
            }
            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            // Ambil halaman
            var pageRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            // --- (Opsional) Ubah CSV ICD menjadi List<string> setelah materialize ---
            var rows = pageRows.Select(x => new
            {
                x.CreateDateTime,
                x.CreateBy,
                x.CreateByName,
                x.SOAPID,
                x.KunjunganId,
                x.PasienId,
                x.Subjective,
                x.Objective,
                daftarICD10 = (x.DaftarICD10 ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                x.Assessment,
                x.Planning,
                x.Evaluasi,
                x.Intervensi,
                x.Reevaluasi,
                x.NamaProfesi,
                x.NamaDokter,
                x.DokterId,
                x.NamaPasien
            });

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


        //[HttpGet("paged")]
        //public async Task<IActionResult> PagedSOAP(
        //    int page = 1,
        //    int perPage = 10,
        //    Guid? search = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        //    DateTime? startDate = null,
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        //    DateTime? endDate = null,
        //    [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        //)
        //{
        //    if (!search.HasValue)
        //    {
        //        return BadRequest(new { message = "PasienId (search) is required." });
        //    }

        //    // Cari Kunjungan berdasarkan PasienId
        //    var kunjungan = await _applicationDbContext.Kunjungans
        //        .FirstOrDefaultAsync(k => k.PasienId == search);

        //    if (kunjungan == null)
        //    {
        //        return NotFound(new { message = "Kunjungan untuk pasien ini tidak ditemukan." });
        //    }

        //    // Query data SOAP berdasarkan KunjunganId yang ditemukan
        //    var query = (from a in _applicationDbContext.SOAPs
        //                 join u in _applicationDbContext.UserActives
        //                     on a.CreateBy equals u.UserActiveId
        //                 join k in _applicationDbContext.Kunjungans
        //                     on a.KunjunganId equals k.KunjunganID
        //                join d in _applicationDbContext.Dokters
        //                     on k.DokterId equals d.DokterId
        //                 join p in _applicationDbContext.PendaftaranPasienBarus
        //                        on k.PasienId equals p.PendaftaranPasienBaruId
        //                 where a.IsDelete == false
        //                 select new
        //                 {
        //                     CreateDateTime = a.CreateDateTime,
        //                     CreateBy = a.CreateBy,
        //                     CreateByName = u.FullName,
        //                     SOAPID = a.SOAPID,
        //                     KunjunganId = a.KunjunganId,
        //                     PasienId = k.PasienId, // Tambahan ini
        //                     Subjective = a.Subjective,
        //                     Objective = a.Objective,
        //                     DaftarICD10 = (a.DaftarICD10 ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        //                     Assessment = a.Assessment,
        //                     Planning = a.Planning,
        //                     Evaluasi = a.Evaluasi,
        //                     Intervensi = a.Intervensi,
        //                     Reevaluasi = a.Reevaluasi,
        //                     Profesi = a.Profesi,
        //                     NamaDokter = d.NmDokter,
        //                     DokterId = d.DokterId, // Tambahan ini untuk mendapatkan DokterId
        //                     NamaPasien = p.NamaLengkap, // Tambahan ini untuk mendapatkan Nama Pasien
        //                 });

        //    // **Filter berdasarkan tanggal**
        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //        DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

        //        query = query.Where(u =>
        //            u.CreateDateTime >= startUtc &&
        //            u.CreateDateTime <= endUtc);
        //    }

        //    // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
        //    if (periode.HasValue)
        //    {
        //        DateTime today = DateTime.UtcNow.Date;

        //        switch (periode)
        //        {
        //            case PeriodeFilter.Today:
        //                query = query.Where(u => u.CreateDateTime.Date == today);
        //                break;
        //            case PeriodeFilter.ThisWeek:
        //                query = query.Where(u =>
        //                    u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
        //                    u.CreateDateTime.Date <= today
        //                );
        //                break;
        //            case PeriodeFilter.LastWeek:
        //                query = query.Where(u =>
        //                    u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
        //                    u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
        //                );
        //                break;
        //            case PeriodeFilter.ThisMonth:
        //                query = query.Where(u =>
        //                    u.CreateDateTime.Month == today.Month &&
        //                    u.CreateDateTime.Year == today.Year
        //                );
        //                break;
        //            case PeriodeFilter.LastMonth:
        //                query = query.Where(u =>
        //                    u.CreateDateTime.Month == today.Month - 1 &&
        //                    u.CreateDateTime.Year == today.Year
        //                );
        //                break;
        //            case PeriodeFilter.ThisYear:
        //                query = query.Where(u => u.CreateDateTime.Year == today.Year);
        //                break;
        //            case PeriodeFilter.LastYear:
        //                query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
        //                break;
        //            case PeriodeFilter.Last3Months:
        //                query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
        //                break;
        //            case PeriodeFilter.Last6Months:
        //                query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
        //                break;
        //        }
        //    }

        //    // Sorting Data
        //    query = sortDirection?.ToLower() == "desc"
        //        ? orderBy switch
        //        {
        //            "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
        //            "CreateByName" => query.OrderByDescending(u => u.CreateByName),
        //            "Subjective" => query.OrderByDescending(u => u.Subjective),
        //            "Objective" => query.OrderByDescending(u => u.Objective),
        //            "Assessment" => query.OrderByDescending(u => u.Assessment),
        //            "Planning" => query.OrderByDescending(u => u.Planning),
        //            _ => query.OrderByDescending(u => u.CreateDateTime)
        //        }
        //        : orderBy switch
        //        {
        //            "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
        //            "CreateByName" => query.OrderBy(u => u.CreateByName),
        //            "Subjective" => query.OrderBy(u => u.Subjective),
        //            "Objective" => query.OrderBy(u => u.Objective),
        //            "Assessment" => query.OrderBy(u => u.Assessment),
        //            "Planning" => query.OrderBy(u => u.Planning),
        //            _ => query.OrderBy(u => u.CreateDateTime)
        //        };

        //    // Pagination
        //    var totalRows = await query.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
        //    var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

        //    if (rows.Count == 0 && page > totalPages)
        //    {
        //        return NotFound(new { message = "Page not found." });
        //    }

        //    return Ok(new
        //    {
        //        status = "success",
        //        message = "Data retrieved successfully",
        //        data = new
        //        {
        //            Rows = rows,
        //            TotalRows = totalRows,
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalPages = totalPages
        //        }
        //    });
        //}



    }
}
