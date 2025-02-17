using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Migrations;
using QuilvianSystemBackendDev.Repositories;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZXing.QrCode.Internal;
using MessagePack;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DokterController
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PendaftaranPasienBaruController> logger,
            IWebHostEnvironment webHostEnvironment


            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAllDokter(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _context.Dokters
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreatedDate = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DokterId = a.DokterId,
                            KdDokter = a.KdDokter,
                            NmDokter = a.NmDokter,
                            Sip = a.Sip,
                            Str = a.Str,
                            TglSip = a.TglSip,
                            TglStr = a.TglStr,
                            PanggilDokter = a.PanggilDokter,
                            Nik = a.Nik,
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

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Dokter
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DokterViewModel model)
        {
            if (model == null || !ModelState.IsValid)
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

                var dateNow = DateTimeOffset.Now;
                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _context.Dokters
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KdDokter)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"DKR{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KdDokter.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"DKR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"DKR{setDateNow}" + (Convert.ToInt32(lastCode.KdDokter.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _context.Dokters
                    .Any(c => c.KdDokter == kode && c.NmDokter == model.NmDokter);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                if (ModelState.IsValid)
                {
                    var dokter = new Dokter
                    {
                        DokterId = Guid.NewGuid(),
                        NmDokter = model.NmDokter,
                        Sip = model.Sip,
                        Str = model.Str,
                        TglSip = model.TglSip,
                        TglStr = model.TglStr,
                        PanggilDokter = model.PanggilDokter,
                        Nik = model.Nik,
                        KdDokter = kode,
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy =UserActiveId,
                        IsDelete = false
                    };
                    _context.Dokters.Add(dokter);
                    _context.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
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

        // PUT: api/Dokter/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DokterViewModel model)
        {
            if (model == null || !ModelState.IsValid)
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
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.NmDokter = model.NmDokter ?? data.NmDokter;
                data.Sip = model.Sip ?? data.Sip;
                data.Str = model.Str ?? data.Str;
                data.TglSip = model.TglSip ?? data.TglSip;
                data.TglStr = model.TglStr ?? data.TglStr;
                data.PanggilDokter = model.PanggilDokter ?? data.PanggilDokter;
                data.Nik = model.Nik ?? data.Nik;

                data.UpdateDateTime = DateTimeOffset.Now;
                data.UpdateBy = UserActiveId;

                _context.Dokters.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil diupdate..." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/Dokter/{id}
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
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.Now;
                data.IsDelete = true;

                _context.Dokters.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // Pagination
        [HttpGet("paged")]
        public IActionResult PagedDokter(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "asc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "StartDate tidak boleh lebih besar dari EndDate." });
            }

            var query = _context.Dokters.Where(a => a.IsDelete == false).AsQueryable();

            // 🔍 Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.KdDokter.Contains(search) ||
                                         u.NmDokter.Contains(search) ||
                                         u.Str.Contains(search));
            }

            // 📅 Filter berdasarkan daterange
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u => u.CreateDateTime.Date >= startDate.Value.Date &&
                                         u.CreateDateTime.Date <= endDate.Value.Date);
            }

            

            // Sorting Data
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, orderBy))
                    : query.OrderBy(e => EF.Property<object>(e, orderBy));
            }

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
