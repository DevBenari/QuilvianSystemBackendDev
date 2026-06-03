using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AR.Models;
using QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ARSettlementController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ARSettlementController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ARSettlementController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ARSettlementController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("viewtabel")]
        public async Task<IActionResult> GetAllJoinData()
        {
            try
            {
                // =========================
                // RAW DATA
                // =========================

                var rawData = await (
                    from ar in _applicationDbContext.ARHeaders

                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ar.AsuransiId equals a.AsuransiId

                    join d in _applicationDbContext.ARDetails
                        on ar.ARHeaderId equals d.ARHeaderId

                    join payment in _applicationDbContext.DetailInvoiceReceiveds
                        on d.KunjunganId equals payment.KunjunganId
                        into paymentGroup

                    from payment in paymentGroup.DefaultIfEmpty()

                    select new
                    {
                        // HEADER

                        ar.ARHeaderId,
                        ar.AsuransiId,
                        AsuransiName = a.NamaAsuransi,
                        ar.Tipe_Kunjungan,
                        ar.JenisAR,
                        ar.NoInvoice,
                        ar.TglPembuatanInvoice,
                        ar.TglJatuhTempo,
                        ar.DueDate,
                        ar.TotalInvoice,

                        // DETAIL

                        d.ARDetailId,
                        d.KunjunganId,
                        d.PasienId,
                        d.NoRM,
                        d.NamaPasien,
                        d.NoBilling,
                        d.NoRegistrasi,
                        d.TglKunjungan,
                        d.TglKeluar,
                        d.TotalPiutang,
                        d.TotalPembayaran,
                        d.DiskonTagihan,
                        d.SelisihTagihan,
                        d.TotalSetelahDiskon,
                        d.IsCanceled,
                        d.Keterangan,

                        // PAYMENT

                        DetailInvoicePaymentId = payment != null
                            ? (Guid?)payment.DetailInvoicePaymentId
                            : null,

                        DetailReceivedPaymentId = payment != null
                            ? payment.DetailReceivedPaymentId
                            : null,

                        TglTerima = payment != null
                            ? payment.TglTerima
                            : null,

                        TglKirim = payment != null
                            ? payment.TglKirim
                            : null,

                        TglTagihan = payment != null
                            ? payment.TglTagihan
                            : null,

                        PiutangTerbayar = payment != null
                            ? payment.PiutangTerbayar
                            : 0,

                        PembayaranKe = payment != null
                            ? payment.PembayaranKe
                            : 0,

                        TotalPiutangDRP = payment != null
                            ? payment.TotalPiutang
                            : 0,

                        TglJaatuhTempo = payment != null
                            ? payment.TglJaatuhTempo
                            : null,

                        IsTerbayar = payment != null
                            ? payment.IsTerbayar
                            : false,

                        KeteranganDRP = payment != null
                            ? payment.Keterangan
                            : null
                    }
                ).ToListAsync();

                // =========================
                // GROUPING
                // =========================

                var result = rawData
                    .GroupBy(x => new
                    {
                        x.ARHeaderId,
                        x.AsuransiId,
                        x.AsuransiName,
                        x.Tipe_Kunjungan,
                        x.JenisAR,
                        x.NoInvoice,
                        x.TglPembuatanInvoice,
                        x.TglJatuhTempo,
                        x.DueDate,
                        x.TotalInvoice
                    })
                    .Select(header => new
                    {
                        header.Key.ARHeaderId,
                        header.Key.AsuransiId,
                        header.Key.AsuransiName,
                        header.Key.Tipe_Kunjungan,
                        header.Key.JenisAR,
                        header.Key.NoInvoice,
                        header.Key.TglPembuatanInvoice,
                        header.Key.TglJatuhTempo,
                        header.Key.DueDate,
                        header.Key.TotalInvoice,

                        ArDetails = header
                            .GroupBy(d => new
                            {
                                d.ARDetailId,
                                d.KunjunganId,
                                d.PasienId,
                                d.NoRM,
                                d.NamaPasien,
                                d.NoBilling,
                                d.NoRegistrasi,
                                d.TglKunjungan,
                                d.TglKeluar,
                                d.TotalPiutang,
                                d.TotalPembayaran,
                                d.DiskonTagihan,
                                d.SelisihTagihan,
                                d.TotalSetelahDiskon,
                                d.IsCanceled,
                                d.Keterangan
                            })
                            .Select(detail => new
                            {
                                detail.Key.ARDetailId,
                                detail.Key.KunjunganId,
                                detail.Key.PasienId,
                                detail.Key.NoRM,
                                detail.Key.NamaPasien,
                                detail.Key.NoBilling,
                                detail.Key.NoRegistrasi,
                                detail.Key.TglKunjungan,
                                detail.Key.TglKeluar,
                                detail.Key.TotalPiutang,
                                detail.Key.TotalPembayaran,
                                detail.Key.DiskonTagihan,
                                detail.Key.SelisihTagihan,
                                detail.Key.TotalSetelahDiskon,
                                detail.Key.IsCanceled,
                                detail.Key.Keterangan,

                                Payments = detail
                                    .Where(p => p.DetailInvoicePaymentId != null)
                                    .Select(p => new
                                    {
                                        p.DetailInvoicePaymentId,
                                        p.DetailReceivedPaymentId,
                                        p.TglTerima,
                                        p.TglKirim,
                                        p.TglTagihan,
                                        p.PiutangTerbayar,
                                        p.PembayaranKe,
                                        p.TotalPiutangDRP,
                                        p.TglJaatuhTempo,
                                        p.IsTerbayar,
                                        p.KeteranganDRP
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    message = "Data berhasil diambil",
                    total = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("sattlementdetail/{idkunjungan}")]
        public async Task<IActionResult> GetAllJoinData(Guid idkunjungan)
        {
            try
            {
                // =========================
                // RAW DATA
                // =========================

                var rawData = await (
                    from ar in _applicationDbContext.ARHeaders

                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ar.AsuransiId equals a.AsuransiId

                    join d in _applicationDbContext.ARDetails
                        on ar.ARHeaderId equals d.ARHeaderId

                    join payment in _applicationDbContext.DetailInvoiceReceiveds
                        on d.KunjunganId equals payment.KunjunganId
                        into paymentGroup
                    from payment in paymentGroup.DefaultIfEmpty()

                    join rp in _applicationDbContext.ReceivedPayments
                        on ar.NoInvoice equals rp.NoInvoice into rpGroup
                    from rp in rpGroup.DefaultIfEmpty()

                    where d.KunjunganId == idkunjungan  // Filter berdasarkan idkunjungan

                    select new
                    {
                        // HEADER

                        ar.ARHeaderId,
                        ar.AsuransiId,
                        AsuransiName = a.NamaAsuransi,
                        ar.Tipe_Kunjungan,
                        ar.JenisAR,
                        ar.NoInvoice,
                        ar.TglPembuatanInvoice,
                        ar.TglJatuhTempo,
                        ar.DueDate,
                        ar.TotalInvoice,

                        // DETAIL

                        d.ARDetailId,
                        d.KunjunganId,
                        d.PasienId,
                        d.NoRM,
                        d.NamaPasien,
                        d.NoBilling,
                        d.NoRegistrasi,
                        d.TglKunjungan,
                        d.TglKeluar,
                        d.TotalPiutang,
                        d.TotalPembayaran,
                        d.DiskonTagihan,
                        d.SelisihTagihan,
                        d.TotalSetelahDiskon,
                        d.IsCanceled,
                        d.Keterangan,

                        // RECEIVED PAYMENT

                        ReceivedPaymentId = rp != null
                        ? (Guid?)rp.ReceivedPaymentId
                        : null,

                        IsCancelledReceivedPayment = rp != null
                        ? rp.IsCanceled
                        : false,

                        // PAYMENT

                        DetailInvoicePaymentId = payment != null
                            ? (Guid?)payment.DetailInvoicePaymentId
                            : null,

                        DetailReceivedPaymentId = payment != null
                            ? payment.DetailReceivedPaymentId
                            : null,

                        TglTerima = payment != null
                            ? payment.TglTerima
                            : null,

                        TglKirim = payment != null
                            ? payment.TglKirim
                            : null,

                        TglTagihan = payment != null
                            ? payment.TglTagihan
                            : null,

                        PiutangTerbayar = payment != null
                            ? payment.PiutangTerbayar
                            : 0,

                        PembayaranKe = payment != null
                            ? payment.PembayaranKe
                            : 0,

                        TotalPiutangDRP = payment != null
                            ? payment.TotalPiutang
                            : 0,

                        TglJaatuhTempo = payment != null
                            ? payment.TglJaatuhTempo
                            : null,

                        IsTerbayar = payment != null
                            ? payment.IsTerbayar
                            : false,

                        KeteranganDRP = payment != null
                            ? payment.Keterangan
                            : null
                    }
                ).ToListAsync();

                // Cek jika data tidak ditemukan
                if (!rawData.Any())
                {
                    return NotFound(new
                    {
                        message = "Data kunjungan tidak ditemukan"
                    });
                }

                // =========================
                // GROUPING
                // =========================

                var result = rawData
                    .GroupBy(x => new
                    {
                        x.ARHeaderId,
                        x.AsuransiId,
                        x.AsuransiName,
                        x.Tipe_Kunjungan,
                        x.JenisAR,
                        x.NoInvoice,
                        x.TglPembuatanInvoice,
                        x.TglJatuhTempo,
                        x.DueDate,
                        x.TotalInvoice,
                        x.IsCancelledReceivedPayment
                    })
                    .Select(header => new
                    {
                        header.Key.ARHeaderId,
                        header.Key.AsuransiId,
                        header.Key.AsuransiName,
                        header.Key.Tipe_Kunjungan,
                        header.Key.JenisAR,
                        header.Key.NoInvoice,
                        header.Key.TglPembuatanInvoice,
                        header.Key.TglJatuhTempo,
                        header.Key.DueDate,
                        header.Key.TotalInvoice,
                        header.Key.IsCancelledReceivedPayment,

                        ArDetails = header
                            .GroupBy(d => new
                            {
                                d.ARDetailId,
                                d.KunjunganId,
                                d.PasienId,
                                d.NoRM,
                                d.NamaPasien,
                                d.NoBilling,
                                d.NoRegistrasi,
                                d.TglKunjungan,
                                d.TglKeluar,
                                d.TotalPiutang,
                                d.TotalPembayaran,
                                d.DiskonTagihan,
                                d.SelisihTagihan,
                                d.TotalSetelahDiskon,
                                d.IsCanceled,
                                d.Keterangan
                            })
                            .Select(detail => new
                            {
                                detail.Key.ARDetailId,
                                detail.Key.KunjunganId,
                                detail.Key.PasienId,
                                detail.Key.NoRM,
                                detail.Key.NamaPasien,
                                detail.Key.NoBilling,
                                detail.Key.NoRegistrasi,
                                detail.Key.TglKunjungan,
                                detail.Key.TglKeluar,
                                detail.Key.TotalPiutang,
                                detail.Key.TotalPembayaran,
                                detail.Key.DiskonTagihan,
                                detail.Key.SelisihTagihan,
                                detail.Key.TotalSetelahDiskon,
                                detail.Key.IsCanceled,
                                detail.Key.Keterangan,

                                Payments = detail
                                    .Where(p => p.DetailInvoicePaymentId != null)
                                    .Select(p => new
                                    {
                                        p.DetailInvoicePaymentId,
                                        p.DetailReceivedPaymentId,
                                        p.TglTerima,
                                        p.TglKirim,
                                        p.TglTagihan,
                                        p.PiutangTerbayar,
                                        p.PembayaranKe,
                                        p.TotalPiutangDRP,
                                        p.TglJaatuhTempo,
                                        p.IsTerbayar,
                                        p.KeteranganDRP
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    message = "Data berhasil diambil",
                    total = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("sattlementdetail")]
        public async Task<IActionResult> GetAllSettlementDetail()
        {
            try
            {
                var result = await (
                    from ar in _applicationDbContext.ARHeaders
                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ar.AsuransiId equals a.AsuransiId
                    join d in _applicationDbContext.ARDetails
                        on ar.ARHeaderId equals d.ARHeaderId
                    select new
                    {
                        // DETAIL
                        d.ARDetailId,
                        d.KunjunganId,
                        d.PasienId,
                        d.NoRM,
                        d.NamaPasien,
                        d.NoBilling,
                        d.NoRegistrasi,
                        d.TglKunjungan,
                        d.TglKeluar,
                        d.TotalPiutang,
                        d.TotalPembayaran,
                        d.DiskonTagihan,
                        d.SelisihTagihan,
                        d.TotalSetelahDiskon,
                        d.IsCanceled,
                        d.Keterangan,

                        // HEADER
                        ar.ARHeaderId,
                        ar.AsuransiId,
                        AsuransiName = a.NamaAsuransi,
                        ar.Tipe_Kunjungan,
                        ar.JenisAR,
                        ar.NoInvoice,
                        ar.TglPembuatanInvoice,
                        ar.TglJatuhTempo,
                        ar.DueDate,
                        ar.TotalInvoice
                    }
                ).ToListAsync();

                return Ok(new
                {
                    message = "Data berhasil diambil",
                    total = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }
        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedARSettlement(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "NamaPasien",
            string? sortDirection = "desc",
            string? arHeaderId = null,
            string? NoInvoice = null,
            string? AsuransiName = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // =========================
                // BASE QUERY (JOIN)
                // =========================
                var query =
                    from ar in _applicationDbContext.ARHeaders.AsNoTracking()
                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on ar.AsuransiId equals a.AsuransiId
                    join d in _applicationDbContext.ARDetails.AsNoTracking()
                        on ar.ARHeaderId equals d.ARHeaderId
                    where ar.IsDelete == false
                    select new
                    {
                        // DETAIL
                        d.ARDetailId,
                        d.KunjunganId,
                        d.PasienId,
                        d.NoRM,
                        d.NamaPasien,
                        d.NoBilling,
                        d.NoRegistrasi,
                        d.TglKunjungan,
                        d.TglKeluar,
                        d.TotalPiutang,
                        d.TotalPembayaran,
                        d.DiskonTagihan,
                        d.SelisihTagihan,
                        d.TotalSetelahDiskon,
                        d.IsCanceled,
                        d.Keterangan,

                        // HEADER
                        ar.ARHeaderId,
                        ar.AsuransiId,
                        AsuransiName = a.NamaAsuransi,
                        ar.Tipe_Kunjungan,
                        ar.JenisAR,
                        ar.NoInvoice,
                        ar.TglPembuatanInvoice,
                        ar.TglJatuhTempo,
                        ar.DueDate,
                        ar.TotalInvoice
                    };

                // SEARCH GLOBAL
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoRM ?? "", keyword) ||
                        EF.Functions.ILike(x.NamaPasien ?? "", keyword) ||
                        EF.Functions.ILike(x.NoInvoice ?? "", keyword) ||
                        EF.Functions.ILike(x.AsuransiName ?? "", keyword)
                    );
                }

                // FILTER NO INVOICE
                if (!string.IsNullOrWhiteSpace(NoInvoice))
                {
                    var keywordInvoice = $"%{NoInvoice.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoInvoice ?? "", keywordInvoice)
                    );
                }

                // FILTER ASURANSI
                if (!string.IsNullOrWhiteSpace(AsuransiName))
                {
                    var keywordAsuransi = $"%{AsuransiName.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.AsuransiName ?? "", keywordAsuransi)
                    );
                }

                // FILTER DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();

                    var endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.TglPembuatanInvoice >= startUtc &&
                        x.TglPembuatanInvoice <= endUtc
                    );
                }

                // =========================
                // SORTING
                // =========================
                var sortColumn = orderBy?.ToLower() ?? "namapasien";
                var isDesc = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "namapasien" =>
                        isDesc ? query.OrderByDescending(x => x.NamaPasien)
                               : query.OrderBy(x => x.NamaPasien),

                    "norm" =>
                        isDesc ? query.OrderByDescending(x => x.NoRM)
                               : query.OrderBy(x => x.NoRM),

                    "noinvoice" =>
                        isDesc ? query.OrderByDescending(x => x.NoInvoice)
                               : query.OrderBy(x => x.NoInvoice),

                    "namaasuransi" =>
                        isDesc ? query.OrderByDescending(x => x.AsuransiName)
                               : query.OrderBy(x => x.AsuransiName),

                    "tglpembuataninvoice" =>
                        isDesc ? query.OrderByDescending(x => x.TglPembuatanInvoice)
                               : query.OrderBy(x => x.TglPembuatanInvoice),

                    "totalpiutang" =>
                        isDesc ? query.OrderByDescending(x => x.TotalPiutang)
                               : query.OrderBy(x => x.TotalPiutang),

                    _ =>
                        query.OrderByDescending(x => x.TglPembuatanInvoice)
                };

                // =========================
                // PAGINATION
                // =========================
                var totalRows = await query.CountAsync();

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    try
        //    {
        //        var data = await _applicationDbContext.ARSettlements
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(x =>
        //                x.SettlementARId == id);

        //        if (data == null)
        //        {
        //            return NotFound(new
        //            {
        //                message = "Data tidak ditemukan."
        //            });
        //        }

        //        return Ok(new
        //        {
        //            status = "success",
        //            data
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        return StatusCode(500, new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //}

        //// =====================================================
        //// CREATE
        //// =====================================================

        //[HttpPost]
        //public async Task<IActionResult> Create(
        //    [FromBody] ARSettlementViewModel vm)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var data = new ARSettlement
        //        {
        //            SettlementARId = Guid.NewGuid(),

        //            KunjunganId = vm.KunjunganId,
        //            PasienId = vm.PasienId,

        //            NamaPasien = vm.NamaPasien,
        //            NoInvoice = vm.NoInvoice,

        //            BeginingBalance = vm.BeginingBalance,
        //            EndingBalance = vm.EndingBalance
        //        };

        //        _applicationDbContext.ARSettlements.Add(data);

        //        int result =
        //            await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Created("", new
        //            {
        //                message = "Tambah data berhasil."
        //            });
        //        }

        //        return StatusCode(500, new
        //        {
        //            message = "Gagal menyimpan data."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        return StatusCode(500, new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //}

        //// =====================================================
        //// UPDATE
        //// =====================================================

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(
        //    Guid id,
        //    [FromBody] ARSettlementViewModel vm)
        //{
        //    try
        //    {
        //        var data =
        //            await _applicationDbContext.ARSettlements
        //            .FirstOrDefaultAsync(x =>
        //                x.SettlementARId == id);

        //        if (data == null)
        //        {
        //            return NotFound(new
        //            {
        //                message = "Data tidak ditemukan."
        //            });
        //        }

        //        data.KunjunganId = vm.KunjunganId;
        //        data.PasienId = vm.PasienId;

        //        data.NamaPasien = vm.NamaPasien;
        //        data.NoInvoice = vm.NoInvoice;

        //        data.BeginingBalance = vm.BeginingBalance;
        //        data.EndingBalance = vm.EndingBalance;

        //        _applicationDbContext.ARSettlements.Update(data);

        //        int result =
        //            await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Ok(new
        //            {
        //                message = "Update data berhasil."
        //            });
        //        }

        //        return StatusCode(500, new
        //        {
        //            message = "Gagal update data."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        return StatusCode(500, new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //}

        //// =====================================================
        //// DELETE
        //// =====================================================

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        var data =
        //            await _applicationDbContext.ARSettlements
        //            .FirstOrDefaultAsync(x =>
        //                x.SettlementARId == id);

        //        if (data == null)
        //        {
        //            return NotFound(new
        //            {
        //                message = "Data tidak ditemukan."
        //            });
        //        }

        //        _applicationDbContext.ARSettlements.Remove(data);

        //        int result =
        //            await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Ok(new
        //            {
        //                message = "Delete berhasil."
        //            });
        //        }

        //        return StatusCode(500, new
        //        {
        //            message = "Gagal delete data."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        return StatusCode(500, new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //}
    
    }
}