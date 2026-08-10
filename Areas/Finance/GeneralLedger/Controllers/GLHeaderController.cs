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


        // ============================================================
        // GET ALL
        // ============================================================

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

            if (perPage > 100)
                perPage = 100;


            var query =
                from gl in _context.GLHeaders

                    // ====================================================
                    // USER CREATE
                    // ====================================================

                join userData in _context.UserActives
                    on gl.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()


                    // ====================================================
                    // KUNJUNGAN
                    // ====================================================

                join kunjunganData in _context.Kunjungans
                    on gl.KunjunganId equals kunjunganData.KunjunganID
                    into kunjunganJoin

                from kunjungan in kunjunganJoin.DefaultIfEmpty()


                    // ====================================================
                    // DOKTER
                    // ====================================================

                join dokterData in _context.Dokters
                        .Where(x => x.IsDelete == false)
                    on kunjungan.DokterId equals dokterData.DokterId
                    into dokterJoin

                from dokter in dokterJoin.DefaultIfEmpty()


                    // ====================================================
                    // RECURRING JOURNAL
                    //
                    // Fin_GLHeader.TempRJId
                    //          ↓
                    // Fin_TempRecurringJournal.TempRJId
                    //          ↓
                    // RecurringJournalName
                    // RecurringJournalDate
                    // ====================================================

                join recurringData in _context.RecurringJournals
                        .Where(x => x.IsDelete == false)
                    on gl.TempRJId equals (Guid?)recurringData.TempRJId
                    into recurringJoin

                from recurringJournal in recurringJoin.DefaultIfEmpty()


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


                    // ================================================
                    // RECURRING JOURNAL
                    // ================================================

                    gl.TempRJId,

                    RecurringJournalName =
                        recurringJournal != null
                            ? recurringJournal.RecurringJournalName
                            : null,

                    RecurringJournalDate =
                        recurringJournal != null
                            ? (DateTime?)recurringJournal.RecurringJournalDate
                            : null,


                    // ================================================
                    // MATA UANG
                    // ================================================

                    gl.MataUangId,
                    gl.NamaMataUang,


                    // ================================================
                    // EXCHANGE RATE
                    // ================================================

                    gl.ExchangeRateId,
                    gl.RateToIdr,


                    // ================================================
                    // UNBALANCE
                    // ================================================

                    gl.UnbalanceAmount,


                    // ================================================
                    // AUDIT
                    // ================================================

                    gl.CreateDateTime,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null,


                    // ================================================
                    // DOKTER
                    // ================================================

                    DokterId =
                        dokter != null
                            ? (Guid?)dokter.DokterId
                            : null,

                    NamaDokter =
                        dokter != null
                            ? dokter.NmDokter
                            : null
                };


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
                        x.SourceNumber!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceGL!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.SourceTypeGL!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.GLStatus!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.RecurringJournalName!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.NamaMataUang!,
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
                        x => x.TglPosting)
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
                        "GLHeaderId tidak valid"
                });
            }


            var data = await (
                from gl in _context.GLHeaders

                    // ====================================================
                    // USER
                    // ====================================================

                join userData in _context.UserActives
                    on gl.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()


                    // ====================================================
                    // KUNJUNGAN
                    // ====================================================

                join kunjunganData in _context.Kunjungans
                    on gl.KunjunganId equals kunjunganData.KunjunganID
                    into kunjunganJoin

                from kunjungan in kunjunganJoin.DefaultIfEmpty()


                    // ====================================================
                    // DOKTER
                    // ====================================================

                join dokterData in _context.Dokters
                        .Where(x => x.IsDelete == false)
                    on kunjungan.DokterId equals dokterData.DokterId
                    into dokterJoin

                from dokter in dokterJoin.DefaultIfEmpty()


                    // ====================================================
                    // RECURRING JOURNAL
                    // GET BERDASARKAN gl.TempRJId
                    // ====================================================

                join recurringData in _context.RecurringJournals
                        .Where(x => x.IsDelete == false)
                    on gl.TempRJId equals (Guid?)recurringData.TempRJId
                    into recurringJoin

                from recurringJournal in recurringJoin.DefaultIfEmpty()


                where
                    gl.GLHeaderId == id &&
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


                    // ================================================
                    // RECURRING JOURNAL
                    // ================================================

                    gl.TempRJId,

                    RecurringJournalName =
                        recurringJournal != null
                            ? recurringJournal.RecurringJournalName
                            : null,

                    RecurringJournalDate =
                        recurringJournal != null
                            ? (DateTime?)recurringJournal.RecurringJournalDate
                            : null,


                    // ================================================
                    // MATA UANG
                    // ================================================

                    gl.MataUangId,
                    gl.NamaMataUang,


                    // ================================================
                    // EXCHANGE RATE
                    // ================================================

                    gl.ExchangeRateId,
                    gl.RateToIdr,


                    // ================================================
                    // UNBALANCE
                    // ================================================

                    gl.UnbalanceAmount,


                    // ================================================
                    // AUDIT
                    // ================================================

                    gl.CreateDateTime,
                    gl.CreateBy,

                    gl.UpdateDateTime,
                    gl.UpdateBy,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null,


                    // ================================================
                    // DOKTER
                    // ================================================

                    DokterId =
                        dokter != null
                            ? (Guid?)dokter.DokterId
                            : null,

                    NamaDokter =
                        dokter != null
                            ? dokter.NmDokter
                            : null
                })
                .FirstOrDefaultAsync();


            if (data == null)
            {
                return NotFound(new
                {
                    message =
                        "GL Header tidak ditemukan"
                });
            }


            return Ok(new
            {
                message = "success",
                data
            });
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] GLHeader model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Data GL Header tidak valid"
                    });
                }


                // ====================================================
                // VALIDASI KUNJUNGAN
                // ====================================================

                if (model.KunjunganId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "KunjunganId wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI SOURCE
                // ====================================================

                if (model.SourceId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "SourceId wajib diisi"
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.SourceGL))
                {
                    return BadRequest(new
                    {
                        message =
                            "SourceGL wajib diisi"
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.SourceTypeGL))
                {
                    return BadRequest(new
                    {
                        message =
                            "SourceTypeGL wajib diisi"
                    });
                }


                // ====================================================
                // VALIDASI TGL TRANSAKSI
                // ====================================================

                if (model.TglTransaksi == default)
                {
                    return BadRequest(new
                    {
                        message =
                            "TglTransaksi wajib diisi"
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
                            x.Email == email &&
                            x.IsDelete == false);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // GET KUNJUNGAN
                // ====================================================

                var kunjungan =
                    await _context.Kunjungans
                        .AsNoTracking()
                        .Where(x =>
                            x.KunjunganID ==
                                model.KunjunganId &&

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
                        message =
                            "Data kunjungan tidak ditemukan"
                    });
                }


                // ====================================================
                // NORMALISASI SOURCE
                // ====================================================

                var sourceGL =
                    model.SourceGL.Trim();


                var sourceTypeGL =
                    model.SourceTypeGL.Trim();


                // ====================================================
                // CEK DUPLIKASI SOURCE GL
                // ====================================================

                var sourceExists =
                    await _context.GLHeaders
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.SourceId ==
                                model.SourceId &&

                            x.SourceGL ==
                                sourceGL &&

                            x.SourceTypeGL ==
                                sourceTypeGL &&

                            x.GLStatus !=
                                "REVERSED" &&

                            x.IsDelete == false);


                if (sourceExists)
                {
                    return Conflict(new
                    {
                        message =
                            "Transaksi tersebut sudah pernah dibuatkan GL"
                    });
                }


                // ====================================================
                // RECURRING JOURNAL
                //
                // GL HEADER HANYA MENYIMPAN TempRJId
                //
                // TIDAK ADA:
                // model.RecurringJournalName
                // model.RecurringJournalDate
                // ====================================================

                string? recurringJournalName =
                    null;

                DateTime? recurringJournalDate =
                    null;


                if (model.TempRJId.HasValue &&
                    model.TempRJId.Value != Guid.Empty)
                {
                    var recurringJournal =
                        await _context
                            .RecurringJournals
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                x.TempRJId ==
                                    model.TempRJId.Value &&

                                x.IsDelete == false);


                    if (recurringJournal == null)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Recurring Journal tidak ditemukan"
                        });
                    }


                    // Yang disimpan ke GL Header hanya ID.
                    model.TempRJId =
                        recurringJournal.TempRJId;


                    // Hanya untuk response.
                    recurringJournalName =
                        recurringJournal
                            .RecurringJournalName;


                    recurringJournalDate =
                        recurringJournal
                            .RecurringJournalDate;
                }
                else
                {
                    model.TempRJId =
                        null;
                }


                // ====================================================
                // MATA UANG
                // ====================================================

                if (model.MataUangId.HasValue &&
                    model.MataUangId.Value == Guid.Empty)
                {
                    model.MataUangId =
                        null;
                }


                if (!string.IsNullOrWhiteSpace(
                    model.NamaMataUang))
                {
                    model.NamaMataUang =
                        model.NamaMataUang.Trim();
                }


                // ====================================================
                // EXCHANGE RATE
                // ====================================================

                if (model.ExchangeRateId.HasValue &&
                    model.ExchangeRateId.Value == Guid.Empty)
                {
                    model.ExchangeRateId =
                        null;
                }


                // ====================================================
                // UNBALANCE
                // ====================================================

                model.UnbalanceAmount =
                    model.UnbalanceAmount ?? 0;


                // ====================================================
                // CREATE GL HEADER
                // ====================================================

                model.GLHeaderId =
                    Guid.NewGuid();


                model.GLKode =
                    await GenerateGLKode();


                model.NoRegistrasi =
                    kunjungan.NoRegistrasi;


                model.JenisKunjungan =
                    kunjungan.JenisKunjungan;


                // ====================================================
                // FIX PASIEN ID
                // ====================================================

                model.PasienId = 
                    kunjungan.KunjunganID;


                model.TglTransaksi =
                    ConvertDateToUtc(
                        model.TglTransaksi);


                model.TglPosting =
                    DateTime.UtcNow;


                model.SourceGL =
                    sourceGL;


                model.SourceTypeGL =
                    sourceTypeGL;


                model.SourceNumber =
                    model.SourceNumber?.Trim();


                model.GLStatus =
                    "POSTED";


                model.Keterangan =
                    model.Keterangan?.Trim();


                model.CreateBy =
                    user.UserActiveId;


                model.CreateDateTime =
                    DateTime.UtcNow;


                model.IsDelete =
                    false;


                _context.GLHeaders.Add(model);


                await _context.SaveChangesAsync();


                return Created("", new
                {
                    message =
                        "GL Header berhasil dibuat",

                    data = new
                    {
                        model.GLHeaderId,
                        model.GLKode,

                        model.KunjunganId,
                        model.NoRegistrasi,
                        model.JenisKunjungan,
                        model.PasienId,

                        model.TglTransaksi,
                        model.TglPosting,

                        model.SourceGL,
                        model.SourceTypeGL,
                        model.SourceId,
                        model.SourceNumber,

                        model.GLStatus,

                        model.TempRJId,

                        RecurringJournalName =
                            recurringJournalName,

                        RecurringJournalDate =
                            recurringJournalDate,

                        model.MataUangId,
                        model.NamaMataUang,

                        model.ExchangeRateId,
                        model.RateToIdr,

                        model.UnbalanceAmount,

                        model.Keterangan,

                        model.CreateDateTime
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                var detail =
                    ex.GetBaseException().Message;


                _logger.LogError(
                    ex,
                    "Gagal menyimpan GL Header. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal menyimpan GL Header",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat membuat GL Header");


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
            [FromBody] GLHeader model)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "GLHeaderId tidak valid"
                    });
                }


                var data =
                    await _context.GLHeaders
                        .FirstOrDefaultAsync(x =>
                            x.GLHeaderId == id &&
                            x.IsDelete == false);


                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "GL Header tidak ditemukan"
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
                            x.Email == email &&
                            x.IsDelete == false);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // RECURRING JOURNAL
                //
                // YANG DIUPDATE HANYA TempRJId
                // ====================================================

                if (model.TempRJId.HasValue &&
                    model.TempRJId.Value != Guid.Empty)
                {
                    var recurringJournal =
                        await _context
                            .RecurringJournals
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                x.TempRJId ==
                                    model.TempRJId.Value &&

                                x.IsDelete == false);


                    if (recurringJournal == null)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Recurring Journal tidak ditemukan"
                        });
                    }


                    data.TempRJId =
                        recurringJournal.TempRJId;
                }
                else
                {
                    data.TempRJId =
                        null;
                }


                // ====================================================
                // DATA GL
                // ====================================================

                if (model.TglTransaksi != default)
                {
                    data.TglTransaksi =
                        ConvertDateToUtc(
                            model.TglTransaksi);
                }


                if (!string.IsNullOrWhiteSpace(
                    model.SourceGL))
                {
                    data.SourceGL =
                        model.SourceGL.Trim();
                }


                if (!string.IsNullOrWhiteSpace(
                    model.SourceTypeGL))
                {
                    data.SourceTypeGL =
                        model.SourceTypeGL.Trim();
                }


                if (model.SourceId != Guid.Empty)
                {
                    data.SourceId =
                        model.SourceId;
                }


                data.SourceNumber =
                    model.SourceNumber?.Trim();


                if (!string.IsNullOrWhiteSpace(
                    model.GLStatus))
                {
                    data.GLStatus =
                        model.GLStatus.Trim();
                }


                data.Keterangan =
                    model.Keterangan?.Trim();


                // ====================================================
                // MATA UANG
                // ====================================================

                data.MataUangId =
                    model.MataUangId.HasValue &&
                    model.MataUangId.Value != Guid.Empty
                        ? model.MataUangId
                        : null;


                data.NamaMataUang =
                    model.NamaMataUang?.Trim();


                // ====================================================
                // EXCHANGE RATE
                // ====================================================

                data.ExchangeRateId =
                    model.ExchangeRateId.HasValue &&
                    model.ExchangeRateId.Value != Guid.Empty
                        ? model.ExchangeRateId
                        : null;


                data.RateToIdr =
                    model.RateToIdr;


                // ====================================================
                // UNBALANCE
                // ====================================================

                data.UnbalanceAmount =
                    model.UnbalanceAmount ?? 0;


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
                        "GL Header berhasil diperbarui",

                    data = new
                    {
                        data.GLHeaderId,
                        data.GLKode,

                        data.TempRJId,

                        data.TglTransaksi,

                        data.SourceGL,
                        data.SourceTypeGL,
                        data.SourceId,
                        data.SourceNumber,

                        data.GLStatus,

                        data.MataUangId,
                        data.NamaMataUang,

                        data.ExchangeRateId,
                        data.RateToIdr,

                        data.UnbalanceAmount,

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
                    "Gagal memperbarui GL Header. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal memperbarui GL Header",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat memperbarui GL Header");


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
                            "GLHeaderId tidak valid"
                    });
                }


                var data =
                    await _context.GLHeaders
                        .FirstOrDefaultAsync(x =>
                            x.GLHeaderId == id &&
                            x.IsDelete == false);


                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "GL Header tidak ditemukan"
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
                            x.Email == email &&
                            x.IsDelete == false);


                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan"
                    });
                }


                // ====================================================
                // SOFT DELETE HEADER
                // ====================================================

                data.IsDelete =
                    true;


                data.DeleteBy =
                    user.UserActiveId;


                data.DeleteDateTime =
                    DateTime.UtcNow;


                // ====================================================
                // SOFT DELETE DETAIL
                // ====================================================

                var details =
                    await _context.GLDetails
                        .Where(x =>
                            x.GLHeaderId == id &&
                            x.IsDelete == false)
                        .ToListAsync();


                foreach (var detail in details)
                {
                    detail.IsDelete =
                        true;


                    detail.DeleteBy =
                        user.UserActiveId;


                    detail.DeleteDateTime =
                        DateTime.UtcNow;
                }


                await _context.SaveChangesAsync();


                return Ok(new
                {
                    message =
                        "GL Header berhasil dihapus"
                });
            }
            catch (DbUpdateException ex)
            {
                var detail =
                    ex.GetBaseException().Message;


                _logger.LogError(
                    ex,
                    "Gagal menghapus GL Header. Detail: {Detail}",
                    detail);


                return StatusCode(500, new
                {
                    message =
                        "Gagal menghapus GL Header",

                    detail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Terjadi kesalahan saat menghapus GL Header");


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
        // PAGED + FILTER
        // ============================================================

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? sourceTypeGL = null,
            string? glStatus = null)
        {
            if (page < 1)
                page = 1;


            if (perPage < 1)
                perPage = 10;


            if (perPage > 100)
                perPage = 100;


            var query =
                from gl in _context.GLHeaders

                    // ====================================================
                    // USER
                    // ====================================================

                join userData in _context.UserActives
                    on gl.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()


                    // ====================================================
                    // KUNJUNGAN
                    // ====================================================

                join kunjunganData in _context.Kunjungans
                    on gl.KunjunganId equals kunjunganData.KunjunganID
                    into kunjunganJoin

                from kunjungan in kunjunganJoin.DefaultIfEmpty()


                    // ====================================================
                    // DOKTER
                    // ====================================================

                join dokterData in _context.Dokters
                        .Where(x => x.IsDelete == false)
                    on kunjungan.DokterId equals dokterData.DokterId
                    into dokterJoin

                from dokter in dokterJoin.DefaultIfEmpty()


                    // ====================================================
                    // RECURRING JOURNAL
                    //
                    // RecurringJournalName dan Date GET berdasarkan
                    // gl.TempRJId
                    // ====================================================

                join recurringData in _context.RecurringJournals
                        .Where(x => x.IsDelete == false)
                    on gl.TempRJId equals (Guid?)recurringData.TempRJId
                    into recurringJoin

                from recurringJournal in recurringJoin.DefaultIfEmpty()


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


                    // ================================================
                    // RECURRING JOURNAL
                    // ================================================

                    gl.TempRJId,

                    RecurringJournalName =
                        recurringJournal != null
                            ? recurringJournal.RecurringJournalName
                            : null,

                    RecurringJournalDate =
                        recurringJournal != null
                            ? (DateTime?)recurringJournal.RecurringJournalDate
                            : null,


                    // ================================================
                    // MATA UANG
                    // ================================================

                    gl.MataUangId,
                    gl.NamaMataUang,


                    // ================================================
                    // EXCHANGE RATE
                    // ================================================

                    gl.ExchangeRateId,
                    gl.RateToIdr,


                    // ================================================
                    // UNBALANCE
                    // ================================================

                    gl.UnbalanceAmount,


                    // ================================================
                    // AUDIT
                    // ================================================

                    gl.CreateDateTime,

                    CreateByName =
                        user != null
                            ? user.FullName
                            : null,


                    // ================================================
                    // DOKTER
                    // ================================================

                    DokterId =
                        dokter != null
                            ? (Guid?)dokter.DokterId
                            : null,

                    NamaDokter =
                        dokter != null
                            ? dokter.NmDokter
                            : null
                };


            // ========================================================
            // FILTER SOURCE TYPE GL
            // ========================================================

            if (!string.IsNullOrWhiteSpace(
                sourceTypeGL))
            {
                var sourceTypeKeyword =
                    sourceTypeGL.Trim();


                query = query.Where(x =>
                    x.SourceTypeGL != null &&

                    EF.Functions.ILike(
                        x.SourceTypeGL,
                        sourceTypeKeyword));
            }


            // ========================================================
            // FILTER GL STATUS
            // ========================================================

            if (!string.IsNullOrWhiteSpace(
                glStatus))
            {
                var statusKeyword =
                    glStatus.Trim();


                query = query.Where(x =>
                    x.GLStatus != null &&

                    EF.Functions.ILike(
                        x.GLStatus,
                        statusKeyword));
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
                        x.SourceNumber!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.GLStatus!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.RecurringJournalName!,
                        keyword) ||

                    EF.Functions.ILike(
                        x.NamaMataUang!,
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
                        x => x.TglPosting)
                    .Skip(
                        (page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();


            return Ok(new
            {
                message = "success",

                filter = new
                {
                    sourceTypeGL,
                    glStatus,
                    search
                },

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
        // GENERATE GL KODE
        // ============================================================

        private async Task<string> GenerateGLKode()
        {
            var nowJakarta =
                DateTime.UtcNow.AddHours(7);


            var kodeDalamBulan =
                await _context.GLHeaders
                    .AsNoTracking()
                    .Where(x =>
                        x.TglPosting.Year ==
                            nowJakarta.Year &&

                        x.TglPosting.Month ==
                            nowJakarta.Month)
                    .Select(x => x.GLKode)
                    .ToListAsync();


            var sequenceTerakhir =
                kodeDalamBulan
                    .Select(x =>
                    {
                        if (string.IsNullOrWhiteSpace(x))
                            return 0;


                        var split =
                            x.Split('-');


                        if (split.Length < 3)
                            return 0;


                        return int.TryParse(
                            split.Last(),
                            out var nomor)
                                ? nomor
                                : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();


            var sequenceBaru =
                sequenceTerakhir + 1;


            return
                $"GL-{nowJakarta:yyMMdd}-{sequenceBaru:00000}";
        }


        // ============================================================
        // CONVERT DATETIME TO UTC
        // ============================================================

        private static DateTime ConvertDateToUtc(
            DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }


            return DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc);
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
//    public class GLHeaderController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<GLHeaderController> _logger;

//        public GLHeaderController(
//            ApplicationDbContext context,
//            ILogger<GLHeaderController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        // ================= GET ALL =================
//        [HttpGet]
//        public async Task<IActionResult> GetAll(
//            int page = 1,
//            int perPage = 10,
//            string? search = null)
//        {
//            if (page < 1)
//                page = 1;

//            if (perPage < 1)
//                perPage = 10;

//            var query =
//                from gl in _context.GLHeaders

//                join userData in _context.UserActives
//                    on gl.CreateBy equals userData.UserActiveId
//                    into userJoin
//                from user in userJoin.DefaultIfEmpty()

//                join kunjungan in _context.Kunjungans
//                    on gl.KunjunganId equals kunjungan.KunjunganID
//                    into kunjunganJoin
//                from kunjungan in kunjunganJoin.DefaultIfEmpty()

//                join dokter in _context.Dokters
//                    on kunjungan.DokterId equals dokter.DokterId
//                    into dokterJoin
//                from dokter in dokterJoin.DefaultIfEmpty()

//                where !gl.IsDelete

//                select new
//                {
//                    gl.GLHeaderId,
//                    gl.GLKode,
//                    gl.KunjunganId,
//                    gl.NoRegistrasi,
//                    gl.JenisKunjungan,
//                    gl.PasienId,
//                    gl.TglTransaksi,
//                    gl.TglPosting,
//                    gl.SourceGL,
//                    gl.SourceTypeGL,
//                    gl.SourceId,
//                    gl.SourceNumber,
//                    gl.GLStatus,
//                    gl.Keterangan,
//                    gl.CreateDateTime,

//                    CreateByName = user != null
//                        ? user.FullName
//                        : null,

//                    DokterId = kunjungan.DokterId,

//                    NamaDokter = dokter != null
//                        ? dokter.NmDokter
//                        : null
//                };

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = $"%{search.Trim()}%";

//                query = query.Where(x =>
//                    EF.Functions.ILike(x.GLKode!, keyword) ||
//                    EF.Functions.ILike(x.NoRegistrasi!, keyword) ||
//                    EF.Functions.ILike(x.SourceNumber!, keyword) ||
//                    EF.Functions.ILike(x.SourceGL!, keyword));
//            }

//            var totalRows = await query.CountAsync();

//            var data = await query
//                .OrderByDescending(x => x.TglPosting)
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
//                from gl in _context.GLHeaders

//                join user in _context.UserActives
//                    on gl.CreateBy equals user.UserActiveId
//                    into userJoin
//                from user in userJoin.DefaultIfEmpty()

//                join kunjungan in _context.Kunjungans
//                    on gl.KunjunganId equals kunjungan.KunjunganID
//                    into kunjunganJoin
//                from kunjungan in kunjunganJoin.DefaultIfEmpty()

//                join dokter in _context.Dokters
//                    on kunjungan.DokterId equals dokter.DokterId
//                    into dokterJoin
//                from dokter in dokterJoin.DefaultIfEmpty()

//                where gl.GLHeaderId == id &&
//                      !gl.IsDelete

//                select new
//                {
//                    gl.GLHeaderId,
//                    gl.GLKode,
//                    gl.KunjunganId,
//                    gl.NoRegistrasi,
//                    gl.JenisKunjungan,
//                    gl.PasienId,
//                    gl.TglTransaksi,
//                    gl.TglPosting,
//                    gl.SourceGL,
//                    gl.SourceTypeGL,
//                    gl.SourceId,
//                    gl.SourceNumber,
//                    gl.GLStatus,
//                    gl.Keterangan,
//                    gl.CreateDateTime,
//                    gl.UpdateDateTime,

//                    CreateByName = user != null ? user.FullName : null,

//                    kunjungan.DokterId,
//                    NamaDokter = dokter.NmDokter
//                })
//                .FirstOrDefaultAsync();

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Header tidak ditemukan"
//                });
//            }

//            return Ok(new
//            {
//                message = "success",
//                data
//            });
//        }

//        // ================= CREATE =================
//        [HttpPost]
//        public async Task<IActionResult> Create(
//            [FromBody] GLHeader model)
//        {
//            if (model.KunjunganId == Guid.Empty)
//            {
//                return BadRequest(new
//                {
//                    message = "KunjunganId wajib diisi"
//                });
//            }

//            if (model.SourceId == Guid.Empty)
//            {
//                return BadRequest(new
//                {
//                    message = "SourceId wajib diisi"
//                });
//            }

//            var email = User
//                .FindFirst(ClaimTypes.NameIdentifier)?
//                .Value;

//            var user = await _context.UserActives
//                .FirstOrDefaultAsync(x => x.Email == email);

//            if (user == null)
//                return Unauthorized();

//            /*
//             * Sesuaikan nama DbSet dan nama field Kunjungan
//             * dengan entity Kunjungan pada project Anda.
//             */
//            var kunjungan = await _context.Kunjungans
//                .Where(x =>
//                    x.KunjunganID == model.KunjunganId &&
//                    x.IsDelete == false)
//                .Select(x => new
//                {
//                    x.KunjunganID,
//                    x.NoRegistrasi,
//                    x.JenisKunjungan,
//                    x.PasienId
//                })
//                .FirstOrDefaultAsync();

//            if (kunjungan == null)
//            {
//                return BadRequest(new
//                {
//                    message = "Data kunjungan tidak ditemukan"
//                });
//            }

//            var sourceExists = await _context.GLHeaders
//                .AnyAsync(x =>
//                    x.SourceId == model.SourceId &&
//                    x.SourceGL == model.SourceGL &&
//                    x.SourceTypeGL == model.SourceTypeGL &&
//                    x.GLStatus != "REVERSED" &&
//                    x.IsDelete == false);

//            if (sourceExists)
//            {
//                return Conflict(new
//                {
//                    message =
//                        "Transaksi tersebut sudah pernah dibuatkan GL"
//                });
//            }

//            model.GLHeaderId = Guid.NewGuid();
//            model.GLKode = await GenerateGLKode();

//            model.NoRegistrasi = kunjungan.NoRegistrasi;
//            model.JenisKunjungan = kunjungan.JenisKunjungan;
//            model.PasienId = kunjungan.KunjunganID;

//            model.TglPosting = DateTime.UtcNow;
//            model.GLStatus = "POSTED";

//            model.CreateBy = user.UserActiveId;
//            model.CreateDateTime = DateTime.UtcNow;
//            model.IsDelete = false;

//            _context.GLHeaders.Add(model);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Header berhasil dibuat",
//                data = new
//                {
//                    model.GLHeaderId,
//                    model.GLKode
//                }
//            });
//        }

//        // ================= UPDATE =================
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(
//            Guid id,
//            [FromBody] GLHeader model)
//        {
//            var data = await _context.GLHeaders
//                .FirstOrDefaultAsync(x =>
//                    x.GLHeaderId == id &&
//                    x.IsDelete == false);

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Header tidak ditemukan"
//                });
//            }

//            var email = User
//                .FindFirst(ClaimTypes.NameIdentifier)?
//                .Value;

//            var user = await _context.UserActives
//                .FirstOrDefaultAsync(x => x.Email == email);

//            if (user == null)
//                return Unauthorized();

//            data.TglTransaksi = model.TglTransaksi;
//            data.SourceGL = model.SourceGL;
//            data.SourceTypeGL = model.SourceTypeGL;
//            data.SourceId = model.SourceId;
//            data.SourceNumber = model.SourceNumber;
//            data.GLStatus = model.GLStatus;
//            data.Keterangan = model.Keterangan;

//            data.UpdateBy = user.UserActiveId;
//            data.UpdateDateTime = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Header berhasil diperbarui"
//            });
//        }

//        // ================= DELETE SOFT =================
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            var data = await _context.GLHeaders
//                .FirstOrDefaultAsync(x =>
//                    x.GLHeaderId == id &&
//                    x.IsDelete == false);

//            if (data == null)
//            {
//                return NotFound(new
//                {
//                    message = "GL Header tidak ditemukan"
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

//            /*
//             * Detail ikut soft delete ketika header dihapus.
//             */
//            var details = await _context.GLDetails
//                .Where(x =>
//                    x.GLHeaderId == id &&
//                    x.IsDelete == false)
//                .ToListAsync();

//            foreach (var detail in details)
//            {
//                detail.IsDelete = true;
//                detail.DeleteBy = user.UserActiveId;
//                detail.DeleteDateTime = DateTime.UtcNow;
//            }

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "GL Header berhasil dihapus"
//            });
//        }

//        // ================= GENERATE GL KODE =================
//        private async Task<string> GenerateGLKode()
//        {
//            var nowJakarta = DateTime.UtcNow.AddHours(7);

//            var kodeDalamBulan = await _context.GLHeaders
//                .Where(x =>
//                    x.TglPosting.Year == nowJakarta.Year &&
//                    x.TglPosting.Month == nowJakarta.Month)
//                .Select(x => x.GLKode)
//                .ToListAsync();

//            var sequenceTerakhir = kodeDalamBulan
//                .Select(x =>
//                {
//                    if (string.IsNullOrWhiteSpace(x))
//                        return 0;

//                    var split = x.Split('-');

//                    if (split.Length < 3)
//                        return 0;

//                    return int.TryParse(split.Last(), out var nomor)
//                        ? nomor
//                        : 0;
//                })
//                .DefaultIfEmpty(0)
//                .Max();

//            var sequenceBaru = sequenceTerakhir + 1;

//            return $"GL-{nowJakarta:yyMMdd}-{sequenceBaru:00000}";
//        }
//        // ================= PAGED =================
//        // ================= PAGED + FILTER =================
//        [HttpGet("paged")]
//        public async Task<IActionResult> Paged(
//            int page = 1,
//            int perPage = 10,
//            string? search = null,
//            string? sourceTypeGL = null,
//            string? glStatus = null)
//        {
//            if (page < 1)
//                page = 1;

//            if (perPage < 1)
//                perPage = 10;

//            if (perPage > 100)
//                perPage = 100;

//            var query =
//                from gl in _context.GLHeaders

//                join userData in _context.UserActives
//                    on gl.CreateBy equals userData.UserActiveId
//                    into userJoin
//                from user in userJoin.DefaultIfEmpty()

//                join kunjungan in _context.Kunjungans
//                    on gl.KunjunganId equals kunjungan.KunjunganID
//                    into kunjunganJoin
//                from kunjungan in kunjunganJoin.DefaultIfEmpty()

//                join dokter in _context.Dokters
//                    on kunjungan.DokterId equals dokter.DokterId
//                    into dokterJoin
//                from dokter in dokterJoin.DefaultIfEmpty()

//                where !gl.IsDelete

//                select new
//                {
//                    gl.GLHeaderId,
//                    gl.GLKode,
//                    gl.KunjunganId,
//                    gl.NoRegistrasi,
//                    gl.JenisKunjungan,
//                    gl.PasienId,
//                    gl.TglTransaksi,
//                    gl.TglPosting,
//                    gl.SourceGL,
//                    gl.SourceTypeGL,
//                    gl.SourceId,
//                    gl.SourceNumber,
//                    gl.GLStatus,
//                    gl.Keterangan,
//                    gl.CreateDateTime,

//                    CreateByName = user != null
//                        ? user.FullName
//                        : null,

//                    DokterId = kunjungan.DokterId,

//                    NamaDokter = dokter != null
//                        ? dokter.NmDokter
//                        : null
//                };

//            // ================= FILTER SOURCE TYPE GL =================
//            if (!string.IsNullOrWhiteSpace(sourceTypeGL))
//            {
//                var sourceTypeKeyword = sourceTypeGL.Trim();

//                query = query.Where(x =>
//                    x.SourceTypeGL != null &&
//                    EF.Functions.ILike(
//                        x.SourceTypeGL,
//                        sourceTypeKeyword));
//            }

//            // ================= FILTER GL STATUS =================
//            if (!string.IsNullOrWhiteSpace(glStatus))
//            {
//                var statusKeyword = glStatus.Trim();

//                query = query.Where(x =>
//                    x.GLStatus != null &&
//                    EF.Functions.ILike(
//                        x.GLStatus,
//                        statusKeyword));
//            }

//            // ================= SEARCH =================
//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = $"%{search.Trim()}%";

//                query = query.Where(x =>
//                    EF.Functions.ILike(x.GLKode!, keyword) ||
//                    EF.Functions.ILike(x.NoRegistrasi!, keyword) ||
//                    EF.Functions.ILike(x.JenisKunjungan!, keyword) ||
//                    EF.Functions.ILike(x.SourceGL!, keyword) ||
//                    EF.Functions.ILike(x.SourceTypeGL!, keyword) ||
//                    EF.Functions.ILike(x.SourceNumber!, keyword) ||
//                    EF.Functions.ILike(x.GLStatus!, keyword) ||
//                    EF.Functions.ILike(x.Keterangan!, keyword));
//            }

//            var totalRows = await query.CountAsync();

//            var data = await query
//                .OrderByDescending(x => x.TglPosting)
//                .Skip((page - 1) * perPage)
//                .Take(perPage)
//                .ToListAsync();

//            return Ok(new
//            {
//                message = "success",
//                filter = new
//                {
//                    sourceTypeGL,
//                    glStatus,
//                    search
//                },
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