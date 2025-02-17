using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class NegaraController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NegaraController
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

        // Get : api/Negara
        [HttpGet]
        public async Task<IActionResult> GetAllNegara(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _context.Negaras
                        join u in _context.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreatedDate = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            NegaraId = a.NegaraId,
                            KodeNegara = a.KodeNegara,
                            NamaNegara = a.NamaNegara

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

        // Get : api/Negara/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Negaras.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // Post : api/Negara
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NegaraViewModel model)
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
                var lastCode = _context.Negaras
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeNegara)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"NGR{setDateNow}0001";

                }
                else
                {
                    var lastCodeTrim = lastCode.KodeNegara.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"NGR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"NGR{setDateNow}" + (Convert.ToInt32(lastCode.KodeNegara.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _context.Negaras
                    .Any(c => c.KodeNegara == kode && c.NamaNegara == model.NamaNegara);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                if (ModelState.IsValid)
                {
                    var negara = new Negara
                    {
                        NegaraId = Guid.NewGuid(),
                        KodeNegara = kode,
                        NamaNegara = model.NamaNegara,
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = UserActiveId,
                        UpdateDateTime = DateTimeOffset.Now,
                        IsDelete = false
                    };
                    _context.Negaras.Add(negara);
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

        // update negara
        // Put : api/Negara/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] NegaraViewModel model)
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
                var data = _context.Negaras.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //update data
                data.NamaNegara = model.NamaNegara ?? data.NamaNegara;


                data.UpdateDateTime = DateTimeOffset.Now;
                data.UpdateBy = UserActiveId;

                _context.Negaras.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil diupdate..." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
          

        }

        // Delete : api/Negara/{id}
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

                // **Cari Data **
                var data = _context.Negaras.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.Now;
                data.IsDelete = true;

                _context.Negaras.Update(data);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
