using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class GLHeaderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GLHeaderController> _logger;

        public GLHeaderController(
            ApplicationDbContext context,
            ILogger<GLHeaderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int perPage = 10,
            string? search = null)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from gl in _context.GLHeaders

                join user in _context.UserActives
                    on gl.CreateBy equals user.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where gl.IsDelete == false

                select new
                {
                    gl.GLHeaderId,
                    gl.GLKode,
                    gl.KunjunganId,
                    gl.NoRegistrasi,
                    gl.JenisKunjungan,
                    gl.PasienId,
                    gl.TglTransaksi,
                    gl.TglPosting,
                    gl.SourceGL,
                    gl.SourceTypeGL,
                    gl.SourceId,
                    gl.SourceNumber,
                    gl.GLStatus,
                    gl.Keterangan,
                    gl.CreateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.GLKode!, keyword) ||
                    EF.Functions.ILike(x.NoRegistrasi!, keyword) ||
                    EF.Functions.ILike(x.SourceNumber!, keyword) ||
                    EF.Functions.ILike(x.SourceGL!, keyword));
            }

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.TglPosting)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                pagination = new
                {
                    page,
                    perPage,
                    totalRows,
                    totalPages = (int)Math.Ceiling(
                        totalRows / (double)perPage)
                }
            });
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await (
                from gl in _context.GLHeaders

                join user in _context.UserActives
                    on gl.CreateBy equals user.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where gl.GLHeaderId == id &&
                      gl.IsDelete == false

                select new
                {
                    gl.GLHeaderId,
                    gl.GLKode,
                    gl.KunjunganId,
                    gl.NoRegistrasi,
                    gl.JenisKunjungan,
                    gl.PasienId,
                    gl.TglTransaksi,
                    gl.TglPosting,
                    gl.SourceGL,
                    gl.SourceTypeGL,
                    gl.SourceId,
                    gl.SourceNumber,
                    gl.GLStatus,
                    gl.Keterangan,
                    gl.CreateDateTime,
                    gl.UpdateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Header tidak ditemukan"
                });
            }

            return Ok(new
            {
                message = "success",
                data
            });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] GLHeader model)
        {
            if (model.KunjunganId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "KunjunganId wajib diisi"
                });
            }

            if (model.SourceId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "SourceId wajib diisi"
                });
            }

            var email = User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            /*
             * Sesuaikan nama DbSet dan nama field Kunjungan
             * dengan entity Kunjungan pada project Anda.
             */
            var kunjungan = await _context.Kunjungans
                .Where(x =>
                    x.KunjunganID == model.KunjunganId &&
                    x.IsDelete == false)
                .Select(x => new
                {
                    x.KunjunganID,
                    x.NoRegistrasi,
                    x.JenisKunjungan,
                    x.PasienId
                })
                .FirstOrDefaultAsync();

            if (kunjungan == null)
            {
                return BadRequest(new
                {
                    message = "Data kunjungan tidak ditemukan"
                });
            }

            var sourceExists = await _context.GLHeaders
                .AnyAsync(x =>
                    x.SourceId == model.SourceId &&
                    x.SourceGL == model.SourceGL &&
                    x.SourceTypeGL == model.SourceTypeGL &&
                    x.GLStatus != "REVERSED" &&
                    x.IsDelete == false);

            if (sourceExists)
            {
                return Conflict(new
                {
                    message =
                        "Transaksi tersebut sudah pernah dibuatkan GL"
                });
            }

            model.GLHeaderId = Guid.NewGuid();
            model.GLKode = await GenerateGLKode();

            model.NoRegistrasi = kunjungan.NoRegistrasi;
            model.JenisKunjungan = kunjungan.JenisKunjungan;
            model.PasienId = kunjungan.KunjunganID;

            model.TglPosting = DateTime.UtcNow;
            model.GLStatus = "POSTED";

            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.GLHeaders.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Header berhasil dibuat",
                data = new
                {
                    model.GLHeaderId,
                    model.GLKode
                }
            });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] GLHeader model)
        {
            var data = await _context.GLHeaders
                .FirstOrDefaultAsync(x =>
                    x.GLHeaderId == id &&
                    x.IsDelete == false);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Header tidak ditemukan"
                });
            }

            var email = User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            data.TglTransaksi = model.TglTransaksi;
            data.SourceGL = model.SourceGL;
            data.SourceTypeGL = model.SourceTypeGL;
            data.SourceId = model.SourceId;
            data.SourceNumber = model.SourceNumber;
            data.GLStatus = model.GLStatus;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user.UserActiveId;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Header berhasil diperbarui"
            });
        }

        // ================= DELETE SOFT =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.GLHeaders
                .FirstOrDefaultAsync(x =>
                    x.GLHeaderId == id &&
                    x.IsDelete == false);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Header tidak ditemukan"
                });
            }

            var email = User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            data.IsDelete = true;
            data.DeleteBy = user.UserActiveId;
            data.DeleteDateTime = DateTime.UtcNow;

            /*
             * Detail ikut soft delete ketika header dihapus.
             */
            var details = await _context.GLDetails
                .Where(x =>
                    x.GLHeaderId == id &&
                    x.IsDelete == false)
                .ToListAsync();

            foreach (var detail in details)
            {
                detail.IsDelete = true;
                detail.DeleteBy = user.UserActiveId;
                detail.DeleteDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Header berhasil dihapus"
            });
        }

        // ================= GENERATE GL KODE =================
        private async Task<string> GenerateGLKode()
        {
            var nowJakarta = DateTime.UtcNow.AddHours(7);

            var kodeDalamBulan = await _context.GLHeaders
                .Where(x =>
                    x.TglPosting.Year == nowJakarta.Year &&
                    x.TglPosting.Month == nowJakarta.Month)
                .Select(x => x.GLKode)
                .ToListAsync();

            var sequenceTerakhir = kodeDalamBulan
                .Select(x =>
                {
                    if (string.IsNullOrWhiteSpace(x))
                        return 0;

                    var split = x.Split('-');

                    if (split.Length < 3)
                        return 0;

                    return int.TryParse(split.Last(), out var nomor)
                        ? nomor
                        : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var sequenceBaru = sequenceTerakhir + 1;

            return $"GL-{nowJakarta:yyMMdd}-{sequenceBaru:00000}";
        }
    }
}