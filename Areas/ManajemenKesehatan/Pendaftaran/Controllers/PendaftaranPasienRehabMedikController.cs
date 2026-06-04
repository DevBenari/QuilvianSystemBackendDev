using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
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
    public class PendaftaranPasienRehabMedikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienRehabMedikController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienRehabMedikController
            (ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ILogger<PendaftaranPasienRehabMedikController> logger, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPdfPasienRehabMedik(int page = 1, int perPage = 10)
        {
            // validasi pagging
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _context.PendaftaranPasienRehabMediks
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienRehabMedikId = a.PendaftaranPasienRehabMedikId,
                            KodePdfPasienRehabMedik = a.KodePdfPasienRehabMedik,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            TanggalLahir = a.TanggalLahir,
                            TanggalPendaftaran = a.TanggalPendaftaran,
                            NamaPasien = a.NamaPasien,
                            AlamatPasien = a.AlamatPasien,
                            NoTelpPasien = a.NoTelpPasien,
                            JenisKelamin = a.JenisKelamin,
                            Email = a.Email,
                            Title = a.Title,
                            Provinsi = a.Provinsi,
                            KabupatenKota = a.KabupatenKota,
                            Kecamatan = a.Kecamatan,
                            TipePasien = a.TipePasien,
                            Asuransi = a.Asuransi,
                            DokterPemeriksa = a.DokterPemeriksa,
                            KodeMember = a.KodeMember,
                            TipePemeriksaan = a.TipePemeriksaan,
                            DiagnosaAwal = a.DiagnosaAwal,
                            TipeRujukan = a.TipeRujukan,
                            JenisKonsul = a.JenisKonsul,
                            NamaRSRujukan = a.NamaRSRujukan
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
            var record = await _context.PendaftaranPasienRehabMediks.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PendaftaranPasienRehabMedikViewModel vm)
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
                var lastCode = _context.PendaftaranPasienRehabMediks
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePdfPasienRehabMedik)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PRM{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KodePdfPasienRehabMedik.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PRM{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PRM{setDateNow}" + (Convert.ToInt32(lastCode.KodePdfPasienRehabMedik.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _context.PendaftaranPasienRehabMediks
                    .Any(c => c.KodePdfPasienRehabMedik == kode);

                if (ModelState.IsValid)
                {
                    var data = new PendaftaranPasienRehabMedik
                    {
                        PendaftaranPasienRehabMedikId = Guid.NewGuid(),
                        KodePdfPasienRehabMedik = kode,
                        PasienId = vm.PasienId,
                        NoRekamMedis = vm.NoRekamMedis,
                        TanggalLahir = vm.TanggalLahir,
                        TanggalPendaftaran = vm.TanggalPendaftaran,
                        NamaPasien = vm.NamaPasien,
                        JenisKelamin = vm.JenisKelamin,
                        Email = vm.Email,
                        Title = vm.Title,
                        Provinsi = vm.Provinsi,
                        KabupatenKota = vm.KabupatenKota,
                        Kecamatan = vm.Kecamatan,
                        TipePasien = vm.TipePasien,
                        Asuransi = vm.Asuransi,
                        DokterPemeriksa = vm.DokterPemeriksa,
                        KodeMember = vm.KodeMember,
                        TipePemeriksaan = vm.TipePemeriksaan,
                        DiagnosaAwal = vm.DiagnosaAwal,
                        TipeRujukan = vm.TipeRujukan,
                        JenisKonsul = vm.JenisKonsul,
                        NamaRSRujukan = vm.NamaRSRujukan,

                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        IsDelete = false,

                    };
                    _context.PendaftaranPasienRehabMediks.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] PendaftaranPasienRehabMedikViewModel vm)
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
                var data = _context.PendaftaranPasienRehabMediks.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.PasienId = vm.PasienId;
                data.NoRekamMedis = vm.NoRekamMedis;
                data.TanggalLahir = vm.TanggalLahir;
                data.TanggalPendaftaran = vm.TanggalPendaftaran;
                data.NamaPasien = vm.NamaPasien;
                data.JenisKelamin = vm.JenisKelamin;
                data.Email = vm.Email;
                data.Title = vm.Title;
                data.Provinsi = vm.Provinsi;
                data.KabupatenKota = vm.KabupatenKota;
                data.Kecamatan = vm.Kecamatan;
                data.TipePasien = vm.TipePasien;
                data.Asuransi = vm.Asuransi;
                data.DokterPemeriksa = vm.DokterPemeriksa;
                data.KodeMember = vm.KodeMember;
                data.TipePemeriksaan = vm.TipePemeriksaan;
                data.DiagnosaAwal = vm.DiagnosaAwal;
                data.TipeRujukan = vm.TipeRujukan;
                data.JenisKonsul = vm.JenisKonsul;
                data.NamaRSRujukan = vm.NamaRSRujukan;

                data.UpdateDateTime = DateTimeOffset.UtcNow; // Tambahkan kolom UpdatedAt di model jika perlu
                data.UpdateBy = UserActiveId; // Pastikan ada kolom UpdateBy jika ingin menyimpan informasi user yang mengupdate

                _context.PendaftaranPasienRehabMediks.Update(data);
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
                var data = _context.PendaftaranPasienRehabMediks.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _context.PendaftaranPasienRehabMediks.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedPendaftaranPasienRehabMedik(
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
            var query = from a in _context.PendaftaranPasienRehabMediks
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienRehabMedikId = a.PendaftaranPasienRehabMedikId,
                            KodePdfPasienRehabMedik = a.KodePdfPasienRehabMedik,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            TanggalLahir = a.TanggalLahir,
                            TanggalPendaftaran = a.TanggalPendaftaran,
                            NamaPasien = a.NamaPasien,
                            AlamatPasien = a.AlamatPasien,
                            NoTelpPasien = a.NoTelpPasien,
                            JenisKelamin = a.JenisKelamin,
                            Email = a.Email,
                            Title = a.Title,
                            Provinsi = a.Provinsi,
                            KabupatenKota = a.KabupatenKota,
                            Kecamatan = a.Kecamatan,
                            TipePasien = a.TipePasien,
                            Asuransi = a.Asuransi,
                            DokterPemeriksa = a.DokterPemeriksa,
                            KodeMember = a.KodeMember,
                            TipePemeriksaan = a.TipePemeriksaan,
                            DiagnosaAwal = a.DiagnosaAwal,
                            TipeRujukan = a.TipeRujukan,
                            JenisKonsul = a.JenisKonsul,
                            NamaRSRujukan = a.NamaRSRujukan
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.NamaPasien.Contains(search) || u.TipePemeriksaan.Contains(search) || u.DokterPemeriksa.Contains(search)
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
                    "TipePemeriksaan" => query.OrderByDescending(u => u.TipePemeriksaan),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "DokterPemeriksa" => query.OrderByDescending(u => u.DokterPemeriksa),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "TipePemeriksaan" => query.OrderByDescending(u => u.TipePemeriksaan),
                    "NamaPasien" => query.OrderByDescending(u => u.NamaPasien),
                    "DokterPemeriksa" => query.OrderByDescending(u => u.DokterPemeriksa),
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
