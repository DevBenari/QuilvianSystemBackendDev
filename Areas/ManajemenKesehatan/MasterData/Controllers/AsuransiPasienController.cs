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
using System.Security.Claims;


namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AsuransiPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AsuransiPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AsuransiPasienController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<AsuransiPasienController> logger,
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
        public async Task<IActionResult> GetAsuransiPasien(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            var result = (from ap in _applicationDbContext.AsuransiPasiens
                         join p in _applicationDbContext.PendaftaranPasienBarus on ap.PasienId equals p.PendaftaranPasienBaruId
                         join a in _applicationDbContext.Asuransis on ap.AsuransiId equals a.AsuransiId
                         select new
                         {
                             ap.AsuransiPasienId,
                             ap.PasienId,
                             ap.AsuransiId,
                             ap.CreateDateTime, 
                             NamaPasien = p.NamaLengkap,
                             NamaAsuransi = a.NamaAsuransi,
                             a.IsPKS,
                             ap.NoPolis,
                             ap.Umur
                         }).OrderByDescending(ap => ap.CreateDateTime);

            // Hitung total data sebelum paginasi
            var totalRows = result.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = result
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
        public async Task<IActionResult> GetAsuransiPasienById(Guid id)
        {
            var listdata = _applicationDbContext.AsuransiPasiens.Find(id);
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

        [HttpGet("AsuransiByPasien/{pasienId}")]
        public async Task<IActionResult> GetAsuransiPasienByPasienId(Guid pasienId)
        {
            var listdata = (from ap in _applicationDbContext.AsuransiPasiens
                            join p in _applicationDbContext.PendaftaranPasienBarus on ap.PasienId equals p.PendaftaranPasienBaruId
                            join a in _applicationDbContext.Asuransis on ap.AsuransiId equals a.AsuransiId
                            where ap.PasienId == pasienId
                            select new
                            {
                                ap.AsuransiPasienId,
                                ap.PasienId,
                                ap.AsuransiId,
                                ap.CreateDateTime,
                                NamaPasien = p.NamaLengkap,
                                NamaAsuransi = a.NamaAsuransi,
                                a.IsPKS,
                                ap.NoPolis,
                                ap.Umur
                            }).OrderByDescending(ap => ap.CreateDateTime);

            if (listdata == null || !listdata.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        //public async Task<IActionResult> GetAsuransiPasienByPasienId(Guid pasienId)
        //{
        //    var listdata = await
        //        (from ap in _applicationDbContext.AsuransiPasiens
        //         join p in _applicationDbContext.PendaftaranPasienBarus on ap.PasienId equals p.Id
        //         join a in _applicationDbContext.Asuransis on ap.AsuransiId equals a.Id
        //         where ap.PasienId == pasienId
        //         select new
        //         {
        //             ap.Id,
        //             ap.PasienId,
        //             NamaPasien = p.NamaLengkap,  // sesuaikan
        //             ap.AsuransiId,
        //             NamaAsuransi = a.Nama,       // sesuaikan
        //             ap.NoPolis,
        //             ap.TanggalMulai,
        //             ap.TanggalAkhir,
        //             ap.IsAktif
        //         })
        //        .ToListAsync();

        //    if (listdata.Count == 0)
        //        return NotFound(new { message = "Data tidak ditemukan." });

        //    return Ok(new { message = "Ditemukan || 200 OK", data = listdata });
        //}

        [HttpPost]
        public async Task<IActionResult> CreateAsuransiPasien([FromBody] AsuransiPasienViewModel request)
        {
            if (request == null || request.PasienId == null || request.AsuransiId == null)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
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

                // Periksa apakah pasien dan asuransi ada di database
                var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                                      .Any(p => p.PendaftaranPasienBaruId == request.PasienId);

                var asuransiExists = _applicationDbContext.Asuransis
                                      .Any(a => a.AsuransiId == request.AsuransiId);

                if (!pasienExists || !asuransiExists)
                {
                    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                }


                //validate model state
                if (ModelState.IsValid)
                {
                    var newAsuransiPasien = new AsuransiPasien
                    {
                        AsuransiPasienId = Guid.NewGuid(),
                        PasienId = request.PasienId,
                        AsuransiId = request.AsuransiId,
                        NoPolis = request.NoPolis,
                        Umur = request.Umur,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId
                    };

                    _applicationDbContext.AsuransiPasiens.Add(newAsuransiPasien);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Data berhasil ditambahkan!", data = newAsuransiPasien });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateAsuransiPasien([FromBody] AsuransiPasienViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
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

        //         var dateNow = DateTime.UtcNow;;
        //        var setDateNow = dateNow.ToString("yyMMdd");

        //        // Ambil data terakhir untuk hari ini (tanpa ToString di query)
        //        var lastCode = _applicationDbContext.AsuransiPasiens
        //            .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //            .OrderByDescending(k => k.KodeAsuransiPasien)
        //            .FirstOrDefault();

        //        string kode;
        //        if (lastCode == null)
        //        {
        //            kode = $"AGM{setDateNow}0001";
        //        }
        //        else
        //        {
        //            var lastCodeTrim = lastCode.KodeAsuransiPasien.Substring(3, 6);

        //            if (lastCodeTrim != setDateNow)
        //            {
        //                kode = $"AGM{setDateNow}0001";
        //            }
        //            else
        //            {
        //                kode = $"AGM{setDateNow}" + (Convert.ToInt32(lastCode.KodeAsuransiPasien.Substring(9)) + 1).ToString("D4");
        //            }
        //        }

        //        // Cek Duplikasi
        //        var isDuplicate = _applicationDbContext.AsuransiPasiens
        //            .Any(c => c.KodeAsuransiPasien == kode && c.NamaAsuransiPasien == vm.NamaAsuransiPasien);

        //        if (isDuplicate)
        //        {
        //            return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
        //        }

        //        // Validate ModelState
        //        if (ModelState.IsValid)
        //        {
        //            // Simpan Data
        //            var data = new AsuransiPasien
        //            {
        //                AsuransiPasienId = Guid.NewGuid(),
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = UserActiveId,
        //                KodeAsuransiPasien = kode,
        //                NamaAsuransiPasien = vm.NamaAsuransiPasien
        //            };

        //            _applicationDbContext.AsuransiPasiens.Add(data);
        //            _applicationDbContext.SaveChanges();

        //            return Created("", new
        //            {
        //                message = "Tambah Data Berhasil || 201 Created",
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsuransiPasien(Guid id, [FromBody] AsuransiPasienViewModel request)
        {
            if (request == null || request.PasienId == null || request.AsuransiId == null)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
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
                // Periksa apakah pasien dan asuransi ada di database
                var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                                      .Any(p => p.PendaftaranPasienBaruId == request.PasienId);
                var asuransiExists = _applicationDbContext.Asuransis
                                      .Any(a => a.AsuransiId == request.AsuransiId);
                if (!pasienExists || !asuransiExists)
                {
                    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                }
                //validate model state
                if (ModelState.IsValid)
                {
                    var data = _applicationDbContext.AsuransiPasiens.Find(id);
                    if (data == null)
                    {
                        return NotFound(new { message = "Data tidak ditemukan." });
                    }
                    data.PasienId = request.PasienId;
                    data.AsuransiId = request.AsuransiId;
                    data.NoPolis = request.NoPolis;
                    data.Umur = request.Umur;

                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;

                    _applicationDbContext.AsuransiPasiens.Update(data);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Data berhasil diubah!", data });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsuransiPasien(Guid id)
        {
            var data = _applicationDbContext.AsuransiPasiens.Find(id);
            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
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

                data.IsDelete = true;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.DeleteBy = UserActiveId;

                _applicationDbContext.AsuransiPasiens.Update(data);
                _applicationDbContext.SaveChanges();
                return Ok(new
                {
                    message = "Data berhasil dihapus. || 200 OK",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedAsuransiPasien(
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
            var query = from ap in _applicationDbContext.AsuransiPasiens
                        join p in _applicationDbContext.PendaftaranPasienBarus on ap.PasienId equals p.PendaftaranPasienBaruId
                        join a in _applicationDbContext.Asuransis on ap.AsuransiId equals a.AsuransiId
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            ap.PasienId,
                            ap.AsuransiId,
                            ap.AsuransiPasienId,
                            NamaPasien = p.NamaLengkap,
                            NamaAsuransi = a.NamaAsuransi,
                            a.IsPKS,
                            ap.NoPolis,
                            ap.Umur
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.NamaPasien.Contains(search) || u.NamaAsuransi.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
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
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
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
