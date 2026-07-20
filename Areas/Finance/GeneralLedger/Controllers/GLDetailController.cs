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
    public class GLDetailController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GLDetailController> _logger;

        public GLDetailController(
            ApplicationDbContext context,
            ILogger<GLDetailController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int perPage = 10,
            Guid? glHeaderId = null,
            string? search = null)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from detail in _context.GLDetails

                join header in _context.GLHeaders
                    on detail.GLHeaderId equals header.GLHeaderId

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                join user in _context.UserActives
                    on detail.CreateBy equals user.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where detail.IsDelete == false &&
                      header.IsDelete == false &&
                      coa.IsDelete == false

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    header.GLKode,
                    header.NoRegistrasi,
                    header.SourceGL,
                    header.SourceTypeGL,
                    header.SourceNumber,
                    header.GLStatus,

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    detail.SourceItemType,
                    detail.SourceItemId,
                    detail.SourceItem,

                    detail.CostCenterId,
                    detail.CostCenterName,
                    detail.Keterangan,

                    detail.CreateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                };

            if (glHeaderId.HasValue)
            {
                query = query.Where(x =>
                    x.GLHeaderId == glHeaderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.GLKode!, keyword) ||
                    EF.Functions.ILike(x.KodeCOA!, keyword) ||
                    EF.Functions.ILike(x.NamaCOA!, keyword) ||
                    EF.Functions.ILike(x.SourceItem!, keyword) ||
                    EF.Functions.ILike(x.SourceNumber!, keyword));
            }

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDateTime)
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
                from detail in _context.GLDetails

                join header in _context.GLHeaders
                    on detail.GLHeaderId equals header.GLHeaderId

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                join user in _context.UserActives
                    on detail.CreateBy equals user.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where detail.GLDetailId == id &&
                      detail.IsDelete == false &&
                      header.IsDelete == false &&
                      coa.IsDelete == false

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    header.GLKode,
                    header.NoRegistrasi,
                    header.SourceGL,
                    header.SourceTypeGL,
                    header.SourceNumber,
                    header.GLStatus,

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    detail.SourceItemType,
                    detail.SourceItemId,
                    detail.SourceItem,

                    detail.CostCenterId,
                    detail.CostCenterName,
                    detail.Keterangan,

                    detail.CreateDateTime,
                    detail.UpdateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Detail tidak ditemukan"
                });
            }

            return Ok(new
            {
                message = "success",
                data
            });
        }

        // ================= GET BY HEADER =================
        [HttpGet("header/{glHeaderId}")]
        public async Task<IActionResult> GetByHeader(
            Guid glHeaderId)
        {
            var headerExists = await _context.GLHeaders
                .AnyAsync(x =>
                    x.GLHeaderId == glHeaderId &&
                    x.IsDelete == false);

            if (!headerExists)
            {
                return NotFound(new
                {
                    message = "GL Header tidak ditemukan"
                });
            }

            var data = await (
                from detail in _context.GLDetails

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                where detail.GLHeaderId == glHeaderId &&
                      detail.IsDelete == false &&
                      coa.IsDelete == false

                orderby detail.CreateDateTime

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,
                    detail.COAId,

                    coa.KodeCOA,
                    coa.NamaCOA,

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    detail.SourceItemType,
                    detail.SourceId,
                    detail.SourceNumber,
                    detail.SourceItemId,
                    detail.SourceItem,

                    detail.CostCenterId,
                    detail.CostCenterName,
                    detail.Keterangan
                })
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                summary = new
                {
                    totalDebit = data.Sum(x => x.NilaiDebit),
                    totalKredit = data.Sum(x => x.NilaiKredit),
                    balance =
                        data.Sum(x => x.NilaiDebit) -
                        data.Sum(x => x.NilaiKredit)
                }
            });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] GLDetail model)
        {
            if (model.GLHeaderId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "GLHeaderId wajib diisi"
                });
            }

            if (model.COAId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "COAId wajib diisi"
                });
            }

            if (model.NilaiDebit < 0 ||
                model.NilaiKredit < 0)
            {
                return BadRequest(new
                {
                    message =
                        "Nilai debit dan kredit tidak boleh negatif"
                });
            }

            if (model.NilaiDebit == 0 &&
                model.NilaiKredit == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Nilai debit atau kredit harus diisi"
                });
            }

            if (model.NilaiDebit > 0 &&
                model.NilaiKredit > 0)
            {
                return BadRequest(new
                {
                    message =
                        "Satu detail tidak boleh memiliki debit dan kredit sekaligus"
                });
            }

            var email = User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            var header = await _context.GLHeaders
                .FirstOrDefaultAsync(x =>
                    x.GLHeaderId == model.GLHeaderId &&
                    x.IsDelete == false);

            if (header == null)
            {
                return BadRequest(new
                {
                    message = "GL Header tidak ditemukan"
                });
            }

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x =>
                    x.COAId == model.COAId &&
                    x.IsDelete == false);

            if (coa == null)
            {
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });
            }

            if (coa.IsPostable != true)
            {
                return BadRequest(new
                {
                    message = "COA tersebut tidak dapat diposting"
                });
            }

            if (coa.IsValid != true)
            {
                return BadRequest(new
                {
                    message = "COA tersebut tidak valid"
                });
            }

            model.GLDetailId = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(model.SourceNumber))
            {
                model.SourceNumber = header.SourceNumber;
            }

            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.GLDetails.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Detail berhasil dibuat",
                data = new
                {
                    model.GLDetailId,
                    model.GLHeaderId
                }
            });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] GLDetail model)
        {
            var data = await _context.GLDetails
                .FirstOrDefaultAsync(x =>
                    x.GLDetailId == id &&
                    x.IsDelete == false);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Detail tidak ditemukan"
                });
            }

            if (model.NilaiDebit < 0 ||
                model.NilaiKredit < 0)
            {
                return BadRequest(new
                {
                    message =
                        "Nilai debit dan kredit tidak boleh negatif"
                });
            }

            if (model.NilaiDebit == 0 &&
                model.NilaiKredit == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Nilai debit atau kredit harus diisi"
                });
            }

            if (model.NilaiDebit > 0 &&
                model.NilaiKredit > 0)
            {
                return BadRequest(new
                {
                    message =
                        "Satu detail tidak boleh memiliki debit dan kredit sekaligus"
                });
            }

            var email = User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x =>
                    x.COAId == model.COAId &&
                    x.IsDelete == false);

            if (coa == null)
            {
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });
            }

            data.COAId = model.COAId;
            data.NilaiDebit = model.NilaiDebit;
            data.NilaiKredit = model.NilaiKredit;

            data.SourceItemType = model.SourceItemType;
            data.SourceId = model.SourceId;
            data.SourceNumber = model.SourceNumber;
            data.SourceItemId = model.SourceItemId;
            data.SourceItem = model.SourceItem;

            data.CostCenterId = model.CostCenterId;
            data.CostCenterName = model.CostCenterName;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user.UserActiveId;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Detail berhasil diperbarui"
            });
        }

        // ================= DELETE SOFT =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.GLDetails
                .FirstOrDefaultAsync(x =>
                    x.GLDetailId == id &&
                    x.IsDelete == false);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "GL Detail tidak ditemukan"
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

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "GL Detail berhasil dihapus"
            });
        }
    }
}