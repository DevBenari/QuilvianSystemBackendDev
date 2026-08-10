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


        // ============================================================
        // GET ALL
        // ============================================================

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

            if (perPage > 100)
                perPage = 100;


            var query =
                from detail in _context.GLDetails

                join header in _context.GLHeaders
                    on detail.GLHeaderId equals header.GLHeaderId

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                join userData in _context.UserActives
                    on detail.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where
                    detail.IsDelete == false &&
                    header.IsDelete == false &&
                    coa.IsDelete == false

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    // ================================================
                    // HEADER
                    // ================================================

                    header.GLKode,
                    header.NoRegistrasi,
                    header.SourceGL,
                    header.SourceTypeGL,
                    header.SourceNumber,
                    header.GLStatus,

                    // ================================================
                    // RECURRING JOURNAL DETAIL
                    // ================================================

                    detail.DetailTempRJId,
                    detail.RoleSetupCOA,

                    // ================================================
                    // COA
                    // ================================================

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    // ================================================
                    // NOMINAL
                    // ================================================

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    // ================================================
                    // SOURCE ITEM
                    // ================================================

                    detail.SourceItemType,
                    detail.SourceId,
                    //detail.SourceNumber,
                    detail.SourceItemId,
                    detail.SourceItem,

                    // ================================================
                    // COST CENTER
                    // ================================================

                    detail.CostCenterId,
                    detail.CostCenterName,

                    detail.Keterangan,

                    // ================================================
                    // AUDIT
                    // ================================================

                    detail.CreateDateTime,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null
                };


            // ========================================================
            // FILTER HEADER
            // ========================================================

            if (glHeaderId.HasValue &&
                glHeaderId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.GLHeaderId == glHeaderId.Value);
            }


            // ========================================================
            // SEARCH
            // ========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword =
                    $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(
                        x.GLKode!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.KodeCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.NamaCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.RoleSetupCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceItemType!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceItem!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceNumber!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.CostCenterName!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.Keterangan!,
                        keyword));
            }


            var totalRows =
                await query.CountAsync();


            var data =
                await query
                    .OrderByDescending(
                        x => x.CreateDateTime)
                    .Skip(
                        (page - 1) * perPage)
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

                    totalPages =
                        (int)Math.Ceiling(
                            totalRows /
                            (double)perPage)
                }
            });
        }


        // ============================================================
        // GET BY ID
        // ============================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    message =
                        "GLDetailId tidak valid"
                });
            }


            var data = await (
                from detail in _context.GLDetails

                join header in _context.GLHeaders
                    on detail.GLHeaderId equals header.GLHeaderId

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                join userData in _context.UserActives
                    on detail.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where
                    detail.GLDetailId == id &&
                    detail.IsDelete == false &&
                    header.IsDelete == false &&
                    coa.IsDelete == false

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    // ================================================
                    // HEADER
                    // ================================================

                    header.GLKode,
                    header.NoRegistrasi,
                    header.SourceGL,
                    header.SourceTypeGL,
                    header.SourceNumber,
                    header.GLStatus,

                    // ================================================
                    // RECURRING JOURNAL DETAIL
                    // ================================================

                    detail.DetailTempRJId,
                    detail.RoleSetupCOA,

                    // ================================================
                    // COA
                    // ================================================

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    // ================================================
                    // NOMINAL
                    // ================================================

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    // ================================================
                    // SOURCE
                    // ================================================

                    detail.SourceItemType,
                    detail.SourceId,
                    //detail.SourceNumber,
                    detail.SourceItemId,
                    detail.SourceItem,

                    // ================================================
                    // COST CENTER
                    // ================================================

                    detail.CostCenterId,
                    detail.CostCenterName,

                    detail.Keterangan,

                    // ================================================
                    // AUDIT
                    // ================================================

                    detail.CreateDateTime,
                    detail.UpdateDateTime,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null
                })
                .FirstOrDefaultAsync();


            if (data == null)
            {
                return NotFound(new
                {
                    message =
                        "GL Detail tidak ditemukan"
                });
            }


            return Ok(new
            {
                message = "success",
                data
            });
        }


        // ============================================================
        // GET BY HEADER
        // ============================================================

        [HttpGet("header/{glHeaderId}")]
        public async Task<IActionResult> GetByHeader(
            Guid glHeaderId)
        {
            if (glHeaderId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message =
                        "GLHeaderId tidak valid"
                });
            }


            var headerExists =
                await _context.GLHeaders
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.GLHeaderId == glHeaderId &&
                        x.IsDelete == false);


            if (!headerExists)
            {
                return NotFound(new
                {
                    message =
                        "GL Header tidak ditemukan"
                });
            }


            var data = await (
                from detail in _context.GLDetails

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                where
                    detail.GLHeaderId == glHeaderId &&
                    detail.IsDelete == false &&
                    coa.IsDelete == false

                orderby detail.CreateDateTime

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    // ================================================
                    // RECURRING JOURNAL DETAIL
                    // ================================================

                    detail.DetailTempRJId,
                    detail.RoleSetupCOA,

                    // ================================================
                    // COA
                    // ================================================

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    // ================================================
                    // NOMINAL
                    // ================================================

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    // ================================================
                    // SOURCE
                    // ================================================

                    detail.SourceItemType,
                    detail.SourceId,
                    detail.SourceNumber,
                    detail.SourceItemId,
                    detail.SourceItem,

                    // ================================================
                    // COST CENTER
                    // ================================================

                    detail.CostCenterId,
                    detail.CostCenterName,

                    detail.Keterangan
                })
                .ToListAsync();


            var totalDebit =
                data.Sum(x => x.NilaiDebit);

            var totalKredit =
                data.Sum(x => x.NilaiKredit);


            return Ok(new
            {
                message = "success",

                data,

                summary = new
                {
                    totalDebit,
                    totalKredit,

                    balance =
                        totalDebit -
                        totalKredit
                }
            });
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] GLDetail model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Data GL Detail tidak valid"
                    });
                }


                // ====================================================
                // VALIDASI HEADER
                // ====================================================

                if (model.GLHeaderId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "GLHeaderId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI DETAIL TEMP RECURRING JOURNAL
                // ====================================================

                if (model.DetailTempRJId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "DetailTempRJId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI COA
                // ====================================================

                if (model.COAId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "COAId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI NOMINAL
                // ====================================================

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


                // ====================================================
                // USER LOGIN
                // ====================================================

                var email =
                    User
                        .FindFirst(
                            ClaimTypes.NameIdentifier)?
                        .Value;


                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi"
                    });
                }


                var user =
                    await _context.UserActives
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Email == email);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // GET HEADER
                // ====================================================

                var header =
                    await _context.GLHeaders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.GLHeaderId ==
                                model.GLHeaderId &&

                            x.IsDelete == false);


                if (header == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "GL Header tidak ditemukan"
                    });
                }


                // ====================================================
                // GET RECURRING JOURNAL DETAIL
                //
                // DetailTempRJId ->
                // Fin_DetailTempRecurringJournal
                //
                // RoleSetupCOA diambil dari sini.
                // Frontend tidak menentukan RoleSetupCOA.
                // ====================================================

                var recurringDetail =
                    await _context
                        .RecurringJournalDetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.DetailTempRJId ==
                                model.DetailTempRJId &&

                            x.IsDelete == false);


                if (recurringDetail == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Detail Recurring Journal tidak ditemukan"
                    });
                }


                // ====================================================
                // ROLE SETUP COA
                // GET BERDASARKAN DetailTempRJId
                // ====================================================

                model.RoleSetupCOA =
                    recurringDetail
                        .RoleSetupCOA?
                        .Trim();


                // ====================================================
                // GET COA
                // ====================================================

                var coa =
                    await _context.MasterCoas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.COAId ==
                                model.COAId &&

                            x.IsDelete == false);


                if (coa == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tidak ditemukan"
                    });
                }


                if (coa.IsPostable != true)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tersebut tidak dapat diposting"
                    });
                }


                if (coa.IsValid != true)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tersebut tidak valid"
                    });
                }


                // ====================================================
                // CREATE GL DETAIL
                // ====================================================

                model.GLDetailId =
                    Guid.NewGuid();


                if (string.IsNullOrWhiteSpace(
                    model.SourceNumber))
                {
                    model.SourceNumber =
                        header.SourceNumber;
                }
                else
                {
                    model.SourceNumber =
                        model.SourceNumber.Trim();
                }


                model.SourceItemType =
                    model.SourceItemType?.Trim();


                model.SourceId =
                    model.SourceId?.Trim();


                model.SourceItem =
                    model.SourceItem?.Trim();


                model.CostCenterName =
                    model.CostCenterName?.Trim();


                model.Keterangan =
                    model.Keterangan?.Trim();


                model.CreateBy =
                    user.UserActiveId;


                model.CreateDateTime =
                    DateTime.UtcNow;


                model.IsDelete =
                    false;


                _context.GLDetails.Add(model);


                await _context.SaveChangesAsync();


                return Created("", new
                {
                    message =
                        "GL Detail berhasil dibuat",

                    data = new
                    {
                        model.GLDetailId,
                        model.GLHeaderId,

                        model.DetailTempRJId,
                        model.RoleSetupCOA,

                        model.COAId,

                        model.NilaiDebit,
                        model.NilaiKredit,

                        model.SourceItemType,
                        model.SourceId,
                        model.SourceNumber,
                        model.SourceItemId,
                        model.SourceItem,

                        model.CostCenterId,
                        model.CostCenterName,

                        model.Keterangan
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                var detail =
                    ex.GetBaseException().Message;


                _logger.LogError(
                    ex,
                    "Gagal menyimpan GL Detail. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal menyimpan GL Detail",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat membuat GL Detail");


                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal",

                    detail =
                        ex.GetBaseException().Message
                });
            }
        }


        // ============================================================
        // UPDATE
        // ============================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] GLDetail model)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "GLDetailId tidak valid"
                    });
                }


                var data =
                    await _context.GLDetails
                        .FirstOrDefaultAsync(x =>
                            x.GLDetailId == id &&
                            x.IsDelete == false);


                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "GL Detail tidak ditemukan"
                    });
                }


                // ====================================================
                // VALIDASI DETAIL TEMP RJ
                // ====================================================

                if (model.DetailTempRJId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "DetailTempRJId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI COA
                // ====================================================

                if (model.COAId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "COAId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI NILAI
                // ====================================================

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


                // ====================================================
                // USER LOGIN
                // ====================================================

                var email =
                    User
                        .FindFirst(
                            ClaimTypes.NameIdentifier)?
                        .Value;


                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi"
                    });
                }


                var user =
                    await _context.UserActives
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Email == email);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // GET RECURRING JOURNAL DETAIL
                // ====================================================

                var recurringDetail =
                    await _context
                        .RecurringJournalDetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.DetailTempRJId ==
                                model.DetailTempRJId &&

                            x.IsDelete == false);


                if (recurringDetail == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Detail Recurring Journal tidak ditemukan"
                    });
                }


                // ====================================================
                // GET COA
                // ====================================================

                var coa =
                    await _context.MasterCoas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.COAId ==
                                model.COAId &&

                            x.IsDelete == false);


                if (coa == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tidak ditemukan"
                    });
                }


                if (coa.IsPostable != true)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tersebut tidak dapat diposting"
                    });
                }


                if (coa.IsValid != true)
                {
                    return BadRequest(new
                    {
                        message =
                            "COA tersebut tidak valid"
                    });
                }


                // ====================================================
                // UPDATE RECURRING DETAIL
                // ====================================================

                data.DetailTempRJId =
                    recurringDetail.DetailTempRJId;


                /*
                 * RoleSetupCOA tidak diambil dari frontend.
                 * Selalu mengikuti DetailTempRJId.
                 */

                data.RoleSetupCOA =
                    recurringDetail
                        .RoleSetupCOA?
                        .Trim();


                // ====================================================
                // UPDATE COA + NILAI
                // ====================================================

                data.COAId =
                    model.COAId;


                data.NilaiDebit =
                    model.NilaiDebit;


                data.NilaiKredit =
                    model.NilaiKredit;


                // ====================================================
                // UPDATE SOURCE
                // ====================================================

                data.SourceItemType =
                    model.SourceItemType?.Trim();


                data.SourceId =
                    model.SourceId?.Trim();


                data.SourceNumber =
                    model.SourceNumber?.Trim();


                data.SourceItemId =
                    model.SourceItemId;


                data.SourceItem =
                    model.SourceItem?.Trim();


                // ====================================================
                // UPDATE COST CENTER
                // ====================================================

                data.CostCenterId =
                    model.CostCenterId;


                data.CostCenterName =
                    model.CostCenterName?.Trim();


                data.Keterangan =
                    model.Keterangan?.Trim();


                // ====================================================
                // AUDIT
                // ====================================================

                data.UpdateBy =
                    user.UserActiveId;


                data.UpdateDateTime =
                    DateTime.UtcNow;


                await _context.SaveChangesAsync();


                return Ok(new
                {
                    message =
                        "GL Detail berhasil diperbarui",

                    data = new
                    {
                        data.GLDetailId,
                        data.GLHeaderId,

                        data.DetailTempRJId,
                        data.RoleSetupCOA,

                        data.COAId,

                        data.NilaiDebit,
                        data.NilaiKredit,

                        data.SourceItemType,
                        data.SourceId,
                        data.SourceNumber,
                        data.SourceItemId,
                        data.SourceItem,

                        data.CostCenterId,
                        data.CostCenterName,

                        data.Keterangan,

                        data.UpdateDateTime
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                var detail =
                    ex.GetBaseException().Message;


                _logger.LogError(
                    ex,
                    "Gagal memperbarui GL Detail. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal memperbarui GL Detail",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat memperbarui GL Detail");


                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal",

                    detail =
                        ex.GetBaseException().Message
                });
            }
        }


        // ============================================================
        // DELETE SOFT
        // ============================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "GLDetailId tidak valid"
                    });
                }


                var data =
                    await _context.GLDetails
                        .FirstOrDefaultAsync(x =>
                            x.GLDetailId == id &&
                            x.IsDelete == false);


                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "GL Detail tidak ditemukan"
                    });
                }


                // ====================================================
                // USER LOGIN
                // ====================================================

                var email =
                    User
                        .FindFirst(
                            ClaimTypes.NameIdentifier)?
                        .Value;


                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi"
                    });
                }


                var user =
                    await _context.UserActives
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Email == email);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // SOFT DELETE
                // ====================================================

                data.IsDelete =
                    true;


                data.DeleteBy =
                    user.UserActiveId;


                data.DeleteDateTime =
                    DateTime.UtcNow;


                await _context.SaveChangesAsync();


                return Ok(new
                {
                    message =
                        "GL Detail berhasil dihapus"
                });
            }
            catch (DbUpdateException ex)
            {
                var detail =
                    ex.GetBaseException().Message;


                _logger.LogError(
                    ex,
                    "Gagal menghapus GL Detail. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal menghapus GL Detail",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat menghapus GL Detail");


                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal",

                    detail =
                        ex.GetBaseException().Message
                });
            }
        }


        // ============================================================
        // PAGED
        // ============================================================

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? glHeaderId = null,
            string? search = null)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            if (perPage > 100)
                perPage = 100;


            var query =
                from detail in _context.GLDetails

                join header in _context.GLHeaders
                    on detail.GLHeaderId equals header.GLHeaderId

                join coa in _context.MasterCoas
                    on detail.COAId equals coa.COAId

                join userData in _context.UserActives
                    on detail.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where
                    detail.IsDelete == false &&
                    header.IsDelete == false &&
                    coa.IsDelete == false

                select new
                {
                    detail.GLDetailId,
                    detail.GLHeaderId,

                    // ================================================
                    // HEADER
                    // ================================================

                    header.GLKode,
                    header.KunjunganId,
                    header.NoRegistrasi,
                    header.JenisKunjungan,
                    header.PasienId,
                    header.TglTransaksi,
                    header.TglPosting,
                    header.SourceGL,
                    header.SourceTypeGL,

                    HeaderSourceId =
                        header.SourceId,

                    HeaderSourceNumber =
                        header.SourceNumber,

                    header.GLStatus,

                    // ================================================
                    // RECURRING JOURNAL DETAIL
                    // ================================================

                    detail.DetailTempRJId,
                    detail.RoleSetupCOA,

                    // ================================================
                    // COA
                    // ================================================

                    detail.COAId,
                    coa.KodeCOA,
                    coa.NamaCOA,

                    // ================================================
                    // NILAI
                    // ================================================

                    detail.NilaiDebit,
                    detail.NilaiKredit,

                    // ================================================
                    // SOURCE DETAIL
                    // ================================================

                    detail.SourceItemType,
                    detail.SourceId,
                    detail.SourceNumber,
                    detail.SourceItemId,
                    detail.SourceItem,

                    // ================================================
                    // COST CENTER
                    // ================================================

                    detail.CostCenterId,
                    detail.CostCenterName,

                    detail.Keterangan,

                    // ================================================
                    // AUDIT
                    // ================================================

                    detail.CreateDateTime,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null
                };


            // ========================================================
            // FILTER HEADER
            // ========================================================

            if (glHeaderId.HasValue &&
                glHeaderId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.GLHeaderId ==
                        glHeaderId.Value);
            }


            // ========================================================
            // SEARCH
            // ========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword =
                    $"%{search.Trim()}%";


                query = query.Where(x =>
                    EF.Functions.ILike(
                        x.GLKode!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.NoRegistrasi!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.JenisKunjungan!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceGL!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceTypeGL!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.HeaderSourceNumber!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.GLStatus!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.KodeCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.NamaCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.RoleSetupCOA!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceItemType!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceNumber!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceItem!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.CostCenterName!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.Keterangan!,
                        keyword));
            }


            var totalRows =
                await query.CountAsync();


            var data =
                await query
                    .OrderByDescending(
                        x => x.CreateDateTime)
                    .Skip(
                        (page - 1) * perPage)
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

                    totalPages =
                        (int)Math.Ceiling(
                            totalRows /
                            (double)perPage)
                }
            });
        }
    }
}
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Models;
//using QuilvianSystemBackendDev.Repositories;
//using System.Security.Claims;

//namespace QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    [EnableCors("FrontendCorsPolicy")]
//    public class GLDetailController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<GLDetailController> _logger;

//        public GLDetailController(
//            ApplicationDbContext context,
//            ILogger<GLDetailController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        // ================= GET ALL =================
//        [HttpGet]
//        public async Task<IActionResult> GetAll(
//            int page = 1,
//            int perPage = 10,
//            Guid? glHeaderId = null,
//            string? search = null)
//        {
//            if (page < 1)
//                page = 1;

//            if (perPage < 1)
//                perPage = 10;

//            var query =
//                from detail in _context.GLDetails

//                join header in _context.GLHeaders
//                    on detail.GLHeaderId equals header.GLHeaderId

//                join coa in _context.MasterCoas
//                    on detail.COAId equals coa.COAId

//                join user in _context.UserActives
//                    on detail.CreateBy equals user.UserActiveId
//                    into userJoin

//                from user in userJoin.DefaultIfEmpty()

//                where detail.IsDelete == false &&
//                      header.IsDelete == false &&
//                      coa.IsDelete == false

//                select new
//                {
//                    detail.GLDetailId,
//                    detail.GLHeaderId,

//                    header.GLKode,
//                    header.NoRegistrasi,
//                    header.SourceGL,
//                    header.SourceTypeGL,
//                    header.SourceNumber,
//                    header.GLStatus,

//                    detail.COAId,
//                    coa.KodeCOA,
//                    coa.NamaCOA,

//                    detail.NilaiDebit,
//                    detail.NilaiKredit,

//                    detail.SourceItemType,
//                    detail.SourceItemId,
//                    detail.SourceItem,

//                    detail.CostCenterId,
//                    detail.CostCenterName,
//                    detail.Keterangan,

//                    detail.CreateDateTime,

//                    CreateByName = user != null
//                        ? user.FullName
//                        : null
//                };

//            if (glHeaderId.HasValue)
//            {
//                query = query.Where(x =>
//                    x.GLHeaderId == glHeaderId.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = $"%{search.Trim()}%";

//                query = query.Where(x =>
//                    EF.Functions.ILike(x.GLKode!, keyword) ||
//                    EF.Functions.ILike(x.KodeCOA!, keyword) ||
//                    EF.Functions.ILike(x.NamaCOA!, keyword) ||
//                    EF.Functions.ILike(x.SourceItem!, keyword) ||
//                    EF.Functions.ILike(x.SourceNumber!, keyword));
//            }

//            var totalRows = await query.CountAsync();

//            var data = await query
//                .OrderByDescending(x => x.CreateDateTime)
//                .Skip((page - 1) * perPage)
//                .Take(perPage)
//                .ToListAsync();

//            return Ok(new
//            {
//                message = "success",
//                data,
//                pagination = new
//                {
//                    page,
//                    perPage,
//                    totalRows,
//                    totalPages = (int)Math.Ceiling(
//                        totalRows / (double)perPage)
//                }
//            });
//        }

//        // ================= GET BY ID =================
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(Guid id)
//        {
//            var data = await (
//                from detail in _context.GLDetails

//                join header in _context.GLHeaders
//                    on detail.GLHeaderId equals header.GLHeaderId

//                join coa in _context.MasterCoas
//                    on detail.COAId equals coa.COAId

//                join user in _context.UserActives
//                    on detail.CreateBy equals user.UserActiveId
//                    into userJoin

//                from user in userJoin.DefaultIfEmpty()

//                where detail.GLDetailId == id &&
//                      detail.IsDelete == false &&
//                      header.IsDelete == false &&
//                      coa.IsDelete == false

//                select new
//                {
//                    detail.GLDetailId,
//                    detail.GLHeaderId,

//                    header.GLKode,
//                    header.NoRegistrasi,
//                    header.SourceGL,
//                    header.SourceTypeGL,
//                    header.SourceNumber,
//                    header.GLStatus,

//                    detail.COAId,
//                    coa.KodeCOA,
//                    coa.NamaCOA,

//                    detail.NilaiDebit,
//                    detail.NilaiKredit,

//                    detail.SourceItemType,
//                    detail.SourceItemId,
//                    detail.SourceItem,

//                    detail.CostCenterId,
//                    detail.CostCenterName,
//                    detail.Keterangan,

//                    detail.CreateDateTime,
//                    detail.UpdateDateTime,

//                    CreateByName = user != null
//                        ? user.FullName
//                        : null
//                })
//                .FirstOrDefaultAsync();

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Detail tidak ditemukan"
//                });
//            }

//            return Ok(new
//            {
//                message = "success",
//                data
//            });
//        }

//        // ================= GET BY HEADER =================
//        [HttpGet("header/{glHeaderId}")]
//        public async Task<IActionResult> GetByHeader(
//            Guid glHeaderId)
//        {
//            var headerExists = await _context.GLHeaders
//                .AnyAsync(x =>
//                    x.GLHeaderId == glHeaderId &&
//                    x.IsDelete == false);

//            if (!headerExists)
//            {
//                return NotFound(new
//                {
//                    message = "GL Header tidak ditemukan"
//                });
//            }

//            var data = await (
//                from detail in _context.GLDetails

//                join coa in _context.MasterCoas
//                    on detail.COAId equals coa.COAId

//                where detail.GLHeaderId == glHeaderId &&
//                      detail.IsDelete == false &&
//                      coa.IsDelete == false

//                orderby detail.CreateDateTime

//                select new
//                {
//                    detail.GLDetailId,
//                    detail.GLHeaderId,
//                    detail.COAId,

//                    coa.KodeCOA,
//                    coa.NamaCOA,

//                    detail.NilaiDebit,
//                    detail.NilaiKredit,

//                    detail.SourceItemType,
//                    detail.SourceId,
//                    detail.SourceNumber,
//                    detail.SourceItemId,
//                    detail.SourceItem,

//                    detail.CostCenterId,
//                    detail.CostCenterName,
//                    detail.Keterangan
//                })
//                .ToListAsync();

//            return Ok(new
//            {
//                message = "success",
//                data,
//                summary = new
//                {
//                    totalDebit = data.Sum(x => x.NilaiDebit),
//                    totalKredit = data.Sum(x => x.NilaiKredit),
//                    balance =
//                        data.Sum(x => x.NilaiDebit) -
//                        data.Sum(x => x.NilaiKredit)
//                }
//            });
//        }

//        // ================= CREATE =================
//        [HttpPost]
//        public async Task<IActionResult> Create(
//            [FromBody] GLDetail model)
//        {
//            if (model.GLHeaderId == Guid.Empty)
//            {
//                return BadRequest(new
//                {
//                    message = "GLHeaderId wajib diisi"
//                });
//            }

//            if (model.COAId == Guid.Empty)
//            {
//                return BadRequest(new
//                {
//                    message = "COAId wajib diisi"
//                });
//            }

//            if (model.NilaiDebit < 0 ||
//                model.NilaiKredit < 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Nilai debit dan kredit tidak boleh negatif"
//                });
//            }

//            if (model.NilaiDebit == 0 &&
//                model.NilaiKredit == 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Nilai debit atau kredit harus diisi"
//                });
//            }

//            if (model.NilaiDebit > 0 &&
//                model.NilaiKredit > 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Satu detail tidak boleh memiliki debit dan kredit sekaligus"
//                });
//            }

//            var email = User
//                .FindFirst(ClaimTypes.NameIdentifier)?
//                .Value;

//            var user = await _context.UserActives
//                .FirstOrDefaultAsync(x => x.Email == email);

//            if (user == null)
//                return Unauthorized();

//            var header = await _context.GLHeaders
//                .FirstOrDefaultAsync(x =>
//                    x.GLHeaderId == model.GLHeaderId &&
//                    x.IsDelete == false);

//            if (header == null)
//            {
//                return BadRequest(new
//                {
//                    message = "GL Header tidak ditemukan"
//                });
//            }

//            var coa = await _context.MasterCoas
//                .FirstOrDefaultAsync(x =>
//                    x.COAId == model.COAId &&
//                    x.IsDelete == false);

//            if (coa == null)
//            {
//                return BadRequest(new
//                {
//                    message = "COA tidak ditemukan"
//                });
//            }

//            if (coa.IsPostable != true)
//            {
//                return BadRequest(new
//                {
//                    message = "COA tersebut tidak dapat diposting"
//                });
//            }

//            if (coa.IsValid != true)
//            {
//                return BadRequest(new
//                {
//                    message = "COA tersebut tidak valid"
//                });
//            }

//            model.GLDetailId = Guid.NewGuid();

//            if (string.IsNullOrWhiteSpace(model.SourceNumber))
//            {
//                model.SourceNumber = header.SourceNumber;
//            }

//            model.CreateBy = user.UserActiveId;
//            model.CreateDateTime = DateTime.UtcNow;
//            model.IsDelete = false;

//            _context.GLDetails.Add(model);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Detail berhasil dibuat",
//                data = new
//                {
//                    model.GLDetailId,
//                    model.GLHeaderId
//                }
//            });
//        }

//        // ================= UPDATE =================
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(
//            Guid id,
//            [FromBody] GLDetail model)
//        {
//            var data = await _context.GLDetails
//                .FirstOrDefaultAsync(x =>
//                    x.GLDetailId == id &&
//                    x.IsDelete == false);

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Detail tidak ditemukan"
//                });
//            }

//            if (model.NilaiDebit < 0 ||
//                model.NilaiKredit < 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Nilai debit dan kredit tidak boleh negatif"
//                });
//            }

//            if (model.NilaiDebit == 0 &&
//                model.NilaiKredit == 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Nilai debit atau kredit harus diisi"
//                });
//            }

//            if (model.NilaiDebit > 0 &&
//                model.NilaiKredit > 0)
//            {
//                return BadRequest(new
//                {
//                    message =
//                        "Satu detail tidak boleh memiliki debit dan kredit sekaligus"
//                });
//            }

//            var email = User
//                .FindFirst(ClaimTypes.NameIdentifier)?
//                .Value;

//            var user = await _context.UserActives
//                .FirstOrDefaultAsync(x => x.Email == email);

//            if (user == null)
//                return Unauthorized();

//            var coa = await _context.MasterCoas
//                .FirstOrDefaultAsync(x =>
//                    x.COAId == model.COAId &&
//                    x.IsDelete == false);

//            if (coa == null)
//            {
//                return BadRequest(new
//                {
//                    message = "COA tidak ditemukan"
//                });
//            }

//            data.COAId = model.COAId;
//            data.NilaiDebit = model.NilaiDebit;
//            data.NilaiKredit = model.NilaiKredit;

//            data.SourceItemType = model.SourceItemType;
//            data.SourceId = model.SourceId;
//            data.SourceNumber = model.SourceNumber;
//            data.SourceItemId = model.SourceItemId;
//            data.SourceItem = model.SourceItem;

//            data.CostCenterId = model.CostCenterId;
//            data.CostCenterName = model.CostCenterName;
//            data.Keterangan = model.Keterangan;

//            data.UpdateBy = user.UserActiveId;
//            data.UpdateDateTime = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Detail berhasil diperbarui"
//            });
//        }

//        // ================= DELETE SOFT =================
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            var data = await _context.GLDetails
//                .FirstOrDefaultAsync(x =>
//                    x.GLDetailId == id &&
//                    x.IsDelete == false);

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Detail tidak ditemukan"
//                });
//            }

//            var email = User
//                .FindFirst(ClaimTypes.NameIdentifier)?
//                .Value;

//            var user = await _context.UserActives
//                .FirstOrDefaultAsync(x => x.Email == email);

//            if (user == null)
//                return Unauthorized();

//            data.IsDelete = true;
//            data.DeleteBy = user.UserActiveId;
//            data.DeleteDateTime = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Detail berhasil dihapus"
//            });
//        }
//        // ================= PAGED =================
//        [HttpGet("paged")]
//        public async Task<IActionResult> Paged(
//            int page = 1,
//            int perPage = 10,
//            Guid? glHeaderId = null,
//            string? search = null)
//        {
//            if (page < 1)
//                page = 1;

//            if (perPage < 1)
//                perPage = 10;

//            if (perPage > 100)
//                perPage = 100;

//            var query =
//                from detail in _context.GLDetails

//                join header in _context.GLHeaders
//                    on detail.GLHeaderId equals header.GLHeaderId

//                join coa in _context.MasterCoas
//                    on detail.COAId equals coa.COAId

//                join userData in _context.UserActives
//                    on detail.CreateBy equals userData.UserActiveId
//                    into userJoin

//                from user in userJoin.DefaultIfEmpty()

//                where detail.IsDelete == false &&
//                      header.IsDelete == false &&
//                      coa.IsDelete == false

//                select new
//                {
//                    detail.GLDetailId,
//                    detail.GLHeaderId,

//                    header.GLKode,
//                    header.KunjunganId,
//                    header.NoRegistrasi,
//                    header.JenisKunjungan,
//                    header.PasienId,
//                    header.TglTransaksi,
//                    header.TglPosting,
//                    header.SourceGL,
//                    header.SourceTypeGL,
//                    HeaderSourceId = header.SourceId,
//                    HeaderSourceNumber = header.SourceNumber,
//                    header.GLStatus,

//                    detail.COAId,
//                    coa.KodeCOA,
//                    coa.NamaCOA,

//                    detail.NilaiDebit,
//                    detail.NilaiKredit,

//                    detail.SourceItemType,
//                    detail.SourceId,
//                    detail.SourceNumber,
//                    detail.SourceItemId,
//                    detail.SourceItem,

//                    detail.CostCenterId,
//                    detail.CostCenterName,
//                    detail.Keterangan,
//                    detail.CreateDateTime,

//                    CreateByName = user != null
//                        ? user.FullName
//                        : null
//                };

//            if (glHeaderId.HasValue &&
//                glHeaderId.Value != Guid.Empty)
//            {
//                query = query.Where(x =>
//                    x.GLHeaderId == glHeaderId.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = $"%{search.Trim()}%";

//                query = query.Where(x =>
//                    EF.Functions.ILike(x.GLKode!, keyword) ||
//                    EF.Functions.ILike(x.NoRegistrasi!, keyword) ||
//                    EF.Functions.ILike(x.JenisKunjungan!, keyword) ||
//                    EF.Functions.ILike(x.SourceGL!, keyword) ||
//                    EF.Functions.ILike(x.SourceTypeGL!, keyword) ||
//                    EF.Functions.ILike(x.HeaderSourceNumber!, keyword) ||
//                    EF.Functions.ILike(x.GLStatus!, keyword) ||
//                    EF.Functions.ILike(x.KodeCOA!, keyword) ||
//                    EF.Functions.ILike(x.NamaCOA!, keyword) ||
//                    EF.Functions.ILike(x.SourceItemType!, keyword) ||
//                    EF.Functions.ILike(x.SourceNumber!, keyword) ||
//                    EF.Functions.ILike(x.SourceItem!, keyword) ||
//                    EF.Functions.ILike(x.CostCenterName!, keyword) ||
//                    EF.Functions.ILike(x.Keterangan!, keyword));
//            }

//            var totalRows = await query.CountAsync();

//            var data = await query
//                .OrderByDescending(x => x.CreateDateTime)
//                .Skip((page - 1) * perPage)
//                .Take(perPage)
//                .ToListAsync();

//            return Ok(new
//            {
//                message = "success",
//                data,
//                pagination = new
//                {
//                    page,
//                    perPage,
//                    totalRows,
//                    totalPages = (int)Math.Ceiling(
//                        totalRows / (double)perPage)
//                }
//            });
//        }
//    }
//}