using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

        public SOAPController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SOAPController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlLSOAP(int page = 1, int perPage = 10)
        {
            // Normalisasi parameter
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // 1) Ambil kamus ICD (Kode -> Nama) sekali saja
            var icdDict = await _applicationDbContext.ICD10s
                .AsNoTracking()
                .Select(x => new { x.ICDCode, x.ICDName })
                .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

            // 2) Ambil data utama (tanpa Split di server, supaya tetap bisa dieksekusi oleh EF/SQL)
            var raw = await (from a in _applicationDbContext.SOAPs
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
                                 a.DaftarICD10, // simpan apa adanya dulu (CSV)
                                 a.Assessment,
                                 a.Planning,
                                 a.Evaluasi,
                                 a.Intervensi,
                                 a.Reevaluasi,
                                 a.Profesi,
                                 NamaDokter = d.NmDokter,
                                 DokterId = d.DokterId,
                                 NamaPasien = p.NamaLengkap
                             })
                            .AsNoTracking()
                            .ToListAsync();

            var projected = raw
                .Select(a =>
                {
                    var codes = (a.DaftarICD10 ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var namaIcd = codes
                        .Select(code =>
                            icdDict.TryGetValue(code, out var nama)
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
                        DaftarICD10 = codes,   // list kode ICD
                        NamaICD = namaIcd,     // list nama ICD hasil "join"
                        a.Assessment,
                        a.Planning,
                        a.Evaluasi,
                        a.Intervensi,
                        a.Reevaluasi,
                        a.Profesi,
                        a.NamaDokter,
                        a.DokterId,
                        a.NamaPasien
                    };
                })
                .OrderByDescending(a => a.CreateDateTime)
                .ToList();

            // 4) Paging di sisi memory (karena sudah perlu materialize untuk parsing ICD)
            var totalRows = projected.Count;
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = projected
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // 5) Return hasil
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

        //[HttpGet]
        //public async Task<IActionResult> GetAlLSOAP(int page = 1, int perPage = 10)
        //{
        //    // Validasi agar page dan perPage minimal bernilai 1
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // Query data
        //    var query = (from a in _applicationDbContext.SOAPs
        //                 join u in _applicationDbContext.UserActives
        //                     on a.CreateBy equals u.UserActiveId
        //                 join k in _applicationDbContext.Kunjungans
        //                     on a.KunjunganId equals k.KunjunganID
        //                 join d in _applicationDbContext.Dokters
        //                      on k.DokterId equals d.DokterId
        //                 join p in _applicationDbContext.PendaftaranPasienBarus
        //                     on k.PasienId equals p.PendaftaranPasienBaruId
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
        //                 }).OrderByDescending(a => a.CreateDateTime).ToList();

        //    // Hitung total data sebelum paginasi
        //    var totalRows = query.Count();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    // Ambil data sesuai paging
        //    var listdata = query
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage).ToList();

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
        public async Task<IActionResult> GetSOAPById(Guid id)
        {
            // 1) Ambil kamus ICD (Kode -> Nama) sekali saja
            var icdDict = await _applicationDbContext.ICD10s
                .AsNoTracking()
                .Select(x => new { x.ICDCode, x.ICDName })
                .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

            // 2) Ambil data utama berdasarkan SOAPID
            var raw = await (from a in _applicationDbContext.SOAPs
                             join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                             join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                             join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                             join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                             where a.SOAPID == id && a.IsDelete == false // Filter berdasarkan SOAPID
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
                                 a.DaftarICD10, // simpan apa adanya dulu (CSV)
                                 a.Assessment,
                                 a.Planning,
                                 a.Evaluasi,
                                 a.Intervensi,
                                 a.Reevaluasi,
                                 a.Profesi,
                                 NamaDokter = d.NmDokter,
                                 DokterId = d.DokterId,
                                 NamaPasien = p.NamaLengkap
                             })
                             .AsNoTracking()
                             .FirstOrDefaultAsync(); // Mengambil satu data

            // 3) Jika data tidak ditemukan, kembalikan 404 Not Found
            if (raw == null)
            {
                return NotFound(new { message = "Data tidak ditemukan" });
            }

            // 4) Proses ICD (parsing dan lookup)
            var codes = (raw.DaftarICD10 ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var namaIcd = codes
                .Select(code =>
                {
                    icdDict.TryGetValue(code, out var nama);
                    return nama ?? $"(Kode tidak ditemukan: {code})";
                })
                .ToList();

            // 5) Proyeksi hasil akhir
            var resultData = new
            {
                raw.CreateDateTime,
                raw.CreateBy,
                raw.CreateByName,
                raw.SOAPID,
                raw.KunjunganId,
                raw.PasienId,
                raw.Subjective,
                raw.Objective,
                DaftarICD10 = codes,    // list kode ICD
                NamaICD = namaIcd,      // list nama ICD hasil "join"
                raw.Assessment,
                raw.Planning,
                raw.Evaluasi,
                raw.Intervensi,
                raw.Reevaluasi,
                raw.Profesi,
                raw.NamaDokter,
                raw.DokterId,
                raw.NamaPasien
            };

            // 6) Return hasil
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = resultData
            });
        }

        [HttpGet("kunjungan/{kunjunganid}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganid)
        {
            // 1) Ambil kamus ICD(Kode->Nama) sekali saja
            var icdDict = await _applicationDbContext.ICD10s
                .AsNoTracking()
                .Select(x => new { x.ICDCode, x.ICDName })
                .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

            // 2) Ambil data utama berdasarkan SOAPID
            var raw = await (from a in _applicationDbContext.SOAPs
                             join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                             join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                             join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                             join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                             where a.KunjunganId == kunjunganid && a.IsDelete == false // Filter berdasarkan SOAPID
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
                                 a.DaftarICD10, // simpan apa adanya dulu (CSV)
                                 a.Assessment,
                                 a.Planning,
                                 a.Evaluasi,
                                 a.Intervensi,
                                 a.Reevaluasi,
                                 a.Profesi,
                                 NamaDokter = d.NmDokter,
                                 DokterId = d.DokterId,
                                 NamaPasien = p.NamaLengkap
                             })
                             .AsNoTracking()
                             .FirstOrDefaultAsync(); // Mengambil satu data

            // 3) Jika data tidak ditemukan, kembalikan 404 Not Found
            if (raw == null)
            {
                return NotFound(new { message = "Data tidak ditemukan" });
            }

            // 4) Proses ICD (parsing dan lookup)
            var codes = (raw.DaftarICD10 ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var namaIcd = codes
                .Select(code =>
                {
                    icdDict.TryGetValue(code, out var nama);
                    return nama ?? $"(Kode tidak ditemukan: {code})";
                })
                .ToList();

            // 5) Proyeksi hasil akhir
            var resultData = new
            {
                raw.CreateDateTime,
                raw.CreateBy,
                raw.CreateByName,
                raw.SOAPID,
                raw.KunjunganId,
                raw.PasienId,
                raw.Subjective,
                raw.Objective,
                DaftarICD10 = codes,    // list kode ICD
                NamaICD = namaIcd,      // list nama ICD hasil "join"
                raw.Assessment,
                raw.Planning,
                raw.Evaluasi,
                raw.Intervensi,
                raw.Reevaluasi,
                raw.Profesi,
                raw.NamaDokter,
                raw.DokterId,
                raw.NamaPasien
            };

            // 6) Return hasil
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = resultData
            });
        }

        [HttpGet("pasien/{pasienid}")]
        public async Task<IActionResult> GetByPasienId(Guid pasienid)
        {
            // 1) Ambil kamus ICD(Kode->Nama) sekali saja  
            var icdDict = await _applicationDbContext.ICD10s
                .AsNoTracking()
                .Select(x => new { x.ICDCode, x.ICDName })
                .ToDictionaryAsync(x => x.ICDCode, x => x.ICDName);

            // 2) Ambil data utama berdasarkan PasienId  
            var raw = await (from a in _applicationDbContext.SOAPs
                             join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                             join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                             join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                             join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                             where k.PasienId == pasienid && a.IsDelete == false
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
                                 a.Assessment,
                                 a.Planning,
                                 a.Evaluasi,
                                 a.Intervensi,
                                 a.Reevaluasi,
                                 a.Profesi,
                                 NamaDokter = d.NmDokter,
                                 DokterId = d.DokterId,
                                 NamaPasien = p.NamaLengkap
                             })
                           .AsNoTracking()
                           .ToListAsync();

            // 3) Jika data tidak ditemukan, kembalikan 404 Not Found  
            if (raw == null || !raw.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan" });
            }

            // 4) Proses ICD (parsing dan lookup)  
            var resultData = raw.Select(data =>
            {
                var codes = (data.DaftarICD10 ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var namaIcd = codes
                    .Select(code =>
                    {
                        icdDict.TryGetValue(code, out var nama);
                        return nama ?? $"(Kode tidak ditemukan: {code})";
                    })
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
                    DaftarICD10 = codes,    // list kode ICD  
                    NamaICD = namaIcd,      // list nama ICD hasil "join"  
                    data.Assessment,
                    data.Planning,
                    data.Evaluasi,
                    data.Intervensi,
                    data.Reevaluasi,
                    data.Profesi,
                    data.NamaDokter,
                    data.DokterId,
                    data.NamaPasien
                };
            }).ToList();

            // 5) Return hasil  
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = resultData
            });
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
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        )
        {
            if (!search.HasValue)
            {
                return BadRequest(new { message = "PasienId (search) is required." });
            }

            // Cari Kunjungan berdasarkan PasienId
            var kunjungan = await _applicationDbContext.Kunjungans
                .FirstOrDefaultAsync(k => k.PasienId == search);

            if (kunjungan == null)
            {
                return NotFound(new { message = "Kunjungan untuk pasien ini tidak ditemukan." });
            }

            // Query data SOAP berdasarkan KunjunganId yang ditemukan
            var query = (from a in _applicationDbContext.SOAPs
                         join u in _applicationDbContext.UserActives
                             on a.CreateBy equals u.UserActiveId
                         join k in _applicationDbContext.Kunjungans
                             on a.KunjunganId equals k.KunjunganID
                        join d in _applicationDbContext.Dokters
                             on k.DokterId equals d.DokterId
                         join p in _applicationDbContext.PendaftaranPasienBarus
                                on k.PasienId equals p.PendaftaranPasienBaruId
                         where a.IsDelete == false
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             SOAPID = a.SOAPID,
                             KunjunganId = a.KunjunganId,
                             PasienId = k.PasienId, // Tambahan ini
                             Subjective = a.Subjective,
                             Objective = a.Objective,
                             DaftarICD10 = (a.DaftarICD10 ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                             Assessment = a.Assessment,
                             Planning = a.Planning,
                             Evaluasi = a.Evaluasi,
                             Intervensi = a.Intervensi,
                             Reevaluasi = a.Reevaluasi,
                             Profesi = a.Profesi,
                             NamaDokter = d.NmDokter,
                             DokterId = d.DokterId, // Tambahan ini untuk mendapatkan DokterId
                             NamaPasien = p.NamaLengkap, // Tambahan ini untuk mendapatkan Nama Pasien
                         });

            // **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
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

            // Sorting Data
            query = sortDirection?.ToLower() == "desc"
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

            // Pagination
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
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



    }
}
