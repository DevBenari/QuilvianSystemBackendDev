using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ResepController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ResepController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResep(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from r in _applicationDbContext.Reseps
                         join u in _applicationDbContext.UserActives
                             on r.CreateBy equals u.UserActiveId
                         where !r.IsDelete // jika ada IsDelete
                         select new
                         {
                             r.ResepId,
                             r.KunjunganId,
                             r.CreateDateTime,
                             r.CreateBy,
                             r.AntrianRegistrasi,
                             r.AntrianResep,
                             r.AsuransiId,
                             r.NamaAsuransi,
                             r.PasienId,
                             r.NamaPasien,
                             r.PoliklinikId,
                             r.NamaPoliklinik,
                             r.DokterId,
                             r.NamaDokter,
                             r.StatusPembuatanResep,
                             r.StatusPengambilanResep,
                             r.IsCancelled,
                             r.IsLunas,
                             r.RanapId,
                             TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                             CreateByName = u.FullName,

                             DaftarObat = (from d in _applicationDbContext.DetailReseps
                                           join o in _applicationDbContext.Obats
                                               on d.ObatId equals o.ObatId
                                           where d.ResepId == r.ResepId && (d.IsRacikan == false || d.IsRacikan == null)
                                           select new
                                           {
                                               d.DetailResepId,
                                               d.ResepId,
                                               d.ObatId,
                                               o.ObatName,
                                               d.Qty,
                                               d.HargaObat,
                                               d.TotalHargaObat,
                                               d.Signa,
                                               d.SignaTambahan,
                                               d.TakaranDosis,
                                               d.IsIteratur,
                                               d.JumlahIteratur,
                                               TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               d.JarakPenebusan,
                                               d.StatusCoverObat,
                                               d.StatusPengambilanObat,
                                               d.CreateBy,
                                               d.CreateDateTime
                                           }).ToList(),

                             DaftarRacikan = (from d in _applicationDbContext.DetailReseps
                                              join ra in _applicationDbContext.Racikans
                                                  on d.RacikanId equals ra.RacikanId
                                              where d.ResepId == r.ResepId && d.IsRacikan == true
                                              select new
                                              {
                                                  ra.RacikanId,
                                                  ra.NamaRacikan,
                                                  d.Qty,
                                                  d.Signa,
                                                  d.SignaTambahan,
                                                  d.HargaObat,
                                                  d.TotalHargaObat,
                                                  ra.CreateBy,
                                                  ra.CreateDateTime,

                                                  DaftarRacikanDetail = (from rd in _applicationDbContext.RacikanDetails
                                                                         join ob in _applicationDbContext.Obats
                                                                             on rd.ObatId equals ob.ObatId
                                                                         where rd.RacikanId == ra.RacikanId
                                                                         select new
                                                                         {
                                                                             rd.DetailRacikanId,
                                                                             rd.ObatId,
                                                                             ob.ObatName,
                                                                             rd.QtyUsed,
                                                                             rd.KomposisiDosis,
                                                                             rd.CreateBy,
                                                                             rd.CreateDateTime
                                                                         }).ToList()
                                              }).ToList()
                         }).OrderByDescending(a => a.CreateDateTime);

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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
        public async Task<IActionResult> GetResepById(Guid id)
        {
            var resep = await _applicationDbContext.Reseps
                .FirstOrDefaultAsync(r => r.ResepId == id);

            if (resep == null)
                return NotFound(new { message = "Resep tidak ditemukan!" });

            var user = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.UserActiveId == resep.CreateBy);

            var daftarObat = await _applicationDbContext.DetailReseps
                .Where(d => d.ResepId == resep.ResepId && (d.IsRacikan == false || d.IsRacikan == null))
                .Join(_applicationDbContext.Obats,
                      d => d.ObatId,
                      o => o.ObatId,
                      (d, o) => new
                      {
                          d.DetailResepId,
                          d.ObatId,
                          o.ObatName,
                          d.Qty,
                          d.HargaObat,
                          d.TotalHargaObat,
                          d.Signa,
                          d.SignaTambahan,
                          d.TakaranDosis,
                          d.IsIteratur,
                          d.JumlahIteratur,
                          MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                          TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                          d.JarakPenebusan,
                          d.StatusCoverObat,
                          d.StatusPengambilanObat,
                          d.CreateBy,
                          d.CreateDateTime
                      })
                .ToListAsync();

            var daftarRacikanRaw = await _applicationDbContext.DetailReseps
                .Where(d => d.ResepId == resep.ResepId && d.RacikanId != null)
                .Join(_applicationDbContext.Racikans,
                    d => d.RacikanId,
                    ra => ra.RacikanId,
                    (d, ra) => new
                    {
                        ra.RacikanId,
                        ra.NamaRacikan,
                        d.Qty,
                        d.Signa,
                        d.SignaTambahan,
                        ra.CreateBy,
                        ra.CreateDateTime,
                        DaftarRacikanDetail = _applicationDbContext.RacikanDetails
                            .Where(rd => rd.RacikanId == ra.RacikanId)
                            .Join(_applicationDbContext.Obats,
                                  rd => rd.ObatId,
                                  ob => ob.ObatId,
                                  (rd, ob) => new
                                  {
                                      rd.DetailRacikanId,
                                      rd.ObatId,
                                      ob.ObatName,
                                      rd.QtyUsed,
                                      rd.KomposisiDosis,
                                      rd.CreateBy,
                                      rd.CreateDateTime
                                  })
                            .ToList()
                    })
                .ToListAsync(); // materialize dulu

            // Ambil unik racikan per RacikanId
            var daftarRacikan = daftarRacikanRaw
                .GroupBy(r => r.RacikanId)
                .Select(g => g.First())
                .ToList();

            var result = new
            {
                resep.ResepId,
                resep.KunjunganId,
                resep.AsuransiId,
                resep.NamaAsuransi,
                resep.PasienId,
                resep.NamaPasien,
                resep.PoliklinikId,
                resep.NamaPoliklinik,
                resep.DokterId,
                resep.NamaDokter,
                resep.AntrianResep,
                resep.AntrianRegistrasi,
                resep.StatusPembuatanResep,
                resep.StatusPengambilanResep,
                resep.IsCancelled,
                resep.IsLunas,
                resep.RanapId,
                resep.CreateBy,
                TanggalPembuatanResep = resep.TanggalPembuatanResep?.ToString("yyyy-MM-dd"),
                DaftarObat = daftarObat,
                DaftarRacikan = daftarRacikan,
            };
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResep([FromBody] ResepViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid!" });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User tidak ditemukan!" });

                var kunjungan = await _applicationDbContext.Kunjungans.FirstOrDefaultAsync(k => k.KunjunganID == vm.KunjunganId);
                if (kunjungan == null)
                    return NotFound(new { message = "Data kunjungan tidak ditemukan." });

                string antrian = kunjungan.Antrian;
                var today = DateTime.UtcNow.Date;
                var todayString = today.ToString("yyyyMMdd");

                var lastResep = await _applicationDbContext.Reseps
                    .Where(r => r.CreateDateTime.Date == today)
                    .OrderByDescending(r => r.AntrianResep)
                    .FirstOrDefaultAsync();
                int nextAntrian = (lastResep?.AntrianResep ?? 0) + 1;

                var resep = new Resep
                {
                    ResepId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    AsuransiId = vm.AsuransiId,
                    NamaAsuransi = vm.NamaAsuransi,
                    PasienId = vm.PasienId,
                    NamaPasien = vm.NamaPasien,
                    PoliklinikId = vm.PoliklinikId,
                    NamaPoliklinik = vm.NamaPoliklinik,
                    DokterId = vm.DokterId,
                    NamaDokter = vm.NamaDokter,
                    AntrianResep = nextAntrian,
                    AntrianRegistrasi = antrian,
                    StatusPembuatanResep = vm.StatusPembuatanResep,
                    StatusPengambilanResep = false,
                    IsCancelled = false,
                    IsLunas = false,
                    TanggalPembuatanResep = DateTime.UtcNow,
                    RanapId = vm.RanapId,

                    CreateBy = getUserActive.UserActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };
                _applicationDbContext.Reseps.Add(resep);

                if (vm.DaftarObat?.Any() == true)
                {
                    var obatIds = vm.DaftarObat.Where(o => o.ObatId != null).Select(o => o.ObatId.Value).Distinct().ToList();
                    var obatDbList = await _applicationDbContext.Obats
                        .Where(o => obatIds.Contains(o.ObatId))
                        .ToDictionaryAsync(o => o.ObatId);

                    int billingIndex = await _applicationDbContext.Billings
                        .CountAsync(b => b.KunjunganId == vm.KunjunganId && b.JenisBilling.ToLower() == "obat");

                    var detailResepDict = new Dictionary<Guid, ResepDetail>();
                    var billingDict = new Dictionary<Guid, Billing>();

                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan != true))
                    {
                        if (obat.ObatId == null) continue;
                        var obatId = obat.ObatId.Value;

                        if (!obatDbList.ContainsKey(obatId))
                            return BadRequest(new { message = $"Obat tidak ditemukan: {obatId}" });

                        var obatDb = obatDbList[obatId];
                        var qtyInput = obat.Qty ?? 0;

                        if (!detailResepDict.TryGetValue(obatId, out var resepDetail))
                        {
                            resepDetail = new ResepDetail
                            {
                                DetailResepId = Guid.NewGuid(),
                                ResepId = resep.ResepId,
                                ObatId = obatId,
                                Qty = qtyInput,
                                Signa = obat.Signa,
                                SignaTambahan = obat.SignaTambahan,
                                HargaObat = obatDb.HargaJual,
                                TotalHargaObat = obatDb.HargaJual * qtyInput,
                                StatusCoverObat = obat.StatusCoverObat,
                                JenisObat = obat.JenisObat,
                                IsRacikan = false,
                                RacikanId = null,
                                TakaranDosis = obatDb.TakaranDosis,
                                StatusPengambilanObat = true,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            detailResepDict[obatId] = resepDetail;
                            _applicationDbContext.DetailReseps.Add(resepDetail);
                        }
                        else
                        {
                            resepDetail.Qty += qtyInput;
                            resepDetail.TotalHargaObat = resepDetail.HargaObat * resepDetail.Qty;
                        }

                        if (!billingDict.TryGetValue(obatId, out var billing))
                        {
                            billingIndex++;
                            billing = new Billing
                            {
                                KunjunganId = vm.KunjunganId,
                                DiskonId = vm.DiskonId,
                                BillingDate = DateTime.UtcNow,
                                BillingKode = $"{billingIndex:D3}",
                                ItemId = obatId,
                                NamaItem = obatDb.ObatName,
                                HargaItem = obatDb.HargaJual,
                                QtyItem = qtyInput,
                                SubTotalItem = obatDb.HargaJual * qtyInput,
                                JenisBilling = "Obat",
                                StatusPengambilan = true,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            billingDict[obatId] = billing;
                            _applicationDbContext.Billings.Add(billing);
                        }
                        else
                        {
                            billing.QtyItem += qtyInput;
                            billing.SubTotalItem = billing.HargaItem * billing.QtyItem;
                        }

                        if (obatDb.Stock < qtyInput)
                            return BadRequest(new { message = $"Stok obat tidak cukup: {obatDb.ObatName}" });

                        obatDb.Stock -= qtyInput;
                        _applicationDbContext.Obats.Update(obatDb);
                    }

                    // Racikan tetap ditangani seperti biasa
                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan == true))
                    {
                        if (obat.Racikan == null || !obat.Racikan.Any())
                            continue;

                        foreach (var racikan in obat.Racikan)
                        {
                            var racikanId = Guid.NewGuid();
                            int racikanCountToday = await _applicationDbContext.Racikans.CountAsync(r => r.CreateDateTime.Date == today);
                            string kodeRacikan = $"RCK-{(racikanCountToday + 1):D3}{todayString}";

                            // Buat entitas racikan
                            var racikanEntity = new Racikan
                            {
                                RacikanId = racikanId,
                                NamaRacikan = racikan.NamaRacikan,
                                Keterangan = racikan.Keterangan,
                                Signa = racikan.Signa,
                                SignaTambahan = racikan.SignaTambahan,
                                QtyRacikan = obat.Qty ?? 1, // default jika null
                                KodeRacikan = kodeRacikan,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.Racikans.Add(racikanEntity);

                            decimal totalHargaRacikan = 0;

                            // Detail racikan (komposisi)
                            foreach (var detailRacikan in racikan.DaftarRacikan)
                            {
                                var obatDbRacikan = await _applicationDbContext.Obats.FindAsync(detailRacikan.ObatId);
                                if (obatDbRacikan == null)
                                    return BadRequest(new { message = $"Obat tidak ditemukan: {detailRacikan.ObatId}" });

                                // Perhitungan jumlah pakai
                                var qtyPakai = Math.Ceiling((decimal)((detailRacikan.KomposisiDosis * racikanEntity.QtyRacikan) / obatDbRacikan.TakaranDosis));
                                var hargaOb = qtyPakai * obatDbRacikan.HargaJual;
                                totalHargaRacikan += hargaOb;

                                if (obatDbRacikan.Stock < qtyPakai)
                                    return BadRequest(new { message = $"Stok tidak cukup untuk obat: {obatDbRacikan.ObatName}" });

                                obatDbRacikan.Stock -= (int)qtyPakai;
                                _applicationDbContext.Obats.Update(obatDbRacikan);

                                // Tambahkan detail racikan
                                var racikanDetail = new RacikanDetail
                                {
                                    DetailRacikanId = Guid.NewGuid(),
                                    RacikanId = racikanId,
                                    ObatId = detailRacikan.ObatId,
                                    QtyUsed = (int)qtyPakai,
                                    KomposisiDosis = detailRacikan.KomposisiDosis,
                                    CreateBy = getUserActive.UserActiveId,
                                    CreateDateTime = DateTimeOffset.UtcNow
                                };
                                _applicationDbContext.RacikanDetails.Add(racikanDetail);
                            }

                            // Tambahkan ke tabel DetailResep
                            var resepDetail = new ResepDetail
                            {
                                DetailResepId = Guid.NewGuid(),
                                ResepId = resep.ResepId,
                                ObatId = null,
                                Qty = racikanEntity.QtyRacikan,
                                Signa = racikanEntity.Signa,
                                SignaTambahan = racikanEntity.SignaTambahan,
                                HargaObat = totalHargaRacikan,
                                TotalHargaObat = totalHargaRacikan * racikanEntity.QtyRacikan,
                                StatusCoverObat = false,
                                JenisObat = obat.JenisObat,
                                IsRacikan = true,
                                RacikanId = racikanId,
                                TakaranDosis = null,
                                StatusPengambilanObat = true,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.DetailReseps.Add(resepDetail);

                            // Tambahkan ke billing
                            billingIndex++;
                            var billing = new Billing
                            {
                                KunjunganId = vm.KunjunganId,
                                DiskonId = vm.DiskonId,
                                BillingDate = DateTime.UtcNow,
                                BillingKode = $"{billingIndex:D3}",
                                ItemId = racikanId,
                                NamaItem = racikanEntity.NamaRacikan,
                                HargaItem = totalHargaRacikan,
                                QtyItem = racikanEntity.QtyRacikan ?? 1,
                                SubTotalItem = totalHargaRacikan * (racikanEntity.QtyRacikan ?? 1),
                                JenisBilling = "Obat",
                                StatusPengambilan = true,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.Billings.Add(billing);
                        }
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();
                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid!" });

        //    try
        //    {
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //            return Unauthorized(new { message = "User tidak ditemukan!" });

        //        var kunjungan = await _applicationDbContext.Kunjungans.FirstOrDefaultAsync(k => k.KunjunganID == vm.KunjunganId);
        //        if (kunjungan == null)
        //            return NotFound(new { message = "Data kunjungan tidak ditemukan." });

        //        string antrian = kunjungan.Antrian;
        //        var today = DateTime.UtcNow.Date;
        //        var todayString = today.ToString("yyyyMMdd");

        //        var lastResep = await _applicationDbContext.Reseps
        //            .Where(r => r.CreateDateTime.Date == today)
        //            .OrderByDescending(r => r.AntrianResep)
        //            .FirstOrDefaultAsync();
        //        int nextAntrian = (lastResep?.AntrianResep ?? 0) + 1;

        //        var resep = new Resep
        //        {
        //            ResepId = Guid.NewGuid(),
        //            KunjunganId = vm.KunjunganId,
        //            AsuransiId = vm.AsuransiId,
        //            NamaAsuransi = vm.NamaAsuransi,
        //            PasienId = vm.PasienId,
        //            NamaPasien = vm.NamaPasien,
        //            PoliklinikId = vm.PoliklinikId,
        //            NamaPoliklinik = vm.NamaPoliklinik,
        //            DokterId = vm.DokterId,
        //            NamaDokter = vm.NamaDokter,
        //            AntrianResep = nextAntrian,
        //            AntrianRegistrasi = antrian,
        //            StatusPembuatanResep = vm.StatusPembuatanResep,
        //            StatusPengambilanResep = false,
        //            IsCancelled = false,
        //            IsLunas = false,
        //            TanggalPembuatanResep = DateTime.UtcNow,
        //            CreateBy = getUserActive.UserActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow
        //        };
        //        _applicationDbContext.Reseps.Add(resep);

        //        if (vm.DaftarObat?.Any() == true)
        //        {
        //            var obatIds = vm.DaftarObat.Where(o => o.ObatId != null).Select(o => o.ObatId.Value).Distinct().ToList();
        //            var obatDbList = await _applicationDbContext.Obats
        //                .Where(o => obatIds.Contains(o.ObatId))
        //                .ToDictionaryAsync(o => o.ObatId);

        //            int billingIndex = await _applicationDbContext.Billings
        //                .CountAsync(b => b.KunjunganId == vm.KunjunganId && b.JenisBilling.ToLower() == "obat");

        //            foreach (var obat in vm.DaftarObat)
        //            {
        //                var obatDb = obat.ObatId.HasValue && obatDbList.ContainsKey(obat.ObatId.Value)
        //                    ? obatDbList[obat.ObatId.Value]
        //                    : null;

        //                Guid? racikanId = null;

        //                if (obat.IsRacikan == true && obat.Racikan != null && obat.Racikan.Any())
        //                    racikanId = Guid.NewGuid();

        //                var resepDetail = new ResepDetail
        //                {
        //                    DetailResepId = Guid.NewGuid(),
        //                    ResepId = resep.ResepId,
        //                    ObatId = obat.IsRacikan == true ? null : obat.ObatId,
        //                    Qty = obat.Qty,
        //                    Signa = obat.Signa,
        //                    SignaTambahan = obat.SignaTambahan,
        //                    HargaObat = obat.HargaObat,
        //                    TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0),
        //                    StatusCoverObat = obat.IsRacikan == true ? false : obat?.StatusCoverObat,
        //                    JenisObat = obat?.JenisObat,
        //                    IsRacikan = obat?.IsRacikan,
        //                    RacikanId = racikanId,
        //                    TakaranDosis = obat?.IsRacikan == true ? null : obatDb?.TakaranDosis,
        //                    StatusPengambilanObat = true,
        //                    CreateBy = getUserActive.UserActiveId,
        //                    CreateDateTime = DateTimeOffset.UtcNow
        //                };
        //                _applicationDbContext.DetailReseps.Add(resepDetail);

        //                if (obat?.IsRacikan == true && racikanId.HasValue)
        //                {
        //                    foreach (var racikan in obat.Racikan)
        //                    {
        //                        int racikanCountToday = await _applicationDbContext.Racikans
        //                            .CountAsync(r => r.CreateDateTime.Date == today);
        //                        string kodeRacikan = $"RCK-{(racikanCountToday + 1):D3}{todayString}";

        //                        var racikanEntity = new Racikan
        //                        {
        //                            RacikanId = racikanId.Value,
        //                            NamaRacikan = racikan.NamaRacikan,
        //                            Keterangan = racikan.Keterangan,
        //                            Signa = racikan.Signa,
        //                            SignaTambahan = racikan.SignaTambahan,
        //                            QtyRacikan = racikan.QtyRacikan,
        //                            KodeRacikan = kodeRacikan,
        //                            CreateBy = getUserActive.UserActiveId,
        //                            CreateDateTime = DateTimeOffset.UtcNow
        //                        };
        //                        _applicationDbContext.Racikans.Add(racikanEntity);

        //                        decimal totalHargaRacikan = 0;

        //                        foreach (var detailRacikan in racikan.DaftarRacikan)
        //                        {
        //                            var obatDbRacikan = await _applicationDbContext.Obats.FindAsync(detailRacikan.ObatId);
        //                            if (obatDbRacikan == null)
        //                                return BadRequest(new { message = $"Obat tidak ditemukan: {detailRacikan.ObatId}" });

        //                            var qtyPakai = Math.Ceiling((decimal)((detailRacikan.KomposisiDosis * racikan.QtyRacikan) / obatDbRacikan.TakaranDosis));
        //                            var hargaOb = qtyPakai * obatDbRacikan.HargaJual;

        //                            totalHargaRacikan += hargaOb;

        //                            if (obatDbRacikan.Stock < qtyPakai)
        //                                return BadRequest(new { message = $"Stok tidak cukup untuk obat: {obatDbRacikan.ObatName}" });

        //                            obatDbRacikan.Stock -= (int)qtyPakai;
        //                            _applicationDbContext.Obats.Update(obatDbRacikan);

        //                            var racikanDetail = new RacikanDetail
        //                            {
        //                                DetailRacikanId = Guid.NewGuid(),
        //                                RacikanId = racikanId.Value,
        //                                ObatId = detailRacikan.ObatId,
        //                                QtyUsed = (int)qtyPakai,
        //                                KomposisiDosis = detailRacikan.KomposisiDosis,
        //                                CreateBy = getUserActive.UserActiveId,
        //                                CreateDateTime = DateTimeOffset.UtcNow
        //                            };
        //                            _applicationDbContext.RacikanDetails.Add(racikanDetail);
        //                        }

        //                        billingIndex++;
        //                        var billing = new Billing
        //                        {
        //                            KunjunganId = vm.KunjunganId,
        //                            DiskonId = vm.DiskonId,
        //                            BillingDate = DateTime.UtcNow,
        //                            BillingKode = $"{billingIndex:D3}",
        //                            ItemId = racikanEntity.RacikanId,
        //                            NamaItem = racikanEntity.NamaRacikan,
        //                            HargaItem = totalHargaRacikan,
        //                            QtyItem = (int)racikan.QtyRacikan,
        //                            SubTotalItem = totalHargaRacikan * (int)racikan.QtyRacikan,
        //                            JenisBilling = "Obat",
        //                            StatusPengambilan = true,
        //                            CreateBy = getUserActive.UserActiveId,
        //                            CreateDateTime = DateTimeOffset.UtcNow
        //                        };
        //                        _applicationDbContext.Billings.Add(billing);
        //                    }
        //                }
        //                else if (obatDb != null)
        //                {
        //                    if (obatDb.Stock < obat.Qty)
        //                        return BadRequest(new { message = $"Stok obat tidak cukup: {obatDb.ObatName}" });

        //                    obatDb.Stock -= (int)obat.Qty;
        //                    _applicationDbContext.Obats.Update(obatDb);

        //                    billingIndex++;
        //                    var billing = new Billing
        //                    {
        //                        KunjunganId = vm.KunjunganId,
        //                        DiskonId = vm.DiskonId,
        //                        BillingDate = DateTime.UtcNow,
        //                        BillingKode = $"{billingIndex:D3}",
        //                        ItemId = obat.ObatId,
        //                        NamaItem = obatDb.ObatName,
        //                        HargaItem = obatDb.HargaJual,
        //                        QtyItem = (int)obat.Qty,
        //                        SubTotalItem = (int)(obatDb.HargaJual * obat.Qty),
        //                        JenisBilling = "Obat",
        //                        StatusPengambilan = true,
        //                        CreateBy = getUserActive.UserActiveId,
        //                        CreateDateTime = DateTimeOffset.UtcNow
        //                    };
        //                    _applicationDbContext.Billings.Add(billing);
        //                }
        //            }
        //        }

        //        int result = await _applicationDbContext.SaveChangesAsync();
        //        if (result > 0)
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created" });

        //        return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResep(Guid id, [FromBody] ResepViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid!" });

            try
            {
                var resep = await _applicationDbContext.Reseps.FirstOrDefaultAsync(r => r.ResepId == id);
                if (resep == null)
                    return NotFound(new { message = "Resep tidak ditemukan!" });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User tidak ditemukan!" });

                var userId = user.UserActiveId;
                var today = DateTime.UtcNow.Date;
                var todayString = today.ToString("yyyyMMdd");

                // rollback stok lama
                var oldDetails = await _applicationDbContext.DetailReseps.Where(d => d.ResepId == id).ToListAsync();
                var racikanIds = oldDetails.Where(d => d.IsRacikan == true && d.RacikanId != null).Select(d => d.RacikanId.Value).ToList();

                foreach (var detail in oldDetails)
                {
                    if (detail.IsRacikan == true && detail.RacikanId.HasValue)
                    {
                        var racikanDetails = await _applicationDbContext.RacikanDetails.Where(r => r.RacikanId == detail.RacikanId).ToListAsync();
                        foreach (var rd in racikanDetails)
                        {
                            var obat = await _applicationDbContext.Obats.FindAsync(rd.ObatId);
                            if (obat != null)
                                obat.Stock += rd.QtyUsed ?? 0;
                        }
                    }
                    else
                    {
                        var obat = await _applicationDbContext.Obats.FindAsync(detail.ObatId);
                        if (obat != null)
                            obat.Stock += detail.Qty ?? 0;
                    }
                }

                var oldRacikanDetails = await _applicationDbContext.RacikanDetails.Where(r => racikanIds.Contains((Guid)r.RacikanId)).ToListAsync();
                var oldRacikans = await _applicationDbContext.Racikans.Where(r => racikanIds.Contains(r.RacikanId)).ToListAsync();
                var oldBillings = await _applicationDbContext.Billings.Where(b => b.KunjunganId == resep.KunjunganId && b.JenisBilling == "Obat").ToListAsync();

                _applicationDbContext.RacikanDetails.RemoveRange(oldRacikanDetails);
                _applicationDbContext.Racikans.RemoveRange(oldRacikans);
                _applicationDbContext.DetailReseps.RemoveRange(oldDetails);
                _applicationDbContext.Billings.RemoveRange(oldBillings);

                // update resep
                resep.KunjunganId = vm.KunjunganId;
                resep.AsuransiId = vm.AsuransiId;
                resep.NamaAsuransi = vm.NamaAsuransi;
                resep.PasienId = vm.PasienId;
                resep.NamaPasien = vm.NamaPasien;
                resep.PoliklinikId = vm.PoliklinikId;
                resep.NamaPoliklinik = vm.NamaPoliklinik;
                resep.DokterId = vm.DokterId;
                resep.NamaDokter = vm.NamaDokter;
                resep.RanapId = vm.RanapId;
                resep.UpdateBy = userId;
                resep.UpdateDateTime = DateTimeOffset.UtcNow;

                // handle non-racikan
                var nonRacikanList = vm.DaftarObat.Where(o => o.IsRacikan != true && o.ObatId.HasValue).ToList();
                var obatIds = nonRacikanList.Select(o => o.ObatId.Value).Distinct().ToList();
                var obatDbList = await _applicationDbContext.Obats.Where(o => obatIds.Contains(o.ObatId)).ToDictionaryAsync(o => o.ObatId);

                var detailDict = new Dictionary<Guid, ResepDetail>();
                var billingDict = new Dictionary<Guid, Billing>();

                int billingIndex = 0;

                foreach (var obat in nonRacikanList)
                {
                    var obatId = obat.ObatId.Value;
                    var qty = obat.Qty ?? 0;
                    var obatDb = obatDbList[obatId];

                    if (!detailDict.TryGetValue(obatId, out var detail))
                    {
                        detail = new ResepDetail
                        {
                            DetailResepId = Guid.NewGuid(),
                            ResepId = resep.ResepId,
                            ObatId = obatId,
                            Qty = qty,
                            Signa = obat.Signa,
                            SignaTambahan = obat.SignaTambahan,
                            HargaObat = obatDb.HargaJual,
                            TotalHargaObat = obatDb.HargaJual * qty,
                            StatusCoverObat = obat.StatusCoverObat,
                            JenisObat = obat.JenisObat,
                            IsRacikan = false,
                            RacikanId = null,
                            TakaranDosis = obatDb.TakaranDosis,
                            StatusPengambilanObat = true,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        detailDict[obatId] = detail;
                        _applicationDbContext.DetailReseps.Add(detail);
                    }
                    else
                    {
                        detail.Qty += qty;
                        detail.TotalHargaObat = detail.HargaObat * detail.Qty;
                    }

                    if (!billingDict.TryGetValue(obatId, out var bill))
                    {
                        billingIndex++;
                        bill = new Billing
                        {
                            KunjunganId = resep.KunjunganId,
                            DiskonId = vm.DiskonId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = $"{billingIndex:D3}",
                            ItemId = obatId,
                            NamaItem = obatDb.ObatName,
                            HargaItem = obatDb.HargaJual,
                            QtyItem = qty,
                            SubTotalItem = obatDb.HargaJual * qty,
                            JenisBilling = "Obat",
                            StatusPengambilan = true,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        billingDict[obatId] = bill;
                        _applicationDbContext.Billings.Add(bill);
                    }
                    else
                    {
                        bill.QtyItem += qty;
                        bill.SubTotalItem = bill.QtyItem * bill.HargaItem;
                    }

                    if (obatDb.Stock < qty)
                        return BadRequest(new { message = $"Stok tidak cukup untuk obat: {obatDb.ObatName}" });

                    obatDb.Stock -= qty;
                    _applicationDbContext.Obats.Update(obatDb);
                }

                // handle racikan
                var racikanGroup = vm.DaftarObat
                    .Where(o => o.IsRacikan == true && o.Racikan != null)
                    .SelectMany(o => o.Racikan.Select(r => new { Obat = o, Racikan = r }))
                    .GroupBy(x => x.Racikan.NamaRacikan?.Trim().ToLower());

                foreach (var group in racikanGroup)
                {
                    var first = group.First();
                    var totalQty = group.Sum(g => g.Obat.Qty ?? 0);
                    Guid racikanId = Guid.NewGuid();

                    var racikanEntity = new Racikan
                    {
                        RacikanId = racikanId,
                        NamaRacikan = group.Key,
                        Keterangan = first.Racikan.Keterangan,
                        Signa = first.Obat.Signa,
                        SignaTambahan = first.Obat.SignaTambahan,
                        QtyRacikan = totalQty,
                        KodeRacikan = $"RCK-{(await _applicationDbContext.Racikans.CountAsync(r => r.CreateDateTime.Date == today) + 1):D3}{todayString}",
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    };
                    _applicationDbContext.Racikans.Add(racikanEntity);

                    _applicationDbContext.DetailReseps.Add(new ResepDetail
                    {
                        DetailResepId = Guid.NewGuid(),
                        ResepId = resep.ResepId,
                        ObatId = null,
                        IsRacikan = true,
                        RacikanId = racikanId,
                        Qty = totalQty,
                        HargaObat = first.Obat.HargaObat,
                        TotalHargaObat = first.Obat.HargaObat * totalQty,
                        Signa = first.Obat.Signa,
                        SignaTambahan = first.Obat.SignaTambahan,
                        JenisObat = first.Obat.JenisObat,
                        StatusPengambilanObat = true,
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });

                    decimal totalHarga = 0;

                    foreach (var rDetail in first.Racikan.DaftarRacikan)
                    {
                        var obatDb = await _applicationDbContext.Obats.FindAsync(rDetail.ObatId);
                        if (obatDb == null)
                            return BadRequest(new { message = $"Obat racikan tidak ditemukan: {rDetail.ObatId}" });

                        var qtyPakai = Math.Ceiling((decimal)(rDetail.KomposisiDosis * totalQty / obatDb.TakaranDosis));
                        totalHarga += qtyPakai * obatDb.HargaJual;

                        if (obatDb.Stock < qtyPakai)
                            return BadRequest(new { message = $"Stok racikan tidak cukup: {obatDb.ObatName}" });

                        obatDb.Stock -= (int)qtyPakai;
                        _applicationDbContext.Obats.Update(obatDb);

                        _applicationDbContext.RacikanDetails.Add(new RacikanDetail
                        {
                            DetailRacikanId = Guid.NewGuid(),
                            RacikanId = racikanId,
                            ObatId = rDetail.ObatId,
                            KomposisiDosis = rDetail.KomposisiDosis,
                            QtyUsed = (int)qtyPakai,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        });
                    }

                    billingIndex++;
                    _applicationDbContext.Billings.Add(new Billing
                    {
                        KunjunganId = resep.KunjunganId,
                        DiskonId = vm.DiskonId,
                        BillingDate = DateTime.UtcNow,
                        BillingKode = $"{billingIndex:D3}",
                        ItemId = racikanId,
                        NamaItem = racikanEntity.NamaRacikan,
                        HargaItem = totalHarga,
                        QtyItem = totalQty,
                        SubTotalItem = totalHarga * totalQty,
                        JenisBilling = "Obat",
                        StatusPengambilan = true,
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });
                }

                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Update resep berhasil!" });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data ke database: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpPut("{id}/is-cancelled")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] IsCancelledResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsCancelled = request.IsCancelled;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/is-taken")]
        public async Task<IActionResult> UpdateStatusAmbilResep(Guid id, [FromBody] StatusPengambilanResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPengambilanResep = request.StatusPengambilan;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/StatusResep")]
        public async Task<IActionResult> UpdateStatusResep(Guid id, [FromBody] StatusResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPembuatanResep = request.Status.ToString();
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/Resep-is-Lunas")]
        public async Task<IActionResult> UpdateIsLunas(Guid id, [FromBody] IsLunasResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsLunas = request.IsLunas;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateResep(Guid id, [FromBody] ResepViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid!" });

        //    var resep = await _applicationDbContext.Reseps.FirstOrDefaultAsync(r => r.ResepId == id);
        //    if (resep == null)
        //        return NotFound(new { message = "Resep tidak ditemukan!" });

        //    var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
        //    if (getUserActive == null)
        //        return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //    var userActiveId = getUserActive.UserActiveId;

        //    resep.KunjunganId = vm.KunjunganId;
        //    resep.AsuransiId = vm.AsuransiId;
        //    resep.NamaAsuransi = vm.NamaAsuransi;
        //    resep.PasienId = vm.PasienId;
        //    resep.NamaPasien = vm.NamaPasien;
        //    resep.PoliklinikId = vm.PoliklinikId;
        //    resep.NamaPoliklinik = vm.NamaPoliklinik;
        //    resep.DokterId = vm.DokterId;
        //    resep.NamaDokter = vm.NamaDokter;
        //    resep.StatusPembuatanResep = vm.StatusPembuatanResep;
        //    resep.UpdateBy = userActiveId;
        //    resep.UpdateDateTime = DateTimeOffset.UtcNow;

        //    // Rollback stok lama
        //    var oldDetails = await _applicationDbContext.DetailReseps.Where(d => d.ResepId == id).ToListAsync();
        //    foreach (var detail in oldDetails)
        //    {
        //        if (detail.IsRacikan == true)
        //        {
        //            var racikanDetails = await _applicationDbContext.RacikanDetails
        //                .Where(rd => rd.RacikanId == detail.RacikanId).ToListAsync();
        //            foreach (var rd in racikanDetails)
        //            {
        //                var obat = await _applicationDbContext.Obats.FindAsync(rd.ObatId);
        //                if (obat != null)
        //                {
        //                    obat.Stock += rd.QtyUsed ?? 0;
        //                    _applicationDbContext.Obats.Update(obat);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var obat = await _applicationDbContext.Obats.FindAsync(detail.ObatId);
        //            if (obat != null)
        //            {
        //                obat.Stock += detail.Qty ?? 0;
        //                _applicationDbContext.Obats.Update(obat);
        //            }
        //        }
        //    }

        //    // Remove old data
        //    var racikanIds = oldDetails.Where(x => x.IsRacikan == true && x.RacikanId != null)
        //                               .Select(x => x.RacikanId.Value).ToList();

        //    var oldRacikanDetails = await _applicationDbContext.RacikanDetails
        //        .Where(rd => racikanIds.Contains((Guid)rd.RacikanId)).ToListAsync();
        //    _applicationDbContext.RacikanDetails.RemoveRange(oldRacikanDetails);

        //    var oldRacikans = await _applicationDbContext.Racikans
        //        .Where(r => racikanIds.Contains(r.RacikanId)).ToListAsync();
        //    _applicationDbContext.Racikans.RemoveRange(oldRacikans);

        //    _applicationDbContext.DetailReseps.RemoveRange(oldDetails);

        //    // Add new
        //    var obatIds = vm.DaftarObat?.Where(o => o.ObatId != null).Select(o => o.ObatId).ToList();
        //    var obatDbList = await _applicationDbContext.Obats
        //        .Where(o => obatIds.Contains(o.ObatId)).ToDictionaryAsync(o => o.ObatId);

        //    var existingBillings = await _applicationDbContext.Billings
        //        .Where(b => b.KunjunganId == vm.KunjunganId && b.JenisBilling == "Obat")
        //        .ToListAsync();
        //    int billingIndex = existingBillings.Count;

        //    var today = DateTime.UtcNow.Date;
        //    var todayString = today.ToString("yyyyMMdd");

        //    foreach (var obat in vm.DaftarObat)
        //    {
        //        var detailResep = new ResepDetail
        //        {
        //            DetailResepId = Guid.NewGuid(),
        //            ResepId = id,
        //            ObatId = obat.IsRacikan == true ? null : obat.ObatId,
        //            Qty = obat.Qty,
        //            Signa = obat.Signa,
        //            SignaTambahan = obat.SignaTambahan,
        //            HargaObat = obat.HargaObat,
        //            TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0),
        //            StatusCoverObat = obat.StatusCoverObat,
        //            JenisObat = obat.JenisObat,
        //            IsRacikan = obat.IsRacikan,
        //            RacikanId = obat.IsRacikan == true ? Guid.NewGuid() : null,
        //            TakaranDosis = obat.IsRacikan == true ? null : obatDbList.GetValueOrDefault(obat.ObatId ?? Guid.Empty)?.TakaranDosis,
        //            StatusPengambilanObat = true,
        //            CreateBy = userActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow
        //        };
        //        _applicationDbContext.DetailReseps.Add(detailResep);

        //        // Racikan
        //        Guid? itemId = null;
        //        string namaItem = "";
        //        decimal hargaItem = 0;
        //        int qtyitem = 0;
        //        decimal subTotalItem = 0;
        //        decimal hargaOb = 0;
        //        decimal totalHargaRacikan = 0;
        //        Guid idRacikan = detailResep.RacikanId ?? Guid.NewGuid();

        //        if (obat.IsRacikan == true && obat.Racikan != null)
        //        {
        //            foreach (var racikan in obat.Racikan)
        //            {
        //                int racikanCountToday = await _applicationDbContext.DetailReseps
        //                    .CountAsync(r => r.CreateDateTime.Date == today && r.ResepId == id && r.IsRacikan == true);
        //                string kodeUrut = (racikanCountToday + 1).ToString("D3");
        //                string kodeRacikan = $"RCK-{kodeUrut}{todayString}";

        //                var racikanEntity = new Racikan
        //                {
        //                    RacikanId = idRacikan,
        //                    NamaRacikan = racikan.NamaRacikan,
        //                    Keterangan = racikan.Keterangan,
        //                    Signa = racikan.Signa,
        //                    SignaTambahan = racikan.SignaTambahan,
        //                    QtyRacikan = racikan.QtyRacikan,
        //                    KodeRacikan = kodeRacikan,
        //                    CreateBy = userActiveId,
        //                    CreateDateTime = DateTimeOffset.UtcNow
        //                };
        //                _applicationDbContext.Racikans.Add(racikanEntity);

        //                foreach (var rd in racikan.DaftarRacikan)
        //                {
        //                    var obatRacik = await _applicationDbContext.Obats.FindAsync(rd.ObatId);
        //                    var qtyUsed = Math.Ceiling((decimal)((rd.KomposisiDosis * racikan.QtyRacikan) / obatRacik.TakaranDosis));
        //                    hargaOb = qtyUsed * obatRacik.HargaJual;
        //                    totalHargaRacikan += hargaOb;

        //                    if (obatRacik.Stock < qtyUsed)
        //                        return BadRequest(new { message = $"Stok obat racikan tidak cukup: {obatRacik.ObatName}" });

        //                    obatRacik.Stock -= (int)qtyUsed;
        //                    _applicationDbContext.Obats.Update(obatRacik);

        //                    _applicationDbContext.RacikanDetails.Add(new RacikanDetail
        //                    {
        //                        DetailRacikanId = Guid.NewGuid(),
        //                        RacikanId = racikanEntity.RacikanId,
        //                        ObatId = rd.ObatId,
        //                        QtyUsed = (int?)qtyUsed,
        //                        KomposisiDosis = rd.KomposisiDosis,
        //                        CreateBy = userActiveId,
        //                        CreateDateTime = DateTimeOffset.UtcNow
        //                    });
        //                }

        //                itemId = racikanEntity.RacikanId;
        //                namaItem = racikanEntity.NamaRacikan;
        //                hargaItem = hargaOb;
        //                subTotalItem = totalHargaRacikan;
        //                qtyitem = (int)racikan.QtyRacikan;
        //            }
        //        }
        //        else
        //        {
        //            var obatDb = obatDbList.GetValueOrDefault(obat.ObatId ?? Guid.Empty);
        //            if (obatDb.Stock < obat.Qty)
        //                return BadRequest(new { message = $"Stok obat tidak cukup: {obatDb.ObatName}" });

        //            obatDb.Stock -= (int)obat.Qty;
        //            _applicationDbContext.Obats.Update(obatDb);

        //            itemId = obat.ObatId;
        //            namaItem = obatDb.ObatName;
        //            hargaItem = obatDb.HargaJual;
        //            subTotalItem = hargaItem * (obat.Qty ?? 0);
        //            qtyitem = (int)(obat.Qty ?? 0);
        //        }

        //        // Billing update/insert
        //        var billing = existingBillings.FirstOrDefault(b => b.ItemId == itemId);
        //        if (billing != null)
        //        {
        //            billing.HargaItem = hargaItem;
        //            billing.QtyItem = qtyitem;
        //            billing.SubTotalItem = subTotalItem;
        //            billing.UpdateBy = userActiveId;
        //            billing.UpdateDateTime = DateTimeOffset.UtcNow;
        //            _applicationDbContext.Billings.Update(billing);
        //        }
        //        else
        //        {
        //            billingIndex++;
        //            string billingKode = $"{billingIndex:D3}";
        //            _applicationDbContext.Billings.Add(new Billing
        //            {
        //                KunjunganId = resep.KunjunganId,
        //                DiskonId = vm.DiskonId,
        //                BillingDate = DateTime.UtcNow,
        //                BillingKode = billingKode,
        //                ItemId = itemId,
        //                NamaItem = namaItem,
        //                HargaItem = hargaItem,
        //                QtyItem = qtyitem,
        //                SubTotalItem = subTotalItem,
        //                JenisBilling = "Obat",
        //                StatusPengambilan = true,
        //                CreateBy = userActiveId,
        //                CreateDateTime = DateTimeOffset.UtcNow
        //            });
        //        }
        //    }

        //    int result = await _applicationDbContext.SaveChangesAsync();
        //    if (result > 0)
        //        return Ok(new { message = "Update resep berhasil!" });

        //    return StatusCode(500, new { message = "Update resep gagal disimpan." });
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResep(Guid id)
        {
            try
            {
                // Autentikasi user
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // Cari data resep
                var resep = await _applicationDbContext.Reseps
                    .FirstOrDefaultAsync(r => r.ResepId == id && r.IsDelete == false);
                if (resep == null)
                    return NotFound(new { message = "Data resep tidak ditemukan atau sudah dihapus." });

                // Soft delete DetailResep
                var detailReseps = await _applicationDbContext.DetailReseps
                    .Where(dr => dr.ResepId == id && dr.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in detailReseps)
                {
                    detail.IsDelete = true;
                    detail.DeleteBy = userActiveId;
                    detail.DeleteDateTime = DateTimeOffset.UtcNow;
                }

                // Soft delete Billing terkait kunjungan
                var billings = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == resep.KunjunganId && b.IsDelete == false)
                    .ToListAsync();

                foreach (var billing in billings)
                {
                    billing.IsDelete = true;
                    billing.DeleteBy = userActiveId;
                    billing.DeleteDateTime = DateTimeOffset.UtcNow;
                }

                // Soft delete Resep
                resep.IsDelete = true;
                resep.DeleteBy = userActiveId;
                resep.DeleteDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus secara soft delete || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public IActionResult PagedResep(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] bool? IsLunas = null,
            [FromQuery] bool? IsCancelled = null,
            [FromQuery] bool? StatusPengambilanResep = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _applicationDbContext.Reseps
                .Where(r => !r.IsDelete)
                .Join(_applicationDbContext.UserActives,
                      r => r.CreateBy,
                      u => u.UserActiveId,
                      (r, u) => new { Resep = r, User = u });

            // Filter by date range
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(q => q.Resep.CreateDateTime >= startUtc && q.Resep.CreateDateTime <= endUtc);
            }

            // Boolean filters
            if (IsLunas.HasValue)
                query = query.Where(q => q.Resep.IsLunas == IsLunas.Value);
            if (StatusPengambilanResep.HasValue)
                query = query.Where(q => q.Resep.StatusPengambilanResep == StatusPengambilanResep.Value);
            if (IsCancelled.HasValue)
                query = query.Where(q => q.Resep.IsCancelled == IsCancelled.Value);


            // Periode filter
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(q => q.Resep.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= startWeek && q.Resep.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= lastWeekStart && q.Resep.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(q => q.Resep.CreateDateTime.Month == today.Month && q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(q => q.Resep.CreateDateTime.Month == lastMonth.Month && q.Resep.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderByDescending(q => q.User.FullName),
                    "createdatetime" => query.OrderByDescending(q => q.Resep.CreateDateTime),
                    _ => query.OrderByDescending(q => q.Resep.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderBy(q => q.User.FullName),
                    "createdatetime" => query.OrderBy(q => q.Resep.CreateDateTime),
                    _ => query.OrderBy(q => q.Resep.CreateDateTime)
                };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            var rows = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList()
                .Select(q => new
                {
                    q.Resep.ResepId,
                    q.Resep.KunjunganId,
                    q.Resep.CreateDateTime,
                    q.Resep.CreateBy,
                    q.Resep.AntrianRegistrasi,
                    q.Resep.AntrianResep,
                    q.Resep.AsuransiId,
                    q.Resep.NamaAsuransi,
                    q.Resep.PasienId,
                    q.Resep.NamaPasien,
                    q.Resep.PoliklinikId,
                    q.Resep.NamaPoliklinik,
                    q.Resep.DokterId,
                    q.Resep.NamaDokter,
                    q.Resep.StatusPembuatanResep,
                    q.Resep.StatusPengambilanResep,
                    q.Resep.IsCancelled,
                    q.Resep.IsLunas,
                    q.Resep.RanapId,
                    TanggalPembuatanResep = q.Resep.TanggalPembuatanResep?.ToString("yyyy-MM-dd"),
                    CreateByName = q.User.FullName,

                    DaftarObat = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && (d.IsRacikan == false || d.IsRacikan == null))
                        .Join(_applicationDbContext.Obats,
                              d => d.ObatId,
                              o => o.ObatId,
                              (d, o) => new
                              {
                                  d.DetailResepId,
                                  d.ObatId,
                                  o.ObatName,
                                  d.Qty,
                                  d.HargaObat,
                                  d.TotalHargaObat,
                                  d.Signa,
                                  d.SignaTambahan,
                                  d.TakaranDosis,
                                  d.IsIteratur,
                                  d.JumlahIteratur,
                                  TglMulaiIteratur = d.TglMulaiIteratur,
                                  MasaAktifIteratur = d.MasaAktifIteratur,
                                  d.JarakPenebusan,
                                  d.StatusCoverObat,
                                  d.StatusPengambilanObat,
                                  d.CreateBy,
                                  d.CreateDateTime
                              })
                        .ToList(),

                    DaftarRacikan = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && d.RacikanId != null)
                        .Join(_applicationDbContext.Racikans,
                              d => d.RacikanId,
                              ra => ra.RacikanId,
                              (d, ra) => new
                              {
                                  ra.RacikanId,
                                  ra.NamaRacikan,
                                  d.Qty,
                                  d.Signa,
                                  d.SignaTambahan,
                                  ra.CreateBy,
                                  ra.CreateDateTime,
                                  DaftarRacikanDetail = _applicationDbContext.RacikanDetails
                                      .Where(rd => rd.RacikanId == ra.RacikanId)
                                      .Join(_applicationDbContext.Obats,
                                            rd => rd.ObatId,
                                            ob => ob.ObatId,
                                            (rd, ob) => new
                                            {
                                                rd.DetailRacikanId,
                                                rd.ObatId,
                                                ob.ObatName,
                                                rd.QtyUsed,
                                                rd.KomposisiDosis,
                                                rd.CreateBy,
                                                rd.CreateDateTime
                                            })
                                      .ToList()
                              })
                        .AsEnumerable() // pindahkan ke memori
                        .GroupBy(r => r.RacikanId)
                        .Select(g => g.First()) // ambil racikan unik
                        .ToList()})
                .ToList();

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


        [HttpGet("pagedResepNotLunas")]
        public IActionResult PagedResepBelumLunas(
        int page = 1,
        int perPage = 10,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null,
        [FromQuery] bool? IsCancelled = null,
        [FromQuery] bool? StatusPengambilanResep = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _applicationDbContext.Reseps
                .Where(r => !r.IsDelete && r.IsLunas==false)
                .Join(_applicationDbContext.UserActives,
                      r => r.CreateBy,
                      u => u.UserActiveId,
                      (r, u) => new { Resep = r, User = u });

            // Filter by date range
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(q => q.Resep.CreateDateTime >= startUtc && q.Resep.CreateDateTime <= endUtc);
            }

            // Boolean filters
            if (StatusPengambilanResep.HasValue)
                query = query.Where(q => q.Resep.StatusPengambilanResep == StatusPengambilanResep.Value);

            // IsCancelled filters
            if (IsCancelled.HasValue)
                query = query.Where(q => q.Resep.IsCancelled == IsCancelled.Value);

            // Periode filter
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(q => q.Resep.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= startWeek && q.Resep.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= lastWeekStart && q.Resep.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(q => q.Resep.CreateDateTime.Month == today.Month && q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(q => q.Resep.CreateDateTime.Month == lastMonth.Month && q.Resep.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderByDescending(q => q.User.FullName),
                    "createdatetime" => query.OrderByDescending(q => q.Resep.CreateDateTime),
                    _ => query.OrderByDescending(q => q.Resep.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderBy(q => q.User.FullName),
                    "createdatetime" => query.OrderBy(q => q.Resep.CreateDateTime),
                    _ => query.OrderBy(q => q.Resep.CreateDateTime)
                };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            var rows = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList()
                .Select(q => new
                {
                    q.Resep.ResepId,
                    q.Resep.KunjunganId,
                    q.Resep.CreateDateTime,
                    q.Resep.CreateBy,
                    q.Resep.AntrianRegistrasi,
                    q.Resep.AntrianResep,
                    q.Resep.AsuransiId,
                    q.Resep.NamaAsuransi,
                    q.Resep.PasienId,
                    q.Resep.NamaPasien,
                    q.Resep.PoliklinikId,
                    q.Resep.NamaPoliklinik,
                    q.Resep.DokterId,
                    q.Resep.NamaDokter,
                    q.Resep.StatusPembuatanResep,
                    q.Resep.StatusPengambilanResep,
                    q.Resep.IsCancelled,
                    q.Resep.IsLunas,
                    TanggalPembuatanResep = q.Resep.TanggalPembuatanResep?.ToString("yyyy-MM-dd"),
                    CreateByName = q.User.FullName,

                    DaftarObat = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && (d.IsRacikan == false || d.IsRacikan == null))
                        .Join(_applicationDbContext.Obats,
                              d => d.ObatId,
                              o => o.ObatId,
                              (d, o) => new
                              {
                                  d.DetailResepId,
                                  d.ObatId,
                                  o.ObatName,
                                  d.Qty,
                                  d.HargaObat,
                                  d.TotalHargaObat,
                                  d.Signa,
                                  d.SignaTambahan,
                                  d.TakaranDosis,
                                  d.IsIteratur,
                                  d.JumlahIteratur,
                                  TglMulaiIteratur = d.TglMulaiIteratur,
                                  MasaAktifIteratur = d.MasaAktifIteratur,
                                  d.JarakPenebusan,
                                  d.StatusCoverObat,
                                  d.StatusPengambilanObat,
                                  d.CreateBy,
                                  d.CreateDateTime
                              })
                        .ToList(),

                    DaftarRacikan = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && d.RacikanId != null)
                        .Join(_applicationDbContext.Racikans,
                              d => d.RacikanId,
                              ra => ra.RacikanId,
                              (d, ra) => new
                              {
                                  ra.RacikanId,
                                  ra.NamaRacikan,
                                  d.Qty,
                                  d.Signa,
                                  d.SignaTambahan,
                                  ra.CreateBy,
                                  ra.CreateDateTime,
                                  DaftarRacikanDetail = _applicationDbContext.RacikanDetails
                                      .Where(rd => rd.RacikanId == ra.RacikanId)
                                      .Join(_applicationDbContext.Obats,
                                            rd => rd.ObatId,
                                            ob => ob.ObatId,
                                            (rd, ob) => new
                                            {
                                                rd.DetailRacikanId,
                                                rd.ObatId,
                                                ob.ObatName,
                                                rd.QtyUsed,
                                                rd.KomposisiDosis,
                                                rd.CreateBy,
                                                rd.CreateDateTime
                                            })
                                      .ToList()
                              })
                        .AsEnumerable() // pindahkan ke memori
                        .GroupBy(r => r.RacikanId)
                        .Select(g => g.First()) // ambil racikan unik
                        .ToList()})
                .ToList();

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

