using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PendaftaranPasienAmbulanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienAmbulanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienAmbulanController
            (ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ILogger<PendaftaranPasienAmbulanController> logger, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPendaftaranPasienAmbulan(int page = 1, int perPage = 10)
        {
            // validasi pagging
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _context.PendaftaranPasienAmbulans
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienAmbulanId = a.PendaftaranPasienAmbulanId,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            NamaPasien = a.NamaPasien,
                            AlamatPasien = a.AlamatPasien,
                            NoTelpPasien = a.NoTelpPasien,
                            JenisKelamin = a.JenisKelamin,
                            TanggalLahir = a.TanggalLahir,
                            Title = a.Title,
                            LayananAmbulan = a.LayananAmbulan,
                            DaerahTujuan = a.DaerahTujuan,
                            KelebihanJarak = a.KelebihanJarak,
                            KelebihanWaktu = a.KelebihanWaktu,
                            JumlahParamedis = a.JumlahParamedis,
                            IsAntarJemput = a.IsAntarJemput,
                            Catatan = a.Catatan
                        };

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
            var record = await _context.PendaftaranPasienAmbulans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PendaftaranPasienAmbulanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                 var dateNow = DateTime.UtcNow;;
                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.PendaftaranPasienAmbulans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePdfPasienAmbulan)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PAM{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KodePdfPasienAmbulan.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PAM{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PAM{setDateNow}" + (Convert.ToInt32(lastCode.KodePdfPasienAmbulan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _context.PendaftaranPasienAmbulans
                    .Any(c => c.KodePdfPasienAmbulan == kode);

                if (ModelState.IsValid)
                {
                    var data = new PendaftaranPasienAmbulan
                    {
                        PendaftaranPasienAmbulanId = Guid.NewGuid(),
                        PasienId = vm.PasienId,
                        KodePdfPasienAmbulan = kode,
                        NoRekamMedis = vm.NoRekamMedis,
                        NamaPasien = vm.NamaPasien,
                        AlamatPasien = vm.AlamatPasien,
                        NoTelpPasien = vm.NoTelpPasien,
                        JenisKelamin = vm.JenisKelamin,
                        TanggalLahir = vm.TanggalLahir,
                        Title = vm.Title,
                        LayananAmbulan = vm.LayananAmbulan,
                        DaerahTujuan = vm.DaerahTujuan,
                        KelebihanJarak = vm.KelebihanJarak,
                        KelebihanWaktu = vm.KelebihanWaktu,
                        JumlahParamedis = vm.JumlahParamedis,
                        IsAntarJemput = vm.IsAntarJemput,
                        Catatan = vm.Catatan,

                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false,

                    };
                    _context.PendaftaranPasienAmbulans.Add(data);
                    _context.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        //uploadFotoUrl = fotoPath != null ? $"{Request.Scheme}://{Request.Host}{fotoPath}" : null
                    });

                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PendaftaranPasienAmbulanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data
                var data = _context.PendaftaranPasienAmbulans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.PasienId = vm.PasienId;
                data.NoRekamMedis = vm.NoRekamMedis;
                data.NamaPasien = vm.NamaPasien;
                data.AlamatPasien = vm.AlamatPasien;
                data.NoTelpPasien = vm.NoTelpPasien;
                data.JenisKelamin = vm.JenisKelamin;
                data.TanggalLahir = vm.TanggalLahir;
                data.Title = vm.Title;
                data.LayananAmbulan = vm.LayananAmbulan;
                data.DaerahTujuan = vm.DaerahTujuan;
                data.KelebihanJarak = vm.KelebihanJarak;
                data.KelebihanWaktu = vm.KelebihanWaktu;
                data.JumlahParamedis = vm.JumlahParamedis;
                data.IsAntarJemput = vm.IsAntarJemput;
                data.Catatan = vm.Catatan;
                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;

                _context.PendaftaranPasienAmbulans.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil diupdate..." });
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
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Dokter**
                var data = _context.PendaftaranPasienAmbulans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _context.PendaftaranPasienAmbulans.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedPendaftaranPasienAmbulan(
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
            var query = from a in _context.PendaftaranPasienAmbulans
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienAmbulanId = a.PendaftaranPasienAmbulanId,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            NamaPasien = a.NamaPasien,
                            AlamatPasien = a.AlamatPasien,
                            NoTelpPasien = a.NoTelpPasien,
                            JenisKelamin = a.JenisKelamin,
                            TanggalLahir = a.TanggalLahir,
                            Title = a.Title,
                            LayananAmbulan = a.LayananAmbulan,
                            DaerahTujuan = a.DaerahTujuan,
                            KelebihanJarak = a.KelebihanJarak,
                            KelebihanWaktu = a.KelebihanWaktu,
                            JumlahParamedis = a.JumlahParamedis,
                            IsAntarJemput = a.IsAntarJemput,
                            Catatan = a.Catatan
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.NamaPasien.Contains(search) || u.LayananAmbulan.Contains(search) || u.DaerahTujuan.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u =>
                    u.CreateDateTime.Date >= startDate.Value.Date &&
                    u.CreateDateTime.Date <= endDate.Value.Date
                );
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
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "LayananAmbulan" => query.OrderByDescending(u => u.LayananAmbulan),
                    "DaerahTujuan" => query.OrderByDescending(u => u.DaerahTujuan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "LayananAmbulan" => query.OrderByDescending(u => u.LayananAmbulan),
                    "DaerahTujuan" => query.OrderByDescending(u => u.DaerahTujuan),
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
